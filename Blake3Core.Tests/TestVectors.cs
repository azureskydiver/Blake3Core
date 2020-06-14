using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;
using Xunit.Abstractions;

namespace Blake3Core.Tests
{
    public class TestVector
    {
        public string Key { get; set; }
        public int InputLength { get; set; }
        public string Hash { get; set; }
        public string KeyedHash { get; set; }
        public string DerivedKeyHash { get; set; }

        public override string ToString()
        {
            return $"{{ InputLength={InputLength}, Hash={Hash}, KeyedHash={KeyedHash}, DerivedKeyHash={DerivedKeyHash}, Key={Key} }}";
        }
    }

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
                sb.Append(b.ToString("x"));
            return sb.ToString();
        }
    }

    public class TestVectors : IEnumerable<object[]>
    {
        public struct TestCase
        {
            [JsonPropertyName("input_len")]
            public int InputLength { get; set; }

            [JsonPropertyName("hash")]
            public string Hash { get; set; }

            [JsonPropertyName("keyed_hash")]
            public string KeyedHash { get; set; }

            [JsonPropertyName("derive_key")]
            public string DerivedKeyHash { get; set; }
        }

        public struct Vectors
        {
            [JsonPropertyName("_comment")]
            public string Comment { get; set; }

            [JsonPropertyName("key")]
            public string Key { get; set; }

            [JsonPropertyName("cases")]
            public List<TestCase> Cases { get; set; }
        }

        public IEnumerator<object[]> GetEnumerator()
        {
            var path = Path.GetRelativePath(Directory.GetCurrentDirectory(), "test_vectors.json");
            if (!File.Exists(path))
                throw new ArgumentException($"Could not find test vector file at path: {path}");

            var jsonTestVectors = JsonSerializer.Deserialize<Vectors>(File.ReadAllText(path));
            var keyBytes = Encoding.ASCII.GetBytes(jsonTestVectors.Key);
            var key = keyBytes.ToHex();

            foreach (var testCase in jsonTestVectors.Cases)
            {
                yield return new object[] {
                    new TestVector()
                    {
                        Key = key,
                        InputLength = testCase.InputLength,
                        Hash = testCase.Hash,
                        KeyedHash = testCase.KeyedHash,
                        DerivedKeyHash = testCase.DerivedKeyHash,
                    }
                };
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        [Fact]
        void CanLoadTestVectors()
        {
            var testVectors = new TestVectors();
            Assert.NotEmpty(testVectors);
            foreach (var objArray in testVectors)
            {
                Assert.IsType<object[]>(objArray);
                Assert.Single(objArray);
                Assert.IsType<TestVector>(objArray[0]);

                var testVector = objArray[0] as TestVector;
                Assert.True(testVector.InputLength >= 0);
                AssertIsValidHexString(testVector.Key);
                AssertIsValidHexString(testVector.Hash);
                AssertIsValidHexString(testVector.KeyedHash);
                AssertIsValidHexString(testVector.DerivedKeyHash);
            }

            static void AssertIsValidHexString(string s)
            {
                Assert.NotNull(s);
                Assert.True(s.Length > 0);
                var bytes = s.FromHex();
                Assert.Equal(s.Length / 2, bytes.Length);
            }
        }
    }
}
