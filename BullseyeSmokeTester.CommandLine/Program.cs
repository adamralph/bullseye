using System.CommandLine;
using Bullseye;
using static Bullseye.Targets;

var foo = new Option<string>("--foo", "-f") { Description = "A value used for something." };

#pragma warning disable IDE0028 // Use collection initializers or expressions
var cmd = new RootCommand { foo, };

// translate from Bullseye to System.CommandLine
cmd.Add(new Argument<string[]>("targets") { Description = "A list of targets to run or list. If not specified, the \"default\" target will be run, or all targets will be listed.", });
#pragma warning restore IDE0028
foreach (var (aliases, description) in Options.Definitions)
{
    cmd.Add(new Option<bool>(aliases[0], [.. aliases.Skip(1)]) { Description = description });
}

cmd.SetAction(cmdLine =>
{
    // translate from System.CommandLine to Bullseye
    var targets = cmdLine.CommandResult.Tokens.Select(token => token.Value);
    var options = new Options(
        Options.Definitions
            .Select(d => d.Aliases[0])
            .Select(alias => (
                alias,
                cmdLine.GetValue(
                    cmd.Options
                        .OfType<Option<bool>>()
                        .Single(o => o.Name == alias || o.Aliases.Contains(alias))))));

    Target("build", () => Console.Out.WriteLineAsync($"foo = {cmdLine.GetValue(foo)}"));

    Target("default", dependsOn: ["build"]);

    return RunTargetsAndExitAsync(targets, options);
});

return await cmd.Parse(args).InvokeAsync();
