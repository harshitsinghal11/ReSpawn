using System.Windows;
using Hardcodet.Wpf.TaskbarNotification;
using ReSpawn.Helpers;
using ReSpawn.ViewModels;

namespace ReSpawn
{
    public partial class App : Application
    {
        private TaskbarIcon? _trayIcon;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            AppDataHelper.EnsureDirectoriesExist();
            _trayIcon = (TaskbarIcon)FindResource("TrayIcon");

            // Wire double-click
            _trayIcon.TrayMouseDoubleClick += (s, args) =>
            {
                MainWindow?.Show();
                MainWindow?.Activate();
                if (MainWindow != null)
                    MainWindow.WindowState = WindowState.Normal;
            };

            // Wait for window to be created before binding DataContext
            this.Activated += OnAppFirstActivated;
        }

        private bool _firstActivation = true;
        private void OnAppFirstActivated(object? sender, EventArgs e)
        {
            if (!_firstActivation) return;
            _firstActivation = false;

            if (MainWindow?.DataContext is MainViewModel vm && _trayIcon != null)
                _trayIcon.DataContext = vm;
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (MainWindow?.DataContext is MainViewModel vm)
                vm.StopMonitor();
            _trayIcon?.Dispose();
            base.OnExit(e);
        }

        private void OpenApp_Click(object sender, RoutedEventArgs e)
        {
            MainWindow?.Show();
            MainWindow?.Activate();
            if (MainWindow != null)
                MainWindow.WindowState = WindowState.Normal;
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Shutdown();
        }
    }
}