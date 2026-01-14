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
        public const int CLEANUP_CHECK_INTERVAL_MS = 2000;   // 2 seconds - fast cleanup checks
        public const int UI_REFRESH_INTERVAL_MS = 2000;      // 2 seconds - responsive UI updates

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
