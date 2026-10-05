using Bullseye;
using Xunit;

namespace Tests;

public static class EnumerableInputs
{
    [Fact]
    public static async Task WithInputs()
    {
        // arrange
        var inputsReceived = new List<int>();

        var targets = new Targets();
        targets.Add("default", [1, 2,], inputsReceived.Add);

        // act
        await targets.RunWithoutExitingAsync([]);

        // assert
        Assert.Equal(2, inputsReceived.Count);
        Assert.Equal(1, inputsReceived[0]);
        Assert.Equal(2, inputsReceived[1]);
    }

    [Fact]
    public static async Task WithoutInputs()
    {
        // arrange
        var ran = false;

        var targets = new Targets();
        targets.Add("default", Enumerable.Empty<object>(), _ => ran = true);

        // act
        await targets.RunWithoutExitingAsync([]);

        // assert
        Assert.False(ran);
    }
}
