using SixLabors.ImageSharp.Formats;

namespace CFS.Surge.ImageSharp
{
    public sealed class SurgeImageFormat : IImageFormat
    {
        public static SurgeImageFormat Instance { get; }

        static SurgeImageFormat() 
        {
            Instance = new();
        }

        public string Name { get; }

        public string DefaultMimeType { get; }

        public IEnumerable<string> MimeTypes { get; }

        public IEnumerable<string> FileExtensions { get; }

        private SurgeImageFormat()
        {
            Name = "SSRG";
            DefaultMimeType = "application/x-cfs-surge";
            MimeTypes = ["application/x-cfs-surge"];
            FileExtensions = ["ssrg"];
        }
    }
}
