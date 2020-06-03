using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Blake3Core
{
    static class SpanExtensions
    {
        public static ReadOnlySpan<byte> AsBytes<T>(this T[] span) where T : struct
            => MemoryMarshal.AsBytes<T>(span);

        public static ReadOnlySpan<uint> AsUints<T>(this T[] span) where T : struct
            => MemoryMarshal.Cast<T, uint>(span);

        public static ReadOnlySpan<uint> AsUints<T>(this ReadOnlySpan<T> span) where T : struct
            => MemoryMarshal.Cast<T, uint>(span);
    }
}
