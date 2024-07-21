using Bullseye.Internal;
using Xunit;
using static BullseyeTests.Infra.Helper;

namespace BullseyeTests;

public static class CaseInsensitivity
{
    [Fact]
    public static async Task MixingCase()
    {
        // arrange
        var (first, second) = (false, false);

        var targets = new TargetCollection
        {
            CreateTarget("first", () => first = true),
            CreateTarget("second", ["FIRST",], () => second = true),
        };

        // act
        await targets.RunAsync(["SECOND",], _ => false, () => "", Console.Out, Console.Error, false);

        // assert
        Assert.True(first);
        Assert.True(second);
    }
}
