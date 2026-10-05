using System.Diagnostics;

namespace Bullseye.Internal;

public class ActionTarget(string name, string description, IReadOnlyCollection<string> dependencies, Func<Task> action)
        : Target(name, description, dependencies)
{
    public override async Task RunAsync(bool dryRun, bool parallel, SemaphoreSlim parallelTargets, Output output,
        Func<Exception, bool> messageOnly, IReadOnlyCollection<Target> dependencyPath)
    {
        if (parallel)
        {
            await parallelTargets.WaitAsync().Tax();
            try
            {
                await RunAsync(dryRun, output, messageOnly, dependencyPath).Tax();
            }
            finally
            {
                _ = parallelTargets.Release();
            }
        }
        else
        {
            await RunAsync(dryRun, output, messageOnly, dependencyPath).Tax();
        }
    }

    private async Task RunAsync(bool dryRun, Output output, Func<Exception, bool> messageOnly, IReadOnlyCollection<Target> dependencyPath)
    {
        await output.BeginGroupAsync(this).Tax();

        try
        {
            await output.StartingAsync(this, dependencyPath).Tax();

            var stopWatch = new Stopwatch();

            if (!dryRun)
            {
                await RunAsync(output, messageOnly, dependencyPath, stopWatch).Tax();
            }

            await output.SucceededAsync(this, dependencyPath, stopWatch.Elapsed).Tax();
        }
        finally
        {
            await output.EndGroupAsync().Tax();
        }
    }

    private async Task RunAsync(Output output, Func<Exception, bool> messageOnly, IReadOnlyCollection<Target> dependencyPath, Stopwatch stopWatch)
    {
        stopWatch.Start();

        try
        {
            await action().Tax();
        }
        catch (Exception ex)
        {
            var duration = stopWatch.Elapsed;

            if (!messageOnly(ex))
            {
                await output.ErrorAsync(this, ex).Tax();
            }

            await output.FailedAsync(this, ex, duration, dependencyPath).Tax();

            throw new TargetFailedException($"Target '{Name}' failed.", ex);
        }
    }
}
