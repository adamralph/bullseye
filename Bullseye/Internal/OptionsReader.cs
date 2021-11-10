using System.Collections.Generic;

namespace Bullseye.Internal
{
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
            Read(IEnumerable<(string, bool)> values)
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
            var host = (Host?)null;
            var unknownOptions = new List<string>();

            foreach (var (name, isSet) in values)
            {
                switch (name)
                {
                    case "-c":
                    case "--clear":
                        clear = isSet;
                        break;
                    case "-n":
                    case "--dry-run":
                        dryRun = isSet;
                        break;
                    case "-d":
                    case "--list-dependencies":
                        listDependencies = isSet;
                        break;
                    case "-i":
                    case "--list-inputs":
                        listInputs = isSet;
                        break;
                    case "-l":
                    case "--list-targets":
                        listTargets = isSet;
                        break;
                    case "-t":
                    case "--list-tree":
                        listTree = isSet;
                        break;
                    case "-N":
                    case "--no-color":
                        noColor = isSet;
                        break;
                    case "-E":
                    case "--no-extended-chars":
                        noExtendedChars = isSet;
                        break;
                    case "-p":
                    case "--parallel":
                        parallel = isSet;
                        break;
                    case "-s":
                    case "--skip-dependencies":
                        skipDependencies = isSet;
                        break;
                    case "-v":
                    case "--verbose":
                        verbose = isSet;
                        break;
                    case "--appveyor":
                        host = isSet ? Host.AppVeyor : host;
                        break;
                    case "--azure-pipelines":
                        host = isSet ? Host.AzurePipelines : host;
                        break;
                    case "--console":
                        host = isSet ? Host.Console : host;
                        break;
                    case "--github-actions":
                        host = isSet ? Host.GitHubActions : host;
                        break;
                    case "--gitlab-ci":
                        host = isSet ? Host.GitLabCI : host;
                        break;
                    case "--travis":
                        host = isSet ? Host.Travis : host;
                        break;
                    case "--teamcity":
                        host = isSet ? Host.TeamCity : host;
                        break;
                    default:
                        unknownOptions.Add(name);
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
}
