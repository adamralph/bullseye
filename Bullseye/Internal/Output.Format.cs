using System;
using System.Collections.Generic;
using System.Linq;

namespace Bullseye.Internal
{
    public partial class Output
    {
        private static string Format(
            string message,
            IEnumerable<Target> targets,
            bool dryRun,
            bool parallel,
            bool skipDependencies,
            string prefix,
            Palette p,
            TimeSpan? duration = null) =>
            $"{p.Prefix}{prefix}:{p.Reset} {message} {p.Target}({targets.Select(target => target.Name).Spaced()}){p.Reset}{(dryRun ? $" {p.Option}(dry run){p.Reset}" : "")}{(parallel ? $" {p.Option}(parallel){p.Reset}" : "")}{(skipDependencies ? $" {p.Option}(skip dependencies){p.Reset}" : "")}{(duration.HasValue ? $" {p.Timing}({duration.Value.Humanize()}){p.Reset}" : "")}";

        private static string Format(
            string message,
            Target target,
            string prefix,
            Palette p,
            IReadOnlyCollection<Target>? dependencyPath = null,
            TimeSpan? duration = null) =>
            $"{p.Prefix}{prefix}:{p.Reset} {p.Target}{target}{p.Default}:{p.Reset} {message}{(duration.HasValue ? $" {p.Timing}({duration.Value.Humanize()}){p.Reset}" : "")}{FormatDependencyPath(dependencyPath, p)}";

        private static string Format<TInput>(
            string message,
            Target target,
            TInput input,
            string prefix,
            Palette p,
            IReadOnlyCollection<Target>? dependencyPath = null,
            TimeSpan? duration = null) =>
            $"{p.Prefix}{prefix}:{p.Reset} {p.Target}{target}{p.Default}/{p.Input}{input}{p.Default}:{p.Reset} {message}{(duration.HasValue ? $" {p.Timing}({duration.Value.Humanize()}){p.Reset}" : "")}{FormatDependencyPath(dependencyPath, p)}";

        private static string Format(
            string message,
            string subject,
            string prefix,
            Palette p) =>
            $"{p.Prefix}{prefix}:{p.Reset} {p.Verbose}{subject}:{p.Reset} {message}";

        private static string FormatDependencyPath(IReadOnlyCollection<Target>? dependencyPath, Palette p) =>
            dependencyPath == null || dependencyPath.Count == 0 ? "" : $" {p.Verbose}(/{string.Join($"/", dependencyPath)}){p.Reset}";
    }
}
