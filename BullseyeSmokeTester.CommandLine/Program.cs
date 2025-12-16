using System.CommandLine;
using BullseyeSmokeTester.CommandLine;
using static Bullseye.Targets;

var foo = new Option<string>("--foo", "-f") { Description = "A value used for something.", };
var targets = Argument.Targets();
var options = IEnumerable<Option>.Bullseye().ToList();

var cmd = new RootCommand { foo, targets, };
cmd.AddOptions(options);

cmd.SetAction(async cmdLine =>
{
    Target("build", () => Console.Out.WriteLineAsync($"{nameof(foo)} = {cmdLine.GetValue(foo)}"));

    Target("default", dependsOn: ["build",]);

    await RunTargetsAndExitAsync(cmdLine.GetRequiredValue(targets), cmdLine.GetOptions(options));
});

return await cmd.Parse(args).InvokeAsync();
