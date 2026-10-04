using Bullseye.Internal;
using Tests.Fixtures;
using Xunit;

namespace Tests;

public static class EnumerableInputs
{
    [Fact]
    public static async Task WithInputs()
    {
        // arrange
        var inputsReceived = new List<int>();

        var targets = new TargetCollection
        {
            Target.Create("default", [1, 2,], inputsReceived.Add),
        };

        // act
        await targets.RunAsync([], _ => false, () => "", Console.Out, Console.Error, false);

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

        var targets = new TargetCollection
        {
            Target.Create("default", Enumerable.Empty<object>(), _ => ran = true),
        };

        // act
        await targets.RunAsync([], _ => false, () => "", Console.Out, Console.Error, false);

        // assert
        Assert.False(ran);
    }
}
