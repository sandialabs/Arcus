using System.Net;
using System.Net.Sockets;
using Arcus.Converters;
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
    ///     Benchmarks for <see cref="IPAddressConverters"/> string conversion methods.
    /// </summary>
    public class IPAddressConvertersBenchmarks
    {
        [Params(AddressFamily.InterNetwork, AddressFamily.InterNetworkV6)]
        public AddressFamily Family;

        private IPAddress _address;

        // ToBase85String is only meaningful for IPv6; this field is always IPv6.
        private static readonly IPAddress Ipv6Address = IPAddress.Parse("2001:db8::dead:beef:cafe:1234");

        [GlobalSetup]
        public void Setup()
        {
            _address =
                Family == AddressFamily.InterNetwork
                    ? IPAddress.Parse("192.168.1.100")
                    : IPAddress.Parse("2001:db8::dead:beef:cafe:1234");
        }

        // ToBase85String applies to IPv6 only; always benchmarks the IPv6 path.
        [Benchmark]
        public static string ToBase85String()
        {
            return Ipv6Address.ToBase85String();
        }

        [Benchmark]
        public string ToDottedQuadString()
        {
            return _address.ToDottedQuadString();
        }

        [Benchmark]
        public string ToUncompressedString()
        {
            return _address.ToUncompressedString();
        }
    }
}
