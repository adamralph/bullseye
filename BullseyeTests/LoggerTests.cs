namespace BullseyeTests
{
    using System;
    using System.IO;
    using Bullseye.Internal;
    using Xbehave;
    using Xunit;

    public class LoggerTests
    {
        [Scenario]
        [Example(0.001D, "<1 ms")]
        [Example(1D, "1 ms")]
        [Example(1_000D, "1 s")]
        [Example(119_000D, "1 m 59 s")]
        [Example(1_000_000D, "16 m 40 s")]
        [Example(1_000_000_000D, "16,667 m")]
        public void Timings(double duration, string expectedSubstring, Logger log, StringWriter writer)
        {
            "Given a logger"
                .x(() => log = new Logger(writer = new StringWriter(), default, default, default, default, new Palette(default, default, default, default), default));

            $"When logging a message with a duration of {duration} milliseconds"
                .x(() => log.Succeeded("foo", TimeSpan.FromMilliseconds(duration)));

            $"Then the message contains \"{expectedSubstring}\""
                .x(() => Assert.Contains(expectedSubstring, writer.ToString()));
        }
    }
}
