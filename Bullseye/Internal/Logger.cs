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
            Start,
            Success,
            Failure,
        }

        private readonly IConsole console;
        private readonly Options options;
        private readonly Palette p;
        private readonly Dictionary<MessageType, string> colors;

        public Logger(IConsole console, Options options, Palette palette)
        {
            this.console = console;
            this.options = options;
            this.p = palette;
            this.colors  = new Dictionary<MessageType, string>
            {
                [MessageType.Start] = palette.White,
                [MessageType.Success] = palette.Green,
                [MessageType.Failure] = palette.BrightRed,
            };
        }

        public Task Running(List<string> targets) =>
            this.console.Out.WriteLineAsync(Message(MessageType.Start, $"Running {targets.Quote()}...", null));

        public Task Failed(List<string> targets, Exception ex, double elapsedMilliseconds) =>
            this.console.Out.WriteLineAsync(Message(MessageType.Failure, $"Failed to run {targets.Quote()}!", elapsedMilliseconds));

        public Task Succeeded(List<string> targets, double elapsedMilliseconds) =>
            this.console.Out.WriteLineAsync(Message(MessageType.Success, $"{targets.Quote()} succeeded.", elapsedMilliseconds));

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

        private string Message(MessageType messageType, string text, double? elapsedMilliseconds) =>
            $"{GetPrefix()}{colors[messageType]}{text}{p.Default}{GetSuffix(messageType, false, elapsedMilliseconds)}";

        private string Message(MessageType messageType, string text, string target, double? elapsedMilliseconds) =>
            $"{GetPrefix(target)}{colors[messageType]}{text}{p.Default}{GetSuffix(messageType, true, elapsedMilliseconds)}";

        private string Message<TInput>(MessageType messageType, string text, string target, TInput input, double? elapsedMilliseconds) =>
            $"{GetPrefix(target, input)}{colors[messageType]}{text}{p.Default}{GetSuffix(messageType, true, elapsedMilliseconds)}";

        private string GetPrefix() =>
            $"{p.Cyan}Bullseye{p.Default}{p.White}: {p.Default}";

        private string GetPrefix(string target) =>
            $"{p.Cyan}Bullseye{p.Default}{p.White}/{p.Default}{p.Cyan}{target.Replace(": ", ":: ").Replace("/", "//")}{p.Default}{p.White}: {p.Default}";

        private string GetPrefix<TInput>(string target, TInput input) =>
            $"{p.Cyan}Bullseye{p.Default}{p.White}/{p.Default}{p.Cyan}{target.Replace(": ", ":: ").Replace("/", "//")}{p.Default}{p.White}/{p.Default}{p.BrightCyan}{input?.ToString().Replace(": ", ":: ").Replace("/", "//")}{p.Default}{p.White}: {p.Default}";

        private string GetSuffix(MessageType messageType, bool specific, double? elapsedMilliseconds) =>
            (!specific && this.options.DryRun ? $"{p.BrightMagenta} (dry run){p.Default}" : "") +
                (!specific && this.options.SkipDependencies ? $"{p.BrightMagenta} (skip dependencies){p.Default}" : "") +
                (!this.options.DryRun && elapsedMilliseconds.HasValue ? $"{p.Magenta} ({ToStringFromMilliseconds(elapsedMilliseconds.Value)}){p.Default}" : "");

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
