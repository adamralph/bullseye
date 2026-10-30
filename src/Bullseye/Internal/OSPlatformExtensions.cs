using System.Runtime.InteropServices;

namespace Bullseye.Internal;

internal static class OSPlatformExtensions
{
    public static string Humanize(this OSPlatform osPlatform) => osPlatform switch
    {
        _ when osPlatform == OSPlatform.Linux => "Linux",
        _ when osPlatform == OSPlatform.OSX => "macOS",
        _ when osPlatform == OSPlatform.Windows => "Windows",
        _ => osPlatform.ToString()
    };
}
