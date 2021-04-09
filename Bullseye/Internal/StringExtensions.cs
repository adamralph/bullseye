using System.Collections.Generic;

namespace Bullseye.Internal
{
    public static class StringExtensions
    {
        public static string Spaced(this IEnumerable<string> strings) => string.Join(" ", strings);

        // pad right printed
        public static string Prp(this string text, int totalWidth, char paddingChar) =>
            text.PadRight(totalWidth + (text.Length - Palette.StripColours(text).Length), paddingChar);
    }
}
