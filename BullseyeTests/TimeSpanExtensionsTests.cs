namespace BullseyeTests
{
    using System;
    using Bullseye.Internal;
    using Xbehave;
    using Xunit;

    public class TimeSpanExtensionsTests
    {
        [Scenario]
        [Example(0.9D, "<1 ms")]
        [Example(1D, "1 ms")]
        [Example(1_000D, "1 s")]
        [Example(119_000D, "1 m 59 s")]
        [Example(1_000_000D, "16 m 40 s")]
        [Example(1_000_000_000D, "16,667 m")]
        public void Humanization(double milliseconds, string expected, TimeSpan timeSpan, string actual)
        {
            $"Given a timespan of {milliseconds} milliseconds"
                .x(() => timeSpan = TimeSpan.FromMilliseconds(milliseconds));

            $"When humanizing the timespan"
                .x(() => actual = timeSpan.Humanize());

            $"Then the result is \"{expected}\""
                .x(() => Assert.Equal(expected, actual));
        }
    }
}
