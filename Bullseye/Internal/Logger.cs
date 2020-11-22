#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable IDE0009 // Member access should be qualified.
#pragma warning disable RS0016 // Add public types and members to the declared API
namespace Bullseye.Internal
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using static System.Math;

    public class Logger
    {
        private readonly ConcurrentDictionary<string, TargetResult> results = new ConcurrentDictionary<string, TargetResult>();
        private readonly TextWriter writer;
        private readonly string prefix;
        private readonly bool skipDependencies;
        private readonly bool dryRun;
        private readonly bool parallel;
        private readonly Palette p;
        private readonly bool verbose;

        private int resultOrdinal;
        private TimeSpan? totalDuration;

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

        public async Task Version(Func<string> getVersion)
        {
            if (this.verbose)
            {
                await this.writer.WriteLineAsync(Message(p.Verbose, $"Bullseye version: {getVersion()}")).Tax();
            }
        }

        public Task Error(string message) => this.writer.WriteLineAsync(Message(p.Failed, message));

        public async Task Verbose(Func<string> getMessage)
        {
            if (this.verbose)
            {
                await this.writer.WriteLineAsync(Message(p.Verbose, getMessage())).Tax();
            }
        }

        public async Task Verbose(Stack<string> targets, string message)
        {
            if (this.verbose)
            {
                await this.writer.WriteLineAsync(Message(targets, p.Verbose, message)).Tax();
            }
        }

        public Task Starting(List<string> targets) =>
            this.writer.WriteLineAsync(Message(p.Default, $"Starting...", targets, null));

        public async Task Failed(List<string> targets)
        {
            await this.Results().Tax();
            await this.writer.WriteLineAsync(Message(p.Failed, $"Failed!", targets, totalDuration)).Tax();
        }

        public async Task Succeeded(List<string> targets)
        {
            await this.Results().Tax();
            await this.writer.WriteLineAsync(Message(p.Succeeded, $"Succeeded", targets, totalDuration)).Tax();
        }

        public Task Starting(string target)
        {
            InternResult(target);

            return this.writer.WriteLineAsync(Message(p.Default, "Starting...", target, null));
        }

        public Task Error(string target, Exception ex) =>
            this.writer.WriteLineAsync(Message(p.Failed, ex.ToString(), target));

        public Task Failed(string target, Exception ex, TimeSpan? duration)
        {
            var result = InternResult(target);
            result.Outcome = TargetOutcome.Failed;
            result.Duration = result.Duration.Add(duration);

            totalDuration = totalDuration.Add(duration);

            return this.writer.WriteLineAsync(Message(p.Failed, $"Failed! {ex.Message}", target, result.Duration));
        }

        public Task Failed(string target)
        {
            var result = InternResult(target);
            result.Outcome = TargetOutcome.Failed;

            return this.writer.WriteLineAsync(Message(p.Failed, $"Failed!", target, result.Duration));
        }

#pragma warning disable RS0027 // Public API with optional parameter(s) should have the most parameters amongst its public overloads.
        public Task Succeeded(string target, TimeSpan? duration = null)
#pragma warning restore RS0027 // Public API with optional parameter(s) should have the most parameters amongst its public overloads.
        {
            var result = InternResult(target);
            result.Outcome = TargetOutcome.Succeeded;
            result.Duration = result.Duration.Add(duration);

            totalDuration = totalDuration.Add(duration);

            return this.writer.WriteLineAsync(Message(p.Succeeded, "Succeeded", target, result.Duration));
        }

        public Task Starting<TInput>(string target, TInput input, Guid inputId)
        {
            var (_, targetInputResult) = Intern(target, inputId);
            targetInputResult.Input = input;

            return this.writer.WriteLineAsync(MessageWithInput(p.Default, "Starting...", target, input, null));
        }

        public Task Error<TInput>(string target, TInput input, Exception ex) =>
            this.writer.WriteLineAsync(MessageWithInput(p.Failed, ex.ToString(), target, input));

        public Task Failed<TInput>(string target, TInput input, Exception ex, TimeSpan? duration, Guid inputId)
        {
            var (targetResult, targetInputResult) = Intern(target, inputId);

            targetInputResult.Input = input;
            targetInputResult.Outcome = TargetInputOutcome.Failed;
            targetInputResult.Duration = targetInputResult.Duration.Add(duration);

            targetResult.Duration = targetResult.Duration.Add(duration);

            totalDuration = totalDuration.Add(duration);

            return this.writer.WriteLineAsync(MessageWithInput(p.Failed, $"Failed! {ex.Message}", target, targetInputResult.Input, targetInputResult.Duration));
        }

        public Task Succeeded<TInput>(string target, TInput input, TimeSpan? duration, Guid inputId)
        {
            var (targetResult, targetInputResult) = Intern(target, inputId);

            targetInputResult.Input = input;
            targetInputResult.Outcome = TargetInputOutcome.Succeeded;
            targetInputResult.Duration = targetInputResult.Duration.Add(duration);

            targetResult.Duration = targetResult.Duration.Add(duration);

            totalDuration = totalDuration.Add(duration);

            return this.writer.WriteLineAsync(MessageWithInput(p.Succeeded, "Succeeded", target, targetInputResult.Input, targetInputResult.Duration));
        }

        public Task NoInputs(string target)
        {
            InternResult(target).Outcome = TargetOutcome.NoInputs;

            return this.writer.WriteLineAsync(Message(p.Warning, "No inputs!", target, null));
        }

        private TargetResult InternResult(string target) => this.results.GetOrAdd(target, key => new TargetResult(Interlocked.Increment(ref this.resultOrdinal)));

        private (TargetResult, TargetInputResult) Intern(string target, Guid inputId)
        {
            var targetResult = InternResult(target);
            var targetInputResult = targetResult.InputResults.GetOrAdd(inputId, key => new TargetInputResult(Interlocked.Increment(ref this.resultOrdinal)));
            return (targetResult, targetInputResult);
        }

        private async Task Results()
        {
            // whitespace (e.g. can change to '·' for debugging)
            var ws = ' ';

            var rows = new List<SummaryRow> { new SummaryRow { TargetOrInput = $"{p.Default}Target{p.Reset}", Outcome = $"{p.Default}Outcome{p.Reset}", Duration = $"{p.Default}Duration{p.Reset}", Percentage = "" } };

            foreach (var item in results.OrderBy(i => i.Value.Ordinal))
            {
                var target = $"{p.Target}{item.Key}{p.Reset}";

                var outcome = item.Value.Outcome == TargetOutcome.Failed
                    ? $"{p.Failed}Failed!{p.Reset}"
                    : item.Value.Outcome == TargetOutcome.NoInputs
                        ? $"{p.Warning}No inputs!{p.Reset}"
                        : $"{p.Succeeded}Succeeded{p.Reset}";

                var duration = item.Value.Duration.HasValue
                    ? $"{p.Timing}{item.Value.Duration.Humanize()}{p.Reset}"
                    : "";

                var percentage = item.Value.Duration.HasValue && totalDuration.HasValue && totalDuration.Value > TimeSpan.Zero
                    ? $"{p.Timing}{100 * item.Value.Duration.Value.TotalMilliseconds / totalDuration.Value.TotalMilliseconds:N1}%{p.Reset}"
                    : "";

                rows.Add(new SummaryRow { TargetOrInput = target, Outcome = outcome, Duration = duration, Percentage = percentage });

                var index = 0;

                foreach (var result in item.Value.InputResults.Values.OrderBy(result => result.Ordinal))
                {
                    var input = $"{ws}{ws}{p.Input}{result.Input}{p.Reset}";

                    var inputOutcome = result.Outcome == TargetInputOutcome.Failed ? $"{p.Failed}Failed!{p.Reset}" : $"{p.Succeeded}Succeeded{p.Reset}";

                    var inputDuration = result.Duration.HasValue
                        ? $"{(index < item.Value.InputResults.Count - 1 ? p.TreeFork : p.TreeCorner)}{p.Timing}{result.Duration.Humanize()}{p.Reset}"
                        : "";

                    var inputPercentage = result.Duration.HasValue && totalDuration.HasValue && totalDuration.Value > TimeSpan.Zero
                        ? $"{(index < item.Value.InputResults.Count - 1 ? p.TreeFork : p.TreeCorner)}{p.Timing}{100 * result.Duration.Value.TotalMilliseconds / totalDuration.Value.TotalMilliseconds:N1}%{p.Reset}"
                        : "";

                    rows.Add(new SummaryRow { TargetOrInput = input, Outcome = inputOutcome, Duration = inputDuration, Percentage = inputPercentage });

                    ++index;
                }
            }

            // target or input column width
            var tarW = rows.Max(row => Palette.StripColours(row.TargetOrInput).Length);

            // outcome column width
            var outW = rows.Max(row => Palette.StripColours(row.Outcome).Length);

            // duration column width
            var durW = rows.Count > 1 ? rows.Skip(1).Max(row => Palette.StripColours(row.Duration).Length) : 0;

            // percentage column width
            var perW = rows.Max(row => Palette.StripColours(row.Percentage).Length);

            // timing column width (duration and percentage)
            var timW = Max(Palette.StripColours(rows[0].Duration).Length, durW + 2 + perW);

            // expand percentage column width to ensure time and percentage are as wide as duration
            perW = Max(timW - durW - 2, perW);

            // summary start separator
            await this.writer.WriteLineAsync($"{GetPrefix()}{p.Default}{"".Prp(tarW + 2 + outW + 2 + timW, p.Dash)}{p.Reset}").Tax();

            // header
            await this.writer.WriteLineAsync($"{GetPrefix()}{rows[0].TargetOrInput.Prp(tarW, ws)}{ws}{ws}{rows[0].Outcome.Prp(outW, ws)}{ws}{ws}{rows[0].Duration.Prp(timW, ws)}").Tax();

            // header separator
            await this.writer.WriteLineAsync($"{GetPrefix()}{p.Default}{"".Prp(tarW, p.Dash)}{p.Reset}{ws}{ws}{p.Default}{"".Prp(outW, p.Dash)}{p.Reset}{ws}{ws}{p.Default}{"".Prp(timW, p.Dash)}{p.Reset}").Tax();

            // targets
            foreach (var row in rows.Skip(1))
            {
                await this.writer.WriteLineAsync($"{GetPrefix()}{row.TargetOrInput.Prp(tarW, ws)}{p.Reset}{ws}{ws}{row.Outcome.Prp(outW, ws)}{p.Reset}{ws}{ws}{row.Duration.Prp(durW, ws)}{p.Reset}{ws}{ws}{row.Percentage.Prp(perW, ws)}{p.Reset}").Tax();
            }

            // summary end separator
            await this.writer.WriteLineAsync($"{GetPrefix()}{p.Default}{"".Prp(tarW + 2 + outW + 2 + timW, p.Dash)}{p.Reset}").Tax();
        }

        private string Message(string color, string text) => $"{GetPrefix()}{color}{text}{p.Reset}";

        private string Message(Stack<string> targets, string color, string text) => $"{GetPrefix(targets)}{color}{text}{p.Reset}";

        private string Message(string color, string text, List<string> targets, TimeSpan? duration) =>
            $"{GetPrefix()}{color}{text}{p.Reset} {p.Target}({targets.Spaced()}){p.Reset}{GetSuffix(false, duration)}{p.Reset}";

        private string Message(string color, string text, string target) =>
            $"{GetPrefix(target)}{color}{text}{p.Reset}";

        private string Message(string color, string text, string target, TimeSpan? duration) =>
            $"{GetPrefix(target)}{color}{text}{p.Reset}{GetSuffix(true, duration)}{p.Reset}";

        private string MessageWithInput<TInput>(string color, string text, string target, TInput input) =>
            $"{GetPrefix(target, input)}{color}{text}{p.Reset}";

        private string MessageWithInput<TInput>(string color, string text, string target, TInput input, TimeSpan? duration) =>
            $"{GetPrefix(target, input)}{color}{text}{p.Reset}{GetSuffix(true, duration)}{p.Reset}";

        private string GetPrefix() =>
            $"{p.Prefix}{prefix}:{p.Reset} ";

        private string GetPrefix(Stack<string> targets) =>
            $"{p.Prefix}{prefix}:{p.Reset} {p.Target}{string.Join($"{p.Default}/{p.Target}", targets.Reverse())}{p.Default}:{p.Reset} ";

        private string GetPrefix(string target) =>
            $"{p.Prefix}{prefix}:{p.Reset} {p.Target}{target}{p.Default}:{p.Reset} ";

        private string GetPrefix<TInput>(string target, TInput input) =>
            $"{p.Prefix}{prefix}:{p.Reset} {p.Target}{target}{p.Default}/{p.Input}{input}{p.Default}:{p.Reset} ";

        private string GetSuffix(bool specific, TimeSpan? duration) =>
            (!specific && this.dryRun ? $" {p.Option}(dry run){p.Reset}" : "") +
                (!specific && this.parallel ? $" {p.Option}(parallel){p.Reset}" : "") +
                (!specific && this.skipDependencies ? $" {p.Option}(skip dependencies){p.Reset}" : "") +
                (!this.dryRun && duration.HasValue ? $" {p.Timing}({duration.Humanize()}){p.Reset}" : "");

        private class TargetResult
        {
            public TargetResult(int ordinal) => this.Ordinal = ordinal;

            public int Ordinal { get; }

            public TargetOutcome Outcome { get; set; }

            public TimeSpan? Duration { get; set; }

            public ConcurrentDictionary<Guid, TargetInputResult> InputResults { get; } = new ConcurrentDictionary<Guid, TargetInputResult>();
        }

        private class TargetInputResult
        {
            public TargetInputResult(int ordinal) => this.Ordinal = ordinal;

            public int Ordinal { get; }

            public object Input { get; set; }

            public TargetInputOutcome Outcome { get; set; }

            public TimeSpan? Duration { get; set; }
        }

        private class SummaryRow
        {
            public string TargetOrInput { get; set; }

            public string Outcome { get; set; }

            public string Duration { get; set; }

            public string Percentage { get; set; }
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
