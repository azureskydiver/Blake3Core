using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace Blake3Core.Tests
{
    public class TestVectors : IEnumerable<object[]>
    {
        struct TestCase
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

        struct Vectors
        {
            [JsonPropertyName("_comment")]
            public string Comment { get; set; }

            [JsonPropertyName("key")]
            public string Key { get; set; }

            [JsonPropertyName("cases")]
            public List<TestCase> Cases { get; set; }
        }

        static Lazy<IEnumerable<object[]>> _testVectors;

        static TestVectors()
            => _testVectors = new Lazy<IEnumerable<object[]>>(() => LoadTestVectors().ToList());

        static IEnumerable<object[]> LoadTestVectors()
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
                        InputLength = testCase.InputLength,
                        Hash = testCase.Hash,
                        KeyedHash = testCase.KeyedHash,
                        DerivedKeyHash = testCase.DerivedKeyHash,
                        Key = key,
                    }
                };
            }
        }

        public IEnumerator<object[]> GetEnumerator() => _testVectors.Value.GetEnumerator();
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
                AssertIsValidHexString(testVector.Hash);
                AssertIsValidHexString(testVector.KeyedHash);
                AssertIsValidHexString(testVector.DerivedKeyHash);
                AssertIsValidHexString(testVector.Key);
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
