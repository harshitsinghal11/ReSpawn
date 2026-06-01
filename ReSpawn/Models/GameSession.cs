namespace ReSpawn.Models
{
    public class GameSession
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public long DurationSeconds { get; set; }
    }
}