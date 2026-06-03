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
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                var exeFiles = files.Where(f =>
                    f.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)).ToList();

                if (exeFiles.Count == 0)
                {
                    MessageBox.Show("Please drop .exe files only.",
                        "Invalid File", MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                if (DataContext is MainViewModel vm)
                    foreach (var exe in exeFiles)
                        vm.AddGameFromPath(exe);
            }
        }
    }
}