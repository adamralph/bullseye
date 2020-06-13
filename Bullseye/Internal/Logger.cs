#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable IDE0009 // Member access should be qualified.
namespace Bullseye.Internal
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using static System.Math;

    public class Logger
    {
        private readonly TextWriter writer;
        private readonly string prefix;
        private readonly bool skipDependencies;
        private readonly bool dryRun;
        private readonly bool parallel;
        private readonly Palette p;
        private readonly bool verbose;
        private readonly Summary summary;

        public Logger(TextWriter writer, string prefix, bool skipDependencies, bool dryRun, bool parallel, Palette palette, bool verbose)
        {
            this.writer = writer;
            this.prefix = prefix;
            this.skipDependencies = skipDependencies;
            this.dryRun = dryRun;
            this.parallel = parallel;
            this.p = palette;
            this.verbose = verbose;
            this.summary = new Summary(palette);
        }

        public async Task Version()
        {
            if (this.verbose)
            {
                var version = typeof(TargetCollectionExtensions).Assembly.GetCustomAttributes(false)
                    .OfType<AssemblyInformationalVersionAttribute>()
                    .FirstOrDefault()
                    ?.InformationalVersion ?? "Unknown";

                await this.writer.WriteLineAsync(Message(p.Verbose, $"Bullseye version: {version}")).Tax();
            }
        }

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
            this.writer.WriteLineAsync(Message(p.Default, $"Starting...", targets, null));

        public async Task Failed(List<string> targets, TimeSpan duration)
        {
            await this.summary.Results().Tax();
            await this.writer.WriteLineAsync(Message(p.Failed, $"Failed!", targets, duration)).Tax();
        }

        public async Task Succeeded(List<string> targets, TimeSpan duration)
        {
            foreach(var )
            await this.writer.WriteLineAsync(Message(p.Succeeded, $"Succeeded.", targets, duration)).Tax();
        }

        public Task Starting(string target)
        {
            this.summary.InternResult(target);

            return this.writer.WriteLineAsync(Message(p.Default, "Starting...", target, null));
        }

        public Task Error(string target, Exception ex) =>
            this.writer.WriteLineAsync(Message(p.Failed, ex.ToString(), target));

        public Task Failed(string target, Exception ex, TimeSpan duration)
        {
            var result = this.summary.InternResult(target);
            result.Outcome = Summary.TargetOutcome.Failed;
            result.Duration = duration;

            return this.writer.WriteLineAsync(Message(p.Failed, $"Failed! {ex.Message}", target, duration));
        }

        public Task Failed(string target, TimeSpan duration)
        {
            var result = this.summary.InternResult(target);
            result.Outcome = Summary.TargetOutcome.Failed;
            result.Duration = duration;

            return this.writer.WriteLineAsync(Message(p.Failed, $"Failed!", target, duration));
        }

        public Task Succeeded(string target, TimeSpan? duration)
        {
            var result = this.summary.InternResult(target);
            result.Outcome = Summary.TargetOutcome.Succeeded;
            result.Duration = duration;

            return this.writer.WriteLineAsync(Message(p.Succeeded, "Succeeded.", target, duration));
        }

        public Task Starting<TInput>(string target, TInput input) =>
            this.writer.WriteLineAsync(MessageWithInput(p.Default, "Starting...", target, input, null));

        public Task Error<TInput>(string target, TInput input, Exception ex) =>
            this.writer.WriteLineAsync(MessageWithInput(p.Failed, ex.ToString(), target, input));

        public Task Failed<TInput>(string target, TInput input, Exception ex, TimeSpan duration)
        {
            this.summary.InternResult(target).InputResults
                .Enqueue(new Summary.TargetInputResult { Input = input, Outcome = Summary.TargetInputOutcome.Failed, Duration = duration });

            return this.writer.WriteLineAsync(MessageWithInput(p.Failed, $"Failed! {ex.Message}", target, input, duration));
        }

        public Task Succeeded<TInput>(string target, TInput input, TimeSpan duration)
        {
            this.summary.InternResult(target).InputResults
                .Enqueue(new Summary.TargetInputResult { Input = input, Outcome = Summary.TargetInputOutcome.Succeeded, Duration = duration });

            return this.writer.WriteLineAsync(MessageWithInput(p.Succeeded, "Succeeded.", target, input, duration));
        }

        public Task NoInputs(string target)
        {
            this.summary.InternResult(target).Outcome = Summary.TargetOutcome.NoInputs;

            return this.writer.WriteLineAsync(Message(p.Warning, "No inputs!", target, null));
        }

        private string Message(string color, string text) => $"{GetPrefix()}{color}{text}{p.Reset}";

        private string Message(Stack<string> targets, string color, string text) => $"{GetPrefix(targets)}{color}{text}{p.Reset}";

        private string Message(string color, string text, List<string> targets, TimeSpan? duration) =>
            $"{GetPrefix()}{color}{text}{p.Reset} {p.Target}({targets.Spaced()}){p.Reset}{GetSuffix(false, duration)}{p.Reset}";

        private string Message(string color, string text, string target) =>
            $"{GetPrefix(target)}{color}{text}{p.Reset}";

        private string Message(string color, string text, string target, TimeSpan? duration) =>
            $"{GetPrefix(target)}{color}{text}{p.Reset}{GetSuffix(true, duration)}{p.Reset}";

        private string MessageWithInput<TInput>(string color, string text, string target, TInput input) =>
            $"{GetPrefix(target, input)}{color}{text}{p.Reset}";

        private string MessageWithInput<TInput>(string color, string text, string target, TInput input, TimeSpan? duration) =>
            $"{GetPrefix(target, input)}{color}{text}{p.Reset}{GetSuffix(true, duration)}{p.Reset}";

        private string GetPrefix() =>
            $"{p.Prefix}{prefix}:{p.Reset} ";

        private string GetPrefix(Stack<string> targets) =>
            $"{p.Prefix}{prefix}:{p.Reset} {p.Target}{string.Join($"{p.Default}/{p.Target}", targets.Reverse())}{p.Default}:{p.Reset} ";

        private string GetPrefix(string target) =>
            $"{p.Prefix}{prefix}:{p.Reset} {p.Target}{target}{p.Default}:{p.Reset} ";

        private string GetPrefix<TInput>(string target, TInput input) =>
            $"{p.Prefix}{prefix}:{p.Reset} {p.Target}{target}{p.Default}/{p.Input}{input}{p.Default}:{p.Reset} ";

        private string GetSuffix(bool specific, TimeSpan? duration) =>
            (!specific && this.dryRun ? $" {p.Option}(dry run){p.Reset}" : "") +
                (!specific && this.parallel ? $" {p.Option}(parallel){p.Reset}" : "") +
                (!specific && this.skipDependencies ? $" {p.Option}(skip dependencies){p.Reset}" : "") +
                (!this.dryRun && duration.HasValue ? $" {p.Timing}({duration.Humanize()}){p.Reset}" : "");
    }
}
