using System;
using Bullseye.Internal;
using Xunit;

namespace BullseyeTests
{
    public static class TimeSpanExtensionsTests
    {
        [Theory]

        // null
        [InlineData(null, "", null)]

        // negative
        [InlineData(-1.2345D, "<1 ms", null)]

        // boundary
        [InlineData(0.5000D, "<1 ms", "1 ms")]
        [InlineData(0.5001D, "1 ms", null)]

        // magnitude
        [InlineData(1.0000D, "1 ms", null)]
        [InlineData(1.2345D, "1 ms", null)]
        [InlineData(4.5678D, "5 ms", null)]

        // magnitude
        [InlineData(10.0000D, "10 ms", null)]
        [InlineData(12.3456D, "12 ms", null)]
        [InlineData(34.5678D, "35 ms", null)]

        // magnitude
        [InlineData(100.0000D, "100 ms", null)]
        [InlineData(123.4567D, "123 ms", null)]
        [InlineData(234.5678D, "235 ms", null)]

        // boundary
        [InlineData(999.4999D, "999 ms", null)]
        [InlineData(999.5000D, "1.00 s", null)]

        // magnitudeG
        [InlineData(1_000.0000D, "1.00 s", null)]
        [InlineData(1_234.5678D, "1.23 s", "1.24 s")]
        [InlineData(2_345.6789D, "2.35 s", null)]

        // magnitude
        [InlineData(10_000.0000D, "10.00 s", null)]
        [InlineData(11_234.5678D, "11.23 s", "11.24 s")]
        [InlineData(23_456.7891D, "23.46 s", null)]

        // boundary
        [InlineData(59_994.9999D, "59.99 s", "1 m")]
        [InlineData(59_995.0000D, "1 m", null)]

        // magnitude
        [InlineData(60_000.0000D, "1 m", null)]
        [InlineData(61_234.5678D, "1 m 1 s", null)]
        [InlineData(64_567.8912D, "1 m 5 s", null)]

        // magnitude
        [InlineData(600_000.0000D, "10 m", null)]
        [InlineData(612_345.6789D, "10 m 12 s", null)]
        [InlineData(634_567.8912D, "10 m 35 s", null)]

        // boundary
        [InlineData(3_599_499.9999D, "59 m 59 s", "60 m")]
        [InlineData(3_599_500.0000D, "60 m", null)]

        // magnitude
        [InlineData(3_600_000.0000D, "60 m", null)]
        [InlineData(3_612_234.5678D, "60 m", null)]
        [InlineData(3_634_567.8912D, "61 m", null)]
        public static void Humanization(
            double? milliseconds,
            string expected,
#pragma warning disable CA1707 // Identifiers should not contain underscores
            string expectedNetCoreApp2_1)
#pragma warning restore CA1707 // Identifiers should not contain underscores
        {
            // arrange
            var timeSpan = milliseconds.HasValue ? TimeSpan.FromMilliseconds(milliseconds.Value) : (TimeSpan?)null;

            // act
            var actual = timeSpan.Humanize();

            // assert
            expected = Environment.Version.Major == 4 ? expectedNetCoreApp2_1 ?? expected : expected;
            Assert.Equal(expected, actual);
        }
    }
}
