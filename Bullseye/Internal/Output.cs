using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bullseye.Internal
{
    public partial class Output
    {
        private const string NoInputsMessage = "No inputs!";
        private const string StartingMessage = "Starting...";
        private const string FailedMessage = "FAILED!";
        private const string SucceededMessage = "Succeeded";

        private readonly TextWriter writer;

        private readonly IReadOnlyCollection<string> args;
        private readonly bool dryRun;
        private readonly Host host;
        private readonly bool hostForced;
        private readonly bool noColor;
        private readonly OperatingSystem operatingSystem;
        private readonly bool parallel;
        private readonly string prefix;
        private readonly bool skipDependencies;

        private readonly Palette palette;
        private readonly string scriptExtension;

        public bool Verbose { get; }

        public Output(
            TextWriter writer,
            IReadOnlyCollection<string> args,
            bool dryRun,
            Host host,
            bool hostForced,
            bool noColor,
            bool noExtendedChars,
            OperatingSystem operatingSystem,
            bool parallel,
            string prefix,
            bool skipDependencies,
            bool verbose)
        {
            this.writer = writer;

            this.args = args;
            this.dryRun = dryRun;
            this.host = host;
            this.hostForced = hostForced;
            this.noColor = noColor;
            this.operatingSystem = operatingSystem;
            this.parallel = parallel;
            this.prefix = prefix;
            this.skipDependencies = skipDependencies;
            this.Verbose = verbose;

            this.palette = new Palette(noColor, noExtendedChars, host, operatingSystem);
            this.scriptExtension = operatingSystem == OperatingSystem.Windows ? "cmd" : "sh";
        }

        public async Task Header(Func<string> getVersion)
        {
            if (!this.Verbose)
            {
                return;
            }

            var version = getVersion();

            var builder = new StringBuilder()
                .AppendLine(Format($"{this.palette.Verbose}{version}{this.palette.Reset}", "Bullseye version", this.prefix, this.palette))
                .AppendLine(Format($"{this.palette.Verbose}{this.host} ({(this.hostForced ? "forced" : "detected")}){this.palette.Reset}", "Host", this.prefix, this.palette))
                .AppendLine(Format($"{this.palette.Verbose}{this.operatingSystem}{this.palette.Reset}", "OS", this.prefix, this.palette))
                .AppendLine(Format($"{this.palette.Verbose}{string.Join(" ", this.args)}{this.palette.Reset}", "Args", this.prefix, this.palette));

            await this.writer.WriteAsync(builder.ToString()).Tax();
        }

        public async Task Usage(TargetCollection targets)
        {
            var usage = GetUsageLines(this.palette, this.scriptExtension)
                + GetListLines(targets, targets.Select(target => target.Name), 0, 0, false, "  ", this.palette);

            await this.writer.WriteAsync(usage).Tax();
        }

        public Task List(TargetCollection targets, IEnumerable<string> rootTargets, int maxDepth, int maxDepthToShowInputs, bool listInputs) =>
            this.writer.WriteAsync(GetListLines(targets, rootTargets, maxDepth, maxDepthToShowInputs, listInputs, "", this.palette));

        public Task Starting(IEnumerable<Target> targets) =>
            this.writer.WriteLineAsync(Format($"{this.palette.Default}{StartingMessage}{this.palette.Reset}", targets, this.dryRun, this.parallel, this.skipDependencies, this.prefix, this.palette));

        public async Task Failed(IEnumerable<Target> targets)
        {
            var message = GetResultLines(this.results, this.totalDuration, this.prefix, this.palette)
                + Format($"{this.palette.Failed}{FailedMessage}{this.palette.Reset}", targets, this.dryRun, this.parallel, this.skipDependencies, this.prefix, this.palette, this.totalDuration);

            await this.writer.WriteLineAsync(message).Tax();
        }

        public async Task Succeeded(IEnumerable<Target> targets)
        {
            var message = GetResultLines(this.results, this.totalDuration, this.prefix, this.palette)
                + Format($"{this.palette.Succeeded}{SucceededMessage}{this.palette.Reset}", targets, this.dryRun, this.parallel, this.skipDependencies, this.prefix, this.palette, this.totalDuration);

            await this.writer.WriteLineAsync(message).Tax();
        }

        public async Task Awaiting(Target target, IReadOnlyCollection<Target> dependencyPath)
        {
            if (this.Verbose)
            {
                await this.writer.WriteLineAsync(Format($"{this.palette.Verbose}Awaiting{this.palette.Reset}", target, this.prefix, this.palette, dependencyPath)).Tax();
            }
        }

        public async Task WalkingDependencies(Target target, IReadOnlyCollection<Target> dependencyPath)
        {
            if (this.Verbose)
            {
                await this.writer.WriteLineAsync(Format($"{this.palette.Verbose}Walking dependencies{this.palette.Reset}", target, this.prefix, this.palette, dependencyPath)).Tax();
            }
        }

        public async Task IgnoringNonExistentDependency(Target target, string dependency, IReadOnlyCollection<Target> dependencyPath)
        {
            if (this.Verbose)
            {
                await this.writer.WriteLineAsync(Format($"{this.palette.Verbose}Ignoring non-existent dependency:{this.palette.Reset} {this.palette.Target}{dependency}{this.palette.Reset}", target, this.prefix, this.palette, dependencyPath)).Tax();
            }
        }

        public async Task BeginGroup(Target target)
        {
            if (!this.parallel && this.host == Host.GitHubActions)
            {
                await this.writer.WriteLineAsync($"::group::{this.palette.Prefix}{this.prefix}:{this.palette.Reset} {this.palette.Target}{target}{this.palette.Reset}").Tax();
            }
        }

        public Task Starting(Target target, IReadOnlyCollection<Target> dependencyPath)
        {
            var targetResult = this.InternResult(target);

            return this.writer.WriteLineAsync(Format($"{this.palette.Default}{StartingMessage}{this.palette.Reset}", target, this.prefix, this.palette, dependencyPath, targetResult.Duration));
        }

        public Task Error(Target target, Exception ex) =>
            this.writer.WriteLineAsync(Format($"{this.palette.Failed}{ex}{this.palette.Reset}", target, this.prefix, this.palette));

        public Task Failed(Target target, Exception ex, TimeSpan? duration, IReadOnlyCollection<Target> dependencyPath)
        {
            var result = this.InternResult(target);
            result.Outcome = TargetOutcome.Failed;
            result.Duration = result.Duration.Add(duration);

            this.totalDuration = this.totalDuration.Add(duration);

            return this.writer.WriteLineAsync(Format($"{this.palette.Failed}{FailedMessage}{this.palette.Reset} {this.palette.Failed}{ex.Message}{this.palette.Reset}", target, this.prefix, this.palette, dependencyPath, duration));
        }

        public Task Failed(Target target, IReadOnlyCollection<Target> dependencyPath)
        {
            var result = this.InternResult(target);
            result.Outcome = TargetOutcome.Failed;

            return this.writer.WriteLineAsync(Format($"{this.palette.Failed}{FailedMessage}{this.palette.Reset}", target, this.prefix, this.palette, dependencyPath, result.Duration));
        }

        public Task Succeeded(Target target, IReadOnlyCollection<Target> dependencyPath, TimeSpan? duration = null)
        {
            var result = this.InternResult(target);
            result.Outcome = TargetOutcome.Succeeded;
            result.Duration = result.Duration.Add(duration);

            this.totalDuration = this.totalDuration.Add(duration);

            return this.writer.WriteLineAsync(Format($"{this.palette.Succeeded}{SucceededMessage}{this.palette.Reset}", target, this.prefix, this.palette, dependencyPath, result.Duration));
        }

        public Task Succeeded(Target target, IReadOnlyCollection<Target> dependencyPath)
        {
            var result = this.InternResult(target);
            result.Outcome = TargetOutcome.Succeeded;

            return this.writer.WriteLineAsync(Format($"{this.palette.Succeeded}{SucceededMessage}{this.palette.Reset}", target, this.prefix, this.palette, dependencyPath, result.Duration));
        }

        public async Task EndGroup()
        {
            if (!this.parallel && this.host == Host.GitHubActions)
            {
                await this.writer.WriteLineAsync("::endgroup::").Tax();
            }
        }

        public Task NoInputs(Target target, IReadOnlyCollection<Target> dependencyPath)
        {
            var targetResult = this.InternResult(target);
            targetResult.Outcome = TargetOutcome.NoInputs;

            return this.writer.WriteLineAsync(Format($"{this.palette.Warning}{NoInputsMessage}{this.palette.Reset}", target, this.prefix, this.palette, dependencyPath, targetResult.Duration));
        }

        public Task Starting<TInput>(Target target, TInput input, Guid inputId, IReadOnlyCollection<Target> dependencyPath)
        {
            var (_, targetInputResult) = this.Intern(target, inputId);
            targetInputResult.Input = input;

            return this.writer.WriteLineAsync(Format(StartingMessage, target, targetInputResult.Input, this.prefix, this.palette, dependencyPath, targetInputResult.Duration));
        }

        public Task Error<TInput>(Target target, TInput input, Exception ex) =>
            this.writer.WriteLineAsync(Format($"{this.palette.Failed}{ex}{this.palette.Reset}", target, input, this.prefix, this.palette));

        public Task Failed<TInput>(Target target, TInput input, Exception ex, TimeSpan? duration, Guid inputId, IReadOnlyCollection<Target> dependencyPath)
        {
            var (targetResult, targetInputResult) = this.Intern(target, inputId);

            targetInputResult.Input = input;
            targetInputResult.Outcome = TargetInputOutcome.Failed;
            targetInputResult.Duration = targetInputResult.Duration.Add(duration);

            targetResult.Duration = targetResult.Duration.Add(duration);

            this.totalDuration = this.totalDuration.Add(duration);

            return this.writer.WriteLineAsync(Format($"{this.palette.Failed}{FailedMessage}{this.palette.Reset} {this.palette.Failed}{ex?.Message}{this.palette.Reset}", target, targetInputResult.Input, this.prefix, this.palette, dependencyPath, targetInputResult.Duration));
        }

        public Task Succeeded<TInput>(Target target, TInput input, TimeSpan? duration, Guid inputId, IReadOnlyCollection<Target> dependencyPath)
        {
            var (targetResult, targetInputResult) = this.Intern(target, inputId);

            targetInputResult.Input = input;
            targetInputResult.Outcome = TargetInputOutcome.Succeeded;
            targetInputResult.Duration = targetInputResult.Duration.Add(duration);

            targetResult.Duration = targetResult.Duration.Add(duration);

            this.totalDuration = this.totalDuration.Add(duration);

            return this.writer.WriteLineAsync(Format($"{this.palette.Succeeded}{SucceededMessage}{this.palette.Reset}", target, targetInputResult.Input, this.prefix, this.palette, dependencyPath, targetInputResult.Duration));
        }

        // editorconfig-checker-disable
        private static string GetUsageLines(Palette p, string scriptExtension) =>
$@"{p.Default}Usage:{p.Reset}
  {p.Invocation}[invocation]{p.Reset} {p.Option}[options]{p.Reset} {p.Target}[<targets>...]{p.Reset}

{p.Default}Arguments:{p.Reset}
  {p.Target}<targets>{p.Reset}    {p.Default}A list of targets to run or list. If not specified, the {p.Target}""default""{p.Default} target will be run, or all targets will be listed.{p.Reset}

{p.Default}Options:{p.Reset}
  {p.Option}-c{p.Default},{p.Reset} {p.Option}--clear{p.Reset}                {p.Default}Clear the console before execution{p.Reset}
  {p.Option}-n{p.Default},{p.Reset} {p.Option}--dry-run{p.Reset}              {p.Default}Do a dry run without executing actions{p.Reset}
  {p.Option}-d{p.Default},{p.Reset} {p.Option}--list-dependencies{p.Reset}    {p.Default}List all (or specified) targets and dependencies, then exit{p.Reset}
  {p.Option}-i{p.Default},{p.Reset} {p.Option}--list-inputs{p.Reset}          {p.Default}List all (or specified) targets and inputs, then exit{p.Reset}
  {p.Option}-l{p.Default},{p.Reset} {p.Option}--list-targets{p.Reset}         {p.Default}List all (or specified) targets, then exit{p.Reset}
  {p.Option}-t{p.Default},{p.Reset} {p.Option}--list-tree{p.Reset}            {p.Default}List all (or specified) targets and dependency trees, then exit{p.Reset}
  {p.Option}-N{p.Default},{p.Reset} {p.Option}--no-color{p.Reset}             {p.Default}Disable colored output{p.Reset}
  {p.Option}-E{p.Default},{p.Reset} {p.Option}--no-extended-chars{p.Reset}    {p.Default}Disable extended characters{p.Reset}
  {p.Option}-p{p.Default},{p.Reset} {p.Option}--parallel{p.Reset}             {p.Default}Run targets in parallel{p.Reset}
  {p.Option}-s{p.Default},{p.Reset} {p.Option}--skip-dependencies{p.Reset}    {p.Default}Do not run targets' dependencies{p.Reset}
  {p.Option}-v{p.Default},{p.Reset} {p.Option}--verbose{p.Reset}              {p.Default}Enable verbose output{p.Reset}
  {p.Option}--appveyor{p.Reset}                 {p.Default}Force AppVeyor mode (normally auto-detected){p.Reset}
  {p.Option}--azure-pipelines{p.Reset}          {p.Default}Force Azure Pipelines mode (normally auto-detected){p.Reset}
  {p.Option}--console{p.Reset}                  {p.Default}Force console mode (normally auto-detected){p.Reset}
  {p.Option}--github-actions{p.Reset}           {p.Default}Force GitHub Actions mode (normally auto-detected){p.Reset}
  {p.Option}--gitlab-ci{p.Reset}                {p.Default}Force GitLab CI mode (normally auto-detected){p.Reset}
  {p.Option}--teamcity{p.Reset}                 {p.Default}Force TeamCity mode (normally auto-detected){p.Reset}
  {p.Option}--travis{p.Reset}                   {p.Default}Force Travis CI mode (normally auto-detected){p.Reset}
  {p.Option}-?{p.Default},{p.Reset} {p.Option}-h{p.Default},{p.Reset} {p.Option}--help{p.Reset}             {p.Default}Show help and usage information, then exit (case insensitive){p.Reset}

{p.Default}Remarks:{p.Reset}
  {p.Default}The {p.Option}--list-xxx{p.Default} options may be combined.{p.Reset}
  {p.Default}The {p.Invocation}invocation{p.Reset} is typically a call to dotnet run, or the path to a script which wraps a call to dotnet run.

{p.Default}Examples:{p.Reset}
  {p.Invocation}./build.{scriptExtension}{p.Reset}
  {p.Invocation}./build.{scriptExtension}{p.Reset} {p.Option}-d{p.Reset}
  {p.Invocation}./build.{scriptExtension}{p.Reset} {p.Option}-t{p.Reset} {p.Option}-i{p.Reset} {p.Target}default{p.Reset}
  {p.Invocation}./build.{scriptExtension}{p.Reset} {p.Target}test{p.Reset} {p.Target}pack{p.Reset}
  {p.Invocation}dotnet run --project targets --{p.Reset} {p.Option}-n{p.Reset} {p.Target}build{p.Reset}

{p.Default}Targets:{p.Reset}
"; // editorconfig-checker-enable

        private static string GetListLines(TargetCollection targets, IEnumerable<string> rootTargets, int maxDepth, int maxDepthToShowInputs, bool listInputs, string startingPrefix, Palette p)
        {
            var lines = new List<(string, string)>();

            foreach (var rootTarget in rootTargets)
            {
                Append(new List<string> { rootTarget }, new Stack<string>(), true, "", 0);
            }

            var maxColumn1Width = lines.Max(line => Palette.StripColours(line.Item1).Length);

            return string.Join("", lines.Select(line => $"{line.Item1.PadRight(maxColumn1Width + line.Item1.Length - Palette.StripColours(line.Item1).Length)}    {line.Item2}{Environment.NewLine}"));

            void Append(IReadOnlyCollection<string> names, Stack<string> seenTargets, bool isRoot, string previousPrefix, int depth)
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
                            ? startingPrefix
                            : $"{previousPrefix.Replace(p.TreeCorner, "  ", StringComparison.Ordinal).Replace(p.TreeFork, p.TreeDown, StringComparison.Ordinal)}{(item.index == names.Count - 1 ? p.TreeCorner : p.TreeFork)}";

                        var isMissing = !targets.Contains(item.name);

                        var line = $"{prefix}{p.Target}{item.name}";

                        if (isMissing)
                        {
                            lines.Add((line + $"{p.Reset} {p.Failed}(missing){p.Reset}", ""));
                            continue;
                        }

                        if (circularDependency)
                        {
                            lines.Add((line + $"{p.Reset} {p.Failed}(circular dependency){p.Reset}", targets[item.name].Description));
                            continue;
                        }

                        lines.Add((line + p.Reset, targets[item.name].Description));

                        var target = targets[item.name];

                        if (listInputs && depth <= maxDepthToShowInputs && target is IHaveInputs hasInputs)
                        {
                            foreach (var inputItem in hasInputs.Inputs.Select((input, index) => new { input, index }))
                            {
                                var inputPrefix = $"{prefix.Replace(p.TreeCorner, "  ", StringComparison.Ordinal).Replace(p.TreeFork, p.TreeDown, StringComparison.Ordinal)}{(target.Dependencies.Count > 0 && depth + 1 <= maxDepth ? p.TreeDown : "  ")}";

                                lines.Add(($"{inputPrefix}{p.Input}{inputItem.input}{p.Reset}", ""));
                            }
                        }

                        Append(target.Dependencies, seenTargets, false, prefix, depth + 1);
                    }
                    finally
                    {
                        _ = seenTargets.Pop();
                    }
                }
            }
        }
    }
}
