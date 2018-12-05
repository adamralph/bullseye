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

        public static Task RunAndExitAsync(this TargetCollection targets, IEnumerable<string> args, IEnumerable<Type> exceptionMessageOnly) =>
            RunAndExitAsync(targets ?? new TargetCollection(), args.Sanitize().ToList(), new SystemConsole(), exceptionMessageOnly ?? Enumerable.Empty<Type>());

        private static async Task RunAsync(this TargetCollection targets, List<string> args, IConsole console)
        {
            var (names, options) = args.Parse();
            var log = await console.Initialize(options).ConfigureAwait(false);

            await RunAsync(targets, names, options, log, args, console).ConfigureAwait(false);
        }

        private static async Task RunAndExitAsync(this TargetCollection targets, List<string> args, IConsole console, IEnumerable<Type> exceptionMessageOnly)
        {
            var (names, options) = args.Parse();
            var log = await console.Initialize(options).ConfigureAwait(false);

            try
            {
                await RunAsync(targets, names, options, log, args, console).ConfigureAwait(false);
            }
            catch (Exception ex) when (exceptionMessageOnly.Concat(new[] { typeof(BullseyeException) }).Any(type => type.IsAssignableFrom(ex.GetType())))
            {
                await log.Error(ex.Message).ConfigureAwait(false);
                Environment.Exit(2);
            }
            catch (Exception ex)
            {
                await log.Error(ex.ToString()).ConfigureAwait(false);
                Environment.Exit(ex.HResult == 0 ? 1 : ex.HResult);
            }
        }

        private static async Task RunAsync(this TargetCollection targets, List<string> names, Options options, Logger log, List<string> args, IConsole console)
        {
            if (options.UnknownOptions.Count > 0)
            {
                throw new BullseyeException($"Unknown option{(options.UnknownOptions.Count > 1 ? "s" : "")} {options.UnknownOptions.Spaced()}. \"--help\" for usage.");
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
