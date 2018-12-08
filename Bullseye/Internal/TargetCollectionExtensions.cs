namespace Bullseye.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public static class TargetCollectionExtensions
    {
        public static Task RunAsync(this TargetCollection targets, IEnumerable<string> args, IConsole console) =>
            RunAsync(targets ?? new TargetCollection(), args.Sanitize().ToList(), console ?? new SystemConsole());

        public static Task RunAndExitAsync(this TargetCollection targets, IEnumerable<string> args, Func<Exception, bool> messageOnly) =>
            RunAndExitAsync(targets ?? new TargetCollection(), args.Sanitize().ToList(), new SystemConsole(), messageOnly ?? (ex => false));

        private static async Task RunAsync(this TargetCollection targets, List<string> args, IConsole console)
        {
            var (names, options) = args.Parse();
            var log = await console.Initialize(options).ConfigureAwait(false);

            await RunAsync(targets, names, options, log, args).ConfigureAwait(false);
        }

        private static async Task RunAndExitAsync(this TargetCollection targets, List<string> args, IConsole console, Func<Exception, bool> messageOnly)
        {
            var (names, options) = args.Parse();
            var log = await console.Initialize(options).ConfigureAwait(false);

            try
            {
                await RunAsync(targets, names, options, log, args).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (typeof(InvalidUsageException).IsAssignableFrom(ex.GetType()) || messageOnly(ex))
                {
                    await log.Error(ex.Message).ConfigureAwait(false);
                    Environment.Exit(2);
                }

                await log.Error(ex.ToString()).ConfigureAwait(false);
                Environment.Exit(ex.HResult == 0 ? 1 : ex.HResult);
            }

            Environment.Exit(0);
        }

        private static async Task RunAsync(this TargetCollection targets, List<string> names, Options options, Logger log, List<string> args)
        {
            if (options.UnknownOptions.Count > 0)
            {
                throw new InvalidUsageException($"Unknown option{(options.UnknownOptions.Count > 1 ? "s" : "")} {options.UnknownOptions.Spaced()}. \"--help\" for usage.");
            }

            await log.Verbose($"Args: {string.Join(" ", args)}").ConfigureAwait(false);

            if (options.ShowHelp)
            {
                await log.Usage().ConfigureAwait(false);
                return;
            }

            if (options.ListTree || options.ListDependencies || options.ListInputs || options.ListTargets)
            {
                var rootTargets = names.Any() ? names : targets.Select(target => target.Name).OrderBy(name => name).ToList();
                var maxDepth = options.ListTree ? int.MaxValue : options.ListDependencies ? 1 : 0;
                var maxDepthToShowInputs = options.ListTree ? int.MaxValue : 0;
                await log.Targets(targets, rootTargets, maxDepth, maxDepthToShowInputs, options.ListInputs).ConfigureAwait(false);
                return;
            }

            if (names.Count == 0)
            {
                names.Add("default");
            }

            await targets.RunAsync(names, options.SkipDependencies, options.DryRun, options.Parallel, log).ConfigureAwait(false);
        }
    }
}
