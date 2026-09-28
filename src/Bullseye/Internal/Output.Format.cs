namespace Bullseye.Internal;

public partial class Output
{
    private static string Format(
        string prefix,
        IEnumerable<Target> targets,
        string message,
        bool dryRun,
        bool parallel,
        bool skipDependencies,
        Palette p) =>
        $"{p.Prefix}{prefix}:{p.Default} {message} {p.Target}({targets.Select(target => target.Name).Spaced()}){p.Default}{(dryRun ? $" {p.Option}(dry run){p.Default}" : "")}{(parallel ? $" {p.Option}(parallel){p.Default}" : "")}{(skipDependencies ? $" {p.Option}(skip dependencies){p.Default}" : "")}";

    private static string Format(
        string prefix,
        IEnumerable<Target> targets,
        string message,
        bool dryRun,
        bool parallel,
        bool skipDependencies,
        TimeSpan duration,
        Palette p) =>
        $"{p.Prefix}{prefix}:{p.Default} {message} {p.Target}({targets.Select(target => target.Name).Spaced()}){p.Default}{(dryRun ? $" {p.Option}(dry run){p.Default}" : "")}{(parallel ? $" {p.Option}(parallel){p.Default}" : "")}{(skipDependencies ? $" {p.Option}(skip dependencies){p.Default}" : "")} {p.Timing}({duration.Humanize()}){p.Default}";

    private static string Format(
        string prefix,
        Target target,
        string message,
        Palette p) =>
        $"{p.Prefix}{prefix}:{p.Default} {p.Target}{target}{p.Text}:{p.Default} {message}";

    private static string Format(
        string prefix,
        Target target,
        string message,
        IReadOnlyCollection<Target> dependencyPath,
        Palette p) =>
        $"{p.Prefix}{prefix}:{p.Default} {p.Target}{target}{p.Text}:{p.Default} {message}{FormatDependencyPath(dependencyPath, p)}";

    private static string Format(
        string prefix,
        Target target,
        string message,
        TimeSpan duration,
        IReadOnlyCollection<Target> dependencyPath,
        Palette p) =>
        $"{p.Prefix}{prefix}:{p.Default} {p.Target}{target}{p.Text}:{p.Default} {message} {p.Timing}({duration.Humanize()}){p.Default}{FormatDependencyPath(dependencyPath, p)}";

    private static string Format<TInput>(
        string prefix,
        Target target,
        TInput input,
        string message,
        Palette p) =>
        $"{p.Prefix}{prefix}:{p.Default} {p.Target}{target}{p.Text}/{p.Input}{input}{p.Text}:{p.Default} {message}";

    private static string Format<TInput>(
        string prefix,
        Target target,
        TInput input,
        string message,
        IReadOnlyCollection<Target> dependencyPath,
        Palette p) =>
        $"{p.Prefix}{prefix}:{p.Default} {p.Target}{target}{p.Text}/{p.Input}{input}{p.Text}:{p.Default} {message}{FormatDependencyPath(dependencyPath, p)}";

    private static string Format<TInput>(
        string prefix,
        Target target,
        TInput input,
        string message,
        TimeSpan duration,
        IReadOnlyCollection<Target> dependencyPath,
        Palette p) =>
        $"{p.Prefix}{prefix}:{p.Default} {p.Target}{target}{p.Text}/{p.Input}{input}{p.Text}:{p.Default} {message} {p.Timing}({duration.Humanize()}){p.Default}{FormatDependencyPath(dependencyPath, p)}";

    private static string Format(
        string prefix,
        string subject,
        string message,
        Palette p) =>
        $"{p.Prefix}{prefix}:{p.Default} {p.Verbose}{subject}:{p.Default} {message}";

    private static string FormatDependencyPath(IReadOnlyCollection<Target> dependencyPath, Palette p) =>
        dependencyPath.Count == 0 ? "" : $" {p.Verbose}(/{string.Join("/", dependencyPath)}){p.Default}";
}
