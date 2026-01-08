namespace ScreenshotSweeper.Helpers
{
    /// <summary>
    /// Application-wide constants
    /// </summary>
    public static class Constants
    {
        public const string CONFIG_FILE_NAME = "config.json";
        public const string KEEP_FOLDER_NAME = "Keep";
        
        // Monitoring intervals
        public const int FILE_MONITOR_POLL_INTERVAL_MS = 1000;
        public const int CLEANUP_CHECK_INTERVAL_MS = 10000;  // 10 seconds
        public const int UI_REFRESH_INTERVAL_MS = 5000;      // 5 seconds

        // Notification action identifiers
        public const string ACTION_SET_TIMER = "set_timer";
        public const string ACTION_KEEP = "keep";
        public const string ACTION_DELETE_NOW = "delete_now";

        // Preset durations
        public static class Presets
        {
            public const int PRESET_15MIN = 15;
            public const int PRESET_30MIN = 30;
            public const int PRESET_1HOUR = 1;
            public const int PRESET_3DAYS = 3;
        }
    }
}
