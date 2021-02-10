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
        public const int MAX_ATTR_SIZE = 4096;
        private static ITsukuImplementation WINDOWS_NTFS = new NtfsAlternateDataStreams();
        private static ITsukuImplementation UNIX_XATTR = new PosixUserExtendedAttributes();

        private static Dictionary<(OSPlatform, string), ITsukuImplementation> TsukuImpls = new()
        {
            { (OSPlatform.Windows, "NTFS"), WINDOWS_NTFS },
            { (OSPlatform.Linux, "ext4"), UNIX_XATTR },
            { (OSPlatform.Linux, "ext3"), UNIX_XATTR },
            { (OSPlatform.Linux, "btrfs"), UNIX_XATTR },
            { (OSPlatform.OSX, "apfs"), UNIX_XATTR },
            { (OSPlatform.OSX, "hfsplus"), UNIX_XATTR },

        };
        internal static string GetFileSystem(this FileInfo @this)
        {
            var rootDir = @this.Directory.Root;
            var drive = DriveInfo.GetDrives().Where(d => d.RootDirectory.FullName == rootDir.FullName).FirstOrDefault();
            return drive?.DriveFormat ?? "Unknown";
        }

        internal static bool IsSymbolicLink(this FileInfo @this)
            => @this.Attributes.HasFlag(FileAttributes.ReparsePoint);

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

        public static void SetTsukuAttribute(this FileInfo @this, string name, byte[] data, bool followSymbolicLink = true)
        {
            DataAssertions.CheckValidity(name, data);
            if (!@this.Exists)
                throw new FileNotFoundException("The requested file does not exist.");
            Tsuku.GetImplementation(@this).Write(@this, name, data, followSymbolicLink);
        }

        public static void SetTsukuAttribute(this FileInfo @this, string name, ReadOnlySpan<byte> data, bool followSymbolicLink = true)
        {
            DataAssertions.CheckValidity(name, data);
            if (!@this.Exists)
                throw new FileNotFoundException("The requested file does not exist.");
            Tsuku.GetImplementation(@this).Write(@this, name, data, followSymbolicLink);
        }

        public static byte[] GetTsukuAttribute(this FileInfo @this, string name, bool followSymbolicLink = true)
        {
            Span<byte> data = stackalloc byte[Tsuku.MAX_ATTR_SIZE];
            data.Clear();
            DataAssertions.CheckValidity(name, data);

            int readBytes = Tsuku.GetImplementation(@this)
                .Read(@this, name, ref data, followSymbolicLink);

            byte[] buf = new byte[readBytes];
            
            data[..readBytes].CopyTo(buf);
            return buf;
        }

        public static bool TryGetTsukuAttribute(this FileInfo @this, string name, ref Span<byte> data, bool followSymbolicLink = true)
        {
            DataAssertions.CheckValidity(name, data);

            try
            { Tsuku.GetImplementation(@this)
                    .Read(@this, name, ref data, followSymbolicLink); } catch { return false; }
            return true;
        }

        public static IEnumerable<TsukuAttributeInfo> GetTsukuAttributeInfos(this FileInfo @this, bool followSymbolicLink = true)
        {
            if (!@this.Exists)
                throw new FileNotFoundException("The requested file does not exist.");

            return Tsuku.GetImplementation(@this).ListInfos(@this, followSymbolicLink);
        }
    }
}
