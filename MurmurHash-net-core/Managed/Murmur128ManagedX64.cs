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
    internal class Murmur128ManagedX64 : Murmur128
    {
        const ulong C1 = 0x87c37b91114253d5;
        const ulong C2 = 0x4cf5ad432745937f;

        internal Murmur128ManagedX64(uint seed = 0)
            : base(seed)
        {
            Reset();
        }

        private int Length { get; set; }
        private ulong H1 { get; set; }
        private ulong H2 { get; set; }

        private void Reset()
        {
            // initialize hash values to seed values
            H1 = H2 = Seed;
            // reset our length back to 0
            Length = 0;
        }

        public override void Initialize()
        {
            Reset();
        }

        protected override void HashCore(byte[] array, int ibStart, int cbSize)
        {
            // increment our length
            Length += cbSize;
            Body(array, ibStart, cbSize);
        }

#if NETFX45
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private void Body(byte[] data, int start, int length)
        {
            int remainder = length & 15;
            int alignedLength = start + (length - remainder);
            for (int i = start; i < alignedLength; i += 16)
            {
                H1 ^= (data.ToUInt64(i) * C1).RotateLeft(31) * C2;
                H1 = (H1.RotateLeft(27) + H2) * 5 + 0x52dce729;

                H2 ^= (data.ToUInt64(i + 8) * C2).RotateLeft(33) * C1;
                H2 = (H2.RotateLeft(31) + H1) * 5 + 0x38495ab5;
            }

            if (remainder > 0)
                Tail(data, alignedLength, remainder);
        }

#if NETFX45
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private void Tail(byte[] tail, int start, int remaining)
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

            H2 ^= (k2 * C2).RotateLeft(33) * C1;
            H1 ^= (k1 * C1).RotateLeft(31) * C2;
        }

        protected override byte[] HashFinal()
        {
            ulong len = (ulong)Length;
            H1 ^= len; H2 ^= len;

            H1 += H2;
            H2 += H1;

            H1 = H1.FMix();
            H2 = H2.FMix();

            H1 += H2;
            H2 += H1;

            var result = new byte[16];
            Array.Copy(BitConverter.GetBytes(H1), 0, result, 0, 8);
            Array.Copy(BitConverter.GetBytes(H2), 0, result, 8, 8);

            return result;
        }
    }
}
