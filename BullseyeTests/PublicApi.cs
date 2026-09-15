using Bullseye;
using BullseyeTests.Infra;
using PublicApiGenerator;
using Xunit;

namespace BullseyeTests;

public static class PublicApi
{
    [Fact]
    public static async Task IsVerified()
    {
        var options = new ApiGeneratorOptions { IncludeAssemblyAttributes = false };

        var received = typeof(Targets).Assembly.GeneratePublicApi(options);

        await Helper.Verify(received);
    }
}
