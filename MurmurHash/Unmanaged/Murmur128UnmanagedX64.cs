using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
                        Tail(data + (blockCount * 16));
                    }
                }
            }
        }

        unsafe private void Body(byte* data, int blockCount)
        {
            int offset = 0;
            ulong* blocks = (ulong*)(data);
            for (int i = 0; i < blockCount; i++)
            {
                ulong k1 = blocks[offset++], k2 = blocks[offset++];

                k1 *= c1; k1 = (k1 << 31 | k1 >> 33); k1 *= c2; h1 ^= k1;

                h1 = (h1 << 27 | h1 >> 37); h1 += h2; h1 = h1 * 5 + 0x52dce729;

                k2 *= c2; k2 = (k2 << 33 | k2 >> 31); k2 *= c1; h2 ^= k2;

                h2 = (h2 << 31 | h2 >> 33); h2 += h1; h2 = h2 * 5 + 0x38495ab5;
            }
        }

        unsafe private void Tail(byte* tail)
        {
            // create our keys and initialize to 0
            ulong k1 = 0, k2 = 0;

            // determine how many bytes we have left to work with based on length
            switch (Length & 15)
            {
                case 15:
                    k2 ^= (ulong)tail[14] << 48;
                    goto case 14;
                case 14:
                    k2 ^= (ulong)tail[13] << 40;
                    goto case 13;
                case 13:
                    k2 ^= (ulong)tail[12] << 32;
                    goto case 12;
                case 12:
                    k2 ^= (ulong)tail[11] << 24;
                    goto case 11;
                case 11:
                    k2 ^= (ulong)tail[10] << 16;
                    goto case 10;
                case 10:
                    k2 ^= (ulong)tail[9] << 8;
                    goto case 9;
                case 9:
                    k2 ^= (ulong)tail[8] << 0;
                    k2 *= c2; k2  = (k2 << 33 | k2 >> 31); k2 *= c1; h2 ^= k2;
                    goto case 8;
                case 8:
                    k1 ^= (ulong)tail[7] << 56;
                    goto case 7;
                case 7:
                    k1 ^= (ulong)tail[6] << 48;
                    goto case 6;
                case 6:
                    k1 ^= (ulong)tail[5] << 40;
                    goto case 5;
                case 5:
                    k1 ^= (ulong)tail[4] << 32;
                    goto case 4;
                case 4:
                    k1 ^= (ulong)tail[3] << 24;
                    goto case 3;
                case 3:
                    k1 ^= (ulong)tail[2] << 16;
                    goto case 2;
                case 2:
                    k1 ^= (ulong)tail[1] << 8;
                    goto case 1;
                case 1:
                    k1 ^= (ulong)tail[0] << 0;
                    k1 *= c1; k1  = (k1 << 31 | k1 >> 33); k1 *= c2; h1 ^= k1;
                    return;
                default: return;
            }
        }

        protected override byte[] HashFinal()
        {
            ulong len = (ulong)Length;
            h1 ^= len; h2 ^= len;

            h1 += h2;
            h2 += h1;

            h1 = fmix(h1);
            h2 = fmix(h2);

            h1 += h2;
            h2 += h1;

            // eh? do i initialize this... or what...
            var result = new byte[16];
            Array.Copy(BitConverter.GetBytes(h1), 0, result, 0, 8);
            Array.Copy(BitConverter.GetBytes(h2), 0, result, 8, 8);
            //unsafe
            //{
            //    fixed (byte* h = result)
            //    {
            //        ulong* r = (ulong*)h;

            //        r[0] = h1;
            //        r[1] = h2;
            //    }
            //}

            return result;
        }

        private static ulong fmix(ulong k)
        {
            k ^= k >> 33;
            k *= 0xff51afd7ed558ccdL;
            k ^= k >> 33;
            k *= 0xc4ceb9fe1a85ec53L;
            k ^= k >> 33;

            return k;
        }
    }
}
