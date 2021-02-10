using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Mono.Unix.Native;
namespace Tsuku.Runtime
{
    class PosixUserExtendedAttributes : ITsukuImplementation
    {
        private void ThrowIfFailed(int result)
        {
            if (result == -1)
            {
                var errno = Syscall.GetLastError();
                throw errno switch
                {
                    Errno.EPERM => new PlatformNotSupportedException("Unable to set attribute on a read-only file or symbolic link."),
                    Errno.E2BIG => new PlatformNotSupportedException("The target file system does not support the size of the attribute value."),
                    Errno.ENODATA => new FileNotFoundException("The requested attribute was not found."),
                    Errno.EOPNOTSUPP => new PlatformNotSupportedException("This filesystem is not supported."),
                    Errno.ERANGE => new ArgumentException("Buffer was too small."),
                    Errno.EACCES => new UnauthorizedAccessException("The caller does not have the required permission."),
                    Errno.ENOENT => new FileNotFoundException($"The specified file was not found."),
                    Errno.ENOTDIR => new DirectoryNotFoundException("The specified path is invalid."),
                    Errno.ENAMETOOLONG => new PathTooLongException("The specified path, file name, or both exceed the system-defined maximum length."),
                    _ => new Exception($"Unknown exception occured with errno {errno}")
                };
            }
        }

        public IEnumerable<TsukuAttributeInfo> ListInfos(FileInfo info)
        {
            string[] names;
            int res = (int)Syscall.listxattr(info.FullName, out names);

            ThrowIfFailed(res);
            foreach (string name in names)
            {
                if (!name.StartsWith("user.tsuku."))
                    continue;
                res = (int)Syscall.getxattr(info.FullName, name, null, 0);
                if (res == -1) 
                    continue;
                yield return new TsukuAttributeInfo(name["user.tsuku.".Length..], res);
            }
        }

        public int Read(FileInfo info, string name, byte[] data)
        {
            int maxRead = Math.Min(data.Length, Tsuku.MAX_ATTR_SIZE);
            int read = (int)Syscall.getxattr(info.FullName, $"user.tsuku.{name}", data, (ulong)maxRead);
            ThrowIfFailed(read);
            return read;
        }

        public void Write(FileInfo info, string name, byte[] data)
        {
            int res = Syscall.setxattr(info.FullName, $"user.tsuku.{name}", data, XattrFlags.XATTR_AUTO);
            ThrowIfFailed(res);
        }

        public void Delete(FileInfo info, string name)
        {
            int res = Syscall.removexattr(info.FullName, $"user.tsuku.{name}");
            ThrowIfFailed(res);
        }
    }
}
