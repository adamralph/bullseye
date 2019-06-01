namespace BullseyeTests
{
    using System.Collections.Generic;
    using System.Linq;
    using Bullseye.Internal;
    using Xbehave;
    using Xunit;
    using static BullseyeTests.Infra.Helper;

    public class EnumerableInputs
    {
        [Scenario]
        public void WithInputs(TargetCollection targets, List<int> inputsReceived)
        {
            "Given a target with inputs 1 and 2"
                .x(() => Ensure(ref targets).Add(CreateTarget("default", new[] { 1, 2 }, input => Ensure(ref inputsReceived).Add(input))));

            "When I run the target"
                .x(() => targets.RunAsync(new List<string>(), default));

            "Then the target is run twice"
                .x(() => Assert.Equal(2, inputsReceived.Count));

            "And target was run with 1 first"
                .x(() => Assert.Equal(1, inputsReceived[0]));

            "And target was run with 2 second"
                .x(() => Assert.Equal(2, inputsReceived[1]));
        }

        [Scenario]
        public void WithoutInputs(TargetCollection targets, bool ran)
        {
            "Given a target with missing inputs"
                .x(() => Ensure(ref targets).Add(CreateTarget("default", Enumerable.Empty<object>(), input => ran = true)));

            "When I run the target"
                .x(() => targets.RunAsync(new List<string>(), default));

            "Then the target is not run"
                .x(() => Assert.False(ran));
        }
    }
}
