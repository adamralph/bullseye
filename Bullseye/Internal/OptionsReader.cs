using System.Collections.Generic;

namespace Bullseye.Internal;

public static class OptionsReader
{
    public static (
        bool Clear,
        bool DryRun,
        bool ListDependencies,
        bool ListInputs,
        bool ListTargets,
        bool ListTree,
        bool NoColor,
        bool NoExtendedChars,
        bool Parallel,
        bool SkipDependencies,
        bool Verbose,
        Host? Host,
        IReadOnlyList<string> UnknownOptions)
        Read(IEnumerable<string> options)
    {
        var clear = false;
        var dryRun = false;
        var listDependencies = false;
        var listInputs = false;
        var listTargets = false;
        var listTree = false;
        var noColor = false;
        var noExtendedChars = false;
        var parallel = false;
        var skipDependencies = false;
        var verbose = false;
        Host? host = null;
        var unknownOptions = new List<string>();

        foreach (var option in options)
        {
            switch (option)
            {
                case "-c":
                case "--clear":
                    clear = true;
                    break;
                case "-n":
                case "--dry-run":
                    dryRun = true;
                    break;
                case "-d":
                case "--list-dependencies":
                    listDependencies = true;
                    break;
                case "-i":
                case "--list-inputs":
                    listInputs = true;
                    break;
                case "-l":
                case "--list-targets":
                    listTargets = true;
                    break;
                case "-t":
                case "--list-tree":
                    listTree = true;
                    break;
                case "-N":
                case "--no-color":
                    noColor = true;
                    break;
                case "-E":
                case "--no-extended-chars":
                    noExtendedChars = true;
                    break;
                case "-p":
                case "--parallel":
                    parallel = true;
                    break;
                case "-s":
                case "--skip-dependencies":
                    skipDependencies = true;
                    break;
                case "-v":
                case "--verbose":
                    verbose = true;
                    break;
                case "--appveyor":
                    host = Host.AppVeyor;
                    break;
                case "--console":
                    host = Host.Console;
                    break;
                case "--github-actions":
                    host = Host.GitHubActions;
                    break;
                case "--gitlab-ci":
                    host = Host.GitLabCI;
                    break;
                case "--teamcity":
                    host = Host.TeamCity;
                    break;
                case "--travis":
                    host = Host.Travis;
                    break;
                default:
                    unknownOptions.Add(option);
                    break;
            }
        }

        return (
            clear,
            dryRun,
            listDependencies,
            listInputs,
            listTargets,
            listTree,
            noColor,
            noExtendedChars,
            parallel,
            skipDependencies,
            verbose,
            host,
            unknownOptions);
    }
}
