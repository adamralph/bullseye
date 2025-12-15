using System.CommandLine;
using BullseyeSmokeTester.CommandLine;
using static Bullseye.Targets;

var foo = new Option<string>(["--foo", "-f"]) { Description = "A value used for something." };
var targets = Argument.Targets();
var options = IEnumerable<Option>.Bullseye().ToList();

var cmd = new RootCommand { foo, targets, };
cmd.AddOptions(options);

cmd.SetHandler(async () =>
{
    var cmdLine = cmd.Parse(args);

    Target("build", () => Console.Out.WriteLineAsync($"{nameof(foo)} = {cmdLine.GetValueForOption(foo)}"));

    Target("default", dependsOn: ["build",]);

    await RunTargetsAndExitAsync(cmdLine.GetValueForArgument(targets), cmdLine.GetOptions(options));
});

return await cmd.InvokeAsync(args);
