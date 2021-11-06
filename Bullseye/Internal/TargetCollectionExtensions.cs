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
        public static Task RunAsync(
            this TargetCollection targets,
            IReadOnlyCollection<string> args,
            TextWriter outputWriter,
            TextWriter diagnostics,
            string messagePrefix,
            Func<Exception, bool> messageOnly,
            bool exit)
        {
            var (options, names) = Options.Parse(args);

            return targets.RunAsync(args, names, options, outputWriter, diagnostics, messagePrefix, messageOnly, exit);
        }

        public static Task RunAsync(
            this TargetCollection targets,
            IEnumerable<string> names,
            Options options,
            TextWriter outputWriter,
            TextWriter diagnostics,
            string messagePrefix,
            Func<Exception, bool> messageOnly,
            bool exit) =>
            targets.RunAsync(new List<string>(), names.Sanitize().ToList(), options, outputWriter, diagnostics, messagePrefix, messageOnly, exit);

        private static async Task RunAsync(
            this TargetCollection targets,
            IReadOnlyCollection<string> args,
            IReadOnlyCollection<string> names,
            Options options,
            TextWriter outputWriter,
            TextWriter diagnostics,
            string messagePrefix,
            Func<Exception, bool> messageOnly,
            bool exit)
        {
            // TODO: move this to an Output level method, and pass all diagnostics messages through output
            // and write full exception details from action execution to diagnostics
            if (exit)
            {
                try
                {
                    await RunAsync(targets, args, names, options, outputWriter, diagnostics, messagePrefix, messageOnly).Tax();
                }
                catch (InvalidUsageException ex)
                {
                    await diagnostics.WriteLineAsync(ex.Message).Tax();
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
                await RunAsync(targets, args, names, options, outputWriter, diagnostics, messagePrefix, messageOnly).Tax();
            }
        }

        private static async Task RunAsync(
            this TargetCollection targets,
            IReadOnlyCollection<string> args,
            IReadOnlyCollection<string> names,
            Options options,
            TextWriter outputWriter,
            TextWriter diagnostics,
            string messagePrefix,
            Func<Exception, bool> messageOnly)
        {
            targets = targets ?? new TargetCollection();
            args = args.Sanitize().ToList();
            names = names.Sanitize().ToList();
            options = options ?? new Options();
            outputWriter = outputWriter ?? Console.Out;
            diagnostics = diagnostics ?? Console.Error;
            messagePrefix = messagePrefix ?? await GetMethodPrefix(diagnostics).Tax();
            messageOnly = messageOnly ?? (_ => false);

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
                    await diagnostics.WriteLineAsync($"{messagePrefix}: Failed to clear the console: {ex}").Tax();
                }
            }

            var noColor = options.NoColor;

            if (Environment.GetEnvironmentVariable("NO_COLOR") != null)
            {
                if (options.Verbose)
                {
                    await diagnostics.WriteLineAsync($"{messagePrefix}: NO_COLOR environment variable is set. Colored output is disabled.").Tax();
                }

                noColor = true;
            }

            var host = options.Host.DetectIfUnknown();

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
                options.Host != Host.Unknown,
                noColor,
                options.NoExtendedChars,
                operatingSystem,
                options.Parallel,
                messagePrefix,
                options.SkipDependencies,
                options.Verbose);

            var outputState = await output.Initialize(options.Verbose ? diagnostics : TextWriter.Null).Tax();

            try
            {
                await output.Header().Tax();

                await RunAsync(
                    targets,
                    output,
                    messageOnly,
                    names,
                    options.DryRun,
                    options.ListDependencies,
                    options.ListInputs,
                    options.ListTargets,
                    options.ListTree,
                    options.Parallel,
                    options.SkipDependencies,
                    options.ShowHelp,
                    options.UnknownOptions).Tax();
            }
            finally
            {
                await outputState.DisposeAsync().Tax();
            }
        }

        private static async Task RunAsync(
            this TargetCollection targets,
            Output output,
            Func<Exception, bool> messageOnly,
            IReadOnlyCollection<string> names,
            bool dryRun,
            bool listDependencies,
            bool listInputs,
            bool listTargets,
            bool listTree,
            bool parallel,
            bool skipDependencies,
            bool showHelp,
            IReadOnlyCollection<string> unknownOptions)
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

            await targets.RunAsync(names, skipDependencies, dryRun, parallel, output, messageOnly).Tax();
        }

        private static async Task<string> GetMethodPrefix(TextWriter diagnostics)
        {
            var messagePrefix = "Bullseye";

            if (Assembly.GetEntryAssembly() is Assembly entryAssembly)
            {
                messagePrefix = entryAssembly.GetName().Name;
            }
            else
            {
                await diagnostics.WriteLineAsync($"{messagePrefix}: Failed to get the entry assembly. Using default message prefix \"{messagePrefix}\".").Tax();
            }

            return messagePrefix;
        }
    }
}
