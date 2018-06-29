namespace Bullseye.Internal
{
    using System.Collections.Generic;
    using System.Linq;

    public static class StringExtensions
    {
        public static string Quote(this IEnumerable<string> strings) =>
            string.Join(", ", strings.Select(@string => Quote(@string)));

        public static string Quote(this string @string) => $"\"{(@string.Replace("\"", "\"\""))}\"";
    }
}
