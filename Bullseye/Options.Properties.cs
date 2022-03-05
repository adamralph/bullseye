namespace Bullseye
{
    /// <summary>
    /// The options to use when running or listing targets.
    /// </summary>
    public partial class Options
    {
        /// <summary>
        /// Gets or sets a value indicating whether the console should be cleared before execution.
        /// </summary>
        public bool Clear { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to do a dry run without executing actions.
        /// </summary>
        public bool DryRun { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to list all (or specified) targets and then exit.
        /// </summary>
        public bool ListDependencies { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to list all (or specified) targets and inputs and then exit.
        /// </summary>
        public bool ListInputs { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to list all (or specified) targets and then exit.
        /// </summary>
        public bool ListTargets { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to list all (or specified) targets and dependency trees and then exit.
        /// </summary>
        public bool ListTree { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to disable colored output.
        /// </summary>
        public bool NoColor { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to disable extended characters.
        /// </summary>
        public bool NoExtendedChars { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to run targets in parallel.
        /// </summary>
        public bool Parallel { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to run target's dependencies.
        /// </summary>
        public bool SkipDependencies { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to enable verbose output.
        /// </summary>
        public bool Verbose { get; set; }

        /// <summary>
        /// Gets or sets a value indicating which mode to use for a specific host environment.
        /// </summary>
        public Host Host { get; set; }
    }
}
