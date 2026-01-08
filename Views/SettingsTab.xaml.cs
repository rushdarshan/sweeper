using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ScreenshotSweeper.Helpers;
using ScreenshotSweeper.Services;
using Wpf.Ui.Controls;
using ScreenshotSweeper; 

namespace ScreenshotSweeper.Views
{
    public partial class SettingsTab : System.Windows.Controls.Page
    {
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
            // Use simple text/color updates to avoid dependency on specific Wpf.Ui enum namespace which caused build errors
            if (isActive)
            {
                StatusLabel.Text = "Monitoring Active";
                StatusLabel.Foreground = new SolidColorBrush(Colors.Green);
                ToggleMonitoringBtn.Content = "Pause Monitoring";
                // Icon update omitted to avoid namespace issues
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

            _configService.SaveConfig(config);
            
            SaveStatus.Text = "Settings saved!";
            
            App.NotificationService?.UpdateConfig(config);
        }

        private void ResetSettings(object sender, RoutedEventArgs e)
        {
            var config = new Models.AppConfig(); // Default values
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
