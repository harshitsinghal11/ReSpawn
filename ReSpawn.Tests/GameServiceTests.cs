using System.IO;
using ReSpawn.Models;
using ReSpawn.Services;
using ReSpawn.Helpers;

namespace ReSpawn.Tests
{
    public class GameServiceTests : IDisposable
    {
        private readonly string _testDataPath;
        private readonly GameService _service;

        public GameServiceTests()
        {
            // Use a temp folder for each test run
            _testDataPath = Path.Combine(
                Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_testDataPath);

            Environment.SetEnvironmentVariable(
                "RESPAWN_TEST_PATH", _testDataPath);
            _service = new GameService(_testDataPath);
        }

        [Fact]
        public void LoadGames_WhenFileNotFound_ReturnsEmptyList()
        {
            var games = _service.LoadGames();
            Assert.Empty(games);
        }

        [Fact]
        public void AddGame_AssignsGuidAndPersists()
        {
            var game = new Game { Name = "TestGame", ExePath = "test.exe" };
            _service.AddGame(game);

            var loaded = _service.LoadGames();
            Assert.Single(loaded);
            Assert.NotEmpty(loaded[0].Id);
        }

        [Fact]
        public void RemoveGame_RemovesCorrectEntry()
        {
            var game = new Game { Name = "ToRemove", ExePath = "x.exe" };
            _service.AddGame(game);
            var id = _service.LoadGames()[0].Id;

            _service.RemoveGame(id);
            Assert.Empty(_service.LoadGames());
        }

        [Fact]
        public void AppendSession_UpdatesTotalPlaytimeAndLastPlayed()
        {
            var game = new Game { Name = "GameA", ExePath = "a.exe" };
            _service.AddGame(game);
            var id = _service.LoadGames()[0].Id;

            var session = new GameSession
            {
                Start = DateTime.UtcNow.AddMinutes(-5),
                End = DateTime.UtcNow,
                DurationSeconds = 300
            };
            _service.AppendSession(id, session);

            var updated = _service.LoadGames()[0];
            Assert.Equal(300, updated.TotalPlaytimeSeconds);
            Assert.NotNull(updated.LastPlayed);
        }

        [Fact]
        public void UpdateGame_WithUnknownId_ThrowsKeyNotFoundException()
        {
            Assert.Throws<KeyNotFoundException>(() =>
                _service.UpdateGame("fake-id", g => g.Name = "x"));
        }

        public void Dispose()
        {
            if (Directory.Exists(_testDataPath))
                Directory.Delete(_testDataPath, true);
        }
    }
}