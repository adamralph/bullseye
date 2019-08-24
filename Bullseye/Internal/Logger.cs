#pragma warning disable IDE0009 // Member access should be qualified.
namespace Bullseye.Internal
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using static System.Math;

    public class Logger
    {
        private static readonly IFormatProvider provider = CultureInfo.InvariantCulture;

        private readonly ConcurrentDictionary<string, TargetResult> results = new ConcurrentDictionary<string, TargetResult>();
        private readonly TextWriter writer;
        private readonly string prefix;
        private readonly bool skipDependencies;
        private readonly bool dryRun;
        private readonly bool parallel;
        private readonly Palette p;
        private readonly bool verbose;

        public Logger(TextWriter writer, string prefix, bool skipDependencies, bool dryRun, bool parallel, Palette palette, bool verbose)
        {
            this.writer = writer;
            this.prefix = prefix;
            this.skipDependencies = skipDependencies;
            this.dryRun = dryRun;
            this.parallel = parallel;
            this.p = palette;
            this.verbose = verbose;
        }

        public async Task Version()
        {
            if (this.verbose)
            {
                var version = typeof(TargetCollectionExtensions).Assembly.GetCustomAttributes(false)
                    .OfType<AssemblyInformationalVersionAttribute>()
                    .FirstOrDefault()
                    ?.InformationalVersion ?? "Unknown";

                await this.writer.WriteLineAsync(Message(p.Verbose, $"Version: {version}")).Tax();
            }
        }

        public Task Error(string message) => this.writer.WriteLineAsync(Message(p.Failed, message));

        public async Task Verbose(string message)
        {
            if (this.verbose)
            {
                await this.writer.WriteLineAsync(Message(p.Verbose, message)).Tax();
            }
        }

        public async Task Verbose(Stack<string> targets, string message)
        {
            if (this.verbose)
            {
                await this.writer.WriteLineAsync(Message(targets, p.Verbose, message)).Tax();
            }
        }

        public Task Running(List<string> targets) =>
            this.writer.WriteLineAsync(Message(p.Starting, $"Starting...", targets, null));

        public async Task Failed(List<string> targets, double elapsedMilliseconds)
        {
            await this.Results().Tax();
            await this.writer.WriteLineAsync(Message(p.Failed, $"Failed!", targets, elapsedMilliseconds)).Tax();
        }

        public async Task Succeeded(List<string> targets, double elapsedMilliseconds)
        {
            await this.Results().Tax();
            await this.writer.WriteLineAsync(Message(p.Succeeded, $"Succeeded.", targets, elapsedMilliseconds)).Tax();
        }

        public Task Starting(string target) =>
            this.writer.WriteLineAsync(Message(p.Starting, "Starting...", target, null));

        public Task Error(string target, Exception ex) =>
            this.writer.WriteLineAsync(Message(p.Failed, ex.ToString(), target));

        public Task Failed(string target, Exception ex, double elapsedMilliseconds)
        {
            var result = this.results.GetOrAdd(target, key => new TargetResult());
            result.Outcome = TargetOutcome.Failed;
            result.DurationMilliseconds = elapsedMilliseconds;

            return this.writer.WriteLineAsync(Message(p.Failed, $"Failed! {ex.Message}", target, elapsedMilliseconds));
        }

        public Task Failed(string target, double elapsedMilliseconds)
        {
            var result = this.results.GetOrAdd(target, key => new TargetResult());
            result.Outcome = TargetOutcome.Failed;
            result.DurationMilliseconds = elapsedMilliseconds;

            return this.writer.WriteLineAsync(Message(p.Failed, $"Failed!", target, elapsedMilliseconds));
        }

        public Task Succeeded(string target, double? elapsedMilliseconds)
        {
            var result = this.results.GetOrAdd(target, key => new TargetResult());
            result.Outcome = TargetOutcome.Succeeded;
            result.DurationMilliseconds = elapsedMilliseconds;

            return this.writer.WriteLineAsync(Message(p.Succeeded, "Succeeded.", target, elapsedMilliseconds));
        }

        public Task Starting<TInput>(string target, TInput input) =>
            this.writer.WriteLineAsync(MessageWithInput(p.Starting, "Starting...", target, input, null));

        public Task Error<TInput>(string target, TInput input, Exception ex) =>
            this.writer.WriteLineAsync(MessageWithInput(p.Failed, ex.ToString(), target, input));

        public Task Failed<TInput>(string target, TInput input, Exception ex, double elapsedMilliseconds)
        {
            this.results.GetOrAdd(target, key => new TargetResult()).InputResults
                .Enqueue(new TargetInputResult { Input = input, Outcome = TargetInputOutcome.Failed, DurationMilliseconds = elapsedMilliseconds });

            return this.writer.WriteLineAsync(MessageWithInput(p.Failed, $"Failed! {ex.Message}", target, input, elapsedMilliseconds));
        }

        public Task Succeeded<TInput>(string target, TInput input, double elapsedMilliseconds)
        {
            this.results.GetOrAdd(target, key => new TargetResult()).InputResults
                .Enqueue(new TargetInputResult { Input = input, Outcome = TargetInputOutcome.Succeeded, DurationMilliseconds = elapsedMilliseconds });

            return this.writer.WriteLineAsync(MessageWithInput(p.Succeeded, "Succeeded.", target, input, elapsedMilliseconds));
        }

        public Task NoInputs(string target)
        {
            this.results.GetOrAdd(target, key => new TargetResult()).Outcome = TargetOutcome.NoInputs;

            return this.writer.WriteLineAsync(Message(p.Warning, "No inputs!", target, null));
        }

        private async Task Results()
        {
            // whitespace (e.g. can change to '·' for debugging)
            var ws = ' ';

            var totalDuration = results.Sum(i => i.Value.DurationMilliseconds ?? 0 + i.Value.InputResults.Sum(i2 => i2.DurationMilliseconds));

            var rows = new List<Tuple<string, string, string, string>> { Tuple.Create($"{p.Label}Duration", "", $"{p.Label}Outcome", $"{p.Label}Target") };

            foreach (var item in results.OrderBy(i => i.Value.DurationMilliseconds))
            {
                var duration = $"{p.Timing}{ToStringFromMilliseconds(item.Value.DurationMilliseconds, true)}";

                var percentage = item.Value.DurationMilliseconds.HasValue && totalDuration > 0
                    ? $"{p.Timing}{100 * item.Value.DurationMilliseconds / totalDuration:N1}%"
                    : "";

                var outcome = item.Value.Outcome == TargetOutcome.Failed
                    ? $"{p.Failed}Failed!"
                    : item.Value.Outcome == TargetOutcome.NoInputs
                        ? $"{p.Warning}No inputs!"
                        : $"{p.Succeeded}Succeeded";

                var target = $"{p.Target}{item.Key}";

                rows.Add(Tuple.Create(duration, percentage, outcome, target));

                var index = 0;

                foreach (var result in item.Value.InputResults.OrderBy(r => r.DurationMilliseconds))
                {
                    var inputDuration = $"{p.Tree}{(index < item.Value.InputResults.Count - 1 ? p.TreeFork : p.TreeCorner)}{p.Timing}{ToStringFromMilliseconds(result.DurationMilliseconds, true)}";

                    var inputPercentage = totalDuration > 0
                        ? $"{p.Tree}{(index < item.Value.InputResults.Count - 1 ? p.TreeFork : p.TreeCorner)}{p.Timing}{100 * result.DurationMilliseconds / totalDuration:N1}%"
                        : "";

                    var inputOutcome = result.Outcome == TargetInputOutcome.Failed ? $"{p.Failed}Failed!" : $"{p.Succeeded}Succeeded";

                    var input = $"{ws}{ws}{p.Input}{result.Input.ToString()}";

                    rows.Add(Tuple.Create(inputDuration, inputPercentage, inputOutcome, input));

                    ++index;
                }
            }

            // time column width
            var timW = rows.Count > 1 ? rows.Skip(1).Max(row => Palette.StripColours(row.Item1).Length) : 0;

            // percentage column width
            var perW = rows.Max(row => Palette.StripColours(row.Item2).Length);

            // duration column width (time and percentage)
            var durW = Max(Palette.StripColours(rows[0].Item1).Length, timW + 2 + perW);

            // expand percentage column width to ensure time and percentage are as wide as duration
            perW = Max(durW - timW - 2, perW);

            // outcome column width
            var outW = rows.Max(row => Palette.StripColours(row.Item3).Length);

            // target name column width
            var tarW = rows.Max(row => Palette.StripColours(row.Item4).Length);

            // summary start separator
            await this.writer.WriteLineAsync($"{GetPrefix()}{p.Symbol}{"".Prp(durW + 2 + outW + 2 + tarW, p.Dash)}").Tax();

            // header
            await this.writer.WriteLineAsync($"{GetPrefix()}{rows[0].Item1.Prp(durW, ws)}{ws}{ws}{rows[0].Item3.Prp(outW, ws)}{ws}{ws}{rows[0].Item4.Prp(tarW, ws)}").Tax();

            // header separator
            await this.writer.WriteLineAsync($"{GetPrefix()}{p.Symbol}{"".Prp(durW, p.Dash)}{ws}{ws}{"".Prp(outW, p.Dash)}{ws}{ws}{"".Prp(tarW, p.Dash)}").Tax();

            // targets
            foreach (var row in rows.Skip(1))
            {
                await this.writer.WriteLineAsync($"{GetPrefix()}{row.Item1.Prp(timW, ws)}{ws}{ws}{row.Item2.Prp(perW, ws)}{ws}{ws}{row.Item3.Prp(outW, ws)}{ws}{ws}{row.Item4.Prp(tarW, ws)}").Tax();
            }

            // summary end separator
            await this.writer.WriteLineAsync($"{GetPrefix()}{p.Symbol}{"".Prp(durW + 2 + outW + 2 + tarW, p.Dash)}{p.Default}").Tax();
        }

        private string Message(string color, string text) => $"{GetPrefix()}{color}{text}{p.Default}";

        private string Message(Stack<string> targets, string color, string text) => $"{GetPrefix(targets)}{color}{text}{p.Default}";

        private string Message(string color, string text, List<string> targets, double? elapsedMilliseconds) =>
            $"{GetPrefix()}{color}{text}{p.Target} ({targets.Spaced()}){p.Default}{GetSuffix(false, elapsedMilliseconds)}{p.Default}";

        private string Message(string color, string text, string target) =>
            $"{GetPrefix(target)}{color}{text}{p.Default}";

        private string Message(string color, string text, string target, double? elapsedMilliseconds) =>
            $"{GetPrefix(target)}{color}{text}{p.Default}{GetSuffix(true, elapsedMilliseconds)}{p.Default}";

        private string MessageWithInput<TInput>(string color, string text, string target, TInput input) =>
            $"{GetPrefix(target, input)}{color}{text}{p.Default}";

        private string MessageWithInput<TInput>(string color, string text, string target, TInput input, double? elapsedMilliseconds) =>
            $"{GetPrefix(target, input)}{color}{text}{p.Default}{GetSuffix(true, elapsedMilliseconds)}{p.Default}";

        private string GetPrefix() =>
            $"{p.Label}{prefix}{p.Symbol}: {p.Default}";

        private string GetPrefix(Stack<string> targets) =>
            $"{p.Label}{prefix}{p.Symbol}/{p.Label}{string.Join($"{p.Symbol}/{p.Label}", targets.Reverse())}{p.Symbol}: {p.Default}";

        private string GetPrefix(string target) =>
            $"{p.Label}{prefix}{p.Symbol}/{p.Label}{target}{p.Symbol}: {p.Default}";

        private string GetPrefix<TInput>(string target, TInput input) =>
            $"{p.Label}{prefix}{p.Symbol}/{p.Label}{target}{p.Symbol}/{p.Input}{input}{p.Symbol}: {p.Default}";

        private string GetSuffix(bool specific, double? elapsedMilliseconds) =>
            (!specific && this.dryRun ? $"{p.Option} (dry run){p.Default}" : "") +
                (!specific && this.parallel ? $"{p.Option} (parallel){p.Default}" : "") +
                (!specific && this.skipDependencies ? $"{p.Option} (skip dependencies){p.Default}" : "") +
                (!this.dryRun && elapsedMilliseconds.HasValue ? $"{p.Timing} ({ToStringFromMilliseconds(elapsedMilliseconds.Value)}){p.Default}" : "");

        private static string ToStringFromMilliseconds(double? milliseconds, bool @fixed) =>
            milliseconds.HasValue ? ToStringFromMilliseconds(milliseconds.Value, @fixed) : string.Empty;

        private static string ToStringFromMilliseconds(double milliseconds, bool @fixed = false)
        {
            // less than one millisecond
            if (milliseconds < 1D)
            {
                return "<1 ms";
            }

            // milliseconds
            if (milliseconds < 1_000D)
            {
                return milliseconds.ToString(@fixed ? "F0" : "G3", provider) + " ms";
            }

            // seconds
            if (milliseconds < 60_000D)
            {
                return (milliseconds / 1_000D).ToString(@fixed ? "F2" : "G3", provider) + " s";
            }

            // minutes and seconds
            if (milliseconds < 3_600_000D)
            {
                var minutes = Math.Floor(milliseconds / 60_000D).ToString("F0", provider);
                var seconds = ((milliseconds % 60_000D) / 1_000D).ToString("F0", provider);
                return seconds == "0"
                    ? $"{minutes} m"
                    : $"{minutes} m {seconds} s";
            }

            // minutes
            return (milliseconds / 60_000d).ToString("N0", provider) + " m";
        }

        private class TargetResult
        {
            public TargetOutcome Outcome { get; set; }

            public double? DurationMilliseconds { get; set; }

            public ConcurrentQueue<TargetInputResult> InputResults { get; } = new ConcurrentQueue<TargetInputResult>();
        }

        private class TargetInputResult
        {
            public object Input { get; set; }

            public TargetInputOutcome Outcome { get; set; }

            public double DurationMilliseconds { get; set; }
        }

        private enum TargetOutcome
        {
            Succeeded,
            Failed,
            NoInputs,
        }

        private enum TargetInputOutcome
        {
            Succeeded,
            Failed,
        }
    }
}
#pragma warning restore IDE0009 // Member access should be qualified.
