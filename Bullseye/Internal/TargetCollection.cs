using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Bullseye.Internal
{
    public class TargetCollection : KeyedCollection<string, Target>
    {
        public TargetCollection() : base(StringComparer.OrdinalIgnoreCase) { }

        protected override string GetKeyForItem(Target item) => item.Name;

        public async Task RunAsync(IEnumerable<string> names, bool skipDependencies, bool dryRun, bool parallel, Logger log, Func<Exception, bool> messageOnly)
        {
            if (!skipDependencies)
            {
                this.CheckForMissingDependencies();
            }

            this.CheckForCircularDependencies();
            this.CheckContains(names);

            await log.Starting(names).Tax();

            try
            {
                var runningTargets = new Dictionary<string, Task>();

                using (var sync = new SemaphoreSlim(1, 1))
                {
                    if (parallel)
                    {
                        var tasks = names.Select(name => this.RunAsync(name, names, skipDependencies, dryRun, true, log, messageOnly, runningTargets, sync));
                        await Task.WhenAll(tasks).Tax();
                    }
                    else
                    {
                        foreach (var name in names)
                        {
                            await this.RunAsync(name, names, skipDependencies, dryRun, false, log, messageOnly, runningTargets, sync).Tax();
                        }
                    }
                }
            }
            catch (Exception)
            {
                await log.Failed(names).Tax();
                throw;
            }

            await log.Succeeded(names).Tax();
        }

        private async Task RunAsync(string name, IEnumerable<string> explicitTargets, bool skipDependencies, bool dryRun, bool parallel, Logger log, Func<Exception, bool> messageOnly, Dictionary<string, Task> runningTargets, SemaphoreSlim sync, Queue<string> dependencyPath = null)
        {
            if (log.IsVerbose)
            {
                // can switch to ImmutableQueue after dropping support for .NET Framework
                dependencyPath = dependencyPath == null ? new Queue<string>() : new Queue<string>(dependencyPath);
                dependencyPath.Enqueue(name);
            }

            if (!this.Contains(name))
            {
                await log.Verbose(dependencyPath, "Doesn't exist. Ignoring.").Tax();
                return;
            }

            bool gotValue;
            Task runningTarget;

            // cannot use WaitAsync() as it is not reentrant
            sync.Wait();

            try
            {
                gotValue = runningTargets.TryGetValue(name, out runningTarget);
            }
            finally
            {
                _ = sync.Release();
            }

            if (gotValue)
            {
                await log.Verbose(dependencyPath, "Awaiting...").Tax();
                await runningTarget.Tax();
                return;
            }

            await log.Verbose(dependencyPath, "Walking dependencies...").Tax();

            var target = this[name];

            if (parallel)
            {
                var tasks = target.Dependencies.Select(dependency => this.RunAsync(dependency, explicitTargets, skipDependencies, dryRun, true, log, messageOnly, runningTargets, sync, dependencyPath));
                await Task.WhenAll(tasks).Tax();
            }
            else
            {
                foreach (var dependency in target.Dependencies)
                {
                    await this.RunAsync(dependency, explicitTargets, skipDependencies, dryRun, false, log, messageOnly, runningTargets, sync, dependencyPath).Tax();
                }
            }

            if (!skipDependencies || explicitTargets.Contains(name))
            {
                await log.Verbose(dependencyPath, "Awaiting...").Tax();

                // cannot use WaitAsync() as it is not reentrant
                sync.Wait();

                try
                {
                    if (!runningTargets.TryGetValue(name, out runningTarget))
                    {
                        runningTarget = target.RunAsync(dryRun, parallel, log, messageOnly);
                        runningTargets.Add(name, runningTarget);
                    }
                }
                finally
                {
                    _ = sync.Release();
                }

                await runningTarget.Tax();
            }
        }

        private void CheckForMissingDependencies()
        {
            var missingDependencies = new SortedDictionary<string, SortedSet<string>>();

            foreach (var target in this)
            {
                foreach (var dependency in target.Dependencies.Where(dependency => !this.Contains(dependency)))
                {
                    _ = (missingDependencies.TryGetValue(dependency, out var set)
                            ? set
                            : missingDependencies[dependency] = new SortedSet<string>())
                        .Add(target.Name);
                }
            }

            if (missingDependencies.Count != 0)
            {
                var message =
                    $"Missing {(missingDependencies.Count > 1 ? "dependencies" : "dependency")}: " +
                    string.Join("; ", missingDependencies.Select(dependency => $"{dependency.Key}, required by {dependency.Value.Spaced()}"));

                throw new InvalidUsageException(message);
            }
        }

        private void CheckForCircularDependencies()
        {
            var dependents = new Stack<string>();

            foreach (var target in this)
            {
                Check(target);
            }

            void Check(Target target)
            {
                if (dependents.Contains(target.Name))
                {
                    throw new InvalidUsageException($"Circular dependency: {string.Join(" -> ", dependents.Reverse().Concat(new[] { target.Name }))}");
                }

                dependents.Push(target.Name);

                foreach (var dependency in target.Dependencies.Where(this.Contains))
                {
                    Check(this[dependency]);
                }

                _ = dependents.Pop();
            }
        }

        private void CheckContains(IEnumerable<string> names)
        {
            var notFound = new SortedSet<string>(names.Where(name => !this.Contains(name)));

            if (notFound.Count > 0)
            {
                throw new InvalidUsageException($"Target{(notFound.Count > 1 ? "s" : "")} not found: {notFound.Spaced()}.");
            }
        }
    }
}
