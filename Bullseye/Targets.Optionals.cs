namespace Bullseye
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public static partial class Targets
    {
        public static ITarget Target(string name) => Target(name, default(IEnumerable<string>), default(Action));

        public static ITarget Target(string name, IEnumerable<string> dependsOn) => Target(name, dependsOn, default(Func<Task>));

        public static ITarget Target(string name, Func<Task> action) => Target(name, default(IEnumerable<string>), action);

        public static ITarget Target(string name, Action action) => Target(name, default(IEnumerable<string>), action);

        public static ITarget Target<TInput>(string name, IEnumerable<TInput> forEach, Func<TInput, Task> action) => Target(name, default(IEnumerable<string>), forEach, action);

        public static ITarget Target<TInput>(string name, IEnumerable<TInput> forEach, Action<TInput> action) => Target(name, default(IEnumerable<string>), forEach, action);

        public static ITarget Target(string name, IEnumerable<ITarget> dependsOn) => Target(name, dependsOn, default(Func<Task>));

        public static Task RunTargetsAsync() => RunTargetsAsync(default);

        public static void RunTargets() => RunTargets(default);
    }
}
