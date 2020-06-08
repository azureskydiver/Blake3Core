using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Blake3Core
{
    unsafe struct ChainingValue
    {
        public fixed uint h[8];

        public void Initialize(ReadOnlySpan<uint> k)
        {
            for (int i = 0; i < 8; i++)
                h[i] = k[i];
        }
    }
}
