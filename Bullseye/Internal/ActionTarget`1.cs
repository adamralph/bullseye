using System.Diagnostics;

namespace Bullseye.Internal;

public class ActionTarget<TInput>(string name, string description, IEnumerable<string> dependencies, IEnumerable<TInput> inputs, Func<TInput, Task> action)
    : Target(name, description, dependencies), IHaveInputs
{
    private readonly IEnumerable<TInput> inputs = inputs;
    private readonly Func<TInput, Task> action = action;

    public IEnumerable<object?> Inputs => this.inputs.Cast<object?>();

    public override async Task RunAsync(bool dryRun, bool parallel, Output output, Func<Exception, bool> messageOnly, IReadOnlyCollection<Target> dependencyPath)
    {
        var inputsList = this.inputs.ToList();

        if (inputsList.Count == 0)
        {
            await output.NoInputs(this, dependencyPath).Tax();
            return;
        }

        if (parallel)
        {
            var tasks = inputsList.Select(input => this.RunAsync(input, Guid.NewGuid(), dryRun, output, messageOnly, dependencyPath)).ToList();

            await Task.WhenAll(tasks).Tax();
        }
        else
        {
            foreach (var input in inputsList)
            {
                await this.RunAsync(input, Guid.NewGuid(), dryRun, output, messageOnly, dependencyPath).Tax();
            }
        }
    }

    private async Task RunAsync(TInput input, Guid id, bool dryRun, Output output, Func<Exception, bool> messageOnly, IReadOnlyCollection<Target> dependencyPath)
    {
        await output.BeginGroup(this, input).Tax();

        try
        {
            await output.Starting(this, input, id, dependencyPath).Tax();

            var stopWatch = new Stopwatch();

            if (!dryRun)
            {
                await this.RunAsync(input, id, output, messageOnly, dependencyPath, stopWatch).Tax();
            }

            await output.Succeeded(this, input, id, dependencyPath, stopWatch.Elapsed).Tax();
        }
        finally
        {
            await output.EndGroup().Tax();
        }
    }

    private async Task RunAsync(TInput input, Guid id, Output output, Func<Exception, bool> messageOnly, IReadOnlyCollection<Target> dependencyPath, Stopwatch stopWatch)
    {
        stopWatch.Start();

        try
        {
            await this.action(input).Tax();
        }
        catch (Exception ex)
        {
            var duration = stopWatch.Elapsed;

            if (!messageOnly(ex))
            {
                await output.Error(this, input, ex).Tax();
            }

            await output.Failed(this, input, id, ex, duration, dependencyPath).Tax();

            throw new TargetFailedException($"Target '{this.Name}' failed with input '{input}'.", ex);
        }
    }
}
