using System.Windows;
using ReSpawn.Helpers;

namespace ReSpawn
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            AppDataHelper.EnsureDirectoriesExist();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (MainWindow?.DataContext is ViewModels.MainViewModel vm)
                vm.StopMonitor();
            base.OnExit(e);
        }
    }
}