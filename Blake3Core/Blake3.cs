using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace Blake3Core
{
    public class Blake3 : HashAlgorithm
    {
        const int HashSizeInBits = 16 * sizeof(uint);

        internal const int ChunkLength = 1024;
        internal const int BlockLength = 16 * sizeof(uint);

        protected static readonly uint [] IV =
            { 0x6A09E667, 0xBB67AE85, 0x3C6EF372, 0xA54FF53A, 0x510E527F, 0x9B05688C, 0x1F83D9AB, 0x5BE0CD19 };

        protected private Flag DefaultFlag;
        protected uint[] Key;

        ulong _blockCount;
        ChunkState _chunkState;
        Stack<ChainingValue> _chainingValueStack;

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
            _blockCount = 0;
            _chunkState = new ChunkState(Key, 0, DefaultFlag);
            _chainingValueStack = new Stack<ChainingValue>();
        }

        Output GetParentOutput(ref ChainingValue l, ref ChainingValue r)
        {
            var block = new uint[16]
            {
                l.h0, l.h1, l.h2, l.h3, l.h4, l.h5, l.h6, l.h7,
                r.h0, r.h1, r.h2, r.h3, r.h4, r.h5, r.h6, r.h7,
            };
            return new Output(key: Key, block: block, flag: DefaultFlag | Flag.Parent);
        }

        void AddChunkChainingValue(ref ChainingValue cv, ulong blockCount)
        {
            while ((blockCount & 1) == 0)
            {
                var left = _chainingValueStack.Pop();
                cv = GetParentOutput(ref left, ref cv).ChainingValue;
                blockCount >>= 1;
            }
            _chainingValueStack.Push(cv);
        }

        protected override void HashCore(byte[] array, int ibStart, int cbSize)
        {
            var data = new ReadOnlyMemory<byte>(array, ibStart, cbSize);
            while (!data.IsEmpty)
            {
                if (_chunkState.IsComplete)
                {
                    var cv = _chunkState.Output.ChainingValue;
                    var newBlockCount = _blockCount + 1;
                    AddChunkChainingValue(ref cv, newBlockCount);
                    _chunkState = new ChunkState(Key, newBlockCount, DefaultFlag);
                }

                var available = Math.Min(_chunkState.Needed, data.Length);
                _chunkState.Update(data.Slice(0, available));
                data = data.Slice(available);
            }
        }

        protected override byte[] HashFinal()
        {
            var output = _chunkState.Output;
            var cv = output.ChainingValue;
            while (_chainingValueStack.Count > 0)
            {
                var left = _chainingValueStack.Pop();
                output = GetParentOutput(ref left, ref cv);
            }

            return output.GetRootBytes(HashSize / 8);
        }
    }
}
