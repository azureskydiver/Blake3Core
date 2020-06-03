using System;
using System.Collections.Generic;
using System.Text;

namespace Blake3Core
{
    class ChunkState
    {
        ChainingValue _cv = new ChainingValue();
        byte[] _block = new byte[Blake3.BlockLength];
        int _blockLength = 0;
        ulong _chunkCount = 0;
        Flag _flags = 0;
        int _blockCount = 0;
    }
}
