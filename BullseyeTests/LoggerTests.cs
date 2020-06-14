namespace BullseyeTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using Bullseye;
    using Bullseye.Internal;
    using BullseyeTests.Infra;
    using Xbehave;
    using OperatingSystem = Bullseye.Internal.OperatingSystem;

    public class LoggerTests
    {
        [Scenario]
        public async Task Logging()
        {
            var writer = new StringWriter();

            var ordinal = 1;

            foreach (var @bool in new[] { true, false })
            {
                await Write(writer, noColor: true, noExtendedChars: !@bool, default, default, skipDependencies: @bool, dryRun: @bool, parallel: @bool, verbose: true, ordinal++);
            }

            foreach (var noColor in new[] { true, false })
            {
                foreach (var host in (Host[])Enum.GetValues(typeof(Host)))
                {
                    foreach (var operatingSystem in (OperatingSystem[])Enum.GetValues(typeof(OperatingSystem)))
                    {
                        await Write(writer, noColor, noExtendedChars: false, host, operatingSystem, skipDependencies: true, dryRun: true, parallel: true, verbose: true, ordinal++);
                    }
                }
            }

            AssertFile.Contains("log.txt", writer.ToString().Replace(Environment.NewLine, "\r\n"));
        }

        private static async Task Write(
            StringWriter writer, bool noColor, bool noExtendedChars, Host host, OperatingSystem operatingSystem, bool skipDependencies, bool dryRun, bool parallel, bool verbose, int ordinal)
        {
            await writer.WriteLineAsync();
            await writer.WriteLineAsync($"noColor: {noColor}");
            await writer.WriteLineAsync($"noExtendedChars: {noExtendedChars}");
            await writer.WriteLineAsync($"host: {host}");
            await writer.WriteLineAsync($"operatingSystem: {operatingSystem}");
            await writer.WriteLineAsync($"skipDependencies: {skipDependencies}");
            await writer.WriteLineAsync($"dryRun: {dryRun}");
            await writer.WriteLineAsync($"parallel: {parallel}");
            await writer.WriteLineAsync($"verbose: {verbose}");
            await writer.WriteLineAsync();

            var palette = new Palette(noColor, noExtendedChars, host, operatingSystem);
            var log = new Logger(writer, $"logPrefix{ordinal}", skipDependencies, dryRun, parallel, palette, verbose);

            await Write(log);
        }

        private static async Task Write(Logger log)
        {
            var badInput = "badInput";
            var badInputDuration = TimeSpan.FromMilliseconds(1.234);
            var badInputEx = new Exception("badInputEx");
            var badInputId = Guid.ParseExact("AA123".PadRight(32, '0'), "N");

            var badInputsTarget = "badInputsTarget";
            var badInputsTargetDuration = TimeSpan.FromMilliseconds(2.345);

            var badTarget = "badTarget";
            var badTargetDuration = TimeSpan.FromMilliseconds(3.456);
            var badTargetEx = new Exception("badTargetEx");

            var emptyTargets = new List<string>();
            var emptyTargetsDuration = TimeSpan.Zero;

            var errorMessage = "errorMessage";

            var goodInput1 = "goodInput1";
            var goodInput2 = "goodInput2";
            var goodInputDuration1 = TimeSpan.FromSeconds(1.234);
            var goodInputDuration2 = TimeSpan.FromSeconds(2.345);
            var goodInputId1 = Guid.ParseExact("BB123".PadRight(32, '0'), "N");
            var goodInputId2 = Guid.ParseExact("BB234".PadRight(32, '0'), "N");

            var goodInputsTarget = "goodInputsTarget";
            var goodInputsTargetDuration = TimeSpan.FromSeconds(3.456);

            var goodTarget1 = "goodTarget1";
            var goodTarget2 = "goodTarget2";
            var goodTargetDuration1 = (TimeSpan?)null;
            var goodTargetDuration2 = TimeSpan.FromMinutes(1.234);

            var looseInput = "looseInput";
            var looseInputDuration = TimeSpan.Zero;
            var looseInputId = Guid.ParseExact("CC123".PadRight(32, '0'), "N");

            var looseTarget = "looseTarget";

            var looseTargets = new List<string> { "looseTarget" };
            var looseTargetsDuration = TimeSpan.FromMinutes(2.345);

            var noInputsTarget = "noInputsTarget";

            var targets = new List<string> { "target1", "target2", "target3" };
            var targetsDuration = TimeSpan.FromMinutes(1.234);

            var verboseTargets = new Stack<string>(new[] { "verboseTarget1", "verboseTarget2", "verboseTarget3" });
            var verboseTargetsMessage = "verboseMessage";

            var version = "version";

            await log.Version(() => version);

            await log.Verbose(() => verboseTargetsMessage);
            await log.Verbose(verboseTargets, verboseTargetsMessage);

            await log.Error(errorMessage);

            await log.Succeeded(emptyTargets, emptyTargetsDuration);

            await log.Succeeded(looseTarget, looseInput, looseInputDuration, looseInputId);
            await log.Succeeded(looseTargets, looseTargetsDuration);

            await log.Starting(targets);
            {
                await log.NoInputs(noInputsTarget);

                await log.Succeeded(goodTarget1, goodTargetDuration1);

                await log.Starting(goodTarget2);
                await log.Succeeded(goodTarget2, goodTargetDuration2);

                await log.Starting(badTarget);
                {
                    await log.Error(badTarget, badTargetEx);
                }
                await log.Failed(badTarget, badTargetEx, badTargetDuration);

                await log.Starting(goodInputsTarget);
                {
                    await log.Starting(goodInputsTarget, goodInput1, goodInputId1);
                    await log.Succeeded(goodInputsTarget, goodInput1, goodInputDuration1, goodInputId1);

                    await log.Starting(goodInputsTarget, goodInput2, goodInputId2);
                    await log.Succeeded(goodInputsTarget, goodInput2, goodInputDuration2, goodInputId2);
                }
                await log.Succeeded(goodInputsTarget, goodInputsTargetDuration);

                await log.Starting(badInputsTarget);
                {
                    await log.Starting(badInputsTarget, goodInput1, goodInputId1);
                    await log.Succeeded(badInputsTarget, goodInput1, goodInputDuration1, goodInputId1);

                    await log.Starting(badInputsTarget, badInput, badInputId);
                    {
                        await log.Error(badInputsTarget, badInput, badInputEx);
                    }
                    await log.Failed(badInputsTarget, badInput, badInputEx, badInputDuration, badInputId);
                }
                await log.Failed(badInputsTarget, badInputsTargetDuration);
            }
            await log.Succeeded(targets, targetsDuration);
            await log.Failed(targets, targetsDuration);
        }
    }
}
