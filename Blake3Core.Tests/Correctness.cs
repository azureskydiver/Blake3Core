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
            => Enumerable.Range(0, length).Select(i => (byte)(i % 252));

        [Theory]
        [ClassData(typeof(TestVectors))]
        void VerifyHash(TestVector testVector)
        {
            var hasher = new Blake3();
            var input = GetInputBytes(testVector.InputLength).ToArray();
            var hash = hasher.ComputeHash(input);
            Assert.Equal(64, hash.Length);
            Assert.Equal(testVector.Hash.Take(hash.Length), hash);
        }
    }
}
