using System.Collections.Generic;

namespace Bullseye
{
    /// <summary>
    /// The options to use when running or listing targets.
    /// </summary>
    public partial class Options : IOptions
    {
        /// <summary>
        /// Gets the definitions of options which can be used when running or listing targets.
        /// </summary>
        public static IReadOnlyList<OptionDefinition> Definitions { get; } = new List<OptionDefinition>()
        {
            new OptionDefinition("-c", "--clear",             "Clear the console before execution"),
            new OptionDefinition("-n", "--dry-run",           "Do a dry run without executing actions"),
            new OptionDefinition("-d", "--list-dependencies", "List all (or specified) targets and dependencies, then exit"),
            new OptionDefinition("-i", "--list-inputs",       "List all (or specified) targets and inputs, then exit"),
            new OptionDefinition("-l", "--list-targets",      "List all (or specified) targets, then exit"),
            new OptionDefinition("-t", "--list-tree",         "List all (or specified) targets and dependency trees, then exit"),
            new OptionDefinition("-N", "--no-color",          "Disable colored output"),
            new OptionDefinition("-E", "--no-extended-chars", "Disable extended characters"),
            new OptionDefinition("-p", "--parallel",          "Run targets in parallel"),
            new OptionDefinition("-s", "--skip-dependencies", "Do not run targets' dependencies"),
            new OptionDefinition("-v", "--verbose",           "Enable verbose output"),
            new OptionDefinition("--appveyor",                "Force AppVeyor mode (normally auto-detected)"),
            new OptionDefinition("--console",                 "Force console mode (normally auto-detected)"),
            new OptionDefinition("--github-actions",          "Force GitHub Actions mode (normally auto-detected)"),
            new OptionDefinition("--gitlab-ci",               "Force GitLab CI mode (normally auto-detected)"),
            new OptionDefinition("--teamcity",                "Force TeamCity mode (normally auto-detected)"),
            new OptionDefinition("--travis",                  "Force Travis CI mode (normally auto-detected)"),
        };
    }
}
