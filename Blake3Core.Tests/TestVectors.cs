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
        public byte[] Key { get; set; }
        public int InputLength { get; set; }
        public byte[] Hash { get; set; }
        public byte[] KeyedHash { get; set; }
        public byte[] DerivedKeyHash { get; set; }

        public override string ToString()
        {
            return $"{{ InputLength={InputLength}, Hash={Hash.ToHex()}, KeyedHash={KeyedHash.ToHex()}, DerivedKeyHash={DerivedKeyHash.ToHex()}, Key={Key.ToHex()} }}";
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
            => BitConverter.ToString(bytes).Replace("-", "");
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

            foreach (var testCase in jsonTestVectors.Cases)
            {
                yield return new object[] {
                    new TestVector()
                    {
                        Key = (byte[]) keyBytes.Clone(),
                        InputLength = testCase.InputLength,
                        Hash = testCase.Hash.FromHex(),
                        KeyedHash = testCase.KeyedHash.FromHex(),
                        DerivedKeyHash = testCase.DerivedKeyHash.FromHex(),
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
                AssertIsValidHashBytes(testVector.Key);
                AssertIsValidHashBytes(testVector.Hash);
                AssertIsValidHashBytes(testVector.KeyedHash);
                AssertIsValidHashBytes(testVector.DerivedKeyHash);
            }

            static void AssertIsValidHashBytes(byte[] s)
            {
                Assert.NotNull(s);
                Assert.True(s.Length > 0);
            }
        }
    }
}
