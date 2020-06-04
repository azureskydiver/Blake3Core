using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Blake3Core
{
    class ChunkState
    {
        ChainingValue _cv = new ChainingValue();
        byte[] _block = new byte[Blake3.BlockLength];

        uint[] _key;
        ulong _blockCount = 0;
        Flag _defaultFlag;

        public int Length { get; private set; } = 0;
        public ulong ChunkCount { get; private set; } = 0;
        public bool IsComplete => Length == Blake3.ChunkLength;
        public int Needed => Blake3.ChunkLength - Length;
        public Output Output => null;

        public ChunkState(ReadOnlySpan<uint> key, ulong chunkCount, Flag defaultFlag)
        {
            _key = key.ToArray();
            _defaultFlag = defaultFlag;
            ChunkCount = chunkCount;
        }

        public void ZeroFillRestOfChunk()
        {
            var zeroesNeeded = Blake3.ChunkLength - Length;
            Debug.Assert(zeroesNeeded >= 0);
            if (zeroesNeeded > 0)
            {
                var buffer = new byte[zeroesNeeded];
                Update(buffer);
            }
        }

        public void Update(ReadOnlyMemory<byte> data)
        {
            Debug.Assert(data.Length <= Blake3.ChunkLength);

            //$ TODO: Cut chunk into blocks and process the blocks
        }

        public ref ChainingValue ComputeChainingValue()
        {
            //$ TODO: compute chaining value with Flag.ChunkEnd and maybe Flag.ChunkStart
            return ref _cv;
        }
    }
}
