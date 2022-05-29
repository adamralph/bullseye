using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Bullseye.Internal
{
    public static class TargetCollectionExtensions
    {
        public static async Task RunAsync(
            this TargetCollection targets,
            IReadOnlyCollection<string> args,
            Func<Exception, bool> messageOnly,
            Func<string> getMessagePrefix,
            TextWriter outputWriter,
            TextWriter diagnosticsWriter,
            bool exit)
        {
            var (names, options, unknownOptions, showHelp) = ArgsParser.Parse(args);

            await targets.RunAsync(
                args,
                names,
                options,
                unknownOptions,
                showHelp,
                messageOnly,
                getMessagePrefix,
                outputWriter,
                diagnosticsWriter,
                exit).Tax();
        }

        public static async Task RunAsync(
            this TargetCollection targets,
            IReadOnlyCollection<string> names,
            IOptions options,
            IReadOnlyCollection<string> unknownOptions,
            bool showHelp,
            Func<Exception, bool> messageOnly,
            Func<string> getMessagePrefix,
            TextWriter outputWriter,
            TextWriter diagnosticsWriter,
            bool exit) =>
            await targets.RunAsync(
                new List<string>(),
                names,
                options,
                unknownOptions,
                showHelp,
                messageOnly,
                getMessagePrefix,
                outputWriter,
                diagnosticsWriter,
                exit).Tax();

        private static async Task RunAsync(
            this TargetCollection targets,
            IReadOnlyCollection<string> args,
            IReadOnlyCollection<string> names,
            IOptions options,
            IReadOnlyCollection<string> unknownOptions,
            bool showHelp,
            Func<Exception, bool> messageOnly,
            Func<string> getMessagePrefix,
            TextWriter outputWriter,
            TextWriter diagnosticsWriter,
            bool exit)
        {
            if (exit)
            {
                try
                {
                    await targets.RunAsync(args, names, options, unknownOptions, showHelp, messageOnly, getMessagePrefix, outputWriter, diagnosticsWriter).Tax();
                }
                catch (InvalidUsageException ex)
                {
                    await diagnosticsWriter.WriteLineAsync(ex.Message).Tax();
                    Environment.Exit(2);
                }
                catch (TargetFailedException)
                {
                    Environment.Exit(1);
                }

                Environment.Exit(0);
            }
            else
            {
                await targets.RunAsync(args, names, options, unknownOptions, showHelp, messageOnly, getMessagePrefix, outputWriter, diagnosticsWriter).Tax();
            }
        }

        private static async Task RunAsync(
            this TargetCollection targets,
            IReadOnlyCollection<string> args,
            IReadOnlyCollection<string> names,
            IOptions options,
            IReadOnlyCollection<string> unknownOptions,
            bool showHelp,
            Func<Exception, bool> messageOnly,
            Func<string> getMessagePrefix,
            TextWriter outputWriter,
            TextWriter diagnosticsWriter)
        {
            if (options.Clear)
            {
                try
                {
                    Console.Clear();
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
                {
                    await diagnosticsWriter.WriteLineAsync($"{getMessagePrefix()}: Failed to clear the console: {ex}").Tax();
                }
            }

            var noColor = options.NoColor;

            if (Environment.GetEnvironmentVariable("NO_COLOR") != null)
            {
                if (options.Verbose)
                {
                    await diagnosticsWriter.WriteLineAsync($"{getMessagePrefix()}: NO_COLOR environment variable is set. Colored output is disabled.").Tax();
                }

                noColor = true;
            }

            var host = options.Host.DetectIfNull();

            var osPlatform =
                RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? OSPlatform.Windows
                    : RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                        ? OSPlatform.Linux
                        : RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
                            ? OSPlatform.OSX
                            : OSPlatform.Create("Unknown");

            var output = new Output(
                outputWriter,
                diagnosticsWriter,
                args,
                options.DryRun,
                host,
                options.Host != null,
                noColor,
                options.NoExtendedChars,
                osPlatform,
                options.Parallel,
                getMessagePrefix,
                options.SkipDependencies,
                options.Verbose);

            var outputState = await output.Initialize().Tax();

            try
            {
                await output.Header(() => typeof(TargetCollection).Assembly.GetVersion()).Tax();

                await targets.RunAsync(
                    names,
                    options.DryRun,
                    options.ListDependencies,
                    options.ListInputs,
                    options.ListTargets,
                    options.ListTree,
                    options.Parallel,
                    options.SkipDependencies,
                    unknownOptions,
                    showHelp,
                    messageOnly,
                    output).Tax();
            }
            finally
            {
                await outputState.DisposeAsync().Tax();
            }
        }

        private static async Task RunAsync(
            this TargetCollection targets,
            IReadOnlyCollection<string> names,
            bool dryRun,
            bool listDependencies,
            bool listInputs,
            bool listTargets,
            bool listTree,
            bool parallel,
            bool skipDependencies,
            IReadOnlyCollection<string> unknownOptions,
            bool showHelp,
            Func<Exception, bool> messageOnly,
            Output output)
        {
            if (unknownOptions.Count > 0)
            {
                throw new InvalidUsageException($"Unknown option{(unknownOptions.Count > 1 ? "s" : "")} {unknownOptions.Spaced()}. \"--help\" for usage.");
            }

            if (showHelp)
            {
                await output.Usage(targets).Tax();
                return;
            }

            names = targets.Expand(names).ToList();

            if (listTree || listDependencies || listInputs || listTargets)
            {
                var rootTargets = names.Count > 0 ? names : (IEnumerable<string>)targets.Select(target => target.Name).OrderBy(name => name);
                var maxDepth = listTree ? int.MaxValue : listDependencies ? 1 : 0;
                var maxDepthToShowInputs = listTree ? int.MaxValue : 0;

                await output.List(targets, rootTargets, maxDepth, maxDepthToShowInputs, listInputs).Tax();
                return;
            }

            names = names.Count > 0 ? names : new List<string> { "default", };

            await targets.RunAsync(names, dryRun, parallel, skipDependencies, messageOnly, output).Tax();
        }

        private static IEnumerable<string> Expand(this TargetCollection targets, IEnumerable<string> names)
        {
            var ambiguousNames = new List<string>();

            foreach (var name in names)
            {
                var match = targets.SingleOrDefault(target => target.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                if (match != null)
                {
                    yield return name;
                    continue;
                }

                var matches = targets.Where(target => target.Name.StartsWith(name, StringComparison.OrdinalIgnoreCase)).ToList();

                if (matches.Count > 1)
                {
                    ambiguousNames.Add($"{name} ({matches.Select(target => target.Name).Spaced()})");
                    continue;
                }

                yield return matches.Any() ? matches[0].Name : name;
            }

            if (ambiguousNames.Any())
            {
                throw new InvalidUsageException($"Ambiguous target{(ambiguousNames.Count > 1 ? "s" : "")}: {ambiguousNames.Spaced()}");
            }
        }
    }
}
