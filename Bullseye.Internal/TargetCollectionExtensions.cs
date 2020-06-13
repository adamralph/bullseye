namespace Bullseye.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public static class TargetCollectionExtensions
    {
        public static async Task RunAsync(this TargetCollection targets, IEnumerable<string> args, Func<Exception, bool> messageOnly, string logPrefix, bool exit)
        {
            targets = targets ?? new TargetCollection();
            var argList = args.Sanitize().ToList();
            messageOnly = messageOnly ?? (_ => false);

            var (options, names) = Options.Parse(argList);
            var (output, log) = await ConsoleExtensions.Initialize(options, logPrefix).Tax();

            await log.Verbose($"Args: {string.Join(" ", argList)}").Tax();

            await RunAsync(targets, names, options, messageOnly, output, log, exit).Tax();
        }

        public static async Task RunAsync(this TargetCollection targets, IEnumerable<string> names, Options options, Func<Exception, bool> messageOnly, string logPrefix, bool exit)
        {
            targets = targets ?? new TargetCollection();
            var nameList = names.Sanitize().ToList();
            options = options ?? new Options();
            messageOnly = messageOnly ?? (_ => false);

            var (output, log) = await ConsoleExtensions.Initialize(options, logPrefix).Tax();

            await RunAsync(targets, nameList, options, messageOnly, output, log, exit).Tax();
        }

        private static async Task RunAsync(TargetCollection targets, List<string> names, Options options, Func<Exception, bool> messageOnly, Output output, Logger log, bool exit)
        {
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
