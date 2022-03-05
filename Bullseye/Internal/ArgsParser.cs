using System;
using System.Collections.Generic;
using System.Linq;

namespace Bullseye.Internal
{
    public static class ArgsParser
    {
        private static readonly IReadOnlyList<string> helpOptions = new List<string> { "--help", "-help", "/help", "-h", "/h", "-?", "/?" };

        public static (IReadOnlyList<string> Targets, Options Options, IReadOnlyList<string> UnknownOptions, bool showHelp) Parse(IReadOnlyCollection<string> args)
        {
            var nonHelpArgs = args.Where(arg => !helpOptions.Contains(arg, StringComparer.OrdinalIgnoreCase)).ToList();

            var targets = nonHelpArgs.Where(arg => !arg.StartsWith("-", StringComparison.Ordinal)).ToList();

            var optionArgs = nonHelpArgs.Where(arg => arg.StartsWith("-", StringComparison.Ordinal)).Select(arg => (arg, true));
            var result = OptionsReader.Read(optionArgs);

            var options = new Options
            {
                Clear = result.Clear,
                DryRun = result.DryRun,
                Host = result.Host,
                ListDependencies = result.ListDependencies,
                ListInputs = result.ListInputs,
                ListTargets = result.ListTargets,
                ListTree = result.ListTree,
                NoColor = result.NoColor,
                NoExtendedChars = result.NoExtendedChars,
                Parallel = result.Parallel,
                SkipDependencies = result.SkipDependencies,
                Verbose = result.Verbose,
            };

            var showHelp = nonHelpArgs.Count != args.Count;

            return (targets, options, result.UnknownOptions, showHelp);
        }
    }
}
