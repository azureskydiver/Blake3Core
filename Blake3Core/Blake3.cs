using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;

namespace Blake3Core
{
    public class Blake3 : HashAlgorithm
    {
        const int HashSizeInBits = HashSizeInBytes * 8;

        internal const int HashSizeInBytes = 16 * sizeof(uint);
        internal const int ChunkLength = 1024;
        internal const int BlockLength = 16 * sizeof(uint);

        internal const uint IV0 = 0x6A09E667;
        internal const uint IV1 = 0xBB67AE85;
        internal const uint IV2 = 0x3C6EF372;
        internal const uint IV3 = 0xA54FF53A;

        internal static readonly uint [] IV =
            { IV0, IV1, IV2, IV3, 0x510E527F, 0x9B05688C, 0x1F83D9AB, 0x5BE0CD19 };

        protected private Flag DefaultFlag;

        ChainingValue _cv;
        ChunkState _chunkState;
        Stack<ChainingValue> _chainingValueStack;
        Output _output;

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
            _output = null;
            if (_chunkState == null)
                Initialize();

            var data = new ReadOnlyMemory<byte>(array, ibStart, cbSize);
            if (data.IsEmpty)
                return;

            data = FillIncompletePreviousChunk(data);
            data = ProcessChunkRuns(data);

            while (!data.IsEmpty)
            {
                if (_chunkState.IsComplete)
                {
                    AddChunkChainingValue(_chunkState.Output.ChainingValue);
                    _chunkState = new ChunkState(_cv, _chunkState.ChunkCount + 1, DefaultFlag);
                }

                var available = Math.Min(_chunkState.Needed, data.Length);
                _chunkState.Update(data.Span.Slice(0, available));
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

            ReadOnlyMemory<byte> FillIncompletePreviousChunk(ReadOnlyMemory<byte> data)
            {
                if (_chunkState.IsComplete)
                {
                    AddChunkChainingValue(_chunkState.Output.ChainingValue);
                    _chunkState = new ChunkState(_cv, _chunkState.ChunkCount + 1, DefaultFlag);
                }

                if (_chunkState.Needed == Blake3.ChunkLength)
                    return data;

                var available = Math.Min(_chunkState.Needed, data.Length);
                _chunkState.Update(data.Span.Slice(0, available));
                data = data.Slice(available);

                if (!data.IsEmpty && _chunkState.IsComplete)
                {
                    AddChunkChainingValue(_chunkState.Output.ChainingValue);
                    _chunkState = new ChunkState(_cv, _chunkState.ChunkCount + 1, DefaultFlag);
                }
                return data;
            }

            ReadOnlyMemory<byte> ProcessChunkRuns(ReadOnlyMemory<byte> data)
            {
                if (data.IsEmpty)
                    return data;

                while (data.Length > 2 * Blake3.ChunkLength)
                {
                    var chunkCount = data.Length / Blake3.ChunkLength;
                    chunkCount = PowerOfTwo(chunkCount);
                    if (chunkCount * Blake3.ChunkLength == data.Length)
                        chunkCount /= 2;
                    if (chunkCount < 2)
                        break;

                    data = ProcessChunkRun(data, chunkCount);
                }
                return data;
            }

            ReadOnlyMemory<byte> ProcessChunkRun(ReadOnlyMemory<byte> data, int chunkCount)
            {
                for (int i = 0; i < chunkCount; i++)
                {
                    _chunkState.Update(data.Span.Slice(0, Blake3.ChunkLength));
                    data = data.Slice(Blake3.ChunkLength);
                    AddChunkChainingValue(_chunkState.Output.ChainingValue);
                    _chunkState = new ChunkState(_cv, _chunkState.ChunkCount + 1, DefaultFlag);
                }
                return data;
            }
        }

        static readonly (uint Mask, int Bits)[] _powerOfTwoBits = new (uint Mask, int Bits)[]
        {
            ( 0xFFFF0000, 16 ),
            ( 0x0000FF00,  8 ),
            ( 0x000000F0,  4 ),
            ( 0x0000000C,  2 ),
            ( 0x00000002,  1 ),
        };

        static int PowerOfTwo(int value)
        {
            if (value == 0)
                return 0;

            var count = 0;
            foreach(var (mask, bits) in _powerOfTwoBits)
            {
                if (((uint)value & mask) != 0)
                {
                    value >>= bits;
                    count |= bits;
                }
            }
            return 1 << count;
        }

        protected override byte[] HashFinal()
        {
            _output = _chunkState.Output;
            while (_chainingValueStack.Count > 0)
                _output = GetParentOutput(_chainingValueStack.Pop(), _output.ChainingValue);

            return GetExtendedOutput().Take(HashSize / 8).ToArray();
        }

        public IEnumerable<byte> GetExtendedOutput() => _output.GetRootBytes();
    }
}
