using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Murmur.Specs
{
    class HashExpection
    {
        readonly uint _Result;
        readonly int _Bits;

        public HashExpection(int bits, uint result)
        {
            _Bits = bits;
            _Result = result;
        }

        public uint Result { get { return _Result; } }
        public int Bits { get { return _Bits; } }
    }
}
