using System.IO;
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
        private bool _startupHideDone = false;

        protected override void OnStartup(StartupEventArgs e)
        {
            // Crash logger
            AppDomain.CurrentDomain.UnhandledException += (s, ex) =>
            {
                File.WriteAllText(
                    Path.Combine(Environment.GetFolderPath(
                        Environment.SpecialFolder.Desktop), "ReSpawn_crash.txt"),
                    ex.ExceptionObject.ToString());
            };

            // Single instance check
            _mutex = new Mutex(true, "ReSpawn_SingleInstance_v1", out bool isNewInstance);
            if (!isNewInstance)
            {
                MessageBox.Show(
                    "ReSpawn is already running.\nCheck your system tray.",
                    "Already Running",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                Environment.Exit(0);
                return;
            }

            base.OnStartup(e);

            // If launched at startup — hide after window created
            // Only minimize if launched WITH --minimized flag
            bool launchedFromStartup = Environment
                .GetCommandLineArgs()
                .Contains("--minimized");

            if (launchedFromStartup)
                this.Activated += HideOnStartup;

            AppDataHelper.EnsureDirectoriesExist();
            _trayIcon = (TaskbarIcon)FindResource("TrayIcon");

            // Set tray icon programmatically
            try
            {
                var iconUri = new Uri("pack://application:,,,/Assets/tray-icon.ico");
                var streamInfo = GetResourceStream(iconUri);
                if (streamInfo != null)
                    _trayIcon.Icon = new System.Drawing.Icon(streamInfo.Stream);
            }
            catch { }

            _trayIcon.TrayMouseDoubleClick += (s, args) =>
            {
                MainWindow?.Show();
                MainWindow?.Activate();
                if (MainWindow != null)
                    MainWindow.WindowState = WindowState.Normal;
            };

            this.Activated += OnAppFirstActivated;
        }

        private void HideOnStartup(object? sender, EventArgs e)
        {
            if (_startupHideDone) return;
            _startupHideDone = true;
            MainWindow?.Hide();
            this.Activated -= HideOnStartup;
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
            string time = TimeFormatter.FormatPlaytime(seconds);
            _trayIcon?.ShowBalloonTip(
                "Session Saved",
                $"{gameName} — {time} recorded",
                BalloonIcon.Info);
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