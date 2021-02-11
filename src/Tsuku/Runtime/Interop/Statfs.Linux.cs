using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Tsuku.Runtime.Interop
{
    internal static partial class Statfs
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct LinuxStatFs
        {
            public uint f_type;
            uint f_bsize;
            uint f_blocks;
            uint f_bfree;
            uint f_bavail;
            uint f_files;
            uint f_ffree;
            uint f_fsid;
            uint f_namelen;
            uint f_frsize;
            uint f_flags;
            [MarshalAs(UnmanagedType.ByValArray,
                ArraySubType = UnmanagedType.I4, SizeConst = 4)]
            uint[] f_spare;
        }
        [DllImport("libc", SetLastError = true)]
        public static extern int linux_statfs(string path, out LinuxStatFs buf);
    }
}
