namespace Bullseye.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class StringExtensions
    {
        public static string Spaced(this IEnumerable<string> strings) => string.Join(" ", strings);

        public static (List<string>, Options) Parse(this IEnumerable<string> args)
        {
            var targetNames = new List<string>();
            var options = new Options();

            var helpOptions = new[] { "--help", "-h", "-?" };

            foreach (var arg in args)
            {
                switch (arg)
                {
                    case "-c":
                    case "--clear":
                        options.Clear = true;
                        break;
                    case "-n":
                    case "--dry-run":
                        options.DryRun = true;
                        break;
                    case "-D":
                    case "--list-dependencies":
                        options.ListDependencies = true;
                        break;
                    case "-I":
                    case "--list-inputs":
                        options.ListInputs = true;
                        break;
                    case "-T":
                    case "--list-targets":
                        options.ListTargets = true;
                        break;
                    case "-t":
                    case "--list-tree":
                        options.ListTree = true;
                        break;
                    case "-N":
                    case "--no-color":
                        options.NoColor = true;
                        break;
                    case "-p":
                    case "--parallel":
                        options.Parallel = true;
                        break;
                    case "-s":
                    case "--skip-dependencies":
                        options.SkipDependencies = true;
                        break;
                    case "-v":
                    case "--verbose":
                        options.Verbose = true;
                        break;
                    case "--appveyor":
                        options.Host = Host.Appveyor;
                        break;
                    case "--azure-pipelines":
                        options.Host = Host.AzurePipelines;
                        break;
                    case "--travis":
                        options.Host = Host.Travis;
                        break;
                    case "--teamcity":
                        options.Host = Host.TeamCity;
                        break;
                    default:
                        if (helpOptions.Contains(arg, StringComparer.OrdinalIgnoreCase))
                        {
                            options.ShowHelp = true;
                        }
                        else if (arg.StartsWith("-"))
                        {
                            options.UnknownOptions.Add(arg);
                        }
                        else
                        {
                            targetNames.Add(arg);
                        }

                        break;
                }
            }

            return (targetNames, options);
        }

        // pad right printed
        public static string Prp(this string text, int totalWidth, char paddingChar) =>
            text.PadRight(totalWidth + (text.Length - Palette.StripColours(text).Length), paddingChar);
    }
}
