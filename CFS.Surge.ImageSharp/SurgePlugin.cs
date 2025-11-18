using SixLabors.ImageSharp;

namespace CFS.Surge.ImageSharp
{
    public static class SurgePlugin
    {
        public static void Initialize(Configuration? configuration = null)
        {
            (configuration ?? Configuration.Default).Configure(new SurgeConfigurationModule());
        }
    }
}
