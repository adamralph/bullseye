using System;
using System.Threading.Tasks;

namespace Bullseye.Internal
{
    public static class ActionExtensions
    {
        public static Func<Task> ToAsync(this Action action) => () => Task.Run(action.Invoke);

        public static Func<T, Task> ToAsync<T>(this Action<T> action) => obj => Task.Run(() => action.Invoke(obj));
    }
}
