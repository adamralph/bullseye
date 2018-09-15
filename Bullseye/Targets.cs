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

        public static ITarget[] DependsOn(params ITarget[] dependencies) => dependencies;

        public static TInput[] ForEach<TInput>(params TInput[] inputs) => inputs;

        public static ITarget Target(string name, IEnumerable<string> dependsOn, Func<Task> action)
        {
            var target = new TargetWithoutInput(name, dependsOn, action);
            targets.Add(target);
            return target;
        }

        public static ITarget Target<TInput>(string name, IEnumerable<string> dependsOn, IEnumerable<TInput> forEach, Func<TInput, Task> action)
        {
            var target = new Target<TInput>(name, dependsOn, forEach, action);
            targets.Add(target);
            return target;
        }

        public static Task RunTargetsAsync(IEnumerable<string> args) =>
            targets.RunAsync(args, new SystemConsole());
    }
}
