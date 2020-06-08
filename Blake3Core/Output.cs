using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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

        public unsafe byte[] GetRootBytes(int count)
        {
            ulong counter = 0;
            var bytes = new byte[count];
            fixed (byte* bytesStart = bytes)
            {
                byte* dst = bytesStart;
                while (count > 0)
                {
                    var state = Compressor.Compress(cv: _cv,
                                                    block: _block,
                                                    counter: counter,
                                                    blockLen: _blockLen,
                                                    flag: _flag | Flag.Root);

                    var bytesNeeded = Math.Min(count, Blake3.HashSizeInBytes);
                    Unsafe.CopyBlock(dst, state.s, (uint)bytesNeeded);

                    dst += bytesNeeded;
                    count -= bytesNeeded;
                    counter++;
                }
            }

            return bytes;
        }
    }
}
