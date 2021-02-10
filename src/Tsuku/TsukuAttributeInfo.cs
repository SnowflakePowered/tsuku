using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Tsuku
{
    /// <summary>
    /// Represents the attribute information for a file.
    /// </summary>
    public struct TsukuAttributeInfo
    {
        internal TsukuAttributeInfo(string name, long size) => (this.Name, this.Size) = (name, size);
        
        /// <summary>
        /// The name of the attribute.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The size of the attribute.
        /// </summary>
        public long Size { get; }

        public void Deconstruct(out string name, out long size) => (name, size) = (this.Name, this.Size);
    }
}
