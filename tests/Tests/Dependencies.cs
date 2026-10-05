using System.Text.RegularExpressions;
using Bullseye;
using Xunit;

namespace Tests;

public static partial class Dependencies
{
    [Fact]
    public static async Task FlatDependencies()
    {
        // arrange
        var ran = new List<string>();

        var targets = new Targets();
        targets.Add("first", () => ran.Add("first"));
        targets.Add("second", () => ran.Add("second"));
        targets.Add("third", ["first", "second",], () => ran.Add("third"));

        // act
        await targets.RunWithoutExitingAsync(["third",]);

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

        var targets = new Targets();
        targets.Add("first", () => ran.Add("first"));
        targets.Add("second", ["first",], () => ran.Add("second"));
        targets.Add("third", ["second",], () => ran.Add("third"));

        // act
        await targets.RunWithoutExitingAsync(["third",]);

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

        var targets = new Targets();
        targets.Add("first", () => ran.Add("first"));
        targets.Add("second", ["first", "first",], () => ran.Add("second"));

        // act
        await targets.RunWithoutExitingAsync(["second",]);

        // assert
        Assert.Equal(2, ran.Count);
        Assert.Equal("first", ran[0]);
        Assert.Equal("second", ran[1]);
    }

    [Fact]
    public static async Task SelfDependency()
    {
        // arrange
        var targets = new Targets();
        targets.Add("first", ["first",]);

        // act
        var exception = await Record.ExceptionAsync(() => targets.RunWithoutExitingAsync(["first",]));

        // assert
        Assert.NotNull(exception);
        Assert.Contains("first -> first", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public static async Task MutualDependency()
    {
        // arrange
        var targets = new Targets();
        targets.Add("first", ["second",]);
        targets.Add("second", ["first",]);

        // act
        var exception = await Record.ExceptionAsync(() => targets.RunWithoutExitingAsync(["second",]));

        // assert
        Assert.NotNull(exception);
        Assert.Contains("first -> second -> first", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public static async Task CircularDependency()
    {
        // arrange
        var targets = new Targets();
        targets.Add("first", ["third",]);
        targets.Add("second", ["first",]);
        targets.Add("third", ["second",]);

        // act
        var exception = await Record.ExceptionAsync(() => targets.RunWithoutExitingAsync(["third",]));

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

        var targets = new Targets();
        targets.Add("first", () => ran.Add("first"));
        targets.Add("second", ["first",], () => ran.Add("second"));
        targets.Add("third", ["first", "second",], () => ran.Add("third"));

        // act
        await targets.RunWithoutExitingAsync(["third", "--no-color", "--verbose",], outputWriter: outputWriter);

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

        var targets = new Targets();
        targets.Add("first", () => anyRan = true);
        targets.Add("second", ["first", "non-existing",], () => anyRan = true);
        targets.Add("third", ["second", "also-non-existing",], () => anyRan = true);

        // act
        var exception = await Record.ExceptionAsync(() => targets.RunWithoutExitingAsync(["third",]));

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

        var targets = new Targets();
        targets.Add("first", () => ran.Add("first"));
        targets.Add("second", ["first", "non-existent",], () => ran.Add("second"));

        // act
        await targets.RunWithoutExitingAsync(["second", "-s",]);

        // assert
        Assert.Contains("second", ran);
        Assert.DoesNotContain("first", ran);
    }

    [Fact]
    public static async Task DependencyOrderWhenSkipping()
    {
        // arrange
        var ran = new List<string>();

        var targets = new Targets();
        targets.Add("first", () => ran.Add("first"));
        targets.Add("second", ["first",], () => ran.Add("second"));

        // act
        await targets.RunWithoutExitingAsync(["--skip-dependencies", "second", "first",]);

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

        var targets = new Targets();
        targets.Add(
            "build",
            () =>
            {
                Thread.Sleep(TimeSpan.FromSeconds(1)); // a weak way to encourage the tests to run first
                buildStartTime = Interlocked.Increment(ref clock);
            });
        targets.Add("test1", ["build",], () => test1StartTime = Interlocked.Increment(ref clock));
        targets.Add("test2", ["build",], () => test2StartTime = Interlocked.Increment(ref clock));

        // act
        await targets.RunWithoutExitingAsync(["--parallel", "--skip-dependencies", "test1", "test2", "build",]);

        // assert
        Assert.Equal(1, buildStartTime);
        Assert.Equal(5, test1StartTime + test2StartTime);
    }
}
