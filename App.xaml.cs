using System.Windows;
using Microsoft.Toolkit.Uwp.Notifications;
using ScreenshotSweeper.Helpers;
using ScreenshotSweeper.Models;
using ScreenshotSweeper.Services;
using ScreenshotSweeper.Views;

namespace ScreenshotSweeper
{
    public partial class App : Application
    {
        // Static references to services for access from views
        public static ConfigService? ConfigService { get; private set; }
        public static FileMonitorService? FileMonitorService { get; private set; }
        public static CleanupService? CleanupService { get; private set; }
        public static TrayIconService? TrayService { get; private set; }
        public static NotificationService? NotificationService { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Initialize services
            ConfigService = new ConfigService();
            var config = ConfigService.LoadConfig();

            NotificationService = new NotificationService(config);
            FileMonitorService = new FileMonitorService(config);
            CleanupService = new CleanupService(FileMonitorService, NotificationService);
            TrayService = new TrayIconService(ConfigService);

            // Start monitoring if folder is configured
            if (!string.IsNullOrEmpty(config.ScreenshotFolderPath))
            {
                FileMonitorService.StartMonitoring();
                CleanupService.Start();
                System.Console.WriteLine("[App] Services started successfully");
            }
            else
            {
                System.Console.WriteLine("[App] Screenshot folder not configured - monitoring disabled");
            }

            // Start minimized if configured
            if (config.Startup.StartMinimized && MainWindow != null)
            {
                MainWindow.WindowState = WindowState.Minimized;
                MainWindow.Hide();
            }

            // Ensure the main window is created and visible
            if (MainWindow == null)
            {
                MainWindow = new MainWindow();
            }

            // If the window is hidden or minimized, show and activate it so user can see it
            try
            {
                if (MainWindow.WindowState == WindowState.Minimized)
                    MainWindow.WindowState = WindowState.Normal;

                MainWindow.Show();
                MainWindow.Activate();
                MainWindow.Topmost = true;
                MainWindow.Topmost = false;
            }
            catch { /* Non-fatal */ }

            // Wire cleanup events to update tray
            CleanupService.StatusUpdated += (files) =>
            {
                long totalBytes = 0;
                foreach (var f in files)
                    totalBytes += f.FileSizeBytes;
                TrayService.UpdateStatus(files.Count, totalBytes, true);
            };

            // Handle notification actions
            ToastNotificationManagerCompat.OnActivated += toastArgs =>
            {
                // Parse arguments from notification button clicks
                ToastArguments args = ToastArguments.Parse(toastArgs.Argument);
                
                string? filePath = null;
                if (args.TryGetValue("path", out var path))
                    filePath = path;
                
                if (string.IsNullOrEmpty(filePath))
                    return;
                
                // Handle different actions
                if (args.TryGetValue(Constants.ACTION_SET_TIMER, out var timerValue))
                {
                    int minutes = timerValue switch
                    {
                        "15_min" => 15,
                        "30_min" => 30,
                        "1_hour" => 60,
                        _ => 30
                    };
                    
                    CleanupService?.UpdateDeleteTime(filePath, minutes, TimeUnit.Minutes);
                    System.Console.WriteLine($"[App] Timer set to {minutes} minutes for: {filePath}");
                }
                else if (args.TryGetValue(Constants.ACTION_KEEP, out _))
                {
                    var config = ConfigService?.LoadConfig();
                    if (config != null)
                    {
                        CleanupService?.MoveToKeep(filePath, config.KeepFolderPath);
                        System.Console.WriteLine($"[App] File moved to Keep folder: {filePath}");
                    }
                }
            };
        }

        protected override void OnExit(ExitEventArgs e)
        {
            FileMonitorService?.StopMonitoring();
            CleanupService?.Stop();
            TrayService?.Dispose();
            base.OnExit(e);
        }
    }
}
