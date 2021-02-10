using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Vanara.Extensions.Reflection;
using Vanara.PInvoke;
namespace Tsuku.Runtime
{
    internal class NtfsAlternateDataStreams : ITsukuImplementation
    {
        public void Write(FileInfo info, string name, ReadOnlySpan<byte> data)
        {
            using Kernel32.SafeHFILE handle = 
                Kernel32.CreateFile($"{info.FullName}:tsuku.{name}",
                Kernel32.FileAccess.FILE_GENERIC_WRITE,
                FileShare.Write, null, FileMode.OpenOrCreate,
                FileFlagsAndAttributes.FILE_ATTRIBUTE_NORMAL);

            if (handle == HFILE.INVALID_HANDLE_VALUE)
            {
                Kernel32.GetLastError().ThrowIfFailed();
            }

            // Kernel32.SafeHFILE should close after on dispose.
            using var stream = new FileStream(new SafeFileHandle(handle.DangerousGetHandle(), false),
                FileAccess.ReadWrite);
            stream.Write(data);
            stream.Flush();
        }

        public int Read(FileInfo info, string name, ref Span<byte> data)
        {
            using Kernel32.SafeHFILE handle =
                Kernel32.CreateFile($"{info.FullName}:tsuku.{name}",
                Kernel32.FileAccess.FILE_GENERIC_READ,
                FileShare.Read, null, FileMode.Open,
                FileFlagsAndAttributes.FILE_ATTRIBUTE_NORMAL);

            if (handle == HFILE.INVALID_HANDLE_VALUE)
            {
                Kernel32.GetLastError().ThrowIfFailed();
            }

            int maxRead = Math.Min(data.Length, Tsuku.MAX_ATTR_SIZE);
            // Kernel32.SafeHFILE should close after on dispose.
            using var stream = new FileStream(new SafeFileHandle(handle.DangerousGetHandle(), false), FileAccess.Read);
            return stream.Read(data[..maxRead]);
        }

        public IEnumerable<TsukuAttributeInfo> ListInfos(FileInfo info)
        {
            SymlinkResolver.ResolveSymlinkWinApi(ref info);

            foreach (var stream in Kernel32.EnumFileStreams(info.FullName))
            {
                string? streamName = stream.GetFieldValue<string>("cStreamName");
                if (streamName?.StartsWith(":tsuku.") == true)
                {
                    yield return new(streamName[":tsuku.".Length..^":$DATA".Length], stream.StreamSize);
                }
            }
        }

        public void Delete(FileInfo info, string name)
        {
            using Kernel32.SafeHFILE handle =
                Kernel32.CreateFile($"{info.FullName}:tsuku.{name}",
                0,
                0,
                null, 
                FileMode.Open,
                FileFlagsAndAttributes.FILE_FLAG_DELETE_ON_CLOSE);

            if (handle == HFILE.INVALID_HANDLE_VALUE)
            {
                Kernel32.GetLastError().ThrowIfFailed();
            }
        }
    }
}
