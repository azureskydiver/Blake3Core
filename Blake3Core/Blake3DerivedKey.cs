using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Blake3Core
{
    public class Blake3DerivedKey : Blake3
    {
        public Blake3DerivedKey(byte[] context, byte[] key)
            : base(Flag.DeriveKeyMaterial, DeriveKey(context, key))
        {
        }

        public Blake3DerivedKey(ReadOnlySpan<byte> context, ReadOnlySpan<byte> key)
            : this(context.ToArray(), key.ToArray())
        {
        }

        static byte [] ComputeHash(Flag flag, ReadOnlySpan<uint> key, byte[] message)
        {
            using (var hashAlgorithm = Blake3.Create(flag, key))
                return hashAlgorithm.ComputeHash(message);
        }

        static byte[] DeriveKey(byte[] context, byte[] key)
        {
            var keyContext = ComputeHash(Flag.DeriveKeyContext, IV, context);
            return ComputeHash(Flag.DeriveKeyMaterial, keyContext.AsUints(), key);
        }
    }
}
