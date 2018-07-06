namespace BullseyeTests.Infra
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Bullseye.Internal;

    internal static class Helper
    {
        public static ref T Ensure<T>(ref T t) where T : class, new()
        {
            t = t ?? new T();
            return ref t;
        }

        public static Target CreateTarget(string name, Action action) => CreateTarget(name, new string[0], action);

        public static Target CreateTarget(string name, string[] dependencies, Action action) =>
            new TargetWithoutInput(name, dependencies.ToList(), action.ToAsync());

        public static Target CreateTarget<TInput>(string name, IEnumerable<TInput> forEach, Action<TInput> action) =>
            new Target<TInput>(name, new string[0], forEach, action.ToAsync());

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
