using System;
using System.Linq;
using System.Threading;
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

        // Command line flags
        private bool _setupMode = false;      // --setup: Run wizard only (installer mode)
        private bool _startMinimized = false; // --minimized: Start minimized to tray

        // Single-instance mutex
        private static Mutex? _instanceMutex;
        private const string MutexName = "ScreenshotSweeper_SingleInstance_8F7A3C2E";

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Prevent app from shutting down when main window closes —
            // we'll control shutdown explicitly (tray Exit menu calls Shutdown()).
            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            // Single-instance check: if another instance is running, bring it to front and exit
            _instanceMutex = new Mutex(true, MutexName, out bool createdNew);
            if (!createdNew)
            {
                // Another instance is already running — signal it to show window and exit
                System.Console.WriteLine("[App] Another instance is already running. Exiting.");
                BringExistingInstanceToFront();
                Shutdown(0);
                return;
            }

            // Parse command-line arguments
            _setupMode = e.Args.Contains("--setup", StringComparer.OrdinalIgnoreCase);
            _startMinimized = e.Args.Contains("--minimized", StringComparer.OrdinalIgnoreCase);

            // Initialize config service
            ConfigService = new ConfigService();
            var config = ConfigService.LoadConfig();

            // Setup mode: show wizard and exit (used by installer)
            if (_setupMode)
            {
                var wizard = new SetupWizard();
                wizard.ShowDialog();
                Shutdown(0);
                return;
            }

            // Normal startup: show wizard only if not configured
            if (string.IsNullOrEmpty(config.ScreenshotFolderPath))
            {
                var wizard = new SetupWizard();
                if (wizard.ShowDialog() != true)
                {
                    // User cancelled - exit app
                    Shutdown(0);
                    return;
                }
                // Reload config after user completes wizard
                config = ConfigService.LoadConfig();
            }

            // Initialize other services with the (possibly updated) config
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

            // Create main window
            if (MainWindow == null)
            {
                MainWindow = new MainWindow();
            }

            // Determine if we should start minimized to tray
            bool shouldMinimize = _startMinimized || config.Startup.StartMinimized;

            if (shouldMinimize)
            {
                // Start minimized to system tray — must Show first then Hide for proper initialization
                MainWindow.WindowState = WindowState.Minimized;
                MainWindow.ShowInTaskbar = false;
                MainWindow.Show(); // Show first to initialize the window
                MainWindow.Hide(); // Then hide
                System.Console.WriteLine("[App] Started minimized to system tray (hidden)");
            }
            else
            {
                // Show window normally
                try
                {
                    MainWindow.WindowState = WindowState.Normal;
                    MainWindow.ShowInTaskbar = true;
                    MainWindow.Show();
                    MainWindow.Activate();
                    MainWindow.Topmost = true;
                    MainWindow.Topmost = false;
                }
                catch { /* Non-fatal */ }
            }

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

            // Release single-instance mutex
            if (_instanceMutex != null)
            {
                _instanceMutex.ReleaseMutex();
                _instanceMutex.Dispose();
                _instanceMutex = null;
            }

            base.OnExit(e);
        }

        /// <summary>
        /// Attempts to bring an existing instance's window to the foreground.
        /// </summary>
        private static void BringExistingInstanceToFront()
        {
            try
            {
                // Find existing window by process name and bring to front
                var currentProcess = System.Diagnostics.Process.GetCurrentProcess();
                var processes = System.Diagnostics.Process.GetProcessesByName(currentProcess.ProcessName);
                foreach (var proc in processes)
                {
                    if (proc.Id != currentProcess.Id && proc.MainWindowHandle != IntPtr.Zero)
                    {
                        // Use Windows API to restore and bring window to front
                        NativeMethods.ShowWindow(proc.MainWindowHandle, NativeMethods.SW_RESTORE);
                        NativeMethods.SetForegroundWindow(proc.MainWindowHandle);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"[App] Failed to bring existing instance to front: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Native Windows API methods for window management.
    /// </summary>
    internal static class NativeMethods
    {
        public const int SW_RESTORE = 9;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);
    }
}
