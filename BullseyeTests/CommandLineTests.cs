using System.Linq;
using Bullseye;
using Xunit;

namespace BullseyeTests
{
    public static class CommandLineTests
    {
        [Fact]
        public static void ParseDefaults()
        {
            // arrange
            var args = Enumerable.Empty<string>();

            // act
            var result = CommandLine.Parse(args);

            // assert
            Assert.False(result.Options.Clear);
            Assert.False(result.Options.DryRun);
            Assert.Null(result.Options.Host);
            Assert.False(result.Options.ListDependencies);
            Assert.False(result.Options.ListInputs);
            Assert.False(result.Options.ListTargets);
            Assert.False(result.Options.ListTree);
            Assert.False(result.Options.NoColor);
            Assert.False(result.Options.NoExtendedChars);
            Assert.False(result.Options.Parallel);
            Assert.False(result.Options.SkipDependencies);
            Assert.False(result.Options.Verbose);

            Assert.False(result.showHelp);
            Assert.Empty(result.Targets);
            Assert.Empty(result.UnknownOptions);
        }

        [Fact]
        public static void ParseShortNames()
        {
            // arrange
            var args = new[]
            {
                "-c",
                "-n",
                "--github-actions",
                "-d",
                "-i",
                "-l",
                "-t",
                "-N",
                "-E",
                "-p",
                "-s",
                "-v",
                "-h",
                "target0",
                "target1",
                "-0",
                "-1",
            };

            // act
            var result = CommandLine.Parse(args);

            // assert
            Assert.True(result.Options.Clear);
            Assert.True(result.Options.DryRun);
            Assert.Equal(Host.GitHubActions, result.Options.Host);
            Assert.True(result.Options.ListDependencies);
            Assert.True(result.Options.ListInputs);
            Assert.True(result.Options.ListTargets);
            Assert.True(result.Options.ListTree);
            Assert.True(result.Options.NoColor);
            Assert.True(result.Options.NoExtendedChars);
            Assert.True(result.Options.Parallel);
            Assert.True(result.Options.SkipDependencies);
            Assert.True(result.Options.Verbose);

            Assert.True(result.showHelp);
            Assert.Equal(new[] { "target0", "target1", }, result.Targets);
            Assert.Equal(new[] { "-0", "-1", }, result.UnknownOptions);
        }

        [Fact]
        public static void ParseLongNames()
        {
            // arrange
            var args = new[]
            {
                "--clear",
                "--dry-run",
                "--github-actions",
                "--list-dependencies",
                "--list-inputs",
                "--list-targets",
                "--list-tree",
                "--no-color",
                "--no-extended-chars",
                "--parallel",
                "--skip-dependencies",
                "--verbose",
                "--help",
                "target0",
                "target1",
                "--unknown0",
                "--unknown1",
            };

            // act
            var result = CommandLine.Parse(args);

            // assert
            Assert.True(result.Options.Clear);
            Assert.True(result.Options.DryRun);
            Assert.Equal(Host.GitHubActions, result.Options.Host);
            Assert.True(result.Options.ListDependencies);
            Assert.True(result.Options.ListInputs);
            Assert.True(result.Options.ListTargets);
            Assert.True(result.Options.ListTree);
            Assert.True(result.Options.NoColor);
            Assert.True(result.Options.NoExtendedChars);
            Assert.True(result.Options.Parallel);
            Assert.True(result.Options.SkipDependencies);
            Assert.True(result.Options.Verbose);

            Assert.True(result.showHelp);
            Assert.Equal(new[] { "target0", "target1", }, result.Targets);
            Assert.Equal(new[] { "--unknown0", "--unknown1", }, result.UnknownOptions);
        }
    }
}
