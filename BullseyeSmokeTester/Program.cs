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
            Add("default", DependsOn("worl:d", "exclai: m", "echo", "combo"));

            Add("hell\"o", () => Console.WriteLine("Hello"));

            Add("comm/a", DependsOn("hell\"o"), () => Console.WriteLine(", "));

            Add("worl:d", DependsOn("comm/a"), () => Console.WriteLine("World"));

            Add("exclai: m", () => Console.WriteLine("!"));

            var foos = new[] { "a", "b" };
            var bars = new[] { 1, 2 };

            Add(
                "echo",
                ForEach(1, 2, 3),
                number => Console.Out.WriteLineAsync(number.ToString()));

            Add(
                "combo",
                foos.SelectMany(foo => bars.Select(bar => new { foo, bar })),
                o => Console.Out.WriteLineAsync($"{o.foo},{o.bar}"));

            return RunAsync(args);
        }
    }
}
