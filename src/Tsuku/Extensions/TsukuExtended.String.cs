using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Tsuku.Extensions
{
    public static partial class TsukuExtended
    {
        /// <summary>
        /// Sets a string attribute for a file. If it already exists, it will be replaced.
        /// The input string must be at most <see cref="Tsuku.MAX_ATTR_SIZE"/> bytes.
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
        public static void SetAttribute(this FileInfo @this, string name, string data)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(data);
            @this.SetAttribute(name, bytes);
        }

        /// <summary>
        /// Gets an attribute from a file as a <see cref="string"/>.
        /// </summary>
        /// <param name="this">The <see cref="FileInfo"/> referring to the file.</param>
        /// <param name="name">The name of the attribute to get.</param>
        /// <param name="followSymbolicLinks">
        /// If <see langword="true"/>, reads the attribute from the resolved target of the symbolic link. 
        /// Otherwise, if <see langword="false"/>, reads the attribute from the link itself.
        /// </param>     
        /// <exception cref="FileNotFoundException">If the file <paramref name="this"/>, or attribute does not exist.</exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="name"/> is longer than <see cref="Tsuku.MAX_NAME_LEN"/>, 
        /// or if the attribute contains invalid Unicode code points.
        /// </exception>
        /// <exception cref="PlatformNotSupportedException">
        /// If the filesystem of the file <paramref name="this"/> does not support extended attributes on the
        /// current operating system.
        /// </exception>
        public static string GetStringAttribute(this FileInfo @this, string name)
        {
            Span<byte> data = stackalloc byte[Tsuku.MAX_ATTR_SIZE];
            data.Clear();
            int read = @this.GetAttribute(name, ref data);
            return Encoding.UTF8.GetString(data[..read]);
        }

        /// <summary>
        /// Tries to gets an attribute from a file as a <see cref="string"/>.
        /// </summary>
        /// <param name="this">The <see cref="FileInfo"/> referring to the file.</param>
        /// <param name="name">The name of the attribute to get.</param>
        /// <param name="result">When this method returns, contains the <see cref="string"/> decoded from the
        /// attribute data if the method succeeded, or <see langword="null"/> if it failed.</param>
        /// <param name="followSymbolicLinks">
        /// If <see langword="true"/>, reads the attribute from the resolved target of the symbolic link. 
        /// Otherwise, if <see langword="false"/>, reads the attribute from the link itself.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the read succeeded, <see langword="false"/> otherwise.
        /// If <see cref="TryGetStringAttribute(FileInfo, string, out string?, bool)"/> returns <see langword="false"/>, 
        /// then <paramref name="result"/> will be <see langword="null"/>.
        /// </returns>
        public static bool TryGetStringAttribute(this FileInfo @this, string name, out string? result)
        {
            try
            {
                result = @this.GetStringAttribute(name);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }
    }
}
