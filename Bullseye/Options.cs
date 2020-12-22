namespace Bullseye
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// The options to use when running or listing targets.
    /// </summary>
    public class Options
    {
        private readonly List<string> unknownOptions = new List<string>();

        /// <summary>
        /// Gets the definitions of options which can be used when running or listing targets.
        /// </summary>
        public static IEnumerable<OptionDefinition> Definitions { get; } = new List<OptionDefinition>()
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
            new OptionDefinition(null, "--appveyor",          "Force Appveyor mode (normally auto-detected)"),
            new OptionDefinition(null, "--azure-pipelines",   "Force Azure Pipelines mode (normally auto-detected)"),
            new OptionDefinition(null, "--github-actions",    "Force GitHub Actions mode (normally auto-detected)"),
            new OptionDefinition(null, "--gitlab-ci",         "Force GitLab CI mode (normally auto-detected)"),
            new OptionDefinition(null, "--teamcity",          "Force TeamCity mode (normally auto-detected)"),
            new OptionDefinition(null, "--travis",            "Force Travis CI mode (normally auto-detected)"),
        };

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
            var helpOptions = new[] { "--help", "-h", "-?" };

            foreach (var (name, isSet) in values ?? Enumerable.Empty<(string, bool)>())
            {
                switch (name)
                {
                    case "-c":
                    case "--clear":
                        this.Clear = isSet;
                        break;
                    case "-n":
                    case "--dry-run":
                        this.DryRun = isSet;
                        break;
                    case "-d":
                    case "--list-dependencies":
                        this.ListDependencies = isSet;
                        break;
                    case "-i":
                    case "--list-inputs":
                        this.ListInputs = isSet;
                        break;
                    case "-l":
                    case "--list-targets":
                        this.ListTargets = isSet;
                        break;
                    case "-t":
                    case "--list-tree":
                        this.ListTree = isSet;
                        break;
                    case "-N":
                    case "--no-color":
                        this.NoColor = isSet;
                        break;
                    case "-E":
                    case "--no-extended-chars":
                        this.NoExtendedChars = isSet;
                        break;
                    case "-p":
                    case "--parallel":
                        this.Parallel = isSet;
                        break;
                    case "-s":
                    case "--skip-dependencies":
                        this.SkipDependencies = isSet;
                        break;
                    case "-v":
                    case "--verbose":
                        this.Verbose = isSet;
                        break;
                    case "--appveyor":
                        if (isSet)
                        {
                            this.Host = Host.Appveyor;
                        }

                        break;
                    case "--azure-pipelines":
                        if (isSet)
                        {
                            this.Host = Host.AzurePipelines;
                        }

                        break;
                    case "--github-actions":
                        if (isSet)
                        {
                            this.Host = Host.GitHubActions;
                        }

                        break;
                    case "--gitlab-ci":
                        if (isSet)
                        {
                            this.Host = Host.GitLabCI;
                        }

                        break;
                    case "--travis":
                        if (isSet)
                        {
                            this.Host = Host.Travis;
                        }

                        break;
                    case "--teamcity":
                        if (isSet)
                        {
                            this.Host = Host.TeamCity;
                        }

                        break;
                    default:
                        if (helpOptions.Contains(name, StringComparer.OrdinalIgnoreCase))
                        {
                            this.ShowHelp = isSet;
                        }
                        else
                        {
                            this.unknownOptions.Add(name);
                        }

                        break;
                }
            }
        }

        /// <summary>
        /// Converts a list of argument strings into an instance of <see cref="Options"/> and a list of target names.
        /// </summary>
        /// <param name="args">A list of argument strings.</param>
        /// <returns>An instance of <see cref="Options"/> and a list of target names.</returns>
        public static (Options, List<string>) Parse(IEnumerable<string> args) => (
            new Options(args.Where(arg => arg.StartsWith("-", StringComparison.Ordinal)).Select(arg => (arg, true))),
            args.Where(arg => !arg.StartsWith("-", StringComparison.Ordinal)).ToList());

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
        /// Gets or sets a value indicating whether to force the mode for a specific host environment (normally auto-detected).
        /// If the value is set to <see cref="Host.Unknown"/> (default), then no mode is forced.
        /// </summary>
        public Host Host { get; set; }

        /// <summary>
        /// Gets a value indicating whether to show help and exit.
        /// </summary>
        public bool ShowHelp { get; }

        /// <summary>
        /// Gets the list of unknown options represented by the named values passed to the constructor.
        /// </summary>
        public IReadOnlyList<string> UnknownOptions => this.unknownOptions;
    }
}
