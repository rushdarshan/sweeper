using System;
using System.Collections.Generic;
using System.IO;
using ScreenshotSweeper.Helpers;
using ScreenshotSweeper.Models;

namespace ScreenshotSweeper.Services
{
    /// <summary>
    /// Monitors the screenshot folder and tracks new files
    /// </summary>
    public class FileMonitorService
    {
        private FileSystemWatcher? _watcher;
        private readonly AppConfig _config;
        private readonly List<string> _trackedFiles = new();
        private bool _isMonitoring;

        public event Action<ScreenshotMetadata>? FileDetected;
        public event Action<string>? FileDeleted;
        // event Action<List<ScreenshotMetadata>>? FilesChanged; // Unused, removing

        public FileMonitorService(AppConfig config)
        {
            _config = config;
        }

        /// <summary>
        /// Starts monitoring the screenshot folder
        /// </summary>
        public void StartMonitoring()
        {
            if (_isMonitoring || !Directory.Exists(_config.ScreenshotFolderPath))
                return;

            try
            {
                _watcher = new FileSystemWatcher(_config.ScreenshotFolderPath)
                {
                    NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
                    Filter = "*.*"
                };

                _watcher.Created += OnFileCreated;
                _watcher.Deleted += OnFileDeleted;
                _watcher.Renamed += OnFileRenamed;

                _watcher.EnableRaisingEvents = true;
                _isMonitoring = true;

                Console.WriteLine($"[FileMonitor] Started monitoring: {_config.ScreenshotFolderPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FileMonitor] Error starting monitor: {ex.Message}");
            }
        }

        /// <summary>
        /// Stops monitoring the screenshot folder
        /// </summary>
        public void StopMonitoring()
        {
            if (!_isMonitoring || _watcher == null)
                return;

            _watcher.EnableRaisingEvents = false;
            _watcher.Dispose();
            _watcher = null;
            _isMonitoring = false;

            Console.WriteLine("[FileMonitor] Stopped monitoring");
        }

        /// <summary>
        /// Gets list of currently tracked files
        /// </summary>
        public List<ScreenshotMetadata> GetTrackedFiles()
        {
            var tracked = new List<ScreenshotMetadata>();

            foreach (var filePath in _trackedFiles)
            {
                if (File.Exists(filePath))
                {
                    tracked.Add(FileHelper.CreateMetadata(
                        filePath,
                        TimeHelper.CalculateDeleteTime(_config.DeleteThresholdValue, _config.DeleteThresholdUnit)
                    ));
                }
            }

            return tracked;
        }

        private void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            if (FileHelper.IsValidScreenshot(e.FullPath, _config.AllowedExtensions))
            {
                // Small delay to ensure file is fully written
                System.Threading.Thread.Sleep(500);

                if (File.Exists(e.FullPath) && !_trackedFiles.Contains(e.FullPath))
                {
                    _trackedFiles.Add(e.FullPath);

                    var metadata = FileHelper.CreateMetadata(
                        e.FullPath,
                        TimeHelper.CalculateDeleteTime(_config.DeleteThresholdValue, _config.DeleteThresholdUnit)
                    );

                    Console.WriteLine($"[FileMonitor] Detected: {metadata.FileName}");
                    FileDetected?.Invoke(metadata);
                }
            }
        }

        private void OnFileDeleted(object sender, FileSystemEventArgs e)
        {
            if (_trackedFiles.Contains(e.FullPath))
            {
                _trackedFiles.Remove(e.FullPath);
                Console.WriteLine($"[FileMonitor] File removed from tracking: {e.Name}");
                FileDeleted?.Invoke(e.FullPath);
            }
        }

        private void OnFileRenamed(object sender, RenamedEventArgs e)
        {
            if (_trackedFiles.Contains(e.OldFullPath))
            {
                _trackedFiles.Remove(e.OldFullPath);
                _trackedFiles.Add(e.FullPath);
                Console.WriteLine($"[FileMonitor] File renamed: {e.OldName} ? {e.Name}");
            }
        }

        public bool IsMonitoring => _isMonitoring;

        /// <summary>
        /// Reconfigures the service with new settings
        /// </summary>
        public void Reconfigure(AppConfig config)
        {
            _trackedFiles.Clear();
            _config.ScreenshotFolderPath = config.ScreenshotFolderPath;
            _config.DeleteThresholdValue = config.DeleteThresholdValue;
            _config.DeleteThresholdUnit = config.DeleteThresholdUnit;
            Console.WriteLine("[FileMonitor] Reconfigured with new settings");
        }
    }
}
