using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Xunit;

namespace Blake3Core.Tests
{
    public static class StringExtensions
    {
        public static byte[] FromHex(this string s)
        {
            Assert.True(s.Length % 2 == 0);
            var bytes = new List<byte>(s.Length / 2);
            var span = s.AsSpan();
            while (!span.IsEmpty)
            {
                if (!byte.TryParse(span.Slice(0, 2), NumberStyles.HexNumber, null, out byte value))
                    throw new InvalidDataException($"Unexpected data string with byte data.");
                bytes.Add(value);
                span = span.Slice(2);
            }
            return bytes.ToArray();
        }
        
        public static string ToHex(this byte[] bytes)
        {
            var sb = new StringBuilder(bytes.Length * 2);
            foreach (var b in bytes)
                sb.Append(b.ToString("x2"));
            return sb.ToString();
        }
    }
}
