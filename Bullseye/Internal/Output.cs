using System.Runtime.InteropServices;
using System.Text;

namespace Bullseye.Internal;

#if NET8_0_OR_GREATER
public partial class Output(
    TextWriter writer,
    TextWriter diagnosticsWriter,
    IReadOnlyCollection<string> args,
    bool dryRun,
    Host host,
    bool hostForced,
    bool noColor,
    bool noExtendedChars,
    OSPlatform osPlatform,
    bool parallel,
    Func<string> getPrefix,
    bool skipDependencies,
    bool verbose)
{
    private const string NoInputsMessage = "No inputs!";
    private const string StartingMessage = "Starting...";
    private const string FailedMessage = "FAILED!";
    private const string SucceededMessage = "Succeeded";

    private readonly TextWriter writer = writer;
    private readonly TextWriter diagnosticsWriter = diagnosticsWriter;

    private readonly IReadOnlyCollection<string> args = args;
    private readonly bool dryRun = dryRun;
    private readonly Host host = host;
    private readonly bool hostForced = hostForced;
    private readonly bool noColor = noColor;
    private readonly OSPlatform osPlatform = osPlatform;
    private readonly bool parallel = parallel;
    private readonly Func<string> getPrefix = getPrefix;
    private readonly bool skipDependencies = skipDependencies;

    private readonly Palette palette = new(noColor, noExtendedChars, host, osPlatform);
    private readonly string scriptExtension = osPlatform == OSPlatform.Windows ? "cmd" : "sh";

    public bool Verbose { get; } = verbose;
#else
public partial class Output
{
    private const string NoInputsMessage = "No inputs!";
    private const string StartingMessage = "Starting...";
    private const string FailedMessage = "FAILED!";
    private const string SucceededMessage = "Succeeded";

    private readonly TextWriter writer;
    private readonly TextWriter diagnosticsWriter;

    private readonly IReadOnlyCollection<string> args;
    private readonly bool dryRun;
    private readonly Host host;
    private readonly bool hostForced;
    private readonly bool noColor;
    private readonly OSPlatform osPlatform;
    private readonly bool parallel;
    private readonly Func<string> getPrefix;
    private readonly bool skipDependencies;

    private readonly Palette palette;
    private readonly string scriptExtension;

    public bool Verbose { get; }

    public Output(
        TextWriter writer,
        TextWriter diagnosticsWriter,
        IReadOnlyCollection<string> args,
        bool dryRun,
        Host host,
        bool hostForced,
        bool noColor,
        bool noExtendedChars,
        OSPlatform osPlatform,
        bool parallel,
        Func<string> getPrefix,
        bool skipDependencies,
        bool verbose)
    {
        this.writer = writer;
        this.diagnosticsWriter = diagnosticsWriter;

        this.args = args;
        this.dryRun = dryRun;
        this.host = host;
        this.hostForced = hostForced;
        this.noColor = noColor;
        this.osPlatform = osPlatform;
        this.parallel = parallel;
        this.getPrefix = getPrefix;
        this.skipDependencies = skipDependencies;
        this.Verbose = verbose;

        this.palette = new Palette(noColor, noExtendedChars, host, osPlatform);
        this.scriptExtension = osPlatform == OSPlatform.Windows ? "cmd" : "sh";
    }
#endif
    public async Task Header(Func<string> getVersion)
    {
        if (!this.Verbose)
        {
            return;
        }

        var version = getVersion();

        var builder = new StringBuilder()
            .AppendLine(Format(this.getPrefix(), "Bullseye version", $"{this.palette.Verbose}{version}{this.palette.Default}", this.palette))
            .AppendLine(Format(this.getPrefix(), "Host", $"{this.palette.Verbose}{this.host} ({(this.hostForced ? "forced" : "detected")}){this.palette.Default}", this.palette))
            .AppendLine(Format(this.getPrefix(), "OS", $"{this.palette.Verbose}{this.osPlatform.Humanize()}{this.palette.Default}", this.palette))
            .AppendLine(Format(this.getPrefix(), "Args", $"{this.palette.Verbose}{string.Join(" ", this.args)}{this.palette.Default}", this.palette));

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
        this.writer.WriteLineAsync(Format(this.getPrefix(), targets, $"{this.palette.Text}{StartingMessage}{this.palette.Default}", this.dryRun, this.parallel, this.skipDependencies, this.palette));

    public async Task Failed(IEnumerable<Target> targets)
    {
        var message = GetResultLines(this.results, this.totalDuration, this.getPrefix, this.palette)
            + Format(this.getPrefix(), targets, $"{this.palette.Failure}{FailedMessage}{this.palette.Default}", this.dryRun, this.parallel, this.skipDependencies, this.totalDuration, this.palette);

        await this.writer.WriteLineAsync(message).Tax();
    }

    public async Task Succeeded(IEnumerable<Target> targets)
    {
        var message = GetResultLines(this.results, this.totalDuration, this.getPrefix, this.palette)
            + Format(this.getPrefix(), targets, $"{this.palette.Success}{SucceededMessage}{this.palette.Default}", this.dryRun, this.parallel, this.skipDependencies, this.totalDuration, this.palette);

        await this.writer.WriteLineAsync(message).Tax();
    }

    public async Task Awaiting(Target target, IReadOnlyCollection<Target> dependencyPath)
    {
        if (this.Verbose)
        {
            await this.writer.WriteLineAsync(Format(this.getPrefix(), target, $"{this.palette.Verbose}Awaiting{this.palette.Default}", dependencyPath, this.palette)).Tax();
        }
    }

    public async Task WalkingDependencies(Target target, IReadOnlyCollection<Target> dependencyPath)
    {
        if (this.Verbose)
        {
            await this.writer.WriteLineAsync(Format(this.getPrefix(), target, $"{this.palette.Verbose}Walking dependencies{this.palette.Default}", dependencyPath, this.palette)).Tax();
        }
    }

    public async Task IgnoringNonExistentDependency(Target target, string dependency, IReadOnlyCollection<Target> dependencyPath)
    {
        if (this.Verbose)
        {
            await this.writer.WriteLineAsync(Format(this.getPrefix(), target, $"{this.palette.Verbose}Ignoring non-existent dependency:{this.palette.Default} {this.palette.Target}{dependency}{this.palette.Default}", dependencyPath, this.palette)).Tax();
        }
    }

    public async Task BeginGroup(Target target)
    {
        if (!this.parallel && this.host == Host.GitHubActions)
        {
            await this.writer.WriteLineAsync($"::group::{this.palette.Prefix}{this.getPrefix()}:{this.palette.Default} {this.palette.Target}{target}{this.palette.Default}").Tax();
        }
    }

    public async Task BeginGroup<TInput>(Target target, TInput input)
    {
        if (!this.parallel && this.host == Host.GitHubActions)
        {
            await this.writer.WriteLineAsync($"::group::{this.palette.Prefix}{this.getPrefix()}:{this.palette.Default} {this.palette.Target}{target}{this.palette.Text}/{this.palette.Input}{input}{this.palette.Default}").Tax();
        }
    }

    public async Task EndGroup()
    {
        if (!this.parallel && this.host == Host.GitHubActions)
        {
            await this.writer.WriteLineAsync("::endgroup::").Tax();
        }
    }

    public Task Starting(Target target, IReadOnlyCollection<Target> dependencyPath)
    {
        _ = this.InternResult(target);

        return this.writer.WriteLineAsync(Format(this.getPrefix(), target, $"{this.palette.Text}{StartingMessage}{this.palette.Default}", dependencyPath, this.palette));
    }

    public Task Error(Target target, Exception ex) =>
        this.writer.WriteLineAsync(Format(this.getPrefix(), target, $"{this.palette.Failure}{ex}{this.palette.Default}", this.palette));

    public Task Failed(Target target, Exception ex, TimeSpan duration, IReadOnlyCollection<Target> dependencyPath)
    {
        var result = this.InternResult(target);
        result.Outcome = TargetOutcome.Failed;
        result.Duration = result.Duration.Add(duration);

        this.totalDuration = this.totalDuration.Add(duration);

        return this.writer.WriteLineAsync(Format(this.getPrefix(), target, $"{this.palette.Failure}{FailedMessage}{this.palette.Default} {this.palette.Failure}{ex.Message}{this.palette.Default}", result.Duration, dependencyPath, this.palette));
    }

    public Task Succeeded(Target target, IReadOnlyCollection<Target> dependencyPath, TimeSpan duration)
    {
        var result = this.InternResult(target);
        result.Outcome = TargetOutcome.Succeeded;
        result.Duration = result.Duration.Add(duration);

        this.totalDuration = this.totalDuration.Add(duration);

        return this.writer.WriteLineAsync(Format(this.getPrefix(), target, $"{this.palette.Success}{SucceededMessage}{this.palette.Default}", result.Duration, dependencyPath, this.palette));
    }

    public Task NoInputs(Target target, IReadOnlyCollection<Target> dependencyPath)
    {
        var result = this.InternResult(target);
        result.Outcome = TargetOutcome.NoInputs;

        return this.writer.WriteLineAsync(Format(this.getPrefix(), target, $"{this.palette.Warning}{NoInputsMessage}{this.palette.Default}", result.Duration, dependencyPath, this.palette));
    }

    public Task Starting<TInput>(Target target, TInput input, Guid inputId, IReadOnlyCollection<Target> dependencyPath)
    {
        var (_, targetInputResult) = this.Intern(target, inputId);
        targetInputResult.Input = input;

        return this.writer.WriteLineAsync(Format(this.getPrefix(), target, targetInputResult.Input, StartingMessage, dependencyPath, this.palette));
    }

    public Task Error<TInput>(Target target, TInput input, Exception ex) =>
        this.writer.WriteLineAsync(Format(this.getPrefix(), target, input, $"{this.palette.Failure}{ex}{this.palette.Default}", this.palette));

    public Task Failed<TInput>(Target target, TInput input, Guid inputId, Exception ex, TimeSpan duration, IReadOnlyCollection<Target> dependencyPath)
    {
        var (targetResult, targetInputResult) = this.Intern(target, inputId);

        targetInputResult.Input = input;
        targetInputResult.Outcome = TargetInputOutcome.Failed;
        targetInputResult.Duration = targetInputResult.Duration.Add(duration);

        targetResult.Outcome = TargetOutcome.Failed;
        targetResult.Duration = targetResult.Duration.Add(duration);

        this.totalDuration = this.totalDuration.Add(duration);

        return this.writer.WriteLineAsync(Format(this.getPrefix(), target, targetInputResult.Input, $"{this.palette.Failure}{FailedMessage}{this.palette.Default} {this.palette.Failure}{ex.Message}{this.palette.Default}", targetInputResult.Duration, dependencyPath, this.palette));
    }

    public Task Succeeded<TInput>(Target target, TInput input, Guid inputId, IReadOnlyCollection<Target> dependencyPath, TimeSpan duration)
    {
        var (targetResult, targetInputResult) = this.Intern(target, inputId);

        targetInputResult.Input = input;
        targetInputResult.Outcome = TargetInputOutcome.Succeeded;
        targetInputResult.Duration = targetInputResult.Duration.Add(duration);

        targetResult.Duration = targetResult.Duration.Add(duration);

        this.totalDuration = this.totalDuration.Add(duration);

        return this.writer.WriteLineAsync(Format(this.getPrefix(), target, targetInputResult.Input, $"{this.palette.Success}{SucceededMessage}{this.palette.Default}", targetInputResult.Duration, dependencyPath, this.palette));
    }

    // editorconfig-checker-disable
    private static string GetUsageLines(Palette p, string scriptExtension) =>
        $@"{p.Text}Usage:{p.Default}
  {p.Invocation}[invocation]{p.Default} {p.Option}[options]{p.Default} {p.Target}[<targets>...]{p.Default}

{p.Text}Arguments:{p.Default}
  {p.Target}<targets>{p.Default}    {p.Text}A list of targets to run or list. If not specified, the {p.Target}""default""{p.Text} target will be run, or all targets will be listed. Target names may be abbreviated. For example, {p.Target}""b""{p.Text} for {p.Target}""build""{p.Text}.{p.Default}

{p.Text}Options:{p.Default}
  {p.Option}-c{p.Text},{p.Default} {p.Option}--clear{p.Default}                {p.Text}Clear the console before execution{p.Default}
  {p.Option}-n{p.Text},{p.Default} {p.Option}--dry-run{p.Default}              {p.Text}Do a dry run without executing actions{p.Default}
  {p.Option}-d{p.Text},{p.Default} {p.Option}--list-dependencies{p.Default}    {p.Text}List all (or specified) targets and dependencies, then exit{p.Default}
  {p.Option}-i{p.Text},{p.Default} {p.Option}--list-inputs{p.Default}          {p.Text}List all (or specified) targets and inputs, then exit{p.Default}
  {p.Option}-l{p.Text},{p.Default} {p.Option}--list-targets{p.Default}         {p.Text}List all (or specified) targets, then exit{p.Default}
  {p.Option}-t{p.Text},{p.Default} {p.Option}--list-tree{p.Default}            {p.Text}List all (or specified) targets and dependency trees, then exit{p.Default}
  {p.Option}-N{p.Text},{p.Default} {p.Option}--no-color{p.Default}             {p.Text}Disable colored output{p.Default}
  {p.Option}-E{p.Text},{p.Default} {p.Option}--no-extended-chars{p.Default}    {p.Text}Disable extended characters{p.Default}
  {p.Option}-p{p.Text},{p.Default} {p.Option}--parallel{p.Default}             {p.Text}Run targets in parallel{p.Default}
  {p.Option}-s{p.Text},{p.Default} {p.Option}--skip-dependencies{p.Default}    {p.Text}Do not run targets' dependencies{p.Default}
  {p.Option}-v{p.Text},{p.Default} {p.Option}--verbose{p.Default}              {p.Text}Enable verbose output{p.Default}
  {p.Option}--appveyor{p.Default}                 {p.Text}Force AppVeyor mode (normally auto-detected){p.Default}
  {p.Option}--console{p.Default}                  {p.Text}Force console mode (normally auto-detected){p.Default}
  {p.Option}--github-actions{p.Default}           {p.Text}Force GitHub Actions mode (normally auto-detected){p.Default}
  {p.Option}--gitlab-ci{p.Default}                {p.Text}Force GitLab CI mode (normally auto-detected){p.Default}
  {p.Option}--teamcity{p.Default}                 {p.Text}Force TeamCity mode (normally auto-detected){p.Default}
  {p.Option}--travis{p.Default}                   {p.Text}Force Travis CI mode (normally auto-detected){p.Default}
  {p.Option}-?{p.Text},{p.Default} {p.Option}-h{p.Text},{p.Default} {p.Option}--help{p.Default}             {p.Text}Show help and usage information, then exit (case insensitive){p.Default}

{p.Text}Remarks:{p.Default}
  {p.Text}The {p.Option}--list-xxx{p.Text} options may be combined.{p.Default}
  {p.Text}The {p.Invocation}invocation{p.Text} is typically a call to dotnet run, or the path to a script which wraps a call to dotnet run.{p.Default}

{p.Text}Examples:{p.Default}
  {p.Invocation}./build.{scriptExtension}{p.Default}
  {p.Invocation}./build.{scriptExtension}{p.Default} {p.Option}-d{p.Default}
  {p.Invocation}./build.{scriptExtension}{p.Default} {p.Option}-t{p.Default} {p.Option}-i{p.Default} {p.Target}default{p.Default}
  {p.Invocation}./build.{scriptExtension}{p.Default} {p.Target}test{p.Default} {p.Target}pack{p.Default}
  {p.Invocation}dotnet run --project targets --{p.Default} {p.Option}-n{p.Default} {p.Target}build{p.Default}

{p.Text}Targets:{p.Default}
"; // editorconfig-checker-enable

    private static string GetListLines(TargetCollection targets, IEnumerable<string> rootTargets, int maxDepth, int maxDepthToShowInputs, bool listInputs, string startingPrefix, Palette p)
    {
        var lines = new List<(string, string)>();

        foreach (var rootTarget in rootTargets)
        {
            Append(new List<string> { rootTarget, }, new Stack<string>(), true, "", 0);
        }

        var maxColumn1Width = lines.Max(line => Palette.StripColors(line.Item1).Length);

        return string.Join("", lines.Select(line => $"{line.Item1.PadRight(maxColumn1Width + line.Item1.Length - Palette.StripColors(line.Item1).Length)}    {line.Item2}{Environment.NewLine}"));

        void Append(IReadOnlyCollection<string> names, Stack<string> seenTargets, bool isRoot, string previousPrefix, int depth)
        {
            if (depth > maxDepth)
            {
                return;
            }

            foreach (var item in names.Select((name, index) => new { name, index, }))
            {
                var circularDependency = seenTargets.Contains(item.name);

                seenTargets.Push(item.name);

                try
                {
                    var prefix = isRoot
                        ? startingPrefix
                        : $"{previousPrefix.Replace(p.TreeCorner, "  ", StringComparison.Ordinal).Replace(p.TreeFork, p.TreeLine, StringComparison.Ordinal)}{(item.index == names.Count - 1 ? p.TreeCorner : p.TreeFork)}";

                    var isMissing = !targets.Contains(item.name);

                    var line = $"{prefix}{p.Target}{item.name}";

                    if (isMissing)
                    {
                        lines.Add((line + $"{p.Default} {p.Failure}(missing){p.Default}", ""));
                        continue;
                    }

                    if (circularDependency)
                    {
                        lines.Add((line + $"{p.Default} {p.Failure}(circular dependency){p.Default}", targets[item.name].Description));
                        continue;
                    }

                    lines.Add((line + p.Default, targets[item.name].Description));

                    var target = targets[item.name];

                    if (listInputs && depth <= maxDepthToShowInputs && target is IHaveInputs hasInputs)
                    {
                        foreach (var inputItem in hasInputs.Inputs.Select((input, index) => new { input, index, }))
                        {
                            var inputPrefix = $"{prefix.Replace(p.TreeCorner, "  ", StringComparison.Ordinal).Replace(p.TreeFork, p.TreeLine, StringComparison.Ordinal)}{(target.Dependencies.Count > 0 && depth + 1 <= maxDepth ? p.TreeLine : "  ")}";

                            lines.Add(($"{inputPrefix}{p.Input}{inputItem.input}{p.Default}", ""));
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
