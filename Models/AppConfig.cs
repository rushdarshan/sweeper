using System;
using System.Collections.Generic;
using System.IO;

namespace ScreenshotSweeper.Models
{
    /// <summary>
    /// Main application configuration model
    /// </summary>
    public class AppConfig
    {
        public string Version { get; set; } = "2.0";
        public string ScreenshotFolderPath { get; set; } = string.Empty;
        public string KeepFolderPath { get; set; } = string.Empty;

        // Enhanced: Separate value and unit for flexible time configuration
        public int DeleteThresholdValue { get; set; } = 30;
        public TimeUnit DeleteThresholdUnit { get; set; } = TimeUnit.Minutes;

        public List<string> AllowedExtensions { get; set; } = new()
        {
            ".png", ".jpg", ".jpeg", ".bmp", ".gif", ".tiff", ".webp"
        };

        public NotificationSettings Notifications { get; set; } = new();
        public StartupSettings Startup { get; set; } = new();
        public MonitoringSettings Monitoring { get; set; } = new();

        /// <summary>
        /// Computed property: Convert current threshold to total minutes
        /// </summary>
        public int DeleteThresholdMinutes
        {
            get
            {
                return DeleteThresholdUnit switch
                {
                    TimeUnit.Minutes => DeleteThresholdValue,
                    TimeUnit.Hours => DeleteThresholdValue * 60,
                    TimeUnit.Days => DeleteThresholdValue * 1440,
                    _ => DeleteThresholdValue
                };
            }
        }

        /// <summary>
        /// Create default configuration with Pictures\Screenshots folder
        /// </summary>
        public static AppConfig Default()
        {
            var userPicturesPath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            var screenshotPath = Path.Combine(userPicturesPath, "Screenshots");

            return new AppConfig
            {
                ScreenshotFolderPath = screenshotPath,
                KeepFolderPath = Path.Combine(screenshotPath, "Keep"),
                DeleteThresholdValue = 30,
                DeleteThresholdUnit = TimeUnit.Minutes
            };
        }
    }

    /// <summary>
    /// Toast notification preferences
    /// </summary>
    public class NotificationSettings
    {
        public bool ShowOnDetection { get; set; } = true;
        public bool ShowOnDeletion { get; set; } = true;
        public bool PlaySound { get; set; } = false;
    }

    /// <summary>
    /// Startup and launch preferences
    /// </summary>
    public class StartupSettings
    {
        public bool LaunchOnStartup { get; set; } = false;
        public bool StartMinimized { get; set; } = true;
    }

    /// <summary>
    /// Monitoring session state
    /// </summary>
    public class MonitoringSettings
    {
        public bool IsActive { get; set; } = true;
        public DateTime? LastStartedAt { get; set; }
    }
}
