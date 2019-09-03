namespace BullseyeTests.Infra
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Bullseye.Internal;

    using IBuildContext = Bullseye.IBuildContext;

    internal static class Helper
    {
        public static ref T Ensure<T>(ref T t) where T : class, new()
        {
            t = t ?? new T();
            return ref t;
        }

        public static Target CreateTarget(string name, Action action) =>
            CreateTarget(name, new string[0], action);

        public static Target CreateTarget(string name, Action<IBuildContext> action, IBuildContext context) =>
            CreateTarget(name, new string[0], action, context);

        /// TDummy disambiguates with another overload
        public static Target CreateTarget<TDummy>(string name, Action<IBuildContext> action) =>
            CreateTarget(name, new string[0], action, default);

        public static Target CreateTarget(string name, string[] dependencies, Action action) =>
            new ActionTarget(name, dependencies.ToList(), action.ToAsync());

        public static Target CreateTarget(string name, string[] dependencies, Action<IBuildContext> action) =>
            new ActionTarget(name, dependencies.ToList(), action.ToAsync(), default);

        public static Target CreateTarget(string name, string[] dependencies, Action<IBuildContext> action, IBuildContext context) =>
            new ActionTarget(name, dependencies.ToList(), action.ToAsync(), context);

        public static Target CreateTarget(string name, string[] dependencies) =>
            new ActionTarget(name, dependencies.ToList(), null);

        public static Target CreateTarget<TInput>(string name, IEnumerable<TInput> forEach, Action<TInput> action) =>
            new ActionTarget<TInput>(name, new string[0], forEach, action.ToAsync());

        private static Func<Task> ToAsync(this Action action) =>
            () =>
            {
                action();
                return Task.FromResult(0);
            };

        private static Func<IBuildContext, Task> ToAsync(this Action<IBuildContext> action) =>
            ctx =>
            {
                action(ctx);
                return Task.FromResult(0);
            };

        private static Func<TInput, Task> ToAsync<TInput>(this Action<TInput> action) =>
            input =>
            {
                action(input);
                return Task.FromResult(0);
            };

        private static Func<TInput, IBuildContext, Task> ToAsync<TInput>(this Action<TInput, IBuildContext> action) =>
            (input, ctx) =>
            {
                action(input, ctx);
                return Task.FromResult(0);
            };
    }
}
