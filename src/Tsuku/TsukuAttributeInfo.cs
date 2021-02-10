using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Tsuku
{
    public sealed class TsukuAttributeInfo
    {
        internal TsukuAttributeInfo(FileInfo file, string name, long size) => (this.File, this.Name, this.Size) = (file, name, size);
        public FileInfo File { get; }
        public string Name { get; }
        public long Size { get; }
    }

}
