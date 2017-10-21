using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Murmur.Specs
{
    static class HashVerifier
    {
        public static uint ComputeVerificationHash(int bits, Func<uint, HashAlgorithm> algorithmFactory)
        {
            int bytes = bits / 8;

            byte[] key = new byte[256];
            byte[] hashes = new byte[bytes * 256];

            for (int i = 0; i < 256; i++)
            {
                key[i] = (byte)i;
                using (var algorithm = algorithmFactory((uint)(256 - i)))
                    Array.Copy(algorithm.ComputeHash(key, 0, i), 0, hashes, i * bytes, bytes);
            }

            using (var algorithm = algorithmFactory(0))
                return BitConverter.ToUInt32(algorithm.ComputeHash(hashes), 0);
        }

        public static uint ComputeVerificationHashOutputStream(int bits, Func<Stream, uint, MurmurOutputStream> streamFactory)
        {
            int bytes = bits / 8;

            byte[] key = new byte[256];
            byte[] hashes = new byte[bytes * 256];

            for (int i = 0; i < 256; i++)
            {
                key[i] = (byte)i;
                using (var algorithm = streamFactory(Stream.Null, (uint)(256 - i)))
                {
                    algorithm.Write(key, 0, i);

                    Array.Copy(algorithm.Hash, 0, hashes, i * bytes, bytes);
                }
            }

            using (var algorithm = streamFactory(Stream.Null, 0))
            {
                algorithm.Write(hashes, 0, hashes.Length);
                return BitConverter.ToUInt32(algorithm.Hash, 0);
            }
        }

        public static uint ComputeVerificationHashInputStream(int bits, Func<Stream, uint, MurmurInputStream> streamFactory)
        {
            int bytes = bits / 8;

            byte[] key = new byte[256];
            byte[] hashes = new byte[bytes * 256];

            for (int i = 0; i < 256; i++)
            {
                key[i] = (byte)i;
                using (var memory = new MemoryStream())
                using (var source = streamFactory(memory, (uint)(256 - i)))
                {
                    memory.Write(key, 0, i);
                    memory.Position = 0L;

                    var buffer = new byte[i];
                    source.Read(buffer, 0, i);

                    Array.Copy(source.Hash, 0, hashes, i * bytes, bytes);
                }
            }

            using (var memory = new MemoryStream())
            using (var source = streamFactory(memory, 0))
            {
                memory.Write(hashes, 0, hashes.Length);
                memory.Position = 0L;

                var buffer = new byte[hashes.Length];
                source.Read(hashes, 0, hashes.Length);

                return BitConverter.ToUInt32(source.Hash, 0);
            }
        }
    }
}
