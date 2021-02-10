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
                    Errno.E2BIG => new PlatformNotSupportedException("The target file system does not support the size of the attribute value."),
                    Errno.ENODATA => new FileNotFoundException("The requested attribute was not found."),
                    Errno.EOPNOTSUPP => new PlatformNotSupportedException("This filesystem is not supported."),
                    Errno.ERANGE => new ArgumentException("Buffer was too small."),
                    Errno.EACCES => new UnauthorizedAccessException("The caller does not have the required permission"),
                    Errno.ENOENT => new FileNotFoundException($"The specified file was not found."),
                    Errno.ENOTDIR => new DirectoryNotFoundException("The specified path is invalid."),
                    Errno.ENAMETOOLONG => new PathTooLongException("The specified path, file name, or both exceed the system-defined maximum length."),
                    _ => new Exception($"Unknown exception occured with errno {errno}")
                };
            }
        }

        public IEnumerable<TsukuAttributeInfo> ListInfos(FileInfo info, bool followSymlinks)
        {
            string[] names;
            int res = followSymlinks switch 
            {
                true => (int)Syscall.listxattr(info.FullName, out names),
                false => (int)Syscall.llistxattr(info.FullName, out names)
            };
            
            ThrowIfFailed(res);
            foreach (string name in names)
            {
                if (!name.StartsWith("user.tsuku."))
                    continue;
                res = followSymlinks switch
                {
                    true => (int)Syscall.getxattr(info.FullName, name, null, 0),
                    false => (int)Syscall.lgetxattr(info.FullName, name, null, 0)
                };
                if (res == -1) 
                    continue;
                yield return new TsukuAttributeInfo(name["user.tsuku.".Length..], res);
            }
        }

        public int Read(FileInfo info, string name, byte[] data, bool followSymlinks)
        {
            int maxRead = Math.Min(data.Length, Tsuku.MAX_ATTR_SIZE);
            int read = followSymlinks switch 
            {
                true => (int)Syscall.getxattr(info.FullName, $"user.tsuku.{name}", data, (ulong)maxRead),
                false => (int)Syscall.lgetxattr(info.FullName, $"user.tsuku.{name}", data, (ulong)maxRead)
            };
            ThrowIfFailed(read);
            return read;
        }

        public void Write(FileInfo info, string name, byte[] data, bool followSymlinks)
        {
            int res = followSymlinks switch
            {
                true => Syscall.setxattr(info.FullName, $"user.tsuku.{name}", data, XattrFlags.XATTR_AUTO),
                false => Syscall.lsetxattr(info.FullName, $"user.tsuku.{name}", data, XattrFlags.XATTR_AUTO)
            };
            ThrowIfFailed(res);
        }
    }
}
