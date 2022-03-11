using System;
using System.Collections.Generic;
using System.Linq;

namespace Bullseye.Internal
{
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
            $"{p.Prefix}{prefix}:{p.Reset} {message} {p.Target}({targets.Select(target => target.Name).Spaced()}){p.Reset}{(dryRun ? $" {p.Option}(dry run){p.Reset}" : "")}{(parallel ? $" {p.Option}(parallel){p.Reset}" : "")}{(skipDependencies ? $" {p.Option}(skip dependencies){p.Reset}" : "")}";

        private static string Format(
            string prefix,
            IEnumerable<Target> targets,
            string message,
            bool dryRun,
            bool parallel,
            bool skipDependencies,
            TimeSpan duration,
            Palette p) =>
            $"{p.Prefix}{prefix}:{p.Reset} {message} {p.Target}({targets.Select(target => target.Name).Spaced()}){p.Reset}{(dryRun ? $" {p.Option}(dry run){p.Reset}" : "")}{(parallel ? $" {p.Option}(parallel){p.Reset}" : "")}{(skipDependencies ? $" {p.Option}(skip dependencies){p.Reset}" : "")} {p.Timing}({duration.Humanize()}){p.Reset}";

        private static string Format(
            string prefix,
            Target target,
            string message,
            Palette p) =>
            $"{p.Prefix}{prefix}:{p.Reset} {p.Target}{target}{p.Default}:{p.Reset} {message}";

        private static string Format(
            string prefix,
            Target target,
            string message,
            IReadOnlyCollection<Target> dependencyPath,
            Palette p) =>
            $"{p.Prefix}{prefix}:{p.Reset} {p.Target}{target}{p.Default}:{p.Reset} {message}{FormatDependencyPath(dependencyPath, p)}";

        private static string Format(
            string prefix,
            Target target,
            string message,
            TimeSpan duration,
            IReadOnlyCollection<Target> dependencyPath,
            Palette p) =>
            $"{p.Prefix}{prefix}:{p.Reset} {p.Target}{target}{p.Default}:{p.Reset} {message} {p.Timing}({duration.Humanize()}){p.Reset}{FormatDependencyPath(dependencyPath, p)}";

        private static string Format<TInput>(
            string prefix,
            Target target,
            TInput input,
            string message,
            Palette p) =>
            $"{p.Prefix}{prefix}:{p.Reset} {p.Target}{target}{p.Default}/{p.Input}{input}{p.Default}:{p.Reset} {message}";

        private static string Format<TInput>(
            string prefix,
            Target target,
            TInput input,
            string message,
            IReadOnlyCollection<Target> dependencyPath,
            Palette p) =>
            $"{p.Prefix}{prefix}:{p.Reset} {p.Target}{target}{p.Default}/{p.Input}{input}{p.Default}:{p.Reset} {message}{FormatDependencyPath(dependencyPath, p)}";

        private static string Format<TInput>(
            string prefix,
            Target target,
            TInput input,
            string message,
            TimeSpan duration,
            IReadOnlyCollection<Target> dependencyPath,
            Palette p) =>
            $"{p.Prefix}{prefix}:{p.Reset} {p.Target}{target}{p.Default}/{p.Input}{input}{p.Default}:{p.Reset} {message} {p.Timing}({duration.Humanize()}){p.Reset}{FormatDependencyPath(dependencyPath, p)}";

        private static string Format(
            string prefix,
            string subject,
            string message,
            Palette p) =>
            $"{p.Prefix}{prefix}:{p.Reset} {p.Verbose}{subject}:{p.Reset} {message}";

        private static string FormatDependencyPath(IReadOnlyCollection<Target> dependencyPath, Palette p) =>
            dependencyPath.Count == 0 ? "" : $" {p.Verbose}(/{string.Join("/", dependencyPath)}){p.Reset}";
    }
}
