namespace Bullseye.Internal;

public class Target(string name, string description, IEnumerable<string> dependencies)
{
    public string Name { get; } = name;

    public string Description { get; } = description;

    public IReadOnlyCollection<string> Dependencies { get; } = [.. dependencies,];

    public virtual Task RunAsync(bool dryRun, bool parallel, SemaphoreSlim parallelTargets, Output output, Func<Exception, bool> messageOnly, IReadOnlyCollection<Target> dependencyPath) => output.Succeeded(this, dependencyPath, TimeSpan.Zero);

    public override string ToString() => this.Name;
}
