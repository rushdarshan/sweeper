using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Interop;
using Wpf.Ui.Controls;
using Wpf.Ui.Appearance;
using ScreenshotSweeper.Views;
using System;
using System.ComponentModel;

namespace ScreenshotSweeper
{
    public partial class MainWindow : FluentWindow
    {
        private const int WM_SYSCOMMAND = 0x0112;
        private const int SC_CLOSE = 0xF060;

        public MainWindow()
        {
            InitializeComponent();
            
            // Navigate to Monitor tab on startup
            Loaded += MainWindow_Loaded;
            SourceInitialized += MainWindow_SourceInitialized;
        }

        private void MainWindow_SourceInitialized(object? sender, EventArgs e)
        {
            // Hook into window messages to intercept close
            var hwndSource = PresentationSource.FromVisual(this) as HwndSource;
            hwndSource?.AddHook(WndProc);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            // Intercept the close command (X button, Alt+F4, taskbar close)
            if (msg == WM_SYSCOMMAND && (wParam.ToInt32() & 0xFFF0) == SC_CLOSE)
            {
                handled = true; // Prevent Windows from closing the window
                HideToTray();
                return IntPtr.Zero;
            }
            return IntPtr.Zero;
        }

        /// <summary>
        /// Handle the TitleBar close button click event - this prevents TitleBar from calling Close()
        /// </summary>
        private void TitleBar_CloseClicked(TitleBar sender, RoutedEventArgs args)
        {
            Console.WriteLine("[MainWindow] TitleBar_CloseClicked - intercepting close, hiding to tray instead");
            args.Handled = true; // Mark as handled to prevent further processing
            HideToTray();
        }

        private void HideToTray()
        {
            Console.WriteLine("[MainWindow] HideToTray called - hiding window");
            try
            {
                this.ShowInTaskbar = false;
                this.WindowState = WindowState.Minimized;
                this.Hide();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MainWindow] Error hiding: {ex.Message}");
            }
        }

        /// <summary>
        /// Backup: Override OnClosing in case WndProc doesn't catch it
        /// </summary>
        protected override void OnClosing(CancelEventArgs e)
        {
            Console.WriteLine("[MainWindow] OnClosing fired - cancelling and hiding to tray");
            e.Cancel = true;
            HideToTray();
            // Don't call base - we're cancelling
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Navigate to Monitor tab as the default page
            NavView.Navigate(typeof(MonitorTab));
            Console.WriteLine("[MainWindow] Initial navigation to MonitorTab complete");
        }

        private void NavView_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // Find the ScrollViewer in the current page content and scroll it
            var scrollViewer = FindDescendant<ScrollViewer>(NavView);
            if (scrollViewer != null)
            {
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - (e.Delta / 2.0));
                e.Handled = true;
            }
        }

        private static T? FindDescendant<T>(DependencyObject parent) where T : DependencyObject
        {
            int childCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T found)
                    return found;
                
                var result = FindDescendant<T>(child);
                if (result != null)
                    return result;
            }
            return null;
        }

        /// <summary>
        /// Restores the window from the system tray.
        /// Called by TrayIconService when user double-clicks the tray icon.
        /// </summary>
        public void RestoreFromTray()
        {
            try
            {
                this.Show();
                this.ShowInTaskbar = true;
                this.WindowState = WindowState.Normal;
                this.Activate();
                // Temporarily set topmost to force focus
                this.Topmost = true;
                this.Topmost = false;
                Console.WriteLine("[MainWindow] Restored from system tray");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MainWindow] Failed to restore from tray: {ex.Message}");
            }
        }
    }
}
