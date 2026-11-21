namespace Bullseye.Internal;

internal abstract class Target(string name, string description, IReadOnlyCollection<string> dependencies)
{
    public string Name { get; } = name;

    public string Description { get; } = description;

    public IReadOnlyCollection<string> Dependencies { get; } = dependencies;

    public abstract Task RunAsync(bool dryRun, bool parallel, SemaphoreSlim parallelTargets, Output output, Func<Exception, bool> messageOnly, IReadOnlyCollection<Target> dependencyPath);

    public override string ToString() => Name;
}
