using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Tsuku.Runtime;

namespace Tsuku
{
    public static partial class Tsuku
    {
        public const int MAX_ATTR_SIZE = 4096;
        public const int MAX_NAME_LEN = 192;
        
        private static ITsukuImplementation WINDOWS_NTFS = new NtfsAlternateDataStreams();
        private static ITsukuImplementation UNIX_XATTR = new PosixUserExtendedAttributes();

        private static Dictionary<(OSPlatform, string), ITsukuImplementation> TsukuImpls = new()
        {
            { (OSPlatform.Windows, "NTFS"), WINDOWS_NTFS },
            { (OSPlatform.Linux, "ext4"), UNIX_XATTR },
            { (OSPlatform.Linux, "ext3"), UNIX_XATTR },
            { (OSPlatform.Linux, "ext2"), UNIX_XATTR },
            { (OSPlatform.Linux, "btrfs"), UNIX_XATTR },
            { (OSPlatform.OSX, "apfs"), UNIX_XATTR },
            { (OSPlatform.OSX, "hfsplus"), UNIX_XATTR },
        };

        private static string GetFileSystem(this FileInfo @this)
        {
            // todo: resolve symlink
            var rootDir = @this.Directory.Root;
            var drive = DriveInfo.GetDrives().Where(d => d.RootDirectory.FullName == rootDir.FullName).FirstOrDefault();
            return drive?.DriveFormat ?? "Unknown";
        }

        private static ITsukuImplementation GetImplementation(FileInfo fileInfo)
        {
            string fsType = fileInfo.GetFileSystem();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (!Tsuku.TsukuImpls.TryGetValue((OSPlatform.Windows, fsType), out var impl))
                {
                    throw new PlatformNotSupportedException($"{fsType} is not supported on Windows");
                }
                return impl;
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                if (!Tsuku.TsukuImpls.TryGetValue((OSPlatform.Linux, fsType), out var impl))
                {
                    throw new PlatformNotSupportedException($"{fsType} is not supported on Linux");
                }
                return impl;
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                if (!Tsuku.TsukuImpls.TryGetValue((OSPlatform.OSX, fsType), out var impl))
                {
                    throw new PlatformNotSupportedException($"{fsType} is not supported on macOS");
                }
                return impl;
            }
            throw new PlatformNotSupportedException("Unable to determine operating system.");
        }
    }
}
