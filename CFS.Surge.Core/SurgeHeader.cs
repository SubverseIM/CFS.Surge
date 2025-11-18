using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace CFS.Surge.Core
{
    public class SurgeHeader
    {
        public int ImageWidth { get; }

        public int ImageHeight { get; }

        public (int N, int M) AspectRatio { get; }

        public uint AveragePixelValue { get; }

        public uint[] ImageLayerOffsets { get; }

        public int[] ImageLayerFactors { get; }

        public SurgeHeader(int imageWidth, int imageHeight, uint averagePixelValue) 
        {
            ImageWidth = imageWidth;
            ImageHeight = imageHeight;

            AveragePixelValue = averagePixelValue;

            int gcd = Primes.GreatestCommonDivisor(imageWidth, imageHeight);
            AspectRatio = (imageWidth / gcd, imageHeight / gcd);

            ImageLayerOffsets = new uint[gcd.Factors().Count() + 1];
            ImageLayerFactors = gcd.Factors().ToArray();
        }

        public static bool TryReadFromStream(Stream stream, [NotNullWhen(true)] out SurgeHeader? header) 
        {
            using BinaryReader reader = new(stream, Encoding.ASCII, true);
            if (new string(reader.ReadChars(4)) != "SSRG")
            {
                header = null;
                return false;
            }
            else
            {
                header = new(reader.ReadInt32(), reader.ReadInt32(), reader.ReadUInt32());
                for (int i = 0; i < header.ImageLayerOffsets.Length; i++) 
                {
                    header.ImageLayerOffsets[i] = reader.ReadUInt32();
                }

                return true;
            }
        }

        public void WriteToStream(Stream stream) 
        {
            using var writer = new BinaryWriter(stream, Encoding.ASCII, true);

            writer.Write(new char[] { 'S', 'S', 'R', 'G' });
            writer.Write(ImageWidth);
            writer.Write(ImageHeight);
            writer.Write(AveragePixelValue);

            foreach (var offsetValue in ImageLayerOffsets)
            {
                writer.Write(offsetValue);
            }
        }
    }
}
