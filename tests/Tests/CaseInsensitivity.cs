using Bullseye.Internal;
using Tests.Fixtures;
using Xunit;

namespace Tests;

public static class CaseInsensitivity
{
    [Fact]
    public static async Task MixingCase()
    {
        // arrange
        var (first, second) = (false, false);

        var targets = new TargetCollection
        {
            Target.Create("first", () => first = true),
            Target.Create("second", ["FIRST",], () => second = true),
        };

        // act
        await targets.RunAsync(["SECOND",], _ => false, () => "", Console.Out, Console.Error, false);

        // assert
        Assert.True(first);
        Assert.True(second);
    }
}
