﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Blake3Core
{
    static class BitOperationExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint RotateRight(this uint value, int count)
            => (value >> count) | (value << (32 - count));
    }
}
