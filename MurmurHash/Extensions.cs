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
    internal static class Extensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static uint RotateLeft(this uint x, byte r)
        {
            return (x << r) | (x >> (32 - r));
            //return (x << r | x >> (32 - (r & 31)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ulong RotateLeft(this ulong x, byte r)
        {
            return (x << r) | (x >> (64 - r));
            //return (x << r | x >> (64 - (r & 63)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static uint FMix(this uint h)
        {
            // pipelining friendly algorithm
            h = (h ^ (h >> 16)) * 0x85ebca6b;
            h = (h ^ (h >> 13)) * 0xc2b2ae35;
            h ^= h >> 16;

            return h;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ulong FMix(this ulong h)
        {
            // pipelining friendly algorithm
            h = (h ^ (h >> 33)) * 0xff51afd7ed558ccd;
            h = (h ^ (h >> 33)) * 0xc4ceb9fe1a85ec53;
            h ^= h >> 33;

            return h;
        }
    }
}
