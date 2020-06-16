using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace Blake3Core
{
    static class Compressor
    {
        public static State Compress(in ChainingValue cv,
                                     ReadOnlySpan<uint> block,
                                     ulong counter = 0,
                                     int blockLen = Blake3.BlockLength,
                                     Flag flag = Flag.None)
        {
            Span<uint> message = stackalloc uint[16];
            block.CopyTo(message);

            State state = new State(cv, counter, blockLen, flag);
            var s = state.AsWritableUints();

            Round(s, message);
            Permute(message);
            Round(s, message);
            Permute(message);
            Round(s, message);
            Permute(message);
            Round(s, message);
            Permute(message);
            Round(s, message);
            Permute(message);
            Round(s, message);
            Permute(message);
            Round(s, message);

            var h = cv.AsUints();
            for (int i = 0; i < 8; i++)
            {
                s[i] ^= s[i + 8];
                s[i + 8] ^= h[i];
            }
            return state;
        }

        static void Round(Span<uint> s, ReadOnlySpan<uint> m)
        {
            // Mix the columns.
            G(s, 0, 4, 8, 12, m[0], m[1]);
            G(s, 1, 5, 9, 13, m[2], m[3]);
            G(s, 2, 6, 10, 14, m[4], m[5]);
            G(s, 3, 7, 11, 15, m[6], m[7]);

            // Mix the diagonals.
            G(s, 0, 5, 10, 15, m[8], m[9]);
            G(s, 1, 6, 11, 12, m[10], m[11]);
            G(s, 2, 7, 8, 13, m[12], m[13]);
            G(s, 3, 4, 9, 14, m[14], m[15]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void G(Span<uint> s, int a, int b, int c, int d, uint mx, uint my)
        {
            unchecked
            {
                s[a] = s[a] + s[b] + mx;
                s[d] = (s[d] ^ s[a]).RotateRight(16);
                s[c] = s[c] + s[d];
                s[b] = (s[b] ^ s[c]).RotateRight(12);
                s[a] = s[a] + s[b] + my;
                s[d] = (s[d] ^ s[a]).RotateRight(8);
                s[c] = s[c] + s[d];
                s[b] = (s[b] ^ s[c]).RotateRight(7);
            }
        }

        static void Permute(Span<uint> m)
        {
            Span<uint> old = stackalloc uint[16];
            for (int i = 0; i < 16; i++)
                old[i] = m[i];
            for (int i = 0; i < 16; i++)
                m[i] = old[Permutation[i]];
        }

        static readonly int[] Permutation = { 2, 6, 3, 10, 7, 0, 4, 13, 1, 11, 12, 5, 9, 14, 15, 8 };
    }
}
