using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Input;
using System.Collections.Generic;
using ScreenshotSweeper.Helpers;
using ScreenshotSweeper.Models;
using ScreenshotSweeper.Services;
using Wpf.Ui.Controls;
using ScreenshotSweeper;

namespace ScreenshotSweeper.Views
{
    public partial class SetupTab : System.Windows.Controls.Page
    {
        private readonly ConfigService _configService;

        public SetupTab()
        {
            InitializeComponent();
            _configService = new ConfigService();
            LoadSettings();
        }

        private void LoadSettings()
        {
            var config = _configService.LoadConfig();
            
            // Display shortened path with tooltip
            if (string.IsNullOrEmpty(config.ScreenshotFolderPath))
            {
                FolderPathTextBox.Text = "Select a folder...";
                FolderPathTooltip.Text = "No folder selected";
            }
            else
            {
                FolderPathTextBox.Text = ShortenPath(config.ScreenshotFolderPath);
                FolderPathTooltip.Text = config.ScreenshotFolderPath;
            }
            
            KeepFolderTextBox.Text = string.IsNullOrEmpty(config.KeepFolderPath) 
                ? "Auto-generated in screenshot folder" 
                : ShortenPath(config.KeepFolderPath);
            DeleteTimeValue.Value = config.DeleteThresholdValue;
            TimeUnitSelector.SelectedIndex = (int)config.DeleteThresholdUnit;
        }

        private string ShortenPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return path;
            
            // Try to shorten user paths (C:\Users\username\... -> ~\...)
            string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            if (path.StartsWith(userProfile, StringComparison.OrdinalIgnoreCase))
            {
                return "~" + path.Substring(userProfile.Length);
            }
            
            // Keep as-is if not in user profile
            return path;
        }

        // Preset handlers for pill-style chips
        private void SetPreset15Min(object sender, MouseButtonEventArgs e)
        {
            DeleteTimeValue.Value = 15;
            TimeUnitSelector.SelectedIndex = (int)TimeUnit.Minutes;
        }

        private void SetPreset30Min(object sender, MouseButtonEventArgs e)
        {
            DeleteTimeValue.Value = 30;
            TimeUnitSelector.SelectedIndex = (int)TimeUnit.Minutes;
        }

        private void SetPreset1Hour(object sender, MouseButtonEventArgs e)
        {
            DeleteTimeValue.Value = 1;
            TimeUnitSelector.SelectedIndex = (int)TimeUnit.Hours;
        }

        private void SetPreset3Days(object sender, MouseButtonEventArgs e)
        {
            DeleteTimeValue.Value = 3;
            TimeUnitSelector.SelectedIndex = (int)TimeUnit.Days;
        }

        private void PresetKeepForever(object sender, MouseButtonEventArgs e)
        {
            DeleteTimeValue.Value = 30;
            TimeUnitSelector.SelectedIndex = (int)TimeUnit.Days;
        }

        private void SaveSettings(object sender, RoutedEventArgs e)
        {
            var config = _configService.LoadConfig();
            
            // Get full folder path from tooltip (stores full path)
            string folderPath = FolderPathTooltip.Text;
            if (folderPath == "No folder selected" || string.IsNullOrWhiteSpace(folderPath))
            {
                StatusMessage.Text = "❌ Please select a screenshot folder";
                StatusMessage.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f38ba8"));
                return;
            }
            
            config.ScreenshotFolderPath = folderPath;
            
            // Parse delete time value
            double val = DeleteTimeValue.Value ?? 0;
            
            if (val < 1)
            {
                StatusMessage.Text = "❌ Please enter a valid time value";
                StatusMessage.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f38ba8"));
                return;
            }

            config.DeleteThresholdValue = (int)val;
            config.DeleteThresholdUnit = (TimeUnit)TimeUnitSelector.SelectedIndex;
            config.KeepFolderPath = System.IO.Path.Combine(config.ScreenshotFolderPath, Constants.KEEP_FOLDER_NAME);

            // Validate settings
            if (!ValidateSettings(config))
                return;

            // Create keep folder if it doesn't exist
            if (!System.IO.Directory.Exists(config.KeepFolderPath))
            {
                try
                {
                    System.IO.Directory.CreateDirectory(config.KeepFolderPath);
                }
                catch (Exception ex)
                {
                    StatusMessage.Text = $"❌ Could not create Keep folder: {ex.Message}";
                    StatusMessage.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f38ba8"));
                    return;
                }
            }

            _configService.SaveConfig(config);
            
            // Restart services with new config
            App.FileMonitorService?.StopMonitoring();
            App.CleanupService?.Stop();
            
            // Reconfigure and restart
            App.FileMonitorService?.Reconfigure(config);
            App.NotificationService?.UpdateConfig(config);
            App.FileMonitorService?.StartMonitoring();
            App.CleanupService?.Start();
            
            StatusMessage.Text = "✅ Settings saved! Monitoring started.";
            StatusMessage.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#a6e3a1"));
            KeepFolderTextBox.Text = ShortenPath(config.KeepFolderPath);
            
            System.Console.WriteLine($"[SetupTab] Settings saved - Folder: {config.ScreenshotFolderPath}");
        }

        private bool ValidateSettings(AppConfig config)
        {
            if (string.IsNullOrWhiteSpace(config.ScreenshotFolderPath))
            {
                StatusMessage.Text = "❌ Please select a screenshot folder";
                StatusMessage.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f38ba8"));
                return false;
            }

            if (!System.IO.Directory.Exists(config.ScreenshotFolderPath))
            {
                StatusMessage.Text = "❌ Selected folder does not exist";
                StatusMessage.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f38ba8"));
                return false;
            }

            var (min, max) = TimeHelper.GetValidationRange(config.DeleteThresholdUnit);
            if (config.DeleteThresholdValue < min || config.DeleteThresholdValue > max)
            {
                StatusMessage.Text = $"❌ Value must be between {min} and {max} {config.DeleteThresholdUnit}";
                StatusMessage.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f38ba8"));
                return false;
            }

            return true;
        }

        private void BrowseFolder(object sender, RoutedEventArgs e)
        {
            using var dialog = new FolderBrowserDialog
            {
                Description = "Select your screenshot folder",
                ShowNewFolderButton = true
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                FolderPathTextBox.Text = ShortenPath(dialog.SelectedPath);
                FolderPathTooltip.Text = dialog.SelectedPath;
                StatusMessage.Text = string.Empty;
            }
        }
    }
}
