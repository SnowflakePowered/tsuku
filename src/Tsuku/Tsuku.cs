using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
namespace Tsuku
{
    public static class Tsuku
    {
        private static Dictionary<OSPlatform, string[]> SafeFileSystems = new()
        {
            { OSPlatform.Windows, new[] { "NTFS" } },
            { OSPlatform.Linux, new[] { "ext4", "btrfs" } },
            { OSPlatform.OSX, new[] { "hfsplus", "apfs" } },
        };

        internal static string GetFileSystem(this FileInfo @this)
        {
            var rootDir = @this.Directory.Root;
            var drive = DriveInfo.GetDrives().Where(d => d.RootDirectory.FullName == rootDir.FullName).FirstOrDefault();
            return drive?.DriveFormat ?? "Unknown";
        }

        private static (string, string) CheckOSSupported(FileInfo fileInfo)
        {
            string fsType = fileInfo.GetFileSystem();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (!Tsuku.SafeFileSystems[OSPlatform.Windows]
                        .Contains(fsType, StringComparer.InvariantCultureIgnoreCase))
                    throw new PlatformNotSupportedException($"{fsType} is not supported on Windows");
                return (fsType, nameof(OSPlatform.Windows));
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                if (!Tsuku.SafeFileSystems[OSPlatform.Linux]
                        .Contains(fsType, StringComparer.InvariantCultureIgnoreCase))
                    throw new PlatformNotSupportedException($"{fsType} is not supported on Linux");
                return (fsType, nameof(OSPlatform.Linux));
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                if (!Tsuku.SafeFileSystems[OSPlatform.OSX]
                        .Contains(fsType, StringComparer.InvariantCultureIgnoreCase))
                    throw new PlatformNotSupportedException($"{fsType} is not supported on macOS");
                return (fsType, nameof(OSPlatform.OSX));
            }
            throw new PlatformNotSupportedException("Unable to determine operating system.");
        }

        public static void SetTsukuAttribute(this FileInfo @this, string name, ReadOnlySpan<byte> data)
        {
            DataAssertions.CheckValidity(@this, name, data);

            switch (Tsuku.CheckOSSupported(@this)) 
            {
                case ("NTFS", nameof(OSPlatform.Windows)):
                    NtfsAlternateDataStreams.WriteStream(@this, name, data);
                    break;
                case (_, nameof(OSPlatform.Linux)):
                case (_, nameof(OSPlatform.OSX)):
                    break;
                default:
                    throw new PlatformNotSupportedException("Unable to determine compatible operating system and filesystem type.");
            }
        }

        public static byte[] GetTsukuAttribute(this FileInfo @this, string name)
        {
            return new byte[] { };
        }

        public static bool TryGetTsukuAttribute(this FileInfo @this, string name, ref Span<byte> data)
        {
            return false;
        }

        public static IEnumerable<TsukuAttributeInfo> GetTsukuAttributeInfos(this FileInfo @this)
        {
            if (!@this.Exists)
                throw new FileNotFoundException("The requested file does not exist.");

            switch (Tsuku.CheckOSSupported(@this))
            {
                case ("NTFS", nameof(OSPlatform.Windows)):
                    return NtfsAlternateDataStreams.GetStreamInfos(@this);
                case (_, nameof(OSPlatform.Linux)):
                case (_, nameof(OSPlatform.OSX)):
                    break;
                default:
                    throw new PlatformNotSupportedException("Unable to determine compatible operating system and filesystem type.");
            }
            return new TsukuAttributeInfo[] { };
        }
    }
}
