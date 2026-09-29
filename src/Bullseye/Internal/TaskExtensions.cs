using System.Runtime.CompilerServices;

namespace Bullseye.Internal;

public static class TaskExtensions
{
#pragma warning disable VSTHRD003 // Avoid awaiting or returning a Task representing work that was not started within your context as that can lead to deadlocks.
    public static ConfiguredTaskAwaitable Tax(this Task task) => task.ConfigureAwait(false);

    public static ConfiguredTaskAwaitable<TResult> Tax<TResult>(this Task<TResult> task) => task.ConfigureAwait(false);
#pragma warning restore VSTHRD003

    public static bool IsAwaitable(this Task task) => task is { IsCanceled: false, IsFaulted: false, IsCompleted: false, };
}
