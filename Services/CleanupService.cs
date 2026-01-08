using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using ScreenshotSweeper.Helpers;
using ScreenshotSweeper.Models;

namespace ScreenshotSweeper.Services
{
    /// <summary>
    /// Periodically checks tracked files and deletes expired ones
    /// </summary>
    public class CleanupService
    {
        private Timer? _cleanupTimer;
        private readonly FileMonitorService _fileMonitor;
        private readonly NotificationService _notificationService;
        private readonly List<ScreenshotMetadata> _trackedMetadata = new();
        private readonly object _lock = new();

        public event Action<ScreenshotMetadata>? FileDeleted;
        public event Action<List<ScreenshotMetadata>>? StatusUpdated;

        public CleanupService(FileMonitorService fileMonitor, NotificationService notificationService)
        {
            _fileMonitor = fileMonitor;
            _notificationService = notificationService;
            _fileMonitor.FileDetected += OnFileDetected;
            _fileMonitor.FileDeleted += OnFileDeletedFromMonitor;
        }

        /// <summary>
        /// Starts the cleanup timer
        /// </summary>
        public void Start()
        {
            _cleanupTimer = new Timer(Constants.CLEANUP_CHECK_INTERVAL_MS)
            {
                AutoReset = true
            };

            _cleanupTimer.Elapsed += OnCleanupTick;
            _cleanupTimer.Start();

            Console.WriteLine("[CleanupService] Started cleanup timer");
        }

        /// <summary>
        /// Stops the cleanup timer
        /// </summary>
        public void Stop()
        {
            _cleanupTimer?.Stop();
            _cleanupTimer?.Dispose();
            Console.WriteLine("[CleanupService] Stopped cleanup timer");
        }

        /// <summary>
        /// Gets currently tracked files with metadata
        /// </summary>
        public List<ScreenshotMetadata> GetTrackedFiles()
        {
            lock (_lock)
            {
                return _trackedMetadata.Where(m => System.IO.File.Exists(m.FilePath)).ToList();
            }
        }

        private void OnFileDetected(ScreenshotMetadata metadata)
        {
            lock (_lock)
            {
                _trackedMetadata.Add(metadata);
            }
            _notificationService.SendDetectionNotification(metadata);
            StatusUpdated?.Invoke(GetTrackedFiles());
        }

        private void OnFileDeletedFromMonitor(string filePath)
        {
            lock (_lock)
            {
                var metadata = _trackedMetadata.FirstOrDefault(m => m.FilePath == filePath);
                if (metadata != null)
                {
                    _trackedMetadata.Remove(metadata);
                }
            }
            StatusUpdated?.Invoke(GetTrackedFiles());
        }

        private void OnCleanupTick(object? sender, ElapsedEventArgs e)
        {
            List<ScreenshotMetadata> filesToDelete;
            lock (_lock)
            {
                filesToDelete = _trackedMetadata
                    .Where(m => m.IsExpired && System.IO.File.Exists(m.FilePath))
                    .ToList();
            }

            foreach (var metadata in filesToDelete)
            {
                if (FileHelper.DeleteFile(metadata.FilePath))
                {
                    lock (_lock)
                    {
                        _trackedMetadata.Remove(metadata);
                    }
                    Console.WriteLine($"[CleanupService] Deleted: {metadata.FileName}");
                    FileDeleted?.Invoke(metadata);
                    _notificationService.SendDeletionNotification(metadata);
                }
            }

            // Update UI with current status
            StatusUpdated?.Invoke(GetTrackedFiles());
        }

        /// <summary>
        /// Manually update delete time for a specific file
        /// </summary>
        public void UpdateDeleteTime(string filePath, int value, TimeUnit unit)
        {
            lock (_lock)
            {
                var metadata = _trackedMetadata.FirstOrDefault(m => m.FilePath == filePath);
                if (metadata != null)
                {
                    metadata.ScheduledDeleteAt = TimeHelper.CalculateDeleteTime(value, unit);
                    Console.WriteLine($"[CleanupService] Updated delete time for: {metadata.FileName}");
                }
            }
            StatusUpdated?.Invoke(GetTrackedFiles());
        }

        /// <summary>
        /// Move file to keep folder
        /// </summary>
        public void MoveToKeep(string filePath, string keepFolderPath)
        {
            bool moved = false;
            lock (_lock)
            {
                var metadata = _trackedMetadata.FirstOrDefault(m => m.FilePath == filePath);
                if (metadata != null && FileHelper.MoveToKeepFolder(filePath, keepFolderPath))
                {
                    _trackedMetadata.Remove(metadata);
                    Console.WriteLine($"[CleanupService] Moved to keep folder: {metadata.FileName}");
                    moved = true;
                }
            }
            if (moved)
                StatusUpdated?.Invoke(GetTrackedFiles());
        }
    }
}
