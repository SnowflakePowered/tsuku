using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Mono.Unix.Native;
namespace Tsuku.Runtime
{
    class PosixUserExtendedAttributes : ITsukuImplementation
    {
        public IEnumerable<TsukuAttributeInfo> ListInfos(FileInfo info, bool followSymlinks)
        {
            throw new NotImplementedException();
        }

        public long Read(FileInfo info, string name, ref Span<byte> data, bool followSymlinks)
        {
            Syscall.getxattr(info.FullName, name, out byte[] buf);
            buf.CopyTo(data);
            return Math.Min(buf.Length, data.Length);
        }

        public void Write(FileInfo info, string name, byte[] data, bool followSymlinks)
        {
            Syscall.setxattr(info.FullName, $"user.tsuku.{name}", data, XattrFlags.XATTR_REPLACE);
            
        }

        public void Write(FileInfo info, string name, ReadOnlySpan<byte> data, bool followSymlinks)
            => this.Write(info, name, data.ToArray(), followSymlinks);
    }
}
