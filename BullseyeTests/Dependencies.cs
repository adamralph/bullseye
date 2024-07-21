using System.Text.RegularExpressions;
using Bullseye.Internal;
using Xunit;
using static BullseyeTests.Infra.Helper;

namespace BullseyeTests;

public static partial class Dependencies
{
    [Fact]
    public static async Task FlatDependencies()
    {
        // arrange
        var ran = new List<string>();

        var targets = new TargetCollection
        {
            CreateTarget("first", () => ran.Add("first")),
            CreateTarget("second", () => ran.Add("second")),
            CreateTarget("third", ["first", "second",], () => ran.Add("third")),
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
            CreateTarget("first", () => ran.Add("first")),
            CreateTarget("second", ["first",], () => ran.Add("second")),
            CreateTarget("third", ["second",], () => ran.Add("third")),
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
            CreateTarget("first", () => ran.Add("first")),
            CreateTarget("second", ["first", "first",], () => ran.Add("second")),
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
            CreateTarget("first", ["first",]),
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
            CreateTarget("first", ["second",]),
            CreateTarget("second", ["first",]),
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
            CreateTarget("first", ["third",]),
            CreateTarget("second", ["first",]),
            CreateTarget("third", ["second",]),
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
            CreateTarget("first", () => ran.Add("first")),
            CreateTarget("second", ["first",], () => ran.Add("second")),
            CreateTarget("third", ["first", "second",], () => ran.Add("third")),
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

#if NET8_0_OR_GREATER
    [GeneratedRegex("first: Walking dependencies...")]
    private static partial Regex FirstWalkingDependencies();
#else
    private static Regex FirstWalkingDependencies() => new("first: Walking dependencies...");
#endif

#if NET8_0_OR_GREATER
    [GeneratedRegex("first: Awaiting...")]
    private static partial Regex FirstAwaiting();
#else
    private static Regex FirstAwaiting() => new("first: Awaiting...");
#endif

    [Fact]
    public static async Task NotExistentDependencies()
    {
        // arrange
        var anyRan = false;

        var targets = new TargetCollection
        {
            CreateTarget("first", () => anyRan = true),
            CreateTarget("second", ["first", "non-existing",], () => anyRan = true),
            CreateTarget("third", ["second", "also-non-existing",], () => anyRan = true),
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
            CreateTarget("first", () => ran.Add("first")),
            CreateTarget("second", ["first", "non-existent",], () => ran.Add("second")),
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
            CreateTarget("first", () => ran.Add("first")),
            CreateTarget("second", ["first",], () => ran.Add("second")),
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
            CreateTarget(
                "build",
                () =>
                {
                    Thread.Sleep(TimeSpan.FromSeconds(1)); // a weak way to encourage the tests to run first
                    buildStartTime = Interlocked.Increment(ref clock);
                }),
            CreateTarget("test1", ["build",], () => test1StartTime = Interlocked.Increment(ref clock)),
            CreateTarget("test2", ["build",], () => test2StartTime = Interlocked.Increment(ref clock)),
        };

        // act
        await targets.RunAsync(["--parallel", "--skip-dependencies", "test1", "test2", "build",], _ => false, () => "", Console.Out, Console.Error, false);

        // assert
        Assert.Equal(1, buildStartTime);
        Assert.Equal(5, test1StartTime + test2StartTime);
    }
}
