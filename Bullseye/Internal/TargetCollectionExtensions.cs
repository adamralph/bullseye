namespace Bullseye.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using static Color;

    public static class TargetCollectionExtensions
    {
        public static Task RunAsync(this TargetCollection targets, IEnumerable<string> args, IConsole console) =>
            RunAsync(targets ?? new TargetCollection(), args.Sanitize().ToList(), console ?? new SystemConsole());

        private static async Task RunAsync(this TargetCollection targets, List<string> args, IConsole console)
        {
            var listDependencies = false;
            var listTargets = false;
            var showHelp = false;
            var options = new Options();

            var helpOptions = new[] { "--help", "-h", "-?" };
            var optionsArgs = args.Where(arg => arg.StartsWith("-", StringComparison.Ordinal)).ToList();
            var unknownOptions = new List<string>();

            foreach (var option in optionsArgs)
            {
                switch (option)
                {
                    case "-D":
                    case "--list-dependencies":
                        listDependencies = true;
                        break;
                    case "-T":
                    case "--list-targets":
                        listTargets = true;
                        break;
                    case "-n":
                    case "--dry-run":
                        options.DryRun = true;
                        break;
                    case "-N":
                    case "--no-color":
                        options.NoColor = true;
                        break;
                    case "-s":
                    case "--skip-dependencies":
                        options.SkipDependencies = true;
                        break;
                    default:
                        if (helpOptions.Contains(option, StringComparer.OrdinalIgnoreCase))
                        {
                            showHelp = true;
                        }
                        else
                        {
                            unknownOptions.Add(option);
                        }

                        break;
                }
            }

            if (unknownOptions.Any())
            {
                throw new Exception("Unknown options {unknownOptions.Quote()}. \"--help\" for usage.");
            }

            if (showHelp)
            {
                await console.Out.WriteLineAsync(GetUsage(options.NoColor)).ConfigureAwait(false);
                return;
            }

            if (listDependencies)
            {
                await console.Out.WriteLineAsync(targets.ToDependencyString(options.NoColor)).ConfigureAwait(false);
                return;
            }

            if (listTargets)
            {
                await console.Out.WriteLineAsync(targets.ToListString()).ConfigureAwait(false);
                return;
            }

            var names = args.Where(arg => !arg.StartsWith("-")).ToList();
            if (!names.Any())
            {
                names.Add("default");
            }

            var log = new Logger(console, options);

            await log.Running(names).ConfigureAwait(false);
            var stopWatch = Stopwatch.StartNew();

            try
            {
                await RunAsync(targets, names, options.SkipDependencies, options.DryRun, log).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await log.Failed(names, ex, stopWatch.Elapsed.TotalMilliseconds).ConfigureAwait(false);
                throw;
            }

            await log.Succeeded(names, stopWatch.Elapsed.TotalMilliseconds).ConfigureAwait(false);
        }

        private static async Task RunAsync(TargetCollection targets, List<string> names, bool skipDependencies, bool dryRun, Logger log)
        {
            if (!skipDependencies)
            {
                targets.ValidateDependencies();
            }

            targets.Validate(names);

            var targetsRan = new HashSet<string>();
            foreach (var name in names)
            {
                await targets.RunAsync(name, skipDependencies, dryRun, targetsRan, log).ConfigureAwait(false);
            }
        }

        private static async Task RunAsync(this TargetCollection targets, string name, bool skipDependencies, bool dryRun, ISet<string> targetsRan, Logger log)
        {
            if (!targetsRan.Add(name))
            {
                return;
            }

            var target = targets[name];

            if (!skipDependencies)
            {
                foreach (var dependency in target.Dependencies)
                {
                    await targets.RunAsync(dependency, skipDependencies, dryRun, targetsRan, log).ConfigureAwait(false);
                }
            }

            await target.RunAsync(dryRun, log).ConfigureAwait(false);
        }

        private static string ToListString(this TargetCollection targets)
        {
            var value = new StringBuilder();
            foreach (var target in targets.OrderBy(target => target.Name))
            {
                value.AppendLine(target.Name);
            }

            return value.ToString();
        }

        private static string ToDependencyString(this TargetCollection targets, bool noColor)
        {
            var value = new StringBuilder();
            foreach (var target in targets.OrderBy(target => target.Name))
            {
                value.AppendLine(target.Name);
                foreach (var dependency in target.Dependencies)
                {
                    value.AppendLine($"  {White(noColor)}{dependency}{Default(noColor)}");
                }
            }

            return value.ToString();
        }

        private static void ValidateDependencies(this TargetCollection targets)
        {
            var unknownDependencies = new SortedDictionary<string, SortedSet<string>>();

            foreach (var target in targets)
            {
                foreach (var dependency in target.Dependencies
                    .Where(dependency => !targets.Contains(dependency)))
                {
                    (unknownDependencies.TryGetValue(dependency, out var set)
                            ? set
                            : unknownDependencies[dependency] = new SortedSet<string>())
                        .Add(target.Name);
                }
            }

            if (!unknownDependencies.Any())
            {
                return;
            }

            var message = $"Missing {(unknownDependencies.Count > 1 ? "dependencies" : "dependency")} detected: " +
                string.Join(
                    "; ",
                    unknownDependencies.Select(missingDependency =>
                        $@"{missingDependency.Key.Quote()}, required by {missingDependency.Value.Quote()}"));

            throw new Exception(message);
        }

        private static void Validate(this TargetCollection targets, List<string> names)
        {
            var unknownNames = new SortedSet<string>(names.Except(targets.Select(target => target.Name)));
            if (unknownNames.Any())
            {
                var message = $"The following target{(unknownNames.Count > 1 ? "s were" : " was")} not found: {unknownNames.Quote()}.";
                throw new Exception(message);
            }
        }

        public static string GetUsage(bool noColor) =>
$@"{Cyan(noColor)}Usage:{Default(noColor)} {BrightYellow(noColor)}<command-line>{Default(noColor)} {White(noColor)}[<options>]{Default(noColor)} [<targets>]

{Cyan(noColor)}command-line: {Default(noColor)}The command line which invokes the build targets.{Default(noColor)}
  {Cyan(noColor)}Examples:{Default(noColor)}
    {BrightYellow(noColor)}build.cmd{Default(noColor)}
    {BrightYellow(noColor)}build.sh{Default(noColor)}
    {BrightYellow(noColor)}dotnet run --project targets --{Default(noColor)}

{Cyan(noColor)}options:{Default(noColor)}
 {White(noColor)}-D, --list-dependencies    {Default(noColor)}Display the targets and dependencies, then exit
 {White(noColor)}-T, --list-targets         {Default(noColor)}Display the targets, then exit
 {White(noColor)}-n, --dry-run              {Default(noColor)}Do a dry run without executing actions
 {White(noColor)}-N, --no-color             {Default(noColor)}Disable colored output
 {White(noColor)}-s, --skip-dependencies    {Default(noColor)}Do not run targets' dependencies

{Cyan(noColor)}targets: {Default(noColor)}A list of targets to run. If not specified, the ""default"" target will be run.

{Cyan(noColor)}Examples:{Default(noColor)}
  {BrightYellow(noColor)}build.cmd{Default(noColor)}
  {BrightYellow(noColor)}build.cmd{Default(noColor)} {White(noColor)}-D{Default(noColor)}
  {BrightYellow(noColor)}build.sh{Default(noColor)} test pack
  {BrightYellow(noColor)}dotnet run --project targets --{Default(noColor)} {White(noColor)}-n{Default(noColor)} build";
    }
}
