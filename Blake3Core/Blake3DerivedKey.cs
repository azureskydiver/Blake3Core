using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Blake3Core
{
    public class Blake3DerivedKey : Blake3
    {
        byte[] _context;
        byte[] _key;

        class Blake3DeriveKeyFirstStage : Blake3
        {
            public Blake3DeriveKeyFirstStage()
                : base(Flag.DeriveKeyContext)
            {
                Key = IV;
            }
        }

        class Blake3DeriveKeySecondStage : Blake3
        {
            public Blake3DeriveKeySecondStage(byte [] firstStageKey)
                : base(Flag.DeriveKeyMaterial)
            {
                Key = new uint[8];
                MemoryMarshal.Cast<byte, uint>(firstStageKey).Slice(0, 8).CopyTo(Key);
            }
        }

        public Blake3DerivedKey(byte[] context, byte[] key)
            : base(Flag.DeriveKeyMaterial)
        {
            _context = (byte[])context.Clone();
            _key = (byte[])key.Clone();
        }

        byte [] ComputeHashOfContext()
        {
            var hasher = new Blake3DeriveKeyFirstStage();
            hasher.TransformFinalBlock(_context, _context.Length, 0);
            return hasher.Hash;
        }

        byte[] ComputeHashOfKey(byte[] firstStageKey)
        {
            var hasher = new Blake3DeriveKeySecondStage(firstStageKey);
            hasher.TransformFinalBlock(_key, _context.Length, 0);
            return hasher.Hash;
        }

        public override void Initialize()
        {
            var keyContext = ComputeHashOfContext();
            var keyBytes = ComputeHashOfKey(keyContext);

            Key = new uint[8];
            MemoryMarshal.Cast<byte, uint>(keyBytes).Slice(0, 8).CopyTo(Key);

            base.Initialize();
        }
    }
}
