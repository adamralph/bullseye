#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable RS0016 // Add public types and members to the declared API
namespace Bullseye.Internal
{
    using System.Linq;
    using System.Reflection;

    public static class AssemblyExtensions
    {
        public static string GetVersion(this Assembly assembly) =>
            assembly.GetCustomAttributes(false).OfType<AssemblyInformationalVersionAttribute>().FirstOrDefault()?.InformationalVersion ?? "Unknown";
    }
}
