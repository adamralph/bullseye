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

        internal Task Running(List<string> targets) =>
            this.console.Out.WriteLineAsync(
                Message(MessageType.Start, $"Running {targets.Quote()}...", options.DryRun, options.SkipDependencies, options.NoColor, null));

        internal Task Failed(List<string> targets, Exception ex, double elapsedMilliseconds) =>
            this.console.Out.WriteLineAsync(
                Message(MessageType.Failure, $"Failed to run {targets.Quote()}!", options.DryRun, options.SkipDependencies, options.NoColor, elapsedMilliseconds));

        internal Task Succeeded(List<string> targets, double elapsedMilliseconds) =>
            this.console.Out.WriteLineAsync(Message(MessageType.Success, $"{targets.Quote()} succeeded.", options.DryRun, options.SkipDependencies, options.NoColor, elapsedMilliseconds));

        public Task Starting(string target) =>
            this.console.Out.WriteLineAsync(Message(MessageType.Start, "Starting...", target, options.DryRun, options.SkipDependencies, options.NoColor, null));

        public Task Failed(string target, Exception ex, double elapsedMilliseconds) =>
            this.console.Out.WriteLineAsync(Message(MessageType.Failure, $"Failed! {ex.Message}", target, options.DryRun, options.SkipDependencies, options.NoColor, elapsedMilliseconds));

        public Task Succeeded(string target, double elapsedMilliseconds) =>
            this.console.Out.WriteLineAsync(Message(MessageType.Success, "Succeeded.", target, options.DryRun, options.SkipDependencies, options.NoColor, elapsedMilliseconds));

        private static string Message(MessageType messageType, string text, bool dryRun, bool skipDependencies, bool noColor, double? elapsedMilliseconds) =>
            $"{GetPrefix(noColor)}{colors[messageType](noColor)}{text}{Default(noColor)}{GetSuffix(messageType, false, dryRun, skipDependencies, noColor, elapsedMilliseconds)}";

        private static string Message(MessageType messageType, string text, string name, bool dryRun, bool skipDependencies, bool noColor, double? elapsedMilliseconds) =>
            $"{GetPrefix(name, noColor)}{colors[messageType](noColor)}{text}{Default(noColor)}{GetSuffix(messageType, true, dryRun, skipDependencies, noColor, elapsedMilliseconds)}";

        private static string GetPrefix(bool noColor) =>
            $"{Cyan(noColor)}Bullseye{Default(noColor)}{White(noColor)}: {Default(noColor)}";

        private static string GetPrefix(string name, bool noColor) =>
            $"{Cyan(noColor)}Bullseye{Default(noColor)}{White(noColor)}/{Default(noColor)}{Cyan(noColor)}{name.Replace(": ", ":: ").Replace("/", "//")}{Default(noColor)}{White(noColor)}: {Default(noColor)}";

        private static string GetSuffix(MessageType messageType, bool specific, bool dryRun, bool skipDependencies, bool noColor, double? elapsedMilliseconds) =>
            (!specific && dryRun ? $"{BrightMagenta(noColor)} (dry run){Default(noColor)}" : "") +
                (!specific && skipDependencies ? $"{BrightMagenta(noColor)} (skip dependencies){Default(noColor)}" : "") +
                (!dryRun && elapsedMilliseconds.HasValue ? $"{Magenta(noColor)} ({ToStringFromMilliseconds(elapsedMilliseconds.Value)}){Default(noColor)}" : "");

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
