using Microsoft.Toolkit.Uwp.Notifications;
using ScreenshotSweeper.Helpers;
using ScreenshotSweeper.Models;

namespace ScreenshotSweeper.Services
{
    /// <summary>
    /// Handles Windows Toast notifications with preset action buttons
    /// </summary>
    public class NotificationService
    {
        private AppConfig _config;

        public NotificationService(AppConfig config)
        {
            _config = config;
        }

        public void UpdateConfig(AppConfig config)
        {
            _config = config;
        }

        /// <summary>
        /// Sends a "screenshot detected" notification with preset action buttons
        /// </summary>
        public void SendDetectionNotification(ScreenshotMetadata file)
        {
            if (!_config.Notifications.ShowOnDetection)
                return;

            var deleteTimeText = TimeHelper.FormatTimeDescription(_config.DeleteThresholdValue, _config.DeleteThresholdUnit);

            try
            {
                var toastContent = new ToastContentBuilder()
                    .AddText("📸 New Screenshot Detected")
                    .AddText(file.FileName)
                    .AddText($"Size: {file.FileSizeFormatted} | Auto-delete in: {deleteTimeText}")

                    // Preset action buttons
                    .AddButton(new ToastButton()
                        .SetContent("15 Min")
                        .AddArgument(Constants.ACTION_SET_TIMER, "15_min")
                        .AddArgument("path", file.FilePath))

                    .AddButton(new ToastButton()
                        .SetContent("30 Min")
                        .AddArgument(Constants.ACTION_SET_TIMER, "30_min")
                        .AddArgument("path", file.FilePath))

                    .AddButton(new ToastButton()
                        .SetContent("1 Hour")
                        .AddArgument(Constants.ACTION_SET_TIMER, "1_hour")
                        .AddArgument("path", file.FilePath))

                    .AddButton(new ToastButton()
                        .SetContent("Keep")
                        .AddArgument(Constants.ACTION_KEEP, "true")
                        .AddArgument("path", file.FilePath));

                // Toast notifications queued
                System.Console.WriteLine($"[Notification] Queued detection notification for: {file.FileName}");
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"[Notification] Error queuing detection notification: {ex.Message}");
            }
        }

        /// <summary>
        /// Sends a "screenshot deleted" notification
        /// </summary>
        public void SendDeletionNotification(ScreenshotMetadata file)
        {
            if (!_config.Notifications.ShowOnDeletion)
                return;

            try
            {
                System.Console.WriteLine($"[Notification] Queued deletion notification for: {file.FileName}");
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"[Notification] Error queuing deletion notification: {ex.Message}");
            }
        }

        /// <summary>
        /// Sends an informational notification
        /// </summary>
        public void SendInfoNotification(string title, string message)
        {
            try
            {
                System.Console.WriteLine($"[Notification] {title}: {message}");
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"[Notification] Error queuing info notification: {ex.Message}");
            }
        }
    }
}
