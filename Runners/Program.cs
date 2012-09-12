using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Murmur;
using System.Security.Cryptography;

namespace MurmurRunner
{
    class Program
    {
        static readonly HashAlgorithm Unmanaged = MurmurHash.Create128(managed: false);
        static readonly HashAlgorithm Managed = MurmurHash.Create128(managed: true);
        static readonly HashAlgorithm Sha1 = SHA1Managed.Create();
        static readonly HashAlgorithm Md5 = MD5.Create();

        static readonly byte[] Data = GenerateRandomData();

        static void Main(string[] args)
        {
            Console.WriteLine("Running {0} comparison", Environment.Is64BitProcess ? "x64" : "x86");
            Console.WriteLine(); Console.WriteLine();

            Run(steps: new Dictionary<string, HashAlgorithm> {
                { "Murmur 128 Unmanaged", Unmanaged },
                { "Murmur 128 Managed", Managed },
                { "SHA1", Sha1 },
                { "MD5", Md5 }
            });

            if (Debugger.IsAttached)
            {
                Console.WriteLine("Press any key to exit.");
                Console.Read();
            }
        }

        private static byte[] GenerateRandomData()
        {
            byte[] data = new byte[261];
            using (var gen = RandomNumberGenerator.Create())
                gen.GetBytes(data);

            return data;
        }

        private static void Run(Dictionary<string, HashAlgorithm> steps = null, int times = 1000000)
        {
            foreach (var step in steps)
            {
                var name = step.Key;
                var algorithm = step.Value;

                Console.WriteLine("* Profiling '{0}' x {1:N0}", name, times);
                var duration = Profile(algorithm, times);
                Console.WriteLine("   ===> {0:N} ms ({1:N3} / ms)", duration.TotalMilliseconds, times / duration.TotalMilliseconds);
                Console.WriteLine("   ===> {0:N0} ticks ({1:N3} / tick)", duration.Ticks, times / (float)duration.Ticks);
                Console.WriteLine(); Console.WriteLine();
            }
        }

        static TimeSpan Profile(HashAlgorithm algorithm, int times)
        {
            Console.WriteLine("   ===> {0}", GetHashAsString(algorithm.ComputeHash(Data)));

            // warmup
            for (int i = 0; i < 100; i++)
                algorithm.ComputeHash(Data);

            // profile
            var timer = Stopwatch.StartNew();
            for (int i = 0; i < times; i++)
            {
                var hash = algorithm.ComputeHash(Data);
                bool a = hash != null;
            }

            timer.Stop();
            return timer.Elapsed;
        }

        private static string GetHashAsString(byte[] hash)
        {
            var builder = new StringBuilder(16);
            for (int i = 0; i < hash.Length; i++)
                builder.Append(hash[i].ToString("x2"));

            return builder.ToString();
        }
    }
}
