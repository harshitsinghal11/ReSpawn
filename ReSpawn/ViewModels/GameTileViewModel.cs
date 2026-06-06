using System.ComponentModel;
using ReSpawn.Helpers;

namespace ReSpawn.ViewModels
{
    public class GameTileViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string IconPath { get; set; } = string.Empty;
        public string ExePath { get; set; } = string.Empty;
        public string ProcessName { get; set; } = string.Empty;
        public long TotalPlaytimeSeconds { get; set; }
        public DateTime? LastPlayed { get; set; }

        public string FormattedPlaytime =>
            TimeFormatter.FormatPlaytime(TotalPlaytimeSeconds);

        public string FormattedLastPlayed =>
            TimeFormatter.FormatLastPlayed(LastPlayed);

        private bool _isPlaying;
        public bool IsPathBroken =>
    !string.IsNullOrEmpty(ExePath) &&
    !ExePath.StartsWith("steam://", StringComparison.OrdinalIgnoreCase) &&
    !System.IO.File.Exists(ExePath);
        public bool IsPlaying
        {
            get => _isPlaying;
            set
            {
                _isPlaying = value;
                OnPropertyChanged(nameof(IsPlaying));
                OnPropertyChanged(nameof(BadgeVisible));
                OnPropertyChanged(nameof(StatusText));
            }
        }

        public bool BadgeVisible => _isPlaying;
        public string StatusText => _isPlaying ? "Now Playing" : FormattedLastPlayed;
    }
}