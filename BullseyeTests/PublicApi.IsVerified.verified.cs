namespace Bullseye
{
    public static class CommandLine
    {
        [return: System.Runtime.CompilerServices.TupleElementNames(new string[] {
                "Targets",
                "Options",
                "UnknownOptions",
                "ShowHelp"})]
        public static ValueTuple<IReadOnlyList<string>, Options, IReadOnlyList<string>, bool> Parse(
                    IEnumerable<string> args) { }
    }
    public enum Host
    {
        AppVeyor = 0,
        Console = 1,
        GitHubActions = 2,
        GitLabCI = 3,
        TeamCity = 4,
        Travis = 5,
        VisualStudioCode = 6,
    }
    public static class HostExtensions
    {
        public static Host DetectIfNull(
                    this Host? host) { }
    }
    public interface IOptions
    {
        bool Clear { get; }
        bool DryRun { get; }
        Host? Host { get; }
        bool ListDependencies { get; }
        bool ListInputs { get; }
        bool ListTargets { get; }
        bool ListTree { get; }
        bool NoColor { get; }
        bool NoExtendedChars { get; }
        bool Parallel { get; }
        bool SkipDependencies { get; }
        bool Verbose { get; }
    }
    public class InvalidUsageException : Exception
    {
        public InvalidUsageException() { }
        public InvalidUsageException(
                    string message) { }
        public InvalidUsageException(
                    string message,
                    Exception innerException) { }
    }
    public class Options : IOptions
    {
        public Options() { }
        public Options(
                    [System.Runtime.CompilerServices.TupleElementNames(new string[] {
                            "Name",
                            "Value"})] IEnumerable<ValueTuple<string, bool>> values) { }
        public bool Clear { get; set; }
        public bool DryRun { get; set; }
        public Host? Host { get; set; }
        public bool ListDependencies { get; set; }
        public bool ListInputs { get; set; }
        public bool ListTargets { get; set; }
        public bool ListTree { get; set; }
        public bool NoColor { get; set; }
        public bool NoExtendedChars { get; set; }
        public bool Parallel { get; set; }
        public bool SkipDependencies { get; set; }
        public bool Verbose { get; set; }
        [System.Runtime.CompilerServices.TupleElementNames(new string[] {
                "Aliases",
                "Description"})]
        public static IReadOnlyList<ValueTuple<IReadOnlyList<string>, string>> Definitions { get; }
    }
    public class Palette
    {
        public Palette(
                    bool noColor,
                    bool noExtendedChars,
                    Host host,
                    System.Runtime.InteropServices.OSPlatform osPlatform) { }
        public string Default { get; }
        public string Failure { get; }
        public char Horizontal { get; }
        public string Input { get; }
        public string Invocation { get; }
        public string Option { get; }
        public string Prefix { get; }
        public string Success { get; }
        public string Target { get; }
        public string Text { get; }
        public string Timing { get; }
        public string TreeCorner { get; }
        public string TreeFork { get; }
        public string TreeLine { get; }
        public string Verbose { get; }
        public string Warning { get; }
        public static string StripColors(
                    string text) { }
    }
    public class TargetFailedException : Exception
    {
        public TargetFailedException() { }
        public TargetFailedException(
                    string message) { }
        public TargetFailedException(
                    string message,
                    Exception innerException) { }
    }
    public class Targets
    {
        public Targets() { }
        public void Add(
                    string name,
                    Action action) { }
        public void Add(
                    string name,
                    IEnumerable<string> dependsOn) { }
        public void Add(
                    string name,
                    Func<Task> action) { }
        public void Add(
                    string name,
                    IEnumerable<string> dependsOn,
                    Action action) { }
        public void Add(
                    string name,
                    IEnumerable<string> dependsOn,
                    Func<Task> action) { }
        public void Add(
                    string name,
                    string description,
                    Action action) { }
        public void Add(
                    string name,
                    string description,
                    IEnumerable<string> dependsOn) { }
        public void Add(
                    string name,
                    string description,
                    Func<Task> action) { }
        public void Add(
                    string name,
                    string description,
                    IEnumerable<string> dependsOn,
                    Action action) { }
        public void Add(
                    string name,
                    string description,
                    IEnumerable<string> dependsOn,
                    Func<Task> action) { }
        public void Add<TInput>(
                    string name,
                    IEnumerable<TInput> forEach,
                    Action<TInput> action) { }
        public void Add<TInput>(
                    string name,
                    IEnumerable<TInput> forEach,
                    Func<TInput, Task> action) { }
        public void Add<TInput>(
                    string name,
                    IEnumerable<string> dependsOn,
                    IEnumerable<TInput> forEach,
                    Action<TInput> action) { }
        public void Add<TInput>(
                    string name,
                    IEnumerable<string> dependsOn,
                    IEnumerable<TInput> forEach,
                    Func<TInput, Task> action) { }
        public void Add<TInput>(
                    string name,
                    string description,
                    IEnumerable<TInput> forEach,
                    Action<TInput> action) { }
        public void Add<TInput>(
                    string name,
                    string description,
                    IEnumerable<TInput> forEach,
                    Func<TInput, Task> action) { }
        public void Add<TInput>(
                    string name,
                    string description,
                    IEnumerable<string> dependsOn,
                    IEnumerable<TInput> forEach,
                    Action<TInput> action) { }
        public void Add<TInput>(
                    string name,
                    string description,
                    IEnumerable<string> dependsOn,
                    IEnumerable<TInput> forEach,
                    Func<TInput, Task> action) { }
        public Task RunAndExitAsync(
                    IEnumerable<string> args,
                    Func<Exception, bool>? messageOnly = null,
                    Func<string>? getMessagePrefix = null,
                    TextWriter? outputWriter = null,
                    TextWriter? diagnosticsWriter = null) { }
        public Task RunAndExitAsync(
                    IEnumerable<string> targets,
                    IOptions options,
                    IEnumerable<string>? unknownOptions = null,
                    bool showHelp = false,
                    Func<Exception, bool>? messageOnly = null,
                    Func<string>? getMessagePrefix = null,
                    TextWriter? outputWriter = null,
                    TextWriter? diagnosticsWriter = null) { }
        public Task RunWithoutExitingAsync(
                    IEnumerable<string> args,
                    Func<Exception, bool>? messageOnly = null,
                    Func<string>? getMessagePrefix = null,
                    TextWriter? outputWriter = null,
                    TextWriter? diagnosticsWriter = null) { }
        public Task RunWithoutExitingAsync(
                    IEnumerable<string> targets,
                    IOptions options,
                    IEnumerable<string>? unknownOptions = null,
                    bool showHelp = false,
                    Func<Exception, bool>? messageOnly = null,
                    Func<string>? getMessagePrefix = null,
                    TextWriter? outputWriter = null,
                    TextWriter? diagnosticsWriter = null) { }
        public static Task RunTargetsAndExitAsync(
                    IEnumerable<string> args,
                    Func<Exception, bool>? messageOnly = null,
                    Func<string>? getMessagePrefix = null,
                    TextWriter? outputWriter = null,
                    TextWriter? diagnosticsWriter = null) { }
        public static Task RunTargetsAndExitAsync(
                    IEnumerable<string> targets,
                    IOptions options,
                    IEnumerable<string>? unknownOptions = null,
                    bool showHelp = false,
                    Func<Exception, bool>? messageOnly = null,
                    Func<string>? getMessagePrefix = null,
                    TextWriter? outputWriter = null,
                    TextWriter? diagnosticsWriter = null) { }
        public static Task RunTargetsWithoutExitingAsync(
                    IEnumerable<string> args,
                    Func<Exception, bool>? messageOnly = null,
                    Func<string>? getMessagePrefix = null,
                    TextWriter? outputWriter = null,
                    TextWriter? diagnosticsWriter = null) { }
        public static Task RunTargetsWithoutExitingAsync(
                    IEnumerable<string> targets,
                    IOptions options,
                    IEnumerable<string>? unknownOptions = null,
                    bool showHelp = false,
                    Func<Exception, bool>? messageOnly = null,
                    Func<string>? getMessagePrefix = null,
                    TextWriter? outputWriter = null,
                    TextWriter? diagnosticsWriter = null) { }
        public static void Target(
                    string name,
                    Action action) { }
        public static void Target(
                    string name,
                    IEnumerable<string> dependsOn) { }
        public static void Target(
                    string name,
                    Func<Task> action) { }
        public static void Target(
                    string name,
                    IEnumerable<string> dependsOn,
                    Action action) { }
        public static void Target(
                    string name,
                    IEnumerable<string> dependsOn,
                    Func<Task> action) { }
        public static void Target(
                    string name,
                    string description,
                    Action action) { }
        public static void Target(
                    string name,
                    string description,
                    IEnumerable<string> dependsOn) { }
        public static void Target(
                    string name,
                    string description,
                    Func<Task> action) { }
        public static void Target(
                    string name,
                    string description,
                    IEnumerable<string> dependsOn,
                    Action action) { }
        public static void Target(
                    string name,
                    string description,
                    IEnumerable<string> dependsOn,
                    Func<Task> action) { }
        public static void Target<TInput>(
                    string name,
                    IEnumerable<TInput> forEach,
                    Action<TInput> action) { }
        public static void Target<TInput>(
                    string name,
                    IEnumerable<TInput> forEach,
                    Func<TInput, Task> action) { }
        public static void Target<TInput>(
                    string name,
                    IEnumerable<string> dependsOn,
                    IEnumerable<TInput> forEach,
                    Action<TInput> action) { }
        public static void Target<TInput>(
                    string name,
                    IEnumerable<string> dependsOn,
                    IEnumerable<TInput> forEach,
                    Func<TInput, Task> action) { }
        public static void Target<TInput>(
                    string name,
                    string description,
                    IEnumerable<TInput> forEach,
                    Action<TInput> action) { }
        public static void Target<TInput>(
                    string name,
                    string description,
                    IEnumerable<TInput> forEach,
                    Func<TInput, Task> action) { }
        public static void Target<TInput>(
                    string name,
                    string description,
                    IEnumerable<string> dependsOn,
                    IEnumerable<TInput> forEach,
                    Action<TInput> action) { }
        public static void Target<TInput>(
                    string name,
                    string description,
                    IEnumerable<string> dependsOn,
                    IEnumerable<TInput> forEach,
                    Func<TInput, Task> action) { }
    }
}
namespace Bullseye.Internal
{
    public static class ActionExtensions
    {
        public static Func<Task> ToAsync(
                    this Action action) { }
        public static Func<T, Task> ToAsync<T>(
                    this Action<T> action) { }
    }
    public class ActionTarget : Target
    {
        public ActionTarget(
                    string name,
                    string description,
                    IReadOnlyCollection<string> dependencies,
                    Func<Task> action) { }
        public override Task RunAsync(
                    bool dryRun,
                    SemaphoreSlim parallelTargets,
                    Output output,
                    Func<Exception, bool> messageOnly,
                    IReadOnlyCollection<Target> dependencyPath) { }
    }
    public class ActionTarget<TInput> : Target, IHaveInputs
    {
        public ActionTarget(
                    string name,
                    string description,
                    IReadOnlyCollection<string> dependencies,
                    IEnumerable<TInput> inputs,
                    Func<TInput, Task> action) { }
        public IEnumerable<object?> Inputs { get; }
        public override Task RunAsync(
                    bool dryRun,
                    SemaphoreSlim parallelTargets,
                    Output output,
                    Func<Exception, bool> messageOnly,
                    IReadOnlyCollection<Target> dependencyPath) { }
    }
    public static class ArgsParser
    {
        [return: System.Runtime.CompilerServices.TupleElementNames(new string[] {
                "Targets",
                "Options",
                "UnknownOptions",
                "showHelp"})]
        public static ValueTuple<IReadOnlyList<string>, Bullseye.Options, IReadOnlyList<string>, bool> Parse(
                    IReadOnlyCollection<string> args) { }
    }
    public static class AssemblyExtensions
    {
        public static string GetVersion(
                    this System.Reflection.Assembly assembly) { }
    }
    public interface IAsyncDisposable
    {
        Task DisposeAsync();
    }
    public interface IHaveInputs
    {
        IEnumerable<object?> Inputs { get; }
    }
    public static class OSPlatformExtensions
    {
        public static string Humanize(
                    this System.Runtime.InteropServices.OSPlatform osPlatform) { }
    }
    public static class OptionsReader
    {
        [return: System.Runtime.CompilerServices.TupleElementNames(new string[] {
                "Clear",
                "DryRun",
                "ListDependencies",
                "ListInputs",
                "ListTargets",
                "ListTree",
                "NoColor",
                "NoExtendedChars",
                "Parallel",
                "SkipDependencies",
                "Verbose",
                "Host",
                "UnknownOptions",
                null,
                null,
                null,
                null,
                null,
                null})]
        public static ValueTuple<bool, bool, bool, bool, bool, bool, bool, ValueTuple<bool, bool, bool, bool, Bullseye.Host?, IReadOnlyList<string>>> Read(
                    IEnumerable<string> options) { }
    }
    public class Output
    {
        public Output(
                    TextWriter writer,
                    TextWriter diagnosticsWriter,
                    IReadOnlyCollection<string> args,
                    bool dryRun,
                    Bullseye.Host host,
                    bool hostForced,
                    bool noColor,
                    bool noExtendedChars,
                    System.Runtime.InteropServices.OSPlatform osPlatform,
                    bool parallel,
                    Func<string> getPrefix,
                    bool skipDependencies,
                    bool verbose) { }
        public bool Verbose { get; }
        public Task Awaiting(
                    Target target,
                    IReadOnlyCollection<Target> dependencyPath) { }
        public Task BeginGroup(
                    Target target) { }
        public Task BeginGroup<TInput>(
                    Target target,
                    TInput input) { }
        public Task EndGroup() { }
        public Task Error(
                    Target target,
                    Exception ex) { }
        public Task Error<TInput>(
                    Target target,
                    TInput input,
                    Exception ex) { }
        public Task Failed(
                    IEnumerable<Target> targets) { }
        public Task Failed(
                    Target target,
                    Exception ex,
                    TimeSpan duration,
                    IReadOnlyCollection<Target> dependencyPath) { }
        public Task Failed<TInput>(
                    Target target,
                    TInput input,
                    Guid inputId,
                    Exception ex,
                    TimeSpan duration,
                    IReadOnlyCollection<Target> dependencyPath) { }
        public Task Header(
                    Func<string> getVersion) { }
        public Task IgnoringNonExistentDependency(
                    Target target,
                    string dependency,
                    IReadOnlyCollection<Target> dependencyPath) { }
        public Task<IAsyncDisposable> Initialize() { }
        public Task List(
                    TargetCollection targets,
                    IEnumerable<string> rootTargets,
                    int maxDepth,
                    int maxDepthToShowInputs,
                    bool listInputs) { }
        public Task NoInputs(
                    Target target,
                    IReadOnlyCollection<Target> dependencyPath) { }
        public Task Starting(
                    IEnumerable<Target> targets) { }
        public Task Starting(
                    Target target,
                    IReadOnlyCollection<Target> dependencyPath) { }
        public Task Starting<TInput>(
                    Target target,
                    TInput input,
                    Guid inputId,
                    IReadOnlyCollection<Target> dependencyPath) { }
        public Task Succeeded(
                    IEnumerable<Target> targets) { }
        public Task Succeeded(
                    Target target,
                    IReadOnlyCollection<Target> dependencyPath,
                    TimeSpan duration) { }
        public Task Succeeded<TInput>(
                    Target target,
                    TInput input,
                    Guid inputId,
                    IReadOnlyCollection<Target> dependencyPath,
                    TimeSpan duration) { }
        public Task Usage(
                    TargetCollection targets) { }
        public Task WalkingDependencies(
                    Target target,
                    IReadOnlyCollection<Target> dependencyPath) { }
    }
    public static class StringExtensions
    {
        public static string Spaced(
                    this IEnumerable<string> strings) { }
    }
    public class Target
    {
        public Target(
                    string name,
                    string description,
                    IReadOnlyCollection<string> dependencies) { }
        public IReadOnlyCollection<string> Dependencies { get; }
        public string Description { get; }
        public string Name { get; }
        public virtual Task RunAsync(
                    bool dryRun,
                    SemaphoreSlim parallelTargets,
                    Output output,
                    Func<Exception, bool> messageOnly,
                    IReadOnlyCollection<Target> dependencyPath) { }
        public override string ToString() { }
    }
    public class TargetCollection : System.Collections.ObjectModel.KeyedCollection<string, Target>
    {
        public TargetCollection() { }
        protected override string GetKeyForItem(
                    Target item) { }
        public Task RunAsync(
                    IReadOnlyCollection<string> names,
                    bool dryRun,
                    bool parallel,
                    bool skipDependencies,
                    Func<Exception, bool> messageOnly,
                    Output output) { }
    }
    public static class TargetCollectionExtensions
    {
        public static Task RunAsync(
                    this TargetCollection targets,
                    IReadOnlyCollection<string> args,
                    Func<Exception, bool> messageOnly,
                    Func<string> getMessagePrefix,
                    TextWriter outputWriter,
                    TextWriter diagnosticsWriter,
                    bool exit) { }
        public static Task RunAsync(
                    this TargetCollection targets,
                    IReadOnlyCollection<string> names,
                    Bullseye.IOptions options,
                    IReadOnlyCollection<string> unknownOptions,
                    bool showHelp,
                    Func<Exception, bool> messageOnly,
                    Func<string> getMessagePrefix,
                    TextWriter outputWriter,
                    TextWriter diagnosticsWriter,
                    bool exit) { }
    }
    public static class TaskExtensions
    {
        public static bool IsAwaitable(
                    this Task task) { }
        public static System.Runtime.CompilerServices.ConfiguredTaskAwaitable Tax(
                    this Task task) { }
        public static System.Runtime.CompilerServices.ConfiguredTaskAwaitable<TResult> Tax<TResult>(
                    this Task<TResult> task) { }
    }
    public static class TimeSpanExtensions
    {
        public static string Humanize(
                    this TimeSpan duration) { }
    }
}
