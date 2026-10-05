using Bullseye;
using Fixtures.Xunit;
using PublicApiGenerator;
using Xunit;

namespace Tests;

public static class PublicApi
{
    [Fact]
    public static async Task Surface()
    {
        var options = new ApiGeneratorOptions { IncludeAssemblyAttributes = false };

        var publicApi = typeof(Targets).Assembly.GeneratePublicApi(options);

        await Assert.Expected(publicApi);
    }
}
