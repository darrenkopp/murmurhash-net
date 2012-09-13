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
    }
}
