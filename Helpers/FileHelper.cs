using System;
using System.Collections.Generic;
using System.IO;
using ScreenshotSweeper.Models;

namespace ScreenshotSweeper.Helpers
{
    /// <summary>
    /// Utility class for file operations and validation
    /// </summary>
    public static class FileHelper
    {
        /// <summary>
        /// Checks if a file is a valid screenshot based on extension
        /// </summary>
        public static bool IsValidScreenshot(string filePath, List<string> allowedExtensions)
        {
            try
            {
                if (!File.Exists(filePath))
                    return false;

                var extension = Path.GetExtension(filePath).ToLower();
                return allowedExtensions.Contains(extension);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets file size in bytes, with retry logic for locked files
        /// </summary>
        public static long GetFileSize(string filePath, int retries = 5)
        {
            for (int i = 0; i < retries; i++)
            {
                try
                {
                    if (!File.Exists(filePath))
                        return 0;

                    // Force refresh the file info to get accurate size
                    var fileInfo = new FileInfo(filePath);
                    fileInfo.Refresh();
                    
                    // If size is 0, the file might still be writing - retry
                    if (fileInfo.Length == 0 && i < retries - 1)
                    {
                        System.Threading.Thread.Sleep(500);
                        continue;
                    }
                    
                    return fileInfo.Length;
                }
                catch (IOException) when (i < retries - 1)
                {
                    System.Threading.Thread.Sleep(500);
                }
                catch
                {
                    return 0;
                }
            }
            return 0;
        }

        /// <summary>
        /// Safely deletes a file with retry logic
        /// </summary>
        public static bool DeleteFile(string filePath, int retries = 3)
        {
            for (int i = 0; i < retries; i++)
            {
                try
                {
                    if (!File.Exists(filePath))
                        return true;

                    File.Delete(filePath);
                    return true;
                }
                catch (IOException) when (i < retries - 1)
                {
                    System.Threading.Thread.Sleep(100);
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }

        /// <summary>
        /// Safely moves a file to the Keep folder
        /// </summary>
        public static bool MoveToKeepFolder(string filePath, string keepFolderPath)
        {
            try
            {
                if (!File.Exists(filePath))
                    return false;

                if (!Directory.Exists(keepFolderPath))
                    Directory.CreateDirectory(keepFolderPath);

                var fileName = Path.GetFileName(filePath);
                var destPath = Path.Combine(keepFolderPath, fileName);

                // Handle duplicate filenames
                int counter = 1;
                while (File.Exists(destPath))
                {
                    var nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                    var ext = Path.GetExtension(fileName);
                    destPath = Path.Combine(keepFolderPath, $"{nameWithoutExt}_{counter}{ext}");
                    counter++;
                }

                File.Move(filePath, destPath, overwrite: false);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Creates a ScreenshotMetadata object from a file
        /// </summary>
        public static ScreenshotMetadata CreateMetadata(string filePath, DateTime scheduledDeleteAt, bool isInKeepFolder = false)
        {
            return new ScreenshotMetadata
            {
                FilePath = filePath,
                FileName = Path.GetFileName(filePath),
                FileSizeBytes = GetFileSize(filePath),
                DetectedAt = DateTime.Now,
                ScheduledDeleteAt = scheduledDeleteAt,
                IsInKeepFolder = isInKeepFolder
            };
        }
    }
}
