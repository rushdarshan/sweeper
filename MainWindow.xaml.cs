using System.Windows;
using Wpf.Ui.Controls;
using Wpf.Ui.Appearance;
using ScreenshotSweeper.Views;
using System.Collections.Generic;
using System;
using System.Linq;

namespace ScreenshotSweeper
{
    public partial class MainWindow : FluentWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            // SystemThemeWatcher removed to enforce Dark theme set in App.xaml
        }

        private void NavView_Loaded(object sender, RoutedEventArgs e)
        {
            // Initial navigation to Monitor tab
            NavView.Navigate(typeof(MonitorTab));
        }

        private void NavView_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (sender is NavigationView navView &&
                navView.SelectedItem is NavigationViewItem selectedItem &&
                selectedItem.TargetPageType != null)
            {
                var instance = Activator.CreateInstance(selectedItem.TargetPageType);
                if (instance != null)
                {
                    ContentFrame.Navigate(instance);
                }
            }
        }
    }
}
