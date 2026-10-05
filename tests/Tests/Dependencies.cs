using System.Text.RegularExpressions;
using Bullseye.Internal;
using Tests.Fixtures;
using Xunit;

namespace Tests;

public static partial class Dependencies
{
    [Fact]
    public static async Task FlatDependencies()
    {
        // arrange
        var ran = new List<string>();

        var targets = new TargetCollection
        {
            Target.Create("first", () => ran.Add("first")),
            Target.Create("second", () => ran.Add("second")),
            Target.Create("third", ["first", "second",], () => ran.Add("third")),
        };

        // act
        await targets.RunAsync(["third",], _ => false, () => "", Console.Out, Console.Error, false);

        // assert
        Assert.Equal(3, ran.Count);
        Assert.Equal("first", ran[0]);
        Assert.Equal("second", ran[1]);
        Assert.Equal("third", ran[2]);
    }

    [Fact]
    public static async Task NestedDependencies()
    {
        // arrange
        var ran = new List<string>();

        var targets = new TargetCollection
        {
            Target.Create("first", () => ran.Add("first")),
            Target.Create("second", ["first",], () => ran.Add("second")),
            Target.Create("third", ["second",], () => ran.Add("third")),
        };

        // act
        await targets.RunAsync(["third",], _ => false, () => "", Console.Out, Console.Error, false);

        // assert
        Assert.Equal(3, ran.Count);
        Assert.Equal("first", ran[0]);
        Assert.Equal("second", ran[1]);
        Assert.Equal("third", ran[2]);
    }

    [Fact]
    public static async Task DoubleDependency()
    {
        // arrange
        var ran = new List<string>();

        var targets = new TargetCollection
        {
            Target.Create("first", () => ran.Add("first")),
            Target.Create("second", ["first", "first",], () => ran.Add("second")),
        };

        // act
        await targets.RunAsync(["second",], _ => false, () => "", Console.Out, Console.Error, false);

        // assert
        Assert.Equal(2, ran.Count);
        Assert.Equal("first", ran[0]);
        Assert.Equal("second", ran[1]);
    }

    [Fact]
    public static async Task SelfDependency()
    {
        // arrange
        var targets = new TargetCollection
        {
            Target.Create("first", ["first",]),
        };

        // act
        var exception = await Record.ExceptionAsync(() => targets.RunAsync(["first",], _ => false, () => "", Console.Out, Console.Error, false));

        // assert
        Assert.NotNull(exception);
        Assert.Contains("first -> first", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public static async Task MutualDependency()
    {
        // arrange
        var targets = new TargetCollection
        {
            Target.Create("first", ["second",]),
            Target.Create("second", ["first",]),
        };

        // act
        var exception = await Record.ExceptionAsync(() => targets.RunAsync(["second",], _ => false, () => "", Console.Out, Console.Error, false));

        // assert
        Assert.NotNull(exception);
        Assert.Contains("first -> second -> first", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public static async Task CircularDependency()
    {
        // arrange
        var targets = new TargetCollection
        {
            Target.Create("first", ["third",]),
            Target.Create("second", ["first",]),
            Target.Create("third", ["second",]),
        };

        // act
        var exception = await Record.ExceptionAsync(() => targets.RunAsync(["third",], _ => false, () => "", Console.Out, Console.Error, false));

        // assert
        Assert.NotNull(exception);
        Assert.Contains("first -> third -> second -> first", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public static async Task DoubleTransitiveDependency()
    {
        // arrange
        var ran = new List<string>();

        await using var outputWriter = new StringWriter();

        var targets = new TargetCollection
        {
            Target.Create("first", () => ran.Add("first")),
            Target.Create("second", ["first",], () => ran.Add("second")),
            Target.Create("third", ["first", "second",], () => ran.Add("third")),
        };

        // act
        await targets.RunAsync(["third", "--no-color", "--verbose",], _ => false, () => "", outputWriter, Console.Error, false);

        // assert
        var output = outputWriter.ToString();

        Assert.Equal(3, ran.Count);
        Assert.Equal("first", ran[0]);
        Assert.Equal("second", ran[1]);
        Assert.Equal("third", ran[2]);
        _ = Assert.Single(FirstWalkingDependencies().Matches(output));
        _ = Assert.Single(FirstAwaiting().Matches(output));
    }

    [GeneratedRegex("first: Walking dependencies...")]
    private static partial Regex FirstWalkingDependencies();

    [GeneratedRegex("first: Awaiting...")]
    private static partial Regex FirstAwaiting();

    [Fact]
    public static async Task NotExistentDependencies()
    {
        // arrange
        var anyRan = false;

        var targets = new TargetCollection
        {
            Target.Create("first", () => anyRan = true),
            Target.Create("second", ["first", "non-existing",], () => anyRan = true),
            Target.Create("third", ["second", "also-non-existing",], () => anyRan = true),
        };

        // act
        var exception = await Record.ExceptionAsync(() => targets.RunAsync(["third",], _ => false, () => "", Console.Out, Console.Error, false));

        // assert
        Assert.NotNull(exception);
        Assert.Contains("non-existing, required by second", exception.Message, StringComparison.Ordinal);
        Assert.Contains("also-non-existing, required by third", exception.Message, StringComparison.Ordinal);
        Assert.False(anyRan);
    }

    [Fact]
    public static async Task SkippingDependencies()
    {
        // arrange
        var ran = new List<string>();

        var targets = new TargetCollection
        {
            Target.Create("first", () => ran.Add("first")),
            Target.Create("second", ["first", "non-existent",], () => ran.Add("second")),
        };

        // act
        await targets.RunAsync(["second", "-s",], _ => false, () => "", Console.Out, Console.Error, false);

        // assert
        Assert.Contains("second", ran);
        Assert.DoesNotContain("first", ran);
    }

    [Fact]
    public static async Task DependencyOrderWhenSkipping()
    {
        // arrange
        var ran = new List<string>();

        var targets = new TargetCollection
        {
            Target.Create("first", () => ran.Add("first")),
            Target.Create("second", ["first",], () => ran.Add("second")),
        };

        // act
        await targets.RunAsync(["--skip-dependencies", "second", "first",], _ => false, () => "", Console.Out, Console.Error, false);

        // assert
        Assert.Equal(2, ran.Count);
        Assert.Equal("first", ran[0]);
        Assert.Equal("second", ran[1]);
    }

    [Fact]
    public static async Task DependencyOrderWhenParallelAndSkipping()
    {
        // arrange
        var clock = 0;
        var (buildStartTime, test1StartTime, test2StartTime) = (0, 0, 0);

        var targets = new TargetCollection
        {
            Target.Create(
                "build",
                () =>
                {
                    Thread.Sleep(TimeSpan.FromSeconds(1)); // a weak way to encourage the tests to run first
                    buildStartTime = Interlocked.Increment(ref clock);
                }),
            Target.Create("test1", ["build",], () => test1StartTime = Interlocked.Increment(ref clock)),
            Target.Create("test2", ["build",], () => test2StartTime = Interlocked.Increment(ref clock)),
        };

        // act
        await targets.RunAsync(["--parallel", "--skip-dependencies", "test1", "test2", "build",], _ => false, () => "", Console.Out, Console.Error, false);

        // assert
        Assert.Equal(1, buildStartTime);
        Assert.Equal(5, test1StartTime + test2StartTime);
    }
}
