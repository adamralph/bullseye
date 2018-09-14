namespace Bullseye.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading.Tasks;

    public static class TargetCollectionExtensions
    {
        public static Task RunAsync(this TargetCollection targets, IEnumerable<string> args, IConsole console) =>
            RunAsync(targets ?? new TargetCollection(), args.Sanitize().ToList(), console ?? new SystemConsole());

        private static async Task RunAsync(this TargetCollection targets, List<string> args, IConsole console)
        {
            var clear = false;
            var dryRun = false;
            var listDependencies = false;
            var listInputs = false;
            var listTargets = false;
            var noColor = false;
            var skipDependencies = false;
            var verbose = false;
            var showHelp = false;

            var helpOptions = new[] { "--help", "-h", "-?" };
            var optionsArgs = args.Where(arg => arg.StartsWith("-", StringComparison.Ordinal)).ToList();
            var unknownOptions = new List<string>();

            foreach (var option in optionsArgs)
            {
                switch (option)
                {
                    case "-c":
                    case "--clear":
                        clear = true;
                        break;
                    case "-n":
                    case "--dry-run":
                        dryRun = true;
                        break;
                    case "-D":
                    case "--list-dependencies":
                        listDependencies = true;
                        break;
                    case "-I":
                    case "--list-inputs":
                        listInputs = true;
                        break;
                    case "-T":
                    case "--list-targets":
                        listTargets = true;
                        break;
                    case "-N":
                    case "--no-color":
                        noColor = true;
                        break;
                    case "-s":
                    case "--skip-dependencies":
                        skipDependencies = true;
                        break;
                    case "-v":
                    case "--verbose":
                        verbose = true;
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

            if (unknownOptions.Count > 0)
            {
                throw new Exception($"Unknown options {unknownOptions.Spaced()}. \"--help\" for usage.");
            }

            if (clear)
            {
                console.Clear();
            }

            if (!noColor && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                await WindowsConsole.TryEnableVirtualTerminalProcessing(console.Out, verbose).ConfigureAwait(false);
            }

            var palette = new Palette(noColor);

            if (showHelp)
            {
                await console.Out.WriteLineAsync(GetUsage(palette)).ConfigureAwait(false);
                return;
            }

            if (listDependencies || listInputs || listTargets)
            {
                await console.Out.WriteLineAsync(targets.ToString(listDependencies, listInputs, palette)).ConfigureAwait(false);
                return;
            }

            var names = args.Where(arg => !arg.StartsWith("-")).ToList();
            if (names.Count == 0)
            {
                names.Add("default");
            }

            await targets.RunAsync(names, skipDependencies, dryRun, new Logger(console, skipDependencies, dryRun, palette)).ConfigureAwait(false);
        }

        private static string ToString(this TargetCollection targets, bool listDependencies, bool listInputs, Palette p)
        {
            var value = new StringBuilder();
            foreach (var target in targets.OrderBy(target => target.Name))
            {
                value.AppendLine($"{p.BrightWhite}{target.Name}{p.Default}");

                if (listDependencies)
                {
                    var writeHeader = listInputs;
                    var indent = writeHeader ? "    " : "  ";
                    foreach (var dependency in target.Dependencies)
                    {
                        if (writeHeader)
                        {
                            value.AppendLine($"  {p.Cyan}Dependencies:{p.Default}");
                            writeHeader = false;
                        }

                        value.AppendLine($"{indent}{p.White}{dependency}{p.Default}");
                    }
                }

                if (listInputs)
                {
                    var writeHeader = listDependencies;
                    var indent = writeHeader ? "    " : "  ";
                    if (target is IHaveInputs hasInputs)
                    {
                        foreach (var input in hasInputs.Inputs)
                        {
                            if (writeHeader)
                            {
                                value.AppendLine($"  {p.Cyan}Inputs:{p.Default}");
                                writeHeader = false;
                            }

                            value.AppendLine($"{indent}{p.BrightCyan}{input}{p.Default}");
                        }
                    }
                }
            }

            return value.ToString();
        }

        public static string GetUsage(Palette p) =>
$@"{p.Cyan}Usage: {p.BrightYellow}<command-line> {p.White}[<options>] {p.BrightWhite}[<targets>]

{p.Cyan}command-line: {p.Default}The command line which invokes the build targets.
  {p.Cyan}Examples:
    {p.BrightYellow}build.cmd
    {p.BrightYellow}build.sh
    {p.BrightYellow}dotnet run --project targets --

{p.Cyan}options:
 {p.White}-c, --clear                {p.Default}Clear the console before execution
 {p.White}-n, --dry-run              {p.Default}Do a dry run without executing actions
 {p.White}-D, --list-dependencies    {p.Default}List the targets and dependencies, then exit
 {p.White}-I, --list-inputs          {p.Default}List the targets and inputs, then exit
 {p.White}-T, --list-targets         {p.Default}List the targets, then exit
 {p.White}-N, --no-color             {p.Default}Disable colored output
 {p.White}-s, --skip-dependencies    {p.Default}Do not run targets' dependencies
 {p.White}-v, --verbose              {p.Default}Enable verbose output
 {p.White}-h, --help                 {p.Default}Show this help (case insensitive) (or -?)

{p.Cyan}targets: {p.Default}A list of targets to run. If not specified, the {p.BrightWhite}""default""{p.Default} target will be run.

{p.Cyan}Examples:
  {p.BrightYellow}build.cmd
  {p.BrightYellow}build.cmd {p.White}-D
  {p.BrightYellow}build.sh {p.BrightWhite}test pack
  {p.BrightYellow}dotnet run --project targets -- {p.White}-n {p.BrightWhite}build{p.Default}";
    }
}
