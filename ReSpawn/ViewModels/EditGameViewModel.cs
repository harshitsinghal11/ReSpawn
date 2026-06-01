using System.ComponentModel;
using System.IO;
using System.Windows.Input;
using Microsoft.Win32;
using ReSpawn.Helpers;
using ReSpawn.Services;

namespace ReSpawn.ViewModels
{
    public class EditGameViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private readonly GameService _gameService;
        private readonly string _gameId;
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

        public ICommand BrowseCommand { get; }
        public ICommand ConfirmCommand { get; }
        public ICommand CancelCommand { get; }

        public EditGameViewModel(GameService gameService, string gameId)
        {
            _gameService = gameService;
            _gameId = gameId;

            var game = gameService.LoadGames().First(g => g.Id == gameId);
            Name = game.Name;
            ExePath = game.ExePath;
            ProcessName = game.ProcessName;

            BrowseCommand = new RelayCommand(OnBrowse);
            ConfirmCommand = new RelayCommand(OnConfirm,
                () => !string.IsNullOrWhiteSpace(Name) &&
                      !string.IsNullOrWhiteSpace(ProcessName));
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
            }
        }

        private void OnConfirm()
        {
            _gameService.UpdateGame(_gameId, g =>
            {
                g.Name = Name;
                g.ExePath = ExePath;
                g.ProcessName = ProcessName;
            });
            CloseAction?.Invoke();
        }
    }
}