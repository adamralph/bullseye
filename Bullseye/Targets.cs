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

        public static void Add(string name, IEnumerable<string> dependsOn, Func<Task> action) =>
            targets.Add(new Target(name, dependsOn, action));

        public static Task RunAsync(IEnumerable<string> args) =>
            targets.RunAsync(args, new SystemConsole());
    }
}
