using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Blake3Core
{
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    unsafe ref struct State
    {
        [FieldOffset(0)] public fixed uint s[16];
        [FieldOffset(0)] public ChainingValue cv;

        public State(in ChainingValue cv,
                     ulong counter = 0,
                     int blockLen = Blake3.BlockLength,
                     Flag flag = Flag.None)
        {
            this.cv = cv;

            fixed (uint* s8 = &s[8])
            {
                uint* dst = s8;
                *dst++ = Blake3.IV0;
                *dst++ = Blake3.IV1;
                *dst++ = Blake3.IV2;
                *dst++ = Blake3.IV3;
                *dst++ = (uint)counter;
                *dst++ = (uint)(counter >> 32);
                *dst++ = (uint)blockLen;
                *dst++ = (uint)flag;
            }
        }

        public void Compress(ReadOnlySpan<uint> block)
        {
            fixed(uint* state = s)
                Compressor.Compress(block, state);
        }

        public void CompressXof(in ChainingValue cv, ReadOnlySpan<uint> block)
        {
            fixed (uint* state = s)
            {
                Compressor.Compress(block, state);

                fixed (uint* h = &cv.h[0])
                {
                    s[ 8] ^= h[0];
                    s[ 9] ^= h[1];
                    s[10] ^= h[2];
                    s[11] ^= h[3];
                    s[12] ^= h[4];
                    s[13] ^= h[5];
                    s[14] ^= h[6];
                    s[15] ^= h[7];
                }
            }
        }

        public ReadOnlySpan<uint> AsUints()
        {
            fixed (uint * states = s)
                return new ReadOnlySpan<uint>(states, 16);
        }

        public ReadOnlySpan<byte> AsBytes()
            => AsUints().AsBytes();

        public Span<uint> AsWritableUints()
        {
            fixed (uint* states = s)
                return new Span<uint>(states, 16);
        }
    }
}
