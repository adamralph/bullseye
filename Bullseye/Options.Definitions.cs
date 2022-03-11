using System.Collections.Generic;

namespace Bullseye
{
    /// <summary>
    /// The options to use when running or listing targets.
    /// </summary>
    public partial class Options
    {
        /// <summary>
        /// Gets the definitions of options which can be used when running or listing targets.
        /// </summary>
        public static IReadOnlyList<(IReadOnlyList<string> Aliases, string Description)> Definitions { get; } = new List<(IReadOnlyList<string> Aliases, string Description)>
        {
            (new List<string>{ "-c", "--clear",             }, "Clear the console before execution"),
            (new List<string>{ "-n", "--dry-run",           }, "Do a dry run without executing actions"),
            (new List<string>{ "-d", "--list-dependencies", }, "List all (or specified) targets and dependencies, then exit"),
            (new List<string>{ "-i", "--list-inputs",       }, "List all (or specified) targets and inputs, then exit"),
            (new List<string>{ "-l", "--list-targets",      }, "List all (or specified) targets, then exit"),
            (new List<string>{ "-t", "--list-tree",         }, "List all (or specified) targets and dependency trees, then exit"),
            (new List<string>{ "-N", "--no-color",          }, "Disable colored output"),
            (new List<string>{ "-E", "--no-extended-chars", }, "Disable extended characters"),
            (new List<string>{ "-p", "--parallel",          }, "Run targets in parallel"),
            (new List<string>{ "-s", "--skip-dependencies", }, "Do not run targets' dependencies"),
            (new List<string>{ "-v", "--verbose",           }, "Enable verbose output"),
            (new List<string>{       "--appveyor",          }, "Force AppVeyor mode (normally auto-detected)"),
            (new List<string>{       "--console",           }, "Force console mode (normally auto-detected)"),
            (new List<string>{       "--github-actions",    }, "Force GitHub Actions mode (normally auto-detected)"),
            (new List<string>{       "--gitlab-ci",         }, "Force GitLab CI mode (normally auto-detected)"),
            (new List<string>{       "--teamcity",          }, "Force TeamCity mode (normally auto-detected)"),
            (new List<string>{       "--travis",            }, "Force Travis CI mode (normally auto-detected)"),
        };
    }
}
