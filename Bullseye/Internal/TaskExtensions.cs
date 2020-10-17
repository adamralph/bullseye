#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable RS0016 // Add public types and members to the declared API
namespace Bullseye.Internal
{
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    public static class TaskExtensions
    {
        public static ConfiguredTaskAwaitable Tax(this Task task) => task.ConfigureAwait(false);

        public static ConfiguredTaskAwaitable<TResult> Tax<TResult>(this Task<TResult> task) => task.ConfigureAwait(false);
    }
}
