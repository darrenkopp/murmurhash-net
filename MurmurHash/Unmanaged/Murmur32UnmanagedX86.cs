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
    internal class Murmur32UnmanagedX86 : Murmur32
    {
        public Murmur32UnmanagedX86(uint seed = 0)
            : base(seed)
        {
        }

        protected override void HashCore(byte[] array, int ibStart, int cbSize)
        {
            Length += cbSize;
            Body(array, ibStart, cbSize);
        }

#if NETFX45
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private void Body(byte[] data, int start, int length)
        {
            int remainder = length & 3;
            int blocks = length / 4;

            unsafe
            {
                // grab pointer to first byte in array
                fixed (byte* d = &data[start])
                {
                    uint* b = (uint*)d;

                    while (blocks-- > 0)
                        H1 = (((H1 ^ (((*b++ * C1).RotateLeft(15)) * C2)).RotateLeft(13)) * 5) + 0xe6546b64;

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
            uint k1 = 0;

            // determine how many bytes we have left to work with based on length
            switch (remainder)
            {
                case 3: k1 ^= (uint)tail[2] << 16; goto case 2;
                case 2: k1 ^= (uint)tail[1] << 8; goto case 1;
                case 1: k1 ^= tail[0]; break;
            }

            H1 ^= (k1 * C1).RotateLeft(15) * C2;
        }
    }
}
