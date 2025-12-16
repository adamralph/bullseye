using Bullseye;
using McMaster.Extensions.CommandLineUtils;

namespace BullseyeSmokeTester.McMaster;

internal static class Extensions
{
    extension(CommandLineApplication app)
    {
        internal CommandArgument TargetsArgument() => app.Argument(
            "targets",
            "A list of targets to run or list. If not specified, the \"default\" target will be run, or all targets will be listed.",
            multipleValues: true);

        internal IReadOnlyList<CommandOption> BullseyeOptions() => [.. Options.Definitions.Select(
            d => app.Option<bool>(string.Join("|", d.Aliases), d.Description, CommandOptionType.NoValue)),];
    }

    extension(IEnumerable<CommandOption> options)
    {
        internal Options GetOptions() => new(Options.Definitions.Select(d =>
            (d.Aliases[0], options.Single(o => d.Aliases.Contains($"--{o.LongName}")).HasValue())));
    }
}
