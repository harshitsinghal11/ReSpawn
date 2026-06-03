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
        public string LibraryStatusText => $"Total Games  |  {Games.Count}";
        private string _trayStatusText = "No game running";
        
        public string TrayStatusText
        {
            get => _trayStatusText;
            set { _trayStatusText = value; OnPropertyChanged(nameof(TrayStatusText)); }
        }

        public ICommand ShutdownCommand { get; }

        public ICommand RefreshCommand { get; }
        public ICommand AddGameCommand { get; }
        public ICommand LaunchGameCommand { get; }
        public ICommand RemoveGameCommand { get; }
        public ICommand EditGameCommand { get; }

        private string _searchText = string.Empty;

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));
                ApplySearch();
            }
        }

        private List<GameTileViewModel> _allGames = new();
        public MainViewModel()
        {
            _processMonitor = new ProcessMonitor(_gameService);
            _processMonitor.GameStarted += OnGameStarted;
            _processMonitor.GameStopped += OnGameStopped;

            RefreshCommand = new RelayCommand(LoadGamesFromDisk);
            AddGameCommand = new RelayCommand(OnAddGame);
            LaunchGameCommand = new RelayCommand<GameTileViewModel>(OnLaunchGame);
            RemoveGameCommand = new RelayCommand<GameTileViewModel>(OnRemoveGame);
            EditGameCommand = new RelayCommand<GameTileViewModel>(OnEditGame);
            ShutdownCommand = new RelayCommand(() => System.Windows.Application.Current.Shutdown());

            _processMonitor.Start();
            LoadGamesFromDisk();
        }

        public void StopMonitor() => _processMonitor.Stop();

        public void LoadGamesFromDisk()
        {
            _allGames = _gameService.LoadGames()
                .Select(g => ToViewModel(g)).ToList();
            ApplySearch();
            OnPropertyChanged(nameof(IsEmpty));
            OnPropertyChanged(nameof(LibraryStatusText));
        }
        public void AddGameFromPath(string exePath)
        {
            var extractor = new IconExtractor();
            string gameId = Guid.NewGuid().ToString();
            string iconPath = extractor.Extract(exePath, gameId);

            var game = new Models.Game
            {
                Id = gameId,
                Name = System.IO.Path.GetFileNameWithoutExtension(exePath),
                ExePath = exePath,
                ProcessName = System.IO.Path.GetFileNameWithoutExtension(exePath),
                IconPath = iconPath
            };
            _gameService.AddGame(game);
            LoadGamesFromDisk();
        }
        private void ApplySearch()
        {
            Games.Clear();
            var filtered = string.IsNullOrWhiteSpace(_searchText)
                ? _allGames
                : _allGames.Where(g => g.Name.Contains(
                    _searchText, StringComparison.OrdinalIgnoreCase)).ToList();

            foreach (var g in filtered)
                Games.Add(g);

            OnPropertyChanged(nameof(IsEmpty));
            OnPropertyChanged(nameof(LibraryStatusText));
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

                // Show balloon notification
                if (e.Session != null && Application.Current is App app)
                    app.ShowSessionSaved(e.Game.Name, e.Session.DurationSeconds);
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