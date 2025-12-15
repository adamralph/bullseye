using System.CommandLine;
using System.CommandLine.Parsing;
using Bullseye;

namespace BullseyeSmokeTester.CommandLine;

internal static class Extensions
{
    extension(Argument)
    {
        internal static Argument<string[]> Targets() => new("targets")
        {
            Description = "A list of targets to run or list. If not specified, the \"default\" target will be run, or all targets will be listed.",
        };
    }

    extension(IEnumerable<Option>)
    {
        internal static IEnumerable<Option<bool>> Bullseye() => Options.Definitions.Select(d =>
            new Option<bool>([.. d.Aliases]) { Description = d.Description });
    }

    extension(Command cmd)
    {
        internal void AddOptions(IEnumerable<Option> options)
        {
            foreach (var option in options)
            {
                cmd.AddOption(option);
            }
        }
    }

    extension(ParseResult result)
    {
        internal Options GetOptions(IEnumerable<Option<bool>> options) =>
            new(options.Select(o => (o.Aliases.First(), result.GetValueForOption(o))));
    }
}
