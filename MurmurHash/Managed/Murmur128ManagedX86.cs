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

using System;
using System.Runtime.CompilerServices;

namespace Murmur
{
    internal class Murmur128ManagedX86 : Murmur128
    {
        const uint c1 = 0x239b961b;
        const uint c2 = 0xab0e9789;
        const uint c3 = 0x38b34ae5;
        const uint c4 = 0xa1e38b93;

        private uint h1;
        private uint h2;
        private uint h3;
        private uint h4;

        internal Murmur128ManagedX86(uint seed = 0)
            : base(seed: seed)
        {
        }

        private int Length { get; set; }

        public override void Initialize()
        {
            // initialize hash values to seed values
            h1 = h2 = h3 = h4 = Seed;
        }

        protected override void HashCore(byte[] array, int ibStart, int cbSize)
        {
            // store the length of the hash (for use later)
            Length = cbSize;

            // only compute the hash if we have data to hash
            if (Length > 0)
            {
                // calculate how many 16 byte segments we have
                var count = (Length / 16);
                var remainder = (Length & 15);

                Body(array, count, remainder);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Body(byte[] data, int count, int remainder)
        {
            int offset = 0;

            // grab reference to the end of our data as uint blocks
            while (count-- > 0)
            {
                // get our values
                uint k1 = BitConverter.ToUInt32(data, offset),
                     k2 = BitConverter.ToUInt32(data, offset + 4),
                     k3 = BitConverter.ToUInt32(data, offset + 8),
                     k4 = BitConverter.ToUInt32(data, offset + 12);

                offset += 16;

                h1 = h1 ^ ((k1 * c1).RotateLeft(15) * c2);
                h1 = (h1.RotateLeft(19) + h2) * 5 + 0x561ccd1b;

                h2 = h2 ^ ((k2 * c2).RotateLeft(16) * c3);
                h2 = (h2.RotateLeft(17) + h3) * 5 + 0x0bcaa747;

                h3 = h3 ^ ((k3 * c3).RotateLeft(17) * c4);
                h3 = (h3.RotateLeft(15) + h4) * 5 + 0x96cd1c35;

                h4 = h4 ^ ((k4 * c4).RotateLeft(18) * c1);
                h4 = (h4.RotateLeft(13) + h1) * 5 + 0x32ac3b17;
            }

            if (remainder > 0)
                Tail(data, offset, remainder);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Tail(byte[] tail, int position, int remainder)
        {
            // create our keys and initialize to 0
            uint k1 = 0, k2 = 0, k3 = 0, k4 = 0;

            // determine how many bytes we have left to work with based on length
            switch (remainder)
            {
                case 15: k4 ^= (uint)tail[position + 14] << 16; goto case 14;
                case 14: k4 ^= (uint)tail[position + 13] << 8; goto case 13;
                case 13: k4 ^= (uint)tail[position + 12] << 0; goto case 12;
                case 12: k3 ^= (uint)tail[position + 11] << 24; goto case 11;
                case 11: k3 ^= (uint)tail[position + 10] << 16; goto case 10;
                case 10: k3 ^= (uint)tail[position + 9] << 8; goto case 9;
                case 9: k3 ^= (uint)tail[position + 8] << 0; goto case 8;
                case 8: k2 ^= (uint)tail[position + 7] << 24; goto case 7;
                case 7: k2 ^= (uint)tail[position + 6] << 16; goto case 6;
                case 6: k2 ^= (uint)tail[position + 5] << 8; goto case 5;
                case 5: k2 ^= (uint)tail[position + 4] << 0; goto case 4;
                case 4: k1 ^= (uint)tail[position + 3] << 24; goto case 3;
                case 3: k1 ^= (uint)tail[position + 2] << 16; goto case 2;
                case 2: k1 ^= (uint)tail[position + 1] << 8; goto case 1;
                case 1: k1 ^= (uint)tail[position] << 0; break;
            }

            h4 = h4 ^ ((k4 * c4).RotateLeft(18) * c1);
            h3 = h3 ^ ((k3 * c3).RotateLeft(17) * c4);
            h2 = h2 ^ ((k2 * c2).RotateLeft(16) * c3);
            h1 = h1 ^ ((k1 * c1).RotateLeft(15) * c2);
        }

        protected override byte[] HashFinal()
        {
            uint len = (uint)Length;
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
            Array.Copy(BitConverter.GetBytes(h1), 0, result, 0, 4);
            Array.Copy(BitConverter.GetBytes(h2), 0, result, 4, 4);
            Array.Copy(BitConverter.GetBytes(h3), 0, result, 8, 4);
            Array.Copy(BitConverter.GetBytes(h4), 0, result, 12, 4);

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint fmix(uint h)
        {
            // pipelining friendly algorithm
            h = (h ^ (h >> 16)) * 0x85ebca6b;
            h = (h ^ (h >> 13)) * 0xc2b2ae35;
            h ^= h >> 16;

            return h;
        }
    }
}
