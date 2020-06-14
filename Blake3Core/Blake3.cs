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
        const int HashSizeInBits = HashSizeInBytes * 8;

        internal const int HashSizeInBytes = 16 * sizeof(uint);
        internal const int ChunkLength = 1024;
        internal const int BlockLength = 16 * sizeof(uint);

        internal static readonly uint [] IV =
            { 0x6A09E667, 0xBB67AE85, 0x3C6EF372, 0xA54FF53A, 0x510E527F, 0x9B05688C, 0x1F83D9AB, 0x5BE0CD19 };

        protected private Flag DefaultFlag;

        ChainingValue _cv;
        ChunkState _chunkState;
        Stack<ChainingValue> _chainingValueStack;

        protected private Blake3(Flag defaultFlag, ReadOnlySpan<uint> key)
        {
            HashSizeValue = HashSizeInBits;
            DefaultFlag = defaultFlag;

            if (key.Length != 8)
                throw new ArgumentException("Must use 256-bit key", nameof(key));
            _cv.Initialize(key.ToArray());
        }

        protected private Blake3(Flag defaultFlag, ReadOnlySpan<byte> key)
            : this(defaultFlag, key.Length == 32 ? key.AsUints() : new ReadOnlySpan<uint>())
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
            _chunkState = new ChunkState(_cv, 0, DefaultFlag);
            _chainingValueStack = new Stack<ChainingValue>();
        }

        Output GetParentOutput(in ChainingValue l, in ChainingValue r)
        {
            Span<ChainingValue> cvs = stackalloc ChainingValue[2] { l, r };
            Span<uint> block = MemoryMarshal.Cast<ChainingValue, uint>(cvs);
            return new Output(cv: _cv, block: block, flag: DefaultFlag | Flag.Parent);
        }

        protected override void HashCore(byte[] array, int ibStart, int cbSize)
        {
            if (_chunkState == null)
                Initialize();

            var data = new ReadOnlySpan<byte>(array, ibStart, cbSize);
            while (!data.IsEmpty)
            {
                if (_chunkState.IsComplete)
                {
                    AddChunkChainingValue(_chunkState.Output.ChainingValue);
                    _chunkState = new ChunkState(_cv, _chunkState.ChunkCount + 1, DefaultFlag);
                }

                var available = Math.Min(_chunkState.Needed, data.Length);
                _chunkState.Update(data.Slice(0, available));
                data = data.Slice(available);
            }

            void AddChunkChainingValue(ChainingValue cv)
            {
                var chunkCount = _chunkState.ChunkCount + 1;
                while ((chunkCount & 1) == 0)
                {
                    cv = GetParentOutput(_chainingValueStack.Pop(), cv).ChainingValue;
                    chunkCount >>= 1;
                }
                _chainingValueStack.Push(cv);
            }
        }

        protected override byte[] HashFinal()
        {
            var output = _chunkState.Output;
            var cv = output.ChainingValue;
            while (_chainingValueStack.Count > 0)
                output = GetParentOutput(_chainingValueStack.Pop(), cv);

            return output.GetRootBytes(HashSize / 8);
        }
    }
}
