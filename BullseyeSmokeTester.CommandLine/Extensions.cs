using System.CommandLine;
using Bullseye;

namespace BullseyeSmokeTester.CommandLine;

internal static class Extensions
{
    extension(Argument)
    {
        internal static Argument<string[]> Targets() => new("targets")
        {
            Description = "A list of targets to run or list. If not specified, the \"default\" target will be run, or all targets will be listed.",
            DefaultValueFactory = _ => []
        };
    }

    extension(IEnumerable<Option>)
    {
        internal static IEnumerable<Option<bool>> Bullseye() => Options.Definitions.Select(d =>
            new Option<bool>(d.Aliases[0], [.. d.Aliases.Skip(1)]) { Description = d.Description });
    }

    extension(Command cmd)
    {
        internal void AddOptions(IEnumerable<Option> options)
        {
            foreach (var option in options)
            {
                cmd.Options.Add(option);
            }
        }
    }

    extension(ParseResult result)
    {
        internal Options GetOptions(IEnumerable<Option<bool>> options) =>
            new(options.Select(o => (o.Name, result.GetValue(o))));
    }
}
