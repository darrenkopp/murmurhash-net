using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Murmur
{
    public static class MurmurHash
    {
        public static HashAlgorithm Create32(uint seed = 0)
        {
            return null;
        }

        public static Murmur128 Create128(uint seed = 0, bool managed = true)
        {
            var algorithm = managed
                ? Pick(seed, s => new Murmur128ManagedX86(s), s => new Murmur128ManagedX64(s))
                : Pick(seed, s => new Murmur128UnmanagedX86(s), s => new Murmur128UnmanagedX64(s));

            return algorithm as Murmur128;
        }

        private static HashAlgorithm Pick<T32, T64>(uint seed, Func<uint, T32> factory32, Func<uint, T64> factory64)
            where T32 : HashAlgorithm
            where T64 : HashAlgorithm
        {
            if (Environment.Is64BitProcess)
                return factory64(seed);

            return factory32(seed);
        }
    }
}
