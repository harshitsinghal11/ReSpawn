using System.Windows;

namespace ReSpawn
{
    public partial class MainWindow : Window
    {
        private bool _shownTrayHint = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();

            if (!_shownTrayHint)
            {
                _shownTrayHint = true;
                var tray = (Hardcodet.Wpf.TaskbarNotification.TaskbarIcon)
                    Application.Current.FindResource("TrayIcon");
                tray.ShowBalloonTip("ReSpawn",
                    "ReSpawn is still running. Right-click the tray icon to exit.",
                    Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Info);
            }
        }
    }
}