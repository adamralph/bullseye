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
        /// <summary>
        /// Converts a list of argument strings into an instance of <see cref="Options"/> and a list of target names.
        /// </summary>
        /// <param name="args">A list of argument strings.</param>
        /// <returns>An instance of <see cref="Options"/> and a list of target names.</returns>
        public static (IReadOnlyList<string> Targets, Options Options, IReadOnlyList<string> UnknownOptions, bool showHelp) Parse(IEnumerable<string> args) =>
            ArgsParser.Parse(args.Sanitize().ToList());
    }
}
