using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Bullseye.Internal;
using Xunit;
using static BullseyeTests.Infra.Helper;

namespace BullseyeTests
{
    public static class RunningTargets
    {
        [Fact]
        public static async Task Default()
        {
            // arrange
            var (@default, other) = (false, false);

            var targets = new TargetCollection
            {
                CreateTarget("default", () => @default = true),
                CreateTarget(nameof(other), () => other = true),
            };

            // act
            await targets.RunAsync(new List<string>(), default, default, default, default(TextWriter), default);

            // assert
            Assert.True(@default);
            Assert.False(other);
        }

        [Fact]
        public static async Task Specific()
        {
            // arrange
            var (first, second, third) = (false, false, false);

            var targets = new TargetCollection
            {
                CreateTarget(nameof(first), () => first = true),
                CreateTarget(nameof(second), () => second = true),
                CreateTarget(nameof(third), () => third = true),
            };

            // act
            await targets.RunAsync(new List<string> { nameof(first), nameof(second) }, default, default, default, default(TextWriter), default);

            // assert
            Assert.True(first);
            Assert.True(second);
            Assert.False(third);
        }

        [Fact]
        public static async Task SingleNonExistent()
        {
            // arrange
            var existing = false;

            var targets = new TargetCollection
            {
                CreateTarget(nameof(existing), () => existing = true),
            };

            // act
            var exception = await Record.ExceptionAsync(() => targets.RunAsync(new List<string> { nameof(existing), "non-existing" }, default, default, default, default(TextWriter), default));

            // assert
            Assert.NotNull(exception);
            Assert.Contains("non-existing", exception.Message, StringComparison.Ordinal);
            Assert.False(existing);
        }

        [Fact]
        public static async Task MultipleNonExistent()
        {
            // arrange
            var existing = false;

            var targets = new TargetCollection
            {
                CreateTarget(nameof(existing), () => existing = true),
            };

            // act
            var exception = await Record.ExceptionAsync(
                () => targets.RunAsync(new List<string> { nameof(existing), "non-existing", "also-non-existing" }, default, default, default, default(TextWriter), default));

            // assert
            Assert.NotNull(exception);
            Assert.Contains("non-existing", exception.Message, StringComparison.Ordinal);
            Assert.Contains("also-non-existing", exception.Message, StringComparison.Ordinal);
            Assert.False(existing);
        }

        [Fact]
        public static async Task DryRun()
        {
            // arrange
            var ran = false;

            var targets = new TargetCollection
            {
                CreateTarget("target", () => ran = true)
            };

            // act
            await targets.RunAsync(new List<string> { "target", "-n" }, default, default, default, default(TextWriter), default);

            // assert
            Assert.False(ran);
        }

        [Fact]
        public static async Task UnknownOption()
        {
            // arrange
            var ran = false;

            var targets = new TargetCollection
            {
                CreateTarget("target", () => ran = true),
            };

            // act
            var exception = await Record.ExceptionAsync(() => targets.RunAsync(new List<string> { "target", "-b" }, default, default, default, default(TextWriter), default));

            // assert
            Assert.NotNull(exception);
            Assert.Contains("Unknown option -b", exception.Message, StringComparison.Ordinal);
            Assert.Contains(". \"--help\" for usage", exception.Message, StringComparison.Ordinal);
            Assert.False(ran);
        }

        [Fact]
        public static async Task UnknownOptions()
        {
            // arrange
            var ran = false;

            var targets = new TargetCollection
            {
                CreateTarget("target", () => ran = true),
            };

            // act
            var exception = await Record.ExceptionAsync(() => targets.RunAsync(new List<string> { "target", "-b", "-z" }, default, default, default, default(TextWriter), default));

            // assert
            Assert.NotNull(exception);
            Assert.Contains("Unknown options -b -z", exception.Message, StringComparison.Ordinal);
            Assert.Contains(". \"--help\" for usage", exception.Message, StringComparison.Ordinal);
            Assert.False(ran);
        }

        [Fact]
        public static async Task Repeated()
        {
            // arrange
            var count = 0;
            var targets = new TargetCollection
            {
                CreateTarget("default", () => ++count),
            };

            // act
            await targets.RunAsync(default, default, default, default, default(TextWriter), default);
            await targets.RunAsync(default, default, default, default, default(TextWriter), default);

            // assert
            Assert.Equal(2, count);
        }
    }
}
