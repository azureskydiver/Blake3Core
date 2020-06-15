using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using Xunit;

namespace Blake3Core.Tests
{
    public class Correctness
    {
        static Dictionary<int, IEnumerable<byte>> _inputBytes = new Dictionary<int, IEnumerable<byte>>();

        static IEnumerable<byte> GetInputBytes(int length)
        {
            if (!_inputBytes.ContainsKey(length))
                _inputBytes[length] = Enumerable.Range(0, length).Select(i => (byte)(i % 251)).ToList();
            return _inputBytes[length];
        }

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

        void VerifyExtendedOutput(Func<Blake3> create, int inputLength, string expectedHash)
        {
            var hasher = create();
            var input = GetInputBytes(inputLength).ToArray();
            var hash = hasher.ComputeHash(input);

            Assert.Equal(expectedHash, hasher.GetExtendedOutput()
                                             .Take(expectedHash.Length / 2)
                                             .ToArray()
                                             .ToHex());
        }

        [Theory]
        [ClassData(typeof(TestVectors))]
        void VerifyHashExtendedOutput(TestVector testVector)
        {
            VerifyExtendedOutput(() => new Blake3(),
                                 testVector.InputLength,
                                 testVector.Hash);
        }

        [Theory]
        [ClassData(typeof(TestVectors))]
        void VerifyKeyedHashExtendedOutput(TestVector testVector)
        {
            VerifyExtendedOutput(() => new Blake3Keyed(testVector.Key.FromHex()),
                                 testVector.InputLength,
                                 testVector.KeyedHash);
        }

        [Theory]
        [ClassData(typeof(TestVectors))]
        void VerifyDerivedKeyExtendedOutput(TestVector testVector)
        {
            const string context = "BLAKE3 2019-12-27 16:29:52 test vectors context";
            VerifyExtendedOutput(() => new Blake3DerivedKey(context, Encoding.ASCII),
                                 testVector.InputLength,
                                 testVector.DerivedKeyHash);
        }
    }
}
