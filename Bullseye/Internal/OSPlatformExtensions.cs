using System.Runtime.InteropServices;

namespace Bullseye.Internal
{
    public static class OSPlatformExtensions
    {
        public static string Humanize(this OSPlatform osPlatform) =>
            osPlatform == OSPlatform.Linux
                ? "Linux"
                : osPlatform == OSPlatform.OSX
                    ? "macOS"
                    : osPlatform == OSPlatform.Windows
                        ? "Windows"
                        : "Unknown";
    }
}
