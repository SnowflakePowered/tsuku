using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Tsuku.Runtime;

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

        internal static bool IsSymbolicLink(this FileInfo @this)
            => @this.Attributes.HasFlag(FileAttributes.ReparsePoint);

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

        public static void SetTsukuAttribute(this FileInfo @this, string name, byte[] data, bool followSymbolicLink = false)
        {
            DataAssertions.CheckValidity(name, data);
            if (!@this.Exists)
                throw new FileNotFoundException("The requested file does not exist.");
            Tsuku.SetTsukuAttributeInternal(@this, name, data, followSymbolicLink);
        }

        public static void SetTsukuAttribute(this FileInfo @this, string name, ReadOnlySpan<byte> data, bool followSymbolicLink = false)
        {
            DataAssertions.CheckValidity(name, data);
            if (!@this.Exists)
                throw new FileNotFoundException("The requested file does not exist.");
            Tsuku.SetTsukuAttributeInternal(@this, name, data, followSymbolicLink);
        }

        // Tiny optimization to avoid a copy on non-byte array ReadOnlySpan.
        private static void SetTsukuAttributeInternal(FileInfo @this, string name, ReadOnlySpan<byte> data, bool followSymbolicLink)
        {
            switch (Tsuku.CheckOSSupported(@this))
            {
                case ("NTFS", nameof(OSPlatform.Windows)):
                    NtfsAlternateDataStreams.WriteStream(@this, name, data, followSymbolicLink);
                    break;
                case (_, nameof(OSPlatform.Linux)):
                case (_, nameof(OSPlatform.OSX)):
                    PosixUserExtendedAttributes.WriteArgs(@this, name, data.ToArray(), followSymbolicLink);
                    break;
                default:
                    throw new PlatformNotSupportedException("Unable to determine compatible operating system and filesystem type.");
            }
        }

        private static void SetTsukuAttributeInternal(FileInfo @this, string name, byte[] data, bool followSymbolicLink)
        {
            switch (Tsuku.CheckOSSupported(@this))
            {
                case ("NTFS", nameof(OSPlatform.Windows)):
                    NtfsAlternateDataStreams.WriteStream(@this, name, data, followSymbolicLink);
                    break;
                case (_, nameof(OSPlatform.Linux)):
                case (_, nameof(OSPlatform.OSX)):
                    PosixUserExtendedAttributes.WriteArgs(@this, name, data, followSymbolicLink);
                    break;
                default:
                    throw new PlatformNotSupportedException("Unable to determine compatible operating system and filesystem type.");
            }
        }

        public static byte[] GetTsukuAttribute(this FileInfo @this, string name, bool followSymbolicLink=false)
        {
            return new byte[] { };
        }

        public static bool TryGetTsukuAttribute(this FileInfo @this, string name, ref Span<byte> data, bool followSymbolicLink=false)
        {
            return false;
        }

        public static IEnumerable<TsukuAttributeInfo> GetTsukuAttributeInfos(this FileInfo @this, bool followSymbolicLink = false)
        {
            if (!@this.Exists)
                throw new FileNotFoundException("The requested file does not exist.");

            switch (Tsuku.CheckOSSupported(@this))
            {
                case ("NTFS", nameof(OSPlatform.Windows)):
                    return NtfsAlternateDataStreams.GetStreamInfos(@this, followSymbolicLink);
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
