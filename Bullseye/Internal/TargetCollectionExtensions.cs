namespace Bullseye.Internal
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading.Tasks;

    public static class TargetCollectionExtensions
    {
        public static Task RunAsync(this TargetCollection targets, IEnumerable<string> args, IConsole console) =>
            RunAsync(targets, Options.Parse(args), console);

        public static async Task RunAsync(this TargetCollection targets, Options options, IConsole console)
        {
            targets = targets ?? new TargetCollection();
            options = options ?? new Options();
            console = console ?? new SystemConsole();

            if (options.Clear)
            {
                console.Clear();
            }

            if (!options.NoColor && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                await WindowsConsole.TryEnableVirtualTerminalProcessing(console.Out, options.Verbose).ConfigureAwait(false);
            }

            var palette = new Palette(options.NoColor);

            if (options.ShowHelp)
            {
                await console.Out.WriteLineAsync(Options.GetUsage(palette)).ConfigureAwait(false);
                return;
            }

            if (options.ListDependencies || options.ListInputs || options.ListTargets)
            {
                await console.Out.WriteLineAsync(targets.ToString(options.ListDependencies, options.ListInputs, palette)).ConfigureAwait(false);
                return;
            }

            var names = options.Targets.ToList();
            if (names.Count == 0)
            {
                names.Add("default");
            }

            await targets.RunAsync(names, options.SkipDependencies, options.DryRun, new Logger(console, options.SkipDependencies, options.DryRun, palette)).ConfigureAwait(false);
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
    }
}
