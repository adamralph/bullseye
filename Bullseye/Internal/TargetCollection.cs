using System.Collections.Immutable;
using System.Collections.ObjectModel;

namespace Bullseye.Internal;

public class TargetCollection() : KeyedCollection<string, Target>(StringComparer.OrdinalIgnoreCase)
{
    private static readonly ImmutableQueue<Target> rootDependencyPath = [];

    protected override string GetKeyForItem(Target item) => item.Name;

    public async Task RunAsync(
        IEnumerable<string> names,
        bool dryRun,
        bool parallel,
        bool skipDependencies,
        Func<Exception, bool> messageOnly,
        Output output)
    {
        if (!skipDependencies)
        {
            this.CheckForMissingDependencies();
        }

        this.CheckForCircularDependencies();

        var targets = new List<Target>();
        var notFound = new List<string>();

        foreach (var name in names)
        {
            if (this.TryGetValue(name, out var target))
            {
                targets.Add(target);
            }
            else
            {
                notFound.Add(name);
            }
        }

        if (notFound.Count > 0)
        {
            throw new InvalidUsageException($"Target{(notFound.Count > 1 ? "s" : "")} not found: {notFound.Spaced()}.");
        }

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

            using var sync = new SemaphoreSlim(1, 1);

            if (parallel)
            {
                var tasks = targets.Select(target => this.RunAsync(target, targets, dryRun, true, skipDependencies, messageOnly, output, runningTargets, sync, rootDependencyPath));
                await Task.WhenAll(tasks).Tax();
            }
            else
            {
                foreach (var target in targets)
                {
                    await this.RunAsync(target, targets, dryRun, false, skipDependencies, messageOnly, output, runningTargets, sync, rootDependencyPath).Tax();
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
        ICollection<Target> explicitTargets,
        bool dryRun,
        bool parallel,
        bool skipDependencies,
        Func<Exception, bool> messageOnly,
        Output output,
        IDictionary<Target, Task> runningTargets,
        SemaphoreSlim sync,
        ImmutableQueue<Target> dependencyPath)
    {
        if (output.Verbose)
        {
            dependencyPath = dependencyPath.Enqueue(target);
        }

        bool targetWasAlreadyStarted;
        Task? runningTarget;

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
            if (runningTarget!.IsAwaitable())
            {
                await output.Awaiting(target, [.. dependencyPath]).Tax();
                await runningTarget!.Tax();
            }

            return;
        }

        await output.WalkingDependencies(target, [.. dependencyPath]).Tax();

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
                await output.IgnoringNonExistentDependency(target, dependency, [.. dependencyPath]).Tax();
            }
            else
            {
                await this.RunAsync(this[dependency], explicitTargets, dryRun, parallel, skipDependencies, messageOnly, output, runningTargets, sync, dependencyPath).Tax();
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
                    runningTarget = target.RunAsync(dryRun, parallel, output, messageOnly, [.. dependencyPath]);
                    runningTargets.Add(target, runningTarget);
                }
            }
            finally
            {
                _ = sync.Release();
            }

            if (!targetWasAlreadyStarted || runningTarget!.IsAwaitable())
            {
                await output.Awaiting(target, [.. dependencyPath]).Tax();
                await runningTarget!.Tax();
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
                        : missingDependencies[dependency] = [])
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
                throw new InvalidUsageException($"Circular dependency: {string.Join(" -> ", dependents.Reverse().Append(target.Name))}");
            }

            dependents.Push(target.Name);

            foreach (var dependency in target.Dependencies.Where(this.Contains))
            {
                Check(this[dependency]);
            }

            _ = dependents.Pop();
        }
    }
}
