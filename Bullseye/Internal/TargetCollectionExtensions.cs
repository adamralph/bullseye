#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Bullseye.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;

    public static class TargetCollectionExtensions
    {
        public static Task RunAsync(this TargetCollection targets, IEnumerable<string> args, Func<Exception, bool> messageOnly, string logPrefix, bool exit, Func<Task> teardown)
        {
            var argList = args.Sanitize().ToList();
            var (options, names) = Options.Parse(argList);

            return RunAsync(targets, names, options, messageOnly, logPrefix, exit, teardown, log => log.Verbose(() => $"Args: {string.Join(" ", argList)}"));
        }

        public static Task RunAsync(this TargetCollection targets, IEnumerable<string> names, Options options, Func<Exception, bool> messageOnly, string logPrefix, bool exit, Func<Task> teardown) =>
            RunAsync(targets, names.Sanitize().ToList(), options, messageOnly, logPrefix, exit, teardown, default);

        private static async Task RunAsync(TargetCollection targets, List<string> names, Options options, Func<Exception, bool> messageOnly, string logPrefix, bool exit, Func<Task> teardown, Func<Logger, Task> logArgs)
        {
            targets = targets ?? new TargetCollection();
            options = options ?? new Options();
            messageOnly = messageOnly ?? (_ => false);
            teardown = teardown ?? (() => Task.CompletedTask);

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

            var operatingSystem =
                RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? OperatingSystem.Windows
                    : RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                        ? OperatingSystem.Linux
                        : RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
                            ? OperatingSystem.MacOS
                            : OperatingSystem.Unknown;

            var terminal = await Terminal.TryConfigure(options.NoColor, operatingSystem, options.Verbose ? Console.Error : NullTextWriter.Instance, logPrefix).Tax();

            try
            {
                await RunAsync(targets, names, options, messageOnly, logPrefix, exit, teardown, logArgs, operatingSystem).Tax();
            }
            finally
            {
                await teardown().Tax();
                await terminal.DisposeAsync().Tax();
            }
        }

        private static async Task RunAsync(TargetCollection targets, List<string> names, Options options, Func<Exception, bool> messageOnly, string logPrefix, bool exit, Func<Task> teardown, Func<Logger, Task> logArgs, OperatingSystem operatingSystem)
        {
            var (host, isHostDetected) = options.Host.DetectIfUnknown();

            var palette = new Palette(options.NoColor, options.NoExtendedChars, host, operatingSystem);
            var output = new Output(Console.Out, palette);
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
                    await teardown().Tax();
                    Environment.Exit(2);
                }
                catch (TargetFailedException)
                {
                    await teardown().Tax();
                    Environment.Exit(1);
                }

                await teardown().Tax();
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
