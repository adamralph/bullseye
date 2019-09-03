using System.Collections.Generic;

namespace BullseyeSmokeTester
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using static Bullseye.Targets;
    using IBuildContext = Bullseye.IBuildContext;

    public class TestContext : Bullseye.IBuildContext
    {
        public Dictionary<string, object> Items { get; }

        public TestContext() =>
            Items = new Dictionary<string, object>();
    }

    internal class Program
    {
        private static Task Main(string[] args)
        {
            Target("default", DependsOn("worl:d", "exclai: m", "no-action", "echo", "combo", "no-inputs", "Foo_parent", "Bar_parent", "Force_parent"));

            Target("hell\"o", () => Console.Out.WriteLine("Hello"));

            Target("comm/a", DependsOn("hell\"o"), () => Console.Out.WriteLine(", "));

            Target("worl:d", DependsOn("comm/a"), () => Console.Out.WriteLine("World"));

            Target("exclai: m", DependsOn("worl:d"), () => Console.Out.WriteLine("!"));

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
                foos.SelectMany(foo => bars.Select(bar => new
                {
                    foo,
                    bar
                })),
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

            // Context flow tests
            Task inlineIncrementFooBarAsync(IBuildContext ctx)
            {
                inlineIncrementFooBar(ctx);
                return Task.CompletedTask;
            }

            void inlineIncrementFooBar(IBuildContext ctx)
            {
                var tc = ctx as TestContext;
                var name = tc.Items["name"] as string;
                tc.Items[name] = ((int)tc.Items[name]) + 1;
                Console.WriteLine($"{name} = {(int)tc.Items[name]}");
            }

            var tc1 = new TestContext { Items = { ["name"] = "foo", ["foo"] = 42 } };
            Target("Foo_parent", DependsOn("Foo_child1"), tc1);
            Target("Foo_child1", DependsOn("Foo_child2"), async (ctx) => await inlineIncrementFooBarAsync(ctx));
            Target("Foo_child2", DependsOn("Foo_child3"), async (ctx) => await inlineIncrementFooBarAsync(ctx));
            Target("Foo_child3", (ctx) => inlineIncrementFooBar(ctx));

            var tc2 = new TestContext { Items = { ["name"] = "bar", ["bar"] = 84 } };
            Target("Bar_parent", DependsOn("Bar_child1"), tc2);
            Target("Bar_child1", DependsOn("Bar_child2"), async (ctx) => await inlineIncrementFooBarAsync(ctx));
            Target("Bar_child2", DependsOn("Bar_child3"), async (ctx) => await inlineIncrementFooBarAsync(ctx));
            Target("Bar_child3", (ctx) => inlineIncrementFooBar(ctx));

            // Run dependencies on a shared context
            var tc3 = new TestContext { Items = { ["name"] = "waz", ["waz"] = 100 } };
            Target("Force_parent", async (ctx) =>
            {
                // Running Foo_parent or Bar_parent does not run dependencies.
                await RunTargetsWithoutExitingAsync("Foo_child1|Bar_child1".Split('|'), context: ctx);
            }, tc3);

            return RunTargetsAndExitAsync(args, ex => ex is InvalidOperationException);
        }
    }
}
