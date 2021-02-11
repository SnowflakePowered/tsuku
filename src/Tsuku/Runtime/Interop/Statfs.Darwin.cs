using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Tsuku.Runtime.Interop
{
    internal static partial class Statfs
    {
        [StructLayout(LayoutKind.Sequential)]
        internal struct DarwinStatfs
        {
            [MarshalAs(UnmanagedType.I2)]
            short f_otype;
            [MarshalAs(UnmanagedType.I2)]
            short f_oflags;
            [MarshalAs(UnmanagedType.I8)]
            long f_bsize;
            [MarshalAs(UnmanagedType.I8)]
            long f_iosize;
            [MarshalAs(UnmanagedType.I8)]
            long f_blocks;
            [MarshalAs(UnmanagedType.I8)]
            long f_bfree;
            [MarshalAs(UnmanagedType.I8)]
            long f_bavail;
            [MarshalAs(UnmanagedType.I8)]
            long f_files;
            [MarshalAs(UnmanagedType.I8)]
            long f_ffree;
            long f_fsid;
            [MarshalAs(UnmanagedType.U4)]
            uint f_owner;
            [MarshalAs(UnmanagedType.I2)]
            short f_reserved1;
            [MarshalAs(UnmanagedType.I2)]
            short f_type;
            [MarshalAs(UnmanagedType.I8)]
            long f_flags;
            [MarshalAs(UnmanagedType.ByValArray,
                ArraySubType = UnmanagedType.I8, SizeConst = 2)]
            long[] f_reserved2;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)]
            public string f_fstypename;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 90)]
            string f_mntonname;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 90)]
            string f_mntfromname;
            [Obsolete("RESERVED: DO NOT USE")]
            sbyte f_reserved3;
            [MarshalAs(UnmanagedType.ByValArray,
                ArraySubType = UnmanagedType.I8, SizeConst = 4)]
            long[] f_reserved4;
        }
        [DllImport("libc", SetLastError = true)]
        public static extern int macos_statfs(string path, out DarwinStatfs buf);
    }
}
