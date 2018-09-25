namespace Bullseye.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Threading.Tasks;

    public class Logger
    {
        private static readonly IFormatProvider provider = CultureInfo.InvariantCulture;

        private enum MessageType
        {
            Verbose,
            Start,
            Success,
            Warning,
            Failure,
        }

        private readonly IConsole console;
        private readonly bool skipDependencies;
        private readonly bool dryRun;
        private readonly bool parallel;
        private readonly Palette p;
        private readonly Dictionary<MessageType, string> colors;
        private readonly bool verbose;

        public Logger(IConsole console, bool skipDependencies, bool dryRun, bool parallel, Palette palette, bool verbose)
        {
            this.console = console;
            this.skipDependencies = skipDependencies;
            this.dryRun = dryRun;
            this.parallel = parallel;
            this.p = palette;
            this.colors  = new Dictionary<MessageType, string>
            {
                [MessageType.Verbose] = palette.BrightBlack,
                [MessageType.Start] = palette.White,
                [MessageType.Success] = palette.Green,
                [MessageType.Warning] = palette.BrightYellow,
                [MessageType.Failure] = palette.BrightRed,
            };

            this.verbose = verbose;
        }

        public Task Running(List<string> targets) =>
            this.console.Out.WriteLineAsync(Message(MessageType.Start, $"Starting...", targets, null));

        public Task Failed(List<string> targets, double elapsedMilliseconds) =>
            this.console.Out.WriteLineAsync(Message(MessageType.Failure, $"Failed!", targets, elapsedMilliseconds));

        public Task Succeeded(List<string> targets, double elapsedMilliseconds) =>
            this.console.Out.WriteLineAsync(Message(MessageType.Success, $"Succeeded.", targets, elapsedMilliseconds));

        public Task Starting(string target) =>
            this.console.Out.WriteLineAsync(Message(MessageType.Start, "Starting...", target, null));

        public Task Failed(string target, Exception ex, double elapsedMilliseconds) =>
            this.console.Out.WriteLineAsync(Message(MessageType.Failure, $"Failed! {ex.Message}", target, elapsedMilliseconds));

        public Task Succeeded(string target, double elapsedMilliseconds) =>
            this.console.Out.WriteLineAsync(Message(MessageType.Success, "Succeeded.", target, elapsedMilliseconds));

        public Task Starting<TInput>(string target, TInput input) =>
            this.console.Out.WriteLineAsync(Message(MessageType.Start, "Starting...", target, input, null));

        public Task Failed<TInput>(string target, TInput input, Exception ex, double elapsedMilliseconds) =>
            this.console.Out.WriteLineAsync(Message(MessageType.Failure, $"Failed! {ex.Message}", target, input, elapsedMilliseconds));

        public Task Succeeded<TInput>(string target, TInput input, double elapsedMilliseconds) =>
            this.console.Out.WriteLineAsync(Message(MessageType.Success, "Succeeded.", target, input, elapsedMilliseconds));

        public Task NoInputs(string target) =>
            this.console.Out.WriteLineAsync(Message(MessageType.Warning, "No inputs!", target, null));

        public Task NoAction(string target) =>
            this.verbose
            ? this.console.Out.WriteLineAsync(Message(MessageType.Verbose, "No action.", target, null))
            : Task.CompletedTask;

        private string Message(MessageType messageType, string text, List<string> targets, double? elapsedMilliseconds) =>
            $"{GetPrefix()}{colors[messageType]}{text}{p.Cyan} ({targets.Spaced()}){p.Default}{GetSuffix(false, elapsedMilliseconds)}";

        private string Message(MessageType messageType, string text, string target, double? elapsedMilliseconds) =>
            $"{GetPrefix(target)}{colors[messageType]}{text}{p.Default}{GetSuffix(true, elapsedMilliseconds)}";

        private string Message<TInput>(MessageType messageType, string text, string target, TInput input, double? elapsedMilliseconds) =>
            $"{GetPrefix(target, input)}{colors[messageType]}{text}{p.Default}{GetSuffix(true, elapsedMilliseconds)}";

        private string GetPrefix() =>
            $"{p.Cyan}Bullseye{p.White}: {p.Default}";

        private string GetPrefix(string target) =>
            $"{p.Cyan}Bullseye{p.White}/{p.Cyan}{target}{p.White}: {p.Default}";

        private string GetPrefix<TInput>(string target, TInput input) =>
            $"{p.Cyan}Bullseye{p.White}/{p.Cyan}{target}{p.White}/{p.BrightCyan}{input}{p.White}: {p.Default}";

        private string GetSuffix(bool specific, double? elapsedMilliseconds) =>
            (!specific && this.dryRun ? $"{p.BrightMagenta} (dry run){p.Default}" : "") +
                (!specific && this.parallel ? $"{p.BrightMagenta} (parallel){p.Default}" : "") +
                (!specific && this.skipDependencies ? $"{p.BrightMagenta} (skip dependencies){p.Default}" : "") +
                (!this.dryRun && elapsedMilliseconds.HasValue ? $"{p.Magenta} ({ToStringFromMilliseconds(elapsedMilliseconds.Value)}){p.Default}" : "");

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
