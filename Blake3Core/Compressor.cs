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
        public static unsafe State Compress(in ChainingValue cv,
                                            ReadOnlySpan<uint> block,
                                            ulong counter = 0,
                                            int blockLen = Blake3.BlockLength,
                                            Flag flag = Flag.None)
        {
            State s = new State(cv, counter, blockLen, flag);
            uint* state = s.s;

            var message = stackalloc uint[16];
            for (int i = 0; i < 16; i++)
                message[i] = block[i];

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
                state[i + 8] ^= cv.h[i];
            }
            return s;

            void Round()
            {
                // Mix the columns.
                G(0, 4, 8, 12, message[0], message[1]);
                G(1, 5, 9, 13, message[2], message[3]);
                G(2, 6, 10, 14, message[4], message[5]);
                G(3, 7, 11, 15, message[6], message[7]);

                // Mix the diagonals.
                G(0, 5, 10, 15, message[8], message[9]);
                G(1, 6, 11, 12, message[10], message[11]);
                G(2, 7, 8, 13, message[12], message[13]);
                G(3, 4, 9, 14, message[14], message[15]);
            }

            void Permute()
            {
                var old = stackalloc uint[16];
                for(int i = 0; i < 16; i++)
                    old[i] = message[i];
                for (int i = 0; i < 16; i++)
                    message[i] = old[Permutation[i]];
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
