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

        static readonly byte[] RandomData = GenerateRandomData();
        static readonly byte[] SampleData = CreateSampleData();
        const int FAST_ITERATION_COUNT = 1000000;
        const int SLOW_ITERATION_COUNT = 100000;
        static readonly IndentingConsoleWriter OutputWriter = new IndentingConsoleWriter();

        static void Main(string[] args)
        {
            OutputWriter.WriteLine("* Environment Architecture: {0}", Environment.Is64BitProcess ? "x64" : "x86");
            OutputWriter.NewLines(1);

            using (OutputWriter.Indent(2))
            {
                Run(name: "Guid x 8", data: SampleData, steps: new Dictionary<string, Tuple<HashAlgorithm, int>> {
                    { "Murmur 128 Unmanaged", Tuple.Create(Unmanaged, FAST_ITERATION_COUNT) },
                    { "Murmur 128 Managed", Tuple.Create(Managed, FAST_ITERATION_COUNT) },
                    { "SHA1", Tuple.Create(Sha1, SLOW_ITERATION_COUNT) },
                    { "MD5", Tuple.Create(Md5, SLOW_ITERATION_COUNT) }
                });

                Run(name: "Random", data: RandomData, steps: new Dictionary<string, Tuple<HashAlgorithm, int>> {
                    { "Murmur 128 Unmanaged", Tuple.Create(Unmanaged, FAST_ITERATION_COUNT) },
                    { "Murmur 128 Managed", Tuple.Create(Managed, FAST_ITERATION_COUNT) },
                    { "SHA1", Tuple.Create(Sha1, SLOW_ITERATION_COUNT) },
                    { "MD5", Tuple.Create(Md5, SLOW_ITERATION_COUNT) }
                });
            }

            if (Debugger.IsAttached)
            {
                Console.WriteLine("Press any key to exit.");
                Console.Read();
            }
        }

        private static void Run(string name, byte[] data, Dictionary<string, Tuple<HashAlgorithm, int>> steps)
        {
            OutputWriter.WriteLine("* Data Set: {0}", name);
            using (OutputWriter.Indent())
            {
                foreach (var step in steps)
                {
                    var algorithmFriendlyName = step.Key;
                    var algorithm = step.Value.Item1;
                    var iterations = step.Value.Item2;

                    OutputWriter.WriteLine("{0} ({1})", algorithmFriendlyName, algorithm.GetType().Name);
                    Profile(algorithm, iterations, data);
                }
            }
        }

        static void Profile(HashAlgorithm algorithm, int iterations, byte[] data)
        {
            using (OutputWriter.Indent())
            {
                WriteProfilingResult("Runs", "{0:N0}", iterations);
                WriteProfilingResult("Output", GetHashAsString(algorithm.ComputeHash(data)));

                // warmup
                for (int i = 0; i < 1000; i++)
                    algorithm.ComputeHash(data);

                // profile
                var timer = Stopwatch.StartNew();
                for (int i = 0; i < iterations; i++)
                {
                    var hash = algorithm.ComputeHash(data);
                    bool a = hash != null;
                }

                timer.Stop();

                WriteProfilingResult("Duration", "{0:N0} ms ({1:N0} ticks)", timer.ElapsedMilliseconds, timer.ElapsedTicks);
                WriteProfilingResult("Ops/Tick", "{0:N3}", Divide(iterations, timer.ElapsedTicks));
                WriteProfilingResult("Ops/ms", "{0:N3}", Divide(iterations, timer.ElapsedMilliseconds));
            }

            OutputWriter.NewLines();
        }

        static double Divide(long a, long b)
        {
            return ((double)a / (double)b);
        }

        static void WriteProfilingResult(string name, string format = "{0}", params object[] args)
        {
            //var value = string.Format("* {0}:\t===>\t", name);
            var value = string.Format("* {0}:\t", name);
            OutputWriter.WriteLine(value + format, args);
        }

        private static string GetHashAsString(byte[] hash)
        {
            var builder = new StringBuilder(16);
            for (int i = 0; i < hash.Length; i++)
                builder.Append(hash[i].ToString("x2"));

            return builder.ToString();
        }

        public static byte[] Build(Guid[] values)
        {
            var target = new byte[values.Length * 16];
            int offset = 0;
            foreach (var v in values)
            {
                Array.Copy(v.ToByteArray(), 0, target, offset, 16);
                offset += 16;
            }

            return target;
        }

        private static byte[] GenerateRandomData()
        {
            byte[] data = new byte[261];
            using (var gen = RandomNumberGenerator.Create())
                gen.GetBytes(data);

            return data;
        }

        public static byte[] CreateSampleData()
        {
            var data = Build(new Guid[] {
                new Guid("1DB2A25C-57A3-471A-B81B-A01900A63F49"),
                new Guid("24185DD0-1CB6-48EF-90A7-9F4A00D9BA0D"),
                new Guid("6D21CDF4-70CC-4424-B72C-9F4A00D9BA0D"),
                new Guid("6D194DF5-F28E-43B6-BBD2-9FB300D0CE52"),
                new Guid("46F54EF7-5C8C-48C5-82EF-9F4A00D9BA0D"),
                new Guid("DC34023A-A985-4EA2-BD22-9F4A00D9BA0D"),
                new Guid("D12CAC55-E7C5-45D9-AE39-A0200092ACB0"),
                new Guid("AD9EE455-27E2-4C0D-B3F1-9F4A00D9BA0D"),
            });

            return data;
        }
    }

    class IndentingConsoleWriter
    {
        private int IndentAmount { get; set; }

        public IDisposable Indent(int count = 4)
        {
            // create a new scope with current amount, then increment our indent
            var scope = new IndentationScope(this, IndentAmount);
            IndentAmount += count;

            return scope;
        }

        public void WriteLine(string format, params object[] args)
        {
            if (IndentAmount > 0)
                Console.Write(new string(' ', IndentAmount));

            Console.WriteLine(format, args);
        }

        public void NewLines(int count = 1)
        {
            for (int i = 0; i < count; i++)
                Console.WriteLine();
        }

        class IndentationScope : IDisposable
        {
            readonly IndentingConsoleWriter Writer;
            readonly int Amount;
            public IndentationScope(IndentingConsoleWriter writer, int amount)
            {
                Writer = writer;
                Amount = amount;
            }

            public void Dispose()
            {
                // restore indent amount
                Writer.IndentAmount = Amount;
            }
        }
    }
}
