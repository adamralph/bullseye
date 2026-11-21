namespace Bullseye.Internal;

internal sealed class NoActionTarget(string name, string description, IReadOnlyCollection<string> dependencies)
    : Target(name, description, dependencies)
{
    public override Task RunAsync(bool dryRun, bool parallel, SemaphoreSlim parallelTargets, Output output, Func<Exception, bool> messageOnly, IReadOnlyCollection<Target> dependencyPath) =>
        output.SucceededAsync(this, dependencyPath, TimeSpan.Zero);
}
