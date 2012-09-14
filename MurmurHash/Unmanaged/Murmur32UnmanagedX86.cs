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
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Murmur
{
    internal class Murmur32UnmanagedX86 : Murmur32
    {
        public Murmur32UnmanagedX86(uint seed = 0) : base(seed: seed) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void HashCore(byte[] data, int offset, int blocks, int remainder)
        {
            unsafe
            {
                // grab pointer to first byte in array
                fixed (byte* d = data)
                {
                    uint* b = (uint*)d;
                    while (blocks-- > 0)
                    {
                        // grab our 4 byte key segments, stepping our offset position back each time
                        // thus we are walking our array backwards
                        h1 = ((h1 ^ (((*b++ * c1).RotateLeft(15)) * c2)).RotateLeft(13)) * 5 + 0xe6546b64;
                    }

                    if (remainder > 0)
                        Tail(d + offset, remainder);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe private void Tail(byte* tail, int remainder)
        {
            // create our keys and initialize to 0
            uint k1 = 0;

            // determine how many bytes we have left to work with based on length
            switch (remainder)
            {
                case 3: k1 ^= (uint)tail[2] << 16; goto case 2;
                case 2: k1 ^= (uint)tail[1] << 8; goto case 1;
                case 1: k1 ^= (uint)tail[0]; break;
            }

            h1 = h1 ^ ((k1 * c1).RotateLeft(15) * c2);
        }
    }
}
