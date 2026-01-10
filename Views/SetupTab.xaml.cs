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
using ScreenshotSweeper; // Add root namespace

namespace ScreenshotSweeper.Views
{
    public partial class SetupTab : System.Windows.Controls.Page
    {
        private readonly ConfigService _configService;
        private readonly List<System.Windows.Controls.Button> _presetButtons;

        public SetupTab()
        {
            InitializeComponent();
            _configService = new ConfigService();
            
            _presetButtons = new List<System.Windows.Controls.Button>
            {
                Preset15MinBtn, Preset30MinBtn, Preset1HourBtn, Preset3DaysBtn, PresetForeverBtn
            };
            
            LoadSettings();
        }

        private void LoadSettings()
        {
            var config = _configService.LoadConfig();
            FolderPathTextBox.Text = string.IsNullOrEmpty(config.ScreenshotFolderPath) 
                ? "Select a folder..." 
                : config.ScreenshotFolderPath;
            KeepFolderTextBox.Text = string.IsNullOrEmpty(config.KeepFolderPath) 
                ? "Auto-generated in screenshot folder" 
                : config.KeepFolderPath;
            DeleteTimeValue.Value = config.DeleteThresholdValue;
            TimeUnitSelector.SelectedIndex = (int)config.DeleteThresholdUnit;
            
            UpdatePresetVisuals();
        }

        private void UpdatePresetVisuals(System.Windows.Controls.Button? activeBtn = null)
        {
            // Reset all
            foreach (var btn in _presetButtons)
            {
                btn.ClearValue(System.Windows.Controls.Control.BackgroundProperty);
                btn.ClearValue(System.Windows.Controls.Control.ForegroundProperty);
            }

            // Highlight active
            if (activeBtn != null)
            {
                activeBtn.Background = (Brush)System.Windows.Application.Current.Resources["AccentFillColorDefaultBrush"];
                activeBtn.Foreground = (Brush)System.Windows.Application.Current.Resources["TextOnAccentFillColorPrimaryBrush"];
            }
        }

        // Preset button handlers
        private void SetPreset15Min(object sender, RoutedEventArgs e)
        {
            DeleteTimeValue.Value = 15;
            TimeUnitSelector.SelectedIndex = (int)TimeUnit.Minutes;
            UpdatePresetVisuals((System.Windows.Controls.Button)sender);
        }

        private void SetPreset30Min(object sender, RoutedEventArgs e)
        {
            DeleteTimeValue.Value = 30;
            TimeUnitSelector.SelectedIndex = (int)TimeUnit.Minutes;
            UpdatePresetVisuals((System.Windows.Controls.Button)sender);
        }

        private void SetPreset1Hour(object sender, RoutedEventArgs e)
        {
            DeleteTimeValue.Value = 1;
            TimeUnitSelector.SelectedIndex = (int)TimeUnit.Hours;
            UpdatePresetVisuals((System.Windows.Controls.Button)sender);
        }

        private void SetPreset3Days(object sender, RoutedEventArgs e)
        {
            DeleteTimeValue.Value = 3;
            TimeUnitSelector.SelectedIndex = (int)TimeUnit.Days;
            UpdatePresetVisuals((System.Windows.Controls.Button)sender);
        }

        private void PresetKeepForever(object sender, RoutedEventArgs e)
        {
            DeleteTimeValue.Value = 30;
            TimeUnitSelector.SelectedIndex = (int)TimeUnit.Days;
            StatusMessage.Text = "💡 All files will be kept for 30 days minimum.";
            StatusMessage.Foreground = new SolidColorBrush(Colors.Orange);
            
            UpdatePresetVisuals((System.Windows.Controls.Button)sender);
        }

        private void SaveSettings(object sender, RoutedEventArgs e)
        {
            var config = _configService.LoadConfig();
            
            // Get folder path - use actual value if not placeholder
            string folderPath = FolderPathTextBox.Text;
            if (folderPath == "Select a folder..." || string.IsNullOrWhiteSpace(folderPath))
            {
                StatusMessage.Text = "❌ Please select a screenshot folder";
                StatusMessage.Foreground = new SolidColorBrush(Colors.Red);
                return;
            }
            
            config.ScreenshotFolderPath = folderPath;
            
            // Parse delete time value
            double val = DeleteTimeValue.Value ?? 0;
            
            if (val < 1)
            {
                StatusMessage.Text = "❌ Please enter a valid time value";
                StatusMessage.Foreground = new SolidColorBrush(Colors.Red);
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
                    StatusMessage.Foreground = new SolidColorBrush(Colors.Red);
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
            StatusMessage.Foreground = new SolidColorBrush(Colors.Green);
            KeepFolderTextBox.Text = config.KeepFolderPath;
            
            System.Console.WriteLine($"[SetupTab] Settings saved - Folder: {config.ScreenshotFolderPath}");
        }

        private bool ValidateSettings(AppConfig config)
        {
            if (string.IsNullOrWhiteSpace(config.ScreenshotFolderPath))
            {
                StatusMessage.Text = "❌ Please select a screenshot folder";
                StatusMessage.Foreground = new SolidColorBrush(Colors.Red);
                return false;
            }

            if (!System.IO.Directory.Exists(config.ScreenshotFolderPath))
            {
                StatusMessage.Text = "❌ Selected folder does not exist";
                StatusMessage.Foreground = new SolidColorBrush(Colors.Red);
                return false;
            }

            var (min, max) = TimeHelper.GetValidationRange(config.DeleteThresholdUnit);
            if (config.DeleteThresholdValue < min || config.DeleteThresholdValue > max)
            {
                StatusMessage.Text = $"❌ Value must be between {min} and {max} {config.DeleteThresholdUnit}";
                StatusMessage.Foreground = new SolidColorBrush(Colors.Red);
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
                FolderPathTextBox.Text = dialog.SelectedPath;
                StatusMessage.Text = string.Empty;
            }
        }
    }
}
