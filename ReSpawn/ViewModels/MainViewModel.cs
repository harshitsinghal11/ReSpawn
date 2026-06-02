using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using ReSpawn.Helpers;
using ReSpawn.Models;
using ReSpawn.Services;

namespace ReSpawn.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private readonly GameService _gameService = new();
        private readonly ProcessMonitor _processMonitor;

        public ObservableCollection<GameTileViewModel> Games { get; } = new();
        public bool IsEmpty => Games.Count == 0;

        private string _trayStatusText = "No game running";
        public string TrayStatusText
        {
            get => _trayStatusText;
            set { _trayStatusText = value; OnPropertyChanged(nameof(TrayStatusText)); }
        }

        public ICommand AddGameCommand { get; }
        public ICommand LaunchGameCommand { get; }
        public ICommand RemoveGameCommand { get; }
        public ICommand EditGameCommand { get; }

        public MainViewModel()
        {
            _processMonitor = new ProcessMonitor(_gameService);
            _processMonitor.GameStarted += OnGameStarted;
            _processMonitor.GameStopped += OnGameStopped;

            AddGameCommand = new RelayCommand(OnAddGame);
            LaunchGameCommand = new RelayCommand<GameTileViewModel>(OnLaunchGame);
            RemoveGameCommand = new RelayCommand<GameTileViewModel>(OnRemoveGame);
            EditGameCommand = new RelayCommand<GameTileViewModel>(OnEditGame);

            _processMonitor.Start();
            LoadGamesFromDisk();
        }

        public void StopMonitor() => _processMonitor.Stop();

        public void LoadGamesFromDisk()
        {
            Games.Clear();
            var games = _gameService.LoadGames();
            foreach (var g in games)
                Games.Add(ToViewModel(g));
            OnPropertyChanged(nameof(IsEmpty));
        }

        private GameTileViewModel ToViewModel(Game g) => new()
        {
            Id = g.Id,
            Name = g.Name,
            IconPath = g.IconPath,
            ExePath = g.ExePath,
            ProcessName = g.ProcessName,
            TotalPlaytimeSeconds = g.TotalPlaytimeSeconds,
            LastPlayed = g.LastPlayed,
            IsPlaying = g.IsPlaying
        };

        private void OnGameStarted(object? sender, GameStatusEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var tile = Games.FirstOrDefault(g => g.Id == e.Game.Id);
                if (tile != null) tile.IsPlaying = true;
                TrayStatusText = $"● Now Playing: {e.Game.Name}";
            });
        }

        private void OnGameStopped(object? sender, GameStatusEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var tile = Games.FirstOrDefault(g => g.Id == e.Game.Id);
                if (tile != null)
                {
                    tile.IsPlaying = false;
                    tile.TotalPlaytimeSeconds += e.Session?.DurationSeconds ?? 0;
                    tile.LastPlayed = e.Session?.End;
                    OnPropertyChanged(nameof(tile.FormattedPlaytime));
                    OnPropertyChanged(nameof(tile.FormattedLastPlayed));
                }
                TrayStatusText = "No game running";
            });
        }

        private void OnAddGame()
        {
            var dialog = new Views.AddGameDialog(_gameService);
            dialog.ShowDialog();
            LoadGamesFromDisk();
        }

        private void OnLaunchGame(GameTileViewModel? tile)
        {
            if (tile == null) return;
            if (!PathValidator.Exists(tile.ExePath))
            {
                System.Windows.MessageBox.Show(
                    $"Game file not found at:\n{tile.ExePath}\n\nPlease re-link the executable.",
                    "File Not Found", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            Process.Start(new ProcessStartInfo(tile.ExePath) { UseShellExecute = true });
        }

        private void OnRemoveGame(GameTileViewModel? tile)
        {
            if (tile == null) return;
            var result = System.Windows.MessageBox.Show(
                $"Remove \"{tile.Name}\" from your library?\nAll session history will be deleted.",
                "Remove Game", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                _gameService.RemoveGame(tile.Id);
                LoadGamesFromDisk();
            }
        }

        private void OnEditGame(GameTileViewModel? tile)
        {
            if (tile == null) return;
            var dialog = new Views.EditGameDialog(_gameService, tile.Id);
            dialog.ShowDialog();
            LoadGamesFromDisk();
        }
    }
}