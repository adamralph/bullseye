namespace Bullseye
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public static partial class Targets
    {
        public static ITarget Target(string name, IEnumerable<string> dependsOn, Action action) =>
            Target(
                name,
                dependsOn,
                action == null
                    ? default(Func<Task>)
                    : () => Task.Run(() => action?.Invoke()));

        public static ITarget Target<TInput>(string name, IEnumerable<string> dependsOn, IEnumerable<TInput> forEach, Action<TInput> action) =>
            Target(
                name,
                dependsOn,
                forEach,
                action == null
                    ? default(Func<TInput, Task>)
                    : input => Task.Run(() => action?.Invoke(input)));

        public static void RunTargets(IEnumerable<string> args) => RunTargetsAsync(args).GetAwaiter().GetResult();
    }
}
