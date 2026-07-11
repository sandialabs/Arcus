using System.Net;
using System.Net.Sockets;
using System.Numerics;
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
    ///     Benchmarks for <see cref="BigEndianBitWrapper"/> construction, arithmetic, and conversion.
    /// </summary>
    public class BigEndianBitWrapperBenchmarks
    {
        [Params(AddressFamily.InterNetwork, AddressFamily.InterNetworkV6)]
        public AddressFamily Family;

        private byte[] _addressBytes;
        private BigEndianBitWrapper _wrapper;
        private int _byteWidth;
        private int _prefixLength;

        [GlobalSetup]
        public void Setup()
        {
            if (Family == AddressFamily.InterNetwork)
            {
                _addressBytes = IPAddress.Parse("192.168.1.100").GetAddressBytes();
                _byteWidth = 4;
                _prefixLength = 24;
            }
            else
            {
                _addressBytes = IPAddress.Parse("2001:db8::dead:beef").GetAddressBytes();
                _byteWidth = 16;
                _prefixLength = 64;
            }

            _wrapper = BigEndianBitWrapper.FromBytes(_addressBytes);
        }

        [Benchmark]
        public int FromBytes()
        {
            return BigEndianBitWrapper.FromBytes(_addressBytes).ByteWidth;
        }

        [Benchmark]
        public bool TryAdd()
        {
            _wrapper.TryAdd(1, out var result);
            return result.ByteWidth > 0;
        }

        [Benchmark]
        public byte[] ToBytes()
        {
            return _wrapper.ToBytes();
        }

        [Benchmark]
        public BigInteger ToBigInteger()
        {
            return _wrapper.ToBigInteger();
        }

        [Benchmark]
        public int CreateMask()
        {
            return BigEndianBitWrapper.CreateMask(_byteWidth, _prefixLength).ByteWidth;
        }
    }
}
