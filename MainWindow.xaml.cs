using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Wpf.Ui.Controls;
using Wpf.Ui.Appearance;
using ScreenshotSweeper.Views;
using System;

namespace ScreenshotSweeper
{
    public partial class MainWindow : FluentWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            
            // Navigate to Monitor tab on startup
            Loaded += MainWindow_Loaded;
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
    }
}
