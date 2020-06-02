using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace Blake3Core
{
    public class Blake3 : HashAlgorithm
    {
        const int HashSizeInBits = 256;

        [Flags]
        protected enum Flag
        {
            ChunkStart          = 1 << 0,
            ChunkEnd            = 1 << 1,
            Parent              = 1 << 2,
            Root                = 1 << 3,
            KeyedHash           = 1 << 4,
            DeriveKeyContext    = 1 << 5,
            DeriveKeyMaterial   = 1 << 6,
            None
        }

        protected static readonly uint [] IV =
            { 0x6A09E667, 0xBB67AE85, 0x3C6EF372, 0xA54FF53A, 0x510E527F, 0x9B05688C, 0x1F83D9AB, 0x5BE0CD19 };

        protected Flag DefaultFlag;
        protected uint[] Key;

        protected Blake3(Flag defaultFlag)
        {
            HashSizeValue = HashSizeInBits;
            DefaultFlag = defaultFlag;
        }

        public Blake3()
            : this(Flag.None)
        {
            Key = IV;
        }

        public override void Initialize()
        {
            throw new NotImplementedException();
        }

        protected override void HashCore(byte[] array, int ibStart, int cbSize)
        {
            throw new NotImplementedException();
        }

        protected override byte[] HashFinal()
        {
            throw new NotImplementedException();
        }
    }
}
