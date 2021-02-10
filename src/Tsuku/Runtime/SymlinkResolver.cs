using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Vanara.PInvoke;
using Mono.Unix.Native;
using Mono.Unix;

namespace Tsuku.Runtime
{
    internal static class SymlinkResolver
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

            info = new FileInfo(fullPath.ToString());
        }
    }
}
