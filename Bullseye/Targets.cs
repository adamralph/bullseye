namespace Bullseye
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Bullseye.Internal;

    public static partial class Targets
    {
        private static readonly TargetCollection targets = new TargetCollection();

        public static string[] DependsOn(params string[] dependencies) => dependencies;

        public static TInput[] ForEach<TInput>(params TInput[] inputs) => inputs;

        public static void Target(string name, IEnumerable<string> dependsOn, Func<Task> action) =>
            targets.Add(new TargetWithoutInput(name, dependsOn, action));

        public static void Target<TInput>(string name, IEnumerable<string> dependsOn, IEnumerable<TInput> forEach, Func<TInput, Task> action) =>
            targets.Add(new Target<TInput>(name, dependsOn, forEach, action));

        public static Task RunTargetsAsync(IEnumerable<string> args) =>
            targets.RunAsync(args, new SystemConsole());

        public static Task RunTargetsAsync(Options options) =>
            targets.RunAsync(options, new SystemConsole());
    }
}
