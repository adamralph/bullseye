namespace BullseyeSmokeTester
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using static Bullseye.Targets;

    class Program
    {
        static Task Main(string[] args)
        {
            Target("default", DependsOn("worl:d", "exclai: m", "echo", "combo"));

            Target("hell\"o", () => Console.WriteLine("Hello"));

            Target("comm/a", DependsOn("hell\"o"), () => Console.WriteLine(", "));

            Target("worl:d", DependsOn("comm/a"), () => Console.WriteLine("World"));

            Target("exclai: m", () => Console.WriteLine("!"));

            var foos = new[] { "a", "b" };
            var bars = new[] { 1, 2 };

            Target(
                "echo",
                ForEach(1, 2, 3),
                number => Console.Out.WriteLineAsync(number.ToString()));

            Target(
                "combo",
                foos.SelectMany(foo => bars.Select(bar => new { foo, bar })),
                o => Console.Out.WriteLineAsync($"{o.foo},{o.bar}"));

            return RunTargetsAsync(args);
        }
    }
}
