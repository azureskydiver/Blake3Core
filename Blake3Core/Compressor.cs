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
        public static unsafe State Compress(ReadOnlySpan<uint> cv,
                                            ReadOnlySpan<uint> blockIn,
                                            ulong counter = 0,
                                            int blockLen = Blake3.BlockLength,
                                            Flag flag = Flag.None)
        {
            State s = new State(cv, counter, blockLen, flag);
            uint* state = s.s;

            var block = stackalloc uint[16];
            for (int i = 0; i < 16; i++)
                block[i] = blockIn[i];

            Round();
            Permute();
            Round();
            Permute();
            Round();
            Permute();
            Round();
            Permute();
            Round();
            Permute();
            Round();
            Permute();
            Round();

            for (int i = 0; i < 8; i++)
            {
                state[i] ^= state[i + 8];
                state[i + 8] ^= cv[i];
            }
            return s;

            void Round()
            {
                // Mix the columns.
                G(0, 4, 8, 12, block[0], block[1]);
                G(1, 5, 9, 13, block[2], block[3]);
                G(2, 6, 10, 14, block[4], block[5]);
                G(3, 7, 11, 15, block[6], block[7]);

                // Mix the diagonals.
                G(0, 5, 10, 15, block[8], block[9]);
                G(1, 6, 11, 12, block[10], block[11]);
                G(2, 7, 8, 13, block[12], block[13]);
                G(3, 4, 9, 14, block[14], block[15]);
            }

            void Permute()
            {
                var old = stackalloc uint[16];
                for(int i = 0; i < 16; i++)
                    old[i] = block[i];
                for (int i = 0; i < 16; i++)
                    block[i] = old[Permutation[i]];
            }

            void G(int a, int b, int c, int d, uint mx, uint my)
            {
                unchecked
                {
                    state[a] = state[a] + state[b] + mx;
                    state[d] = (state[d] ^ state[a]).RotateRight(16);
                    state[c] = state[c] + state[d];
                    state[b] = (state[b] ^ state[c]).RotateRight(12);
                    state[a] = state[a] + state[b] + my;
                    state[d] = (state[d] ^ state[a]).RotateRight(8);
                    state[c] = state[c] + state[d];
                    state[b] = (state[b] ^ state[c]).RotateRight(7);
                }
            }
        }

        static readonly int[] Permutation = { 2, 6, 3, 10, 7, 0, 4, 13, 1, 11, 12, 5, 9, 14, 15, 8 };
    }
}
