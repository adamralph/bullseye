using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bullseye.Internal;

namespace BullseyeTests.Infra
{
    internal static class Helper
    {
        public static ref T Ensure<T>(ref T t) where T : class, new()
        {
            t = t ?? new T();
            return ref t;
        }

        public static Target CreateTarget(string name, Action action) => CreateTarget(name, Array.Empty<string>(), action);

        public static Target CreateTarget(string name, string[] dependencies, Action action) =>
            new ActionTarget(name, null, dependencies.ToList(), action.ToAsync());

        public static Target CreateTarget(string name, string[] dependencies) =>
            new Target(name, null, dependencies.ToList());

        public static Target CreateTarget<TInput>(string name, IEnumerable<TInput> forEach, Action<TInput> action) =>
            new ActionTarget<TInput>(name, null, Array.Empty<string>(), forEach, action.ToAsync());

        private static Func<Task> ToAsync(this Action action) =>
            () =>
            {
                action();
                return Task.FromResult(0);
            };

        private static Func<TInput, Task> ToAsync<TInput>(this Action<TInput> action) =>
            input =>
            {
                action(input);
                return Task.FromResult(0);
            };
    }
}
