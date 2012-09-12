using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
                var blockCount = (Length / 16);
                unsafe
                {
                    // grab pointer to first byte in array
                    fixed (byte* data = &array[0])
                    {
                        Body(data, blockCount);
                        Tail(data);
                    }
                }
            }
        }

        unsafe private void Body(byte* data, int blockCount)
        {
            int remaining = blockCount;
            // calculate our block-aligned starting offset
            int offset = blockCount * 4;

            // grab reference to the end of our data as uint blocks
            uint* blocks = (uint*)(data + offset);
            while (remaining > 0)
            {
                // decrement our remaining block count
                remaining--;

                // grab our 4 byte key segments, stepping our offset position back each time
                // thus we are walking our array backwards
                uint k1 = blocks[offset--],
                     k2 = blocks[offset--],
                     k3 = blocks[offset--],
                     k4 = blocks[offset--];

                k1 *= c1; k1 = (k1 << 15 | k1 >> 17); k1 *= c2; h1 ^= k1;

                h1 = (h1 << 19 | h1 >> 13); h1 += h2; h1 = h1 * 5 + 0x561ccd1b;

                k2 *= c2; k2 = (k2 << 16 | k2 >> 16); k2 *= c3; h2 ^= k2;

                h2 = (h2 << 17 | h2 >> 15); h2 += h3; h2 = h2 * 5 + 0x0bcaa747;

                k3 *= c3; k3 = (k3 << 17 | k3 >> 15); k3 *= c4; h3 ^= k3;

                h3 = (h3 << 15 | h3 >> 17); h3 += h4; h3 = h3 * 5 + 0x96cd1c35;

                k4 *= c4; k4 = (k4 << 18 | k4 >> 14); k4 *= c1; h4 ^= k4;

                h4 = (h4 << 13 | h4 >> 19); h4 += h1; h4 = h4 * 5 + 0x32ac3b17;
            }
        }

        unsafe private void Tail(byte* tail)
        {
            // create our keys and initialize to 0
            uint k1 = 0, k2 = 0, k3 = 0, k4 = 0;

            // determine how many bytes we have left to work with based on length
            switch (Length & 15)
            {
                case 15:
                    k4 ^= (uint)tail[14] << 16;
                    goto case 14;
                case 14:
                    k4 ^= (uint)tail[13] << 8;
                    goto case 13;
                case 13:
                    k4 ^= (uint)tail[12] << 0;
                    k4 *= c4; k4 = (k4 << 18 | k4 >> 14); k4 *= c1; h4 ^= k4;
                    goto case 12;
                case 12:
                    k3 ^= (uint)tail[11] << 24;
                    goto case 11;
                case 11:
                    k3 ^= (uint)tail[10] << 16;
                    goto case 10;
                case 10:
                    k3 ^= (uint)tail[9] << 8;
                    goto case 9;
                case 9:
                    k3 ^= (uint)tail[8] << 0;
                    k3 *= c3; k3 = (k3 << 17 | k3 >> 15); k3 *= c4; h3 ^= k3;
                    goto case 8;
                case 8:
                    k2 ^= (uint)tail[7] << 24;
                    goto case 7;
                case 7:
                    k2 ^= (uint)tail[6] << 16;
                    goto case 6;
                case 6:
                    k2 ^= (uint)tail[5] << 8;
                    goto case 5;
                case 5:
                    k2 ^= (uint)tail[4] << 0;
                    k2 *= c2; k2 = (k2 << 16 | k2 >> 16); k2 *= c3; h2 ^= k2;
                    goto case 4;
                case 4:
                    k1 ^= (uint)tail[3] << 24;
                    goto case 3;
                case 3:
                    k1 ^= (uint)tail[2] << 16;
                    goto case 2;
                case 2:
                    k1 ^= (uint)tail[1] << 8;
                    goto case 1;
                case 1:
                    k1 ^= (uint)tail[0] << 0;
                    k1 *= c1; k1 = (k1 << 15 | k1 >> 17); k1 *= c2; h1 ^= k1;
                    return;
                default: return;
            }
        }

        protected override byte[] HashFinal()
        {
            uint len = (uint)Length;
            h1 ^= len; h2 ^= len; h3 ^= len; h4 ^= len;

            h1 += h2; h1 += h3; h1 += h4;
            h2 += h1; h3 += h1; h4 += h1;

            h1 = fmix(h1);
            h2 = fmix(h2);
            h3 = fmix(h3);
            h4 = fmix(h4);

            h1 += h2; h1 += h3; h1 += h4;
            h2 += h1; h3 += h1; h4 += h1;

            // eh? do i initialize this... or what...
            var result = new byte[16];
            unsafe
            {
                fixed (byte* h = result)
                {
                    uint* r = (uint*)h;

                    r[0] = h1;
                    r[1] = h2;
                    r[2] = h3;
                    r[3] = h4;
                }
            }

            return result;
        }

        private static uint fmix(uint h)
        {
            h ^= h >> 16;
            h *= 0x85ebca6b;
            h ^= h >> 13;
            h *= 0xc2b2ae35;
            h ^= h >> 16;

            return h;
        }
    }
}
