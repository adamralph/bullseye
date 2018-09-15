#if NETCOREAPP2_1
namespace BullseyeTests
{
    using System;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using ApprovalTests;
    using ApprovalTests.Namers.StackTraceParsers;
    using ApprovalTests.Reporters;
    using ApprovalTests.StackTraceParsers;
    using Bullseye;
    using PublicApiGenerator;
    using Xunit;
    using Xunit.Sdk;

    public class Api
    {
        static Api() => StackTraceParser.AddParser(new WindowsFactStackTraceParser());

        [WindowsFact]
        [UseReporter(typeof(QuietReporter))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void IsUnchanged() =>
            Approvals.Verify(
                ApiGenerator.GeneratePublicApi(
                    typeof(Targets).Assembly,
                    typeof(Targets).Assembly.GetExportedTypes().Where(type => !type.Namespace.Contains("Internal")).ToArray()));

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
        [XunitTestCaseDiscoverer("Xunit.Sdk.FactDiscoverer", "xunit.execution.{Platform}")]
        class WindowsFactAttribute : FactAttribute
        {
            public WindowsFactAttribute() : base() =>
                this.Skip = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? null : "Only works on Windows.";
        }

        class WindowsFactStackTraceParser : XUnitStackTraceParser
        {
            public override string GetAttributeType() => "BullseyeTests.Api+WindowsFactAttribute";
        }
    }
}
#endif
