﻿using Microsoft.Win32.SafeHandles;
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
        public void Write(FileInfo info, string name, ReadOnlySpan<byte> data, bool followSymlinks)
        {
            using Kernel32.SafeHFILE handle = 
                Kernel32.CreateFile($"{info.FullName}:tsuku.{name}",
                Kernel32.FileAccess.FILE_GENERIC_WRITE,
                FileShare.Write, null, FileMode.OpenOrCreate, 
                followSymlinks // if not follow symlinks, directly open the symlink data.
                    ? FileFlagsAndAttributes.FILE_ATTRIBUTE_NORMAL 
                    : FileFlagsAndAttributes.FILE_FLAG_OPEN_REPARSE_POINT);

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

        public int Read(FileInfo info, string name, ref Span<byte> data, bool followSymlinks)
        {
            using Kernel32.SafeHFILE handle =
                Kernel32.CreateFile($"{info.FullName}:tsuku.{name}",
                Kernel32.FileAccess.FILE_GENERIC_READ,
                FileShare.Read, null, FileMode.Open,
                followSymlinks // if not follow symlinks, directly open the symlink data.
                    ? FileFlagsAndAttributes.FILE_ATTRIBUTE_NORMAL
                    : FileFlagsAndAttributes.FILE_FLAG_OPEN_REPARSE_POINT);

            if (handle == HFILE.INVALID_HANDLE_VALUE)
            {
                Kernel32.GetLastError().ThrowIfFailed();
            }

            // Kernel32.SafeHFILE should close after on dispose.
            using var stream = new FileStream(new SafeFileHandle(handle.DangerousGetHandle(), false), FileAccess.Read);
            return stream.Read(data);
        }

        public IEnumerable<TsukuAttributeInfo> ListInfos(FileInfo info, bool followSymlinks)
        {
            if (followSymlinks && info.IsSymbolicLink())
            {
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

            foreach (var stream in Kernel32.EnumFileStreams(info.FullName))
            {
                string? streamName = stream.GetFieldValue<string>("cStreamName");
                if (streamName?.StartsWith(":tsuku.") == true)
                {
                    yield return new(streamName[":tsuku.".Length..^":$DATA".Length], stream.StreamSize);
                }
            }
        }
    }
}