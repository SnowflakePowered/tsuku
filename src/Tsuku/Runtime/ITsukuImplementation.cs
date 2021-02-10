using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Tsuku.Runtime
{
    internal interface ITsukuImplementation
    {
        void Write(FileInfo info, string name, byte[] data, bool followSymlinks)
            => Write(info, name, (ReadOnlySpan<byte>)data, followSymlinks);
        void Write(FileInfo info, string name, ReadOnlySpan<byte> data, bool followSymlinks);
        long Read(FileInfo info, string name, ref Span<byte> data, bool followSymlinks);
        IEnumerable<TsukuAttributeInfo> ListInfos(FileInfo info, bool followSymlinks);
    }
}
