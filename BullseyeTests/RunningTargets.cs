using Bullseye.Internal;
using Xunit;
using static BullseyeTests.Infra.Helper;

namespace BullseyeTests;

public static class RunningTargets
{
    [Fact]
    public static async Task Default()
    {
        // arrange
        var (@default, other) = (false, false);

        var targets = new TargetCollection
        {
            CreateTarget("default", () => @default = true),
            CreateTarget(nameof(other), () => other = true),
        };

        // act
        await targets.RunAsync([], _ => false, () => "", Console.Out, Console.Error, false);

        // assert
        Assert.True(@default);
        Assert.False(other);
    }

    [Fact]
    public static async Task Specific()
    {
        // arrange
        var (first, second, third) = (false, false, false);

        var targets = new TargetCollection
        {
            CreateTarget(nameof(first), () => first = true),
            CreateTarget(nameof(second), () => second = true),
            CreateTarget(nameof(third), () => third = true),
        };

        // act
        await targets.RunAsync([nameof(first), nameof(second),], _ => false, () => "", Console.Out, Console.Error, false);

        // assert
        Assert.True(first);
        Assert.True(second);
        Assert.False(third);
    }

    [Fact]
    public static async Task TargetNameAbbreviation()
    {
        // arrange
        var (foo, bar) = (false, false);

        var targets = new TargetCollection
        {
            CreateTarget(nameof(foo), () => foo = true),
            CreateTarget(nameof(bar), () => bar = true),
        };

        // act
        await targets.RunAsync([nameof(foo)[..1],], _ => false, () => "", Console.Out, Console.Error, false);

        // assert
        Assert.True(foo);
        Assert.False(bar);
    }

    [Fact]
    public static async Task TargetNameAbbreviationWithTargetMatchingAbbreviation()
    {
        // arrange
        var (foo, foo1) = (false, false);

        var targets = new TargetCollection
        {
            CreateTarget(nameof(foo), () => foo = true),
            CreateTarget(nameof(foo1), () => foo1 = true),
        };

        // act
        await targets.RunAsync([nameof(foo),], _ => false, () => "", Console.Out, Console.Error, false);

        // assert
        Assert.True(foo);
        Assert.False(foo1);
    }

    [Fact]
    public static async Task AmbiguousTargetNameAbbreviation()
    {
        // arrange
        var targets = new TargetCollection
        {
            CreateTarget("foo1", () => { }),
            CreateTarget("foo2", () => { }),
        };

        // act
        var exception = await Record.ExceptionAsync(() => targets.RunAsync(["f",], _ => false, () => "", Console.Out, Console.Error, false));

        // assert
        Assert.NotNull(exception);
        Assert.Contains("ambiguous target", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public static async Task SingleNonExistent()
    {
        // arrange
        var existing = false;

        var targets = new TargetCollection
        {
            CreateTarget(nameof(existing), () => existing = true),
        };

        // act
        var exception = await Record.ExceptionAsync(() => targets.RunAsync([nameof(existing), "non-existing",], _ => false, () => "", Console.Out, Console.Error, false));

        // assert
        Assert.NotNull(exception);
        Assert.Contains("non-existing", exception.Message, StringComparison.Ordinal);
        Assert.False(existing);
    }

    [Fact]
    public static async Task MultipleNonExistent()
    {
        // arrange
        var existing = false;

        var targets = new TargetCollection
        {
            CreateTarget(nameof(existing), () => existing = true),
        };

        // act
        var exception = await Record.ExceptionAsync(
            () => targets.RunAsync([nameof(existing), "non-existing", "also-non-existing",], _ => false, () => "", Console.Out, Console.Error, false));

        // assert
        Assert.NotNull(exception);
        Assert.Contains("non-existing", exception.Message, StringComparison.Ordinal);
        Assert.Contains("also-non-existing", exception.Message, StringComparison.Ordinal);
        Assert.False(existing);
    }

    [Fact]
    public static async Task DryRun()
    {
        // arrange
        var ran = false;

        var targets = new TargetCollection
        {
            CreateTarget("target", () => ran = true),
        };

        // act
        await targets.RunAsync(["target", "-n",], _ => false, () => "", Console.Out, Console.Error, false);

        // assert
        Assert.False(ran);
    }

    [Fact]
    public static async Task UnknownOption()
    {
        // arrange
        var ran = false;

        var targets = new TargetCollection
        {
            CreateTarget("target", () => ran = true),
        };

        // act
        var exception = await Record.ExceptionAsync(() => targets.RunAsync(["target", "-b",], _ => false, () => "", Console.Out, Console.Error, false));

        // assert
        Assert.NotNull(exception);
        Assert.Contains("Unknown option -b", exception.Message, StringComparison.Ordinal);
        Assert.Contains(". \"--help\" for usage", exception.Message, StringComparison.Ordinal);
        Assert.False(ran);
    }

    [Fact]
    public static async Task UnknownOptions()
    {
        // arrange
        var ran = false;

        var targets = new TargetCollection
        {
            CreateTarget("target", () => ran = true),
        };

        // act
        var exception = await Record.ExceptionAsync(() => targets.RunAsync(["target", "-b", "-z",], _ => false, () => "", Console.Out, Console.Error, false));

        // assert
        Assert.NotNull(exception);
        Assert.Contains("Unknown options -b -z", exception.Message, StringComparison.Ordinal);
        Assert.Contains(". \"--help\" for usage", exception.Message, StringComparison.Ordinal);
        Assert.False(ran);
    }

    [Fact]
    public static async Task Repeated()
    {
        // arrange
        var count = 0;
        var targets = new TargetCollection
        {
            CreateTarget("default", () => ++count),
        };

        // act
        await targets.RunAsync(["default",], _ => false, () => "", Console.Out, Console.Error, false);
        await targets.RunAsync(["default",], _ => false, () => "", Console.Out, Console.Error, false);

        // assert
        Assert.Equal(2, count);
    }
}
