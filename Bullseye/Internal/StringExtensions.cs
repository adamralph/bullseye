using System.Collections.Generic;

namespace Bullseye.Internal
{
    public static class StringExtensions
    {
        public static string Spaced(this IEnumerable<string> strings) => string.Join(" ", strings);
    }
}
