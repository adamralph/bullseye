using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Bullseye.Internal
{
    public static class TaskExtensions
    {
        public static ConfiguredTaskAwaitable Tax(this Task task) => task?.ConfigureAwait(false) ?? default;

        public static ConfiguredTaskAwaitable<TResult> Tax<TResult>(this Task<TResult> task) => task?.ConfigureAwait(false) ?? default;

        public static bool IsAwaitable(this Task task) => task != null && !task.IsCanceled && !task.IsFaulted && !task.IsCompleted;
    }
}
