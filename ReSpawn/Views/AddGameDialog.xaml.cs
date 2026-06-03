using System.Windows;
using ReSpawn.Services;
using ReSpawn.ViewModels;

namespace ReSpawn.Views
{
    public partial class AddGameDialog : Window
    {
        public AddGameDialog(GameService gameService)
        {
            InitializeComponent();
            var vm = new AddGameViewModel(gameService);
            vm.CloseAction = () => this.Close();
            DataContext = vm;
        }
        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                var vm = DataContext as AddGameViewModel;
                if (vm == null) return;

                // Only add if exe is already selected
                if (!string.IsNullOrEmpty(vm.ExePath) &&
                    vm.ConfirmCommand.CanExecute(null))
                {
                    vm.ConfirmCommand.Execute(null);
                    e.Handled = true;
                }
            }
        }
    }
}