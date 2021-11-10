using System.Collections.Generic;
using System.Linq;
using Bullseye.Internal;

namespace Bullseye
{
    /// <summary>
    /// The options to use when running or listing targets.
    /// </summary>
    public partial class Options : IOptions
    {
        /// <summary>
        /// Constructs a new instance of <see cref="Options"/>.
        /// </summary>
        public Options()
        {
        }

        /// <summary>
        /// Constructs a new instance of <see cref="Options"/>.
        /// </summary>
        /// <param name="values">A list of named options and their values.</param>
        public Options(IEnumerable<(string, bool)> values)
        {
            var result = OptionsReader.Read(values);

            if (result.UnknownOptions.Count > 0)
            {
                throw new InvalidUsageException($"Unknown option{(result.UnknownOptions.Count > 1 ? "s" : "")} {result.UnknownOptions.Spaced()}.");
            }

            this.Clear = result.Clear;
            this.DryRun = result.DryRun;
            this.Host = result.Host;
            this.ListDependencies = result.ListDependencies;
            this.ListInputs = result.ListInputs;
            this.ListTargets = result.ListTargets;
            this.ListTree = result.ListTree;
            this.NoColor = result.NoColor;
            this.NoExtendedChars = result.NoExtendedChars;
            this.Parallel = result.Parallel;
            this.SkipDependencies = result.SkipDependencies;
            this.Verbose = result.Verbose;
        }
    }
}
