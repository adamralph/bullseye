using System.Globalization;
using Bullseye;
using static Bullseye.Targets;

// spell-checker:disable
Target("default", DependsOn("world", "exclaim", "echo", "single", "combo", "no-inputs"));

Target("hell\"o", "Says hello", () => Console.Out.WriteLineAsync("Hello"));

Target("comma", DependsOn("hell\"o"), () => Console.Out.WriteLineAsync(", "));

Target("world", DependsOn("comma"), () => Console.Out.WriteLineAsync("World"));

Target("exclaim", DependsOn("world"), () => Console.Out.WriteLineAsync("!"));

// spell-checker:enable
var foos = new[] { "a", "b", null, };
var bars = new[] { 1, 2, };

Target(
    "foo",
    "foos",
    ForEach(10, 20, 30, (int?)null), delay => Task.Delay(delay ?? 0));

Target(
    "bar",
    "bars", () => Task.Delay(1));

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
    "single",
    ForEach(foos),
    foo => Console.Out.WriteLineAsync($"{foo}"));

Target(
    "combo",
    foos.SelectMany(foo => bars.Select(bar => new { foo, bar, })),
    async o =>
    {
        await Task.Delay((4 - o.bar) * 10);
        await Console.Out.WriteLineAsync($"{o.foo},{o.bar}");
    });

Target("no-inputs", Enumerable.Empty<string>(), _ => { });

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

#pragma warning disable IDE0010 // Add missing cases to switch statement
        switch (delay)
        {
            case 10:
                throw new NotImplementedException("bad");
            case 20:
                throw new NotImplementedException("ugly");
        }
#pragma warning restore IDE0010 // Add missing cases to switch statement
    });

Target(
    "fail2",
    ForEach(2000, 1500, 200, 150),
    async delay =>
    {
        await Task.Delay(delay);

#pragma warning disable IDE0010 // Add missing cases to switch statement
        switch (delay)
        {
            case 1500:
                throw new InvalidOperationException("bad");
            case 150:
                throw new InvalidOperationException("ugly");
        }
#pragma warning restore IDE0010 // Add missing cases to switch statement
    });

var targets = new Targets();
targets.Add("abc", () => Console.Out.WriteLineAsync("abc"));
targets.Add("def", DependsOn("abc"), () => Console.Out.WriteLineAsync("def"));
targets.Add("default", DependsOn("def"));

var largeGraph = new Targets();
var largeGraphTargetNames = new List<string>();

foreach (var name in Enumerable.Range(1, 10).Select(i => i.ToString(CultureInfo.InvariantCulture)))
{
    largeGraph.Add(name, largeGraphTargetNames.TakeLast(2), () => { });
    largeGraphTargetNames.Add(name);
}

largeGraph.Add("large-graph", DependsOn(largeGraphTargetNames.Last()));

var (targetNames, options, unknownOptions, showHelp) = CommandLine.Parse(args);

if (targetNames.Contains("large-graph"))
{
    await largeGraph.RunAndExitAsync(targetNames, options, unknownOptions, showHelp);
}

if (!showHelp)
{
    await targets.RunWithoutExitingAsync(targetNames, options, unknownOptions);
}

var line = 0;

await RunTargetsAndExitAsync(args, ex => ex is InvalidOperationException, () => $"{line++} targets");
