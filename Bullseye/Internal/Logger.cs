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
        private static readonly IFormatProvider Provider = CultureInfo.InvariantCulture;

        private readonly ConcurrentDictionary<string, TargetResult> _results = new ConcurrentDictionary<string, TargetResult>();
        private readonly TextWriter _writer;
        private readonly bool _skipDependencies;
        private readonly bool _dryRun;
        private readonly bool _parallel;
        private readonly Palette _p;
        private readonly bool _verbose;

        public Logger(TextWriter writer, bool skipDependencies, bool dryRun, bool parallel, Palette palette, bool verbose)
        {
            _writer = writer;
            _skipDependencies = skipDependencies;
            _dryRun = dryRun;
            _parallel = parallel;
            _p = palette;
            _verbose = verbose;
        }

        public async Task Version()
        {
            if (_verbose)
            {
                var version = typeof(TargetCollectionExtensions).Assembly.GetCustomAttributes(false)
                    .OfType<AssemblyInformationalVersionAttribute>()
                    .FirstOrDefault()
                    ?.InformationalVersion ?? "Unknown";

                await _writer.WriteLineAsync(Message(_p.Verbose, $"Version: {version}")).Tax();
            }
        }

        public Task Usage(TargetCollection targets) => _writer.WriteLineAsync(GetUsage(targets));

        public Task Targets(TargetCollection targets, List<string> rootTargets, int maxDepth, int maxDepthToShowInputs, bool listInputs) =>
            _writer.WriteLineAsync(List(targets, rootTargets, maxDepth, maxDepthToShowInputs, listInputs));

        public Task Error(string message) => _writer.WriteLineAsync(Message(_p.Failed, message));

        public async Task Verbose(string message)
        {
            if (_verbose)
            {
                await _writer.WriteLineAsync(Message(_p.Verbose, message)).Tax();
            }
        }

        public async Task Verbose(Stack<string> targets, string message)
        {
            if (_verbose)
            {
                await _writer.WriteLineAsync(Message(targets, _p.Verbose, message)).Tax();
            }
        }

        public Task Running(List<string> targets) =>
            _writer.WriteLineAsync(Message(_p.Starting, $"Starting...", targets, null));

        public async Task Failed(List<string> targets, double elapsedMilliseconds)
        {
            await Results().Tax();
            await _writer.WriteLineAsync(Message(_p.Failed, $"Failed!", targets, elapsedMilliseconds)).Tax();
        }

        public async Task Succeeded(List<string> targets, double elapsedMilliseconds)
        {
            await Results().Tax();
            await _writer.WriteLineAsync(Message(_p.Succeeded, $"Succeeded.", targets, elapsedMilliseconds)).Tax();
        }

        public Task Starting(string target) =>
            _writer.WriteLineAsync(Message(_p.Starting, "Starting...", target, null));

        public Task Error(string target, Exception ex) =>
            _writer.WriteLineAsync(Message(_p.Failed, ex.ToString(), target));

        public Task Failed(string target, Exception ex, double elapsedMilliseconds)
        {
            var result = _results.GetOrAdd(target, key => new TargetResult());
            result.Outcome = TargetOutcome.Failed;
            result.DurationMilliseconds = elapsedMilliseconds;

            return _writer.WriteLineAsync(Message(_p.Failed, $"Failed! {ex.Message}", target, elapsedMilliseconds));
        }

        public Task Failed(string target, double elapsedMilliseconds)
        {
            var result = _results.GetOrAdd(target, key => new TargetResult());
            result.Outcome = TargetOutcome.Failed;
            result.DurationMilliseconds = elapsedMilliseconds;

            return _writer.WriteLineAsync(Message(_p.Failed, $"Failed!", target, elapsedMilliseconds));
        }

        public Task Succeeded(string target, double? elapsedMilliseconds)
        {
            var result = _results.GetOrAdd(target, key => new TargetResult());
            result.Outcome = TargetOutcome.Succeeded;
            result.DurationMilliseconds = elapsedMilliseconds;

            return _writer.WriteLineAsync(Message(_p.Succeeded, "Succeeded.", target, elapsedMilliseconds));
        }

        public Task Starting<TInput>(string target, TInput input) =>
            _writer.WriteLineAsync(MessageWithInput(_p.Starting, "Starting...", target, input, null));

        public Task Error<TInput>(string target, TInput input, Exception ex) =>
            _writer.WriteLineAsync(MessageWithInput(_p.Failed, ex.ToString(), target, input));

        public Task Failed<TInput>(string target, TInput input, Exception ex, double elapsedMilliseconds)
        {
            _results.GetOrAdd(target, key => new TargetResult()).InputResults
                .Enqueue(new TargetInputResult { Input = input, Outcome = TargetInputOutcome.Failed, DurationMilliseconds = elapsedMilliseconds });

            return _writer.WriteLineAsync(MessageWithInput(_p.Failed, $"Failed! {ex.Message}", target, input, elapsedMilliseconds));
        }

        public Task Succeeded<TInput>(string target, TInput input, double elapsedMilliseconds)
        {
            _results.GetOrAdd(target, key => new TargetResult()).InputResults
                .Enqueue(new TargetInputResult { Input = input, Outcome = TargetInputOutcome.Succeeded, DurationMilliseconds = elapsedMilliseconds });

            return _writer.WriteLineAsync(MessageWithInput(_p.Succeeded, "Succeeded.", target, input, elapsedMilliseconds));
        }

        public Task NoInputs(string target)
        {
            _results.GetOrAdd(target, key => new TargetResult()).Outcome = TargetOutcome.NoInputs;

            return _writer.WriteLineAsync(Message(_p.Warning, "No inputs!", target, null));
        }

        private async Task Results()
        {
            // whitespace (e.g. can change to '·' for debugging)
            var ws = ' ';

            var totalDuration = _results.Sum(i => i.Value.DurationMilliseconds ?? 0 + i.Value.InputResults.Sum(i2 => i2.DurationMilliseconds));

            var rows = new List<Tuple<string, string, string, string>> { Tuple.Create($"{_p.Label}Duration", "", $"{_p.Label}Outcome", $"{_p.Label}Target") };

            foreach (var item in _results.OrderBy(i => i.Value.DurationMilliseconds))
            {
                var duration = $"{_p.Timing}{ToStringFromMilliseconds(item.Value.DurationMilliseconds, true)}";

                var percentage = item.Value.DurationMilliseconds.HasValue && totalDuration > 0
                    ? $"{_p.Timing}{100 * item.Value.DurationMilliseconds / totalDuration:N1}%"
                    : "";

                var outcome = item.Value.Outcome == TargetOutcome.Failed
                    ? $"{_p.Failed}Failed!"
                    : item.Value.Outcome == TargetOutcome.NoInputs
                        ? $"{_p.Warning}No inputs!"
                        : $"{_p.Succeeded}Succeeded";

                var target = $"{_p.Target}{item.Key}";

                rows.Add(Tuple.Create(duration, percentage, outcome, target));

                var index = 0;

                foreach (var result in item.Value.InputResults.OrderBy(r => r.DurationMilliseconds))
                {
                    var inputDuration = $"{_p.Tree}{(index < item.Value.InputResults.Count - 1 ? _p.TreeFork : _p.TreeCorner)}{_p.Timing}{ToStringFromMilliseconds(result.DurationMilliseconds, true)}";

                    var inputPercentage = totalDuration > 0
                        ? $"{_p.Tree}{(index < item.Value.InputResults.Count - 1 ? _p.TreeFork : _p.TreeCorner)}{_p.Timing}{100 * result.DurationMilliseconds / totalDuration:N1}%"
                        : "";

                    var inputOutcome = result.Outcome == TargetInputOutcome.Failed ? $"{_p.Failed}Failed!" : $"{_p.Succeeded}Succeeded";

                    var input = $"{ws}{ws}{_p.Input}{result.Input.ToString()}";

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
            await _writer.WriteLineAsync($"{GetPrefix()}{_p.Symbol}{"".Prp(durW + 2 + outW + 2 + tarW, _p.Dash)}").Tax();

            // header
            await _writer.WriteLineAsync($"{GetPrefix()}{rows[0].Item1.Prp(durW, ws)}{ws}{ws}{rows[0].Item3.Prp(outW, ws)}{ws}{ws}{rows[0].Item4.Prp(tarW, ws)}").Tax();

            // header separator
            await _writer.WriteLineAsync($"{GetPrefix()}{_p.Symbol}{"".Prp(durW, _p.Dash)}{ws}{ws}{"".Prp(outW, _p.Dash)}{ws}{ws}{"".Prp(tarW, _p.Dash)}").Tax();

            // targets
            foreach (var row in rows.Skip(1))
            {
                await _writer.WriteLineAsync($"{GetPrefix()}{row.Item1.Prp(timW, ws)}{ws}{ws}{row.Item2.Prp(perW, ws)}{ws}{ws}{row.Item3.Prp(outW, ws)}{ws}{ws}{row.Item4.Prp(tarW, ws)}").Tax();
            }

            // summary end separator
            await _writer.WriteLineAsync($"{GetPrefix()}{_p.Symbol}{"".Prp(durW + 2 + outW + 2 + tarW, _p.Dash)}{_p.Default}").Tax();
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
                            : $"{previousPrefix.Replace(_p.TreeCorner, "  ").Replace(_p.TreeFork, _p.TreeDown)}{(item.index == names.Count - 1 ? _p.TreeCorner : _p.TreeFork)}";

                        var isMissing = !targets.Contains(item.name);

                        value.Append($"{_p.Tree}{prefix}{(isRoot ? _p.Target : _p.Dependency)}{item.name}{_p.Default}");

                        if (isMissing)
                        {
                            value.AppendLine($" {_p.Failed}(missing){_p.Default}");
                            continue;
                        }

                        if (circularDependency)
                        {
                            value.AppendLine($" {_p.Failed}(circular dependency){_p.Default}");
                            continue;
                        }

                        value.AppendLine(_p.Default);

                        var target = targets[item.name];

                        if (listInputs && depth <= maxDepthToShowInputs && target is IHaveInputs hasInputs)
                        {
                            foreach (var inputItem in hasInputs.Inputs.Select((input, index) => new { input, index }))
                            {
                                var inputPrefix = $"{prefix.Replace(_p.TreeCorner, "  ").Replace(_p.TreeFork, _p.TreeDown)}{(target.Dependencies.Any() && depth + 1 <= maxDepth ? _p.TreeDown : "  ")}";

                                value.AppendLine($"{_p.Tree}{inputPrefix}{_p.Input}{inputItem.input}{_p.Default}");
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

        private string Message(string color, string text) => $"{GetPrefix()}{color}{text}{_p.Default}";

        private string Message(Stack<string> targets, string color, string text) => $"{GetPrefix(targets)}{color}{text}{_p.Default}";

        private string Message(string color, string text, List<string> targets, double? elapsedMilliseconds) =>
            $"{GetPrefix()}{color}{text}{_p.Target} ({targets.Spaced()}){_p.Default}{GetSuffix(false, elapsedMilliseconds)}{_p.Default}";

        private string Message(string color, string text, string target) =>
            $"{GetPrefix(target)}{color}{text}{_p.Default}";

        private string Message(string color, string text, string target, double? elapsedMilliseconds) =>
            $"{GetPrefix(target)}{color}{text}{_p.Default}{GetSuffix(true, elapsedMilliseconds)}{_p.Default}";

        private string MessageWithInput<TInput>(string color, string text, string target, TInput input) =>
            $"{GetPrefix(target, input)}{color}{text}{_p.Default}";

        private string MessageWithInput<TInput>(string color, string text, string target, TInput input, double? elapsedMilliseconds) =>
            $"{GetPrefix(target, input)}{color}{text}{_p.Default}{GetSuffix(true, elapsedMilliseconds)}{_p.Default}";

        private string GetPrefix() =>
            $"{_p.Label}Bullseye{_p.Symbol}: {_p.Default}";

        private string GetPrefix(Stack<string> targets) =>
            $"{_p.Label}Bullseye{_p.Symbol}/{_p.Label}{string.Join($"{_p.Symbol}/{_p.Label}", targets.Reverse())}{_p.Symbol}: {_p.Default}";

        private string GetPrefix(string target) =>
            $"{_p.Label}Bullseye{_p.Symbol}/{_p.Label}{target}{_p.Symbol}: {_p.Default}";

        private string GetPrefix<TInput>(string target, TInput input) =>
            $"{_p.Label}Bullseye{_p.Symbol}/{_p.Label}{target}{_p.Symbol}/{_p.Input}{input}{_p.Symbol}: {_p.Default}";

        private string GetSuffix(bool specific, double? elapsedMilliseconds) =>
            (!specific && _dryRun ? $"{_p.Option} (dry run){_p.Default}" : "") +
                (!specific && _parallel ? $"{_p.Option} (parallel){_p.Default}" : "") +
                (!specific && _skipDependencies ? $"{_p.Option} (skip dependencies){_p.Default}" : "") +
                (!_dryRun && elapsedMilliseconds.HasValue ? $"{_p.Timing} ({ToStringFromMilliseconds(elapsedMilliseconds.Value)}){_p.Default}" : "");

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
                return milliseconds.ToString(@fixed ? "F0" : "G3", Provider) + " ms";
            }

            // seconds
            if (milliseconds < 60_000D)
            {
                return (milliseconds / 1_000D).ToString(@fixed ? "F2" : "G3", Provider) + " s";
            }

            // minutes and seconds
            if (milliseconds < 3_600_000D)
            {
                var minutes = Math.Floor(milliseconds / 60_000D).ToString("F0", Provider);
                var seconds = ((milliseconds % 60_000D) / 1_000D).ToString("F0", Provider);
                return seconds == "0"
                    ? $"{minutes} m"
                    : $"{minutes} m {seconds} s";
            }

            // minutes
            return (milliseconds / 60_000d).ToString("N0", Provider) + " m";
        }

        private string GetUsage(TargetCollection targets) =>
$@"{_p.Label}Usage: {_p.CommandLine}<command-line> {_p.Option}[<options>] {_p.Target}[<targets>]

{_p.Label}command-line: {_p.Text}The command line which invokes the build targets.
  {_p.Label}Examples:
    {_p.CommandLine}build.cmd
    {_p.CommandLine}build.sh
    {_p.CommandLine}dotnet run --project targets --

{_p.Label}options:
 {_p.Option}-c, --clear                {_p.Text}Clear the console before execution
 {_p.Option}-n, --dry-run              {_p.Text}Do a dry run without executing actions
 {_p.Option}-d, --list-dependencies    {_p.Text}List all (or specified) targets and dependencies, then exit
 {_p.Option}-i, --list-inputs          {_p.Text}List all (or specified) targets and inputs, then exit
 {_p.Option}-l, --list-targets         {_p.Text}List all (or specified) targets, then exit
 {_p.Option}-t, --list-tree            {_p.Text}List all (or specified) targets and dependency trees, then exit
 {_p.Option}-N, --no-color             {_p.Text}Disable colored output
 {_p.Option}-p, --parallel             {_p.Text}Run targets in parallel
 {_p.Option}-s, --skip-dependencies    {_p.Text}Do not run targets' dependencies
 {_p.Option}-v, --verbose              {_p.Text}Enable verbose output
 {_p.Option}    --appveyor             {_p.Text}Force Appveyor mode (normally auto-detected)
 {_p.Option}    --azure-pipelines      {_p.Text}Force Azure Pipelines mode (normally auto-detected)
 {_p.Option}    --teamcity             {_p.Text}Force TeamCity mode (normally auto-detected)
 {_p.Option}    --travis               {_p.Text}Force Travis CI mode (normally auto-detected)
 {_p.Option}-h, --help, -?             {_p.Text}Show this help, then exit (case insensitive)

{_p.Label}targets: {_p.Text}A list of targets to run or list.
  If not specified, the {_p.Target}""default""{_p.Text} target will be run, or all targets will be listed.

{_p.Label}Remarks:
  {_p.Text}The {_p.Option}--list-xxx {_p.Text}options can be combined.

{_p.Label}Examples:
  {_p.CommandLine}build.cmd
  {_p.CommandLine}build.cmd {_p.Option}-D
  {_p.CommandLine}build.sh {_p.Option}-t -I {_p.Target}default
  {_p.CommandLine}build.sh {_p.Target}test pack
  {_p.CommandLine}dotnet run --project targets -- {_p.Option}-n {_p.Target}build{_p.Default}

{_p.Label}Targets:
"
            + string.Join(
@"
",
                targets.Select(target => $"  {_p.Target}{target.Name}{_p.Default}"));

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
