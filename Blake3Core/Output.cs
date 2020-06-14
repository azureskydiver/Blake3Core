using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Blake3Core
{
    class Output
    {
        public ChainingValue ChainingValue { get; }

        ChainingValue _cv;
        uint[] _block;
        int _blockLen;
        Flag _flag;

        public Output(in ChainingValue cv,
                      ReadOnlySpan<uint> block,
                      ulong counter = 0,
                      int blockLen = Blake3.BlockLength,
                      Flag flag = Flag.None)
        {
            _cv = cv;
            _block = block.ToArray();
            _blockLen = blockLen;
            _flag = flag;

            ChainingValue = Compressor.Compress(cv: _cv,
                                                block: _block,
                                                counter: counter,
                                                blockLen: _blockLen,
                                                flag: _flag).cv;
        }

        public IEnumerable<byte> GetRootBytes()
        {
            ulong counter = 0;
            while (true)
            {
                var state = Compressor.Compress(cv: _cv,
                                                block: _block,
                                                counter: counter,
                                                blockLen: _blockLen,
                                                flag: _flag | Flag.Root);

                var stateBytes = state.AsBytes().ToArray();
                foreach(var b in stateBytes)
                    yield return b;

                counter++;
            }
        }
    }
}
