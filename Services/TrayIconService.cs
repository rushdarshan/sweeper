using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
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
                // Set a fallback icon using an embedded resource or default. System.Drawing.SystemIcons requires referencing System.Drawing.
                try
                {
                    // Prefer the app icon resource if available
                    var exePath = AppDomain.CurrentDomain.BaseDirectory;
                    var iconPath = Path.Combine(exePath, "Resources", "Icons", "sweeper.ico");
                    if (File.Exists(iconPath))
                    {
                        _trayIcon.Icon = new System.Drawing.Icon(iconPath);
                    }
                    else
                    {
                        _trayIcon.Icon = System.Drawing.SystemIcons.Application; // Fallback icon
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[TrayIconService] Could not load custom icon: {ex.Message}");
                    // ignore and keep fallback
                }

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
            var mainWindow = System.Windows.Application.Current?.MainWindow;
            if (mainWindow != null)
            {
                mainWindow.Show();
                mainWindow.WindowState = System.Windows.WindowState.Normal;
                mainWindow.Activate();
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

        private void OnExit(object? sender, EventArgs e)
        {
            if (_trayIcon != null)
            {
                _trayIcon.Visible = false;
            }
            System.Windows.Application.Current?.Shutdown();
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
