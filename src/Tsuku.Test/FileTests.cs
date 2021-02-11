using System;
using System.IO;
using Xunit;
using Tsuku;
using Tsuku.Extensions;
using System.Linq;

namespace Tsuku.Test
{
    public class FileTests
    {
        [Fact]
        public void SuccessStringAttr_Test()
        {
            var file = new FileInfo(Path.GetTempFileName());

            file.SetAttribute("TestAttribute", "Hello World");
            Assert.Equal("Hello World", file.GetStringAttribute("TestAttribute"));
            Assert.True(file.TryGetStringAttribute("TestAttribute", out var attribute));
            Assert.Equal("Hello World", attribute);
        }

        [Fact]
        public void SuccessDeleteAttr_Test()
        {
            var file = new FileInfo(Path.GetTempFileName());

            file.SetAttribute("TestAttribute", "Hello World");
            Assert.Equal("Hello World", file.GetStringAttribute("TestAttribute"));
            Assert.True(file.TryGetStringAttribute("TestAttribute", out var attribute));
            Assert.Equal("Hello World", attribute);

            file.DeleteAttribute("TestAttribute");
            Assert.False(file.TryGetStringAttribute("TestAttribute", out var _));
            Assert.Empty(file.EnumerateAttributeInfos());
        }

        [Fact]
        public void SuccessBoolAttr_Test()
        {
            var file = new FileInfo(Path.GetTempFileName());

            file.SetAttribute("TestAttribute", true);
            Assert.True(file.GetBoolAttribute("TestAttribute"));
            Assert.True(file.TryGetBoolAttribute("TestAttribute", out var attribute));
            Assert.True(attribute);
        }

        [Fact]
        public void SuccessGuidAttr_Test()
        {
            var file = new FileInfo(Path.GetTempFileName());
            var guid = Guid.NewGuid();

            file.SetAttribute("TestAttribute", guid);
            Assert.Equal(guid, file.GetGuidAttribute("TestAttribute"));
            Assert.True(file.TryGetGuidAttribute("TestAttribute", out var attribute));
            Assert.Equal(guid, attribute);
        }

        [Fact]
        public void SuccessZeroByteAttr_Test()
        {
            var file = new FileInfo(Path.GetTempFileName());
            Span<byte> span = stackalloc byte[0];
            Span<byte> res = stackalloc byte[0];

            file.SetAttribute("TestAttribute", span);
            Assert.Empty(file.GetAttribute("TestAttribute"));
            Assert.True(file.TryGetAttribute("TestAttribute", ref res));
        }

        [Fact]
        public void SuccessMaxAttrBuf_Test()
        {
            var file = new FileInfo(Path.GetTempFileName());
            Span<byte> span = stackalloc byte[Tsuku.MAX_ATTR_SIZE];
            span.Fill(0x20);

            Span<byte> res = stackalloc byte[Tsuku.MAX_ATTR_SIZE];

            file.SetAttribute("TestAttribute", span);
            Assert.True(span.SequenceEqual(file.GetAttribute("TestAttribute")));
            Assert.True(file.TryGetAttribute("TestAttribute", ref res));
            Assert.True(span.SequenceEqual(res));
        }

        [Fact]
        public void SuccessMaxName_Test()
        {
            var file = new FileInfo(Path.GetTempFileName());
            file.SetAttribute(String.Concat(Enumerable.Repeat("A", Tsuku.MAX_NAME_LEN)), "Hello World");
            Assert.Equal("Hello World", file.GetStringAttribute(String.Concat(Enumerable.Repeat("A", Tsuku.MAX_NAME_LEN))));
        }

        [Fact]
        public void SuccessPartialRead_Test()
        {
            var file = new FileInfo(Path.GetTempFileName());
            Span<byte> span = stackalloc byte[Tsuku.MAX_ATTR_SIZE];
            span.Fill(0x20);
            Span<byte> res = stackalloc byte[Tsuku.MAX_ATTR_SIZE / 2];

            file.SetAttribute("TestAttribute", span);

            Assert.True(file.TryGetAttribute("TestAttribute", ref res));
            Assert.True(span[..(Tsuku.MAX_ATTR_SIZE / 2)].SequenceEqual(res));
        }

        [Fact]
        public void SuccessEnumerate_Test()
        {
            var file = new FileInfo(Path.GetTempFileName());
            Span<byte> span = stackalloc byte[0];

            file.SetAttribute("TestAttribute", span);
            Assert.NotEmpty(file.EnumerateAttributeInfos());
        }

        [Fact]
        public void FailureNameTooLong_Test()
        {
            var file = new FileInfo(Path.GetTempFileName());

            file.SetAttribute(String.Concat(Enumerable.Repeat("A", Tsuku.MAX_NAME_LEN)), "Hello World");
            Assert.Throws<ArgumentException>(
                () => file.SetAttribute(String.Concat(Enumerable.Repeat("B", Tsuku.MAX_NAME_LEN + 1)), "Hello World"));
            Assert.Throws<ArgumentException>(
                () => file.GetAttribute(String.Concat(Enumerable.Repeat("B", Tsuku.MAX_NAME_LEN + 1))));
            Assert.Throws<ArgumentException>(
               () => file.DeleteAttribute(String.Concat(Enumerable.Repeat("B", Tsuku.MAX_NAME_LEN + 1))));
        }

        [Fact]
        public void FailureBufTooLong_Test()
        {
            var file = new FileInfo(Path.GetTempFileName());
            var span = new byte[Tsuku.MAX_ATTR_SIZE + 1];
            
            Assert.Throws<ArgumentException>(
                () => file.SetAttribute("TestAttribute", span));
            Assert.Throws<ArgumentException>(
                () => file.SetAttribute("TestAttribute", span.AsSpan()));
        }

        [Fact]
        public void FailureAttrNotFoundTest()
        {
            var file = new FileInfo(Path.GetTempFileName());

            Assert.Throws<FileNotFoundException>(
                () => file.GetAttribute("TestAttribute"));
            Assert.Throws<FileNotFoundException>(
               () => file.DeleteAttribute("TestAttribute"));
        }

        [Fact]
        public void FileNotFoundTest()
        {
            var file = new FileInfo(Path.GetTempFileName());
            file.Delete();

            Assert.Throws<FileNotFoundException>(
                () => file.GetAttribute("TestAttribute"));
            Assert.Throws<FileNotFoundException>(
               () => file.DeleteAttribute("TestAttribute"));
        }
    }
}
