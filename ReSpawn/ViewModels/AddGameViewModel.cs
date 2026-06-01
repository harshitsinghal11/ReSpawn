using System.ComponentModel;
using System.IO;
using System.Windows.Input;
using Microsoft.Win32;
using ReSpawn.Helpers;
using ReSpawn.Models;
using ReSpawn.Services;

namespace ReSpawn.ViewModels
{
    public class AddGameViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private readonly GameService _gameService;
        public Action? CloseAction { get; set; }

        private string _exePath = string.Empty;
        public string ExePath
        {
            get => _exePath;
            set { _exePath = value; OnPropertyChanged(nameof(ExePath)); }
        }

        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(nameof(Name)); }
        }

        private string _processName = string.Empty;
        public string ProcessName
        {
            get => _processName;
            set { _processName = value; OnPropertyChanged(nameof(ProcessName)); }
        }

        private string _iconPath = "Assets/default-icon.png";
        public string IconPath
        {
            get => _iconPath;
            set { _iconPath = value; OnPropertyChanged(nameof(IconPath)); }
        }

        public ICommand BrowseCommand { get; }
        public ICommand ConfirmCommand { get; }
        public ICommand CancelCommand { get; }

        public AddGameViewModel(GameService gameService)
        {
            _gameService = gameService;
            BrowseCommand = new RelayCommand(OnBrowse);
            ConfirmCommand = new RelayCommand(OnConfirm,
                () => !string.IsNullOrWhiteSpace(Name) &&
                      !string.IsNullOrWhiteSpace(ProcessName) &&
                      File.Exists(ExePath));
            CancelCommand = new RelayCommand(() => CloseAction?.Invoke());
        }

        private void OnBrowse()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Executables|*.exe",
                Title = "Select Game Executable"
            };
            if (dialog.ShowDialog() == true)
            {
                ExePath = dialog.FileName;
                Name = Path.GetFileNameWithoutExtension(dialog.FileName);
                ProcessName = Path.GetFileNameWithoutExtension(dialog.FileName);
            }
        }

        private void OnConfirm()
        {
            var game = new Game
            {
                Name = Name,
                ExePath = ExePath,
                ProcessName = ProcessName,
                IconPath = IconPath
            };
            _gameService.AddGame(game);
            CloseAction?.Invoke();
        }
    }
}