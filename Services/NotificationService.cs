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
                new ToastContentBuilder()
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
                        .AddArgument("path", file.FilePath))
                    
                    .Show(); // ACTUALLY SHOW THE NOTIFICATION!

                System.Console.WriteLine($"[Notification] Sent detection notification for: {file.FileName}");
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"[Notification] Error sending detection notification: {ex.Message}");
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
                new ToastContentBuilder()
                    .AddText("🗑️ Screenshot Deleted")
                    .AddText(file.FileName)
                    .AddText($"Size: {file.FileSizeFormatted}")
                    .Show();
                    
                System.Console.WriteLine($"[Notification] Sent deletion notification for: {file.FileName}");
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"[Notification] Error sending deletion notification: {ex.Message}");
            }
        }

        /// <summary>
        /// Sends an informational notification
        /// </summary>
        public void SendInfoNotification(string title, string message)
        {
            try
            {
                new ToastContentBuilder()
                    .AddText(title)
                    .AddText(message)
                    .Show();
                    
                System.Console.WriteLine($"[Notification] {title}: {message}");
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"[Notification] Error sending info notification: {ex.Message}");
            }
        }
    }
}
