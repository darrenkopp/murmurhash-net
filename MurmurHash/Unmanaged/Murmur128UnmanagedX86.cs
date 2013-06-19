/// Copyright 2012 Darren Kopp
///
/// Licensed under the Apache License, Version 2.0 (the "License");
/// you may not use this file except in compliance with the License.
/// You may obtain a copy of the License at
///
///    http://www.apache.org/licenses/LICENSE-2.0
///
/// Unless required by applicable law or agreed to in writing, software
/// distributed under the License is distributed on an "AS IS" BASIS,
/// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
/// See the License for the specific language governing permissions and
/// limitations under the License.

using System.Runtime.CompilerServices;

namespace Murmur
{
    internal class Murmur128UnmanagedX86 : Murmur128
    {
        const uint c1 = 0x239b961b;
        const uint c2 = 0xab0e9789;
        const uint c3 = 0x38b34ae5;
        const uint c4 = 0xa1e38b93;

        private uint h1;
        private uint h2;
        private uint h3;
        private uint h4;

        internal Murmur128UnmanagedX86(uint seed = 0)
            : base(seed: seed)
        {
        }

        private int Length { get; set; }

        public override void Initialize()
        {
            // initialize hash values to seed values
            h1 = h2 = h3 = h4 = Seed;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void HashCore(byte[] array, int ibStart, int cbSize)
        {
            // store the length of the hash (for use later)
            Length = cbSize;

            // only compute the hash if we have data to hash
            if (Length > 0)
            {
                int count = Length / 16;
                int remainder = Length & 15;

                unsafe
                {
                    // grab pointer to first byte in array
                    fixed (byte* data = &array[0])
                    {
                        Body(data, count);
                        if (remainder > 0)
                            Tail(data + (Length - remainder), remainder);
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe private void Body(byte* data, int count)
        {
            // grab a reference to blocks
            uint* blocks = (uint*)data;
            while (count-- > 0)
            {
                // grab our 4 byte key segments, stepping our offset position back each time
                // thus we are walking our array backwards
                uint k1 = *blocks++,
                     k2 = *blocks++,
                     k3 = *blocks++,
                     k4 = *blocks++;

                h1 = h1 ^ ((k1 * c1).RotateLeft(15) * c2);
                h1 = (h1.RotateLeft(19) + h2) * 5 + 0x561ccd1b;

                h2 = h2 ^ ((k2 * c2).RotateLeft(16) * c3);
                h2 = (h2.RotateLeft(17) + h3) * 5 + 0x0bcaa747;

                h3 = h3 ^ ((k3 * c3).RotateLeft(17) * c4);
                h3 = (h3.RotateLeft(15) + h4) * 5 + 0x96cd1c35;

                h4 = h4 ^ ((k4 * c4).RotateLeft(18) * c1);
                h4 = (h4.RotateLeft(13) + h1) * 5 + 0x32ac3b17;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe private void Tail(byte* tail, int remainder)
        {
            // create our keys and initialize to 0
            uint k1 = 0, k2 = 0, k3 = 0, k4 = 0;
            int position = Length - 1;

            // determine how many bytes we have left to work with based on length
            switch (remainder)
            {
                case 15: k4 ^= (uint)tail[14] << 16; goto case 14;
                case 14: k4 ^= (uint)tail[13] << 8; goto case 13;
                case 13: k4 ^= (uint)tail[12] << 0; goto case 12;
                case 12: k3 ^= (uint)tail[11] << 24; goto case 11;
                case 11: k3 ^= (uint)tail[10] << 16; goto case 10;
                case 10: k3 ^= (uint)tail[9] << 8; goto case 9;
                case 9: k3 ^= (uint)tail[8] << 0; goto case 8;
                case 8: k2 ^= (uint)tail[7] << 24; goto case 7;
                case 7: k2 ^= (uint)tail[6] << 16; goto case 6;
                case 6: k2 ^= (uint)tail[5] << 8; goto case 5;
                case 5: k2 ^= (uint)tail[4] << 0; goto case 4;
                case 4: k1 ^= (uint)tail[3] << 24; goto case 3;
                case 3: k1 ^= (uint)tail[2] << 16; goto case 2;
                case 2: k1 ^= (uint)tail[1] << 8; goto case 1;
                case 1: k1 ^= (uint)tail[0] << 0; break;
            }

            h4 = h4 ^ ((k4 * c4).RotateLeft(18) * c1);
            h3 = h3 ^ ((k3 * c3).RotateLeft(17) * c4);
            h2 = h2 ^ ((k2 * c2).RotateLeft(16) * c3);
            h1 = h1 ^ ((k1 * c1).RotateLeft(15) * c2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override byte[] HashFinal()
        {
            uint len = (uint)Length;
            // pipelining friendly algorithm
            h1 ^= len; h2 ^= len; h3 ^= len; h4 ^= len;

            h1 += (h2 + h3 + h4);
            h2 += h1; h3 += h1; h4 += h1;

            h1 = h1.FMix();
            h2 = h2.FMix();
            h3 = h3.FMix();
            h4 = h4.FMix();

            h1 += (h2 + h3 + h4);
            h2 += h1; h3 += h1; h4 += h1;

            var result = new byte[16];
            unsafe
            {
                fixed (byte* h = result)
                {
                    var r = (uint*)h;

                    r[0] = h1;
                    r[1] = h2;
                    r[2] = h3;
                    r[3] = h4;
                }
            }

            return result;
        }
    }
}
