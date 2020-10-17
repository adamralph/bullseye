#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable RS0016 // Add public types and members to the declared API
namespace Bullseye.Internal
{
    using System.Collections.Generic;

    public static class StringExtensions
    {
        public static string Spaced(this IEnumerable<string> strings) => string.Join(" ", strings);

        // pad right printed
        public static string Prp(this string text, int totalWidth, char paddingChar) =>
            text.PadRight(totalWidth + (text.Length - Palette.StripColours(text).Length), paddingChar);
    }
}
