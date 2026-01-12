using System;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using Microsoft.Win32;
using ScreenshotSweeper.Models;

namespace ScreenshotSweeper.Views
{
    public partial class SetupWizard : Window
    {
        private const string StartupRegistryKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        private const string AppName = "ScreenshotSweeper";

        public SetupWizard()
        {
            InitializeComponent();
            LoadCurrentSettings();
        }

        private void LoadCurrentSettings()
        {
            var config = ScreenshotSweeper.App.ConfigService?.LoadConfig();
            if (config != null)
            {
                // Set default screenshot folder if not configured
                if (string.IsNullOrEmpty(config.ScreenshotFolderPath))
                {
                    var defaultPath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                        "Screenshots");
                    FolderPathText.Text = defaultPath;
                }
                else
                {
                    FolderPathText.Text = config.ScreenshotFolderPath;
                }

                StartMinimizedCheck.IsChecked = config.Startup?.StartMinimized ?? true;
                RunAtStartupCheck.IsChecked = config.Startup?.LaunchOnStartup ?? true;
            }
            else
            {
                // Defaults
                FolderPathText.Text = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                    "Screenshots");
                StartMinimizedCheck.IsChecked = true;
                RunAtStartupCheck.IsChecked = true;
            }
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            using var dlg = new FolderBrowserDialog();
            dlg.Description = "Select folder where screenshots are saved";
            dlg.UseDescriptionForTitle = true;
            if (!string.IsNullOrEmpty(FolderPathText.Text) && Directory.Exists(FolderPathText.Text))
            {
                dlg.SelectedPath = FolderPathText.Text;
            }
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                FolderPathText.Text = dlg.SelectedPath;
            }
        }

        private void FinishButton_Click(object sender, RoutedEventArgs e)
        {
            // Validate folder path
            if (string.IsNullOrWhiteSpace(FolderPathText.Text))
            {
                System.Windows.MessageBox.Show("Please select a screenshots folder.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Create folder if it doesn't exist
            try
            {
                if (!Directory.Exists(FolderPathText.Text))
                {
                    Directory.CreateDirectory(FolderPathText.Text);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Could not create folder: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var configService = ScreenshotSweeper.App.ConfigService;
            if (configService == null)
            {
                Console.WriteLine("ConfigService not available");
                DialogResult = false;
                Close();
                return;
            }

            var config = configService.LoadConfig();
            config.ScreenshotFolderPath = FolderPathText.Text;

            // Create Keep folder inside screenshots folder
            config.KeepFolderPath = Path.Combine(FolderPathText.Text, "Keep");
            try
            {
                if (!Directory.Exists(config.KeepFolderPath))
                {
                    Directory.CreateDirectory(config.KeepFolderPath);
                }
            }
            catch { /* Non-critical */ }

            if (config.Startup == null) config.Startup = new StartupSettings();
            config.Startup.StartMinimized = StartMinimizedCheck.IsChecked ?? true;
            config.Startup.LaunchOnStartup = RunAtStartupCheck.IsChecked ?? true;

            configService.SaveConfig(config);

            // Configure Windows startup
            SetWindowsStartup(config.Startup.LaunchOnStartup);

            // Launch app after wizard if requested
            if (LaunchAfterSetupCheck.IsChecked == true)
            {
                try
                {
                    var exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName;
                    if (!string.IsNullOrEmpty(exePath))
                    {
                        var startInfo = new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = exePath,
                            UseShellExecute = true
                        };
                        System.Diagnostics.Process.Start(startInfo);
                        Console.WriteLine("[SetupWizard] Launched app after setup");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[SetupWizard] Failed to launch app: {ex.Message}");
                }
            }

            DialogResult = true;
            Close();
        }

        private void SetWindowsStartup(bool enable)
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(StartupRegistryKey, true);
                if (key == null) return;

                if (enable)
                {
                    // Get the executable path
                    var exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName;
                    if (!string.IsNullOrEmpty(exePath))
                    {
                        // Add --minimized flag so app starts in tray
                        key.SetValue(AppName, $"\"{exePath}\" --minimized");
                        Console.WriteLine($"[SetupWizard] Added to Windows startup: {exePath}");
                    }
                }
                else
                {
                    // Remove from startup
                    key.DeleteValue(AppName, false);
                    Console.WriteLine("[SetupWizard] Removed from Windows startup");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SetupWizard] Failed to configure Windows startup: {ex.Message}");
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
