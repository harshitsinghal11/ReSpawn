namespace ReSpawn.Models
{
    public class Game
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public string ExePath { get; set; } = string.Empty;
        public string ProcessName { get; set; } = string.Empty;
        public string IconPath { get; set; } = string.Empty;
        public long TotalPlaytimeSeconds { get; set; } = 0;
        public DateTime? LastPlayed { get; set; }
        public bool IsPlaying { get; set; } = false;
        public List<GameSession> Sessions { get; set; } = new();
    }
}