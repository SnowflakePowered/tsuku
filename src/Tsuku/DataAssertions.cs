using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Tsuku
{
    internal static class DataAssertions
    {
        public static bool CheckNameLength(string name)
        {
            return name.Length <= 192;
        }

        public static bool CheckNameValid(string name)
        {
            return !String.IsNullOrWhiteSpace(name) && (name.IndexOfAny(Path.GetInvalidFileNameChars()) == -1);
        }

        public static bool CheckDataLength(ReadOnlySpan<byte> span)
        {
            return span.Length <= 4096;
        }

        public static void CheckValidity(string name, ReadOnlySpan<byte> data)
        {
            if (!DataAssertions.CheckNameLength(name))
                throw new ArgumentException("Attribute name is longer than 192 characters.");
            if (!DataAssertions.CheckNameValid(name))
                throw new ArgumentException("Attribute name contains invalid characters.");
            if (!DataAssertions.CheckDataLength(data))
                throw new ArgumentException("Input data is longer than 4096 bytes.");
        }
    }
}
