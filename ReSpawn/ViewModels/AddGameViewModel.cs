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
        private bool _isSteamGame = false;
        public bool IsSteamGame
        {
            get => _isSteamGame;
            set { _isSteamGame = value; OnPropertyChanged(nameof(IsSteamGame)); }
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
          (!string.IsNullOrWhiteSpace(ExePath)));
            CancelCommand = new RelayCommand(() => CloseAction?.Invoke());

        }

        private void OnBrowse()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Supported Files|*.exe;*.lnk;*.url|" +
                         "Executables|*.exe|" +
                         "Shortcuts|*.lnk|" +
                         "URL Shortcuts|*.url",
                Title = "Select Game or Shortcut"
            };

            if (dialog.ShowDialog() != true) return;

            string selectedPath = dialog.FileName;
            string resolvedExePath = selectedPath;
            string gameName = Path.GetFileNameWithoutExtension(selectedPath);

            // Handle .lnk shortcuts
            if (selectedPath.EndsWith(".lnk", StringComparison.OrdinalIgnoreCase))
            {
                string? target = ShortcutResolver.ResolveLnk(selectedPath);
                if (!string.IsNullOrEmpty(target))
                    resolvedExePath = target;
            }

            // Handle .url Steam shortcuts
            if (selectedPath.EndsWith(".url", StringComparison.OrdinalIgnoreCase))
            {
                string[] lines = File.ReadAllLines(selectedPath);
                string? urlLine = lines.FirstOrDefault(l =>
                    l.StartsWith("URL=", StringComparison.OrdinalIgnoreCase));

                if (urlLine != null)
                {
                    resolvedExePath = urlLine.Substring(4); // Remove "URL="
                    IsSteamGame = resolvedExePath.StartsWith("steam://",
                        StringComparison.OrdinalIgnoreCase);
                }
            }

            ExePath = resolvedExePath;
            Name = gameName;
            ProcessName = IsSteamGame
                ? gameName
                : Path.GetFileNameWithoutExtension(resolvedExePath);

            // Extract icon
            var extractor = new IconExtractor();
            string tempId = Guid.NewGuid().ToString();
            if (!IsSteamGame && File.Exists(resolvedExePath))
                IconPath = extractor.Extract(resolvedExePath, tempId);
        }

        private void OnConfirm()
        {
            // Check for duplicate process name
            var existing = _gameService.LoadGames()
                .FirstOrDefault(g => g.ProcessName
                    .ToLower() == ProcessName.ToLower());

            if (existing != null)
            {
                var result = System.Windows.MessageBox.Show(
                    $"⚠️ \"{existing.Name}\" already uses process name \"{ProcessName}\".\n\n" +
                    "ReSpawn may track both games as the same session.\n\n" +
                    "Add anyway?",
                    "Duplicate Process Name",
                    System.Windows.MessageBoxButton.YesNo,
                    System.Windows.MessageBoxImage.Warning);

                if (result == System.Windows.MessageBoxResult.No)
                    return;
            }

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