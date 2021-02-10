using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Mono.Unix.Native;
namespace Tsuku.Runtime
{
    class PosixUserExtendedAttributes
    {
        internal static void WriteArgs(FileInfo info, string name, ReadOnlySpan<byte> data)
        {
            Syscall.setxattr(info.FullName, $"user.tsuku.{name}", data.ToArray(), XattrFlags.XATTR_REPLACE);
            
        }
    }
}
