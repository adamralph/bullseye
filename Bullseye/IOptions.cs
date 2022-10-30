namespace Bullseye
{
    /// <summary>
    /// The options to use when running or listing targets.
    /// </summary>
    public interface IOptions
    {
        /// <summary>
        /// Gets a value indicating whether the console should be cleared before execution.
        /// </summary>
        bool Clear { get; }

        /// <summary>
        /// Gets a value indicating whether to do a dry run without executing actions.
        /// </summary>
        bool DryRun { get; }

        /// <summary>
        /// Gets a value indicating whether to list all (or specified) targets and then exit.
        /// </summary>
        bool ListDependencies { get; }

        /// <summary>
        /// Gets a value indicating whether to list all (or specified) targets and inputs and then exit.
        /// </summary>
        bool ListInputs { get; }

        /// <summary>
        /// Gets a value indicating whether to list all (or specified) targets and then exit.
        /// </summary>
        bool ListTargets { get; }

        /// <summary>
        /// Gets a value indicating whether to list all (or specified) targets and dependency trees and then exit.
        /// </summary>
        bool ListTree { get; }

        /// <summary>
        /// Gets a value indicating whether to disable colored output.
        /// </summary>
        bool NoColor { get; }

        /// <summary>
        /// Gets a value indicating whether to disable extended characters.
        /// </summary>
        bool NoExtendedChars { get; }

        /// <summary>
        /// Gets a value indicating whether to run targets in parallel.
        /// </summary>
        bool Parallel { get; }

        /// <summary>
        /// Gets a value indicating whether to run target's dependencies.
        /// </summary>
        bool SkipDependencies { get; }

        /// <summary>
        /// Gets a value indicating whether to enable verbose output.
        /// </summary>
        bool Verbose { get; }

        /// <summary>
        /// Gets a value indicating which mode to use for a specific host environment.
        /// </summary>
        Host? Host { get; }
    }
}
