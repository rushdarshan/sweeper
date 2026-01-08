using System;
using ScreenshotSweeper.Models;

namespace ScreenshotSweeper.Helpers
{
    /// <summary>
    /// Utility class for time unit conversions and formatting
    /// </summary>
    public static class TimeHelper
    {
        /// <summary>
        /// Converts a time value and unit to total minutes
        /// </summary>
        public static int ToMinutes(int value, TimeUnit unit)
        {
            return unit switch
            {
                TimeUnit.Minutes => value,
                TimeUnit.Hours => value * 60,
                TimeUnit.Days => value * 1440,
                _ => value
            };
        }

        /// <summary>
        /// Formats a TimeSpan for user-friendly display
        /// Shows the two largest appropriate units
        /// </summary>
        public static string FormatTimeSpan(TimeSpan span)
        {
            if (span.TotalSeconds < 0)
                return "Deleting...";

            if (span.TotalDays >= 1)
                return $"{(int)span.TotalDays}d {span.Hours}h";

            if (span.TotalHours >= 1)
                return $"{(int)span.TotalHours}h {span.Minutes}m";

            if (span.TotalMinutes >= 1)
                return $"{(int)span.TotalMinutes}m {span.Seconds}s";

            return $"{span.Seconds}s";
        }

        /// <summary>
        /// Gets validation range (min, max) for a given time unit
        /// </summary>
        public static (int Min, int Max) GetValidationRange(TimeUnit unit)
        {
            return unit switch
            {
                TimeUnit.Minutes => (5, 1440),      // 5 min to 24 hours
                TimeUnit.Hours => (1, 168),         // 1 hour to 7 days
                TimeUnit.Days => (1, 30),           // 1 day to 30 days
                _ => (5, 1440)
            };
        }

        /// <summary>
        /// Calculates absolute deletion time from now
        /// </summary>
        public static DateTime CalculateDeleteTime(int value, TimeUnit unit)
        {
            var minutes = ToMinutes(value, unit);
            return DateTime.Now.AddMinutes(minutes);
        }

        /// <summary>
        /// Returns a human-readable description of a time value and unit
        /// </summary>
        public static string FormatTimeDescription(int value, TimeUnit unit)
        {
            var unitName = unit switch
            {
                TimeUnit.Minutes => value > 1 ? "minutes" : "minute",
                TimeUnit.Hours => value > 1 ? "hours" : "hour",
                TimeUnit.Days => value > 1 ? "days" : "day",
                _ => "minutes"
            };

            return $"{value} {unitName}";
        }
    }
}
