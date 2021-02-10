using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Tsuku.Runtime;

namespace Tsuku
{
    public static partial class Tsuku
    {
        /// <summary>
        /// Sets an attribute for a file. If it already exists, it will be replaced.
        /// </summary>
        /// <param name="this">The <see cref="FileInfo"/> referring to the file.</param>
        /// <param name="name">The name of the attribute to set.</param>
        /// <param name="data">The attribute data to write.</param>
        /// <param name="followSymbolicLinks">
        /// If <see langword="true"/>, writes the attribute to the resolved target of the symbolic link. 
        /// Otherwise, if <see langword="false"/>, writes the attribute to the link itself.
        /// Setting this to <see langword="false"/> is unsupported on Linux.
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
        public static void SetAttribute(this FileInfo @this, string name, byte[] data)
        {
            DataAssertions.CheckValidity(name, data);
            if (!@this.Exists)
                throw new FileNotFoundException("The requested file does not exist.");
            Tsuku.GetImplementation(@this).Write(@this, name, data);
        }

        /// <summary>
        /// Sets an attribute for a file. If it already exists, it will be replaced.
        /// </summary>
        /// <param name="this">The <see cref="FileInfo"/> referring to the file.</param>
        /// <param name="name">The name of the attribute to set.</param>
        /// <param name="data">The attribute data to write.</param>
        /// <param name="followSymbolicLinks">
        /// If <see langword="true"/>, writes the attribute to the resolved target of the symbolic link. 
        /// Otherwise, if <see langword="false"/>, writes the attribute from the link itself.
        /// Setting this to <see langword="false"/> is unsupported on Linux.
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
        public static void SetAttribute(this FileInfo @this, string name, ReadOnlySpan<byte> data)
        {
            DataAssertions.CheckValidity(name, data);
            if (!@this.Exists)
                throw new FileNotFoundException("The requested file does not exist.");
            Tsuku.GetImplementation(@this).Write(@this, name, data);
        }

        /// <summary>
        /// Gets an attribute from a file.
        /// </summary>
        /// <param name="this">The <see cref="FileInfo"/> referring to the file.</param>
        /// <param name="name">The name of the attribute to get.</param>
        /// <param name="followSymbolicLinks">
        /// If <see langword="true"/>, reads the attribute from the resolved target of the symbolic link. 
        /// Otherwise, if <see langword="false"/>, reads the attribute from the link itself.
        /// Setting this to <see langword="false"/> is unsupported on Linux.
        /// </param>     
        /// <exception cref="FileNotFoundException">If the file <paramref name="this"/>, or attribute does not exist.</exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="name"/> is longer than <see cref="Tsuku.MAX_NAME_LEN"/>.
        /// </exception>
        /// <exception cref="PlatformNotSupportedException">
        /// If the filesystem of the file <paramref name="this"/> does not support extended attributes on the
        /// current operating system.
        /// </exception>
        public static byte[] GetAttribute(this FileInfo @this, string name)
        {
            Span<byte> data = stackalloc byte[Tsuku.MAX_ATTR_SIZE];
            data.Clear();
            DataAssertions.CheckValidity(name, data);

            int readBytes = Tsuku.GetImplementation(@this)
                .Read(@this, name, ref data);

            byte[] buf = new byte[readBytes];
            
            data[..readBytes].CopyTo(buf);
            return buf;
        }

        /// <summary>
        /// Gets an attribute from a file.
        /// </summary>
        /// <param name="this">The <see cref="FileInfo"/> referring to the file.</param>
        /// <param name="name">The name of the attribute to get.</param>
        /// <param name="data">The buffer to write the attribute data to. Will read at most <see cref="Tsuku.MAX_ATTR_SIZE"/> bytes.</param>
        /// <param name="followSymbolicLinks">
        /// If <see langword="true"/>, reads the attribute from the resolved target of the symbolic link. 
        /// Otherwise, if <see langword="false"/>, reads the attribute from the link itself.
        /// Setting this to <see langword="false"/> is unsupported on Linux.
        /// </param>     
        /// <exception cref="FileNotFoundException">If the file <paramref name="this"/>, or attribute does not exist.</exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="data"/> is larger than <see cref="Tsuku.MAX_ATTR_SIZE"/> bytes, or if
        /// <paramref name="name"/> is longer than <see cref="Tsuku.MAX_NAME_LEN"/>.
        /// </exception>
        /// <exception cref="PlatformNotSupportedException">
        /// If the filesystem of the file <paramref name="this"/> does not support extended attributes on the
        /// current operating system.
        /// </exception>
        /// <returns>The number of bytes read.</returns>
        public static int GetAttribute(this FileInfo @this, string name, ref Span<byte> data)
        {
            DataAssertions.CheckReadValidity(name);
            return Tsuku.GetImplementation(@this)
                      .Read(@this, name, ref data);
        }

        /// <summary>
        /// Tries to gets an attribute from a file.
        /// </summary>
        /// <param name="this">The <see cref="FileInfo"/> referring to the file.</param>
        /// <param name="name">The name of the attribute to get.</param>
        /// <param name="result">The buffer to write the attribute data to. Will read at most <see cref="Tsuku.MAX_ATTR_SIZE"/> bytes.</param>
        /// <param name="followSymbolicLinks">
        /// If <see langword="true"/>, reads the attribute from the resolved target of the symbolic link. 
        /// Otherwise, if <see langword="false"/>, reads the attribute from the link itself.
        /// Setting this to <see langword="false"/> is unsupported on Linux.
        /// </param>     
        /// <returns><see langword="true"/> if the read succeeded, <see langword="false"/> otherwise.</returns>
        public static bool TryGetAttribute(this FileInfo @this, string name, ref Span<byte> result)
        {
            try
            {
                @this.GetAttribute(name, ref result);
                return true;
            }
            catch 
            { 
                return false; 
            }
        }

        /// <summary>
        /// Enumerates the attributes attached to a file.
        /// 
        /// This method will only enumerate attributes created by <see cref="Tsuku"/>, and will not
        /// expose other extended attributes.
        /// </summary>
        /// <param name="this">The <see cref="FileInfo"/> referring to the file.</param>
        /// <param name="followSymbolicLinks">
        /// If <see langword="true"/>, reads the attribute from the resolved target of the symbolic link. 
        /// Otherwise, if <see langword="false"/>, reads the attribute from the link itself.
        /// 
        /// Setting this to <see langword="false"/> is unsupported on Linux.
        /// </param>     
        /// <returns>A list of <see cref="TsukuAttributeInfo"/> objects for the file.</returns>
        public static IEnumerable<TsukuAttributeInfo> EnumerateAttributeInfos(this FileInfo @this)
        {
            if (!@this.Exists)
                throw new FileNotFoundException("The requested file does not exist.");

            return Tsuku.GetImplementation(@this).ListInfos(@this);
        }
    }
}
