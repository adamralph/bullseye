using BullseyeSmokeTester.McMaster;
using McMaster.Extensions.CommandLineUtils;
using static Bullseye.Targets;

using var app = new CommandLineApplication();

_ = app.HelpOption();
var foo = app.Option<string>("-f|--foo <foo>", "A value used for something.", CommandOptionType.SingleValue);
var targets = app.TargetsArgument();
var options = app.BullseyeOptions();

app.OnExecuteAsync(async _ =>
{
    Target("build", () => Console.Out.WriteLineAsync($"{nameof(foo)} = {foo.Value()}"));

    Target("default", dependsOn: ["build",]);

    await RunTargetsAndExitAsync(targets.Values!, options.GetOptions());
});

return await app.ExecuteAsync(args);
