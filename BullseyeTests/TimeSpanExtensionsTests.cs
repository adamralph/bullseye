using System;
using Bullseye.Internal;
using Xbehave;
using Xunit;

namespace BullseyeTests
{
    public class TimeSpanExtensionsTests
    {
        [Scenario]

        // null
        [Example(null, "", null)]

        // negative
        [Example(-1.2345D, "<1 ms", null)]

        // boundary
        [Example(0.5000D, "<1 ms", "1 ms")]
        [Example(0.5001D, "1 ms", null)]

        // magnitude
        [Example(1.0000D, "1 ms", null)]
        [Example(1.2345D, "1 ms", null)]
        [Example(4.5678D, "5 ms", null)]

        // magnitude
        [Example(10.0000D, "10 ms", null)]
        [Example(12.3456D, "12 ms", null)]
        [Example(34.5678D, "35 ms", null)]

        // magnitude
        [Example(100.0000D, "100 ms", null)]
        [Example(123.4567D, "123 ms", null)]
        [Example(234.5678D, "235 ms", null)]

        // boundary
        [Example(999.4999D, "999 ms", null)]
        [Example(999.5000D, "1.00 s", null)]

        // magnitudeG
        [Example(1_000.0000D, "1.00 s", null)]
        [Example(1_234.5678D, "1.23 s", "1.24 s")]
        [Example(2_345.6789D, "2.35 s", null)]

        // magnitude
        [Example(10_000.0000D, "10.00 s")]
        [Example(11_234.5678D, "11.23 s", "11.24 s")]
        [Example(23_456.7891D, "23.46 s", null)]

        // boundary
        [Example(59_994.9999D, "59.99 s", "1 m")]
        [Example(59_995.0000D, "1 m", null)]

        // magnitude
        [Example(60_000.0000D, "1 m", null)]
        [Example(61_234.5678D, "1 m 1 s", null)]
        [Example(64_567.8912D, "1 m 5 s", null)]

        // magnitude
        [Example(600_000.0000D, "10 m", null)]
        [Example(612_345.6789D, "10 m 12 s", null)]
        [Example(634_567.8912D, "10 m 35 s", null)]

        // boundary
        [Example(3_599_499.9999D, "59 m 59 s", "60 m")]
        [Example(3_599_500.0000D, "60 m", null)]

        // magnitude
        [Example(3_600_000.0000D, "60 m", null)]
        [Example(3_612_234.5678D, "60 m", null)]
        [Example(3_634_567.8912D, "61 m", null)]
        public void Humanization(double? milliseconds, string expected, string expectedNetCoreApp2_1, TimeSpan? timeSpan, string actual)
        {
            expected = Environment.Version.Major == 4 ? expectedNetCoreApp2_1 ?? expected : expected;

            $"Given a timespan of {milliseconds} milliseconds"
                .x(() => timeSpan = milliseconds.HasValue ? TimeSpan.FromMilliseconds(milliseconds.Value) : (TimeSpan?)null);

            $"When humanizing the timespan"
                .x(() => actual = timeSpan.Humanize());

            $"Then the result is \"{expected}\""
                .x(() => Assert.Equal(expected, actual));
        }
    }
}
