#if NETCOREAPP2_1
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
                "../../../api-netcoreapp2_1.txt",
                ApiGenerator
                    .GeneratePublicApi(
                        typeof(Targets).Assembly,
                        typeof(Targets).Assembly.GetExportedTypes().Where(type => !type.Namespace.Contains("Internal")).ToArray())
                    .Replace(Environment.NewLine, "\r\n"));
    }
}
#endif
