using System.Windows;
using ReSpawn.Services;
using ReSpawn.ViewModels;

namespace ReSpawn.Views
{
    public partial class EditGameDialog : Window
    {
        public EditGameDialog(GameService gameService, string gameId)
        {
            InitializeComponent();
            var vm = new EditGameViewModel(gameService, gameId);
            vm.CloseAction = () => this.Close();
            DataContext = vm;
        }
    }
}