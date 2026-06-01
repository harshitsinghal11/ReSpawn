using ReSpawn.Helpers;

namespace ReSpawn.Tests
{
    public class TimeFormatterTests
    {
        [Fact]
        public void FormatPlaytime_ZeroSeconds_ReturnsZeroHoursZeroMinutes()
            => Assert.Equal("0h 00m", TimeFormatter.FormatPlaytime(0));

        [Fact]
        public void FormatPlaytime_90Seconds_ReturnsZeroHoursOneMinute()
            => Assert.Equal("0h 01m", TimeFormatter.FormatPlaytime(90));

        [Fact]
        public void FormatPlaytime_LargeValue_ReturnsCorrectHoursAndMinutes()
            => Assert.Equal("2h 30m", TimeFormatter.FormatPlaytime(9000));

        [Fact]
        public void FormatLastPlayed_Null_ReturnsNever()
            => Assert.Equal("Never", TimeFormatter.FormatLastPlayed(null));

        [Fact]
        public void FormatLastPlayed_Today_ReturnsToday()
            => Assert.Equal("Today", TimeFormatter.FormatLastPlayed(DateTime.UtcNow));

        [Fact]
        public void FormatLastPlayed_Yesterday_ReturnsYesterday()
            => Assert.Equal("Yesterday", TimeFormatter.FormatLastPlayed(DateTime.UtcNow.AddDays(-1)));

        [Fact]
        public void FormatLastPlayed_OldDate_ReturnsFormattedDate()
        {
            var old = new DateTime(2025, 12, 15, 0, 0, 0, DateTimeKind.Utc);
            Assert.Equal("Dec 15, 2025", TimeFormatter.FormatLastPlayed(old));
        }
    }
}