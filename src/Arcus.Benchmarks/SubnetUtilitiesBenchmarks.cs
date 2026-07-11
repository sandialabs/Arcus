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
    ///     Benchmarks for <see cref="SubnetUtilities.FewestConsecutiveSubnetsFor"/> across address families and range sizes.
    /// </summary>
    public class SubnetUtilitiesBenchmarks
    {
        [Params(AddressFamily.InterNetwork, AddressFamily.InterNetworkV6)]
        public AddressFamily Family;

        [Params(RangeSize.Small, RangeSize.Medium, RangeSize.Large)]
        public RangeSize Size;

        private IPAddress _low;
        private IPAddress _high;

        [GlobalSetup]
        public void Setup()
        {
            // CodeQL [cs/missed-ternary-operator] if/else is clearer than nested ternaries for this setup logic — each branch has a distinct family-specific switch with documentation
            if (Family == AddressFamily.InterNetwork)
            {
                // Non-power-of-two ranges force recursive binary subdivision.
                (_low, _high) = Size switch
                {
                    RangeSize.Small => (IPAddress.Parse("10.0.0.1"), IPAddress.Parse("10.0.0.10")),
                    RangeSize.Medium => (IPAddress.Parse("10.0.0.1"), IPAddress.Parse("10.0.0.254")),
                    RangeSize.Large => (IPAddress.Parse("10.0.0.1"), IPAddress.Parse("10.0.15.254")),
                    _ => throw new System.ArgumentOutOfRangeException(),
                };
            }
            else
            {
                (_low, _high) = Size switch
                {
                    RangeSize.Small => (IPAddress.Parse("2001:db8::1"), IPAddress.Parse("2001:db8::a")),
                    RangeSize.Medium => (IPAddress.Parse("2001:db8::1"), IPAddress.Parse("2001:db8::fe")),
                    RangeSize.Large => (IPAddress.Parse("2001:db8::1"), IPAddress.Parse("2001:db8::0:fffd")),
                    _ => throw new System.ArgumentOutOfRangeException(),
                };
            }
        }

        [Benchmark]
        public List<Subnet> FewestConsecutiveSubnetsFor()
        {
            var result = new List<Subnet>();
            foreach (var subnet in SubnetUtilities.FewestConsecutiveSubnetsFor(_low, _high))
            {
                result.Add(subnet);
            }

            return result;
        }
    }
}
