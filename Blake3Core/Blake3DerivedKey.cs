using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Blake3Core
{
    public class Blake3DerivedKey : Blake3
    {
        public Blake3DerivedKey(byte[] context)
            : base(Flag.DeriveKeyMaterial, DeriveKey(context))
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

        static byte [] ComputeHash(Flag flag, ReadOnlySpan<uint> key, byte[] message)
        {
            using (var hashAlgorithm = Blake3.Create(flag, key))
                return hashAlgorithm.ComputeHash(message);
        }

        static byte[] DeriveKey(byte[] context)
        {
            var keyContext = ComputeHash(Flag.DeriveKeyContext, IV, context);
            return keyContext.AsSpan().Slice(0, 8 * sizeof(uint)).ToArray();
        }
    }
}
