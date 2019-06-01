namespace BullseyeTests
{
    using System;
    using System.Collections.Generic;
    using Bullseye.Internal;
    using Xbehave;
    using Xunit;
    using static BullseyeTests.Infra.Helper;

    public class RunningTargets
    {
        [Scenario]
        public void Default(TargetCollection targets, bool @default, bool other)
        {
            "Given a default target"
                .x(() => Ensure(ref targets).Add(CreateTarget("default", () => @default = true)));

            "And another target"
                .x(() => targets.Add(CreateTarget(nameof(other), () => other = true)));

            "When I run without specifying any target names"
                .x(() => targets.RunAsync(new List<string>(), default));

            "Then the default target is run"
                .x(() => Assert.True(@default));

            "But the other target is not run"
                .x(() => Assert.False(other));
        }

        [Scenario]
        public void Specific(TargetCollection targets, bool first, bool second, bool third)
        {
            "Given three targets"
                .x(() => targets = new TargetCollection
                {
                    CreateTarget(nameof(first), () => first = true),
                    CreateTarget(nameof(second), () => second = true),
                    CreateTarget(nameof(third), () => third = true),
                });

            "When I run the first two targets"
                .x(() => targets.RunAsync(new List<string> { nameof(first), nameof(second) }, default));

            "Then the first target is run"
                .x(() => Assert.True(first));

            "And the second target is run"
                .x(() => Assert.True(second));

            "But the third target is not run"
                .x(() => Assert.False(third));
        }

        [Scenario]
        public void SingleNonExistent(TargetCollection targets, bool existing, Exception exception)
        {
            "Given an existing target"
                .x(() => Ensure(ref targets).Add(CreateTarget(nameof(existing), () => existing = true)));

            "When I run that target and a non-existent target"
                .x(async () => exception = await Record.ExceptionAsync(() => targets.RunAsync(new List<string> { nameof(existing), "non-existing" }, default)));

            "Then the operation fails"
                .x(() => Assert.NotNull(exception));

            "Then I am told that the non-existent target could not be found"
                .x(() => Assert.Contains("non-existing", exception.Message));

            "And the existing target is not run"
                .x(() => Assert.False(existing));
        }

        [Scenario]
        public void MultipleNonExistent(
            TargetCollection targets, bool existing, Exception exception)
        {
            "Given an existing target"
                .x(() => Ensure(ref targets).Add(CreateTarget(nameof(existing), () => existing = true)));

            "When I run that target and two non-existent targets"
                .x(async () => exception = await Record.ExceptionAsync(() => targets.RunAsync(
                    new List<string> { nameof(existing), "non-existing", "also-non-existing" }, default)));

            "Then the operation fails"
                .x(() => Assert.NotNull(exception));

            "Then I am told that the first non-existent target could not be found"
                .x(() => Assert.Contains("non-existing", exception.Message));

            "Then I am told that the second non-existent target could not be found"
                .x(() => Assert.Contains("also-non-existing", exception.Message));

            "And the existing target is not run"
                .x(() => Assert.False(existing));
        }

        [Scenario]
        public void DryRun(TargetCollection targets, bool ran)
        {
            "Given a target"
                .x(() => Ensure(ref targets).Add(CreateTarget("target", () => ran = true)));

            "When I run the target specifying a dry run"
                .x(() => targets.RunAsync(new List<string> { "target", "-n" }, default));

            "Then the target is not run"
                .x(() => Assert.False(ran));
        }

        [Scenario]
        public void UnknownOption(TargetCollection targets, bool ran, Exception exception)
        {
            "Given a target"
                .x(() => Ensure(ref targets).Add(CreateTarget("target", () => ran = true)));

            "When I run the target specifying an unknown option"
                .x(async () => exception = await Record.ExceptionAsync(() => targets.RunAsync(new List<string> { "target", "-b" }, default)));

            "Then the operation fails"
                .x(() => Assert.NotNull(exception));

            "Then I am told that the option is unknown"
                .x(() => Assert.Contains("Unknown option -b", exception.Message));

            "Then I am told how to get help"
                .x(() => Assert.Contains(". \"--help\" for usage", exception.Message));

            "And the target is not run"
                .x(() => Assert.False(ran));
        }

        [Scenario]
        public void UnknownOptions(TargetCollection targets, bool ran, Exception exception)
        {
            "Given a target"
                .x(() => Ensure(ref targets).Add(CreateTarget("target", () => ran = true)));

            "When I run the target specifying unknown options"
                .x(async () => exception = await Record.ExceptionAsync(() => targets.RunAsync(new List<string> { "target", "-b", "-z" }, default)));

            "Then the operation fails"
                .x(() => Assert.NotNull(exception));

            "Then I am told that the option is unknown"
                .x(() => Assert.Contains("Unknown options -b -z", exception.Message));

            "Then I am told how to get help"
                .x(() => Assert.Contains(". \"--help\" for usage", exception.Message));

            "And the target is not run"
                .x(() => Assert.False(ran));
        }
    }
}
