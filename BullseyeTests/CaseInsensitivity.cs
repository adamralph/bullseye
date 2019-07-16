namespace BullseyeTests
{
    using Bullseye.Internal;
    using Xbehave;
    using Xunit;
    using static BullseyeTests.Infra.Helper;

    public class CaseInsensitivity
    {
        [Scenario]
        public void MixingCase(TargetCollection targets, bool first, bool second)
        {
            "Given a target"
                .x(() => Ensure(ref targets).Add(CreateTarget("first", () => first = true)));

            "And another target which depends on the first target with a different case"
                .x(() => targets.Add(CreateTarget("second", new[] { "FIRST" }, () => second = true)));

            "When I run the second target with a different case"
                .x(() => targets.RunAsync(new[] { "SECOND" }, default));

            "Then the first target is run"
                .x(() => Assert.True(first));

            "And the second target is run"
                .x(() => Assert.True(second));
        }
    }
}
