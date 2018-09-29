namespace Bullseye.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Threading.Tasks;

    public class Logger
    {
        private static readonly IFormatProvider provider = CultureInfo.InvariantCulture;

        private readonly IConsole console;
        private readonly bool skipDependencies;
        private readonly bool dryRun;
        private readonly bool parallel;
        private readonly Palette p;

        public Logger(IConsole console, bool skipDependencies, bool dryRun, bool parallel, Palette palette)
        {
            this.console = console;
            this.skipDependencies = skipDependencies;
            this.dryRun = dryRun;
            this.parallel = parallel;
            this.p = palette;
        }

        public Task Running(List<string> targets) =>
            this.console.Out.WriteLineAsync(Message(p.Starting, $"Starting...", targets, null));

        public Task Failed(List<string> targets, double elapsedMilliseconds) =>
            this.console.Out.WriteLineAsync(Message(p.Failed, $"Failed!", targets, elapsedMilliseconds));

        public Task Succeeded(List<string> targets, double elapsedMilliseconds) =>
            this.console.Out.WriteLineAsync(Message(p.Succeeded, $"Succeeded.", targets, elapsedMilliseconds));

        public Task Starting(string target) =>
            this.console.Out.WriteLineAsync(Message(p.Starting, "Starting...", target, null));

        public Task Failed(string target, Exception ex, double elapsedMilliseconds) =>
            this.console.Out.WriteLineAsync(Message(p.Failed, $"Failed! {ex.Message}", target, elapsedMilliseconds));

        public Task Succeeded(string target, double? elapsedMilliseconds) =>
            this.console.Out.WriteLineAsync(Message(p.Succeeded, "Succeeded.", target, elapsedMilliseconds));

        public Task Starting<TInput>(string target, TInput input) =>
            this.console.Out.WriteLineAsync(Message(p.Starting, "Starting...", target, input, null));

        public Task Failed<TInput>(string target, TInput input, Exception ex, double elapsedMilliseconds) =>
            this.console.Out.WriteLineAsync(Message(p.Failed, $"Failed! {ex.Message}", target, input, elapsedMilliseconds));

        public Task Succeeded<TInput>(string target, TInput input, double elapsedMilliseconds) =>
            this.console.Out.WriteLineAsync(Message(p.Succeeded, "Succeeded.", target, input, elapsedMilliseconds));

        public Task NoInputs(string target) =>
            this.console.Out.WriteLineAsync(Message(p.Warning, "No inputs!", target, null));

        private string Message(string color, string text, List<string> targets, double? elapsedMilliseconds) =>
            $"{GetPrefix()}{color}{text}{p.Target} ({targets.Spaced()}){p.Default}{GetSuffix(false, elapsedMilliseconds)}";

        private string Message(string color, string text, string target, double? elapsedMilliseconds) =>
            $"{GetPrefix(target)}{color}{text}{p.Default}{GetSuffix(true, elapsedMilliseconds)}";

        private string Message<TInput>(string color, string text, string target, TInput input, double? elapsedMilliseconds) =>
            $"{GetPrefix(target, input)}{color}{text}{p.Default}{GetSuffix(true, elapsedMilliseconds)}";

        private string GetPrefix() =>
            $"{p.Label}Bullseye{p.Symbol}: {p.Default}";

        private string GetPrefix(string target) =>
            $"{p.Label}Bullseye{p.Symbol}/{p.Label}{target}{p.Symbol}: {p.Default}";

        private string GetPrefix<TInput>(string target, TInput input) =>
            $"{p.Label}Bullseye{p.Symbol}/{p.Label}{target}{p.Symbol}/{p.Input}{input}{p.Symbol}: {p.Default}";

        private string GetSuffix(bool specific, double? elapsedMilliseconds) =>
            (!specific && this.dryRun ? $"{p.Option} (dry run){p.Default}" : "") +
                (!specific && this.parallel ? $"{p.Option} (parallel){p.Default}" : "") +
                (!specific && this.skipDependencies ? $"{p.Option} (skip dependencies){p.Default}" : "") +
                (!this.dryRun && elapsedMilliseconds.HasValue ? $"{p.Timing} ({ToStringFromMilliseconds(elapsedMilliseconds.Value)}){p.Default}" : "");

        private static string ToStringFromMilliseconds(double milliseconds)
        {
            // less than one millisecond
            if (milliseconds < 1D)
            {
                return "<1 ms";
            }

            // milliseconds
            if (milliseconds < 1_000D)
            {
                return milliseconds.ToString("G3", provider) + " ms";
            }

            // seconds
            if (milliseconds < 60_000D)
            {
                return (milliseconds / 1_000D).ToString("G3", provider) + " s";
            }

            // minutes and seconds
            if (milliseconds < 3_600_000D)
            {
                var minutes = Math.Floor(milliseconds / 60_000D).ToString("F0", provider);
                var seconds = ((milliseconds % 60_000D) / 1_000D).ToString("F0", provider);
                return seconds == "0"
                    ? $"{minutes} min"
                    : $"{minutes} min {seconds} s";
            }

            // minutes
            return (milliseconds / 60_000d).ToString("N0", provider) + " min";
        }
    }
}
