using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using ReSpawn.Helpers;

namespace ReSpawn.Services
{
    public class IconExtractor
    {
        /// <summary>Extracts icon from .exe and saves as PNG. Returns fallback path on failure.</summary>
        public string Extract(string exePath, string gameId)
        {
            string outputPath = Path.Combine(
                AppDataHelper.GetIconsPath(), $"{gameId}.png");

            try
            {
                using var icon = Icon.ExtractAssociatedIcon(exePath);
                if (icon == null) return GetFallbackPath();

                using var bitmap = icon.ToBitmap();
                bitmap.Save(outputPath, ImageFormat.Png);
                return outputPath;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[IconExtractor] Failed for {exePath}: {ex.Message}");
                return GetFallbackPath();
            }
        }

        /// <summary>Safely deletes a cached icon file.</summary>
        public void DeleteIcon(string iconPath)
        {
            try
            {
                if (File.Exists(iconPath))
                    File.Delete(iconPath);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[IconExtractor] Delete failed: {ex.Message}");
            }
        }

        /// <summary>Checks if icon file exists on disk.</summary>
        public bool IconExists(string iconPath) => File.Exists(iconPath);

        private static string GetFallbackPath() =>
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                "Assets", "default-icon.png");
    }
}