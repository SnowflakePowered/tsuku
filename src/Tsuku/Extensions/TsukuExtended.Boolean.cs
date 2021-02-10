using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Tsuku.Extensions
{
    public static partial class TsukuExtended
    {
        /// <summary>
        /// Sets a boolean attribute for a file. If it already exists, it will be replaced.
        /// </summary>
        /// <param name="this">The <see cref="FileInfo"/> referring to the file.</param>
        /// <param name="name">The name of the attribute to set.</param>
        /// <param name="data">The attribute data to write.</param>
        /// <param name="followSymbolicLinks">
        /// If <see langword="true"/>, writes the attribute to the resolved target of the symbolic link. 
        /// Otherwise, if <see langword="false"/>, writes the attribute to the link itself.
        /// </param>     
        /// <exception cref="FileNotFoundException">If the file <paramref name="this"/> does not exist.</exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="data"/> is larger than <see cref="Tsuku.MAX_ATTR_SIZE"/> bytes, or if
        /// <paramref name="name"/> is longer than <see cref="Tsuku.MAX_NAME_LEN"/>.
        /// </exception>
        /// <exception cref="PlatformNotSupportedException">
        /// If the filesystem of the file <paramref name="this"/> does not support extended attributes on the
        /// current operating system.
        /// </exception>
        public static void SetAttribute(this FileInfo @this, string name, bool data, bool followSymbolicLinks = true)
        {
            Span<byte> bytes = stackalloc byte[1];
            bytes[0] = data ? 1 : 0;
            @this.SetAttribute(name, bytes, followSymbolicLinks);
        }

        /// <summary>
        /// Gets an attribute from a file as a <see cref="bool"/>.
        /// </summary>
        /// <param name="this">The <see cref="FileInfo"/> referring to the file.</param>
        /// <param name="name">The name of the attribute to get.</param>
        /// <param name="followSymbolicLinks">
        /// If <see langword="true"/>, reads the attribute from the resolved target of the symbolic link. 
        /// Otherwise, if <see langword="false"/>, reads the attribute from the link itself.
        /// </param>     
        /// <exception cref="FileNotFoundException">If the file <paramref name="this"/>, or attribute does not exist.</exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="name"/> is longer than <see cref="Tsuku.MAX_NAME_LEN"/>.
        /// </exception>
        /// <exception cref="PlatformNotSupportedException">
        /// If the filesystem of the file <paramref name="this"/> does not support extended attributes on the
        /// current operating system.
        /// </exception>
        public static bool GetBoolAttribute(this FileInfo @this, string name, bool followSymbolicLinks = true)
        {
            Span<byte> data = stackalloc byte[1];
            data.Clear();
            @this.GetAttribute(name, ref data, followSymbolicLinks);
            return data[0] == 1;
        }

        /// <summary>
        /// Tries to gets an attribute from a file as a <see cref="bool"/>.
        /// </summary>
        /// <param name="this">The <see cref="FileInfo"/> referring to the file.</param>
        /// <param name="name">The name of the attribute to get.</param>
        /// <param name="result">When this method returns, contains the <see cref="bool"/> decoded from the
        /// attribute data if the method succeeded, or <see langword="false"/> if it failed.</param>
        /// <param name="followSymbolicLinks">
        /// If <see langword="true"/>, reads the attribute from the resolved target of the symbolic link. 
        /// Otherwise, if <see langword="false"/>, reads the attribute from the link itself.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the read succeeded, <see langword="false"/> otherwise.
        /// If <see cref="TryGetBoolAttribute(FileInfo, string, out bool, bool)"/> returns <see langword="false"/>, 
        /// then <paramref name="result"/> will be <see langword="false"/>.
        /// </returns>
        public static bool TryGetBoolAttribute(this FileInfo @this, string name, out bool data, bool followSymbolicLinks = true)
        {
            try
            {
                data = @this.GetBoolAttribute(name, followSymbolicLinks);
                return true;
            }
            catch
            {
                data = false;
                return false;
            }
        }
    }
}
