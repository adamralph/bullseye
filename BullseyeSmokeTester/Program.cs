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
            var hello = Target("hell\"o", () => Console.WriteLine("Hello"));

            var comma = Target("comm/a", DependsOn(hello), () => Console.WriteLine(", "));

            var world = Target("worl:d", DependsOn(comma), () => Console.WriteLine("World"));

            var exclaim = Target("exclai: m", DependsOn(world), () => Console.WriteLine("!"));

            var foos = new[] { "a", "b" };
            var bars = new[] { 1, 2 };

            var foo = Target(
                "foo",
                () => Task.Delay(100));

            var bar = Target(
                "bar",
                () => Task.Delay(1));

            var echo = Target(
                "echo",
                DependsOn(foo, bar),
                ForEach(1, 2, 3),
                async number =>
                {
                    await Task.Delay((4 - number) * 10);
                    await Console.Out.WriteLineAsync(number.ToString());
                });

            var combo = Target(
                "combo",
                foos.SelectMany(f => bars.Select(b => new { f, b })),
                async o =>
                {
                    await Task.Delay((4 - o.b) * 10);
                    await Console.Out.WriteLineAsync($"{o.f},{o.b}");
                });

            Target("default", DependsOn(world, exclaim, echo, combo));

            return RunTargetsAsync(args);
        }
    }
}
