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
        readonly ChainingValue _cv;
        readonly uint[] _block;
        readonly int _blockLen;
        readonly ulong _counter;
        readonly Flag _flag;

        public ChainingValue ChainingValue
        {
            get
            {
                var state = new State(cv: _cv,
                                      counter: _counter,
                                      blockLen: _blockLen,
                                      flag: _flag);
                state.Compress(_block);
                return state.cv;
            }
        }

        public Output(in ChainingValue cv,
                      ReadOnlySpan<uint> block,
                      ulong counter = 0,
                      int blockLen = Blake3.BlockLength,
                      Flag flag = Flag.None)
        {
            _cv = cv;
            _block = block.ToArray();
            _counter = counter;
            _blockLen = blockLen;
            _flag = flag;
        }

        public IEnumerable<byte> GetRootBytes()
        {
            ulong counter = 0;
            while (true)
            {
                var state = new State(cv: _cv,
                                      counter: counter,
                                      blockLen: _blockLen,
                                      flag: _flag | Flag.Root);
                state.CompressXof(_cv, _block);
                var stateBytes = state.AsBytes().ToArray();
                foreach(var b in stateBytes)
                    yield return b;

                counter++;
            }
        }
    }
}
