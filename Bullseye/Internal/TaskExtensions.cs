using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Bullseye.Internal;

public static class TaskExtensions
{
    public static ConfiguredTaskAwaitable Tax(this Task task) => task.ConfigureAwait(false);

    public static ConfiguredTaskAwaitable<TResult> Tax<TResult>(this Task<TResult> task) => task.ConfigureAwait(false);

    public static bool IsAwaitable(this Task task) => task is { IsCanceled: false, IsFaulted: false, IsCompleted: false, };
}
