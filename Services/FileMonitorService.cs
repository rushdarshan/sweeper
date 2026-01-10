using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        private AppConfig _config;
        private readonly List<string> _trackedFiles = new();
        private bool _isMonitoring;
        
        // DUPLICATE PREVENTION: Track recently added files to ignore duplicate events
        private readonly HashSet<string> _recentlyAddedFiles = new();
        private const int DUPLICATE_TIMEOUT_MS = 5000; // 5 seconds

        public event Action<ScreenshotMetadata>? FileDetected;
        public event Action<string>? FileDeleted;

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
            // DUPLICATE PREVENTION: Ignore if file was recently added
            if (_recentlyAddedFiles.Contains(e.FullPath))
            {
                Console.WriteLine($"[FileMonitor] DUPLICATE IGNORED: {Path.GetFileName(e.FullPath)}");
                return;
            }

            if (FileHelper.IsValidScreenshot(e.FullPath, _config.AllowedExtensions))
            {
                // Longer delay to ensure file is fully written (screenshots can be large)
                System.Threading.Thread.Sleep(1500);

                if (File.Exists(e.FullPath) && !_trackedFiles.Contains(e.FullPath))
                {
                    // Mark file as recently added to prevent duplicate processing
                    _recentlyAddedFiles.Add(e.FullPath);
                    
                    // Remove from recently added list after timeout
                    _ = System.Threading.Tasks.Task.Delay(DUPLICATE_TIMEOUT_MS)
                        .ContinueWith(_ => _recentlyAddedFiles.Remove(e.FullPath));

                    _trackedFiles.Add(e.FullPath);

                    var metadata = FileHelper.CreateMetadata(
                        e.FullPath,
                        TimeHelper.CalculateDeleteTime(_config.DeleteThresholdValue, _config.DeleteThresholdUnit)
                    );

                    Console.WriteLine($"[FileMonitor] Detected: {metadata.FileName} at {DateTime.Now:HH:mm:ss.fff}");
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
            // Stop current monitoring
            StopMonitoring();
            
            // Update config reference
            _config = config;
            _trackedFiles.Clear();
            _recentlyAddedFiles.Clear();
            
            Console.WriteLine($"[FileMonitor] Reconfigured - New folder: {config.ScreenshotFolderPath}");
            Console.WriteLine($"[FileMonitor] Delete threshold: {config.DeleteThresholdValue} {config.DeleteThresholdUnit}");
        }
    }
}
