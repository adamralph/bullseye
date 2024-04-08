using Bullseye.Internal;
using Xunit;

namespace BullseyeTests;

public static class TimeSpanExtensionsTests
{
    [Theory]

    // negative
    [InlineData(-1.2345D, "<0 ms")]

    // zero
    [InlineData(0, "0 ms")]

    // boundary
    [InlineData(0.5000D, "<1 ms")]
    [InlineData(0.5001D, "1 ms")]

    // magnitude
    [InlineData(1.0000D, "1 ms")]
    [InlineData(1.2345D, "1 ms")]
    [InlineData(4.5678D, "5 ms")]

    // magnitude
    [InlineData(10.0000D, "10 ms")]
    [InlineData(12.3456D, "12 ms")]
    [InlineData(34.5678D, "35 ms")]

    // magnitude
    [InlineData(100.0000D, "100 ms")]
    [InlineData(123.4567D, "123 ms")]
    [InlineData(234.5678D, "235 ms")]

    // boundary
    [InlineData(999.4999D, "999 ms")]
    [InlineData(999.5000D, "1.00 s")]

    // magnitudeG
    [InlineData(1_000.0000D, "1.00 s")]
    [InlineData(1_234.5678D, "1.23 s")]
    [InlineData(2_345.6789D, "2.35 s")]

    // magnitude
    [InlineData(10_000.0000D, "10.00 s")]
    [InlineData(11_234.5678D, "11.23 s")]
    [InlineData(23_456.7891D, "23.46 s")]

    // boundary
    [InlineData(59_994.9999D, "59.99 s")]
    [InlineData(59_995.0000D, "1 m")]

    // magnitude
    [InlineData(60_000.0000D, "1 m")]
    [InlineData(61_234.5678D, "1 m 1 s")]
    [InlineData(64_567.8912D, "1 m 5 s")]

    // magnitude
    [InlineData(600_000.0000D, "10 m")]
    [InlineData(612_345.6789D, "10 m 12 s")]
    [InlineData(634_567.8912D, "10 m 35 s")]

    // boundary
    [InlineData(3_599_499.9999D, "59 m 59 s")]
    [InlineData(3_599_500.0000D, "60 m")]

    // magnitude
    [InlineData(3_600_000.0000D, "60 m")]
    [InlineData(3_612_234.5678D, "60 m")]
    [InlineData(3_634_567.8912D, "61 m")]
    public static void Humanization(double milliseconds, string expected)
    {
        // arrange
        var timeSpan = TimeSpan.FromMilliseconds(milliseconds);

        // act
        var actual = timeSpan.Humanize();

        // assert
        Assert.Equal(expected, actual);
    }
}
