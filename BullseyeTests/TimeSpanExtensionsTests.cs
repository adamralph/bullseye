namespace BullseyeTests
{
    using System;
    using Bullseye.Internal;
    using Xbehave;
    using Xunit;

    public class TimeSpanExtensionsTests
    {
        [Scenario]
        [Example(null, "")]
        [Example(0.9D, "<1 ms")]
        [Example(1D, "1 ms")]
        [Example(1.234D, "1 ms")]
        [Example(123.4D, "123 ms")]
        [Example(1_000D, "1.00 s")]
        [Example(1_234D, "1.23 s")]
        [Example(12_340D, "12.34 s")]
        [Example(60_000D, "1 m")]
        [Example(61_234D, "1 m 1 s")]
        [Example(3_599_999D, "59 m 59 s")]
        [Example(3_600_000D, "60 m")]
        [Example(3_612_345D, "60 m")]
        [Example(3_661_234D, "61 m")]
        public void Humanization(double? milliseconds, string expected, TimeSpan? timeSpan, string actual)
        {
            $"Given a timespan of {milliseconds} milliseconds"
                .x(() => timeSpan = milliseconds.HasValue ? TimeSpan.FromMilliseconds(milliseconds.Value) : (TimeSpan?)null);

            $"When humanizing the timespan"
                .x(() => actual = timeSpan.Humanize());

            $"Then the result is \"{expected}\""
                .x(() => Assert.Equal(expected, actual));
        }
    }
}
