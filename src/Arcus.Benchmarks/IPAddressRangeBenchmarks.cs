using System.Net;
using System.Net.Sockets;
using Arcus.Math;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace Arcus.Benchmarks
{
    [SimpleJob(RuntimeMoniker.Net48)]
    [SimpleJob(RuntimeMoniker.Net80)]
    [SimpleJob(RuntimeMoniker.Net90)]
    [SimpleJob(RuntimeMoniker.Net10_0)]
    [MemoryDiagnoser]
    /// <summary>
    ///     Benchmarks for <see cref="IPAddressRange"/> collapse and exclusion operations.
    /// </summary>
    public class IPAddressRangeBenchmarks
    {
        [Params(AddressFamily.InterNetwork, AddressFamily.InterNetworkV6)]
        public AddressFamily Family;

        [Params(10, 100, 1000)]
        public int Count;

        private List<IPAddressRange> _ranges;
        private IPAddressRange _initialRange;
        private List<IPAddressRange> _exclusions;

        [GlobalSetup]
        public void Setup()
        {
            var start = Family == AddressFamily.InterNetwork ? IPAddress.Parse("10.0.0.0") : IPAddress.Parse("2001:db8::");

            // Build Count adjacent /28-equivalent ranges (16 addresses each) with no gaps,
            // so TryCollapseAll has a realistic merge workload.
            _ranges = new List<IPAddressRange>(Count);
            var cursor = start;
            for (var i = 0; i < Count; i++)
            {
                var rangeEnd = cursor.Increment(15);
                _ranges.Add(new IPAddressRange(cursor, rangeEnd));
                cursor = rangeEnd.Increment(1);
            }

            // Build a large initial range and Count non-overlapping exclusion ranges within it.
            var bigEnd = cursor.Increment(-1);
            _initialRange = new IPAddressRange(start, bigEnd);

            _exclusions = new List<IPAddressRange>(Count);
            cursor = start.Increment(1);
            for (var i = 0; i < Count; i++)
            {
                var exEnd = cursor.Increment(6);
                _exclusions.Add(new IPAddressRange(cursor, exEnd));
                cursor = exEnd.Increment(2);
            }
        }

        [Benchmark]
        public bool TryCollapseAll()
        {
            return IPAddressRange.TryCollapseAll(_ranges, out _);
        }

        [Benchmark]
        public bool TryExcludeAll()
        {
            return IPAddressRange.TryExcludeAll(_initialRange, _exclusions, out _);
        }
    }
}
