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
    internal class Murmur128UnmanagedX64 : Murmur128
    {
        const ulong c1 = 0x87c37b91114253d5L;
        const ulong c2 = 0x4cf5ad432745937fL;

        private ulong h1;
        private ulong h2;

        internal Murmur128UnmanagedX64(uint seed = 0)
            : base(seed: seed)
        {
        }

        private int Length { get; set; }

        public override void Initialize()
        {
            // initialize hash values to seed values
            h1 = h2 = Seed;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void HashCore(byte[] array, int ibStart, int cbSize)
        {
            // store the length of the hash (for use later)
            Length = cbSize;

            // only compute the hash if we have data to hash
            if (Length > 0)
            {
                unsafe
                {
                    // grab pointer to first byte in array
                    fixed (byte* data = array)
                        Body(data);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe private void Body(byte* data)
        {
            int remaining = Length;
            int position = 0;
            ulong* current = (ulong*)data;

            while (remaining >= 16)
            {
                ulong k1 = *current++, k2 = *current++;
                remaining -= 16; position += 16;

                // a variant of original algorithm optimized for processor instruction pipelining
                h1 = h1 ^ ((k1 * c1).RotateLeft(31) * c2);
                h1 = (h1.RotateLeft(27) + h2) * 5 + 0x52dce729;

                h2 = h2 ^ ((k2 * c2).RotateLeft(33) * c1);
                h2 = (h2.RotateLeft(31) + h1) * 5 + 0x38495ab5;
            }

            if (remaining > 0)
                Tail(data, position, remaining);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe private void Tail(byte* tail, int start, int remaining)
        {
            // create our keys and initialize to 0
            ulong k1 = 0, k2 = 0;

            // determine how many bytes we have left to work with based on length
            switch (remaining)
            {
                case 15: k2 ^= (ulong)tail[start + 14] << 48; goto case 14;
                case 14: k2 ^= (ulong)tail[start + 13] << 40; goto case 13;
                case 13: k2 ^= (ulong)tail[start + 12] << 32; goto case 12;
                case 12: k2 ^= (ulong)tail[start + 11] << 24; goto case 11;
                case 11: k2 ^= (ulong)tail[start + 10] << 16; goto case 10;
                case 10: k2 ^= (ulong)tail[start + 9] << 8; goto case 9;
                case 9: k2 ^= (ulong)tail[start + 8] << 0; goto case 8;
                case 8: k1 ^= (ulong)tail[start + 7] << 56; goto case 7;
                case 7: k1 ^= (ulong)tail[start + 6] << 48; goto case 6;
                case 6: k1 ^= (ulong)tail[start + 5] << 40; goto case 5;
                case 5: k1 ^= (ulong)tail[start + 4] << 32; goto case 4;
                case 4: k1 ^= (ulong)tail[start + 3] << 24; goto case 3;
                case 3: k1 ^= (ulong)tail[start + 2] << 16; goto case 2;
                case 2: k1 ^= (ulong)tail[start + 1] << 8; goto case 1;
                case 1: k1 ^= (ulong)tail[start] << 0; break;
            }

            h2 = h2 ^ ((k2 * c2).RotateLeft(33) * c1);
            h1 = h1 ^ ((k1 * c1).RotateLeft(31) * c2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override byte[] HashFinal()
        {
            ulong len = (ulong)Length;
            h1 ^= len; h2 ^= len;

            h1 += h2;
            h2 += h1;

            h1 = h1.FMix();
            h2 = h2.FMix();

            h1 += h2;
            h2 += h1;

            var result = new byte[16];
            unsafe
            {
                fixed (byte* h = result)
                {
                    ulong* r = (ulong*)h;

                    r[0] = h1;
                    r[1] = h2;
                }
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong fmix(ulong k)
        {
            k = (k ^ (k >> 33)) * 0xff51afd7ed558ccdL;
            k = (k ^ (k >> 33)) * 0xc4ceb9fe1a85ec53L;
            k ^= k >> 33;

            return k;
        }
    }
}
