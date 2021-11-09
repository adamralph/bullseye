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

        public async Task RunAsync(
            IEnumerable<string> names,
            bool dryRun,
            bool parallel,
            bool skipDependencies,
            Func<Exception, bool> messageOnly,
            Output output)
        {
            var nameList = names.Sanitize().ToList();
            output = output ?? throw new ArgumentNullException(nameof(output));

            if (!skipDependencies)
            {
                this.CheckForMissingDependencies();
            }

            this.CheckForCircularDependencies();
            this.CheckContains(nameList);

            var targets = nameList.Select(name => this[name]).ToList();

            await this.RunAsync(targets, dryRun, parallel, skipDependencies, messageOnly, output).Tax();
        }

        private async Task RunAsync(
            List<Target> targets,
            bool dryRun,
            bool parallel,
            bool skipDependencies,
            Func<Exception, bool> messageOnly,
            Output output)
        {
            await output.Starting(targets).Tax();

            try
            {
                var runningTargets = new Dictionary<Target, Task>();

                using (var sync = new SemaphoreSlim(1, 1))
                {
                    if (parallel)
                    {
                        var tasks = targets.Select(target => this.RunAsync(target, targets, dryRun, true, skipDependencies, messageOnly, output, runningTargets, sync));
                        await Task.WhenAll(tasks).Tax();
                    }
                    else
                    {
                        foreach (var target in targets)
                        {
                            await this.RunAsync(target, targets, dryRun, false, skipDependencies, messageOnly, output, runningTargets, sync).Tax();
                        }
                    }
                }
            }
            catch (Exception)
            {
                await output.Failed(targets).Tax();
                throw;
            }

            await output.Succeeded(targets).Tax();
        }

        private async Task RunAsync(
            Target target,
            List<Target> explicitTargets,
            bool dryRun,
            bool parallel,
            bool skipDependencies,
            Func<Exception, bool> messageOnly,
            Output output,
            Dictionary<Target, Task> runningTargets,
            SemaphoreSlim sync,
            Queue<Target> dependencyPath = null)
        {
            if (output.Verbose)
            {
                // can switch to ImmutableQueue after dropping support for .NET Framework
                dependencyPath = dependencyPath == null ? new Queue<Target>() : new Queue<Target>(dependencyPath);
                dependencyPath.Enqueue(target);
            }

            bool targetWasAlreadyStarted;
            Task runningTarget;

            // cannot use WaitAsync() as it is not reentrant
#pragma warning disable CA1849 // Call async methods when in an async method
            sync.Wait();
#pragma warning restore CA1849 // Call async methods when in an async method

            try
            {
                targetWasAlreadyStarted = runningTargets.TryGetValue(target, out runningTarget);
            }
            finally
            {
                _ = sync.Release();
            }

            if (targetWasAlreadyStarted)
            {
                if (runningTarget.IsAwaitable())
                {
                    await output.Awaiting(target, dependencyPath).Tax();
                    await runningTarget.Tax();
                }

                return;
            }

            await output.WalkingDependencies(target, dependencyPath).Tax();

            if (parallel)
            {
                var tasks = target.Dependencies.Select(RunDependencyAsync);
                await Task.WhenAll(tasks).Tax();
            }
            else
            {
                foreach (var dependency in target.Dependencies)
                {
                    await RunDependencyAsync(dependency).Tax();
                }
            }

            async Task RunDependencyAsync(string dependency)
            {
                if (!this.Contains(dependency))
                {
                    await output.IgnoringNonExistentDependency(target, dependency, dependencyPath).Tax();
                }
                else
                {
                    await this.RunAsync(this[dependency], explicitTargets, dryRun, false, skipDependencies, messageOnly, output, runningTargets, sync, dependencyPath).Tax();
                }
            }

            if (!skipDependencies || explicitTargets.Contains(target))
            {
                // cannot use WaitAsync() as it is not reentrant
#pragma warning disable CA1849 // Call async methods when in an async method
                sync.Wait();
#pragma warning restore CA1849 // Call async methods when in an async method

                try
                {
                    targetWasAlreadyStarted = runningTargets.TryGetValue(target, out runningTarget);

                    if (!targetWasAlreadyStarted)
                    {
                        runningTarget = target.RunAsync(dryRun, parallel, output, messageOnly, dependencyPath);
                        runningTargets.Add(target, runningTarget);
                    }
                }
                finally
                {
                    _ = sync.Release();
                }

                if (!targetWasAlreadyStarted || runningTarget.IsAwaitable())
                {
                    await output.Awaiting(target, dependencyPath).Tax();
                    await runningTarget.Tax();
                }
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

        private void CheckContains(List<string> names)
        {
            var notFound = new SortedSet<string>(names.Where(name => !this.Contains(name)));

            if (notFound.Count > 0)
            {
                throw new InvalidUsageException($"Target{(notFound.Count > 1 ? "s" : "")} not found: {notFound.Spaced()}.");
            }
        }
    }
}
