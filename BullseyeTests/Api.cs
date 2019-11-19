namespace BullseyeTests
{
    using System;
    using System.Linq;
    using Bullseye;
    using BullseyeTests.Infra;
    using PublicApiGenerator;
    using Xunit;

    public class Api
    {
        [Fact]
        public void IsUnchanged() =>
            AssertFile.Contains(
                "api.txt",
                typeof(Targets).Assembly
                    .GeneratePublicApi(
                        new ApiGeneratorOptions
                        {
                            IncludeTypes = typeof(Targets).Assembly.GetExportedTypes()
                                .Where(type => type.Namespace != null && !type.Namespace.Contains("Internal"))
                                .ToArray()
                        })
                    .Replace(Environment.NewLine, "\r\n"));
    }
}
