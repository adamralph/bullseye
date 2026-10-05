namespace Bullseye.Internal;

internal static class StringExtensions
{
    public static string Spaced(this IEnumerable<string> strings) => string.Join(" ", strings);
}
