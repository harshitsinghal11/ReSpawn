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
    }
}