using CFS.Surge.Core;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;

namespace CFS.Surge.ImageSharp
{
    public sealed class SurgeImageDecoder : ImageDecoder
    {
        public static SurgeImageDecoder Instance { get; }

        static SurgeImageDecoder()
        {
            Instance = new();
        }

        private SurgeImageDecoder() { }

        protected override Image<TPixel> Decode<TPixel>(DecoderOptions options, Stream stream, CancellationToken cancellationToken)
        {
            if (SurgeStreamDecoder.TryCreateDecoder(stream, options.Configuration, true, out SurgeStreamDecoder? decoder))
            {
                using (decoder)
                {
                    int frameIdx = 0;
                    Image<TPixel>? prevImage = null;
                    foreach (Image<TPixel> currImage in decoder.DecodeAsync<TPixel>().ToBlockingEnumerable(cancellationToken))
                    {
                        if (options.MaxFrames < ++frameIdx) break;
                        prevImage?.Dispose();
                        prevImage = currImage;
                    }

                    ScaleToTargetSize(options, prevImage!);
                    return prevImage!;
                }
            }
            else
            {
                throw new InvalidImageContentException("Failed to retrieve SSRG header from stream.");
            }
        }

        protected override Image Decode(DecoderOptions options, Stream stream, CancellationToken cancellationToken) =>
            Decode<Rgba32>(options, stream, cancellationToken);

        protected override ImageInfo Identify(DecoderOptions options, Stream stream, CancellationToken cancellationToken)
        {
            if (SurgeStreamDecoder.TryCreateDecoder(stream, options.Configuration, true, out SurgeStreamDecoder? decoder))
            {
                using (decoder)
                {
                    return new ImageInfo(
                        new PixelTypeInfo(32, PixelAlphaRepresentation.Unassociated),
                        new Size(decoder.Header.ImageWidth, decoder.Header.ImageHeight),
                        null);
                }
            }
            else
            {
                throw new InvalidImageContentException("Failed to retrieve SSRG header from stream.");
            }
        }
    }
}
