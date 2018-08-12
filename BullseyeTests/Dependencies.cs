namespace BullseyeTests
{
    using System;
    using System.Collections.Generic;
    using Bullseye.Internal;
    using BullseyeTests.Infra;
    using Xbehave;
    using Xunit;
    using static BullseyeTests.Infra.Helper;

    public class Dependencies
    {
        [Scenario]
        public void FlatDependencies(TargetCollection targets, TestConsole console, List<string> ran)
        {
            "Given a target"
                .x(() => Ensure(ref targets).Add(CreateTarget("first", () => Ensure(ref ran).Add("first"))));

            "And a second target"
                .x(() => targets.Add(CreateTarget("second", () => Ensure(ref ran).Add("second"))));

            "And a third target which depends on the first and second target"
                .x(() => targets.Add(CreateTarget("third", new[] { "first", "second" }, () => Ensure(ref ran).Add("third"))));

            "When I run the third target"
                .x(() => targets.RunAsync(new List<string> { "third" }, console = new TestConsole()));

            "Then all targets are run"
                .x(() => Assert.Equal(3, ran.Count));

            "And the first target is run first"
                .x(() => Assert.Equal("first", ran[0]));

            "And the second target is run second"
                .x(() => Assert.Equal("second", ran[1]));

            "And the third target is run third"
                .x(() => Assert.Equal("third", ran[2]));
        }

        [Scenario]
        public void NestedDependencies(TargetCollection targets, TestConsole console, List<string> ran)
        {
            "Given a target"
                .x(() => Ensure(ref targets).Add(CreateTarget("first", () => Ensure(ref ran).Add("first"))));

            "And a second target which depends on the first target"
                .x(() => targets.Add(CreateTarget("second", new[] { "first" }, () => Ensure(ref ran).Add("second"))));

            "And a third target which depends on the second target"
                .x(() => targets.Add(CreateTarget("third", new[] { "second" }, () => Ensure(ref ran).Add("third"))));

            "When I run the third target"
                .x(() => targets.RunAsync(new List<string> { "third" }, console = new TestConsole()));

            "Then all targets are run"
                .x(() => Assert.Equal(3, ran.Count));

            "And the first target is run first"
                .x(() => Assert.Equal("first", ran[0]));

            "And the second target is run second"
                .x(() => Assert.Equal("second", ran[1]));

            "And the third target is run third"
                .x(() => Assert.Equal("third", ran[2]));
        }

        [Scenario]
        public void DoubleDependency(TargetCollection targets, TestConsole console, List<string> ran)
        {
            "Given a target"
                .x(() => Ensure(ref targets).Add(CreateTarget("first", () => Ensure(ref ran).Add("first"))));

            "And a second target which depends on the first target twice"
                .x(() => targets.Add(CreateTarget("second", new[] { "first", "first" }, () => Ensure(ref ran).Add("second"))));

            "When I run the second target"
                .x(() => targets.RunAsync(new List<string> { "second" }, console = new TestConsole()));

            "Then both targets are run once"
                .x(() => Assert.Equal(2, ran.Count));

            "And the first target is run first"
                .x(() => Assert.Equal("first", ran[0]));

            "And the second target is run second"
                .x(() => Assert.Equal("second", ran[1]));
        }

        [Scenario]
        public void SelfDependency(TargetCollection targets, TestConsole console, List<string> ran)
        {
            "Given a target which depends on itself"
                .x(() => Ensure(ref targets).Add(CreateTarget("first", new[] { "first" }, () => Ensure(ref ran).Add("first"))));

            "When I run the target"
                .x(() => targets.RunAsync(new List<string> { "first" }, console = new TestConsole()));

            "Then the target is run once"
                .x(() => Assert.Single(ran));
        }

        [Scenario]
        public void MutualDependency(TargetCollection targets, TestConsole console, List<string> ran)
        {
            "Given a target which depends on a second target"
                .x(() => Ensure(ref targets).Add(CreateTarget("first", new[] { "second" }, () => Ensure(ref ran).Add("first"))));

            "And the other target depends on the first target"
                .x(() => targets.Add(CreateTarget("second", new[] { "first" }, () => Ensure(ref ran).Add("second"))));

            "When I run the second target"
                .x(() => targets.RunAsync(new List<string> { "second" }, console = new TestConsole()));

            "Then both targets are run once"
                .x(() => Assert.Equal(2, ran.Count));

            "And the first target is run first"
                .x(() => Assert.Equal("first", ran[0]));

            "And the second target is run second"
                .x(() => Assert.Equal("second", ran[1]));
        }

        [Scenario]
        public void CircularDependency(TargetCollection targets, TestConsole console, List<string> ran)
        {
            "Given a target which depends on a third target"
                .x(() => Ensure(ref targets).Add(CreateTarget("first", new[] { "third" }, () => Ensure(ref ran).Add("first"))));

            "And a second target which depends on the first target"
                .x(() => targets.Add(CreateTarget("second", new[] { "first" }, () => Ensure(ref ran).Add("second"))));

            "And a third target which depends on the second target"
                .x(() => targets.Add(CreateTarget("third", new[] { "second" }, () => Ensure(ref ran).Add("third"))));

            "When I run the third target"
                .x(() => targets.RunAsync(new List<string> { "third" }, console = new TestConsole()));

            "Then all targets are run"
                .x(() => Assert.Equal(3, ran.Count));

            "And the first target is run first"
                .x(() => Assert.Equal("first", ran[0]));

            "And the second target is run second"
                .x(() => Assert.Equal("second", ran[1]));

            "And the third target is run third"
                .x(() => Assert.Equal("third", ran[2]));
        }

        [Scenario]
        public void DoubleTransitiveDependency(TargetCollection targets, TestConsole console, List<string> ran)
        {
            "Given a target"
                .x(() => Ensure(ref targets).Add(CreateTarget("first", () => Ensure(ref ran).Add("first"))));

            "And a second target which depends on the first target"
                .x(() => targets.Add(CreateTarget("second", new[] { "first" }, () => Ensure(ref ran).Add("second"))));

            "And a third target which depends on the first target and the second target"
                .x(() => targets.Add(CreateTarget("third", new[] { "first", "second" }, () => Ensure(ref ran).Add("third"))));

            "When I run the third target"
                .x(() => targets.RunAsync(new List<string> { "third" }, console = new TestConsole()));

            "Then all targets are run"
                .x(() => Assert.Equal(3, ran.Count));

            "And the first target is run first"
                .x(() => Assert.Equal("first", ran[0]));

            "And the second target is run second"
                .x(() => Assert.Equal("second", ran[1]));

            "And the third target is run third"
                .x(() => Assert.Equal("third", ran[2]));
        }

        [Scenario]
        public void NotExistentDependencies(TargetCollection targets, TestConsole console, bool anyRan, Exception exception)
        {
            "Given a target"
                .x(() => Ensure(ref targets).Add(CreateTarget("first", () => anyRan = true)));

            "And a second target which depends on the first target and a non-existent target"
                .x(() => targets.Add(CreateTarget("second", new[] { "first", "non-existing" }, () => anyRan = true)));

            "And a third target which depends on the second target and another non-existent target"
                .x(() => targets.Add(CreateTarget("third", new[] { "second", "also-non-existing" }, () => anyRan = true)));

            "When I run the third target"
                .x(async () => exception = await Record.ExceptionAsync(() => targets.RunAsync(new List<string> { "third" }, console = new TestConsole())));

            "Then the operation fails"
                .x(() => Assert.NotNull(exception));

            "And I am told that the first non-existent target could not be found"
                .x(() => Assert.Contains("non-existing, required by second", exception.Message));

            "And I am told that the second non-existent target could not be found"
                .x(() => Assert.Contains("also-non-existing, required by third", exception.Message));

            "And the other targets are not run"
                .x(() => Assert.False(anyRan));
        }

        [Scenario]
        public void SkippingDependencies(TargetCollection targets, TestConsole console, List<string> ran)
        {
            "Given a target"
                .x(() => Ensure(ref targets).Add(CreateTarget("first", () => Ensure(ref ran).Add("first"))));

            "And a second target which depends on the first target and a non-existent target"
                .x(() => targets.Add(CreateTarget("second", new[] { "first", "non-existent" }, () => Ensure(ref ran).Add("second"))));

            "When I run the second target, skipping dependencies"
                .x(() => targets.RunAsync(new List<string> { "second", "-s" }, console = new TestConsole()));

            "Then the second target is run"
                .x(() => Assert.Contains("second", ran));

            "But the first target is not run"
                .x(() => Assert.DoesNotContain("first", ran));
        }
    }
}
