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
    ///     Benchmarks for <see cref="IPAddressMath"/> comparison and arithmetic methods.
    /// </summary>
    public class IPAddressMathBenchmarks
    {
        [Params(AddressFamily.InterNetwork, AddressFamily.InterNetworkV6)]
        public AddressFamily Family;

        private IPAddress _address;
        private IPAddress _low;
        private IPAddress _high;

        [GlobalSetup]
        public void Setup()
        {
            if (Family == AddressFamily.InterNetwork)
            {
                _address = IPAddress.Parse("192.168.1.100");
                _low = IPAddress.Parse("192.168.1.0");
                _high = IPAddress.Parse("192.168.1.255");
            }
            else
            {
                _address = IPAddress.Parse("2001:db8::dead:beef");
                _low = IPAddress.Parse("2001:db8::");
                _high = IPAddress.Parse("2001:db8::ffff:ffff:ffff:ffff");
            }
        }

        [Benchmark]
        public IPAddress Increment()
        {
            return _address.Increment();
        }

        [Benchmark]
        public bool IsGreaterThan()
        {
            return _address.IsGreaterThan(_low);
        }

        [Benchmark]
        public bool IsBetween()
        {
            return _address.IsBetween(_low, _high);
        }
    }
}
