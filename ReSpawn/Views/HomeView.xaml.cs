using System.IO;
using System.Windows;
using System.Windows.Controls;
using ReSpawn.ViewModels;

namespace ReSpawn.Views
{
    public partial class HomeView : UserControl
    {
        public HomeView()
        {
            InitializeComponent();
        }

        private void Grid_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;

            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            var supported = files.Where(f =>
                f.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) ||
                f.EndsWith(".lnk", StringComparison.OrdinalIgnoreCase) ||
                f.EndsWith(".url", StringComparison.OrdinalIgnoreCase)).ToList();

            if (supported.Count == 0)
            {
                MessageBox.Show(
                    "Please drop .exe, .lnk or .url files only.",
                    "Invalid File",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            if (DataContext is MainViewModel vm)
                foreach (var file in supported)
                    vm.AddGameFromPath(file);
        }
    }
}