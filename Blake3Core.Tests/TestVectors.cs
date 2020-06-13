using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace Blake3Core.Tests
{
    public class TestVector
    {
        public string Key { get; set; }
        public int InputLength { get; set; }
        public string Hash { get; set; }
        public string KeyedHash { get; set; }
        public string DerivedKeyHash { get; set; }
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
            foreach (var testCase in jsonTestVectors.Cases)
            {
                yield return new object[] {
                    new TestVector()
                    {
                        Key = jsonTestVectors.Key,
                        InputLength = testCase.InputLength,
                        Hash = testCase.Hash,
                        KeyedHash = testCase.KeyedHash,
                        DerivedKeyHash = testCase.DerivedKeyHash
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
                AssertIsValidHashString(testVector.Key);
                AssertIsValidHashString(testVector.Hash);
                AssertIsValidHashString(testVector.KeyedHash);
                AssertIsValidHashString(testVector.DerivedKeyHash);
            }

            static void AssertIsValidHashString(string s)
            {
                Assert.NotNull(s);
                Assert.True(s.Length > 0);
                Assert.True(IsEven(s.Length));
                Assert.True(s.All(ch => "0123456789ABCDEFabcedef".Contains(ch)));
            }

            static bool IsEven(int n) => n % 2 == 0;
        }
    }
}
