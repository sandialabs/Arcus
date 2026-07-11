using System.Net;
using System.Net.Sockets;
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
    ///     Benchmarks for <see cref="Subnet"/> construction, parsing, and membership operations.
    /// </summary>
    public class SubnetBenchmarks
    {
        [Params(AddressFamily.InterNetwork, AddressFamily.InterNetworkV6)]
        public AddressFamily Family { get; set; }

        private IPAddress _low;
        private IPAddress _high;
        private IPAddress _address;
        private int _routingPrefix;
        private string _cidrString;
        private Subnet _subnet;
        private Subnet _innerSubnet;
        private IPAddress _containedAddress;
        private Subnet _adjacentSubnet;

        // FromNetMask is IPv4-only; these fields are always IPv4 regardless of Family param.
        private static readonly IPAddress Ipv4Address = IPAddress.Parse("192.168.1.42");
        private static readonly IPAddress Ipv4Netmask = IPAddress.Parse("255.255.255.0");

        [GlobalSetup]
        public void Setup()
        {
            if (Family == AddressFamily.InterNetwork)
            {
                _low = IPAddress.Parse("192.168.1.0");
                _high = IPAddress.Parse("192.168.1.255");
                _address = IPAddress.Parse("192.168.1.0");
                _routingPrefix = 24;
                _cidrString = "192.168.1.0/24";
                _subnet = new Subnet(_address, _routingPrefix);
                _innerSubnet = new Subnet(IPAddress.Parse("192.168.1.64"), 26);
                _containedAddress = IPAddress.Parse("192.168.1.200");
                _adjacentSubnet = new Subnet(IPAddress.Parse("192.168.2.0"), 24);
            }
            else
            {
                _low = IPAddress.Parse("2001:db8::");
                _high = IPAddress.Parse("2001:db8:0:0:ffff:ffff:ffff:ffff");
                _address = IPAddress.Parse("2001:db8::");
                _routingPrefix = 64;
                _cidrString = "2001:db8::/64";
                _subnet = new Subnet(_address, _routingPrefix);
                _innerSubnet = new Subnet(IPAddress.Parse("2001:db8::1:0:0:0"), 96);
                _containedAddress = IPAddress.Parse("2001:db8::dead:beef");
                _adjacentSubnet = new Subnet(IPAddress.Parse("2001:db8:0:1::"), 64);
            }
        }

        [Benchmark]
        public Subnet ConstructFromTwoAddresses()
        {
            return new(_low, _high);
        }

        [Benchmark]
        public Subnet ConstructFromAddressAndPrefix()
        {
            return new(_address, _routingPrefix);
        }

        [Benchmark]
        public Subnet Parse()
        {
            return Subnet.Parse(_cidrString);
        }

        [Benchmark]
        public bool TryParse()
        {
            return Subnet.TryParse(_cidrString, out _);
        }

        // Always benchmarks the IPv4 path regardless of Family param.
        [Benchmark]
        public static Subnet FromNetMask()
        {
            return Subnet.FromNetMask(Ipv4Address, Ipv4Netmask);
        }

        [Benchmark]
        public bool Contains_Address()
        {
            return _subnet.Contains(_containedAddress);
        }

        [Benchmark]
        public bool Contains_Range()
        {
            return _subnet.Contains(_innerSubnet);
        }

        [Benchmark]
        public bool Overlaps()
        {
            return _subnet.Overlaps(_innerSubnet);
        }

        [Benchmark]
        public bool Touches()
        {
            return _subnet.Touches(_adjacentSubnet);
        }
    }
}
