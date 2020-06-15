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

        void VerifyAllAtOnce(Func<Blake3> create, int inputLength, string expectedHash)
        {
            var hasher = create();
            var input = GetInputBytes(inputLength).ToArray();
            var hash = hasher.ComputeHash(input);

            Assert.Equal(64, hash.Length);
            Assert.Equal(expectedHash, hasher.GetExtendedOutput()
                                             .Take(expectedHash.Length / 2)
                                             .ToArray()
                                             .ToHex());
        }

        void VerifyOneAtATime(Func<Blake3> create, int inputLength, string expectedHash)
        {
            var hasher = create();
            var input = GetInputBytes(inputLength).ToArray();
            for (int i = 0; i < input.Length; i++)
                hasher.TransformBlock(input, i, 1, null, 0);
            hasher.TransformFinalBlock(input, 0, 0);

            Assert.Equal(64, hasher.Hash.Length);
            Assert.Equal(expectedHash, hasher.GetExtendedOutput()
                                             .Take(expectedHash.Length / 2)
                                             .ToArray()
                                             .ToHex());
        }

        [Theory]
        [ClassData(typeof(TestVectors))]
        void VerifyHash(TestVector testVector)
        {
            VerifyAllAtOnce(() => new Blake3(),
                            testVector.InputLength,
                            testVector.Hash);
        }

        [Theory]
        [ClassData(typeof(TestVectors))]
        void VerifyHashOneAtATime(TestVector testVector)
        {
            VerifyOneAtATime(() => new Blake3(),
                             testVector.InputLength,
                             testVector.Hash);
        }

        [Theory]
        [ClassData(typeof(TestVectors))]
        void VerifyKeyedHash(TestVector testVector)
        {
            VerifyAllAtOnce(() => new Blake3Keyed(testVector.Key.FromHex()),
                            testVector.InputLength,
                            testVector.KeyedHash);
        }

        [Theory]
        [ClassData(typeof(TestVectors))]
        void VerifyKeyedHashOneAtATime(TestVector testVector)
        {
            VerifyOneAtATime(() => new Blake3Keyed(testVector.Key.FromHex()),
                             testVector.InputLength,
                             testVector.KeyedHash);
        }

        const string DerivedKeyContext = "BLAKE3 2019-12-27 16:29:52 test vectors context";
        readonly Encoding DerivedKeyContextEncoding = Encoding.ASCII;

        [Theory]
        [ClassData(typeof(TestVectors))]
        void VerifyDerivedKey(TestVector testVector)
        {
            VerifyAllAtOnce(() => new Blake3DerivedKey(DerivedKeyContext, DerivedKeyContextEncoding),
                            testVector.InputLength,
                            testVector.DerivedKeyHash);
        }

        [Theory]
        [ClassData(typeof(TestVectors))]
        void VerifyDerivedKeyOneAtATime(TestVector testVector)
        {
            VerifyOneAtATime(() => new Blake3DerivedKey(DerivedKeyContext, DerivedKeyContextEncoding),
                             testVector.InputLength,
                             testVector.DerivedKeyHash);
        }
    }
}
