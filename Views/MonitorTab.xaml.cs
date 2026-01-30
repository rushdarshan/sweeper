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
            
            // Filter out 0-byte files (corrupt/incomplete) and apply search
            var validFiles = _currentFiles.Where(f => f.FileSizeBytes > 0);
            
            _filteredFiles = string.IsNullOrEmpty(searchText) 
                ? validFiles.ToList() 
                : validFiles.Where(f => f.FileName.ToLower().Contains(searchText)).ToList();
            
            FileListControl.ItemsSource = _filteredFiles;
            EmptyMessage.Visibility = _filteredFiles.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            
            // Disable Clear All button when no files
            ClearAllBtn.IsEnabled = _filteredFiles.Count > 0;
            ClearAllBtn.Opacity = _filteredFiles.Count > 0 ? 1.0 : 0.5;
            
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

        private void OnPreviewButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is ScreenshotMetadata metadata)
            {
                _previewFile = metadata;
                UpdatePreview(metadata);
            }
        }

        private void UpdatePreview(ScreenshotMetadata file)
        {
            // Show preview panel and expand columns
            PreviewPanel.Visibility = Visibility.Visible;
            DividerColumn.Width = new GridLength(1, GridUnitType.Auto);
            PreviewColumn.Width = new GridLength(350);
            
            PreviewName.Text = file.FileName;
            PreviewSize.Text = $"Size: {file.FileSizeFormatted}";
            PreviewDetected.Text = $"Detected: {file.DetectedAtFormatted}";
            PreviewTimeLeft.Text = $"Time left: {file.TimeUntilDeletionFormatted}";

            PreviewImage.Source = null;
            PreviewErrorPanel.Visibility = Visibility.Collapsed;

            try
            {
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.UriSource = new Uri(file.FilePath, UriKind.Absolute);
                bmp.DecodePixelWidth = 600;
                bmp.EndInit();
                bmp.Freeze();
                PreviewImage.Source = bmp;
                Console.WriteLine($"[MonitorTab] Preview loaded: {file.FileName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MonitorTab] Preview load failed: {ex.Message}");
                PreviewErrorText.Text = "Unable to preview image";
                PreviewErrorPanel.Visibility = Visibility.Visible;
            }
        }

        private void UpdatePreviewIfExists()
        {
            if (_previewFile == null)
            {
                HidePreviewPanel();
                return;
            }

            var match = _filteredFiles.FirstOrDefault(f => f.FilePath == _previewFile.FilePath);
            if (match != null)
            {
                UpdatePreview(match);
            }
            else
            {
                HidePreviewPanel();
                _previewFile = null;
            }
        }

        private void HidePreviewPanel()
        {
            PreviewPanel.Visibility = Visibility.Collapsed;
            DividerColumn.Width = new GridLength(0);
            PreviewColumn.Width = new GridLength(0);
            PreviewImage.Source = null;
        }

        private void OnOpenFileClick(object sender, RoutedEventArgs e)
        {
            if (_previewFile == null) return;
            if (!System.IO.File.Exists(_previewFile.FilePath))
            {
                MessageBox.Show("File not found on disk.", "Open Image", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var filePath = _previewFile.FilePath;
            var ext = System.IO.Path.GetExtension(filePath)?.ToLowerInvariant();

            // Before trying to launch, verify the file is accessible (this handles OneDrive online-only placeholders).
            try
            {
                using var stream = System.IO.File.Open(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read);
            }
            catch (Exception accessEx)
            {
                Console.WriteLine($"[MonitorTab] File not accessible for reading: {accessEx.Message}");
                MessageBox.Show("The file is not available locally (it may be stored in OneDrive or another cloud provider).\nOpening its folder so you can download or open it manually.", "Open Image", MessageBoxButton.OK, MessageBoxImage.Information);
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "explorer.exe",
                        Arguments = $"/select,\"{filePath}\"",
                        UseShellExecute = true
                    });
                }
                catch (Exception exSel)
                {
                    Console.WriteLine($"[MonitorTab] Explorer select failed while handling inaccessible file: {exSel.Message}");
                }
                return;
            }

            // Try to open with the system default (open verb) which should launch Photos or the default image viewer
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true,
                    Verb = "open",
                    WorkingDirectory = System.IO.Path.GetDirectoryName(filePath) ?? ""
                };
                Process.Start(psi);
                return;
            }
            catch (Exception ex1)
            {
                Console.WriteLine($"[MonitorTab] Default open (verb=open) failed: {ex1.Message}");
            }

            // Try launching via explorer (should delegate to registered handler)
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = $"\"{filePath}\"",
                    UseShellExecute = true
                });
                return;
            }
            catch (Exception ex2)
            {
                Console.WriteLine($"[MonitorTab] Explorer open failed: {ex2.Message}");
            }

            // Show the "Open With" dialog so user may choose an app
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "rundll32.exe",
                    Arguments = $"shell32,OpenAs_RunDLL \"{filePath}\"",
                    UseShellExecute = true
                });
                return;
            }
            catch (Exception ex3)
            {
                Console.WriteLine($"[MonitorTab] OpenWith dialog failed: {ex3.Message}");
            }

            // As a last visual fallback, show file in File Explorer (selected)
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = $"/select,\"{filePath}\"",
                    UseShellExecute = true
                });
                return;
            }
            catch (Exception ex4)
            {
                Console.WriteLine($"[MonitorTab] Explorer select failed: {ex4.Message}");
            }

            MessageBox.Show("Unable to open image with the system default apps. You can use 'Show in Folder' to locate the file and open it manually.", "Open Image", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private void OnClosePreviewClick(object sender, RoutedEventArgs e)
        {
            HidePreviewPanel();
            _previewFile = null;
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
