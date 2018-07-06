namespace Bullseye
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    partial class Targets
    {
        [Obsolete("Use Target() instead. Will be removed in version 2.0.0. Will be removed in version 2.0.0.")]
        public static void Add(string name, IEnumerable<string> dependsOn, Func<Task> action) =>
            Target(name, dependsOn, action);

        [Obsolete("Use Target() instead. Will be removed in version 2.0.0.")]
        public static void Add<TInput>(string name, IEnumerable<string> dependsOn, IEnumerable<TInput> forEach, Func<TInput, Task> action) =>
            Target(name, dependsOn, forEach, action);

        [Obsolete("Use Target() instead. Will be removed in version 2.0.0.")]
        public static void Add(string name) =>
            Target(name);

        [Obsolete("Use Target() instead. Will be removed in version 2.0.0.")]
        public static void Add(string name, IEnumerable<string> dependsOn) =>
            Target(name, dependsOn);

        [Obsolete("Use Target() instead. Will be removed in version 2.0.0.")]
        public static void Add(string name, Func<Task> action) =>
            Target(name, action);

        [Obsolete("Use Target() instead. Will be removed in version 2.0.0.")]
        public static void Add(string name, Action action) =>
            Target(name, action);

        [Obsolete("Use Target() instead. Will be removed in version 2.0.0.")]
        public static void Add<TInput>(string name, IEnumerable<TInput> forEach, Func<TInput, Task> action) =>
            Target(name, forEach, action);

        [Obsolete("Use Target() instead. Will be removed in version 2.0.0.")]
        public static void Add<TInput>(string name, IEnumerable<TInput> forEach, Action<TInput> action) =>
            Target(name, forEach, action);

        [Obsolete("Use Target() instead. Will be removed in version 2.0.0.")]
        public static void Add(string name, IEnumerable<string> dependsOn, Action action) =>
            Target(name, dependsOn, action);

        [Obsolete("Use Target() instead. Will be removed in version 2.0.0.")]
        public static void Add<TInput>(string name, IEnumerable<string> dependsOn, IEnumerable<TInput> forEach, Action<TInput> action) =>
            Target(name, dependsOn, forEach, action);

        [Obsolete("Use RunTargets() instead. Will be removed in version 2.0.0.")]
        public static void Run() =>
            RunTargets();

        [Obsolete("Use RunTargets() instead. Will be removed in version 2.0.0.")]
        public static void Run(IEnumerable<string> args) =>
            RunTargets(args);

        [Obsolete("Use RunTargetsAsync() instead. Will be removed in version 2.0.0.")]
        public static Task RunAsync(IEnumerable<string> args) =>
            RunTargetsAsync(args);

        [Obsolete("Use RunTargetsAsync() instead. Will be removed in version 2.0.0.")]
        public static Task RunAsync() =>
            RunTargetsAsync();
    }
}
