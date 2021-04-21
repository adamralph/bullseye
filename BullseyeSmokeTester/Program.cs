using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Bullseye;
using static Bullseye.Targets;

// spell-checker:disable
Target("default", DependsOn("worl:d", "exclai: m", "null-action", "echo", "combo", "no-inputs"));

Target("hell\"o", "Says hello", () => Console.Out.WriteLine("Hello"));

Target("comm/a", DependsOn("hell\"o"), () => Console.Out.WriteLine(", "));

Target("worl:d", DependsOn("comm/a"), () => Console.Out.WriteLine("World"));

Target("exclai: m", DependsOn("worl:d"), () => Console.Out.WriteLine("!"));

// spell-checker:enable
Target("null-action", "does nothing", ForEach(1, 2), null);

var foos = new[] { "a", "b" };
var bars = new[] { 1, 2 };

Target(
    "foo",
    "foos",
    ForEach(10, 20, 30),
    Task.Delay);

Target(
    "bar",
    "bars",
    () => Task.Delay(1));

Target(
    "echo",
    "echoes",
    DependsOn("foo", "bar"),
    ForEach(1, 2, 3),
    async number =>
    {
        await Task.Delay((4 - number) * 10);
        await Console.Out.WriteLineAsync(number.ToString(CultureInfo.InvariantCulture));
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
                throw new NotImplementedException("bad");
            case 20:
                throw new NotImplementedException("ugly");
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
        }
    });

var targets = new Targets();
targets.Add("abc", () => Console.Out.WriteLine("abc"));
targets.Add("def", DependsOn("abc"), () => Console.Out.WriteLine("def"));
targets.Add("default", DependsOn("def"));

var (options, targetNames) = Options.Parse(args);

if (!options.ShowHelp)
{
    await targets.RunWithoutExitingAsync(targetNames, options);
}

await RunTargetsAndExitAsync(targetNames, options, ex => ex is InvalidOperationException);
