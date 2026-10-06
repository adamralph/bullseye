using Bullseye;
using Xunit;

namespace Tests;

public static class RunningTargets
{
    [Fact]
    public static async Task Default()
    {
        // arrange
        var (@default, other) = (false, false);

        var targets = new Targets();
        targets.Add("default", () => @default = true);
        targets.Add(nameof(other), () => other = true);

        // act
        await targets.RunWithoutExitingAsync([]);

        // assert
        Assert.True(@default);
        Assert.False(other);
    }

    [Fact]
    public static async Task Specific()
    {
        // arrange
        var (first, second, third) = (false, false, false);

        var targets = new Targets();
        targets.Add(nameof(first), () => first = true);
        targets.Add(nameof(second), () => second = true);
        targets.Add(nameof(third), () => third = true);

        // act
        await targets.RunWithoutExitingAsync([nameof(first), nameof(second),]);

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

        var targets = new Targets();
        targets.Add(nameof(foo), () => foo = true);
        targets.Add(nameof(bar), () => bar = true);

        // act
        await targets.RunWithoutExitingAsync([nameof(foo)[..1],]);

        // assert
        Assert.True(foo);
        Assert.False(bar);
    }

    [Fact]
    public static async Task TargetNameAbbreviationWithTargetMatchingAbbreviation()
    {
        // arrange
        var (foo, foo1) = (false, false);

        var targets = new Targets();
        targets.Add(nameof(foo), () => foo = true);
        targets.Add(nameof(foo1), () => foo1 = true);

        // act
        await targets.RunWithoutExitingAsync([nameof(foo),]);

        // assert
        Assert.True(foo);
        Assert.False(foo1);
    }

    [Fact]
    public static async Task AmbiguousTargetNameAbbreviation()
    {
        // arrange
        var targets = new Targets();
        targets.Add("foo1", () => { });
        targets.Add("foo2", () => { });

        // act
        var exception = await Record.ExceptionAsync(() => targets.RunWithoutExitingAsync(["f",]));

        // assert
        Assert.NotNull(exception);
        Assert.Contains("ambiguous target", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public static async Task SingleNonExistent()
    {
        // arrange
        var existing = false;

        var targets = new Targets();
        targets.Add(nameof(existing), () => existing = true);

        // act
        var exception = await Record.ExceptionAsync(() => targets.RunWithoutExitingAsync([nameof(existing), "non-existing",]));

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

        var targets = new Targets();
        targets.Add(nameof(existing), () => existing = true);

        // act
        var exception = await Record.ExceptionAsync(
            () => targets.RunWithoutExitingAsync([nameof(existing), "non-existing", "also-non-existing",]));

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

        var targets = new Targets();
        targets.Add("target", () => ran = true);

        // act
        await targets.RunWithoutExitingAsync(["target", "-n",]);

        // assert
        Assert.False(ran);
    }

    [Fact]
    public static async Task UnknownOption()
    {
        // arrange
        var ran = false;

        var targets = new Targets();
        targets.Add("target", () => ran = true);

        // act
        var exception = await Record.ExceptionAsync(() => targets.RunWithoutExitingAsync(["target", "-b",]));

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

        var targets = new Targets();
        targets.Add("target", () => ran = true);

        // act
        var exception = await Record.ExceptionAsync(() => targets.RunWithoutExitingAsync(["target", "-b", "-z",]));

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
        var targets = new Targets();
        targets.Add("default", () => ++count);

        // act
        await targets.RunWithoutExitingAsync(["default",]);
        await targets.RunWithoutExitingAsync(["default",]);

        // assert
        Assert.Equal(2, count);
    }
}
