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
            var parallel = false;
            var skipDependencies = false;
            var verbose = false;
            var host = Host.Unknown;
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
                    case "-p":
                    case "--parallel":
                        parallel = true;
                        break;
                    case "-s":
                    case "--skip-dependencies":
                        skipDependencies = true;
                        break;
                    case "-v":
                    case "--verbose":
                        verbose = true;
                        break;
                    case "--appveyor":
                        host = Host.Appveyor;
                        break;
                    case "--travis":
                        host = Host.Travis;
                        break;
                    case "--teamcity":
                        host = Host.TeamCity;
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
                throw new BullseyeException($"Unknown options {unknownOptions.Spaced()}. \"--help\" for usage.");
            }

            if (clear)
            {
                console.Clear();
            }

            var operatingSystem = OperatingSystem.Unknown;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                operatingSystem = OperatingSystem.Windows;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                operatingSystem = OperatingSystem.Linux;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                operatingSystem = OperatingSystem.MacOS;
            }

            if (!noColor && operatingSystem == OperatingSystem.Windows)
            {
                await WindowsConsole.TryEnableVirtualTerminalProcessing(console.Out, verbose).ConfigureAwait(false);
            }

            var isHostForced = true;
            if (host == Host.Unknown)
            {
                isHostForced = false;

                if (Environment.GetEnvironmentVariable("APPVEYOR")?.ToUpperInvariant() == "TRUE")
                {
                    host = Host.Appveyor;
                }
                else if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("TRAVIS_OS_NAME")))
                {
                    host = Host.Travis;
                }
                else if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("TEAMCITY_PROJECT_NAME")))
                {
                    host = Host.TeamCity;
                }
            }

            var palette = new Palette(noColor, host, operatingSystem);
            var log = new Logger(console, skipDependencies, dryRun, parallel, palette, verbose);

            await log.Version().ConfigureAwait(false);
            await log.Verbose($"Host: {host}{(host != Host.Unknown ? $" ({(isHostForced ? "forced" : "detected")})" : "")}").ConfigureAwait(false);
            await log.Verbose($"OS: {operatingSystem}").ConfigureAwait(false);
            await log.Verbose($"Args: {string.Join(" ", args)}").ConfigureAwait(false);

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

            await targets.RunAsync(names, skipDependencies, dryRun, parallel, log).ConfigureAwait(false);
        }

        private static string ToString(this TargetCollection targets, bool listDependencies, bool listInputs, Palette p)
        {
            var value = new StringBuilder();
            foreach (var target in targets.OrderBy(target => target.Name))
            {
                value.AppendLine($"{p.Target}{target.Name}{p.Default}");

                if (listDependencies)
                {
                    var writeHeader = listInputs;
                    var indent = writeHeader ? "    " : "  ";
                    foreach (var dependency in target.Dependencies)
                    {
                        if (writeHeader)
                        {
                            value.AppendLine($"  {p.Label}Dependencies:{p.Default}");
                            writeHeader = false;
                        }

                        value.AppendLine($"{indent}{p.Dependency}{dependency}{(targets.Contains(dependency) ? "" : $" {p.Failed}(missing)")}{p.Default}");
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
                                value.AppendLine($"  {p.Label}Inputs:{p.Default}");
                                writeHeader = false;
                            }

                            value.AppendLine($"{indent}{p.Input}{input}{p.Default}");
                        }
                    }
                }
            }

            return value.ToString();
        }

        public static string GetUsage(Palette p) =>
$@"{p.Label}Usage: {p.CommandLine}<command-line> {p.Option}[<options>] {p.Target}[<targets>]

{p.Label}command-line: {p.Text}The command line which invokes the build targets.
  {p.Label}Examples:
    {p.CommandLine}build.cmd
    {p.CommandLine}build.sh
    {p.CommandLine}dotnet run --project targets --

{p.Label}options:
 {p.Option}-c, --clear                {p.Text}Clear the console before execution
 {p.Option}-n, --dry-run              {p.Text}Do a dry run without executing actions
 {p.Option}-D, --list-dependencies    {p.Text}List the targets and dependencies, then exit
 {p.Option}-I, --list-inputs          {p.Text}List the targets and inputs, then exit
 {p.Option}-T, --list-targets         {p.Text}List the targets, then exit
 {p.Option}-N, --no-color             {p.Text}Disable colored output
 {p.Option}-p, --parallel             {p.Text}Run targets in parallel
 {p.Option}-s, --skip-dependencies    {p.Text}Do not run targets' dependencies
 {p.Option}-v, --verbose              {p.Text}Enable verbose output
 {p.Option}    --appveyor             {p.Text}Force Appveyor mode (normally auto-detected)
 {p.Option}    --teamcity             {p.Text}Force TeamCity mode (normally auto-detected)
 {p.Option}    --travis               {p.Text}Force Travis CI mode (normally auto-detected)
 {p.Option}-h, --help, -?             {p.Text}Show this help, then exit (case insensitive)

{p.Label}targets: {p.Text}A list of targets to run. If not specified, the {p.Target}""default""{p.Text} target will be run.

{p.Label}Examples:
  {p.CommandLine}build.cmd
  {p.CommandLine}build.cmd {p.Option}-D
  {p.CommandLine}build.sh {p.Target}test pack
  {p.CommandLine}dotnet run --project targets -- {p.Option}-n {p.Target}build{p.Default}";
    }
}
