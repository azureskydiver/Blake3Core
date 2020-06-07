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

        public unsafe State(uint * state)
        {
            cv = new ChainingValue();
            for (int i = 0; i < 16; i++)
                s[i] = state[i];
        }
    }
}
