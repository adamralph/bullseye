using Bullseye;
using Xunit;

namespace Tests;

public static class CaseInsensitivity
{
    [Fact]
    public static async Task MixingCase()
    {
        // arrange
        var (first, second) = (false, false);

        var targets = new Targets();
        targets.Add("first", () => first = true);
        targets.Add("second", ["FIRST",], () => second = true);

        // act
        await targets.RunWithoutExitingAsync(["SECOND"]);

        // assert
        Assert.True(first);
        Assert.True(second);
    }
}
