using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Murmur
{
    internal class Murmur32ManagedX86 : Murmur32
    {
        public Murmur32ManagedX86(uint seed = 0) : base(seed: seed) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void HashCore(byte[] data, int offset, int blocks, int remainder)
        {
            // grab reference to the end of our data as uint blocks
            while (blocks-- > 0)
            {
                // get our values
                uint k1 = BitConverter.ToUInt32(data, offset); offset += 4;

                h1 = ((h1 ^ (((k1 * c1).RotateLeft(15)) * c2)).RotateLeft(13)) * 5 + 0xe6546b64;
            }

            if (remainder > 0)
                Tail(data, offset, remainder);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Tail(byte[] tail, int position, int remainder)
        {
            // create our keys and initialize to 0
            uint k1 = 0;

            // determine how many bytes we have left to work with based on length
            switch (remainder)
            {
                case 3: k1 ^= (uint)tail[position + 2] << 16; goto case 2;
                case 2: k1 ^= (uint)tail[position + 1] << 8; goto case 1;
                case 1: k1 ^= (uint)tail[position]; break;
            }

            h1 = h1 ^ ((k1 * c1).RotateLeft(15) * c2);
        }
    }
}
