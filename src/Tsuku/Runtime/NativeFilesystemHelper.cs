using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Vanara.PInvoke;
using Mono.Unix.Native;
using Mono.Unix;
using System.Linq;
using Tsuku.Runtime.Interop;

namespace Tsuku.Runtime
{
    internal static class NativeFilesystemHelper
    {
        private static bool IsSymbolicLink(FileInfo @this)
              => @this.Attributes.HasFlag(FileAttributes.ReparsePoint);

        public static void ResolveSymlinkPosix(ref FileInfo info)
        {
            if (!IsSymbolicLink(info))
                return;
            var newPath = new StringBuilder();
            int res = Syscall.readlink(info.FullName, newPath);
            if (res == -1)
            {
                var errno = Syscall.GetLastError();
                throw errno switch
                {
                    Errno.ENOENT => new FileNotFoundException("The specified file was not found"),
                    Errno.EACCES => new UnauthorizedAccessException("The caller does not have the required permission."),
                    Errno.ENOTDIR => new DirectoryNotFoundException("The specified path is invalid."),
                    Errno.ENAMETOOLONG => new PathTooLongException("The specified path, file name, or both exceed the system-defined maximum length."),
                    _ => new Exception($"Unknown exception occured with errno {errno}")
                };
            }
            info = new FileInfo(newPath.ToString());
        }

        public static void ResolveSymlinkWinApi(ref FileInfo info)
        {
            if (!IsSymbolicLink(info))
                return;
            // If we follow the symbolic link, we need to resolve it.
            using Kernel32.SafeHFILE handle = Kernel32
                .CreateFile(info.FullName, 0, 0, null, FileMode.Open, FileFlagsAndAttributes.FILE_ATTRIBUTE_NORMAL);

            if (handle == HFILE.INVALID_HANDLE_VALUE)
            {
                Kernel32.GetLastError().ThrowIfFailed();
            }

            var fullPath = new StringBuilder(Kernel32.MAX_PATH + 1);
            Kernel32.GetFinalPathNameByHandle(handle, fullPath,
                Kernel32.MAX_PATH, Kernel32.FinalPathNameOptions.FILE_NAME_NORMALIZED);
            Kernel32.GetLastError().ThrowIfFailed();

            string filePath = fullPath.ToString();
            info = new FileInfo(filePath.StartsWith(@"\\?\") 
                ? filePath[@"\\?\".Length..]
                : filePath);
        }

        public static string GetDarwinFilesystem(FileInfo fileInfo)
        {
            Statfs.macos_statfs(fileInfo.FullName, out var statfs);
            return statfs.f_fstypename;
        }

        public static string GetLinuxFilesystem(FileInfo fileInfo)
        {
            // https://man7.org/linux/man-pages/man2/statfs.2.html
            // https://github.com/dotnet/runtime/blob/e8339af091988247c90bd7d347753da05f7e74cd/src/libraries/Common/src/Interop/Unix/System.Native/Interop.MountPoints.FormatInfo.cs
            Statfs.linux_statfs(fileInfo.FullName, out var statfs);
            return statfs.f_type switch
            {
                0xef53 => "ext4",
                0x5346544e => "NTFS",
                0x9123683e => "btrfs",
                uint i => $"Unknown ({i})"
            };
        }

        public static string GetWindowsFilesystem(FileInfo fileInfo)
        {
            var rootDir = fileInfo.Directory.Root;
            var drive = DriveInfo.GetDrives()
                .Where(d => d?.RootDirectory?.FullName == rootDir?.FullName).FirstOrDefault();
            return drive?.DriveFormat ?? "Unknown";
        }
    }
}
