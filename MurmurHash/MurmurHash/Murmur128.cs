using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Murmur
{
    public abstract class Murmur128 : HashAlgorithm
    {
        protected readonly uint Seed;
        protected Murmur128(uint seed)
        {
            Seed = seed;
        }

        public override int HashSize { get { return 128; } }
    }
}
