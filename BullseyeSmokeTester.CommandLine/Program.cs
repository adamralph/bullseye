using System.CommandLine;
using System.Linq;
using Bullseye;
using static Bullseye.Targets;

var foo = new Option<string>(new[] { "--foo", "-f", }, "A value used for something.");

#pragma warning disable IDE0028
var cmd = new RootCommand { foo, };

// translate from Bullseye to System.CommandLine
cmd.Add(new Argument<string[]>("targets") { Description = "A list of targets to run or list. If not specified, the \"default\" target will be run, or all targets will be listed.", });
#pragma warning restore IDE0028
foreach (var (aliases, description) in Options.Definitions)
{
    cmd.Add(new Option<bool>(aliases.ToArray(), description));
}

cmd.SetHandler(async () =>
{
    // translate from System.CommandLine to Bullseye
    var cmdLine = cmd.Parse(args);
    var targets = cmdLine.CommandResult.Tokens.Select(token => token.Value);
    var options = new Options(Options.Definitions.Select(d => (d.Aliases[0], cmdLine.GetValueForOption(cmd.Options.OfType<Option<bool>>().Single(o => o.HasAlias(d.Aliases[0]))))));

    Target("build", async () => await System.Console.Out.WriteLineAsync($"foo = {cmdLine.GetValueForOption(foo)}"));

    Target("default", DependsOn("build"));

    await RunTargetsAndExitAsync(targets, options);
});

return await cmd.InvokeAsync(args);
