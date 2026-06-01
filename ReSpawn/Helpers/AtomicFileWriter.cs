using System.IO;

namespace ReSpawn.Helpers
{
    public static class AtomicFileWriter
    {
        public static void WriteAllText(string path, string content)
        {
            string tempPath = path + ".tmp";
            string backupPath = path + ".bak";

            File.WriteAllText(tempPath, content, System.Text.Encoding.UTF8);

            if (File.Exists(path))
                File.Replace(tempPath, path, backupPath);
            else
                File.Move(tempPath, path);
        }
    }
}