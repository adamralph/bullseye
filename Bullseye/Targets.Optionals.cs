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

        [Obsolete("This method will be removed in 3.0.0. Consider switching to RunTargetsAndExitAsync, which represents the canonical usage. If you really need to continue code execution after running the targets, use RunTargetsWithoutExitingAsync instead.")]
        public static Task RunTargetsAsync() => RunTargetsWithoutExitingAsync(default);

        [Obsolete("This method will be removed in 3.0.0. Consider switching to RunTargetsAndExit, which represents the canonical usage. If you really need to continue code execution after running the targets, use RunTargetsWithoutExiting instead.")]
        public static void RunTargets() => RunTargetsWithoutExiting(default);
    }
}
