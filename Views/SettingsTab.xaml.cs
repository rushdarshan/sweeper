using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;
using ScreenshotSweeper.Helpers;
using ScreenshotSweeper.Services;
using Wpf.Ui.Controls;
using ScreenshotSweeper; 

namespace ScreenshotSweeper.Views
{
    public partial class SettingsTab : System.Windows.Controls.Page
    {
        private const string StartupRegistryKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        private const string AppName = "ScreenshotSweeper";
        private readonly ConfigService _configService;

        public SettingsTab()
        {
            InitializeComponent();
            _configService = new ConfigService();
            LoadSettings();
        }

        private void LoadSettings()
        {
            var config = _configService.LoadConfig();
            ShowDetectionNotif.IsChecked = config.Notifications.ShowOnDetection;
            ShowDeletionNotif.IsChecked = config.Notifications.ShowOnDeletion;
            PlaySoundNotif.IsChecked = config.Notifications.PlaySound;
            LaunchOnStartup.IsChecked = config.Startup.LaunchOnStartup;
            StartMinimized.IsChecked = config.Startup.StartMinimized;
            ShowInTaskbar.IsChecked = config.Startup.ShowInTaskbar ?? true;
            
            UpdateMonitoringStatus(config.Monitoring.IsActive);
        }

        private void ToggleMonitoring(object sender, RoutedEventArgs e)
        {
            var config = _configService.LoadConfig();
            config.Monitoring.IsActive = !config.Monitoring.IsActive;
            _configService.SaveConfig(config);
            
            UpdateMonitoringStatus(config.Monitoring.IsActive);
            
            if (config.Monitoring.IsActive)
                App.FileMonitorService?.StartMonitoring();
            else
                App.FileMonitorService?.StopMonitoring();
        }

        private void UpdateMonitoringStatus(bool isActive)
        {
            if (isActive)
            {
                StatusLabel.Text = "Monitoring Active";
                StatusLabel.Foreground = new SolidColorBrush(Colors.Green);
                ToggleMonitoringBtn.Content = "Pause Monitoring";
            }
            else
            {
                StatusLabel.Text = "Monitoring Paused";
                StatusLabel.Foreground = new SolidColorBrush(Colors.Orange);
                ToggleMonitoringBtn.Content = "Resume Monitoring";
            }
        }

        private void SaveSettings(object sender, RoutedEventArgs e)
        {
            var config = _configService.LoadConfig();
            config.Notifications.ShowOnDetection = ShowDetectionNotif.IsChecked == true;
            config.Notifications.ShowOnDeletion = ShowDeletionNotif.IsChecked == true;
            config.Notifications.PlaySound = PlaySoundNotif.IsChecked == true;
            config.Startup.LaunchOnStartup = LaunchOnStartup.IsChecked == true;
            config.Startup.StartMinimized = StartMinimized.IsChecked == true;
            config.Startup.ShowInTaskbar = ShowInTaskbar.IsChecked == true;

            _configService.SaveConfig(config);

            // Register or unregister Windows startup
            SetWindowsStartup(config.Startup.LaunchOnStartup);
            
            SaveStatus.Text = "Settings saved!";
            
            App.NotificationService?.UpdateConfig(config);
        }

        /// <summary>
        /// Add or remove the app from Windows startup (HKCU Run key).
        /// </summary>
        private void SetWindowsStartup(bool enable)
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(StartupRegistryKey, true);
                if (key == null) return;

                if (enable)
                {
                    var exePath = Environment.ProcessPath;
                    if (!string.IsNullOrEmpty(exePath))
                    {
                        // Start minimized so it goes straight to tray
                        key.SetValue(AppName, $"\"{exePath}\" --minimized");
                        Console.WriteLine($"[SettingsTab] Added to Windows startup: {exePath}");
                    }
                }
                else
                {
                    key.DeleteValue(AppName, false);
                    Console.WriteLine("[SettingsTab] Removed from Windows startup");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SettingsTab] Failed to configure Windows startup: {ex.Message}");
            }
        }

        private void ResetSettings(object sender, RoutedEventArgs e)
        {
            var config = new Models.AppConfig();
            _configService.SaveConfig(config);
            LoadSettings();
            SaveStatus.Text = "Reset to defaults.";
        }

        private void ClearTracked(object sender, RoutedEventArgs e)
        {
            App.FileMonitorService?.Reconfigure(_configService.LoadConfig());
            SaveStatus.Text = "Tracked files cleared.";
        }
    }
}
