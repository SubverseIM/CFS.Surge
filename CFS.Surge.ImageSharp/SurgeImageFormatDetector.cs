using SixLabors.ImageSharp.Formats;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace CFS.Surge.ImageSharp
{
    public sealed class SurgeImageFormatDetector : IImageFormatDetector
    {
        public int HeaderSize { get; }

        public SurgeImageFormatDetector() 
        {
            HeaderSize = 16;
        }

        public bool TryDetectFormat(ReadOnlySpan<byte> header, [NotNullWhen(true)] out IImageFormat? format)
        {
            if (header.StartsWith(Encoding.ASCII.GetBytes("SSRG")))
            {
                format = SurgeImageFormat.Instance;
                return true;
            }
            else if (header[0] == 0x1f && header[1] == 0x8b)
            {
                format = SurgeImageFormat.Instance;
                return true;
            }
            else 
            {
                format = null;
                return false;
            }
        }
    }
}
