#pragma warning disable CA1034 // Nested types should not be visible
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Bullseye.Internal
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using static System.Math;

    public class Summary
    {
        private readonly ConcurrentDictionary<string, TargetResult> results = new ConcurrentDictionary<string, TargetResult>();
        private readonly Palette palette;
        private int resultOrdinal;

        public Summary(Palette palette) => this.palette = palette;

        public TargetResult Intern(string target) => this.results.GetOrAdd(target, key => new TargetResult(Interlocked.Increment(ref this.resultOrdinal)));

        private async Task Results()
        {
            // whitespace (e.g. can change to '·' for debugging)
            var ws = ' ';

            var totalDuration = results.Aggregate(
                TimeSpan.Zero,
                (total, result) =>
                    total +
                    (result.Value.Duration ?? result.Value.InputResults.Aggregate(TimeSpan.Zero, (inputTotal, input) => inputTotal + input.Duration)));

            var rows = new List<SummaryRow> { new SummaryRow { TargetOrInput = $"{p.Default}Target{p.Reset}", Outcome = $"{p.Default}Outcome{p.Reset}", Duration = $"{p.Default}Duration{p.Reset}", Percentage = "" } };

            foreach (var item in results.OrderBy(i => i.Value.Ordinal))
            {
                var target = $"{p.Target}{item.Key}{p.Reset}";

                var outcome = item.Value.Outcome == TargetOutcome.Failed
                    ? $"{p.Failed}Failed!{p.Reset}"
                    : item.Value.Outcome == TargetOutcome.NoInputs
                        ? $"{p.Warning}No inputs!{p.Reset}"
                        : $"{p.Succeeded}Succeeded{p.Reset}";

                var duration = $"{p.Timing}{ToString(item.Value.Duration, true)}{p.Reset}";

                var percentage = item.Value.Duration.HasValue && totalDuration > TimeSpan.Zero
                    ? $"{p.Timing}{100 * item.Value.Duration.Value.TotalMilliseconds / totalDuration.TotalMilliseconds:N1}%{p.Reset}"
                    : "";

                rows.Add(new SummaryRow { TargetOrInput = target, Outcome = outcome, Duration = duration, Percentage = percentage });

                var index = 0;

                foreach (var result in item.Value.InputResults)
                {
                    var input = $"{ws}{ws}{p.Input}{result.Input}{p.Reset}";

                    var inputOutcome = result.Outcome == TargetInputOutcome.Failed ? $"{p.Failed}Failed!{p.Reset}" : $"{p.Succeeded}Succeeded{p.Reset}";

                    var inputDuration = $"{(index < item.Value.InputResults.Count - 1 ? p.TreeFork : p.TreeCorner)}{p.Timing}{ToString(result.Duration, true)}{p.Reset}";

                    var inputPercentage = totalDuration > TimeSpan.Zero
                        ? $"{(index < item.Value.InputResults.Count - 1 ? p.TreeFork : p.TreeCorner)}{p.Timing}{100 * result.Duration.TotalMilliseconds / totalDuration.TotalMilliseconds:N1}%{p.Reset}"
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

        public class TargetResult
        {
            public TargetResult(int ordinal) => this.Ordinal = ordinal;

            public int Ordinal { get; }

            public TargetOutcome Outcome { get; set; }

            public TimeSpan? Duration { get; set; }

            public ConcurrentQueue<TargetInputResult> InputResults { get; } = new ConcurrentQueue<TargetInputResult>();
        }

        public class TargetInputResult
        {
            public object Input { get; set; }

            public TargetInputOutcome Outcome { get; set; }

            public TimeSpan Duration { get; set; }
        }

        private class SummaryRow
        {
            public string TargetOrInput { get; set; }

            public string Outcome { get; set; }

            public string Duration { get; set; }

            public string Percentage { get; set; }
        }

        public enum TargetOutcome
        {
            Succeeded,
            Failed,
            NoInputs,
        }

        public enum TargetInputOutcome
        {
            Succeeded,
            Failed,
        }
    }
}
