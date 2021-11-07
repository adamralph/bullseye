using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Bullseye.Internal;
using Xunit;
using static BullseyeTests.Infra.Helper;

namespace BullseyeTests
{
    public static class Dependencies
    {
        [Fact]
        public static async Task FlatDependencies()
        {
            // arrange
            var ran = new List<string>();

            var targets = new TargetCollection
            {
                CreateTarget("first", () => ran.Add("first")),
                CreateTarget("second", () => ran.Add("second")),
                CreateTarget("third", new[] { "first", "second" }, () => ran.Add("third")),
            };

            // act
            await targets.RunAsync(new List<string> { "third" }, default, default, default, default(TextWriter), default);

            // assert
            Assert.Equal(3, ran.Count);
            Assert.Equal("first", ran[0]);
            Assert.Equal("second", ran[1]);
            Assert.Equal("third", ran[2]);
        }

        [Fact]
        public static async Task NestedDependencies()
        {
            // arrange
            var ran = new List<string>();

            var targets = new TargetCollection
            {
                CreateTarget("first", () => ran.Add("first")),
                CreateTarget("second", new[] { "first" }, () => ran.Add("second")),
                CreateTarget("third", new[] { "second" }, () => ran.Add("third")),
            };

            // act
            await targets.RunAsync(new List<string> { "third" }, default, default, default, default(TextWriter), default);

            // assert
            Assert.Equal(3, ran.Count);
            Assert.Equal("first", ran[0]);
            Assert.Equal("second", ran[1]);
            Assert.Equal("third", ran[2]);
        }

        [Fact]
        public static async Task DoubleDependency()
        {
            // arrange
            var ran = new List<string>();

            var targets = new TargetCollection
            {
                CreateTarget("first", () => ran.Add("first")),
                CreateTarget("second", new[] { "first", "first" }, () => ran.Add("second")),
            };

            // act
            await targets.RunAsync(new List<string> { "second" }, default, default, default, default(TextWriter), default);

            // assert
            Assert.Equal(2, ran.Count);
            Assert.Equal("first", ran[0]);
            Assert.Equal("second", ran[1]);
        }

        [Fact]
        public static async Task SelfDependency()
        {
            // arrange
            var targets = new TargetCollection
            {
                CreateTarget("first", new[] { "first" }),
            };

            // act
            var exception = await Record.ExceptionAsync(() => targets.RunAsync(new List<string> { "first" }, default, default, default, default(TextWriter), default));

            // assert
            Assert.NotNull(exception);
            Assert.Contains("first -> first", exception.Message, StringComparison.Ordinal);
        }

        [Fact]
        public static async Task MutualDependency()
        {
            // arrange
            var targets = new TargetCollection
            {
                CreateTarget("first", new[] { "second" }),
                CreateTarget("second", new[] { "first" }),
            };

            // act
            var exception = await Record.ExceptionAsync(() => targets.RunAsync(new List<string> { "second" }, default, default, default, default(TextWriter), default));

            // assert
            Assert.NotNull(exception);
            Assert.Contains("first -> second -> first", exception.Message, StringComparison.Ordinal);
        }

        [Fact]
        public static async Task CircularDependency()
        {
            // arrange
            var targets = new TargetCollection
            {
                CreateTarget("first", new[] { "third" }),
                CreateTarget("second", new[] { "first" }),
                CreateTarget("third", new[] { "second" }),
            };

            // act
            var exception = await Record.ExceptionAsync(() => targets.RunAsync(new List<string> { "third" }, default, default, default, default(TextWriter), default));

            // assert
            Assert.NotNull(exception);
            Assert.Contains("first -> third -> second -> first", exception.Message, StringComparison.Ordinal);
        }

        [Fact]
        public static async Task DoubleTransitiveDependency()
        {
            // arrange
            var ran = new List<string>();
            string output = null;

            using (var outputWriter = new StringWriter())
            {
                var targets = new TargetCollection
                {
                    CreateTarget("first", () => ran.Add("first")),
                    CreateTarget("second", new[] { "first" }, () => ran.Add("second")),
                    CreateTarget("third", new[] { "first", "second" }, () => ran.Add("third")),
                };

                // act
                await targets.RunAsync(new List<string> { "third", "--no-color", "--verbose" }, default, default, outputWriter, default, default);

                // assert
                output = outputWriter.ToString();
            }

            Assert.Equal(3, ran.Count);
            Assert.Equal("first", ran[0]);
            Assert.Equal("second", ran[1]);
            Assert.Equal("third", ran[2]);
            _ = Assert.Single(Regex.Matches(output, "first: Walking dependencies..."));
            _ = Assert.Single(Regex.Matches(output, "first: Awaiting..."));
        }

        [Fact]
        public static async Task NotExistentDependencies()
        {
            // arrange
            var anyRan = false;

            var targets = new TargetCollection
            {
                CreateTarget("first", () => anyRan = true),
                CreateTarget("second", new[] { "first", "non-existing" }, () => anyRan = true),
                CreateTarget("third", new[] { "second", "also-non-existing" }, () => anyRan = true),
            };

            // act
            var exception = await Record.ExceptionAsync(() => targets.RunAsync(new List<string> { "third" }, default, default, default, default(TextWriter), default));

            // assert
            Assert.NotNull(exception);
            Assert.Contains("non-existing, required by second", exception.Message, StringComparison.Ordinal);
            Assert.Contains("also-non-existing, required by third", exception.Message, StringComparison.Ordinal);
            Assert.False(anyRan);
        }

        [Fact]
        public static async Task SkippingDependencies()
        {
            // arrange
            var ran = new List<string>();

            var targets = new TargetCollection
            {
                CreateTarget("first", () => ran.Add("first")),
                CreateTarget("second", new[] { "first", "non-existent" }, () => ran.Add("second")),
            };

            // act
            await targets.RunAsync(new List<string> { "second", "-s" }, default, default, default, default(TextWriter), default);

            // assert
            Assert.Contains("second", ran);
            Assert.DoesNotContain("first", ran);
        }

        [Fact]
        public static async Task DependencyOrderWhenSkipping()
        {
            // arrange
            var ran = new List<string>();

            var targets = new TargetCollection
            {
                CreateTarget("first", () => ran.Add("first")),
                CreateTarget("second", new[] { "first" }, () => ran.Add("second")),
            };

            // act
            await targets.RunAsync(new List<string> { "--skip-dependencies", "second", "first" }, default, default, default, default(TextWriter), default);

            // assert
            Assert.Equal(2, ran.Count);
            Assert.Equal("first", ran[0]);
            Assert.Equal("second", ran[1]);
        }

        [Fact]
        public static async Task DependencyOrderWhenParallelAndSkipping()
        {
            // arrange
            var clock = 0;
            var (buildStartTime, test1StartTime, test2StartTime) = (0, 0, 0);

            var targets = new TargetCollection
            {
                CreateTarget(
                    "build",
                    () =>
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(1)); // a weak way to encourage the tests to run first
                        buildStartTime = Interlocked.Increment(ref clock);
                    }),
                CreateTarget("test1", new[] { "build" }, () => test1StartTime = Interlocked.Increment(ref clock)),
                CreateTarget("test2", new[] { "build" }, () => test2StartTime = Interlocked.Increment(ref clock)),
            };

            // act
            await targets.RunAsync(new List<string> { "--parallel", "--skip-dependencies", "test1", "test2", "build" }, default, default, default, default(TextWriter), default);

            // assert
            Assert.Equal(1, buildStartTime);
            Assert.Equal(5, test1StartTime + test2StartTime);
        }
    }
}
