namespace Bullseye.Internal
{
    using System.Collections.Generic;

    public static class StringExtensions
    {
        public static string Spaced(this IEnumerable<string> strings) => string.Join(" ", strings);
    }
}
