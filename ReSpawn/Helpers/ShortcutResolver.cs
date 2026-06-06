using System.IO;
using System.Runtime.InteropServices;

namespace ReSpawn.Helpers
{
    public static class ShortcutResolver
    {
        /// <summary>Resolves .lnk shortcut to its target path using Windows API.</summary>
        public static string? ResolveLnk(string lnkPath)
        {
            try
            {
                // Read .lnk file manually to extract target path
                using var stream = File.OpenRead(lnkPath);
                using var reader = new BinaryReader(stream);

                // Skip header (76 bytes) + LinkFlags + FileAttributes
                stream.Seek(76, SeekOrigin.Begin);

                // Read LinkFlags
                uint linkFlags = reader.ReadUInt32();
                bool hasLinkTargetIDList = (linkFlags & 0x1) != 0;
                bool hasLinkInfo = (linkFlags & 0x2) != 0;

                // Skip FileAttributes (4) + times (24) + size (4) + icon index (4) 
                // + show command (4) + hotkey (2) + reserved (10)
                stream.Seek(76 + 4 + 4 + 24 + 4 + 4 + 4 + 2 + 10, SeekOrigin.Begin);

                // Skip IDList if present
                if (hasLinkTargetIDList)
                {
                    ushort idListSize = reader.ReadUInt16();
                    stream.Seek(idListSize, SeekOrigin.Current);
                }

                if (!hasLinkInfo) return null;

                // Read LinkInfo
                long linkInfoStart = stream.Position;
                uint linkInfoSize = reader.ReadUInt32();
                reader.ReadUInt32(); // LinkInfoHeaderSize
                uint linkInfoFlags = reader.ReadUInt32();
                reader.ReadUInt32(); // VolumeIDOffset
                uint localBasePathOffset = reader.ReadUInt32();

                bool hasLocalBasePath = (linkInfoFlags & 0x1) != 0;
                if (!hasLocalBasePath) return null;

                stream.Seek(linkInfoStart + localBasePathOffset, SeekOrigin.Begin);

                // Read null-terminated string
                var pathBytes = new System.Collections.Generic.List<byte>();
                byte b;
                while ((b = reader.ReadByte()) != 0)
                    pathBytes.Add(b);

                return System.Text.Encoding.Default.GetString(pathBytes.ToArray());
            }
            catch
            {
                return null;
            }
        }

        /// <summary>Checks if path is a Steam URL.</summary>
        public static bool IsSteamUrl(string path) =>
            path.StartsWith("steam://", StringComparison.OrdinalIgnoreCase);

        /// <summary>Gets process name from exe path.</summary>
        public static string GetProcessName(string exePath) =>
            string.IsNullOrEmpty(exePath)
                ? string.Empty
                : Path.GetFileNameWithoutExtension(exePath);
    }
}