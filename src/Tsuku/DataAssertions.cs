using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Tsuku
{
    internal static class DataAssertions
    {
        private static bool CheckNameLength(string name)
        {
            return name.Length <= Tsuku.MAX_NAME_LEN;
        }

        private static bool CheckNameValid(string name)
        {
            return !String.IsNullOrWhiteSpace(name) && (name.IndexOfAny(Path.GetInvalidFileNameChars()) == -1);
        }

        private static bool CheckDataLength(ReadOnlySpan<byte> span)
        {
            return span.Length <= Tsuku.MAX_ATTR_SIZE;
        }

        /// <summary>
        /// Check that the outputs are valid for reading and writing.
        /// 
        /// Ensures that <paramref name="name"/> is less than or equal to <see cref="Tsuku.MAX_NAME_LEN"/> characters, and the size of the
        /// <paramref name="data"/> buffer is less than <see cref="Tsuku.MAX_ATTR_SIZE"/>.
        /// </summary>
        /// <param name="name">The name of the attribute.</param>
        public static void CheckReadValidity(string name)
        {
            if (!DataAssertions.CheckNameLength(name))
                throw new ArgumentException($"Attribute name is longer than {Tsuku.MAX_NAME_LEN} characters.");
            if (!DataAssertions.CheckNameValid(name))
                throw new ArgumentException("Attribute name contains invalid characters.");
        }

        /// <summary>
        /// Check that the inputs are valid for reading and writing.
        /// 
        /// Ensures that <paramref name="name"/> is less than or equal to <see cref="Tsuku.MAX_NAME_LEN"/> characters, and the size of the
        /// <paramref name="data"/> buffer is less than <see cref="Tsuku.MAX_ATTR_SIZE"/>.
        /// </summary>
        /// <param name="name">The name of the attribute.</param>
        /// <param name="data">The buffer to read or write to.</param>
        public static void CheckValidity(string name, ReadOnlySpan<byte> data)
        {
            if (!DataAssertions.CheckNameLength(name))
                throw new ArgumentException($"Attribute name is longer than {Tsuku.MAX_NAME_LEN} characters.");
            if (!DataAssertions.CheckNameValid(name))
                throw new ArgumentException("Attribute name contains invalid characters.");
            if (!DataAssertions.CheckDataLength(data))
                throw new ArgumentException($"Buffer is longer than {Tsuku.MAX_ATTR_SIZE} bytes.");
        }
    }
}
