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
        /// <summary>
        /// The maximum size of an attribute data in bytes.
        /// </summary>
        public const int MAX_ATTR_SIZE = 512;

        /// <summary>
        /// The maximum length of an attribute name, in characters.
        /// </summary>
        public const int MAX_NAME_LEN = 112;
        
        private static ITsukuImplementation WINDOWS_NTFS = new NtfsAlternateDataStreams();
        private static ITsukuImplementation UNIX_XATTR = new PosixUserExtendedAttributes();

        private static Dictionary<(OSPlatform, string), ITsukuImplementation> TsukuImpls = new()
        {
            { (OSPlatform.Windows, "NTFS"), WINDOWS_NTFS },
            { (OSPlatform.Linux, "ext4"), UNIX_XATTR },
            { (OSPlatform.Linux, "ext2"), UNIX_XATTR },
            { (OSPlatform.Linux, "btrfs"), UNIX_XATTR },
            { (OSPlatform.OSX, "apfs"), UNIX_XATTR },
            { (OSPlatform.OSX, "hfsplus"), UNIX_XATTR },
        };

        private static string GetFileSystem(this FileInfo @this)
        {
            FileInfo fileInfo = @this;
            if (@this.Attributes.HasFlag(FileAttributes.ReparsePoint))
            {
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    NativeFilesystemHelper.ResolveSymlinkWinApi(ref fileInfo);
                }
                else if (Environment.OSVersion.Platform == PlatformID.Unix)
                {
                    NativeFilesystemHelper.ResolveSymlinkPosix(ref fileInfo);
                }
                else 
                {
                    throw new PlatformNotSupportedException("Unable to resolve symbolic link.");
                }
            }

            return Environment.OSVersion.Platform switch
            {
                PlatformID.Win32NT => NativeFilesystemHelper.GetWindowsFilesystem(fileInfo),
                PlatformID.Unix => NativeFilesystemHelper.GetUnixFilesystem(fileInfo),
                _ => throw new PlatformNotSupportedException(),
            };
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
