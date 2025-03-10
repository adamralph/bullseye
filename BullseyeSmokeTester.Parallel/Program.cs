using static Bullseye.Targets;

var targetsCount = Environment.ProcessorCount;
var targetNames = Enumerable.Range(1, targetsCount).Select(i => $"target-{i:000}").ToList();

var runningTargetsCount = 0;

foreach (var targetName in targetNames)
{
    var dep1Name = $"{targetName}-dep-1";
    var dep2Name = $"{targetName}-dep-2";

    Target(dep1Name, RunTarget);
    Target(dep2Name, RunTarget);
    Target(targetName, [dep1Name, dep2Name], () => { });
}

await Console.Out.WriteLineAsync($"Running {targetsCount:N0} targets each with 2 dependencies on {Environment.ProcessorCount:N0} processors...");

await RunTargetsAndExitAsync([.. targetNames, "--parallel"]);

async Task RunTarget()
{
    _ = Interlocked.Increment(ref runningTargetsCount);
    await Console.Out.WriteLineAsync($"Starting target {runningTargetsCount} at {DateTime.UtcNow}...");
    await Task.Delay(TimeSpan.FromSeconds(5));
}
