using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using static System.Math;

namespace Bullseye.Internal;

public partial class Output
{
    private readonly ConcurrentDictionary<Target, TargetResult> results = new();

    private int resultOrdinal;
    private TimeSpan totalDuration;

    private TargetResult InternResult(Target target) => this.results.GetOrAdd(target, _ => new TargetResult(Interlocked.Increment(ref this.resultOrdinal)));

    private (TargetResult, TargetInputResult) Intern(Target target, Guid inputId)
    {
        var targetResult = this.InternResult(target);
        var targetInputResult = targetResult.InputResults.GetOrAdd(inputId, _ => new TargetInputResult(Interlocked.Increment(ref this.resultOrdinal)));

        return (targetResult, targetInputResult);
    }

    private static string GetResultLines(IEnumerable<KeyValuePair<Target, TargetResult>> results, TimeSpan totalDuration, Func<string> getPrefix, Palette p)
    {
        // whitespace (e.g. can change to '·' for debugging)
        var ws = ' ';

        var rows = new List<SummaryRow> { new($"{p.Text}Target{p.Default}", $"{p.Text}Outcome{p.Default}", $"{p.Text}Duration{p.Default}", ""), };

        foreach (var (target, targetResult) in results.OrderBy(i => i.Value.Ordinal))
        {
            var outcome = targetResult.Outcome switch
            {
                TargetOutcome.Failed => $"{p.Failure}{FailedMessage}{p.Default}",
                TargetOutcome.NoInputs => $"{p.Warning}{NoInputsMessage}{p.Default}",
                TargetOutcome.Succeeded => $"{p.Success}{SucceededMessage}{p.Default}",
                _ => throw new InvalidOperationException(),
            };

            var duration = $"{p.Timing}{targetResult.Duration.Humanize()}{p.Default}";

            var percentage = totalDuration > TimeSpan.Zero
                ? $"{p.Timing}{100 * targetResult.Duration.TotalMilliseconds / totalDuration.TotalMilliseconds:N1}%{p.Default}"
                : "";

            rows.Add(new SummaryRow($"{p.Target}{target}{p.Default}", outcome, duration, percentage));

            var index = 0;

            foreach (var inputResult in targetResult.InputResults.Values.OrderBy(result => result.Ordinal))
            {
                var input = $"{ws}{ws}{p.Input}{inputResult.Input}{p.Default}";

                var inputOutcome = inputResult.Outcome == TargetInputOutcome.Failed ? $"{p.Failure}{FailedMessage}{p.Default}" : $"{p.Success}{SucceededMessage}{p.Default}";

                var inputDuration = $"{(index < targetResult.InputResults.Count - 1 ? p.TreeFork : p.TreeCorner)}{p.Timing}{inputResult.Duration.Humanize()}{p.Default}";

                var inputPercentage = totalDuration > TimeSpan.Zero
                    ? $"{(index < targetResult.InputResults.Count - 1 ? p.TreeFork : p.TreeCorner)}{p.Timing}{100 * inputResult.Duration.TotalMilliseconds / totalDuration.TotalMilliseconds:N1}%{p.Default}"
                    : "";

                rows.Add(new SummaryRow(input, inputOutcome, inputDuration, inputPercentage));

                ++index;
            }
        }

        // target or input column width
        var tarW = rows.Max(row => Palette.StripColors(row.TargetOrInput).Length);

        // outcome column width
        var outW = rows.Max(row => Palette.StripColors(row.Outcome).Length);

        // duration column width
        var durW = rows.Count > 1 ? rows.Skip(1).Max(row => Palette.StripColors(row.Duration).Length) : 0;

        // percentage column width
        var perW = rows.Max(row => Palette.StripColors(row.Percentage).Length);

        // timing column width (duration and percentage)
        var timW = Max(Palette.StripColors(rows[0].Duration).Length, durW + 2 + perW);

        // expand percentage column width to ensure time and percentage are as wide as duration
        perW = Max(timW - durW - 2, perW);

        var builder = new StringBuilder();

        // summary start separator
        _ = builder.AppendLine(CultureInfo.InvariantCulture, $"{p.Prefix}{getPrefix()}:{p.Default} {p.Text}{Prp("", tarW + 2 + outW + 2 + timW, p.Horizontal)}{p.Default}");

        // header
        _ = builder.AppendLine(CultureInfo.InvariantCulture, $"{p.Prefix}{getPrefix()}:{p.Default} {Prp(rows[0].TargetOrInput, tarW, ws)}{ws}{ws}{Prp(rows[0].Outcome, outW, ws)}{ws}{ws}{Prp(rows[0].Duration, timW, ws)}");

        // header separator
        _ = builder.AppendLine(CultureInfo.InvariantCulture, $"{p.Prefix}{getPrefix()}:{p.Default} {p.Text}{Prp("", tarW, p.Horizontal)}{p.Default}{ws}{ws}{p.Text}{Prp("", outW, p.Horizontal)}{p.Default}{ws}{ws}{p.Text}{Prp("", timW, p.Horizontal)}{p.Default}");

        // targets
        foreach (var row in rows.Skip(1))
        {
            _ = builder.AppendLine(CultureInfo.InvariantCulture, $"{p.Prefix}{getPrefix()}:{p.Default} {Prp(row.TargetOrInput, tarW, ws)}{p.Default}{ws}{ws}{Prp(row.Outcome, outW, ws)}{p.Default}{ws}{ws}{Prp(row.Duration, durW, ws)}{p.Default}{ws}{ws}{Prp(row.Percentage, perW, ws)}{p.Default}");
        }

        // summary end separator
        _ = builder.AppendLine(CultureInfo.InvariantCulture, $"{p.Prefix}{getPrefix()}:{p.Default} {p.Text}{Prp("", tarW + 2 + outW + 2 + timW, p.Horizontal)}{p.Default}");

        return builder.ToString();

        // pad right printed
        static string Prp(string text, int totalWidth, char paddingChar) =>
            text.PadRight(totalWidth + (text.Length - Palette.StripColors(text).Length), paddingChar);
    }

#if NET8_0_OR_GREATER
    private sealed class TargetResult(int ordinal)
    {
        public int Ordinal { get; } = ordinal;
#else
    private sealed class TargetResult
    {
        public TargetResult(int ordinal) => this.Ordinal = ordinal;

        public int Ordinal { get; }
#endif
        public TargetOutcome Outcome { get; set; }

        public TimeSpan Duration { get; set; }

        public ConcurrentDictionary<Guid, TargetInputResult> InputResults { get; } = new();
    }

#if NET8_0_OR_GREATER
    private sealed class TargetInputResult(int ordinal)
    {
        public int Ordinal { get; } = ordinal;
#else
    private sealed class TargetInputResult
    {
        public TargetInputResult(int ordinal) => this.Ordinal = ordinal;

        public int Ordinal { get; }
#endif
        public object? Input { get; set; }

        public TargetInputOutcome Outcome { get; set; }

        public TimeSpan Duration { get; set; }
    }

    private sealed record SummaryRow(string TargetOrInput, string Outcome, string Duration, string Percentage);

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
