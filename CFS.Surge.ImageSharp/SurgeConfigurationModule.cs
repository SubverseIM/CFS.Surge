using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;

namespace CFS.Surge.ImageSharp
{
    public sealed class SurgeConfigurationModule : IImageFormatConfigurationModule
    {
        public void Configure(Configuration configuration)
        {
            configuration.ImageFormatsManager.AddImageFormat(SurgeImageFormat.Instance);
            configuration.ImageFormatsManager.SetDecoder(SurgeImageFormat.Instance, SurgeImageDecoder.Instance);
            configuration.ImageFormatsManager.AddImageFormatDetector(new SurgeImageFormatDetector());
        }
    }
}
