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

        // ← Now notifies FormattedPlaytime when changed
        private long _totalPlaytimeSeconds;
        public long TotalPlaytimeSeconds
        {
            get => _totalPlaytimeSeconds;
            set
            {
                _totalPlaytimeSeconds = value;
                OnPropertyChanged(nameof(TotalPlaytimeSeconds));
                OnPropertyChanged(nameof(FormattedPlaytime));
            }
        }

        // ← Now notifies FormattedLastPlayed and StatusText when changed
        private DateTime? _lastPlayed;
        public DateTime? LastPlayed
        {
            get => _lastPlayed;
            set
            {
                _lastPlayed = value;
                OnPropertyChanged(nameof(LastPlayed));
                OnPropertyChanged(nameof(FormattedLastPlayed));
                OnPropertyChanged(nameof(StatusText));
            }
        }

        public string FormattedPlaytime =>
            TimeFormatter.FormatPlaytime(TotalPlaytimeSeconds);

        public string FormattedLastPlayed =>
            TimeFormatter.FormatLastPlayed(LastPlayed);

        public bool IsPathBroken =>
            !string.IsNullOrEmpty(ExePath) &&
            !ExePath.StartsWith("steam://", StringComparison.OrdinalIgnoreCase) &&
            !System.IO.File.Exists(ExePath);

        private bool _isPlaying;
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