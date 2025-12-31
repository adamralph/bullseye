using Bullseye;
using PublicApiGenerator;
using Xunit;

namespace BullseyeTests;

public static class PublicApi
{
    [Fact]
    public static async Task IsVerified()
    {
        var options = new ApiGeneratorOptions { IncludeAssemblyAttributes = false };

        var publicApi = typeof(Targets).Assembly.GeneratePublicApi(options);

        _ = await Verify(publicApi);
    }
}
