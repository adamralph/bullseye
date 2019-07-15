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
    using System.Text;
    using System.Threading.Tasks;
    using static System.Math;

    public class Logger
    {
        private static readonly IFormatProvider provider = CultureInfo.InvariantCulture;

        private readonly ConcurrentDictionary<string, TargetResult> results = new ConcurrentDictionary<string, TargetResult>();
        private readonly TextWriter writer;
        private readonly bool skipDependencies;
        private readonly bool dryRun;
        private readonly bool parallel;
        private readonly Palette p;
        private readonly bool verbose;

        public Logger(TextWriter writer, bool skipDependencies, bool dryRun, bool parallel, Palette palette, bool verbose)
        {
            this.writer = writer;
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

        public Task Usage() => this.writer.WriteLineAsync(this.GetUsage());

        public Task Targets(TargetCollection targets, List<string> rootTargets, int maxDepth, int maxDepthToShowInputs, bool listInputs) =>
            this.writer.WriteLineAsync(this.List(targets, rootTargets, maxDepth, maxDepthToShowInputs, listInputs));

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

        private string List(TargetCollection targets, List<string> rootTargets, int maxDepth, int maxDepthToShowInputs, bool listInputs)
        {
            var value = new StringBuilder();

            foreach (var rootTarget in rootTargets)
            {
                Append(new List<string> { rootTarget }, new Stack<string>(), true, "", 0);
            }

            return value.ToString();

            void Append(List<string> names, Stack<string> seenTargets, bool isRoot, string previousPrefix, int depth)
            {
                if (depth > maxDepth)
                {
                    return;
                }

                foreach (var item in names.Select((name, index) => new { name, index }))
                {
                    var circularDependency = seenTargets.Contains(item.name);

                    seenTargets.Push(item.name);

                    try
                    {
                        var prefix = isRoot
                            ? ""
                            : $"{previousPrefix.Replace(p.TreeCorner, "  ").Replace(p.TreeFork, p.TreeDown)}{(item.index == names.Count - 1 ? p.TreeCorner : p.TreeFork)}";

                        var isMissing = !targets.Contains(item.name);

                        value.Append($"{p.Tree}{prefix}{(isRoot ? p.Target : p.Dependency)}{item.name}{p.Default}");

                        if (isMissing)
                        {
                            value.AppendLine($" {p.Failed}(missing){p.Default}");
                            continue;
                        }

                        if (circularDependency)
                        {
                            value.AppendLine($" {p.Failed}(circular dependency){p.Default}");
                            continue;
                        }

                        value.AppendLine(p.Default);

                        var target = targets[item.name];

                        if (listInputs && depth <= maxDepthToShowInputs && target is IHaveInputs hasInputs)
                        {
                            foreach (var inputItem in hasInputs.Inputs.Select((input, index) => new { input, index }))
                            {
                                var inputPrefix = $"{prefix.Replace(p.TreeCorner, "  ").Replace(p.TreeFork, p.TreeDown)}{(target.Dependencies.Any() && depth + 1 <= maxDepth ? p.TreeDown : "  ")}";

                                value.AppendLine($"{p.Tree}{inputPrefix}{p.Input}{inputItem.input}{p.Default}");
                            }
                        }

                        Append(target.Dependencies, seenTargets, false, prefix, depth + 1);
                    }
                    finally
                    {
                        seenTargets.Pop();
                    }
                }
            }
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
            $"{p.Label}Bullseye{p.Symbol}: {p.Default}";

        private string GetPrefix(Stack<string> targets) =>
            $"{p.Label}Bullseye{p.Symbol}/{p.Label}{string.Join($"{p.Symbol}/{p.Label}", targets.Reverse())}{p.Symbol}: {p.Default}";

        private string GetPrefix(string target) =>
            $"{p.Label}Bullseye{p.Symbol}/{p.Label}{target}{p.Symbol}: {p.Default}";

        private string GetPrefix<TInput>(string target, TInput input) =>
            $"{p.Label}Bullseye{p.Symbol}/{p.Label}{target}{p.Symbol}/{p.Input}{input}{p.Symbol}: {p.Default}";

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
                    ? $"{minutes} min"
                    : $"{minutes} min {seconds} s";
            }

            // minutes
            return (milliseconds / 60_000d).ToString("N0", provider) + " min";
        }

        private string GetUsage() =>
$@"{p.Label}Usage: {p.CommandLine}<command-line> {p.Option}[<options>] {p.Target}[<targets>]

{p.Label}command-line: {p.Text}The command line which invokes the build targets.
  {p.Label}Examples:
    {p.CommandLine}build.cmd
    {p.CommandLine}build.sh
    {p.CommandLine}dotnet run --project targets --

{p.Label}options:
 {p.Option}-c, --clear                {p.Text}Clear the console before execution
 {p.Option}-n, --dry-run              {p.Text}Do a dry run without executing actions
 {p.Option}-D, --list-dependencies    {p.Text}List all (or specified) targets and dependencies, then exit
 {p.Option}-I, --list-inputs          {p.Text}List all (or specified) targets and inputs, then exit
 {p.Option}-T, --list-targets         {p.Text}List all (or specified) targets, then exit
 {p.Option}-t, --list-tree            {p.Text}List all (or specified) targets and dependency trees, then exit
 {p.Option}-N, --no-color             {p.Text}Disable colored output
 {p.Option}-p, --parallel             {p.Text}Run targets in parallel
 {p.Option}-s, --skip-dependencies    {p.Text}Do not run targets' dependencies
 {p.Option}-v, --verbose              {p.Text}Enable verbose output
 {p.Option}    --appveyor             {p.Text}Force Appveyor mode (normally auto-detected)
 {p.Option}    --azure-pipelines      {p.Text}Force Azure Pipelines mode (normally auto-detected)
 {p.Option}    --teamcity             {p.Text}Force TeamCity mode (normally auto-detected)
 {p.Option}    --travis               {p.Text}Force Travis CI mode (normally auto-detected)
 {p.Option}-h, --help, -?             {p.Text}Show this help, then exit (case insensitive)

{p.Label}targets: {p.Text}A list of targets to run or list.
  If not specified, the {p.Target}""default""{p.Text} target will be run, or all targets will be listed.

{p.Label}Remarks:
  {p.Text}The {p.Option}--list-xxx {p.Text}options can be combined.

{p.Label}Examples:
  {p.CommandLine}build.cmd
  {p.CommandLine}build.cmd {p.Option}-D
  {p.CommandLine}build.sh {p.Option}-t -I {p.Target}default
  {p.CommandLine}build.sh {p.Target}test pack
  {p.CommandLine}dotnet run --project targets -- {p.Option}-n {p.Target}build{p.Default}";

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
