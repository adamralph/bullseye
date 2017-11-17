namespace Bullseye
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public static partial class Targets
    {
        public static void Add(string name) => Add(name, null, null);

        public static void Add(string name, IEnumerable<string> dependsOn) => Add(name, dependsOn, null);

        public static void Add(string name, Func<Task> action) => Add(name, null, action);

        public static void Add(string name, Action action) => Add(name, action.ToAsync());

        public static void Add(string name, IEnumerable<string> dependsOn, Action action) => Add(name, dependsOn, action.ToAsync());

        public static Task<int> RunAsync() => RunAsync(null);

        public static int Run() => Run(null);

        public static int Run(IEnumerable<string> args) => RunAsync(args).GetAwaiter().GetResult();

        private static Func<Task> ToAsync(this Action action) =>
            action == null
            ? default(Func<Task>)
            : () =>
                {
                    action?.Invoke();
                    return Task.FromResult(0);
                };
    }
}
