using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Blake3Core
{
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    unsafe ref struct State
    {
        [FieldOffset(0)] public fixed uint s[16];
        [FieldOffset(0)] public ChainingValue cv;

        public State(ReadOnlySpan<uint> cvIn,
                     ulong counter = 0,
                     int blockLen = Blake3.BlockLength,
                     Flag flag = Flag.None)
        {
            cv = MemoryMarshal.Cast<uint, ChainingValue>(cvIn)[0];

            s[8] = Blake3.IV[0];
            s[9] = Blake3.IV[1];
            s[10] = Blake3.IV[2];
            s[11] = Blake3.IV[3];
            s[12] = (uint)counter;
            s[13] = (uint)(counter >> 32);
            s[14] = (uint)blockLen;
            s[15] = (uint)flag;
        }
    }
}
