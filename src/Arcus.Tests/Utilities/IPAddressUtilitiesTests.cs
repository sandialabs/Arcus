using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Arcus.Math;
using Arcus.Utilities;
using Gulliver;
using Xunit;

namespace Arcus.Tests.Utilities
{
#if NET6_0_OR_GREATER
#pragma warning disable IDE0062 // Make local function static (IDE0062); purposely allowing non-static functions that could be static for .net4.8 compatibility
#endif
    public class IPAddressUtilitiesTests
    {
        #region IPv4MaxAddress

        [Fact]
        public void IPv4MaxAddress_Test()
        {
            // Arrange
            // Act
            var address = IPAddressUtilities.IPv4MaxAddress;

            // Assert
            Assert.Equal(IPAddress.Parse("255.255.255.255"), address);
        }

        #endregion // end: IPv4MaxAddress

        #region IPv4MinAddress

        [Fact]
        public void IPv4MinAddress_Test()
        {
            // Arrange
            // Act
            var address = IPAddressUtilities.IPv4MinAddress;

            // Assert
            Assert.Equal(IPAddress.Parse("0.0.0.0"), address);
        }

        #endregion // end: IPv4MinAddress

        #region IPv4OctetCount

        [Fact]
        public void IPv4OctetCount_Test()
        {
            // Arrange
            // Act
            // Assert
            Assert.Equal(4, IPAddressUtilities.IPv4OctetCount);
        }

        #endregion end: IPv4OctetCount

        #region IPv6HextetCount

        [Fact]
        public void IPv6HextetCount_Test()
        {
            // Arrange
            // Act
            // Assert
            Assert.Equal(8, IPAddressUtilities.IPv6HextetCount);
        }

        #endregion end: IPv6HextetCount

        #region IPv6MaxAddress

        [Fact]
        public void IPv6MaxAddress_Test()
        {
            // Arrange
            // Act
            var address = IPAddressUtilities.IPv6MaxAddress;

            // Assert
            Assert.Equal(IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff"), address);
        }

        #endregion // end: IPv6MaxAddress

        #region IPv6MinAddress

        [Fact]
        public void IPv6MinAddress_Test()
        {
            // Arrange
            // Act
            var address = IPAddressUtilities.IPv6MinAddress;

            // Assert
            Assert.Equal(IPAddress.Parse("::"), address);
        }

        #endregion // end: IPv6MinAddress

        #region IsIPv4

        [Theory]
        [InlineData(false, null)]
        [InlineData(false, "::")]
        [InlineData(false, "ffff::")]
        [InlineData(true, "192.168.1.1")]
        [InlineData(true, "0.0.0.0")]
        public void IsIPv4_Test(bool expected, string input)
        {
            // Arrange
            _ = IPAddress.TryParse(input, out var address);

            // Act
            var result = address.IsIPv4();

            // Assert
            Assert.Equal(expected, result);
        }

        #endregion // end: IsIPv4

        #region IsIPv4MappedIPv6

        [Theory]
        [InlineData(false, null)]
        [InlineData(false, "::")]
        [InlineData(false, "192.168.1.1")]
        [InlineData(true, "::ffff:222.1.41.90")]
        [InlineData(true, "::ffff:ab:cd")]
        [InlineData(false, "1234::ffff:222.1.41.90")]
        [InlineData(false, "1234::ffff:ab:cd")]
        public void IsIPv4MappedIPv6_Test(bool expected, string input)
        {
            // Arrange
            _ = IPAddress.TryParse(input, out var address);

            // Act
            var result = address.IsIPv4MappedIPv6();

            // Assert
            Assert.Equal(expected, result);
        }

        #endregion // end: IsIPv4MappedIPv6

        #region IsIPv6

        [Theory]
        [InlineData(false, null)]
        [InlineData(true, "::")]
        [InlineData(true, "ffff::")]
        [InlineData(false, "192.168.1.1")]
        [InlineData(false, "0.0.0.0")]
        public void IsIPv6_Test(bool expected, string input)
        {
            // Arrange
            _ = IPAddress.TryParse(input, out var address);

            // Act
            var result = address.IsIPv6();

            // Assert
            Assert.Equal(expected, result);
        }

        #endregion // end: IsIPv6

        #region other members

        private static IEnumerable<AddressFamily> NonStandardAddressFamilies()
        {
            return Enum.GetValues(typeof(AddressFamily))
                .Cast<AddressFamily>()
                .Except(new[] { AddressFamily.InterNetwork, AddressFamily.InterNetworkV6 });
        }

        public static IEnumerable<object[]> InvalidAddressFamily_Values()
        {
            return Enum.GetValues(typeof(AddressFamily))
                .Cast<AddressFamily>()
                .Where(addressFamily =>
                    addressFamily != AddressFamily.InterNetworkV6 && addressFamily != AddressFamily.InterNetwork
                )
                .Distinct()
                .Select(e => new object[] { e });
        }

        public static IEnumerable<object[]> ValidAddressFamily_Values()
        {
            yield return new object[] { AddressFamily.InterNetwork };
            yield return new object[] { AddressFamily.InterNetworkV6 };
        }

        private static IEnumerable<IPAddress> GeneralPurposeIPv4Addresses()
        {
            var addressStrings = new[] { "10.0.0.0", "10.0.0.128", "0.0.0.0", "255.255.255.255", "192.168.1.1" };

            foreach (var addressString in addressStrings)
            {
                yield return IPAddress.Parse(addressString);
            }

            yield return IPAddress.Any;
            yield return IPAddress.Broadcast;
            yield return IPAddress.Loopback;
            yield return IPAddress.None;
        }

        private static IEnumerable<IPAddress> GeneralPurposeIPv6Addresses()
        {
            var addressStrings = new[]
            {
                "::",
                "::1",
                "1::",
                "ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff",
                "1234:ffff:ffff:ffff::",
                "::ffff:ffff:ffff:abcd",
                "1234::abcd",
                "1234::ffff:abcd",
                "1234:ffff::abcd",
                "1234:ffff::ffff:abcd",
                "1:2:3:4:5:6:7:8",
                "a:b:c::",
                "::a:b:c",
            };

            foreach (var addressString in addressStrings)
            {
                yield return IPAddress.Parse(addressString);
            }

            yield return IPAddress.IPv6Any;
            yield return IPAddress.IPv6Loopback;
        }

        #endregion

        #region IsValidNetMask

        public static IEnumerable<object[]> IsValidNetMask_Test_Values()
        {
            // all valid netmask values
            for (var i = 0; i <= 32; i++)
            {
                var netmaskBytes = Enumerable.Repeat((byte)0xFF, 4).ToArray().ShiftBitsLeft(32 - i);

                yield return new object[] { true, new IPAddress(netmaskBytes) };
            }

            yield return new object[] { false, null };

            var invalidNetmaskAddressStrings = new[]
            {
                "::",
                "ffff::",
                "255.255.0.255",
                "255.255.0.255",
                "255.0.255.255",
                "0.255.255.255",
                "0.0.0.255",
                "0.0.0.1",
            };
            foreach (var s in invalidNetmaskAddressStrings)
            {
                yield return new object[] { false, IPAddress.Parse(s) };
            }
        }

        [Theory]
        [MemberData(nameof(IsValidNetMask_Test_Values))]
        public void IsValidNetMask_Test(bool expected, IPAddress input)
        {
            // Arrange
            // Act
            var result = input.IsValidNetMask();

            // Assert
            Assert.Equal(expected, result);
        }

        #endregion // end: IsValidNetMask

        #region Parse / TryParse

        #region ParseFromHexString / TryParseFromHexString

        public static IEnumerable<object[]> ParseFromHexString_Test_Values()
        {
            foreach (var address in Addresses())
            {
                var asHex = AddressToHexString(address);

                yield return new object[] { address, asHex.ToUpperInvariant(), address.AddressFamily };
                yield return new object[] { address, asHex.ToLowerInvariant(), address.AddressFamily };
                yield return new object[] { address, $"0x{asHex}".ToUpperInvariant(), address.AddressFamily };
                yield return new object[] { address, $"0x{asHex}".ToLowerInvariant(), address.AddressFamily };

                // removed most significant zero bytes
                var msbZeroTrim = new string(asHex.SkipWhile(c => c == '0').ToArray());

                if (!string.IsNullOrEmpty(msbZeroTrim))
                {
                    yield return new object[] { address, msbZeroTrim.ToUpperInvariant(), address.AddressFamily };
                    yield return new object[] { address, msbZeroTrim.ToLowerInvariant(), address.AddressFamily };
                    yield return new object[] { address, $"0x{msbZeroTrim}".ToUpperInvariant(), address.AddressFamily };
                    yield return new object[] { address, $"0x{msbZeroTrim}".ToLowerInvariant(), address.AddressFamily };
                }
            }

            yield return new object[] { IPAddress.Parse("128.128.128.128"), "00000000080808080", AddressFamily.InterNetwork }; // extra zero msb

            // expected failures
            yield return new object[] { null, null, AddressFamily.InterNetwork };
            yield return new object[] { null, null, AddressFamily.InterNetworkV6 };
            yield return new object[]
            {
                null,
                AddressToHexString(IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff")),
                AddressFamily.InterNetwork,
            };

            // non standard address family
            foreach (var addressFamily in NonStandardAddressFamilies())
            {
                yield return new object[] { null, "0", addressFamily };
            }

            IEnumerable<IPAddress> Addresses()
            {
                yield return IPAddress.Any;
                yield return IPAddress.Loopback;
                yield return IPAddress.None;
                yield return IPAddress.Parse("192.168.1.1");
                yield return IPAddress.Parse("255.255.255");

                yield return IPAddress.IPv6Any;
                yield return IPAddress.IPv6Loopback;
                yield return IPAddress.Parse("2001:0db8:85a3:0042:1000:8a2e:0370:7334");
                yield return IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff");
                yield return IPAddress.Parse("ffff:ffff:ffff:ffff::");
                yield return IPAddress.Parse("::abc:ffff:ffff:ffff");
            }

            string AddressToHexString(IPAddress address)
            {
                return string.Concat(address.GetAddressBytes().Select(b => Convert.ToString(b, 16).PadLeft(2, '0')));
            }
        }

        [Theory]
        [MemberData(nameof(ParseFromHexString_Test_Values))]
        public void ParseFromHexString_Test(IPAddress expected, string addressString, AddressFamily addressFamily)
        {
            // Arrange
            if (expected == null)
            {
                return; // ignore invalid test
            }

            // Act
            var result = IPAddressUtilities.ParseFromHexString(addressString, addressFamily);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ParseFromHexString_NullInput_Throws_ArgumentNullException_Test()
        {
            // Arrange
            // Act
            // Assert

            Assert.Throws<ArgumentNullException>(() => IPAddressUtilities.ParseFromHexString(null, default));
        }

        [Theory]
        [InlineData("")]
        [InlineData("\t")]
        [InlineData(" ")]
        public void ParseFromHexString_EmptyOrWhitespaceInput_Throws_ArgumentException_Test(string input)
        {
            // Arrange
            // Act
            // Assert

            Assert.Throws<ArgumentException>(() => IPAddressUtilities.ParseFromHexString(input, default));
        }

        [Theory]
        [MemberData(nameof(InvalidAddressFamily_Values))]
        public void ParseFromHexString_InvalidAddressFamily_Throws_ArgumentException_Test(AddressFamily addressFamily)
        {
            // Arrange
            // Act
            // Assert

            Assert.Throws<ArgumentException>(() => IPAddressUtilities.ParseFromHexString("abc123", addressFamily));
        }

        [Theory]
        [InlineData("abcdxyz")]
        [InlineData("potato")]
        [InlineData("%$#")]
        public void ParseFromHexString_NonHexInput_Throws_ArgumentException_Test(string input)
        {
            // Arrange
            // Act
            // Assert

            Assert.Throws<ArgumentException>(() => IPAddressUtilities.ParseFromHexString(input, AddressFamily.InterNetwork));
        }

        [Theory]
        [MemberData(nameof(ParseFromHexString_Test_Values))]
        public void TryParseFromHexString_Test(IPAddress expected, string addressString, AddressFamily addressFamily)
        {
            // Arrange
            // Act
            var success = IPAddressUtilities.TryParseFromHexString(addressString, addressFamily, out var result);

            // Assert
            Assert.Equal(expected != null, success);
            Assert.Equal(expected, result);
        }

        #endregion // end: ParseFromHexString / TryParseFromHexString

        #region ParseIgnoreOctalInIPv4 / TryParseIgnoreOctalInIPv4

        public static IEnumerable<object[]> ParseIgnoreOctalInIPv4_Test_Values()
        {
            foreach (var address in Addresses())
            {
                yield return new object[] { address, address.ToString() };

                if (address.AddressFamily == AddressFamily.InterNetwork)
                {
                    yield return new object[] { address, AddressToQuads(address) };
                }
            }

            yield return new object[] { IPAddress.Parse("0.0.0.192"), "192" };
            yield return new object[] { IPAddress.Parse("1.0.0.192"), "1.192" };
            yield return new object[] { IPAddress.Parse("1.255.0.192"), "1.255.192" };

            // octal case
            yield return new object[] { IPAddress.Parse("7.7.7.0"), "007.007.7.0" };

            // expected failures
            yield return new object[] { null, null };
            yield return new object[] { null, "potato" };
            yield return new object[] { null, "255.255.255.255.255" };

            IEnumerable<IPAddress> Addresses()
            {
                yield return IPAddress.Any;
                yield return IPAddress.Loopback;
                yield return IPAddress.None;
                yield return IPAddress.Parse("7.7.7.7"); // explicit octal
                yield return IPAddress.Parse("0.0.0.192");
                yield return IPAddress.Parse("192.168.1.1");
                yield return IPAddress.Parse("255.255.255");

                yield return IPAddress.IPv6Any;
                yield return IPAddress.IPv6Loopback;
                yield return IPAddress.Parse("2001:0db8:85a3:0042:1000:8a2e:0370:7334");
                yield return IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff");
                yield return IPAddress.Parse("ffff:ffff:ffff:ffff::");
                yield return IPAddress.Parse("::abc:ffff:ffff:ffff");
            }

            string AddressToQuads(IPAddress address)
            {
                return string.Join(".", address.GetAddressBytes().Select(b => Convert.ToString(b, 10).PadLeft(3, '0')));
            }
        }

        [Theory]
        [MemberData(nameof(ParseIgnoreOctalInIPv4_Test_Values))]
        public void ParseIgnoreOctalInIPv4_Test(IPAddress expected, string input)
        {
            // Arrange
            if (expected == null)
            {
                return; // ignore invalid test
            }

            // Act
            var result = IPAddressUtilities.ParseIgnoreOctalInIPv4(input);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ParseIgnoreOctalInIPv4_NullInput_ThrowsArgumentNullException_Test()
        {
            // Arrange
            // Act
            // Assert

            Assert.Throws<ArgumentNullException>(() => IPAddressUtilities.ParseIgnoreOctalInIPv4(null));
        }

        [Theory]
        [InlineData("")]
        [InlineData("\t")]
        [InlineData(" ")]
        public void ParseIgnoreOctalInIPv4_EmptyOrWhitespaceInput_Throws_ArgumentException_Test(string input)
        {
            // Arrange
            // Act
            // Assert

            Assert.Throws<ArgumentException>(() => IPAddressUtilities.ParseIgnoreOctalInIPv4(input));
        }

        [Theory]
        [MemberData(nameof(ParseIgnoreOctalInIPv4_Test_Values))]
        public void TryParseIgnoreOctalInIPv4_Test(IPAddress expected, string input)
        {
            // Arrange
            var success = IPAddressUtilities.TryParseIgnoreOctalInIPv4(input, out var result);

            // Assert
            Assert.Equal(expected != null, success);
            Assert.Equal(expected, result);
        }

        #endregion // end: ParseIgnoreOctalInIPv4 / TryParseIgnoreOctalInIPv4

        #region TryParse(byte[])

        public static IEnumerable<object[]> Parse_BytesArray_Test_Values()
        {
            foreach (var address in Addresses())
            {
                yield return new object[] { address, address.GetAddressBytes().ToArray(), address.AddressFamily };
            }

            // underflow, add msb zeros
            yield return new object[] { IPAddress.Parse("0.0.0.0"), Array.Empty<byte>(), AddressFamily.InterNetwork };
            yield return new object[] { IPAddress.Parse("::"), Array.Empty<byte>(), AddressFamily.InterNetworkV6 };

            yield return new object[] { IPAddress.Parse("0.0.0.255"), new byte[] { 0x00, 0xff }, AddressFamily.InterNetwork };
            yield return new object[]
            {
                IPAddress.Parse("::acca"),
                new byte[] { 0x00, 0x00, 0xac, 0xca },
                AddressFamily.InterNetworkV6,
            };

            // ipv4 overflow
            yield return new object[] { null, Enumerable.Repeat((byte)0xff, 5).ToArray(), AddressFamily.InterNetwork };

            // ipv6 overflow
            yield return new object[] { null, Enumerable.Repeat((byte)0xff, 17).ToArray(), AddressFamily.InterNetworkV6 };

            yield return new object[] { null, null, AddressFamily.InterNetwork };
            yield return new object[] { null, null, AddressFamily.InterNetworkV6 };

            // non standard address family
            foreach (var addressFamily in NonStandardAddressFamilies())
            {
                yield return new object[] { null, Array.Empty<byte>(), addressFamily };
            }

            IEnumerable<IPAddress> Addresses()
            {
                foreach (var address in GeneralPurposeIPv4Addresses().Concat(GeneralPurposeIPv6Addresses()))
                {
                    yield return address;
                }
            }
        }

        [Theory]
        [MemberData(nameof(Parse_BytesArray_Test_Values))]
        public void Parse_ByteArray_Test(IPAddress expected, byte[] bytes, AddressFamily addressFamily)
        {
            // Arrange
            if (expected == null)
            {
                return; // ignore invalid test
            }

            // Act
            var result = IPAddressUtilities.Parse(bytes, addressFamily);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [MemberData(nameof(InvalidAddressFamily_Values))]
        public void Parse_Bytes_InvalidAddressFamily_Throws_ArgumentOutOfRangeException_Test(AddressFamily addressFamily)
        {
            // Arrange
            // Assert
            // Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => IPAddressUtilities.Parse(new byte[] { 0x42 }, addressFamily));
        }

        [Theory]
        [InlineData(17, AddressFamily.InterNetworkV6)]
        [InlineData(5, AddressFamily.InterNetwork)]
        public void Parse_Bytes_InputTooLong_Throws_ArgumentOutOfRangeException_Test(int count, AddressFamily addressFamily)
        {
            // Arrange
            // Assert
            // Assert
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                IPAddressUtilities.Parse(Enumerable.Repeat((byte)0x00, count).ToArray(), addressFamily)
            );
        }

        [Theory]
        [MemberData(nameof(Parse_BytesArray_Test_Values))]
        public void TryParse_ByteArray_Test(IPAddress expected, byte[] bytes, AddressFamily addressFamily)
        {
            // Arrange
            // Act
            var success = IPAddressUtilities.TryParse(bytes, addressFamily, out var result);

            // Assert
            Assert.Equal(expected != null, success);
            Assert.Equal(expected, result);
        }

        #endregion // end: TryParse(byte[])

        #endregion // end: Parse / TryParse

        #region MaxIPAddress

        [Fact]
        public void MaxIPAddress_Ipv4_Test()
        {
            // Arrange
            // Act
            var result = AddressFamily.InterNetwork.MaxIPAddress();

            // Assert
            Assert.Same(IPAddressUtilities.IPv4MaxAddress, result);
        }

        [Fact]
        public void MaxIPAddress_Ipv6_Test()
        {
            // Arrange
            // Act
            var result = AddressFamily.InterNetworkV6.MaxIPAddress();

            // Assert
            Assert.Same(IPAddressUtilities.IPv6MaxAddress, result);
        }

        [Theory]
        [MemberData(nameof(InvalidAddressFamily_Values))]
        public void MaxIPAddress_InvalidAddressFamily_Throws_ArgumentException_Test(AddressFamily addressFamily)
        {
            // Arrange
            // Act
            // Assert
            Assert.Throws<ArgumentException>(() => addressFamily.MaxIPAddress());
        }

        #endregion // end: MaxIPAddress

        #region MinIPAddress

        [Fact]
        public void MinIPAddress_Ipv4_Test()
        {
            // Arrange
            // Act
            var result = AddressFamily.InterNetwork.MinIPAddress();

            // Assert
            Assert.Same(IPAddressUtilities.IPv4MinAddress, result);
        }

        [Fact]
        public void MinIPAddress_Ipv6_Test()
        {
            // Arrange
            // Act
            var result = AddressFamily.InterNetworkV6.MinIPAddress();

            // Assert
            Assert.Same(IPAddressUtilities.IPv6MinAddress, result);
        }

        [Theory]
        [MemberData(nameof(InvalidAddressFamily_Values))]
        public void MinIPAddress_InvalidAddressFamily_Throws_ArgumentException_Test(AddressFamily addressFamily)
        {
            // Arrange
            // Act
            // Assert
            Assert.Throws<ArgumentException>(() => addressFamily.MinIPAddress());
        }

        #endregion // end: MinIPAddress

        #region Constant Values

        #region BitCount

        [Fact]
        public void IPv4BitCount_Value_Test()
        {
            // Arrange
            // Act
            // Assert
            Assert.Equal(IPAddressUtilities.IPv4BitCount, IPAddress.Any.GetAddressBytes().Length * 8);
        }

        [Fact]
        public void IPv6BitCount_Value_Test()
        {
            // Arrange
            // Act
            // Assert
            Assert.Equal(IPAddressUtilities.IPv6BitCount, IPAddress.IPv6Any.GetAddressBytes().Length * 8);
        }

        #endregion // end: BitCount

        #region ByteCount

        [Fact]
        public void IPv4ByteCount_Value_Test()
        {
            // Arrange
            // Act
            // Assert
            Assert.Equal(IPAddressUtilities.IPv4ByteCount, IPAddress.Any.GetAddressBytes().Length);
        }

        [Fact]
        public void IPv6ByteCount_Value_Test()
        {
            // Arrange
            // Act
            // Assert
            Assert.Equal(IPAddressUtilities.IPv6ByteCount, IPAddress.IPv6Any.GetAddressBytes().Length);
        }

        #endregion // end: ByteCount

        #region MaxAddress

        [Fact]
        public void IPv6MaxAddress_Value_Test()
        {
            // Arrange
            // Act
            var address = IPAddressUtilities.IPv6MaxAddress;

            // Assert
            Assert.Equal(new IPAddress(Enumerable.Repeat((byte)0xff, 16).ToArray()), address);
            Assert.Equal(AddressFamily.InterNetworkV6, address.AddressFamily);
        }

        [Fact]
        public void IPv4MaxAddress_Value_Test()
        {
            // Arrange
            // Act
            var address = IPAddressUtilities.IPv4MaxAddress;

            // Assert
            Assert.Equal(new IPAddress(Enumerable.Repeat((byte)0xff, 4).ToArray()), address);
            Assert.Equal(AddressFamily.InterNetwork, address.AddressFamily);
        }

        #endregion // end: MaxAddress

        #region MinAddress

        [Fact]
        public void IPv6MinAddress_Value_Test()
        {
            // Arrange
            // Act
            var address = IPAddressUtilities.IPv6MinAddress;

            // Assert
            Assert.Equal(new IPAddress(Enumerable.Repeat((byte)0x00, 16).ToArray()), address);
            Assert.Equal(AddressFamily.InterNetworkV6, address.AddressFamily);
        }

        [Fact]
        public void IPv4MinAddress_Value_Test()
        {
            // Arrange
            // Act
            var address = IPAddressUtilities.IPv4MinAddress;

            // Assert
            Assert.Equal(new IPAddress(Enumerable.Repeat((byte)0x00, 4).ToArray()), address);
            Assert.Equal(AddressFamily.InterNetwork, address.AddressFamily);
        }

        #endregion // end: MinAddress

        #region ValidAddressFamilies

        [Fact]
        public void ValidAddressFamilies_Test()
        {
            // Arrange
            var validAddressFamilies = IPAddressUtilities.ValidAddressFamilies;

            // Act

            // Assert
            Assert.IsAssignableFrom<IReadOnlyCollection<AddressFamily>>(validAddressFamilies); // explicitly read only
            Assert.Equal(2, validAddressFamilies.Count);
            Assert.Contains(AddressFamily.InterNetworkV6, validAddressFamilies);
            Assert.Contains(AddressFamily.InterNetwork, validAddressFamilies);
        }

        #endregion // end: ValidAddressFamilies

        #endregion // end: Constant Values

        #region IsPrivate

        public static IEnumerable<object[]> IsPrivate_Test_Values()
        {
            foreach (var subnet in SubnetUtilities.PrivateIPAddressRangesList)
            {
                yield return new object[] { true, subnet.NetworkPrefixAddress };
                yield return new object[] { true, subnet.NetworkPrefixAddress.Increment(2) };
                yield return new object[] { true, subnet.BroadcastAddress };
                yield return new object[] { true, subnet.BroadcastAddress.Increment(-2) };
            }

            yield return new object[] { false, IPAddressUtilities.IPv4MaxAddress };
            yield return new object[] { false, IPAddressUtilities.IPv4MinAddress };

            yield return new object[] { false, IPAddressUtilities.IPv6MaxAddress };
            yield return new object[] { false, IPAddressUtilities.IPv6MinAddress };
        }

        [Theory]
        [MemberData(nameof(IsPrivate_Test_Values))]
        public void IsPrivate_Test(bool expected, IPAddress address)
        {
            // Arrange
            // Act
            var isPrivate = address.IsPrivate();

            // Assert
            Assert.Equal(expected, isPrivate);
        }

        #endregion
    }
}
