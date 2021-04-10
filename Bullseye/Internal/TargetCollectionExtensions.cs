using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Bullseye.Internal
{
    public static class TargetCollectionExtensions
    {
        public static Task RunAsync(this TargetCollection targets, IEnumerable<string> args, Func<Exception, bool> messageOnly, string logPrefix, bool exit)
        {
            var argList = args.Sanitize().ToList();
            var (options, names) = Options.Parse(argList);

            return RunAsync(targets, names, options, messageOnly, logPrefix, exit, log => log.Verbose(() => $"Args: {string.Join(" ", argList)}"));
        }

        public static Task RunAsync(this TargetCollection targets, IEnumerable<string> names, Options options, Func<Exception, bool> messageOnly, string logPrefix, bool exit) =>
            RunAsync(targets, names.Sanitize().ToList(), options, messageOnly, logPrefix, exit, default);

        private static async Task RunAsync(TargetCollection targets, List<string> names, Options options, Func<Exception, bool> messageOnly, string logPrefix, bool exit, Func<Logger, Task> logArgs)
        {
            targets = targets ?? new TargetCollection();
            options = options ?? new Options();
            messageOnly = messageOnly ?? (_ => false);

            if (logPrefix == null)
            {
                logPrefix = "Bullseye";
                var entryAssembly = Assembly.GetEntryAssembly();
                if (entryAssembly == null)
                {
                    await Console.Error.WriteLineAsync($"{logPrefix}: Failed to get the entry assembly. Using default log prefix \"{logPrefix}\".").Tax();
                }
                else
                {
                    logPrefix = entryAssembly.GetName().Name;
                }
            }

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
                    await Console.Error.WriteLineAsync($"{logPrefix}: Failed to clear the console: {ex}").Tax();
                }
            }

            var noColor = options.NoColor;
            if (Environment.GetEnvironmentVariable("NO_COLOR") != null)
            {
                if (options.Verbose)
                {
                    await Console.Error.WriteLineAsync($"{logPrefix}: NO_COLOR environment variable is set. Colored output is disabled.").Tax();
                }

                noColor = true;
            }

            var operatingSystem =
                RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? OperatingSystem.Windows
                    : RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                        ? OperatingSystem.Linux
                        : RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
                            ? OperatingSystem.MacOS
                            : OperatingSystem.Unknown;

            var terminal = await Terminal.TryConfigure(noColor, operatingSystem, options.Verbose ? Console.Error : NullTextWriter.Instance, logPrefix).Tax();

            try
            {
                await RunAsync(targets, names, noColor, options, messageOnly, logPrefix, exit, logArgs, operatingSystem).Tax();
            }
            finally
            {
                await terminal.DisposeAsync().Tax();
            }
        }

        private static async Task RunAsync(TargetCollection targets, List<string> names, bool noColor, Options options, Func<Exception, bool> messageOnly, string logPrefix, bool exit, Func<Logger, Task> logArgs, OperatingSystem operatingSystem)
        {
            var (host, isHostDetected) = options.Host.DetectIfUnknown();

            var palette = new Palette(noColor, options.NoExtendedChars, host, operatingSystem);
            var output = new Output(Console.Out, palette, operatingSystem);
            var log = new Logger(Console.Error, logPrefix, options.SkipDependencies, options.DryRun, options.Parallel, palette, options.Verbose);

            await log.Version(() => typeof(TargetCollectionExtensions).Assembly.GetVersion()).Tax();
            await log.Verbose(() => $"Host: {host}{(host != Host.Unknown ? $" ({(isHostDetected ? "detected" : "forced")})" : "")}").Tax();
            await log.Verbose(() => $"OS: {operatingSystem}").Tax();

            if (logArgs != null)
            {
                await logArgs(log).Tax();
            }

            if (exit)
            {
                try
                {
                    await RunAsync(targets, names, options, messageOnly, output, log).Tax();
                }
                catch (InvalidUsageException ex)
                {
                    await log.Error(ex.Message).Tax();
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
                await RunAsync(targets, names, options, messageOnly, output, log).Tax();
            }
        }

        private static async Task RunAsync(this TargetCollection targets, List<string> names, Options options, Func<Exception, bool> messageOnly, Output output, Logger log)
        {
            if (options.UnknownOptions.Count > 0)
            {
                throw new InvalidUsageException($"Unknown option{(options.UnknownOptions.Count > 1 ? "s" : "")} {options.UnknownOptions.Spaced()}. \"--help\" for usage.");
            }

            if (options.ShowHelp)
            {
                await output.Usage(targets).Tax();
                return;
            }

            if (options.ListTree || options.ListDependencies || options.ListInputs || options.ListTargets)
            {
                var rootTargets = names.Any() ? names : targets.Select(target => target.Name).OrderBy(name => name).ToList();
                var maxDepth = options.ListTree ? int.MaxValue : options.ListDependencies ? 1 : 0;
                var maxDepthToShowInputs = options.ListTree ? int.MaxValue : 0;
                await output.Targets(targets, rootTargets, maxDepth, maxDepthToShowInputs, options.ListInputs).Tax();
                return;
            }

            if (names.Count == 0)
            {
                names.Add("default");
            }

            await targets.RunAsync(names, options.SkipDependencies, options.DryRun, options.Parallel, log, messageOnly).Tax();
        }
    }
}
