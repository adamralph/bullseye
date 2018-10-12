namespace BullseyeTests
{
    using Bullseye.Internal;
    using BullseyeTests.Infra;
    using Xbehave;
    using Xunit;

    public class LoggerTests
    {
        [Scenario]
        [Example(0.001D, "<1 ms")]
        [Example(1D, "1 ms")]
        [Example(1_000D, "1 s")]
        [Example(119_000D, "1 min 59 s")]
        [Example(1_000_000D, "16 min 40 s")]
        [Example(1_000_000_000D, "16,667 min")]
        public void Timings(double elapsed, string expectedSubstring, Logger log, TestConsole console)
        {
            "Given a logger"
                .x(() => log = new Logger(console = new TestConsole(), false, false, false, new Palette(false, Host.Unknown, OperatingSystem.Unknown), false));

            $"When logging a message with an elapsed time in milliseconds of {elapsed}"
                .x(() => log.Succeeded("foo", elapsed));

            $"Then the message contains \"{expectedSubstring}\""
                .x(() => Assert.Contains(expectedSubstring, console.Out.ToString()));
        }
    }
}
