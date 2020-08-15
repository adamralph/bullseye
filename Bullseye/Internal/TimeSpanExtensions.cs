#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Bullseye.Internal
{
    using System;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using static System.Math;

    public static class TimeSpanExtensions
    {
        private static readonly IFormatProvider provider = CultureInfo.InvariantCulture;

        public static string Humanize(this TimeSpan? duration) =>
            duration.HasValue ? Humanize(duration.Value) : string.Empty;

        public static string Humanize(this TimeSpan duration)
        {
            // less than one millisecond
            if (duration.TotalMilliseconds < 1D)
            {
                return "<1 ms";
            }

            // milliseconds
            if (duration.TotalSeconds < 1D)
            {
                return duration.TotalMilliseconds.ToString("F0", provider) + " ms";
            }

            // seconds
            if (duration.TotalMinutes < 1D)
            {
                return duration.TotalSeconds.ToString("F2", provider) + " s";
            }

            // minutes and seconds
            if (duration.TotalHours < 1D)
            {
                var minutes = Floor(duration.TotalMinutes).ToString("F0", provider);
                var seconds = duration.Seconds.ToString("F0", provider);
                return seconds == "0"
                    ? $"{minutes} m"
                    : $"{minutes} m {seconds} s";
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
