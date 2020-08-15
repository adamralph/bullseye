namespace BullseyeTests
{
    using System;
    using Bullseye.Internal;
    using Xbehave;
    using Xunit;

    public class TimeSpanExtensionsTests
    {
        [Scenario]

        // null
        [Example(null, "")]

        // negative
        [Example(-1.2345D, "<1 ms")]

        // boundary
        [Example(0.5000D, "<1 ms")]
        [Example(0.5001D, "1 ms")]

        // magnitude
        [Example(1.0000D, "1 ms")]
        [Example(1.2345D, "1 ms")]
        [Example(4.5678D, "5 ms")]

        // magnitude
        [Example(10.0000D, "10 ms")]
        [Example(12.3456D, "12 ms")]
        [Example(34.5678D, "35 ms")]

        // magnitude
        [Example(100.0000D, "100 ms")]
        [Example(123.4567D, "123 ms")]
        [Example(234.5678D, "235 ms")]

        // boundary
        [Example(999.4999D, "999 ms")]
        [Example(999.5000D, "1.00 s")]

        // magnitude
        [Example(1_000.0000D, "1.00 s")]
        [Example(1_234.5678D, "1.23 s")]
        [Example(2_345.6789D, "2.35 s")]

        // magnitude
        [Example(10_000.0000D, "10.00 s")]
        [Example(11_234.5678D, "11.23 s")]
        [Example(23_456.7891D, "23.46 s")]

        // boundary
        [Example(59_994.9999D, "59.99 s")]
        [Example(59_995.0000D, "1 m")]

        // magnitude
        [Example(60_000.0000D, "1 m")]
        [Example(61_234.5678D, "1 m 1 s")]
        [Example(64_567.8912D, "1 m 5 s")]

        // magnitude
        [Example(600_000.0000D, "10 m")]
        [Example(612_345.6789D, "10 m 12 s")]
        [Example(634_567.8912D, "10 m 35 s")]

        // boundary
        [Example(3_599_499.9999D, "59 m 59 s")]
        [Example(3_599_500.0000D, "60 m")]

        // magnitude
        [Example(3_600_000.0000D, "60 m")]
        [Example(3_612_234.5678D, "60 m")]
        [Example(3_634_567.8912D, "61 m")]
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
