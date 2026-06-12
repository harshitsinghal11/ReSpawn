using Microsoft.Win32;

namespace ReSpawn.Helpers
{
    public static class StartupManager
    {
        private const string AppName = "ReSpawn";
        private const string RegistryKey =
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";

        /// <summary>Check if ReSpawn is set to run at startup.</summary>
        public static bool IsStartupEnabled()
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryKey, false);
            return key?.GetValue(AppName) != null;
        }

        /// <summary>Enable or disable run at startup.</summary>
        public static void SetStartup(bool enable)
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryKey, true);
            if (key == null) return;

            if (enable)
            {
                string exePath = System.Diagnostics.Process
                    .GetCurrentProcess().MainModule!.FileName;
                key.SetValue(AppName, $"\"{exePath}\"");
            }
            else
            {
                key.DeleteValue(AppName, throwOnMissingValue: false);
            }
        }
    }
}