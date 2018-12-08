namespace Bullseye
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public static partial class Targets
    {
        public static void Target(string name, Func<Task> action) => Target(name, default, action);

        public static void Target(string name, Action action) => Target(name, default, action);

        public static void Target<TInput>(string name, IEnumerable<TInput> forEach, Func<TInput, Task> action) => Target(name, default, forEach, action);

        public static void Target<TInput>(string name, IEnumerable<TInput> forEach, Action<TInput> action) => Target(name, default, forEach, action);

        [Obsolete("Use RunTargetsAndExitAsync instead. This method will be removed in 3.0.0.")]
        public static Task RunTargetsAsync() => RunTargetsAsync(default);

        [Obsolete("Use RunTargetsAndExit instead. This method will be removed in 3.0.0.")]
        public static void RunTargets() => RunTargets(default);
    }
}
