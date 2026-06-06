using System.Threading;
using System.Windows;
using Hardcodet.Wpf.TaskbarNotification;
using ReSpawn.Helpers;
using ReSpawn.ViewModels;

namespace ReSpawn
{
    public partial class App : Application
    {
        private TaskbarIcon? _trayIcon;

        private static Mutex? _mutex;

        protected override void OnStartup(StartupEventArgs e)
        {
            // Check single instance FIRST before anything else
            _mutex = new Mutex(true, "ReSpawn_SingleInstance_v1", out bool isNewInstance);

            if (!isNewInstance)
            {
                // Don't call base.OnStartup — just exit immediately
                MessageBox.Show(
                    "ReSpawn is already running.\nCheck your system tray.",
                    "Already Running",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                Environment.Exit(0);
                return;
            }

            base.OnStartup(e);
            AppDataHelper.EnsureDirectoriesExist();
            _trayIcon = (TaskbarIcon)FindResource("TrayIcon");

            _trayIcon.TrayMouseDoubleClick += (s, args) =>
            {
                MainWindow?.Show();
                MainWindow?.Activate();
                if (MainWindow != null)
                    MainWindow.WindowState = WindowState.Normal;
            };

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
            _mutex?.ReleaseMutex();
            _mutex?.Dispose();
            base.OnExit(e);
        }

        public void ShowSessionSaved(string gameName, long seconds)
        {
            string time = ReSpawn.Helpers.TimeFormatter.FormatPlaytime(seconds);
            _trayIcon?.ShowBalloonTip(
                "Session Saved",
                $"{gameName} — {time} recorded",
                Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Info);
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