using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace Blake3Core.Tests
{
    public class Correctness
    {
        static IEnumerable<byte> GetInputBytes(int length)
            => Enumerable.Range(0, length).Select(i => (byte)(i % 251));

        [Theory]
        [ClassData(typeof(TestVectors))]
        void VerifyHash(TestVector testVector)
        {
            var hasher = new Blake3();
            var input = GetInputBytes(testVector.InputLength).ToArray();
            var hash = hasher.ComputeHash(input);
            var expectedHash = testVector.Hash.FromHex();

            Assert.Equal(64, hash.Length);
            Assert.Equal(expectedHash.Take(hash.Length).ToArray().ToHex(), hash.ToHex());
        }

        [Theory]
        [ClassData(typeof(TestVectors))]
        void VerifyHashExtendedOutput(TestVector testVector)
        {
            var hasher = new Blake3();
            var input = GetInputBytes(testVector.InputLength).ToArray();
            var hash = hasher.ComputeHash(input);
            var expectedHash = testVector.Hash.FromHex();

            Assert.Equal(testVector.Hash, hasher.GetExtendedOutput().Take(expectedHash.Length).ToArray().ToHex());
        }

        [Theory]
        [ClassData(typeof(TestVectors))]
        void VerifyKeyedHashExtendedOutput(TestVector testVector)
        {
            var hasher = new Blake3Keyed(testVector.Key.FromHex());
            var input = GetInputBytes(testVector.InputLength).ToArray();
            var hash = hasher.ComputeHash(input);
            var expectedHash = testVector.KeyedHash.FromHex();

            Assert.Equal(testVector.KeyedHash, hasher.GetExtendedOutput().Take(expectedHash.Length).ToArray().ToHex());
        }
    }
}
