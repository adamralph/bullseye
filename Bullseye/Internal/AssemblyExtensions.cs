using System.Reflection;

namespace Bullseye.Internal;

public static class AssemblyExtensions
{
    public static string GetVersion(this Assembly assembly) =>
        assembly.GetCustomAttributes(false).OfType<AssemblyInformationalVersionAttribute>().FirstOrDefault()?.InformationalVersion ?? "Unknown";

    public static string GetNameOrDefault(this Assembly? assembly, out bool isDefault)
    {
        if (assembly?.GetName().Name is { } name)
        {
            isDefault = false;
            return name;
        }

        isDefault = true;
        return "Bullseye";
    }
}
