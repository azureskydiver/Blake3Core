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
        ulong _chunkCount = 0;
        Flag _defaultFlag = Flag.None;
        Flag _flags = Flag.None;
        int _blockCount = 0;

        public bool IsComplete => Length == 0;
        public int Length { get; private set; } = 0;

        public ChunkState(Flag defaultFlag)
        {
            _defaultFlag = defaultFlag;
            _flags = defaultFlag;
        }

        public void MoveToNextChunk()
        {
            _chunkCount++;
            _flags = _defaultFlag | Flag.ChunkStart;
            Length = 0;
        }

        public void Update(ReadOnlyMemory<byte> data)
        {
            Debug.Assert(data.Length <= Blake3.ChunkLength);

            //$ TODO: Cut chunk into blocks and process the blocks
        }

        public void EndChunk()
        {
            //$ TODO: end a chunk
        }

        public ref ChainingValue ComputeChainingValue()
        {
            //$ TODO: compute chaining value with Flag.ChunkEnd and maybe Flag.ChunkStart
            return ref _cv;
        }
    }
}
