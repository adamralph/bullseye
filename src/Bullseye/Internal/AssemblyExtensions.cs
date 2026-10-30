using System.Reflection;

namespace Bullseye.Internal;

internal static class AssemblyExtensions
{
    public static string? GetInformationalVersion(this Assembly assembly) =>
        assembly.GetCustomAttributes(false).OfType<AssemblyInformationalVersionAttribute>().FirstOrDefault()?.InformationalVersion;
}
