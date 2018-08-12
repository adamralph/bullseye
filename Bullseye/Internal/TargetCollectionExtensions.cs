namespace Bullseye.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

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
            var noColor = false;
            var clear = false;

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
                        noColor = true;
                        break;
                    case "-s":
                    case "--skip-dependencies":
                        options.SkipDependencies = true;
                        break;
                    case "-c":
                    case "--clear":
                        clear = true;
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
                throw new Exception($"Unknown options {unknownOptions.Spaced()}. \"--help\" for usage.");
            }

            if (clear)
            {
                console.Clear();
            }

            var palette = new Palette(noColor);

            if (showHelp)
            {
                await console.Out.WriteLineAsync(GetUsage(palette)).ConfigureAwait(false);
                return;
            }

            if (listDependencies)
            {
                await console.Out.WriteLineAsync(targets.ToDependencyString(palette)).ConfigureAwait(false);
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

            await targets.RunAsync(names, options.SkipDependencies, options.DryRun, new Logger(console, options, palette));
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

        private static string ToDependencyString(this TargetCollection targets, Palette p)
        {
            var value = new StringBuilder();
            foreach (var target in targets.OrderBy(target => target.Name))
            {
                value.AppendLine(target.Name);
                foreach (var dependency in target.Dependencies)
                {
                    value.AppendLine($"  {p.White}{dependency}{p.Default}");
                }
            }

            return value.ToString();
        }

        public static string GetUsage(Palette p) =>
$@"{p.Cyan}Usage:{p.Default} {p.BrightYellow}<command-line>{p.Default} {p.White}[<options>]{p.Default} [<targets>]

{p.Cyan}command-line: {p.Default}The command line which invokes the build targets.{p.Default}
  {p.Cyan}Examples:{p.Default}
    {p.BrightYellow}build.cmd{p.Default}
    {p.BrightYellow}build.sh{p.Default}
    {p.BrightYellow}dotnet run --project targets --{p.Default}

{p.Cyan}options:{p.Default}
 {p.White}-D, --list-dependencies    {p.Default}Display the targets and dependencies, then exit
 {p.White}-T, --list-targets         {p.Default}Display the targets, then exit
 {p.White}-n, --dry-run              {p.Default}Do a dry run without executing actions
 {p.White}-N, --no-color             {p.Default}Disable colored output
 {p.White}-s, --skip-dependencies    {p.Default}Do not run targets' dependencies
 {p.White}-c, --clear                {p.Default}Clear the console before execution

{p.Cyan}targets: {p.Default}A list of targets to run. If not specified, the ""default"" target will be run.

{p.Cyan}Examples:{p.Default}
  {p.BrightYellow}build.cmd{p.Default}
  {p.BrightYellow}build.cmd{p.Default} {p.White}-D{p.Default}
  {p.BrightYellow}build.sh{p.Default} test pack
  {p.BrightYellow}dotnet run --project targets --{p.Default} {p.White}-n{p.Default} build";
    }
}
