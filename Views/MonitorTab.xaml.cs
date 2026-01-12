using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using ScreenshotSweeper.Helpers;
using ScreenshotSweeper.Models;
using ScreenshotSweeper.Services;
using ScreenshotSweeper;

namespace ScreenshotSweeper.Views
{
    public partial class MonitorTab : Page
    {
        private Timer? _refreshTimer;
        private List<ScreenshotMetadata> _currentFiles = new();
        private List<ScreenshotMetadata> _filteredFiles = new();
        private ScreenshotMetadata? _previewFile;

        public MonitorTab()
        {
            InitializeComponent();
            Console.WriteLine($"[MonitorTab] Instance created at {DateTime.Now:HH:mm:ss.fff}");
            InitializeMonitoring();
        }

        private void InitializeMonitoring()
        {
            var cleanupService = App.CleanupService;
            
            if (cleanupService != null)
            {
                cleanupService.StatusUpdated += RefreshFileList;
                _refreshTimer = new Timer(Constants.UI_REFRESH_INTERVAL_MS);
                _refreshTimer.Elapsed += (s, e) => RefreshFileList(GetCurrentFiles());
                _refreshTimer.Start();
                
                RefreshFileList(GetCurrentFiles());
            }
        }

        private void RefreshFileList(List<ScreenshotMetadata> files)
        {
            Dispatcher.Invoke(() =>
            {
                _currentFiles = files;
                ApplyFilter();
            });
        }

        private void ApplyFilter()
        {
            var searchText = SearchBox?.Text?.ToLower() ?? "";
            
            _filteredFiles = string.IsNullOrEmpty(searchText) 
                ? _currentFiles.ToList() 
                : _currentFiles.Where(f => f.FileName.ToLower().Contains(searchText)).ToList();
            
            FileListControl.ItemsSource = _filteredFiles;
            EmptyMessage.Visibility = _filteredFiles.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            
            UpdateStatusBar(_filteredFiles);

            if (_previewFile == null && _filteredFiles.Count > 0)
            {
                _previewFile = _filteredFiles[0];
            }
            UpdatePreviewIfExists();
        }

        private void UpdateStatusBar(List<ScreenshotMetadata> files)
        {
            try
            {
                int fileCount = files?.Count ?? 0;
                long totalBytes = files?.Sum(f => f.FileSizeBytes) ?? 0;
                string sizeText = FormatBytes(totalBytes);
                
                StatusText.Text = $"📁 {fileCount} files • {sizeText} total";
                NextCleanupText.Text = $"Next cleanup: {GetNextCleanupTime(files)}";
            }
            catch (Exception ex)
            {
                StatusText.Text = $"⚠️ Error: {ex.Message}";
            }
        }

        private string GetNextCleanupTime(List<ScreenshotMetadata>? files)
        {
            if (files == null || !files.Any())
                return "--";

            var nextFile = files
                .Where(f => !f.IsExpired)
                .OrderBy(f => f.ScheduledDeleteAt)
                .FirstOrDefault();

            if (nextFile == null)
                return "Now";

            var timeUntil = nextFile.ScheduledDeleteAt - DateTime.Now;
            if (timeUntil.TotalSeconds <= 0)
                return "Now";

            if (timeUntil.TotalMinutes < 1)
                return $"{(int)timeUntil.TotalSeconds}s";
            if (timeUntil.TotalHours < 1)
                return $"{(int)timeUntil.TotalMinutes}m {timeUntil.Seconds}s";

            return $"{(int)timeUntil.TotalHours}h {timeUntil.Minutes}m";
        }

        private List<ScreenshotMetadata> GetCurrentFiles()
        {
            return App.CleanupService?.GetTrackedFiles() ?? new List<ScreenshotMetadata>();
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void OnSelectAllChecked(object sender, RoutedEventArgs e)
        {
            foreach (var file in _filteredFiles)
                file.IsSelected = true;
            FileListControl.ItemsSource = null;
            FileListControl.ItemsSource = _filteredFiles;
        }

        private void OnSelectAllUnchecked(object sender, RoutedEventArgs e)
        {
            foreach (var file in _filteredFiles)
                file.IsSelected = false;
            FileListControl.ItemsSource = null;
            FileListControl.ItemsSource = _filteredFiles;
        }

        private void OnClearAllClick(object sender, RoutedEventArgs e)
        {
            var selected = _filteredFiles.Where(f => f.IsSelected).ToList();
            
            if (selected.Count == 0)
            {
                MessageBox.Show("Please select files to delete.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show(
                $"Are you sure you want to delete {selected.Count} file(s)?",
                "Confirm Deletion",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (result == MessageBoxResult.Yes)
            {
                foreach (var file in selected)
                {
                    FileHelper.DeleteFile(file.FilePath);
                }
                RefreshFileList(GetCurrentFiles());
            }
        }

        private void OnRefreshClick(object sender, RoutedEventArgs e)
        {
            RefreshFileList(GetCurrentFiles());
        }

        private void OnKeepClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is ScreenshotMetadata metadata)
            {
                var config = App.ConfigService?.LoadConfig();
                if (config != null)
                {
                    App.CleanupService?.MoveToKeep(metadata.FilePath, config.KeepFolderPath);
                    RefreshFileList(GetCurrentFiles());
                }
            }
        }

        private void OnDeleteClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is ScreenshotMetadata metadata)
            {
                var result = MessageBox.Show(
                    $"Delete {metadata.FileName}?",
                    "Confirm",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning
                );

                if (result == MessageBoxResult.Yes)
                {
                    FileHelper.DeleteFile(metadata.FilePath);
                    RefreshFileList(GetCurrentFiles());
                }
            }
        }

        private void OnRowClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is System.Windows.Controls.Border border && border.DataContext is ScreenshotMetadata metadata)
            {
                _previewFile = metadata;
                UpdatePreview(metadata);
                e.Handled = true;
            }
        }

        private void UpdatePreview(ScreenshotMetadata file)
        {
            PreviewPanel.Visibility = Visibility.Visible;
            PreviewName.Text = file.FileName;
            PreviewSize.Text = $"Size: {file.FileSizeFormatted}";
            PreviewDetected.Text = $"Detected: {file.DetectedAtFormatted}";
            PreviewTimeLeft.Text = $"Time left: {file.TimeUntilDeletionFormatted}";

            PreviewImage.Source = null;
            PreviewErrorText.Visibility = Visibility.Collapsed;

            try
            {
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.UriSource = new Uri(file.FilePath);
                bmp.DecodePixelWidth = 600;
                bmp.EndInit();
                bmp.Freeze();
                PreviewImage.Source = bmp;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MonitorTab] Preview load failed: {ex.Message}");
                PreviewErrorText.Text = "Preview unavailable";
                PreviewErrorText.Visibility = Visibility.Visible;
            }
        }

        private void UpdatePreviewIfExists()
        {
            if (_previewFile == null)
            {
                PreviewPanel.Visibility = Visibility.Collapsed;
                PreviewImage.Source = null;
                return;
            }

            var match = _filteredFiles.FirstOrDefault(f => f.FilePath == _previewFile.FilePath);
            if (match != null)
            {
                UpdatePreview(match);
            }
            else
            {
                PreviewPanel.Visibility = Visibility.Collapsed;
                PreviewImage.Source = null;
                _previewFile = null;
            }
        }

        private void OnOpenFileClick(object sender, RoutedEventArgs e)
        {
            if (_previewFile == null) return;
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = _previewFile.FilePath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MonitorTab] Failed to open file: {ex.Message}");
            }
        }

        private void OnOpenFolderClick(object sender, RoutedEventArgs e)
        {
            if (_previewFile == null) return;
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = $"/select,\"{_previewFile.FilePath}\""
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MonitorTab] Failed to open folder: {ex.Message}");
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
                len /= 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }
}
