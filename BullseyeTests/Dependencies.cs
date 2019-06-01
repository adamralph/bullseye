namespace BullseyeTests
{
    using System;
    using System.Threading;
    using System.Collections.Generic;
    using Bullseye.Internal;
    using Xbehave;
    using Xunit;
    using static BullseyeTests.Infra.Helper;

    public class Dependencies
    {
        [Scenario]
        public void FlatDependencies(TargetCollection targets, List<string> ran)
        {
            "Given a target"
                .x(() => Ensure(ref targets).Add(CreateTarget("first", () => Ensure(ref ran).Add("first"))));

            "And a second target"
                .x(() => targets.Add(CreateTarget("second", () => Ensure(ref ran).Add("second"))));

            "And a third target which depends on the first and second target"
                .x(() => targets.Add(CreateTarget("third", new[] { "first", "second" }, () => Ensure(ref ran).Add("third"))));

            "When I run the third target"
                .x(() => targets.RunAsync(new List<string> { "third" }, default));

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
        public void NestedDependencies(TargetCollection targets, List<string> ran)
        {
            "Given a target"
                .x(() => Ensure(ref targets).Add(CreateTarget("first", () => Ensure(ref ran).Add("first"))));

            "And a second target which depends on the first target"
                .x(() => targets.Add(CreateTarget("second", new[] { "first" }, () => Ensure(ref ran).Add("second"))));

            "And a third target which depends on the second target"
                .x(() => targets.Add(CreateTarget("third", new[] { "second" }, () => Ensure(ref ran).Add("third"))));

            "When I run the third target"
                .x(() => targets.RunAsync(new List<string> { "third" }, default));

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
        public void DoubleDependency(TargetCollection targets, List<string> ran)
        {
            "Given a target"
                .x(() => Ensure(ref targets).Add(CreateTarget("first", () => Ensure(ref ran).Add("first"))));

            "And a second target which depends on the first target twice"
                .x(() => targets.Add(CreateTarget("second", new[] { "first", "first" }, () => Ensure(ref ran).Add("second"))));

            "When I run the second target"
                .x(() => targets.RunAsync(new List<string> { "second" }, default));

            "Then both targets are run once"
                .x(() => Assert.Equal(2, ran.Count));

            "And the first target is run first"
                .x(() => Assert.Equal("first", ran[0]));

            "And the second target is run second"
                .x(() => Assert.Equal("second", ran[1]));
        }

        [Scenario]
        public void SelfDependency(TargetCollection targets, Exception exception)
        {
            "Given a target which depends on itself"
                .x(() => Ensure(ref targets).Add(CreateTarget("first", new[] { "first" })));

            "When I run the target"
                .x(async () => exception = await Record.ExceptionAsync(() => targets.RunAsync(new List<string> { "first" }, default)));

            "Then the operation fails"
                .x(() => Assert.NotNull(exception));

            "And I am told that the circular dependency was detected"
                .x(() => Assert.Contains("first -> first", exception.Message));
        }

        [Scenario]
        public void MutualDependency(TargetCollection targets, Exception exception)
        {
            "Given a target which depends on a second target"
                .x(() => Ensure(ref targets).Add(CreateTarget("first", new[] { "second" })));

            "And the other target depends on the first target"
                .x(() => targets.Add(CreateTarget("second", new[] { "first" })));

            "When I run the second target"
                .x(async () => exception = await Record.ExceptionAsync(() => targets.RunAsync(new List<string> { "second" }, default)));

            "Then the operation fails"
                .x(() => Assert.NotNull(exception));

            "And I am told that the circular dependency was detected"
                .x(() => Assert.Contains("first -> second -> first", exception.Message));
        }

        [Scenario]
        public void CircularDependency(TargetCollection targets, Exception exception)
        {
            "Given a target which depends on a third target"
                .x(() => Ensure(ref targets).Add(CreateTarget("first", new[] { "third" })));

            "And a second target which depends on the first target"
                .x(() => targets.Add(CreateTarget("second", new[] { "first" })));

            "And a third target which depends on the second target"
                .x(() => targets.Add(CreateTarget("third", new[] { "second" })));

            "When I run the third target"
                .x(async () => exception = await Record.ExceptionAsync(() => targets.RunAsync(new List<string> { "third" }, default)));

            "Then the operation fails"
                .x(() => Assert.NotNull(exception));

            "And I am told that the circular dependency was detected"
                .x(() => Assert.Contains("first -> third -> second -> first", exception.Message));
        }

        [Scenario]
        public void DoubleTransitiveDependency(TargetCollection targets, List<string> ran)
        {
            "Given a target"
                .x(() => Ensure(ref targets).Add(CreateTarget("first", () => Ensure(ref ran).Add("first"))));

            "And a second target which depends on the first target"
                .x(() => targets.Add(CreateTarget("second", new[] { "first" }, () => Ensure(ref ran).Add("second"))));

            "And a third target which depends on the first target and the second target"
                .x(() => targets.Add(CreateTarget("third", new[] { "first", "second" }, () => Ensure(ref ran).Add("third"))));

            "When I run the third target"
                .x(() => targets.RunAsync(new List<string> { "third" }, default));

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
        public void NotExistentDependencies(TargetCollection targets, bool anyRan, Exception exception)
        {
            "Given a target"
                .x(() => Ensure(ref targets).Add(CreateTarget("first", () => anyRan = true)));

            "And a second target which depends on the first target and a non-existent target"
                .x(() => targets.Add(CreateTarget("second", new[] { "first", "non-existing" }, () => anyRan = true)));

            "And a third target which depends on the second target and another non-existent target"
                .x(() => targets.Add(CreateTarget("third", new[] { "second", "also-non-existing" }, () => anyRan = true)));

            "When I run the third target"
                .x(async () => exception = await Record.ExceptionAsync(() => targets.RunAsync(new List<string> { "third" }, default)));

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
        public void SkippingDependencies(TargetCollection targets, List<string> ran)
        {
            "Given a target"
                .x(() => Ensure(ref targets).Add(CreateTarget("first", () => Ensure(ref ran).Add("first"))));

            "And a second target which depends on the first target and a non-existent target"
                .x(() => targets.Add(CreateTarget("second", new[] { "first", "non-existent" }, () => Ensure(ref ran).Add("second"))));

            "When I run the second target, skipping dependencies"
                .x(() => targets.RunAsync(new List<string> { "second", "-s" }, default));

            "Then the second target is run"
                .x(() => Assert.Contains("second", ran));

            "But the first target is not run"
                .x(() => Assert.DoesNotContain("first", ran));
        }

        [Scenario]
        public void DependencyOrderWhenSkipping(TargetCollection targets, List<string> ran)
        {
            "Given a target"
                .x(() => Ensure(ref targets).Add(CreateTarget("first", () => Ensure(ref ran).Add("first"))));

            "And a second target which depends on the first target"
                .x(() => targets.Add(CreateTarget("second", new[] { "first" }, () => Ensure(ref ran).Add("second"))));

            "When I run the second and first targets, skipping dependencies"
                .x(() => targets.RunAsync(new List<string> { "--skip-dependencies", "second", "first" }, default));

            "Then all targets are run"
                .x(() => Assert.Equal(2, ran.Count));

            "And the first target is run first"
                .x(() => Assert.Equal("first", ran[0]));

            "And the second target is run second"
                .x(() => Assert.Equal("second", ran[1]));
        }

        [Scenario]
        public void DependencyOrderWhenParallelAndSkipping(
            TargetCollection targets,
            int clock,
            int buildStartTime,
            int test1StartTime,
            int test2StartTime)
        {
            "Given a target that takes a long time to start up"
                .x(() => Ensure(ref targets).Add(CreateTarget(
                    "build", 
                    () => {
                        Thread.Sleep(TimeSpan.FromSeconds(1)); // a weak way to encourage the tests to run first
                        buildStartTime = Interlocked.Increment(ref clock);
                    })));

            "And a second target which depends on the first target"
                .x(() => targets.Add(CreateTarget("test1", new[] { "build" }, () => test1StartTime = Interlocked.Increment(ref clock))));

            "And a third target which depends on the first target"
                .x(() => targets.Add(CreateTarget("test2", new[] { "build" }, () => test2StartTime = Interlocked.Increment(ref clock))));

            "When I run all the targets with parallelism, skipping dependencies"
                .x(() => targets.RunAsync(new List<string> { "--parallel", "--skip-dependencies", "test1", "test2", "build" }, default));

            "Then the first target is run first"
                .x(() => Assert.Equal(1, buildStartTime));

            "And the other targets are run later"
                .x(() => Assert.Equal(5, test1StartTime + test2StartTime));
        }
    }
}
