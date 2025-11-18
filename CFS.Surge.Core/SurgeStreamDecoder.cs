using CFS.Surge.Core.Buffers;
using CFS.Surge.Core.Buffers.Implementation;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace CFS.Surge.Core
{
    public class SurgeStreamDecoder : IDisposable
    {
        private record struct DecoderState(int CurrLayerDepth, IReadOnlyBuffer<int> DecodeBuffer, Image<Rgba32> DecodedImage, CancellationToken CancellationToken);

        private const int DEFAULT_BLOCK_SIZE = 81920;

        private readonly Stream decodeStream;

        private readonly SurgeHeader header;

        private readonly Configuration? configuration;

        private readonly bool leaveOpen;

        private bool disposedValue;

        public SurgeHeader Header => header;

        private SurgeStreamDecoder(Stream decodeStream, SurgeHeader header, Configuration? configuration, bool leaveOpen)
        {
            this.decodeStream = decodeStream;
            this.header = header;

            this.configuration = configuration;

            this.leaveOpen = leaveOpen;
        }

        private void DecodeInner(long currOffset, int currLayerIdx, int x0, int y0, int width, int height, DecoderState state)
        {
            int layerFactorW = currLayerIdx < 0 ? header.AspectRatio.N : header.ImageLayerFactors[^(currLayerIdx + 1)];
            int layerFactorH = currLayerIdx < 0 ? header.AspectRatio.M : header.ImageLayerFactors[^(currLayerIdx + 1)];
            for (int i = 0; i < layerFactorW; i++)
            {
                state.CancellationToken.ThrowIfCancellationRequested();
                for (int j = 0; j < layerFactorH; j++)
                {
                    state.CancellationToken.ThrowIfCancellationRequested();
                    int value = state.DecodeBuffer[currOffset++];
                    if (currLayerIdx < state.CurrLayerDepth && (value > 0 || value == int.MinValue))
                    {
                        int offset = value == int.MinValue ? 0 : value >> 2;
                        DecodeInner(currOffset + offset, currLayerIdx + 1,
                            x0 + width * i / layerFactorW,
                            y0 + height * j / layerFactorH,
                            width / layerFactorW,
                            height / layerFactorH,
                            state); 
                        ++currOffset;
                    }
                    else if(currLayerIdx == state.CurrLayerDepth)
                    {
                        if (value > 0 || value == int.MinValue)
                        {
                            value = state.DecodeBuffer[currOffset++];
                        }
                        else if (value < 0 && value != int.MinValue)
                        {
                            value = -value;
                        }

                        int unpackedColor = ((value & 0x7F000000) << 1) | (value & 0x00FFFFFF);
                        for (int y = y0 + height * j / layerFactorH; y < y0 + height * (j + 1) / layerFactorH; y++)
                        {
                            for (int x = x0 + width * i / layerFactorW; x < x0 + width * (i + 1) / layerFactorW; x++)
                            {
                                Rgba32 currPixel = state.DecodedImage[x, y];
                                Span<byte> currPixelVec = MemoryMarshal.AsBytes(new Span<Rgba32>(ref currPixel));
                                for (int ch = 0; ch < 4; ch++)
                                {
                                    currPixelVec[ch] = (byte)Math.Clamp(currPixelVec[ch] + 
                                        ((sbyte)((unpackedColor >> (ch * 8)) & 0xFF) << 1), 
                                        0, 255);
                                }
                                state.DecodedImage[x, y] = currPixel;
                            }
                        }
                    }
                }
            }
        }

        private async IAsyncEnumerable<IBuffer<int>> ReadChunksAsync(int maxReadSize, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            int idx = 0;
            bool isCompleted = false;
            while (!isCompleted && idx < header.ImageLayerOffsets.Length)
            {
                int readCount = 0;
                byte[] chunkBuffer = new byte[header.ImageLayerOffsets[idx]];
                while (!isCompleted && readCount < chunkBuffer.Length)
                {
                    int toRead = Math.Min(maxReadSize, chunkBuffer.Length - readCount);
                    int bytesRead = await decodeStream.ReadAsync(chunkBuffer, readCount, toRead, cancellationToken);

                    isCompleted = bytesRead == 0;
                    readCount += bytesRead;
                }

                yield return new BitBuffer<int>(chunkBuffer);
                ++idx;
            }
        }

        public async IAsyncEnumerable<Image<TPixel>> DecodeAsync<TPixel>([EnumeratorCancellation] CancellationToken cancellationToken = default)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            uint averagePixelValue = 
                ((header.AveragePixelValue & 0x7F000000) << 1) | 
                (header.AveragePixelValue & 0x00FFFFFF);
            Rgba32 averagePixel = Unsafe.As<uint, Rgba32>(ref averagePixelValue);

            using Image<Rgba32> decodedImage = new(header.ImageWidth, header.ImageHeight);
            decodedImage.Mutate(ctx => ctx.BackgroundColor(Color.FromPixel(averagePixel)));

            int currLayerIdx = -1;
            AggregateBuffer<int>? decodeBuffer = null;
            await foreach (IBuffer<int> pieceBuffer in ReadChunksAsync(
                configuration?.StreamProcessingBufferSize 
                ?? DEFAULT_BLOCK_SIZE, cancellationToken))
            {
                decodeBuffer = decodeBuffer?.Append(pieceBuffer) ?? new(pieceBuffer);

                DecoderState state = new(currLayerIdx, decodeBuffer, decodedImage, cancellationToken);
                DecodeInner(0, -1, 0, 0, decodedImage.Width, decodedImage.Height, state);

                yield return decodedImage.CloneAs<TPixel>();
                ++currLayerIdx;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (!leaveOpen || decodeStream is GZipStream)
                    {
                        decodeStream.Dispose();
                    }
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public static bool TryCreateDecoder(Stream stream, Configuration? configuration, bool leaveOpen, [NotNullWhen(true)] out SurgeStreamDecoder? decoder)
        {
            if (configuration?.ReadOrigin == ReadOrigin.Begin) 
            {
                stream.Seek(0, SeekOrigin.Begin);
            }

            Stream? decodeStream = null;
            if (stream.CanSeek)
            {
                Span<byte> magic = stackalloc byte[4];
                stream.ReadExactly(magic);
                stream.Seek(0, SeekOrigin.Begin);
                if (magic[0] == 0x1f && magic[1] == 0x8b)
                {
                    decodeStream = new GZipStream(stream, CompressionMode.Decompress, leaveOpen);
                }
            }

            if (SurgeHeader.TryReadFromStream(decodeStream ?? stream, out SurgeHeader? header))
            {
                decoder = new(decodeStream ?? stream, header, configuration, leaveOpen);
                return true;
            }
            else
            {
                decoder = null;
                return false;
            }
        }
    }
}
