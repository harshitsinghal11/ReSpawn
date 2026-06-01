namespace ReSpawn.Helpers
{
    public static class TimeFormatter
    {
        public static string FormatPlaytime(long totalSeconds)
        {
            long hours = totalSeconds / 3600;
            long minutes = (totalSeconds % 3600) / 60;
            return $"{hours}h {minutes:D2}m";
        }

        public static string FormatLastPlayed(DateTime? lastPlayed)
        {
            if (lastPlayed == null) return "Never";

            DateTime local = lastPlayed.Value.ToLocalTime();
            DateTime today = DateTime.Today;
            int daysAgo = (today - local.Date).Days;

            return daysAgo switch
            {
                0 => "Today",
                1 => "Yesterday",
                <= 7 => $"{daysAgo} days ago",
                _ => local.ToString("MMM dd, yyyy")
            };
        }
    }
}