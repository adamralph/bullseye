using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Bullseye.Internal;

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

    private readonly Palette _palette = new(noColor, noExtendedChars, host, osPlatform);
    private readonly Palette _mdPalette = new(noColor: true, noExtendedChars, host, osPlatform);
    private readonly string _scriptExtension = osPlatform == OSPlatform.Windows ? "cmd" : "sh";

    public bool Verbose { get; } = verbose;
    public async Task Header(Func<string> getVersion)
    {
        if (!Verbose)
        {
            return;
        }

        var version = getVersion();

        var builder = new StringBuilder()
            .AppendLine(Format(getPrefix(), "Bullseye version", $"{_palette.Verbose}{version}{_palette.Default}", _palette))
            .AppendLine(Format(getPrefix(), "Host", $"{_palette.Verbose}{host} ({(hostForced ? "forced" : "detected")}){_palette.Default}", _palette))
            .AppendLine(Format(getPrefix(), "OS", $"{_palette.Verbose}{osPlatform.Humanize()}{_palette.Default}", _palette))
            .AppendLine(Format(getPrefix(), "Args", $"{_palette.Verbose}{string.Join(" ", args)}{_palette.Default}", _palette));

        await writer.WriteAsync(builder.ToString()).Tax();
    }

    public async Task Usage(TargetCollection targets)
    {
        var usage = GetUsageLines(_palette, _scriptExtension)
                    + GetListLines(targets, targets.Select(target => target.Name), 0, 0, false, "  ", _palette);

        await writer.WriteAsync(usage).Tax();
    }

    public Task List(TargetCollection targets, IEnumerable<string> rootTargets, int maxDepth, int maxDepthToShowInputs, bool listInputs) =>
        writer.WriteAsync(GetListLines(targets, rootTargets, maxDepth, maxDepthToShowInputs, listInputs, "", _palette));

    public Task Starting(IEnumerable<Target> targets) =>
        writer.WriteLineAsync(Format(getPrefix(), targets, $"{_palette.Text}{StartingMessage}{_palette.Default}", dryRun, parallel, skipDependencies, _palette));

    public async Task Failed(IEnumerable<Target> targets)
    {
        var message = GetResultLines(_results, _totalDuration, getPrefix, _palette)
            + Format(getPrefix(), targets, $"{_palette.Failure}{FailedMessage}{_palette.Default}", dryRun, parallel, skipDependencies, _totalDuration, _palette);

        await writer.WriteLineAsync(message).Tax();

        if (host == Host.GitHubActions && Environment.GetEnvironmentVariable("GITHUB_STEP_SUMMARY") is { } path)
        {
            var stepSummary = GetResultLines(_results, _totalDuration, getPrefix, _mdPalette)
                              + Format(getPrefix(), targets, $"{_mdPalette.Success}{SucceededMessage}{_mdPalette.Default}", dryRun, parallel, skipDependencies, _totalDuration, _mdPalette);

            await WriteGitHubStepSummary("🔴", $"<pre>{stepSummary}</pre>", path).Tax();
        }
    }

    public async Task Succeeded(IEnumerable<Target> targets)
    {
        var message = GetResultLines(_results, _totalDuration, getPrefix, _palette)
            + Format(getPrefix(), targets, $"{_palette.Success}{SucceededMessage}{_palette.Default}", dryRun, parallel, skipDependencies, _totalDuration, _palette);

        await writer.WriteLineAsync(message).Tax();

        if (host == Host.GitHubActions && Environment.GetEnvironmentVariable("GITHUB_STEP_SUMMARY") is { } path)
        {
            var stepSummary = GetResultLines(_results, _totalDuration, getPrefix, _mdPalette)
                          + Format(getPrefix(), targets, $"{_mdPalette.Success}{SucceededMessage}{_mdPalette.Default}", dryRun, parallel, skipDependencies, _totalDuration, _mdPalette);

            await WriteGitHubStepSummary("🟢", $"<pre>{stepSummary}</pre>", path).Tax();
        }
    }

    private async Task WriteGitHubStepSummary(string titleSymbol, string summaryMarkdown, string path)
    {
        var titleText = Assembly.GetEntryAssembly().GetNameOrDefault(out var isDefault);

        if (isDefault)
        {
            await diagnosticsWriter.WriteLineAsync($"{titleText}: Failed to get the entry assembly name. Using default step summary title \"{titleText}\".").Tax();
        }

        var markdown = $"<details><summary><b>{titleSymbol} {titleText}</b></summary><p>{summaryMarkdown}</p></details>";
        await File.AppendAllLinesAsync(path, [markdown]).Tax();
    }

    public async Task Awaiting(Target target, IReadOnlyCollection<Target> dependencyPath)
    {
        if (Verbose)
        {
            await writer.WriteLineAsync(Format(getPrefix(), target, $"{_palette.Verbose}Awaiting{_palette.Default}", dependencyPath, _palette)).Tax();
        }
    }

    public async Task WalkingDependencies(Target target, IReadOnlyCollection<Target> dependencyPath)
    {
        if (Verbose)
        {
            await writer.WriteLineAsync(Format(getPrefix(), target, $"{_palette.Verbose}Walking dependencies{_palette.Default}", dependencyPath, _palette)).Tax();
        }
    }

    public async Task IgnoringNonExistentDependency(Target target, string dependency, IReadOnlyCollection<Target> dependencyPath)
    {
        if (Verbose)
        {
            await writer.WriteLineAsync(Format(getPrefix(), target, $"{_palette.Verbose}Ignoring non-existent dependency:{_palette.Default} {_palette.Target}{dependency}{_palette.Default}", dependencyPath, _palette)).Tax();
        }
    }

    public async Task BeginGroup(Target target)
    {
        if (!parallel && host == Host.GitHubActions)
        {
            await writer.WriteLineAsync($"::group::{_palette.Prefix}{getPrefix()}:{_palette.Default} {_palette.Target}{target}{_palette.Default}").Tax();
        }
    }

    public async Task BeginGroup<TInput>(Target target, TInput input)
    {
        if (!parallel && host == Host.GitHubActions)
        {
            await writer.WriteLineAsync($"::group::{_palette.Prefix}{getPrefix()}:{_palette.Default} {_palette.Target}{target}{_palette.Text}/{_palette.Input}{input}{_palette.Default}").Tax();
        }
    }

    public async Task EndGroup()
    {
        if (!parallel && host == Host.GitHubActions)
        {
            await writer.WriteLineAsync("::endgroup::").Tax();
        }
    }

    public Task Starting(Target target, IReadOnlyCollection<Target> dependencyPath)
    {
        _ = InternResult(target);

        return writer.WriteLineAsync(Format(getPrefix(), target, $"{_palette.Text}{StartingMessage}{_palette.Default}", dependencyPath, _palette));
    }

    public Task Error(Target target, Exception ex) =>
        writer.WriteLineAsync(Format(getPrefix(), target, $"{_palette.Failure}{ex}{_palette.Default}", _palette));

    public Task Failed(Target target, Exception ex, TimeSpan duration, IReadOnlyCollection<Target> dependencyPath)
    {
        var result = InternResult(target);
        result.Outcome = TargetOutcome.Failed;
        result.Duration = result.Duration.Add(duration);

        _totalDuration = _totalDuration.Add(duration);

        return writer.WriteLineAsync(Format(getPrefix(), target, $"{_palette.Failure}{FailedMessage}{_palette.Default} {_palette.Failure}{ex.Message}{_palette.Default}", result.Duration, dependencyPath, _palette));
    }

    public Task Succeeded(Target target, IReadOnlyCollection<Target> dependencyPath, TimeSpan duration)
    {
        var result = InternResult(target);
        result.Outcome = TargetOutcome.Succeeded;
        result.Duration = result.Duration.Add(duration);

        _totalDuration = _totalDuration.Add(duration);

        return writer.WriteLineAsync(Format(getPrefix(), target, $"{_palette.Success}{SucceededMessage}{_palette.Default}", result.Duration, dependencyPath, _palette));
    }

    public Task NoInputs(Target target, IReadOnlyCollection<Target> dependencyPath)
    {
        var result = InternResult(target);
        result.Outcome = TargetOutcome.NoInputs;

        return writer.WriteLineAsync(Format(getPrefix(), target, $"{_palette.Warning}{NoInputsMessage}{_palette.Default}", result.Duration, dependencyPath, _palette));
    }

    public Task Starting<TInput>(Target target, TInput input, Guid inputId, IReadOnlyCollection<Target> dependencyPath)
    {
        var (_, targetInputResult) = Intern(target, inputId);
        targetInputResult.Input = input;

        return writer.WriteLineAsync(Format(getPrefix(), target, targetInputResult.Input, StartingMessage, dependencyPath, _palette));
    }

    public Task Error<TInput>(Target target, TInput input, Exception ex) =>
        writer.WriteLineAsync(Format(getPrefix(), target, input, $"{_palette.Failure}{ex}{_palette.Default}", _palette));

    public Task Failed<TInput>(Target target, TInput input, Guid inputId, Exception ex, TimeSpan duration, IReadOnlyCollection<Target> dependencyPath)
    {
        var (targetResult, targetInputResult) = Intern(target, inputId);

        targetInputResult.Input = input;
        targetInputResult.Outcome = TargetInputOutcome.Failed;
        targetInputResult.Duration = targetInputResult.Duration.Add(duration);

        targetResult.Outcome = TargetOutcome.Failed;
        targetResult.Duration = targetResult.Duration.Add(duration);

        _totalDuration = _totalDuration.Add(duration);

        return writer.WriteLineAsync(Format(getPrefix(), target, targetInputResult.Input, $"{_palette.Failure}{FailedMessage}{_palette.Default} {_palette.Failure}{ex.Message}{_palette.Default}", targetInputResult.Duration, dependencyPath, _palette));
    }

    public Task Succeeded<TInput>(Target target, TInput input, Guid inputId, IReadOnlyCollection<Target> dependencyPath, TimeSpan duration)
    {
        var (targetResult, targetInputResult) = Intern(target, inputId);

        targetInputResult.Input = input;
        targetInputResult.Outcome = TargetInputOutcome.Succeeded;
        targetInputResult.Duration = targetInputResult.Duration.Add(duration);

        targetResult.Duration = targetResult.Duration.Add(duration);

        _totalDuration = _totalDuration.Add(duration);

        return writer.WriteLineAsync(Format(getPrefix(), target, targetInputResult.Input, $"{_palette.Success}{SucceededMessage}{_palette.Default}", targetInputResult.Duration, dependencyPath, _palette));
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
            Append([rootTarget,], new Stack<string>(), true, "", 0);
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
