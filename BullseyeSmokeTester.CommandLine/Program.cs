#pragma warning disable CA1812 // https://github.com/dotnet/roslyn-analyzers/issues/5628
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
using Bullseye;
using static Bullseye.Targets;

var cmd = new RootCommand()
{
    new Option<string>( new[] { "--foo", "-f" }, "A value used for something."),
};

// translate from Bullseye to System.CommandLine
cmd.Add(new Argument("targets") { Arity = ArgumentArity.ZeroOrMore, Description = "A list of targets to run or list. If not specified, the \"default\" target will be run, or all targets will be listed." });
foreach (var option in Options.Definitions)
{
    cmd.Add(new Option(option.Aliases.ToArray(), option.Description));
}

cmd.Handler = CommandHandler.Create<string>(async foo =>
{
    // translate from System.CommandLine to Bullseye
    var cmdLine = cmd.Parse(args);
    var targets = cmdLine.CommandResult.Tokens.Select(token => token.Value);
    var options = new Options(Options.Definitions.Select(o => (o.LongName, cmdLine.ValueForOption<bool>(o.LongName))));

    Target("build", async () => await System.Console.Out.WriteLineAsync($"foo = {foo}"));

    Target("default", DependsOn("build"));

    await RunTargetsAndExitAsync(targets, options);
});

return await cmd.InvokeAsync(args);
