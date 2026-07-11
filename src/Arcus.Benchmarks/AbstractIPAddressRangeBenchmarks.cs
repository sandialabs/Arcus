using System.Net;
using System.Net.Sockets;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace Arcus.Benchmarks
{
    public enum RangeSize
    {
        Small, // 4 addresses  (/30 IPv4, /126 IPv6)
        Medium, // 256 addresses (/24 IPv4, /120 IPv6)
        Large, // 4096 addresses (/20 IPv4, /116 IPv6)
    }

    [SimpleJob(RuntimeMoniker.Net48)]
    [SimpleJob(RuntimeMoniker.Net80)]
    [SimpleJob(RuntimeMoniker.Net90)]
    [SimpleJob(RuntimeMoniker.Net10_0)]
    [MemoryDiagnoser]
    /// <summary>
    ///     Benchmarks for <see cref="AbstractIPAddressRange"/> enumeration across address families and range sizes.
    /// </summary>
    public class AbstractIPAddressRangeBenchmarks
    {
        [Params(AddressFamily.InterNetwork, AddressFamily.InterNetworkV6)]
        public AddressFamily Family;

        [Params(RangeSize.Small, RangeSize.Medium, RangeSize.Large)]
        public RangeSize Size;

        private Subnet _subnet;

        [GlobalSetup]
        public void Setup()
        {
            _subnet = (Family, Size) switch
            {
                (AddressFamily.InterNetwork, RangeSize.Small) => new Subnet(IPAddress.Parse("10.0.0.0"), 30),
                (AddressFamily.InterNetwork, RangeSize.Medium) => new Subnet(IPAddress.Parse("10.0.0.0"), 24),
                (AddressFamily.InterNetwork, RangeSize.Large) => new Subnet(IPAddress.Parse("10.0.0.0"), 20),
                (AddressFamily.InterNetworkV6, RangeSize.Small) => new Subnet(IPAddress.Parse("2001:db8::"), 126),
                (AddressFamily.InterNetworkV6, RangeSize.Medium) => new Subnet(IPAddress.Parse("2001:db8::"), 120),
                (AddressFamily.InterNetworkV6, RangeSize.Large) => new Subnet(IPAddress.Parse("2001:db8::"), 116),
                _ => throw new System.ArgumentOutOfRangeException(),
            };
        }

        [Benchmark(Baseline = true)]
#pragma warning disable CS0618 // Type or member is obsolete - intentionally testing backwards-compat path
        public int Enumerate_Foreach()
        {
            var count = 0;
            foreach (var _ in _subnet)
            {
                count++;
            }

            return count;
        }
#pragma warning restore CS0618

        [Benchmark]
        public int Enumerate_Enumerate()
        {
            var count = 0;
            foreach (var _ in _subnet.ToIPAddresses())
            {
                count++;
            }

            return count;
        }
    }
}
