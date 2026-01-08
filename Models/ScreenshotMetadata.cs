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

        public string FileSizeFormatted
        {
            get => FormatBytes(FileSizeBytes);
        }

        public TimeSpan TimeRemaining
        {
            get => ScheduledDeleteAt - DateTime.Now;
        }

        public bool IsExpired
        {
            get => DateTime.Now >= ScheduledDeleteAt;
        }

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
