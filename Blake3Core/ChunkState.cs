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
        Flag _defaultFlag = Flag.None;
        Flag _flags = Flag.None;
        int _blockCount = 0;

        public int Length { get; private set; } = 0;
        public ulong ChunkCount { get; private set; } = 0;

        public ChunkState(Flag defaultFlag)
        {
            _defaultFlag = defaultFlag;
            _flags = defaultFlag;
        }

        public void MoveToNextChunk()
        {
            ChunkCount++;
            _flags = _defaultFlag | Flag.ChunkStart;
            Length = 0;
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
