using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Tsuku.Runtime
{
    /// <summary>
    /// Tsuku OS implementation.
    /// 
    /// One of <see cref="Write(FileInfo, string, byte[], bool)"/> or <see cref="Write(FileInfo, string, ReadOnlySpan{byte}, bool)"/>
    /// must be implemented.
    /// </summary>
    internal interface ITsukuImplementation
    {
        /// <summary>
        /// Write an attribute to a file.
        /// 
        /// All parameters must be valid before calling this function.
        /// If this is unimplemented, <see cref="Write(FileInfo, string, ReadOnlySpan{byte}, bool)"/> must be implemented.
        /// </summary>
        /// <param name="info">The <see cref="FileInfo"/> to write.</param>
        /// <param name="name">The name of the attribute to write.</param>
        /// <param name="data">The buffer to write. 
        /// This must be less than <see cref="Tsuku.MAX_ATTR_SIZE"/> (usually 4096).
        /// <strong>This should be verified before calling this function.</strong>
        /// </param>
        /// <param name="followSymlinks">
        /// If <see langword="true"/>, writes the attribute to the resolved target of the symbolic link. 
        /// Otherwise, if <see langword="false"/>, writes the attribute to the link itself.</param>
        void Write(FileInfo info, string name, byte[] data, bool followSymlinks)
            => Write(info, name, (ReadOnlySpan<byte>)data, followSymlinks);

        /// <summary>
        /// Write an attribute to a file.
        /// 
        /// All parameters must be valid before calling this function.
        /// <param name="info">The <see cref="FileInfo"/> to write.</param>
        /// <param name="name">The name of the attribute to write.</param>
        /// <param name="data">The buffer to write. 
        /// This must be less than <see cref="Tsuku.MAX_ATTR_SIZE"/> (usually 4096).
        /// <strong>This should be verified before calling this function.</strong>
        /// </param>
        /// <param name="followSymlinks">
        /// If <see langword="true"/>, writes the attribute to the resolved target of the symbolic link. 
        /// Otherwise, if <see langword="false"/>, writes the attribute to the link itself.</param>
        void Write(FileInfo info, string name, ReadOnlySpan<byte> data, bool followSymlinks)
            => Write(info, name, data.ToArray(), followSymlinks);

        /// <summary>
        /// Read attribute data from a file.
        /// </summary>
        /// <param name="info">The <see cref="FileInfo"/> to read.</param>
        /// <param name="name">The name of the attribute to read.</param>
        /// <param name="data">The buffer to read into. 
        /// This must be less than or equal to <see cref="Tsuku.MAX_ATTR_SIZE"/> (usually 4096).
        /// <strong>This should be verified before calling this function.</strong>
        /// </param>
        /// <param name="followSymlinks">
        /// If <see langword="true"/>, reads the attribute from the resolved target of the symbolic link. 
        /// Otherwise, if <see langword="false"/>, reads the attribute from the link itself.</param>
        /// <returns>The number of bytes read. At most <see cref="Tsuku.MAX_ATTR_SIZE"/>.</returns>
        int Read(FileInfo info, string name, ref Span<byte> data, bool followSymlinks)
        {
            byte[] buf = new byte[Tsuku.MAX_ATTR_SIZE];
            int read = this.Read(info, name, buf, followSymlinks);
            int maxRead = Math.Min(data.Length, read);
            buf.AsSpan()[..maxRead].CopyTo(data);
            return maxRead;
        }

        /// <summary>
        /// Read attribute data from a file.
        /// </summary>
        /// <param name="info">The <see cref="FileInfo"/> to read.</param>
        /// <param name="name">The name of the attribute to read.</param>
        /// <param name="data">The buffer to read into. 
        /// This must be less than or equal to <see cref="Tsuku.MAX_ATTR_SIZE"/> (usually 4096).
        /// <strong>This should be verified before calling this function.</strong>
        /// </param>
        /// <param name="followSymlinks">
        /// If <see langword="true"/>, reads the attribute from the resolved target of the symbolic link. 
        /// Otherwise, if <see langword="false"/>, reads the attribute from the link itself.
        /// </param>
        /// <returns>The number of bytes read. At most <see cref="Tsuku.MAX_ATTR_SIZE"/>.</returns>
        int Read(FileInfo info, string name, byte[] data, bool followSymlinks)
        {
            var span = data.AsSpan();
            return this.Read(info, name, ref span, followSymlinks);
        }


        /// <summary>
        /// Enumerates attribute information.
        /// </summary>
        /// <param name="info">The <see cref="FileInfo"/> to read.</param>
        /// <param name="followSymlinks">
        /// If <see langword="true"/>, reads the attribute from the resolved target of the symbolic link. 
        /// Otherwise, if <see langword="false"/>, reads the attribute from the link itself.</param>
        /// <returns></returns>
        IEnumerable<TsukuAttributeInfo> ListInfos(FileInfo info, bool followSymlinks);
    }
}
