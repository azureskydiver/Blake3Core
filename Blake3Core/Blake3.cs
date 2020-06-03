using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace Blake3Core
{
    public class Blake3 : HashAlgorithm
    {
        const int HashSizeInBits = 8 * sizeof(uint);

        internal const int ChunkLength = 1024;
        internal const int BlockLength = 16 * sizeof(uint);

        protected static readonly uint [] IV =
            { 0x6A09E667, 0xBB67AE85, 0x3C6EF372, 0xA54FF53A, 0x510E527F, 0x9B05688C, 0x1F83D9AB, 0x5BE0CD19 };

        protected private Flag DefaultFlag;
        protected uint[] Key;

        ChunkState _chunkState;
        ChainingValueStack _chainingValueStack;

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
            _chunkState = new ChunkState(DefaultFlag);
            _chainingValueStack = new ChainingValueStack();
        }

        void MoveToNextChunk()
        {
            var cv = _chunkState.ComputeChainingValue();
            _chainingValueStack.Push(ref cv);
            _chunkState.MoveToNextChunk();
        }

        protected override void HashCore(byte[] array, int ibStart, int cbSize)
        {
            var data = new ReadOnlyMemory<byte>(array, ibStart, cbSize);

            if (!_chunkState.IsComplete)
            {
                var needToFill = Math.Min(ChunkLength - _chunkState.Length, data.Length);
                _chunkState.Update(data.Slice(0, needToFill));
                MoveToNextChunk();
                data = data.Slice(needToFill);
            }

            while (data.Length >= ChunkLength)
            {
                _chunkState.Update(data.Slice(0, ChunkLength));
                MoveToNextChunk();
                data = data.Slice(ChunkLength);
            }

            if (data.Length > 0)
                _chunkState.Update(data.Slice(0, data.Length));
        }

        protected override byte[] HashFinal()
        {
            _chunkState.EndChunk();
            var cv = _chunkState.ComputeChainingValue();
            _chainingValueStack.Push(ref cv);

            //$ TODO: Merge chaining value stage and get final hash value
            return null;
        }
    }
}
