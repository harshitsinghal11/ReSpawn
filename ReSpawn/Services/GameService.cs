using System.IO;
using System.Text.Json;
using ReSpawn.Helpers;
using ReSpawn.Models;

namespace ReSpawn.Services
{
    public class GameService
    {
        private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };
        private readonly string _dataPath;

        public GameService(string? dataPath = null)
        {
            _dataPath = dataPath ?? AppDataHelper.GetAppDataPath();
        }

        private string GamesFilePath =>
            Path.Combine(_dataPath, Constants.GamesFileName);

        /// <summary>Loads all games from games.json. Returns empty list if missing or corrupt.</summary>
        public List<Game> LoadGames()
        {
            string path = GamesFilePath;

            if (!File.Exists(path))
                return new List<Game>();

            try
            {
                string json = File.ReadAllText(path);
                return JsonSerializer.Deserialize<List<Game>>(json) ?? new List<Game>();
            }
            catch
            {
                if (File.Exists(path))
                    File.Copy(path, path + ".bak", overwrite: true);
                return new List<Game>();
            }
        }

        /// <summary>Saves all games to games.json atomically.</summary>
        public void SaveGames(List<Game> games)
        {
            AppDataHelper.EnsureDirectoriesExist();
            string json = JsonSerializer.Serialize(games, _jsonOptions);
            AtomicFileWriter.WriteAllText(GamesFilePath, json);
        }

        /// <summary>Adds a new game to the library.</summary>
        public void AddGame(Game game)
        {
            if (string.IsNullOrEmpty(game.Id))
                game.Id = Guid.NewGuid().ToString();

            var games = LoadGames();
            games.Add(game);
            SaveGames(games);
        }

        /// <summary>Updates an existing game by ID using a mutate action.</summary>
        public void UpdateGame(string id, Action<Game> mutate)
        {
            var games = LoadGames();
            var game = games.FirstOrDefault(g => g.Id == id)
                ?? throw new KeyNotFoundException($"Game with ID {id} not found.");
            mutate(game);
            SaveGames(games);
        }

        /// <summary>Removes a game and deletes its cached icon.</summary>
        public void RemoveGame(string id)
        {
            var games = LoadGames();
            var game = games.FirstOrDefault(g => g.Id == id)
                ?? throw new KeyNotFoundException($"Game with ID {id} not found.");

            if (File.Exists(game.IconPath))
                File.Delete(game.IconPath);

            games.Remove(game);
            SaveGames(games);
        }

        /// <summary>Appends a session and updates playtime totals.</summary>
        public void AppendSession(string gameId, GameSession session)
        {
            UpdateGame(gameId, game =>
            {
                game.Sessions.Add(session);
                game.TotalPlaytimeSeconds += session.DurationSeconds;
                game.LastPlayed = session.End;
                game.IsPlaying = false;
            });
        }
    }
}