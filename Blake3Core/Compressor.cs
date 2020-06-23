using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
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
            uint* s = &state.s[0];

            fixed (uint * m = block)
            {
                Round(s, m, 0);
                Round(s, m, 1);
                Round(s, m, 2);
                Round(s, m, 3);
                Round(s, m, 4);
                Round(s, m, 5);
                Round(s, m, 6);
            }

            s[0] ^= s[ 8];
            s[1] ^= s[ 9];
            s[2] ^= s[10];
            s[3] ^= s[11];
            s[4] ^= s[12];
            s[5] ^= s[13];
            s[6] ^= s[14];
            s[7] ^= s[15];

            fixed (uint * h = &cv.h[0])
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

            return state;
        }

        static unsafe void Round(uint * s, uint* m, int round)
        {
            fixed (uint* schedule = &MessageSchedule[round][0])
            {
                // Mix the columns.
                G(s, 0, 4,  8, 12, m[schedule[ 0]], m[schedule[ 1]]);
                G(s, 1, 5,  9, 13, m[schedule[ 2]], m[schedule[ 3]]);
                G(s, 2, 6, 10, 14, m[schedule[ 4]], m[schedule[ 5]]);
                G(s, 3, 7, 11, 15, m[schedule[ 6]], m[schedule[ 7]]);

                // Mix the diagonals.
                G(s, 0, 5, 10, 15, m[schedule[ 8]], m[schedule[ 9]]);
                G(s, 1, 6, 11, 12, m[schedule[10]], m[schedule[11]]);
                G(s, 2, 7,  8, 13, m[schedule[12]], m[schedule[13]]);
                G(s, 3, 4,  9, 14, m[schedule[14]], m[schedule[15]]);
            }
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
                sd ^= sa;
                sd = (sd >> 16) | (sd << (32 - 16));
                sc += sd;
                sb ^= sc;
                sb = (sb >> 12) | (sb << (32 - 12));
                sa += sb + my;
                sd ^= sa;
                sd = (sd >> 8) | (sd << (32 - 8));
                sc += sd;
                sb ^= sc;
                sb = (sb >> 7) | (sb << (32 - 7));

                s[a] = sa;
                s[b] = sb;
                s[c] = sc;
                s[d] = sd;
            }
        }

        static Compressor()
        {
            BuildMessageSchedule();
        }

        static readonly uint[][] MessageSchedule = new uint[7][];

        static void BuildMessageSchedule()
        {
            var old = Enumerable.Range(0, 16).Select(i => (uint)i).ToArray();
            MessageSchedule[0] = old;

            for(int i = 1; i < 7; i++)
            {
                var m = new uint[16];
                MessageSchedule[i] = m;

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

                old = m;
            }
        }
    }
}
