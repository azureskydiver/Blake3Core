using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
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
                bytes.Add((byte)((GetNibble(span[0]) << 4) + GetNibble(span[1])));
                span = span.Slice(2);
            }
            return bytes.ToArray();

            int GetNibble(char ch)
            {
                int index = "0123456789abcdef".IndexOf(ch);
                if (index < 0)
                    index = "0123456789ABCDEF".IndexOf(ch);
                if (index < 0)
                    throw new InvalidDataException($"Unexpected data string with byte data.");
                return index;
            }
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
