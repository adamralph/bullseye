namespace Bullseye.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using static Color;

    public static class DictionaryExtensions
    {
        public static async Task<int> RunAsync(this IDictionary<string, Target> targets, List<string> args, IConsole console)
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
                await console.Error.WriteLineAsync($"Unknown options {unknownOptions.Quote()}. \"--help\" for usage.");
                return 1;
            }

            if (showHelp)
            {
                await console.Out.WriteLineAsync(GetUsage(options.NoColor));
                return 0;
            }

            if (listDependencies)
            {
                await console.Out.WriteLineAsync(targets.ToDependencyString(options.NoColor));
                return 0;
            }

            if (listTargets)
            {
                await console.Out.WriteLineAsync(targets.ToListString());
                return 0;
            }

            var names = args.Where(arg => !arg.StartsWith("-")).ToList();
            if (!names.Any())
            {
                names.Add("default");
            }

            await console.Out.WriteLineAsync(names.ToTargetsRunning(options));
            var stopWatch = Stopwatch.StartNew();

            try
            {
                await RunAsync(targets, names, options, console);
            }
            catch (Exception ex)
            {
                await console.Out.WriteLineAsync(names.ToTargetsFailed(options, stopWatch.Elapsed.TotalMilliseconds));
                await console.Error.WriteLineAsync(ex.ToString());
                return 1;
            }

            await console.Out.WriteLineAsync(names.ToTargetsSucceeded(options, stopWatch.Elapsed.TotalMilliseconds));

            return 0;
        }

        private static async Task RunAsync(IDictionary<string, Target> targets, List<string> names, Options options, IConsole console)
        {
            if (!options.SkipDependencies)
            {
                targets.ValidateDependencies();
            }

            targets.Validate(names);

            var targetsRan = new HashSet<string>();
            foreach (var name in names)
            {
                await targets.RunAsync(name, options, targetsRan, console);
            }
        }

        private static async Task RunAsync(this IDictionary<string, Target> targets, string name, Options options, ISet<string> targetsRan, IConsole console)
        {
            if (!targetsRan.Add(name))
            {
                return;
            }

            var target = targets[name];

            if (!options.SkipDependencies)
            {
                foreach (var dependency in target.Dependencies)
                {
                    await targets.RunAsync(dependency, options, targetsRan, console);
                }
            }

            if (target.Action != default)
            {
                await console.Out.WriteLineAsync(name.ToTargetStarting(options));
                var stopWatch = Stopwatch.StartNew();

                if (!options.DryRun)
                {
                    try
                    {
                        await target.Action();
                    }
                    catch (Exception ex)
                    {
                        await console.Out.WriteLineAsync(name.ToTargetFailed(ex, options, stopWatch.Elapsed.TotalMilliseconds));
                        throw;
                    }
                }

                await console.Out.WriteLineAsync(name.ToTargetSucceeded(options, stopWatch.Elapsed.TotalMilliseconds));
            }
        }

        public static string ToListString(this IDictionary<string, Target> targets)
        {
            var value = new StringBuilder();
            foreach (var target in targets.OrderBy(pair => pair.Key))
            {
                value.AppendLine(target.Key);
            }

            return value.ToString();
        }

        public static string ToDependencyString(this IDictionary<string, Target> targets, bool noColor)
        {
            var value = new StringBuilder();
            foreach (var target in targets.OrderBy(pair => pair.Key))
            {
                value.AppendLine(target.Key);
                foreach (var dependency in target.Value.Dependencies)
                {
                    value.AppendLine($"  {White(noColor)}{dependency}{Default(noColor)}");
                }
            }

            return value.ToString();
        }

        private static void ValidateDependencies(this IDictionary<string, Target> targets)
        {
            var unknownDependencies = new SortedDictionary<string, SortedSet<string>>();

            foreach (var targetEntry in targets)
            {
                foreach (var dependency in targetEntry.Value.Dependencies
                    .Where(dependency => !targets.ContainsKey(dependency)))
                {
                    (unknownDependencies.TryGetValue(dependency, out var set)
                            ? set
                            : unknownDependencies[dependency] = new SortedSet<string>())
                        .Add(targetEntry.Key);
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

        private static void Validate(this IDictionary<string, Target> targets, List<string> names)
        {
            var unknownNames = new SortedSet<string>(names.Except(targets.Keys));
            if (unknownNames.Any())
            {
                var message = $"The following target{(unknownNames.Count > 1 ? "s were" : " was")} not found: {unknownNames.Quote()}.";
                throw new Exception(message);
            }
        }

        public static string GetUsage(bool noColor) =>
$@"{Cyan(noColor)}Usage:{Default(noColor)} {BrightYellow(noColor)}<command-line>{Default(noColor)} {White(noColor)}[<options>]{Default(noColor)} [<targets>]

{Cyan(noColor)}command-line: {Default(noColor)}The command line which invokes the application. E.g. {BrightYellow(noColor)}dotnet exec Foo.dll{Default(noColor)}.

{Cyan(noColor)}options:{Default(noColor)}
 {White(noColor)}-D, --list-dependencies    {Default(noColor)}Display the targets and dependencies, then exit
 {White(noColor)}-T, --list-targets         {Default(noColor)}Display the targets, then exit
 {White(noColor)}-n, --dry-run              {Default(noColor)}Do a dry run without executing actions
 {White(noColor)}-N, --no-color             {Default(noColor)}Disable colored output
 {White(noColor)}-s, --skip-dependencies    {Default(noColor)}Do not run targets' dependencies

{Cyan(noColor)}targets: {Default(noColor)}A list of targets to run. If not specified, the ""default"" target will be run.

{Cyan(noColor)}Examples:{Default(noColor)}
  {BrightYellow(noColor)}dotnet exec Foo.dll{Default(noColor)}
  {BrightYellow(noColor)}dotnet exec Foo.dll --{Default(noColor)} {White(noColor)}-T{Default(noColor)}
  {BrightYellow(noColor)}dotnet exec Foo.dll --{Default(noColor)} test pack
  {BrightYellow(noColor)}dotnet exec Foo.dll --{Default(noColor)} {White(noColor)}-n{Default(noColor)} build";
    }
}
