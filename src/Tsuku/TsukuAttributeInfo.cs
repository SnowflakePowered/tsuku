using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Tsuku
{
    public sealed class TsukuAttributeInfo
    {
        internal TsukuAttributeInfo(string name, long size) => (this.Name, this.Size) = (name, size);
        public string Name { get; }
        public long Size { get; }
    }
}
