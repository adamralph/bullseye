#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable RS0016 // Add public types and members to the declared API
namespace Bullseye.Internal
{
    using System;
    using System.Globalization;
    using static System.Math;

    public static class TimeSpanExtensions
    {
        private static readonly IFormatProvider provider = CultureInfo.InvariantCulture;

        public static string Humanize(this TimeSpan? duration) =>
            duration.HasValue ? Humanize(duration.Value) : string.Empty;

        public static string Humanize(this TimeSpan duration)
        {
            // less than one millisecond
            if (Convert.ToInt64(duration.TotalMilliseconds) < 1L)
            {
                return "<1 ms";
            }

            // milliseconds
            if (Convert.ToInt64(duration.TotalMilliseconds) < 1_000L)
            {
                return duration.TotalMilliseconds.ToString("F0", provider) + " ms";
            }

            // seconds
            if (Convert.ToInt64(duration.TotalSeconds * 100L) < 60_00L)
            {
                return duration.TotalSeconds.ToString("F2", provider) + " s";
            }

            // minutes and seconds
            if (Convert.ToInt64(duration.TotalSeconds) < 3600L)
            {
                var minutes = DivRem(Convert.ToInt64(duration.TotalSeconds), 60L, out var seconds);
                return seconds == 0
                    ? $"{minutes.ToString("F0", provider)} m"
                    : $"{minutes.ToString("F0", provider)} m {seconds.ToString("F0", provider)} s";
            }

            // minutes
            return duration.TotalMinutes.ToString("N0", provider) + " m";
        }

        public static TimeSpan? Add(this TimeSpan? x, TimeSpan? y) =>
            y.HasValue
            ? (x ?? TimeSpan.Zero).Add(y.Value)
            : x;
    }
}
