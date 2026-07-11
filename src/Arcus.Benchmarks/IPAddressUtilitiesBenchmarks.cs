using System.Net;
using System.Net.Sockets;
using Arcus.Utilities;
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
    ///     Benchmarks for <see cref="IPAddressUtilities"/> hex parsing methods.
    /// </summary>
    public class IPAddressUtilitiesBenchmarks
    {
        [Params(AddressFamily.InterNetwork, AddressFamily.InterNetworkV6)]
        public AddressFamily Family;

        private string _hexString;

        [GlobalSetup]
        public void Setup()
        {
            _hexString =
                Family == AddressFamily.InterNetwork
                    ? "C0A80164" // 192.168.1.100
                    : "20010DB8DEADBEEFCAFE000000001234"; // 2001:db8::dead:beef:cafe:0:1234
        }

        [Benchmark]
        public IPAddress ParseFromHexString()
        {
            return IPAddressUtilities.ParseFromHexString(_hexString, Family);
        }
    }
}
