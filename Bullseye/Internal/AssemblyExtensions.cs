using System.Linq;
using System.Reflection;

namespace Bullseye.Internal
{
    public static class AssemblyExtensions
    {
        public static string GetVersion(this Assembly assembly) =>
            assembly?.GetCustomAttributes(false).OfType<AssemblyInformationalVersionAttribute>().FirstOrDefault()?.InformationalVersion ?? "Unknown";
    }
}
