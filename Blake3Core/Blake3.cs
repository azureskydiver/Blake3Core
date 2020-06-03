using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace Blake3Core
{
    [Flags]
    enum Flag
    {
        ChunkStart = 1 << 0,
        ChunkEnd = 1 << 1,
        Parent = 1 << 2,
        Root = 1 << 3,
        KeyedHash = 1 << 4,
        DeriveKeyContext = 1 << 5,
        DeriveKeyMaterial = 1 << 6,
        None
    }

    public partial class Blake3 : HashAlgorithm
    {
        const int HashSizeInBits = 8 * sizeof(uint);

        internal const int BlockLength = 16 * sizeof(uint);

        protected static readonly uint [] IV =
            { 0x6A09E667, 0xBB67AE85, 0x3C6EF372, 0xA54FF53A, 0x510E527F, 0x9B05688C, 0x1F83D9AB, 0x5BE0CD19 };

        protected private Flag DefaultFlag;
        protected uint[] Key;

        ChunkState _chunkState = new ChunkState();
        ChainingValueStack _chainingValueStack = new ChainingValueStack();

        protected private Blake3(Flag defaultFlag, ReadOnlySpan<uint> key)
        {
            HashSizeValue = HashSizeInBits;
            DefaultFlag = defaultFlag;
            Key = key.Slice(0, 8).ToArray();
        }

        protected private Blake3(Flag defaultFlag, ReadOnlySpan<byte> key)
            : this(defaultFlag, key.AsUints())
        {
        }

        protected private static Blake3 Create(Flag defaultFlag, ReadOnlySpan<uint> key)
            => new Blake3(defaultFlag, key);

        public Blake3()
            : this(Flag.None, IV)
        {
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
