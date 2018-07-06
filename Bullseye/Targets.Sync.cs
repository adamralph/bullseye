namespace Bullseye
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public static partial class Targets
    {
        public static void Add(string name, IEnumerable<string> dependsOn, Action action) =>
            Add(
                name,
                dependsOn,
                action == null
                    ? default(Func<Task>)
                    : () =>
                        {
                            action?.Invoke();
                            return Task.FromResult(0);
                        });

        public static void Add<TInput>(string name, IEnumerable<string> dependsOn, IEnumerable<TInput> forEach, Action<TInput> action) =>
            Add(
                name,
                dependsOn,
                forEach,
                action == null
                    ? default(Func<TInput, Task>)
                    : input =>
                    {
                        action?.Invoke(input);
                        return Task.FromResult(0);
                    });

        public static void Run(IEnumerable<string> args) => RunAsync(args).GetAwaiter().GetResult();
    }
}
