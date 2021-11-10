using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
            string? messagePrefix,
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
                messagePrefix ?? await GetMessagePrefix(diagnosticsWriter).Tax(),
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
            string? messagePrefix,
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
                messagePrefix ?? await GetMessagePrefix(diagnosticsWriter).Tax(),
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
            string messagePrefix,
            TextWriter outputWriter,
            TextWriter diagnosticsWriter,
            bool exit)
        {
            if (exit)
            {
                try
                {
                    await targets.RunAsync(args, names, options, unknownOptions, showHelp, messageOnly, messagePrefix, outputWriter, diagnosticsWriter).Tax();
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
                await targets.RunAsync(args, names, options, unknownOptions, showHelp, messageOnly, messagePrefix, outputWriter, diagnosticsWriter).Tax();
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
            string messagePrefix,
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
                    await diagnosticsWriter.WriteLineAsync($"{messagePrefix}: Failed to clear the console: {ex}").Tax();
                }
            }

            var noColor = options.NoColor;

            if (Environment.GetEnvironmentVariable("NO_COLOR") != null)
            {
                if (options.Verbose)
                {
                    await diagnosticsWriter.WriteLineAsync($"{messagePrefix}: NO_COLOR environment variable is set. Colored output is disabled.").Tax();
                }

                noColor = true;
            }

            var host = options.Host.DetectIfNull();

            var operatingSystem =
                RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? OperatingSystem.Windows
                    : RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                        ? OperatingSystem.Linux
                        : RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
                            ? OperatingSystem.MacOS
                            : OperatingSystem.Unknown;

            var output = new Output(
                outputWriter,
                args,
                options.DryRun,
                host,
                options.Host.HasValue,
                noColor,
                options.NoExtendedChars,
                operatingSystem,
                options.Parallel,
                messagePrefix,
                options.SkipDependencies,
                options.Verbose);

            var outputState = await output.Initialize(options.Verbose ? diagnosticsWriter : TextWriter.Null).Tax();

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

            if (listTree || listDependencies || listInputs || listTargets)
            {
                var rootTargets = names.Any() ? names : targets.Select(target => target.Name).OrderBy(name => name).ToList();
                var maxDepth = listTree ? int.MaxValue : listDependencies ? 1 : 0;
                var maxDepthToShowInputs = listTree ? int.MaxValue : 0;

                await output.List(targets, rootTargets, maxDepth, maxDepthToShowInputs, listInputs).Tax();
                return;
            }

            names = names.Count > 0 ? names : new List<string> { "default" };

            await targets.RunAsync(names, dryRun, parallel, skipDependencies, messageOnly, output).Tax();
        }

        private static async Task<string> GetMessagePrefix(TextWriter diagnosticsWriter)
        {
            var messagePrefix = "Bullseye";

            if (Assembly.GetEntryAssembly() is Assembly entryAssembly)
            {
                messagePrefix = entryAssembly.GetName().Name;
            }
            else
            {
                await diagnosticsWriter.WriteLineAsync($"{messagePrefix}: Failed to get the entry assembly. Using default message prefix \"{messagePrefix}\".").Tax();
            }

            return messagePrefix;
        }
    }
}
