using System.IO;

namespace ReSpawn.Helpers
{
    public static class AppDataHelper
    {
        public static string GetAppDataPath() =>
            Path.Combine(System.Environment.GetFolderPath(
                System.Environment.SpecialFolder.ApplicationData), Constants.AppDataFolder);

        public static string GetIconsPath() =>
            Path.Combine(GetAppDataPath(), Constants.IconsFolder);

        public static string GetGamesFilePath() =>
            Path.Combine(GetAppDataPath(), Constants.GamesFileName);

        public static void EnsureDirectoriesExist()
        {
            Directory.CreateDirectory(GetAppDataPath());
            Directory.CreateDirectory(GetIconsPath());
        }
    }
}