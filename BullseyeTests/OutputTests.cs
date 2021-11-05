using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Bullseye;
using Bullseye.Internal;
using BullseyeTests.Infra;
using Xunit;
using OperatingSystem = Bullseye.Internal.OperatingSystem;

namespace BullseyeTests
{
    public static class OutputTests
    {
        [Fact]
        public static async Task Output()
        {
            // arrange
            using var output = new StringWriter();
            var ordinal = 1;

            // act
            foreach (var @bool in new[] { true, false })
            {
                await Write(output, noColor: true, noExtendedChars: !@bool, default, hostDetected: @bool, default, skipDependencies: @bool, dryRun: @bool, parallel: @bool, verbose: true, new[] { "arg1", "arg2" }, ordinal++);
            }

            foreach (var noColor in new[] { true, false })
            {
                foreach (var host in (Host[])Enum.GetValues(typeof(Host)))
                {
                    foreach (var operatingSystem in (OperatingSystem[])Enum.GetValues(typeof(OperatingSystem)))
                    {
                        await Write(output, noColor, noExtendedChars: false, host, hostDetected: false, operatingSystem, skipDependencies: true, dryRun: true, parallel: true, verbose: true, args: new List<string>(), ordinal++);
                    }
                }
            }

            // assert
            await AssertFile.Contains("../../../output.txt", output.ToString().Replace(Environment.NewLine, "\r\n", StringComparison.Ordinal));
        }

        private static async Task Write(
            StringWriter writer, bool noColor, bool noExtendedChars, Host host, bool hostDetected, OperatingSystem operatingSystem, bool skipDependencies, bool dryRun, bool parallel, bool verbose, IReadOnlyCollection<string> args, int ordinal)
        {
            await writer.WriteLineAsync();
            await writer.WriteLineAsync($"noColor: {noColor}");
            await writer.WriteLineAsync($"noExtendedChars: {noExtendedChars}");
            await writer.WriteLineAsync($"host: {host}");
            await writer.WriteLineAsync($"hostDetected: {hostDetected}");
            await writer.WriteLineAsync($"operatingSystem: {operatingSystem}");
            await writer.WriteLineAsync($"skipDependencies: {skipDependencies}");
            await writer.WriteLineAsync($"dryRun: {dryRun}");
            await writer.WriteLineAsync($"parallel: {parallel}");
            await writer.WriteLineAsync($"verbose: {verbose}");
            await writer.WriteLineAsync($"args: {string.Join(" ", args)}");
            await writer.WriteLineAsync();

            var output = new Output(writer, args, dryRun, host, hostDetected, noColor, noExtendedChars, operatingSystem, parallel, $"prefix{ordinal}", skipDependencies, verbose);

            await Write(output, dryRun);
        }

        private static async Task Write(Output output, bool dryRun)
        {
            var badInput = new Target("badInput", default, default);
            var badInputDuration = dryRun ? (TimeSpan?)null : TimeSpan.FromMilliseconds(1.234);
            var badInputEx = new InvalidOperationException("badInputEx");
            var badInputId = Guid.ParseExact("AA123".PadRight(32, '0'), "N");

            var badInputsTarget = new Target("badInputsTarget", default, default);

            var badTarget = new Target("badTarget", default, default);
            var badTargetDuration = dryRun ? (TimeSpan?)null : TimeSpan.FromMilliseconds(3.456);
            var badTargetEx = new InvalidOperationException("badTargetEx");

            var emptyTargets = Enumerable.Empty<Target>();

            var goodInput1 = new Target("goodInput1", default, default);
            var goodInput2 = new Target("goodInput2", default, default);
            var goodInputDuration1 = dryRun ? (TimeSpan?)null : TimeSpan.FromSeconds(1.234);
            var goodInputDuration2 = dryRun ? (TimeSpan?)null : TimeSpan.FromSeconds(2.345);
            var goodInputId1 = Guid.ParseExact("BB123".PadRight(32, '0'), "N");
            var goodInputId2 = Guid.ParseExact("BB234".PadRight(32, '0'), "N");

            var goodInputsTarget = new Target("goodInputsTarget", default, default);

            var goodTarget1 = new Target("goodTarget1", default, default);
            var goodTarget2 = new Target("goodTarget2", default, default);
            var goodTargetDuration1 = (TimeSpan?)null;
            var goodTargetDuration2 = dryRun ? (TimeSpan?)null : TimeSpan.FromMinutes(1.234);

            var looseInput = new Target("looseInput", default, default);
            var looseInputDuration = (TimeSpan?)null;
            var looseInputId = Guid.ParseExact("CC123".PadRight(32, '0'), "N");

            var looseTarget = new Target("looseTarget", default, default);

            var looseTargets = new List<Target> { new Target("looseTarget", default, default) };

            var noInputsTarget = new Target("noInputsTarget", default, default);

            var targets = new List<Target>
            {
                new Target("target1", default, default),
                new Target("target2", default, default),
                new Target("target3", default, default),
            };

            var verboseTargets = new Queue<Target>(
                new[]
                {
                    new Target("verboseTarget1", default, default),
                    new Target("verboseTarget2", default, default),
                    new Target("verboseTarget3", default, default),
                });

            var version = "version";

            await output.Header(() => version);

            await output.Awaiting(verboseTargets.Last(), verboseTargets);
            await output.WalkingDependencies(verboseTargets.Last(), verboseTargets);

            await output.Succeeded(emptyTargets);

            await output.Succeeded(looseTarget, looseInput, looseInputDuration, looseInputId, verboseTargets);
            await output.Succeeded(looseTargets);

            await output.Starting(targets);
            {
                await output.NoInputs(noInputsTarget, verboseTargets);

                await output.Succeeded(goodTarget1, goodTargetDuration1, verboseTargets);

                await output.Starting(goodTarget2, verboseTargets);
                await output.Succeeded(goodTarget2, goodTargetDuration2, verboseTargets);

                await output.Starting(badTarget, verboseTargets);
                {
                    await output.Error(badTarget, badTargetEx);
                }
                await output.Failed(badTarget, badTargetEx, badTargetDuration, verboseTargets);

                await output.Starting(goodInputsTarget, verboseTargets);
                {
                    await output.Starting(goodInputsTarget, goodInput1, goodInputId1, verboseTargets);
                    await output.Succeeded(goodInputsTarget, goodInput1, goodInputDuration1, goodInputId1, verboseTargets);

                    await output.Starting(goodInputsTarget, goodInput2, goodInputId2, verboseTargets);
                    await output.Succeeded(goodInputsTarget, goodInput2, goodInputDuration2, goodInputId2, verboseTargets);
                }
                await output.Succeeded(goodInputsTarget, verboseTargets);

                await output.Starting(badInputsTarget, verboseTargets);
                {
                    await output.Starting(badInputsTarget, goodInput1, goodInputId1, verboseTargets);
                    await output.Succeeded(badInputsTarget, goodInput1, goodInputDuration1, goodInputId1, verboseTargets);

                    await output.Starting(badInputsTarget, badInput, badInputId, verboseTargets);
                    {
                        await output.Error(badInputsTarget, badInput, badInputEx);
                    }
                    await output.Failed(badInputsTarget, badInput, badInputEx, badInputDuration, badInputId, verboseTargets);
                }
                await output.Failed(badInputsTarget, verboseTargets);
            }
            await output.Succeeded(targets);
            await output.Failed(targets);
        }
    }
}
