namespace Bullseye.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Threading.Tasks;
    using static Color;

    public class Logger
    {
        private static readonly IFormatProvider provider = CultureInfo.InvariantCulture;

        private static readonly Dictionary<MessageType, Func<bool, string>> colors = new Dictionary<MessageType, Func<bool, string>>
        {
            [MessageType.Start] = color => White(color),
            [MessageType.Success] = color => Green(color),
            [MessageType.Failure] = color => BrightRed(color),
        };

        private enum MessageType
        {
            Start,
            Success,
            Failure,
        }

        private readonly IConsole console;
        private readonly Options options;

        public Logger(IConsole console, Options options)
        {
            this.console = console;
            this.options = options;
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

        private string Message(MessageType messageType, string text, double? elapsedMilliseconds) =>
            $"{GetPrefix()}{colors[messageType](this.options.NoColor)}{text}{Default(this.options.NoColor)}{GetSuffix(messageType, false, elapsedMilliseconds)}";

        private string Message(MessageType messageType, string text, string target, double? elapsedMilliseconds) =>
            $"{GetPrefix(target)}{colors[messageType](this.options.NoColor)}{text}{Default(this.options.NoColor)}{GetSuffix(messageType, true, elapsedMilliseconds)}";

        private string GetPrefix() =>
            $"{Cyan(this.options.NoColor)}Bullseye{Default(this.options.NoColor)}{White(this.options.NoColor)}: {Default(this.options.NoColor)}";

        private string GetPrefix(string target) =>
            $"{Cyan(this.options.NoColor)}Bullseye{Default(this.options.NoColor)}{White(this.options.NoColor)}/{Default(this.options.NoColor)}{Cyan(this.options.NoColor)}{target.Replace(": ", ":: ").Replace("/", "//")}{Default(this.options.NoColor)}{White(this.options.NoColor)}: {Default(this.options.NoColor)}";

        private string GetSuffix(MessageType messageType, bool specific, double? elapsedMilliseconds) =>
            (!specific && this.options.DryRun ? $"{BrightMagenta(this.options.NoColor)} (dry run){Default(this.options.NoColor)}" : "") +
                (!specific && this.options.SkipDependencies ? $"{BrightMagenta(this.options.NoColor)} (skip dependencies){Default(this.options.NoColor)}" : "") +
                (!this.options.DryRun && elapsedMilliseconds.HasValue ? $"{Magenta(this.options.NoColor)} ({ToStringFromMilliseconds(elapsedMilliseconds.Value)}){Default(this.options.NoColor)}" : "");

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
