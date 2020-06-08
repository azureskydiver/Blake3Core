using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace Blake3Core
{
    class ChunkState
    {
        ChainingValue _cv = new ChainingValue();
        byte[] _block = new byte[Blake3.BlockLength];

        int _blockCount = 0;
        int _blockLength = 0;
        Flag _defaultFlag;

        public ulong ChunkCount { get; private set; } = 0;
        public int Length => _blockCount * Blake3.ChunkLength + _blockLength;
        public int Needed => Blake3.ChunkLength - Length;
        public bool IsComplete => Length == Blake3.ChunkLength;

        public Output Output
        {
            get
            {
                for (int i = _blockLength; i < Blake3.BlockLength; i++)
                    _block[i] = 0;

                var isStart = _blockCount == 0 ? Flag.ChunkStart : Flag.None;
                return new Output(cv: _cv,
                                  block: _block.AsLittleEndianUints(),
                                  counter: ChunkCount,
                                  blockLen: _blockLength,
                                  flag: _defaultFlag | isStart | Flag.ChunkEnd);
            }
        }

        public ChunkState(in ChainingValue cv, ulong chunkCount, Flag defaultFlag)
        {
            _cv = cv;
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

        void CompressBlock(ReadOnlySpan<byte> block)
        {
            var isStart = _blockCount == 0 ? Flag.ChunkStart : Flag.None;
            _cv = Compressor.Compress(cv: _cv,
                                      block: block.AsLittleEndianUints(),
                                      counter: ChunkCount,
                                      flag: _defaultFlag | isStart).cv;
            _blockCount++;
            _blockLength = 0;
        }

        public void Update(ReadOnlySpan<byte> data)
        {
            Debug.Assert(data.Length <= Blake3.ChunkLength);

            if (_blockLength == Blake3.BlockLength)
                CompressBlock(_block);

            while (data.Length > Blake3.BlockLength)
            {
                CompressBlock(data.Slice(0, Blake3.BlockLength));
                data = data.Slice(Blake3.BlockLength);
            }

            var available = Math.Min(data.Length, Blake3.BlockLength - _blockLength);
            data.Slice(0, available).CopyTo(_block);
            _blockLength += available;
        }
    }
}
