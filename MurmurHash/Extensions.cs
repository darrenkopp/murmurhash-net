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
            return (x << r | x >> (32 - r));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ulong RotateLeft(this ulong x, byte r)
        {
            return (x << r | x >> (64 - r));
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
    }
}
