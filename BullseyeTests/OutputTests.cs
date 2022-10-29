using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Bullseye;
using Bullseye.Internal;
using BullseyeTests.Infra;
using Xunit;

namespace BullseyeTests
{
    public static class OutputTests
    {
        [Fact]
        public static async Task DefaultHost()
        {
            // arrange
            await using var output = new StringWriter();
            var ordinal = 1;

            // act
            foreach (var @bool in new[] { true, false, })
            {
                await Write(output, noColor: true, noExtendedChars: !@bool, host: default, hostForced: @bool, OSPlatform.Create("Unknown"), skipDependencies: @bool, dryRun: @bool, parallel: @bool, verbose: true, new[] { "arg1", "arg2", }, ordinal++);
            }

            // assert
#if NETCOREAPP3_1
            var expectedPath = "../../../output.default.host.netcoreapp3.1.txt";
#endif
#if NET6_0
            var expectedPath = "../../../output.default.host.net6.0.txt";
#endif

            await AssertFile.Contains(expectedPath, output.ToString().Replace(Environment.NewLine, "\r\n", StringComparison.Ordinal));
        }

        [Theory]
        [MemberData(nameof(Hosts))]
        public static async Task AllHosts(Host host)
        {
            // arrange
            await using var output = new StringWriter();
            var ordinal = 1;

            foreach (var noColor in new[] { true, false, })
            {
                foreach (var osPlatform in new List<OSPlatform>
                    {
                        OSPlatform.Create("Unknown"),
                        OSPlatform.Windows,
                        OSPlatform.Linux,
                        OSPlatform.OSX,
                    })
                {
                    await Write(output, noColor, noExtendedChars: false, host, hostForced: true, osPlatform, skipDependencies: true, dryRun: true, parallel: true, verbose: true, args: new List<string>(), ordinal++);
                }
            }

            // assert
#if NETCOREAPP3_1
            var expectedPath = $"../../../output.all.hosts.{host}.netcoreapp3.1.txt";
#endif
#if NET6_0
            var expectedPath = $"../../../output.all.hosts.{host}.net6.0.txt";
#endif

            await AssertFile.Contains(expectedPath, output.ToString().Replace(Environment.NewLine, "\r\n", StringComparison.Ordinal));
        }

        public static IEnumerable<object[]> Hosts() =>
            ((Host[])Enum.GetValues(typeof(Host))).Where(host => host != Host.Automatic).Select(host => new object[] { host, });

        private static async Task Write(
            TextWriter writer, bool noColor, bool noExtendedChars, Host host, bool hostForced, OSPlatform osPlatform, bool skipDependencies, bool dryRun, bool parallel, bool verbose, IReadOnlyCollection<string> args, int ordinal)
        {
            await writer.WriteLineAsync();
            await writer.WriteLineAsync($"noColor: {noColor}");
            await writer.WriteLineAsync($"noExtendedChars: {noExtendedChars}");
            await writer.WriteLineAsync($"host: {host}");
            await writer.WriteLineAsync($"hostForced: {hostForced}");
            await writer.WriteLineAsync($"osPlatform: {osPlatform}");
            await writer.WriteLineAsync($"skipDependencies: {skipDependencies}");
            await writer.WriteLineAsync($"dryRun: {dryRun}");
            await writer.WriteLineAsync($"parallel: {parallel}");
            await writer.WriteLineAsync($"verbose: {verbose}");
            await writer.WriteLineAsync($"args: {string.Join(" ", args)}");
            await writer.WriteLineAsync();

            var output = new Output(writer, TextWriter.Null, args, dryRun, host, hostForced, noColor, noExtendedChars, osPlatform, parallel, () => $"prefix{ordinal}", skipDependencies, verbose);

            await Write(output, dryRun);
        }

        private static async Task Write(Output output, bool dryRun)
        {
            var badInput = new Target("badInput", "", Enumerable.Empty<string>());
            var badInputDuration = dryRun ? TimeSpan.Zero : TimeSpan.FromMilliseconds(1.234);
            var badInputEx = new InvalidOperationException("badInputEx");
            var badInputId = Guid.ParseExact("AA123".PadRight(32, '0'), "N");

            var badInputsTarget = new Target("badInputsTarget", "", Enumerable.Empty<string>());

            var badTarget = new Target("badTarget", "", Enumerable.Empty<string>());
            var badTargetDuration = dryRun ? TimeSpan.Zero : TimeSpan.FromMilliseconds(3.456);
            var badTargetEx = new InvalidOperationException("badTargetEx");

            var emptyTargets = Enumerable.Empty<Target>();

            var goodInput1 = new Target("goodInput1", "", Enumerable.Empty<string>());
            var goodInput2 = new Target("goodInput2", "", Enumerable.Empty<string>());
            var goodInputDuration1 = dryRun ? TimeSpan.Zero : TimeSpan.FromSeconds(1.234);
            var goodInputDuration2 = dryRun ? TimeSpan.Zero : TimeSpan.FromSeconds(2.345);
            var goodInputId1 = Guid.ParseExact("BB123".PadRight(32, '0'), "N");
            var goodInputId2 = Guid.ParseExact("BB234".PadRight(32, '0'), "N");

            var goodInputsTarget = new Target("goodInputsTarget", "", Enumerable.Empty<string>());

            var goodTarget1 = new Target("goodTarget1", "", Enumerable.Empty<string>());
            var goodTarget2 = new Target("goodTarget2", "", Enumerable.Empty<string>());
            var goodTargetDuration1 = TimeSpan.Zero;
            var goodTargetDuration2 = dryRun ? TimeSpan.Zero : TimeSpan.FromMinutes(1.234);

            var looseInput = new Target("looseInput", "", Enumerable.Empty<string>());
            var looseInputDuration = TimeSpan.Zero;
            var looseInputId = Guid.ParseExact("CC123".PadRight(32, '0'), "N");

            var looseTarget = new Target("looseTarget", "", Enumerable.Empty<string>());

            var looseTargets = new List<Target> { new Target("looseTarget", "", Enumerable.Empty<string>()), };

            var noInputsTarget = new Target("noInputsTarget", "", Enumerable.Empty<string>());

            var targets = new List<Target>
            {
                new Target("target1", "", Enumerable.Empty<string>()),
                new Target("target2", "", Enumerable.Empty<string>()),
                new Target("target3", "", Enumerable.Empty<string>()),
            };

            var verboseTargets = new Queue<Target>(
                new[]
                {
                    new Target("verboseTarget1", "", Enumerable.Empty<string>()),
                    new Target("verboseTarget2", "", Enumerable.Empty<string>()),
                    new Target("verboseTarget3", "", Enumerable.Empty<string>()),
                });

            var version = "version";

            await output.Header(() => version);

            await output.Awaiting(verboseTargets.Last(), verboseTargets);
            await output.WalkingDependencies(verboseTargets.Last(), verboseTargets);

            await output.Succeeded(emptyTargets);

            await output.Succeeded(looseTarget, looseInput, looseInputId, verboseTargets, looseInputDuration);
            await output.Succeeded(looseTargets);

            await output.Starting(targets);
            {
                await output.NoInputs(noInputsTarget, verboseTargets);

                await output.Succeeded(goodTarget1, verboseTargets, goodTargetDuration1);

                await output.Starting(goodTarget2, verboseTargets);
                await output.Succeeded(goodTarget2, verboseTargets, goodTargetDuration2);

                await output.Starting(badTarget, verboseTargets);
                {
                    await output.Error(badTarget, badTargetEx);
                }
                await output.Failed(badTarget, badTargetEx, badTargetDuration, verboseTargets);

                await output.Starting(goodInputsTarget, goodInput1, goodInputId1, verboseTargets);
                await output.Succeeded(goodInputsTarget, goodInput1, goodInputId1, verboseTargets, goodInputDuration1);

                await output.Starting(goodInputsTarget, goodInput2, goodInputId2, verboseTargets);
                await output.Succeeded(goodInputsTarget, goodInput2, goodInputId2, verboseTargets, goodInputDuration2);

                await output.Starting(badInputsTarget, goodInput1, goodInputId1, verboseTargets);
                await output.Succeeded(badInputsTarget, goodInput1, goodInputId1, verboseTargets, goodInputDuration1);

                await output.Starting(badInputsTarget, badInput, badInputId, verboseTargets);
                {
                    await output.Error(badInputsTarget, badInput, badInputEx);
                }
                await output.Failed(badInputsTarget, badInput, badInputId, badInputEx, badInputDuration, verboseTargets);
            }
            await output.Succeeded(targets);
            await output.Failed(targets);
        }
    }
}
