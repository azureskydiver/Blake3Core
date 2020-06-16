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

            unsafe
            {
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

        static void Permute(Span<uint> message)
        {
            Span<uint> old = stackalloc uint[16];
            message.CopyTo(old);

            unsafe
            {
                fixed (uint* src = old, m = message)
                {
                    uint* dst = m;
                    *dst++ = src[2];
                    *dst++ = src[6];
                    *dst++ = src[3];
                    *dst++ = src[10];
                    *dst++ = src[7];
                    *dst++ = src[0];
                    *dst++ = src[4];
                    *dst++ = src[13];
                    *dst++ = src[1];
                    *dst++ = src[11];
                    *dst++ = src[12];
                    *dst++ = src[5];
                    *dst++ = src[9];
                    *dst++ = src[14];
                    *dst++ = src[15];
                    *dst++ = src[8];
                }
            }
        }
    }
}
