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
        
        public ICommand ToggleStartupCommand { get; }

        private bool _runAtStartup;
        public bool RunAtStartup
        {
            get => _runAtStartup;
            set
            {
                _runAtStartup = value;
                OnPropertyChanged(nameof(RunAtStartup));
                StartupManager.SetStartup(value);
            }
        }

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
            _runAtStartup = StartupManager.IsStartupEnabled();
            ToggleStartupCommand = new RelayCommand(() => RunAtStartup = !RunAtStartup);

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
        public void AddGameFromPath(string path)
        {
            string resolvedPath = path;
            string gameName = System.IO.Path.GetFileNameWithoutExtension(path);
            bool isSteam = false;

            // Resolve .lnk
            if (path.EndsWith(".lnk", StringComparison.OrdinalIgnoreCase))
            {
                string? target = Helpers.ShortcutResolver.ResolveLnk(path);
                if (!string.IsNullOrEmpty(target))
                    resolvedPath = target;
            }

            // Resolve .url
            if (path.EndsWith(".url", StringComparison.OrdinalIgnoreCase))
            {
                var lines = System.IO.File.ReadAllLines(path);
                var urlLine = lines.FirstOrDefault(l =>
                    l.StartsWith("URL=", StringComparison.OrdinalIgnoreCase));
                if (urlLine != null)
                {
                    resolvedPath = urlLine.Substring(4);
                    isSteam = resolvedPath.StartsWith("steam://",
                        StringComparison.OrdinalIgnoreCase);
                }
            }

            var extractor = new IconExtractor();
            string gameId = Guid.NewGuid().ToString();
            string iconPath = (!isSteam && System.IO.File.Exists(resolvedPath))
                ? extractor.Extract(resolvedPath, gameId)
                : "Assets/default-icon.png";

            var game = new Models.Game
            {
                Id = gameId,
                Name = gameName,
                ExePath = resolvedPath,
                ProcessName = System.IO.Path.GetFileNameWithoutExtension(
                    isSteam ? gameName : resolvedPath),
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

                // Auto hide to tray when game launches
                if (Application.Current.MainWindow != null)
                    Application.Current.MainWindow.Hide();
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
            try
            {
                Process.Start(new ProcessStartInfo(tile.ExePath)
                { UseShellExecute = true });

                // Always hide to tray when launching any game
                Application.Current.MainWindow?.Hide();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    $"Could not launch \"{tile.Name}\".\n\n{ex.Message}",
                    "Launch Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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