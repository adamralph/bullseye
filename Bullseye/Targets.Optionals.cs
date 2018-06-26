namespace Bullseye
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public static partial class Targets
    {
        public static void Add(string name) => Add(name, default, default);

        public static void Add(string name, IEnumerable<string> dependsOn) => Add(name, dependsOn, default);

        public static void Add(string name, Func<Task> action) => Add(name, default, action);

        public static void Add(string name, Action action) => Add(name, default, action);

        public static Task RunAsync() => RunAsync(default);

        public static void Run() => Run(default);
    }
}
