using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Murmur
{
    public abstract class Murmur32 : HashAlgorithm
    {
        protected const uint c1 = 0xcc9e2d51;
        protected const uint c2 = 0x1b873593;

        protected readonly uint Seed;
        protected uint h1;

        protected Murmur32(uint seed = 0)
        {
            Seed = 0;
        }

        protected int Length { get; set; }

        public override int HashSize { get { return 32; } }

        public override void Initialize()
        {
            // Initialize our base value to the seed
            h1 = Seed;
        }

        protected override void HashCore(byte[] array, int ibStart, int cbSize)
        {
            Length = cbSize;
            if (Length > 0)
            {
                var blocks = (Length / 4);
                var remainder = (Length & 3);

                HashCore(array, ibStart, blocks, remainder);
            }
        }

        protected override byte[] HashFinal()
        {
            h1 = (h1 ^ (uint)Length).FMix();

            var result = new byte[4];
            Array.Copy(BitConverter.GetBytes(h1), result, 4);

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract void HashCore(byte[] data, int offset, int blocks, int remainder);
    }
}
