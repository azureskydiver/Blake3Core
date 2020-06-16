using System;
using System.Diagnostics;
using System.Linq;

namespace Blake3Core.Benchmark
{
    class Program
    {
        const int TestDataSize = 64 * 1024;
        const int TrialRunCount = 3;
        readonly TimeSpan RunDuration = TimeSpan.FromSeconds(3);

        static Random _random = new Random();

        byte [] CreateTestData(int size)
            => Enumerable.Range(0, size).Select(i => (byte)_random.Next(256)).ToArray();

        double ToMegabytes(double size) => size / 1024 / 1024.0;

        double BenchmarkHashingThroughput(int trial, TimeSpan duration)
        {
            Console.Write($"Run {trial}: ");
            var data = CreateTestData(TestDataSize);
            var elapsed = TimeSpan.Zero;
            var totalData = 0;

            do
            {
                var hasher = new Blake3();
                var stopwatch = Stopwatch.StartNew();
                hasher.ComputeHash(data);
                stopwatch.Stop();
                elapsed += stopwatch.Elapsed;
                totalData += data.Length;
            } while(elapsed < duration);

            var mbps = ToMegabytes(totalData / elapsed.TotalSeconds);
            Console.WriteLine($"{mbps:N3}MB/second");
            return mbps;
        }

        void Run()
        {
            Console.WriteLine($"Best of {TrialRunCount} tries hashing with {TestDataSize / 1024}KB buffer size for {RunDuration.TotalSeconds} seconds.");
            var bestRun = Enumerable.Range(1, TrialRunCount)
                                    .Select(i => BenchmarkHashingThroughput(i, RunDuration))
                                    .Max();
            Console.WriteLine($"{bestRun:N3}MB/second");
        }

        static void Main() => new Program().Run();
    }
}
