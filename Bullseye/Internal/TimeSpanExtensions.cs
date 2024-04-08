using System.Globalization;
using static System.Math;

namespace Bullseye.Internal;

public static class TimeSpanExtensions
{
    private static readonly IFormatProvider provider = CultureInfo.InvariantCulture;

    public static string Humanize(this TimeSpan duration)
    {
        // negative
        if (duration < TimeSpan.Zero)
        {
            return "<0 ms";
        }

        // zero
        if (duration == TimeSpan.Zero)
        {
            return "0 ms";
        }

        var totalMilliseconds = Convert.ToInt64(duration.TotalMilliseconds);

        // less than one millisecond
        if (totalMilliseconds < 1L)
        {
            return "<1 ms";
        }

        // milliseconds
        if (totalMilliseconds < 1_000L)
        {
            return duration.TotalMilliseconds.ToString("F0", provider) + " ms";
        }

        // seconds
        if (Convert.ToInt64(duration.TotalSeconds * 100L) < 60_00L)
        {
            return duration.TotalSeconds.ToString("F2", provider) + " s";
        }

        var totalSeconds = Convert.ToInt64(duration.TotalSeconds);

        // minutes and seconds
        if (totalSeconds < 3600L)
        {
            var minutes = DivRem(totalSeconds, 60L, out var seconds);
            return seconds == 0
                ? $"{minutes.ToString("F0", provider)} m"
                : $"{minutes.ToString("F0", provider)} m {seconds.ToString("F0", provider)} s";
        }

        // minutes
        return duration.TotalMinutes.ToString("N0", provider) + " m";
    }
}
