namespace Bullseye
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public static partial class Targets
    {
        public static void Target(string name, IEnumerable<string> dependsOn, Action action) =>
            Target(
                name,
                dependsOn,
                action == null
                    ? default(Func<Task>)
                    : () =>
                        {
                            action?.Invoke();
                            return Task.FromResult(0);
                        });

        public static void Target<TInput>(string name, IEnumerable<string> dependsOn, IEnumerable<TInput> forEach, Action<TInput> action) =>
            Target(
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

        public static void RunTargets(IEnumerable<string> args) => RunTargetsAsync(args).GetAwaiter().GetResult();

        public static void RunTargets(Options options) => RunTargetsAsync(options).GetAwaiter().GetResult();
    }
}
