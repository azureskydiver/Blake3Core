﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace Blake3Core
{
    static class Compressor
    {
        public static unsafe State Compress(in ChainingValue cv,
                                            ReadOnlySpan<uint> block,
                                            ulong counter = 0,
                                            int blockLen = Blake3.BlockLength,
                                            Flag flag = Flag.None)
        {
            State state = new State(cv, counter, blockLen, flag);

            uint* m = stackalloc uint[16];
            fixed(uint * b = block)
            {
                m[ 0] = b[ 0];
                m[ 1] = b[ 1];
                m[ 2] = b[ 2];
                m[ 3] = b[ 3];
                m[ 4] = b[ 4];
                m[ 5] = b[ 5];
                m[ 6] = b[ 6];
                m[ 7] = b[ 7];
                m[ 8] = b[ 8];
                m[ 9] = b[ 9];
                m[10] = b[10];
                m[11] = b[11];
                m[12] = b[12];
                m[13] = b[13];
                m[14] = b[14];
                m[15] = b[15];
            }

            uint* s = &state.s[0];
            Round(s, m);
            Permute(m);
            Round(s, m);
            Permute(m);
            Round(s, m);
            Permute(m);
            Round(s, m);
            Permute(m);
            Round(s, m);
            Permute(m);
            Round(s, m);
            Permute(m);
            Round(s, m);

            fixed (uint * hashes = &cv.h[0])
            {
                uint* lo = &state.s[0];
                uint* hi = &lo[8];
                uint* hi2 = &lo[8];
                uint* h = hashes;

                for(int i = 0; i < 8; i++)
                {
                    *lo++ ^= *hi++;
                    *hi2++ ^= *h++;
                }
            }

            return state;
        }

        static unsafe void Round(uint * s, uint* m)
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
        static unsafe void G(uint * s, int a, int b, int c, int d, uint mx, uint my)
        {
            unchecked
            {
                uint sa = s[a];
                uint sb = s[b];
                uint sc = s[c];
                uint sd = s[d];

                sa += sb + mx;
                sd = (sd ^ sa).RotateRight(16);
                sc += sd;
                sb = (sb ^ sc).RotateRight(12);
                sa += sb + my;
                sd = (sd ^ sa).RotateRight(8);
                sc += sd;
                sb = (sb ^ sc).RotateRight(7);

                s[a] = sa;
                s[b] = sb;
                s[c] = sc;
                s[d] = sd;
            }
        }

        static unsafe void Permute(uint* m)
        {
            uint* old = stackalloc uint[16];
            Buffer.MemoryCopy(m, old, 16 * sizeof(uint), 16 * sizeof(uint));

            m[ 0] = old[ 2];
            m[ 1] = old[ 6];
            m[ 2] = old[ 3];
            m[ 3] = old[10];
            m[ 4] = old[ 7];
            m[ 5] = old[ 0];
            m[ 6] = old[ 4];
            m[ 7] = old[13];
            m[ 8] = old[ 1];
            m[ 9] = old[11];
            m[10] = old[12];
            m[11] = old[ 5];
            m[12] = old[ 9];
            m[13] = old[14];
            m[14] = old[15];
            m[15] = old[ 8];
        }
    }
}
