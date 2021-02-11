using System;
using System.IO;
using Xunit;
using Tsuku;
using Tsuku.Extensions;
using System.Linq;
using Emet.FileSystems;

namespace Tsuku.Test
{
    public class SymlinkTests
    {
        [Fact]
        public void SuccessStringAttr_Test()
        {
            var file = new FileInfo(Path.GetTempFileName());

            var link = new FileInfo(Path.GetTempFileName());
            link.Delete();
            FileSystem.CreateSymbolicLink(file.FullName, link.FullName, FileType.File);

            link.SetAttribute("TestAttribute", "Hello World");
            Assert.Equal("Hello World", file.GetStringAttribute("TestAttribute"));
            Assert.True(file.TryGetStringAttribute("TestAttribute", out var attribute));
            Assert.Equal("Hello World", attribute);
        }

        [Fact]
        public void SuccessDeleteAttr_Test()
        {
            var file = new FileInfo(Path.GetTempFileName());
            var link = new FileInfo(Path.GetTempFileName());
            link.Delete();
            FileSystem.CreateSymbolicLink(file.FullName, link.FullName, FileType.File);

            link.SetAttribute("TestAttribute", "Hello World");
            Assert.Equal("Hello World", file.GetStringAttribute("TestAttribute"));
            Assert.True(link.TryGetStringAttribute("TestAttribute", out var attribute));
            Assert.Equal("Hello World", attribute);

            link.DeleteAttribute("TestAttribute");
            Assert.False(file.TryGetStringAttribute("TestAttribute", out var _));
            Assert.Empty(file.EnumerateAttributeInfos());
            Assert.Empty(link.EnumerateAttributeInfos());
        }

        [Fact]
        public void SuccessBoolAttr_Test()
        {
            var file = new FileInfo(Path.GetTempFileName());
            var link = new FileInfo(Path.GetTempFileName());
            link.Delete();
            FileSystem.CreateSymbolicLink(file.FullName, link.FullName, FileType.File);

            link.SetAttribute("TestAttribute", true);
            Assert.True(file.GetBoolAttribute("TestAttribute"));
            Assert.True(link.TryGetBoolAttribute("TestAttribute", out var attribute));
            Assert.True(attribute);
        }

        [Fact]
        public void SuccessGuidAttr_Test()
        {
            var file = new FileInfo(Path.GetTempFileName());
            var link = new FileInfo(Path.GetTempFileName());
            link.Delete();
            FileSystem.CreateSymbolicLink(file.FullName, link.FullName, FileType.File);
            var guid = Guid.NewGuid();

            link.SetAttribute("TestAttribute", guid);
            Assert.Equal(guid, file.GetGuidAttribute("TestAttribute"));
            Assert.True(link.TryGetGuidAttribute("TestAttribute", out var attribute));
            Assert.Equal(guid, attribute);
        }

        [Fact]
        public void SuccessZeroByteAttr_Test()
        {
            var file = new FileInfo(Path.GetTempFileName());
            var link = new FileInfo(Path.GetTempFileName());
            link.Delete();
            FileSystem.CreateSymbolicLink(file.FullName, link.FullName, FileType.File);

            Span<byte> span = stackalloc byte[0];
            Span<byte> res = stackalloc byte[0];

            file.SetAttribute("TestAttribute", span);
            Assert.Empty(link.GetAttribute("TestAttribute"));
            Assert.True(file.TryGetAttribute("TestAttribute", ref res));
        }

        [Fact]
        public void SuccessMaxAttrBuf_Test()
        {
            var file = new FileInfo(Path.GetTempFileName());
            var link = new FileInfo(Path.GetTempFileName());
            link.Delete();
            FileSystem.CreateSymbolicLink(file.FullName, link.FullName, FileType.File);

            Span<byte> span = stackalloc byte[Tsuku.MAX_ATTR_SIZE];
            span.Fill(0x20);

            Span<byte> res = stackalloc byte[Tsuku.MAX_ATTR_SIZE];

            file.SetAttribute("TestAttribute", span);
            Assert.True(span.SequenceEqual(file.GetAttribute("TestAttribute")));
            Assert.True(link.TryGetAttribute("TestAttribute", ref res));
            Assert.True(span.SequenceEqual(res));
        }

        [Fact]
        public void SuccessPartialRead_Test()
        {
            var file = new FileInfo(Path.GetTempFileName());
            var link = new FileInfo(Path.GetTempFileName());
            link.Delete();
            FileSystem.CreateSymbolicLink(file.FullName, link.FullName, FileType.File);

            Span<byte> span = stackalloc byte[Tsuku.MAX_ATTR_SIZE];
            span.Fill(0x20);
            Span<byte> res = stackalloc byte[Tsuku.MAX_ATTR_SIZE / 2];

            file.SetAttribute("TestAttribute", span);

            Assert.True(link.TryGetAttribute("TestAttribute", ref res));
            Assert.True(span[..(Tsuku.MAX_ATTR_SIZE / 2)].SequenceEqual(res));
        }

        [Fact]
        public void SuccessEnumerate_Test()
        {
            var file = new FileInfo(Path.GetTempFileName());
            var link = new FileInfo(Path.GetTempFileName());
            link.Delete();
            FileSystem.CreateSymbolicLink(file.FullName, link.FullName, FileType.File);
            Span<byte> span = stackalloc byte[0];

            file.SetAttribute("TestAttribute", span);
            Assert.NotEmpty(link.EnumerateAttributeInfos());
        }


        [Fact]
        public void FailureAttrNotFoundTest()
        {
            var file = new FileInfo(Path.GetTempFileName());
            var link = new FileInfo(Path.GetTempFileName());
            link.Delete();
            FileSystem.CreateSymbolicLink(file.FullName, link.FullName, FileType.File);

            Assert.Throws<FileNotFoundException>(
                () => link.GetAttribute("TestAttribute"));
            Assert.Throws<FileNotFoundException>(
               () => link.DeleteAttribute("TestAttribute"));
        }

        [Fact]
        public void FileNotFoundTest()
        {
            var file = new FileInfo(Path.GetTempFileName());
            var link = new FileInfo(Path.GetTempFileName());
            link.Delete();
            FileSystem.CreateSymbolicLink(file.FullName, link.FullName, FileType.File);
            file.Delete();

            Assert.Throws<FileNotFoundException>(
                () => link.GetAttribute("TestAttribute"));
            Assert.Throws<FileNotFoundException>(
               () => link.DeleteAttribute("TestAttribute"));
        }
    }
}
