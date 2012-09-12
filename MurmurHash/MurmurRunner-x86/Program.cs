using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Murmur;
using System.Security.Cryptography;

namespace MurmurRunner_x86
{
    class Program
    {
        static readonly HashAlgorithm Unmanaged = MurmurHash.Create128(managed: false);
        static readonly HashAlgorithm Managed = MurmurHash.Create128(managed: true);
        static readonly byte[] Data = GenerateRandomData();

        static void Main(string[] args)
        {
            Run(steps: new Dictionary<string,HashAlgorithm> {
                { "Unmanaged", Unmanaged },
                { "Managed", Managed }
            });

            Console.WriteLine();
            Console.WriteLine("Press any key to exit.");
            Console.Read();
        }

        private static byte[] GenerateRandomData()
        {
            byte[] data = new byte[263];
            using (var gen = RandomNumberGenerator.Create())
                gen.GetBytes(data);

            return data;
        }

        private static void Run(Dictionary<string,HashAlgorithm> steps = null, int times = 10000000)
        {
            foreach (var step in steps)
            {
                var name = step.Key;
                var algorithm = step.Value;

                Console.WriteLine("* Profiling '{0}' x {1:N}", name, times);
                var duration = Profile(algorithm, times);
                Console.WriteLine("   ===> {0:N}ms ({1:N} / ms)", duration.TotalMilliseconds, times / duration.TotalMilliseconds);
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
