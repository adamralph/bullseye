using System;
using System.Threading.Tasks;

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
