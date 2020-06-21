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
    class RoundState
    {
        public Vector<uint> sa;
        public Vector<uint> sb;
        public Vector<uint> sc;
        public Vector<uint> sd;

        public unsafe RoundState(uint* s)
        {
            sa = new Vector<uint>(stackalloc uint[] { s[ 0], s[ 1], s[ 2], s[ 3] });
            sb = new Vector<uint>(stackalloc uint[] { s[ 4], s[ 5], s[ 6], s[ 7] });
            sc = new Vector<uint>(stackalloc uint[] { s[ 8], s[ 9], s[10], s[11] });
            sd = new Vector<uint>(stackalloc uint[] { s[12], s[13], s[14], s[15] });
        }

        public unsafe void Round(uint* m)
        {
            G(new Vector<uint>(stackalloc uint[] { m[0], m[2], m[4], m[6] }),
              new Vector<uint>(stackalloc uint[] { m[1], m[3], m[5], m[7] }));
            Diagonalize();
            G(new Vector<uint>(stackalloc uint[] { m[8], m[10], m[12], m[14] }),
              new Vector<uint>(stackalloc uint[] { m[9], m[11], m[13], m[15] }));
            Undiagonalize();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void G(in Vector<uint> mx, in Vector<uint> my)
        {
            sa += sb + mx;
            sd = RotateRight(sd ^ sa, 16);
            sc += sd;
            sb = RotateRight(sb ^ sc, 12);
            sa += sb + my;
            sd = RotateRight(sd ^ sa, 8);
            sc += sd;
            sb = RotateRight(sb ^ sc, 7);
        }

        void Diagonalize()
        {
            //$ NYI
            var a = new Vector<uint>(stackalloc uint[] { sa[0], sa[1], sa[2], sa[3] });
            var b = new Vector<uint>(stackalloc uint[] { sb[0], sb[1], sb[2], sb[3] });
            var c = new Vector<uint>(stackalloc uint[] { sc[0], sc[1], sc[2], sc[3] });
            var d = new Vector<uint>(stackalloc uint[] { sd[0], sd[1], sd[2], sd[3] });

            sa = a;
            sb = b;
            sc = c;
            sd = d;
        }

        void Undiagonalize()
        {
            //$ NYI
            var a = new Vector<uint>(stackalloc uint[] { sa[0], sa[1], sa[2], sa[3] });
            var b = new Vector<uint>(stackalloc uint[] { sb[0], sb[1], sb[2], sb[3] });
            var c = new Vector<uint>(stackalloc uint[] { sc[0], sc[1], sc[2], sc[3] });
            var d = new Vector<uint>(stackalloc uint[] { sd[0], sd[1], sd[2], sd[3] });

            sa = a;
            sb = b;
            sc = c;
            sd = d;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe Vector<uint> RotateRight(in Vector<uint> v, int count)
        {
            Span<uint> newValues = stackalloc uint[]
            {
                v[0].RotateRight(count),
                v[1].RotateRight(count),
                v[2].RotateRight(count),
                v[3].RotateRight(count),
            };
            return new Vector<uint>(newValues);
        }
    }

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
            uint* scheduledMessages = stackalloc uint[16 * 6];

            var roundState = new RoundState(state.s);

            fixed (uint * m = block)
            {
                ScheduleMessages(scheduledMessages, m);
                roundState.Round(m);
            }

            roundState.Round(&scheduledMessages[0 * 16]);
            roundState.Round(&scheduledMessages[1 * 16]);
            roundState.Round(&scheduledMessages[2 * 16]);
            roundState.Round(&scheduledMessages[3 * 16]);
            roundState.Round(&scheduledMessages[4 * 16]);
            roundState.Round(&scheduledMessages[5 * 16]);

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

        static unsafe void ScheduleMessages(uint *scheduledMessages, uint* m)
        {
            uint* dst = scheduledMessages;
            for (int i = 0; i < 6; i++)
            {
                fixed(uint * schedule = &MessageSchedule[i][0])
                {
                    dst[ 0] = m[schedule[ 0]];
                    dst[ 1] = m[schedule[ 1]];
                    dst[ 2] = m[schedule[ 2]];
                    dst[ 3] = m[schedule[ 3]];
                    dst[ 4] = m[schedule[ 4]];
                    dst[ 5] = m[schedule[ 5]];
                    dst[ 6] = m[schedule[ 6]];
                    dst[ 7] = m[schedule[ 7]];
                    dst[ 8] = m[schedule[ 8]];
                    dst[ 9] = m[schedule[ 9]];
                    dst[10] = m[schedule[10]];
                    dst[11] = m[schedule[11]];
                    dst[12] = m[schedule[12]];
                    dst[13] = m[schedule[13]];
                    dst[14] = m[schedule[14]];
                    dst[15] = m[schedule[15]];

                    dst += 16;
                }
            }
        }

        static unsafe void Round(uint * s, uint* m)
        {
            // Mix the columns.
            G(s, 0, 4,  8, 12, m[ 0], m[ 1]);
            G(s, 1, 5,  9, 13, m[ 2], m[ 3]);
            G(s, 2, 6, 10, 14, m[ 4], m[ 5]);
            G(s, 3, 7, 11, 15, m[ 6], m[ 7]);

            // Mix the diagonals.
            G(s, 0, 5, 10, 15, m[ 8], m[ 9]);
            G(s, 1, 6, 11, 12, m[10], m[11]);
            G(s, 2, 7,  8, 13, m[12], m[13]);
            G(s, 3, 4,  9, 14, m[14], m[15]);
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

        static Compressor()
        {
            BuildMessageSchedule();
        }

        static readonly uint[][] MessageSchedule = new uint[6][];

        static void BuildMessageSchedule()
        {
            var old = Enumerable.Range(0, 16).Select(i => (uint)i).ToArray();
            for(int i = 0; i < 6; i++)
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
