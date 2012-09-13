using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
                            Tail(data + Length, remainder);
                    }
                }
            }
        }

        unsafe private void Body(byte* data, int count)
        {
            // grab reference to the end of our data as uint blocks
            uint* blocks = (uint*)(data + Length);
            while (count-- > 0)
            {
                // grab our 4 byte key segments, stepping our offset position back each time
                // thus we are walking our array backwards
                uint k1 = *--blocks,
                     k2 = *--blocks,
                     k3 = *--blocks,
                     k4 = *--blocks;

                // original algorithm
                //k1 *= c1; k1 = (k1 << 15 | k1 >> 17); k1 *= c2; h1 ^= k1;
                //h1 = (h1 << 19 | h1 >> 13); h1 += h2; h1 = h1 * 5 + 0x561ccd1b;
                //k2 *= c2; k2 = (k2 << 16 | k2 >> 16); k2 *= c3; h2 ^= k2;
                //h2 = (h2 << 17 | h2 >> 15); h2 += h3; h2 = h2 * 5 + 0x0bcaa747;
                //k3 *= c3; k3 = (k3 << 17 | k3 >> 15); k3 *= c4; h3 ^= k3;
                //h3 = (h3 << 15 | h3 >> 17); h3 += h4; h3 = h3 * 5 + 0x96cd1c35;
                //k4 *= c4; k4 = (k4 << 18 | k4 >> 14); k4 *= c1; h4 ^= k4;
                //h4 = (h4 << 13 | h4 >> 19); h4 += h1; h4 = h4 * 5 + 0x32ac3b17;

                // pipelining friendly algorithm
                h1 = h1 ^ (((k1 * c1) << 15 | (k1 * c1) >> 17) * c2);
                h1 = ((h1 << 19 | h1 >> 13) + h2) * 5 + 0x561ccd1b;
                h2 = h2 ^ (((k2 * c2) << 16 | (k2 * c2) >> 16) * c3);
                h2 = ((h2 << 17 | h2 >> 15) + h3) * 5 + 0x0bcaa747;
                h3 = h3 ^ (((k3 * c3) << 17 | (k3 * c3) >> 15) * c4);
                h3 = ((h3 << 15 | h3 >> 17) + h4) * 5 + 0x96cd1c35;
                h4 = h4 ^ (((k4 * c4) << 18 | (k4 * c4) >> 14) * c1);
                h4 = ((h4 << 13 | h4 >> 19) + h1) * 5 + 0x32ac3b17;
            }
        }

        unsafe private void Tail(byte* tail, int remainder)
        {
            // create our keys and initialize to 0
            uint k1 = 0, k2 = 0, k3 = 0, k4 = 0;

            // determine how many bytes we have left to work with based on length
            switch (remainder)
            {
                case 15:
                    k4 ^= (uint)*tail-- << 16;
                    goto case 14;
                case 14:
                    k4 ^= (uint)*tail-- << 8;
                    goto case 13;
                case 13:
                    k4 ^= (uint)*tail-- << 0;
                    h4 = (h4 ^ ((k4 * c4) << 18 | (k4 * c4) >> 14)) * c1;
                    //k4 *= c4; k4 = (k4 << 18 | k4 >> 14); k4 *= c1; h4 ^= k4;
                    goto case 12;
                case 12:
                    k3 ^= (uint)*tail-- << 24;
                    goto case 11;
                case 11:
                    k3 ^= (uint)*tail-- << 16;
                    goto case 10;
                case 10:
                    k3 ^= (uint)*tail-- << 8;
                    goto case 9;
                case 9:
                    k3 ^= (uint)*tail-- << 0;
                    h3 = (h3 ^ ((k3 * c3) << 17 | (k3 * c3) >> 15)) * c4;
                    //k3 *= c3; k3 = (k3 << 17 | k3 >> 15); k3 *= c4; h3 ^= k3;
                    goto case 8;
                case 8:
                    k2 ^= (uint)*tail-- << 24;
                    goto case 7;
                case 7:
                    k2 ^= (uint)*tail-- << 16;
                    goto case 6;
                case 6:
                    k2 ^= (uint)*tail-- << 8;
                    goto case 5;
                case 5:
                    k2 ^= (uint)*tail-- << 0;
                    h2 = (h2 ^ ((k2 * c2) << 16 | (k2 * c2) >> 16)) * c3;
                    //k2 *= c2; k2 = (k2 << 16 | k2 >> 16); k2 *= c3; h2 ^= k2;
                    goto case 4;
                case 4:
                    k1 ^= (uint)*tail-- << 24;
                    goto case 3;
                case 3:
                    k1 ^= (uint)*tail-- << 16;
                    goto case 2;
                case 2:
                    k1 ^= (uint)*tail-- << 8;
                    goto case 1;
                case 1:
                    k1 ^= (uint)*tail-- << 0;
                    h1 = (h1 ^ ((k1 * c1) << 15 | (k1 * c1) >> 17)) * c2;
                    //k1 *= c1; k1 = (k1 << 15 | k1 >> 17); k1 *= c2; h1 ^= k1;
                    break;
            }
        }

        protected override byte[] HashFinal()
        {
            uint len = (uint)Length;

            // original algorithm
            //h1 ^= len; h2 ^= len; h3 ^= len; h4 ^= len;

            //h1 += h2; h1 += h3; h1 += h4;
            //h2 += h1; h3 += h1; h4 += h1;

            //h1 = fmix(h1);
            //h2 = fmix(h2);
            //h3 = fmix(h3);
            //h4 = fmix(h4);

            //h1 += h2; h1 += h3; h1 += h4;
            //h2 += h1; h3 += h1; h4 += h1;

            // pipelining friendly algorithm
            h1 ^= len; h2 ^= len; h3 ^= len; h4 ^= len;

            h1 += (h2 + h3 + h4);
            h2 += h1; h3 += h1; h4 += h1;

            h1 = fmix(h1);
            h2 = fmix(h2);
            h3 = fmix(h3);
            h4 = fmix(h4);

            h1 += (h2 + h3 + h4);

            var result = new byte[16];
            unsafe
            {
                fixed (byte* h = result)
                {
                    var r = (uint*)h;

                    *r++ = h1;
                    *r++ = h2 + h1;
                    *r++ = h3 + h1;
                    *r = h4 + h1;
                }
            }

            return result;
        }


        private static uint fmix(uint h)
        {
            // original algorithm
            //h ^= h >> 16;
            //h *= 0x85ebca6b;
            //h ^= h >> 13;
            //h *= 0xc2b2ae35;
            //h ^= h >> 16;

            // pipelining friendly algorithm
            h = (h ^ (h >> 16)) * 0x85ebca6b;
            h = (h ^ (h >> 13)) * 0xc2b2ae35;
            h ^= h >> 16;

            return h;
        }
    }
}
