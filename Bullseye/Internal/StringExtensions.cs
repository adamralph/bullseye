namespace Bullseye.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using static Color;

    public static class StringExtensions
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

        public static string Quote(this IEnumerable<string> strings) =>
            string.Join(", ", strings.Select(@string => Quote(@string)));

        public static string Quote(this string @string) => $"\"{(@string.Replace("\"", "\"\""))}\"";

        public static string ToTargetsRunning(this IList<string> names, Options options) =>
            Message(MessageType.Start, $"Running {Quote(names)}...", options.DryRun, options.SkipDependencies, options.NoColor, null);

        public static string ToTargetsFailed(this IList<string> names, Options options, double elapsedMilliseconds) =>
            Message(MessageType.Failure, $"Failed to run {Quote(names)}!", options.DryRun, options.SkipDependencies, options.NoColor, elapsedMilliseconds);

        public static string ToTargetsSucceeded(this IList<string> names, Options options, double elapsedMilliseconds) =>
            Message(MessageType.Success, $"{Quote(names)} succeeded.", options.DryRun, options.SkipDependencies, options.NoColor, elapsedMilliseconds);

        public static string ToTargetStarting(this string name, Options options) =>
            Message(MessageType.Start, "Starting...", name, options.DryRun, options.SkipDependencies, options.NoColor, null);

        public static string ToTargetFailed(this string name, Exception ex, Options options, double elapsedMilliseconds) =>
            Message(MessageType.Failure, $"Failed! {ex.Message}", name, options.DryRun, options.SkipDependencies, options.NoColor, elapsedMilliseconds);

        public static string ToTargetSucceeded(this string name, Options options, double elapsedMilliseconds) =>
            Message(MessageType.Success, "Succeeded.", name, options.DryRun, options.SkipDependencies, options.NoColor, elapsedMilliseconds);

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
            // nanoseconds
            if (milliseconds < 0.001D)
            {
                return (milliseconds * 1_000_000D).ToString("G3", provider) + " ns";
            }

            // microseconds
            if (milliseconds < 1D)
            {
                return (milliseconds * 1_000D).ToString("G3", provider) + " \u00B5s"; // µs
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
