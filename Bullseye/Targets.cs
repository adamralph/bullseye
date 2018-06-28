namespace Bullseye
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Bullseye.Internal;

    public static partial class Targets
    {
        private static readonly Dictionary<string, Target> targets = new Dictionary<string, Target>();

        public static string[] DependsOn(params string[] dependencies) => dependencies;

        public static void Add(string name, IEnumerable<string> dependsOn, Func<Task> action) =>
            targets.Add(ValidateName(name), new Target(dependsOn, action));

        public static Task RunAsync(IEnumerable<string> args) =>
            targets.RunAsync(args, new SystemConsole());

        private static string ValidateName(string name)
        {
            if (name == null)
            {
                throw new Exception("A target name cannot be null.");
            }

            return name;
        }
    }
}
