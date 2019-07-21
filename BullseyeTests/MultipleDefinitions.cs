namespace BullseyeTests
{
    using System.Collections.Generic;
    using Bullseye.Internal;
    using Xbehave;
    using Xunit;
    using static BullseyeTests.Infra.Helper;

    public class MultipleDefinitions
    {
        [Scenario]
        public void TwoActions(TargetCollection targets, bool firstAction, bool secondAction)
        {
            "Given a target with an action"
                .x(() => Ensure(ref targets).Add(CreateTarget("default", () => firstAction = true)));

            "And a second definition of the target with another action"
                .x(() => targets.Add(CreateTarget("default", () => secondAction = true)));

            "When I run the target"
                .x(() => targets.RunAsync(new List<string>(), default));

            "Then the first action is invoked"
                .x(() => Assert.True(firstAction));

            "And the second action is invoked"
                .x(() => Assert.True(secondAction));
        }

        [Scenario]
        public void TwoActionsWithInputs(
            TargetCollection targets, string firstFirst, string firstSecond, int secondFirst, int secondSecond)
        {
            "Given a target with an action with inputs"
                .x(() => Ensure(ref targets).Add(CreateTarget(
                    "default",
                    new[] { "one", "two" },
                    o =>
                    {
                        if (o == "one")
                        {
                            firstFirst = o;
                        }
                        else
                        {
                            firstSecond = o;
                        }
                    })));

            "And a second definition of the target with another action with inputs"
                .x(() => targets.Add(CreateTarget(
                    "default",
                    new[] { 1, 2 },
                    o =>
                    {
                        if (o == 1)
                        {
                            secondFirst = o;
                        }
                        else
                        {
                            secondSecond = o;
                        }
                    })));

            "When I run the target"
                .x(() => targets.RunAsync(new List<string>(), default));

            "Then the first action is invoked for the first input"
                .x(() => Assert.Equal("one", firstFirst));

            "And the first action is invoked for the second input"
                .x(() => Assert.Equal("two", firstSecond));

            "And the second action is invoked for the first input"
                .x(() => Assert.Equal(1, secondFirst));

            "And the second action is invoked for the second input"
                .x(() => Assert.Equal(2, secondSecond));
        }

        [Scenario]
        public void TwoSetsOfDependencies(TargetCollection targets, bool first, bool second)
        {
            "Given a target"
                .x(() => Ensure(ref targets).Add(CreateTarget("first", () => first = true)));

            "And a second target"
                .x(() => targets.Add(CreateTarget("second", () => second = true)));

            "And a third target which depends on the first target"
                .x(() => targets.Add(CreateTarget("third", new[] { "first" })));

            "And another definition of the third target which depends on the second target"
                .x(() => targets.Add(CreateTarget("third", new[] { "second" })));

            "When I run the third target"
                .x(() => targets.RunAsync(new[] { "third" }, default));

            "Then the first target is run"
                .x(() => Assert.True(first));

            "And the second target is run"
                .x(() => Assert.True(second));
        }
    }
}
