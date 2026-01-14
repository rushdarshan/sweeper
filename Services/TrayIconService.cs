using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Windows;
using System.Windows.Resources;
using ScreenshotSweeper.Helpers;
using ScreenshotSweeper.Models;

namespace ScreenshotSweeper.Services
{
    /// <summary>
    /// Manages the Windows system tray icon and context menu
    /// </summary>
    public class TrayIconService : IDisposable
    {
        private NotifyIcon? _trayIcon; // Nullable
        private readonly ConfigService _configService;
        private bool _isActive;

        public TrayIconService(ConfigService configService)
        {
            _configService = configService;
            InitializeTrayIcon();
        }

        /// <summary>
        /// Initialize tray icon with context menu
        /// </summary>
        private void InitializeTrayIcon()
        {
            try
            {
                _trayIcon = new NotifyIcon();
                _trayIcon.Visible = true;
                _trayIcon.Text = "ScreenshotSweeper - Initializing";

                // Load tray icon (prefers embedded pack resource; falls back to disk; last fallback is system icon)
                _trayIcon.Icon = LoadTrayIcon();

                var contextMenu = new ContextMenuStrip();

                var openItem = new ToolStripMenuItem("📂 Open App", null, OnOpenApp);
                var pauseItem = new ToolStripMenuItem("⏸️ Pause Monitoring", null, OnToggleMonitoring);
                var settingsItem = new ToolStripMenuItem("⚙️ Settings", null, OnOpenSettings);
                var exitItem = new ToolStripMenuItem("❌ Exit", null, OnExit);

                contextMenu.Items.Add(openItem);
                contextMenu.Items.Add(pauseItem);
                contextMenu.Items.Add(new ToolStripSeparator());
                contextMenu.Items.Add(settingsItem);
                contextMenu.Items.Add(new ToolStripSeparator());
                contextMenu.Items.Add(exitItem);

                _trayIcon.ContextMenuStrip = contextMenu;
                _trayIcon.DoubleClick += (s, e) => OnOpenApp(null, EventArgs.Empty);

                _isActive = true;
                Console.WriteLine("[TrayIconService] Initialized");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TrayIconService] Error initializing: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates tray icon status and tooltip
        /// </summary>
        public void UpdateStatus(int fileCount, long totalBytes, bool isActive)
        {
            if (_trayIcon == null)
                return;

            _isActive = isActive;
            var statusText = isActive ? "Active" : "Paused";
            var sizeText = FormatBytes(totalBytes);

            _trayIcon.Text = $"ScreenshotSweeper\n{fileCount} files | {sizeText}\n{statusText}";
            
            // Update context menu pause/resume text
            if (_trayIcon.ContextMenuStrip?.Items[1] is ToolStripMenuItem pauseItem)
            {
                pauseItem.Text = isActive ? "⏸️ Pause Monitoring" : "▶️ Resume Monitoring";
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

        private void OnOpenApp(object? sender, EventArgs e)
        {
            var mainWindow = System.Windows.Application.Current?.MainWindow as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.RestoreFromTray();
            }
        }

        private void OnToggleMonitoring(object? sender, EventArgs e)
        {
            var config = _configService.LoadConfig();
            config.Monitoring.IsActive = !config.Monitoring.IsActive;
            _configService.SaveConfig(config);

            Console.WriteLine($"[TrayIconService] Monitoring toggled: {config.Monitoring.IsActive}");
        }

        private void OnOpenSettings(object? sender, EventArgs e)
        {
            OnOpenApp(null, EventArgs.Empty);
            // TODO: Navigate to settings tab in main window
        }

        /// <summary>
        /// Show a balloon tip in the system tray to inform the user.
        /// </summary>
        public void ShowInfo(string title, string message, int timeoutMs = 3000)
        {
            if (_trayIcon == null) return;
            try
            {
                _trayIcon.BalloonTipTitle = title;
                _trayIcon.BalloonTipText = message;
                _trayIcon.ShowBalloonTip(timeoutMs);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TrayIconService] Failed to show balloon tip: {ex.Message}");
            }
        }

        private void OnExit(object? sender, EventArgs e)
        {
            if (_trayIcon != null)
            {
                _trayIcon.Visible = false;
            }
            System.Windows.Application.Current?.Shutdown();
        }

        private System.Drawing.Icon LoadTrayIcon()
        {
            // Try pack resource first (works even when running single-file)
            try
            {
                var uri = new Uri("pack://application:,,,/Resources/Icons/sweeper.ico");
                StreamResourceInfo? sri = System.Windows.Application.GetResourceStream(uri);
                if (sri != null)
                {
                    using var stream = sri.Stream;
                    return new System.Drawing.Icon(stream);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TrayIconService] pack icon load failed: {ex.Message}");
            }

            // Fallback to file on disk (for unpacked/published builds)
            try
            {
                var exePath = AppDomain.CurrentDomain.BaseDirectory;
                var iconPath = Path.Combine(exePath, "Resources", "Icons", "sweeper.ico");
                if (File.Exists(iconPath))
                {
                    return new System.Drawing.Icon(iconPath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TrayIconService] file icon load failed: {ex.Message}");
            }

            // Last resort: generic application icon
            return System.Drawing.SystemIcons.Application;
        }

        public void Dispose()
        {
            if (_trayIcon != null)
            {
                _trayIcon.Visible = false;
                _trayIcon.Dispose();
            }
        }
    }
}
