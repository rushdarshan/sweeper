using System.Timers;
using System.Windows.Controls;
using ScreenshotSweeper.Helpers;
using ScreenshotSweeper.Models;
using ScreenshotSweeper.Services;
using ScreenshotSweeper; // Add root namespace for App class access

namespace ScreenshotSweeper.Views
{
    public partial class MonitorTab : Page
    {
        private Timer? _refreshTimer;

        public MonitorTab()
        {
            InitializeComponent();
            
            // Get cleanup service from app
            var cleanupService = App.CleanupService;
            
            if (cleanupService != null)
            {
                cleanupService.StatusUpdated += RefreshFileList;
                _refreshTimer = new Timer(Constants.UI_REFRESH_INTERVAL_MS);
                _refreshTimer.Elapsed += (s, e) => RefreshFileList(GetCurrentFiles());
                _refreshTimer.Start();
                
                // Initial refresh
                RefreshFileList(GetCurrentFiles());
            }
        }

        private void RefreshFileList(System.Collections.Generic.List<ScreenshotMetadata> files)
        {
            Dispatcher.Invoke(() =>
            {
                FileListGrid.ItemsSource = files;
                EmptyMessage.Visibility = files.Count == 0 ? 
                    System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;

                // Update status bar
                long totalBytes = 0;
                foreach (var file in files)
                    totalBytes += file.FileSizeBytes;

                var sizeText = FormatBytes(totalBytes);
                StatusText.Text = $"?? Monitoring: {files.Count} files | {sizeText} | Next cleanup in...";
            });
        }

        private System.Collections.Generic.List<ScreenshotMetadata> GetCurrentFiles()
        {
            return App.CleanupService?.GetTrackedFiles() ?? new System.Collections.Generic.List<ScreenshotMetadata>();
        }

        private void OnKeepClick(object sender, System.Windows.RoutedEventArgs e)
        {
            if (FileListGrid.SelectedItem is ScreenshotMetadata metadata)
            {
                var config = App.ConfigService?.LoadConfig();
                if (config != null)
                {
                    App.CleanupService?.MoveToKeep(metadata.FilePath, config.KeepFolderPath);
                }
            }
        }

        private void OnDeleteClick(object sender, System.Windows.RoutedEventArgs e)
        {
            if (FileListGrid.SelectedItem is ScreenshotMetadata metadata)
            {
                FileHelper.DeleteFile(metadata.FilePath);
                RefreshFileList(GetCurrentFiles());
            }
        }

        private string FormatBytes(long bytes)
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
