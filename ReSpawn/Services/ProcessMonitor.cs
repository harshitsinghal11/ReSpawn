using System.Diagnostics;
using ReSpawn.Models;

namespace ReSpawn.Services
{
    public class GameStatusEventArgs : EventArgs
    {
        public Game Game { get; set; } = null!;
        public GameSession? Session { get; set; }
    }

    public class ProcessMonitor
    {
        private readonly GameService _gameService;
        private System.Threading.Timer? _timer;
        private readonly Dictionary<string, DateTime> _activeSessions = new();

        public event EventHandler<GameStatusEventArgs>? GameStarted;
        public event EventHandler<GameStatusEventArgs>? GameStopped;

        public ProcessMonitor(GameService gameService)
        {
            _gameService = gameService;
        }

        /// <summary>Starts the background polling timer and cleans orphaned sessions.</summary>
        public void Start()
        {
            CleanOrphanedSessions();
            _timer = new System.Threading.Timer(
                OnTick, null,
                TimeSpan.Zero,
                TimeSpan.FromMilliseconds(Constants.PollIntervalMs));
        }

        /// <summary>Stops the tracker and saves any in-flight sessions.</summary>
        public void Stop()
        {
            _timer?.Dispose();
            _timer = null;

            foreach (var kvp in _activeSessions.ToList())
            {
                var games = _gameService.LoadGames();
                var game = games.FirstOrDefault(g =>
                    g.ProcessName.ToLower() == kvp.Key);
                if (game != null)
                    HandleSessionEnd(game, kvp.Value);
            }
        }

        private void CleanOrphanedSessions()
        {
            var games = _gameService.LoadGames();
            foreach (var game in games.Where(g => g.IsPlaying))
            {
                Debug.WriteLine($"[Startup] Cleared orphaned session for {game.Name}");
                _gameService.UpdateGame(game.Id, g => g.IsPlaying = false);
            }
        }

        private void OnTick(object? state)
        {
            try
            {
                var runningProcesses = Process.GetProcesses()
                    .Select(p => p.ProcessName.ToLower())
                    .ToHashSet();

                var games = _gameService.LoadGames();

                foreach (var game in games)
                {
                    string key = game.ProcessName.ToLower();
                    bool isRunning = runningProcesses.Contains(key);
                    bool isTracked = _activeSessions.ContainsKey(key);

                    // IDLE → PLAYING
                    if (isRunning && !isTracked)
                    {
                        _activeSessions[key] = DateTime.UtcNow;
                        _gameService.UpdateGame(game.Id, g => g.IsPlaying = true);
                        GameStarted?.Invoke(this, new GameStatusEventArgs { Game = game });
                        Debug.WriteLine($"[Tracker] Started: {game.Name}");
                    }

                    // PLAYING → IDLE
                    else if (!isRunning && isTracked)
                    {
                        var sessionStart = _activeSessions[key];
                        HandleSessionEnd(game, sessionStart);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Tracker] Poll error: {ex.Message}");
            }
        }

        private void HandleSessionEnd(Game game, DateTime sessionStart)
        {
            string key = game.ProcessName.ToLower();
            _activeSessions.Remove(key);

            double durationSeconds = (DateTime.UtcNow - sessionStart).TotalSeconds;

            // Discard sessions under minimum
            if (durationSeconds < Constants.MinSessionSeconds)
            {
                Debug.WriteLine($"[Tracker] Discarded short session for {game.Name} ({durationSeconds:F0}s)");
                _gameService.UpdateGame(game.Id, g => g.IsPlaying = false);
                return;
            }

            // Cap sessions over maximum
            if (durationSeconds > Constants.MaxSessionHours * 3600)
            {
                Debug.WriteLine($"[Tracker] Capped session for {game.Name}");
                durationSeconds = Constants.MaxSessionHours * 3600;
            }

            var session = new GameSession
            {
                Id = Guid.NewGuid().ToString(),
                Start = sessionStart,
                End = DateTime.UtcNow,
                DurationSeconds = (long)durationSeconds
            };

            _gameService.AppendSession(game.Id, session);
            Debug.WriteLine($"[Tracker] Saved session for {game.Name}: {durationSeconds:F0}s");

            GameStopped?.Invoke(this, new GameStatusEventArgs
            {
                Game = game,
                Session = session
            });
        }
    }
}