#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Bullseye.Internal
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;

    public class TargetCollection : KeyedCollection<string, Target>
    {
        public TargetCollection() : base(StringComparer.OrdinalIgnoreCase) { }

        protected override string GetKeyForItem(Target item) => item.Name;

        public async Task RunAsync(List<string> names, bool skipDependencies, bool dryRun, bool parallel, Logger log, Func<Exception, bool> messageOnly)
        {
            if (!skipDependencies)
            {
                this.ValidateDependenciesAreAllDefined();
            }

            this.ValidateTargetGraphIsCycleFree();
            this.Validate(names);

            await log.Starting(names).Tax();
            var stopWatch = Stopwatch.StartNew();

            try
            {
                var targetsRan = new ConcurrentDictionary<string, Task>();
                if (parallel)
                {
                    var tasks = names.Select(name => this.RunAsync(name, names, skipDependencies, dryRun, true, targetsRan, log, messageOnly, new Stack<string>()));
                    await Task.WhenAll(tasks).Tax();
                }
                else
                {
                    foreach (var name in names)
                    {
                        await this.RunAsync(name, names, skipDependencies, dryRun, false, targetsRan, log, messageOnly, new Stack<string>()).Tax();
                    }
                }
            }
            catch (Exception)
            {
                await log.Failed(names, stopWatch.Elapsed).Tax();
                throw;
            }

            await log.Succeeded(names, stopWatch.Elapsed).Tax();
        }

        private async Task RunAsync(string name, ICollection<string> explicitTargets, bool skipDependencies, bool dryRun, bool parallel, ConcurrentDictionary<string, Task> targetsRan, Logger log, Func<Exception, bool> messageOnly, Stack<string> targets)
        {
            targets.Push(name);

            if (!this.Contains(name))
            {
                await log.Verbose(targets, "Doesn't exist. Ignoring.").Tax();
                return;
            }

            await log.Verbose(targets, "Walking dependencies...").Tax();

            var target = this[name];

            if (parallel)
            {
                var tasks = target.Dependencies.Select(dependency => this.RunAsync(dependency, explicitTargets, skipDependencies, dryRun, true, targetsRan, log, messageOnly, targets));
                await Task.WhenAll(tasks).Tax();
            }
            else
            {
                foreach (var dependency in target.Dependencies)
                {
                    await this.RunAsync(dependency, explicitTargets, skipDependencies, dryRun, false, targetsRan, log, messageOnly, targets).Tax();
                }
            }

            if (!skipDependencies || explicitTargets.Contains(name))
            {
                await log.Verbose(targets, "Awaiting...").Tax();
                await targetsRan.GetOrAdd(name, _ => target.RunAsync(dryRun, parallel, log, messageOnly)).Tax();
            }

            targets.Pop();
        }

        private void ValidateDependenciesAreAllDefined()
        {
            var unknownDependencies = new SortedDictionary<string, SortedSet<string>>();

            foreach (var target in this)
            {
                foreach (var dependency in target.Dependencies
                    .Where(dependency => !this.Contains(dependency)))
                {
                    (unknownDependencies.TryGetValue(dependency, out var set)
                            ? set
                            : unknownDependencies[dependency] = new SortedSet<string>())
                        .Add(target.Name);
                }
            }

            if (unknownDependencies.Count != 0)
            {
                var message = $"Missing {(unknownDependencies.Count > 1 ? "dependencies" : "dependency")}: " +
                    string.Join(
                        "; ",
                        unknownDependencies.Select(missingDependency =>
                            $"{missingDependency.Key}, required by {missingDependency.Value.Spaced()}"));

                throw new InvalidUsageException(message);
            }
        }

        private void ValidateTargetGraphIsCycleFree()
        {
            var dependencyChain = new Stack<string>();
            foreach (var target in this)
            {
                this.WalkDependencies(target, dependencyChain);
            }
        }

        private void WalkDependencies(Target target, Stack<string> dependencyChain)
        {
            if (dependencyChain.Contains(target.Name))
            {
                dependencyChain.Push(target.Name);
                throw new InvalidUsageException($"Circular dependency: {string.Join(" -> ", dependencyChain.Reverse())}");
            }

            dependencyChain.Push(target.Name);

            foreach (var dependency in target.Dependencies.Where(this.Contains))
            {
                this.WalkDependencies(this[dependency], dependencyChain);
            }

            dependencyChain.Pop();
        }

        private void Validate(List<string> names)
        {
            var unknownNames = new SortedSet<string>(names.Where(name => !this.Contains(name)));
            if (unknownNames.Count > 0)
            {
                var message = $"Target{(unknownNames.Count > 1 ? "s" : "")} not found: {unknownNames.Spaced()}.";
                throw new InvalidUsageException(message);
            }
        }
    }
}
