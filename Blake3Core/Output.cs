using System;
using System.Collections.Generic;
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

        public byte[] GetRootBytes(int count)
        {
            ulong counter = 0;
            var bytes = new byte[count];
            var outputSpan = bytes.AsSpan();
            while (!outputSpan.IsEmpty)
            {
                var state = Compressor.Compress(cv: _cv,
                                                block: _block,
                                                counter: counter,
                                                blockLen: _blockLen,
                                                flag: _flag | Flag.Root);

                var stateBytes = state.AsBytes();
                var bytesNeeded = Math.Min(outputSpan.Length, stateBytes.Length);
                stateBytes.Slice(0, bytesNeeded).CopyTo(outputSpan);
                outputSpan = outputSpan.Slice(bytesNeeded);

                count -= bytesNeeded;
                counter++;
            }

            return bytes;
        }
    }
}
