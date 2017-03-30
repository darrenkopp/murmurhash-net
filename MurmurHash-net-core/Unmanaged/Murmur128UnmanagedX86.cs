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
        const uint C1 = 0x239b961b;
        const uint C2 = 0xab0e9789;
        const uint C3 = 0x38b34ae5;
        const uint C4 = 0xa1e38b93;
        internal Murmur128UnmanagedX86(uint seed = 0)
            : base(seed)
        {
            Reset();
        }

        private uint H1 { get; set; }
        private uint H2 { get; set; }
        private uint H3 { get; set; }
        private uint H4 { get; set; }
        private int Length { get; set; }

        private void Reset()
        {
            // initialize hash values to seed values
            H1 = H2 = H3 = H4 = Seed;
            Length = 0;
        }

        public override void Initialize()
        {
            Reset();
        }

        protected override void HashCore(byte[] array, int ibStart, int cbSize)
        {
            // store the length of the hash (for use later)
            Length += cbSize;
            Body(array, ibStart, cbSize);
        }

#if NETFX45
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private void Body(byte[] data, int start, int length)
        {
            int remainder = length & 15;
            int blocks = length / 16;

            unsafe
            {
                fixed (byte* d = &data[start])
                {
                    // grab a reference to blocks
                    uint* b = (uint*)d;
                    while (blocks-- > 0)
                    {
                        // K1 - consume first integer
                        H1 ^= (*b++ * C1).RotateLeft(15) * C2;
                        H1 = (H1.RotateLeft(19) + H2) * 5 + 0x561ccd1b;

                        // K2 - consume second integer
                        H2 ^= (*b++ * C2).RotateLeft(16) * C3;
                        H2 = (H2.RotateLeft(17) + H3) * 5 + 0x0bcaa747;

                        // K3 - consume third integer
                        H3 ^= (*b++ * C3).RotateLeft(17) * C4;
                        H3 = (H3.RotateLeft(15) + H4) * 5 + 0x96cd1c35;

                        // K4 - consume fourth integer
                        H4 ^= (*b++ * C4).RotateLeft(18) * C1;
                        H4 = (H4.RotateLeft(13) + H1) * 5 + 0x32ac3b17;
                    }

                    if (remainder > 0)
                        Tail(d + (length - remainder), remainder);
                }
            }
        }

#if NETFX45
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        unsafe private void Tail(byte* tail, int remainder)
        {
            // create our keys and initialize to 0
            uint k1 = 0, k2 = 0, k3 = 0, k4 = 0;

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

            H4 ^= (k4 * C4).RotateLeft(18) * C1;
            H3 ^= (k3 * C3).RotateLeft(17) * C4;
            H2 ^= (k2 * C2).RotateLeft(16) * C3;
            H1 ^= (k1 * C1).RotateLeft(15) * C2;
        }

        protected override byte[] HashFinal()
        {
            uint len = (uint)Length;
            // pipelining friendly algorithm
            H1 ^= len; H2 ^= len; H3 ^= len; H4 ^= len;

            H1 += (H2 + H3 + H4);
            H2 += H1; H3 += H1; H4 += H1;

            H1 = H1.FMix();
            H2 = H2.FMix();
            H3 = H3.FMix();
            H4 = H4.FMix();

            H1 += (H2 + H3 + H4);
            H2 += H1; H3 += H1; H4 += H1;

            var result = new byte[16];
            unsafe
            {
                fixed (byte* h = result)
                {
                    var r = (uint*)h;

                    r[0] = H1;
                    r[1] = H2;
                    r[2] = H3;
                    r[3] = H4;
                }
            }

            return result;
        }
    }
}
