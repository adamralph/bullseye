using Bullseye;
using McMaster.Extensions.CommandLineUtils;
using static Bullseye.Targets;

using var app = new CommandLineApplication { UsePagerForHelpText = false, };
app.HelpOption();
var foo = app.Option<string>("-f|--foo <foo>", "A value used for something.", CommandOptionType.SingleValue);

// translate from Bullseye to McMaster.Extensions.CommandLineUtils
app.Argument("targets", "A list of targets to run or list. If not specified, the \"default\" target will be run, or all targets will be listed.", true);
foreach (var (aliases, description) in Options.Definitions)
{
    _ = app.Option(string.Join("|", aliases), description, CommandOptionType.NoValue);
}

app.OnExecuteAsync(_ =>
{
    // translate from McMaster.Extensions.CommandLineUtils to Bullseye
    var targets = app.Arguments[0].Values.OfType<string>();
    var options = new Options(Options.Definitions.Select(d => (d.Aliases[0], app.Options.Single(o => d.Aliases.Contains($"--{o.LongName}")).HasValue())));

    Target("build", () => Console.Out.WriteLineAsync($"foo = {foo.Value()}"));

    Target("default", DependsOn("build"));

    return RunTargetsAndExitAsync(targets, options);
});

return await app.ExecuteAsync(args);
