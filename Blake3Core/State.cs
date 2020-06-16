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
