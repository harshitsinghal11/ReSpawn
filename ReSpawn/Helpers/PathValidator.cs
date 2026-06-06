using System.IO;

namespace ReSpawn.Helpers
{
    public static class PathValidator
    {
        public static bool Exists(string exePath)
        {
            if (string.IsNullOrEmpty(exePath)) return false;

            // Steam URLs are always valid
            if (exePath.StartsWith("steam://",
                StringComparison.OrdinalIgnoreCase)) return true;

            return File.Exists(exePath);
        }
    }
}