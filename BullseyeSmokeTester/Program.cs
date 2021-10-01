using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Bullseye;
using static Bullseye.Targets;

// spell-checker:disable
Target("default", DependsOn("world", "exclaim", "null-action", "echo", "combo", "no-inputs"));

Target("hell\"o", "Says hello", async () => await Console.Out.WriteLineAsync("Hello"));

Target("comma", DependsOn("hell\"o"), async () => await Console.Out.WriteLineAsync(", "));

Target("world", DependsOn("comma"), async () => await Console.Out.WriteLineAsync("World"));

Target("exclaim", DependsOn("world"), async () => await Console.Out.WriteLineAsync("!"));

// spell-checker:enable
Target("null-action", "does nothing", ForEach(1, 2), null);

var foos = new[] { "a", "b" };
var bars = new[] { 1, 2 };

Target(
    "foo",
    "foos",
    ForEach(10, 20, 30),
    async delay => await Task.Delay(delay));

Target(
    "bar",
    "bars",
    async () => await Task.Delay(1));

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

var targets = new Targets();
targets.Add("abc", async () => await Console.Out.WriteLineAsync("abc"));
targets.Add("def", DependsOn("abc"), async () => await Console.Out.WriteLineAsync("def"));
targets.Add("default", DependsOn("def"));

var largeGraph = new Targets();
var largeGraphTargetNames = new List<string>();

foreach (var name in Enumerable.Range(1, 10).Select(i => i.ToString(CultureInfo.InvariantCulture)))
{
    largeGraph.Add(name, largeGraphTargetNames.TakeLast(2), () => { });
    largeGraphTargetNames.Add(name);
}

largeGraph.Add("large-graph", DependsOn(largeGraphTargetNames.Last()));

var (options, targetNames) = Options.Parse(args);

if (targetNames.Contains("large-graph"))
{
    await largeGraph.RunAndExitAsync(targetNames, options);
}

if (!options.ShowHelp)
{
    await targets.RunWithoutExitingAsync(targetNames, options);
}

await RunTargetsAndExitAsync(args, ex => ex is InvalidOperationException);
