#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Bullseye.Internal
{
    using System.Collections.Generic;
    using System.Linq;

    public static class EnumerableExtensions
    {
        public static IEnumerable<T> Sanitize<T>(this IEnumerable<T> items) where T : class =>
            items?.Where(item => item != null) ?? Enumerable.Empty<T>();
    }
}
