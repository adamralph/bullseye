namespace BullseyeTests
{
    using System;
    using System.Collections.Generic;
    using Bullseye.Internal;
    using BullseyeTests.Infra;
    using Xbehave;
    using Xunit;
    using static BullseyeTests.Infra.Helper;

    public class RunningTargets
    {
        [Scenario]
        public void Default(Dictionary<string, Target> targets, TestConsole console, bool @default, bool other)
        {
            "Given a default target"
                .x(() => Ensure(ref targets)["default"] = CreateTarget(() => @default = true));

            "And another target"
                .x(() => targets[nameof(other)] = CreateTarget(() => other = true));

            "When I run without specifying any target names"
                .x(() => targets.RunAsync(new List<string>(), console = new TestConsole()));

            "Then the default target is run"
                .x(() => Assert.True(@default));

            "But the other target is not run"
                .x(() => Assert.False(other));
        }

        [Scenario]
        public void Specific(Dictionary<string, Target> targets, TestConsole console, bool first, bool second, bool third)
        {
            "Given three targets"
                .x(() => targets = new Dictionary<string, Target>
                {
                    [nameof(first)] = CreateTarget(() => first = true),
                    [nameof(second)] = CreateTarget(() => second = true),
                    [nameof(third)] = CreateTarget(() => third = true),
                });

            "When I run the first two targets"
                .x(() => targets.RunAsync(new List<string> { nameof(first), nameof(second) }, console = new TestConsole()));

            "Then the first target is run"
                .x(() => Assert.True(first));

            "And the second target is run"
                .x(() => Assert.True(second));

            "But the third target is not run"
                .x(() => Assert.False(third));
        }

        [Scenario]
        public void SingleNonExistent(Dictionary<string, Target> targets, TestConsole console, bool existing, int exitCode)
        {
            "Given an existing target"
                .x(() => Ensure(ref targets)[nameof(existing)] = CreateTarget(() => existing = true));

            "When I run that target and a non-existent target"
                .x(async () => exitCode = await targets.RunAsync(new List<string> { nameof(existing), "non-existing" }, console = new TestConsole()));

            "Then the operation fails"
                .x(() => Assert.NotEqual(0, exitCode));

            "Then I am told that the non-existent target could not be found"
                .x(() => Assert.Contains("non-existing", console.Error.Read()));

            "And the existing target is not run"
                .x(() => Assert.False(existing));
        }

        [Scenario]
        public void MultipleNonExistent(
            Dictionary<string, Target> targets, TestConsole console, bool existing, int exitCode)
        {
            "Given an existing target"
                .x(() => Ensure(ref targets)[nameof(existing)] = CreateTarget(() => existing = true));

            "When I run that target and two non-existent targets"
                .x(async () => exitCode = await targets.RunAsync(
                    new List<string> { nameof(existing), "non-existing", "also-non-existing" },
                    console = new TestConsole()));

            "Then the operation fails"
                .x(() => Assert.NotEqual(0, exitCode));

            "Then I am told that the first non-existent target could not be found"
                .x(() => Assert.Contains("non-existing", console.Error.Read()));

            "Then I am told that the second non-existent target could not be found"
                .x(() => Assert.Contains("also-non-existing", console.Error.Read()));

            "And the existing target is not run"
                .x(() => Assert.False(existing));
        }

        [Scenario]
        public void DryRun(Dictionary<string, Target> targets, TestConsole console, bool ran)
        {
            "Given a target"
                .x(() => Ensure(ref targets)["target"] = CreateTarget(() => ran = true));

            "When I run the target specifying a dry run"
                .x(() => targets.RunAsync(new List<string> { "target", "-n" }, console = new TestConsole()));

            "Then the target is not run"
                .x(() => Assert.False(ran));
        }
    }
}
