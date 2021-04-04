using System;
using System.Threading.Tasks;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Bullseye.Internal
{
    public static class ActionExtensions
    {
        public static Func<Task> ToAsync(this Action action) =>
            action == null
                ? (Func<Task>)null
                : () => Task.Run(action.Invoke);

        public static Func<T, Task> ToAsync<T>(this Action<T> action) =>
            action == null
                ? (Func<T, Task>)null
                : obj => Task.Run(() => action.Invoke(obj));
    }
}
