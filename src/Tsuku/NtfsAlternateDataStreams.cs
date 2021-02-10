using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.IO;
using Vanara.Extensions.Reflection;
using Vanara.PInvoke;
namespace Tsuku
{
    public class NtfsAlternateDataStreams
    {
        internal static void WriteStream(FileInfo info, string name, ReadOnlySpan<byte> data)
        {
            using Kernel32.SafeHFILE handle = Kernel32.CreateFile($"{info.FullName}:tsuku.{name}",
                Kernel32.FileAccess.FILE_GENERIC_WRITE, 
                FileShare.Write, null, FileMode.Create, FileFlagsAndAttributes.FILE_ATTRIBUTE_NORMAL);

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

        public static IEnumerable<TsukuAttributeInfo> GetStreamInfos(FileInfo info)
        {
            foreach (var stream in Kernel32.EnumFileStreams(info.FullName))
            {
                string? streamName = stream.GetFieldValue<string>("cStreamName");
                if (streamName?.StartsWith(":tsuku.") == true)
                {
                    yield return new(info, streamName[":tsuku.".Length..^":$DATA".Length], stream.StreamSize);
                }
            }
        }


    }
}
