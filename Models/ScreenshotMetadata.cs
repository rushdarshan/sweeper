using System;

namespace ScreenshotSweeper.Models
{
    /// <summary>
    /// Metadata for a tracked screenshot file
    /// </summary>
    public class ScreenshotMetadata
    {
        public string FilePath { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public long FileSizeBytes { get; set; }
        public DateTime DetectedAt { get; set; }
        public DateTime ScheduledDeleteAt { get; set; }
        public bool IsInKeepFolder { get; set; }
        public bool IsSelected { get; set; }

        // Store original scheduled time to calculate progress
        private DateTime _originalScheduledAt;
        public DateTime OriginalScheduledAt 
        { 
            get => _originalScheduledAt == default ? ScheduledDeleteAt : _originalScheduledAt;
            set => _originalScheduledAt = value;
        }

        public string FileSizeFormatted => FormatBytes(FileSizeBytes);

        public TimeSpan TimeRemaining => ScheduledDeleteAt - DateTime.Now;

        public bool IsExpired => DateTime.Now >= ScheduledDeleteAt;

        public string DetectedAtFormatted => DetectedAt.ToString("MM-dd HH:mm:ss");

        public string TimeUntilDeletionFormatted
        {
            get
            {
                var span = TimeRemaining;
                if (span.TotalSeconds <= 0) return "Deleting...";
                if (span.TotalDays >= 1) return $"{(int)span.TotalDays}d {span.Hours}h";
                if (span.TotalHours >= 1) return $"{(int)span.TotalHours}h {span.Minutes}m";
                if (span.TotalMinutes >= 1) return $"{(int)span.TotalMinutes}m {span.Seconds}s";
                return $"{span.Seconds}s";
            }
        }

        /// <summary>
        /// Progress width for the progress bar (0-100 representing remaining time as percentage)
        /// </summary>
        public double ProgressWidth
        {
            get
            {
                var totalTime = OriginalScheduledAt - DetectedAt;
                var remaining = TimeRemaining;
                
                if (totalTime.TotalSeconds <= 0) return 0;
                if (remaining.TotalSeconds <= 0) return 0;
                
                var percentage = (remaining.TotalSeconds / totalTime.TotalSeconds) * 100;
                return Math.Max(0, Math.Min(100, percentage));
            }
        }

        public bool IsUrgent => TimeRemaining.TotalMinutes < 2;
        public bool IsWarning => TimeRemaining.TotalMinutes >= 2 && TimeRemaining.TotalMinutes < 5;
        public bool IsSafe => TimeRemaining.TotalMinutes >= 5;

        private static string FormatBytes(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }
}
