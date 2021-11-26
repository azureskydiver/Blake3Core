using System;
using System.Text;

namespace Blake3Core
{
    public class Blake3DerivedKey : Blake3
    {
        public Blake3DerivedKey(byte[] context)
            : base(Flag.DeriveKeyMaterial, DeriveKey(context), HashSizeInBits)
        {
        }

        public Blake3DerivedKey(ReadOnlySpan<byte> context)
            : this(context.ToArray())
        {
        }

        public Blake3DerivedKey(string context, Encoding encoding)
            : this(encoding.GetBytes(context))
        {
        }

        public Blake3DerivedKey(string context)
            : this(context, Encoding.Default)
        {
        }

        static byte [] ComputeHash(Flag flag, ReadOnlySpan<uint> key, int outputSizeInBits, byte[] message)
        {
            using (var hashAlgorithm = Blake3.Create(flag, key, outputSizeInBits))
                return hashAlgorithm.ComputeHash(message);
        }

        static byte[] DeriveKey(byte[] context)
        {
            var keyContext = ComputeHash(Flag.DeriveKeyContext, IV, HashSizeInBits, context);
            return keyContext.AsSpan().Slice(0, 8 * sizeof(uint)).ToArray();
        }
    }
}
