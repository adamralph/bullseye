namespace Bullseye
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Bullseye.Internal;

    public class Options
    {
        private static readonly string[] helpOptions = new[] { "--help", "-h", "-?" };

        public ICollection<string> Targets { get; } = new List<string>();

        public bool Clear { get; set; }

        public bool DryRun { get; set; }

        public bool ListDependencies { get; set; }

        public bool ListInputs { get; set; }

        public bool ListTargets { get; set; }

        public bool NoColor { get; set; }

        public bool SkipDependencies { get; set; }

        public bool Verbose { get; set; }

        public bool ShowHelp { get; set; }

        public static Options Parse(IEnumerable<string> args) =>
            Parse(args.Sanitize().ToList());

        private static Options Parse(List<string> args)
        {
            var optionsArgs = args.Where(arg => arg.StartsWith("-", StringComparison.Ordinal)).ToList();
            var unknownOptions = new List<string>();

            var options = new Options();

            foreach (var option in optionsArgs)
            {
                switch (option)
                {
                    case "-c":
                    case "--clear":
                        options.Clear = true;
                        break;
                    case "-n":
                    case "--dry-run":
                        options.DryRun = true;
                        break;
                    case "-D":
                    case "--list-dependencies":
                        options.ListDependencies = true;
                        break;
                    case "-I":
                    case "--list-inputs":
                        options.ListInputs = true;
                        break;
                    case "-T":
                    case "--list-targets":
                        options.ListTargets = true;
                        break;
                    case "-N":
                    case "--no-color":
                        options.NoColor = true;
                        break;
                    case "-s":
                    case "--skip-dependencies":
                        options.SkipDependencies = true;
                        break;
                    case "-v":
                    case "--verbose":
                        options.Verbose = true;
                        break;
                    default:
                        if (helpOptions.Contains(option, StringComparer.OrdinalIgnoreCase))
                        {
                            options.ShowHelp = true;
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
                throw new Exception($"Unknown options {unknownOptions.Spaced()}. \"{helpOptions[0]}\" for usage.");
            }

            foreach (var target in args.Where(arg => !arg.StartsWith("-")))
            {
                options.Targets.Add(target);
            }

            return options;
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
