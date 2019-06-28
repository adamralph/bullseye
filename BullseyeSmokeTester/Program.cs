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
                Task.Delay);

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

            Target(
                "fail",
                ForEach(30, 20, 10),
                async delay =>
                {
                    await Task.Delay(delay);

                    switch (delay)
                    {
                        case 10:
                            throw new Exception("bad");
                        case 20:
                            throw new Exception("ugly");
                        default:
                            break;
                    }
                });

            Target(
                "fail2",
                ForEach(2000, 1500, 200, 150),
                async delay =>
                {
                    await Task.Delay(delay);

                    switch (delay)
                    {
                        case 1500:
                            throw new InvalidOperationException("bad");
                        case 150:
                            throw new InvalidOperationException("ugly");
                        default:
                            break;
                    }
                });

            return RunTargetsAndExitAsync(args, ex => ex is InvalidOperationException);
        }
    }
}
