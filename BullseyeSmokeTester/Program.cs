namespace BullseyeSmokeTester
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using static Bullseye.Targets;

    internal class Program
    {
        private static Task Main(string[] args)
        {
            Target("default", DependsOn("worl:d", "exclai: m", "no-action", "echo", "combo", "no-inputs"));

            Target("hell\"o", () => Console.WriteLine("Hello"));

            Target("comm/a", DependsOn("hell\"o"), () => Console.WriteLine(", "));

            Target("worl:d", DependsOn("comm/a"), () => Console.WriteLine("World"));

            Target("exclai: m", DependsOn("worl:d"), () => Console.WriteLine("!"));

            Target("no-action", ForEach(1, 2), null);

            var foos = new[] { "a", "b" };
            var bars = new[] { 1, 2 };

            Target(
                "foo",
                ForEach(10, 20, 30),
                delay => Task.Delay(delay));

            Target(
                "bar",
                () => Task.Delay(1));

            Target(
                "echo",
                DependsOn("foo", "bar"),
                ForEach(1, 2, 3),
                async number =>
                {
                    await Task.Delay((4 - number) * 10);
                    await Console.Out.WriteLineAsync(number.ToString());
                });

            Target(
                "combo",
                foos.SelectMany(foo => bars.Select(bar => new { foo, bar })),
                async o =>
                {
                    await Task.Delay((4 - o.bar) * 10);
                    await Console.Out.WriteLineAsync($"{o.foo},{o.bar}");
                });

            Target("no-inputs", Enumerable.Empty<string>(), input => { });

            Target("build", () => { });
            Target("test", DependsOn("build"), () => { });
            Target("pack", DependsOn("build"), () => { });
            Target("publish", DependsOn("pack"), () => { });

            return RunTargetsAndExitAsync(args);
        }
    }
}
