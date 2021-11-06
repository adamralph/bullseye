using System;
using System.Collections.Generic;
using System.Linq;
using Bullseye.Internal;

namespace Bullseye
{
    /// <summary>
    /// Contains methods for parsing command line arguments.
    /// </summary>
    public static class CommandLine
    {
        private static readonly IReadOnlyList<string> helpOptions = new List<string> { "--help", "-help", "/help", "-h", "/h", "-?", "/?" };

        /// <summary>
        /// Converts a list of argument strings into an instance of <see cref="Options"/> and a list of target names.
        /// </summary>
        /// <param name="args">A list of argument strings.</param>
        /// <returns>An instance of <see cref="Options"/> and a list of target names.</returns>
        public static (IReadOnlyList<string> Targets, Options Options, IReadOnlyList<string> UnknownOptions, bool showHelp) Parse(IEnumerable<string> args)
        {
            var argList = args.Sanitize().ToList();

            var helpArgs = argList.Where(arg => helpOptions.Contains(arg, StringComparer.OrdinalIgnoreCase));
            var nonHelpArgs = argList.Where(arg => !helpOptions.Contains(arg, StringComparer.OrdinalIgnoreCase));

            var targets = nonHelpArgs.Where(arg => !arg.StartsWith("-", StringComparison.Ordinal)).ToList();

            var nonTargets = nonHelpArgs.Where(arg => arg.StartsWith("-", StringComparison.Ordinal)).ToList();

            var optionArgs = nonTargets.Where(arg => !helpOptions.Contains(arg, StringComparer.OrdinalIgnoreCase)).Select(arg => (arg, true));
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

            return (targets, options, result.UnknownOptions, helpArgs.Any());
        }
    }
}
