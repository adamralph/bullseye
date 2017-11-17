namespace Bullseye
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public static partial class Targets
    {
        public static void Add(string name, IEnumerable<string> dependsOn, Action action) =>
            Add(
                name,
                dependsOn,
                action == null
                    ? default(Func<Task>)
                    : () =>
                        {
                            action?.Invoke();
                            return Task.FromResult(0);
                        });

        public static int Run(IEnumerable<string> args) => RunAsync(args).GetAwaiter().GetResult();
    }
}
