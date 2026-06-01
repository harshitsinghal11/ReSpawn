using System.IO;

namespace ReSpawn.Helpers
{
    public static class PathValidator
    {
        public static bool Exists(string exePath) => File.Exists(exePath);
    }
}