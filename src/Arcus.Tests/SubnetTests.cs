using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Text;
using Arcus;
using Arcus.Tests.XunitSerializers;
using Gulliver;
using Xunit;
using Xunit.Sdk;
#if NET48   // maintained for .NET 4.8 compatibility
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
#endif

[assembly: RegisterXunitSerializer(typeof(SubnetXunitSerializer), typeof(Subnet))]

namespace Arcus.Tests
{
    public class SubnetTests
    {
        #region Addresses

        [Theory]
        [InlineData("192.168.1.0/24")]
        [InlineData("16.8.14.12/28")]
        [InlineData("16.8.14.12/32")]
        [InlineData("::/128")]
        [InlineData("feed:beef::/120")]
        public void Addresses_Test(string input)
        {
            // Arrange
            var subnet = Subnet.Parse(input);

            // Act
            var hosts = subnet.ToList();

            // Assert
            Assert.Equal(subnet.ToArray(), hosts);
        }

        #endregion // end: Addresses

        #region Class

        [Theory]
        [InlineData(typeof(AbstractIPAddressRange))]
        [InlineData(typeof(IEquatable<Subnet>))]
        [InlineData(typeof(IComparable<Subnet>))]
        [InlineData(typeof(IComparable))]
#if NET48
        [InlineData(typeof(ISerializable))]
#endif
        public void Assignability_Test(Type assignableFromType)
        {
            // Arrange
            var type = typeof(Subnet);

            // Act
            var isAssignableFrom = assignableFromType.IsAssignableFrom(type);

            // Assert
            Assert.True(isAssignableFrom);
        }

        #endregion // end: Class

        #region CompareTo / Operators

        public static IEnumerable<object[]> Comparison_Values()
        {
            yield return new object[] { 0, Subnet.Parse("192.168.0.0/16"), Subnet.Parse("192.168.0.0/16") };
            yield return new object[] { 0, Subnet.Parse("ab:cd::/64"), Subnet.Parse("ab:cd::/64") };
            yield return new object[] { 1, Subnet.Parse("192.168.0.0/16"), null };
            yield return new object[] { 1, Subnet.Parse("ab:cd::/64"), null };
            yield return new object[] { 1, Subnet.Parse("192.168.0.0/16"), Subnet.Parse("192.168.0.0/20") };
            yield return new object[] { -1, Subnet.Parse("192.168.0.0/20"), Subnet.Parse("192.168.0.0/16") };
            yield return new object[] { 1, Subnet.Parse("ab:cd::/64"), Subnet.Parse("ab:cd::/96") };
            yield return new object[] { -1, Subnet.Parse("ab:cd::/96"), Subnet.Parse("ab:cd::/64") };
            yield return new object[] { -1, Subnet.Parse("0.0.0.0/0"), Subnet.Parse("::/0") };
            yield return new object[] { 1, Subnet.Parse("::/0"), Subnet.Parse("0.0.0.0/0") };
            yield return new object[] { -1, Subnet.Parse("0.0.0.0/32"), Subnet.Parse("::/128") };
            yield return new object[] { 1, Subnet.Parse("::/128"), Subnet.Parse("0.0.0.0/32") };
        }

        [Theory]
        [MemberData(nameof(Comparison_Values))]
        public void CompareTo_Test(int expected, Subnet left, Subnet right)
        {
            // Arrange
            // Act
            var result = left.CompareTo(right);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [MemberData(nameof(Comparison_Values))]
        public void Operator_Equals_Test(int expected, Subnet left, Subnet right)
        {
            // Arrange
            // Act
            var result = left == right;

            // Assert
            Assert.Equal(expected == 0, result);
        }

        [Theory]
        [MemberData(nameof(Comparison_Values))]
        public void Operator_NotEquals_Test(int expected, Subnet left, Subnet right)
        {
            // Arrange
            // Act
            var result = left != right;

            // Assert
            Assert.Equal(expected != 0, result);
        }

        [Theory]
        [MemberData(nameof(Comparison_Values))]
        public void Operator_GreaterThan_Test(int expected, Subnet left, Subnet right)
        {
            // Arrange
            // Act
            var result = left > right;

            // Assert
            Assert.Equal(expected > 0, result);
        }

        [Theory]
        [MemberData(nameof(Comparison_Values))]
        public void Operator_GreaterThanOrEqual_Test(int expected, Subnet left, Subnet right)
        {
            // Arrange
            // Act
            var result = left >= right;

            // Assert
            Assert.Equal(expected >= 0, result);
        }

        [Theory]
        [MemberData(nameof(Comparison_Values))]
        public void Operator_LessThan_Test(int expected, Subnet left, Subnet right)
        {
            // Arrange
            // Act
            var result = left < right;

            // Assert
            Assert.Equal(expected < 0, result);
        }

        [Theory]
        [MemberData(nameof(Comparison_Values))]
        public void Operator_LessThanOrEqual_Test(int expected, Subnet left, Subnet right)
        {
            // Arrange
            // Act
            var result = left <= right;

            // Assert
            Assert.Equal(expected <= 0, result);
        }

        #endregion

        #region Netmask

        [Fact]
        public void Netmask_Null_ForIPv6_Test()
        {
            // Arrange
            var subnet = new Subnet(IPAddress.IPv6Any, 32);

            // Act
            var netmask = subnet.Netmask;

            // Assert
            Assert.Null(netmask);
        }

        #endregion // end: Netmask

        #region Overlaps

        [Theory]
        [InlineData(true, "0.0.0.0/0", "0.0.0.0/0")]
        [InlineData(true, "::/0", "::/0")]
        [InlineData(true, "0.0.0.0/0", "255.255.0.0/16")]
        [InlineData(true, "255.255.0.0/16", "0.0.0.0/0")]
        [InlineData(true, "::/0", "abcd:ef01::/64")]
        [InlineData(true, "abcd:ef01::/64", "::/0")]
        [InlineData(false, "0.0.0.0/0", null)]
        [InlineData(false, "::/0", null)]
        [InlineData(false, "0.0.0.0/0", "::/0")]
        [InlineData(false, "::/0", "0.0.0.0/0")]
        public void Overlaps_Test(bool expected, string subnetAString, string subnetBString)
        {
            // Arrange
            var subnetA = Subnet.Parse(subnetAString);
            _ = Subnet.TryParse(subnetBString, out var subnetB);

            // Act
            var result = subnetA.Overlaps(subnetB);

            // Assert
            Assert.Equal(expected, result);
        }

        #endregion // end: Overlaps

        #region ToString

        [Theory]
        [InlineData("192.168.1.1/32", "192.168.1.1/32")]
        [InlineData("192.168.0.0/16", "192.168.1.1/16")]
        [InlineData("0.0.0.0/0", "192.168.1.1/0")]
        [InlineData("::/128", "::/128")]
        [InlineData("::/64", "::/64")]
        [InlineData("::/0", "::/0")]
        public void ToString_Test(string expected, string input)
        {
            // Arrange
            // Act
            var result = Subnet.Parse(input).ToString();

            // Assert
            Assert.Equal(expected, result);
        }

        #endregion

        #region UsableHostAddressCount

        public static IEnumerable<object[]> UsableHostAddressCount_Test_Values()
        {
            foreach (var ipAddress in IPv4Addresses())
            {
                for (var i = 0; i <= 32; i++)
                {
                    var routePrefix = 32 - i;
                    var count = routePrefix < 2 ? BigInteger.Zero : BigInteger.Subtract(BigInteger.Pow(2, routePrefix), 2);

                    var subnet = new Subnet(ipAddress, i);
                    yield return new object[] { count, subnet };
                }
            }

            foreach (var ipAddress in IPv6Addresses())
            {
                for (var i = 0; i <= 128; i++)
                {
                    var routePrefix = 128 - i;
                    var count = routePrefix < 2 ? BigInteger.Zero : BigInteger.Subtract(BigInteger.Pow(2, routePrefix), 2);

                    var subnet = new Subnet(ipAddress, i);
                    yield return new object[] { count, subnet };
                }
            }

            IEnumerable<IPAddress> IPv4Addresses()
            {
                yield return IPAddress.Parse("192.168.1.1");
            }

            IEnumerable<IPAddress> IPv6Addresses()
            {
                yield return IPAddress.Parse("2001:0db8:85a3:0042:1000:8a2e:0370:7334");
            }
        }

        [Theory]
        [MemberData(nameof(UsableHostAddressCount_Test_Values))]
        public void UsableHostAddressCount_Test(BigInteger expected, Subnet subnet)
        {
            // Arrange

            // Act
            var result = subnet.UsableHostAddressCount;

            // Assert
            Assert.Equal(expected, result);
        }

        #endregion

        #region Contains

        #region Contains(IPAddress)

        [Theory]
        [InlineData(true, "192.168.0.0/16", "192.168.0.0")]
        [InlineData(true, "192.168.0.0/16", "192.168.0.16")]
        [InlineData(false, "192.168.0.0/16", "192.255.0.0")]
        [InlineData(false, "0.0.0.0/0", null)]
        [InlineData(false, "0.0.0.0/0", "::")]
        [InlineData(true, "::/0", "::")]
        [InlineData(true, "2001:0db8:85a3:0042::/64", "2001:0db8:85a3:0042:1000:8a2e:0370:7334")]
        [InlineData(false, "2001:0db8:85a3:0042::/64", "2007:0db8:85a3::abc")]
        [InlineData(false, "::/0", null)]
        [InlineData(false, "::/0", "192.168.1.1")]
        public void Contains_IPAddress_Test(bool expected, string subnetString, string containsIPAddressString)
        {
            // Arrange
            var subnet = Subnet.Parse(subnetString);
            var containsIPAddress = containsIPAddressString != null ? IPAddress.Parse(containsIPAddressString) : null;

            // Act
            var result = subnet.Contains(containsIPAddress);

            // Assert
            Assert.Equal(expected, result);
        }

        #endregion // end: Contains(IPAddress)

        #region Contains(Subnet)

        [Theory]
        [InlineData(true, "192.168.0.0/16", "192.168.0.0/16")]
        [InlineData(true, "192.168.0.0/16", "192.168.0.0/32")]
        [InlineData(false, "192.168.0.0/16", "192.168.0.0/8")]
        [InlineData(false, "192.168.0.0/16", null)]
        public void Contains_Subnet_Test(bool expected, string subnetString, string containsSubnetString)
        {
            // Arrange
            var subnet = Subnet.Parse(subnetString);
            var containsSubnet = containsSubnetString != null ? Subnet.Parse(containsSubnetString) : null;

            // Act
            var result = subnet.Contains(containsSubnet);

            // Arrange
            Assert.Equal(expected, result);
        }

        [Fact]
        public void Ipv4SubnetDoesNotContainIPv6Test()
        {
            // Arrange
            var subnet = Subnet.Parse("0.0.0.0/0");

            // Act

            // Assert
            Assert.False(subnet.Contains(IPAddress.Parse("::")));
        }

        #endregion // end: Contains(Subnet)

        #endregion // end: Contains

        #region TryIPv4FromPartial

        public static IEnumerable<object[]> TryIPv4FromPartial_Test_Values()
        {
            yield return new object[] { null, null };
            yield return new object[] { null, string.Empty };
            yield return new object[] { null, "potato" };
            yield return new object[] { Subnet.Parse("192.0.0.0/8"), "192" };
            yield return new object[] { Subnet.Parse("192.0.0.0/8"), "192." };
            yield return new object[] { Subnet.Parse("192.168.0.0/16"), "192.168" };
            yield return new object[] { Subnet.Parse("192.168.0.0/16"), "192.168." };
            yield return new object[] { Subnet.Parse("192.168.1.0/24"), "192.168.1" };
            yield return new object[] { Subnet.Parse("192.168.1.0/24"), "192.168.1." };
            yield return new object[] { Subnet.Parse("192.168.1.1/32"), "192.168.1.1" };
            yield return new object[] { null, "192.168.1.1." };
            yield return new object[] { null, "192.168.0.1.5" };
            yield return new object[] { null, "10.209.005.029" }; // Addresses #59 - [BUG] Subnet.TryIPv4FromPartial(string , out Subnet) throws FormatException if the string input contains a malformed 0-prefixed octet that is not a valid octal number.
        }

        [Theory]
        [MemberData(nameof(TryIPv4FromPartial_Test_Values))]
        public void TryIPv4FromPartial_Test(Subnet expected, string input)
        {
            // Arrange

            // Act
            var success = Subnet.TryIPv4FromPartial(input, out var subnet);

            // Assert
            Assert.Equal(expected != null, success);
            Assert.Equal(expected, subnet);
        }

        #endregion // end: TryIPv4FromPartial

        #region TryIPv6FromPartial

        public static IEnumerable<object[]> TryIPv6FromPartial_Test_Values()
        {
            // bad input formats
            yield return new object[] { Enumerable.Empty<Subnet>(), null };
            yield return new object[] { Enumerable.Empty<Subnet>(), string.Empty };
            yield return new object[] { Enumerable.Empty<Subnet>(), "potato" };

            // invalid input
            yield return new object[] { Enumerable.Empty<Subnet>(), ":" };
            yield return new object[] { Enumerable.Empty<Subnet>(), ":::" };
            yield return new object[] { Enumerable.Empty<Subnet>(), "0:0:0:0:0:0:0:0:0" }; // too many hextets
            yield return new object[] { Enumerable.Empty<Subnet>(), "0:0:0:0:0:0:0:0::" };

            // invalid input, multiple "::"
            yield return new object[] { Enumerable.Empty<Subnet>(), "::0::" };
            yield return new object[] { Enumerable.Empty<Subnet>(), "0::0::0" };
            yield return new object[] { Enumerable.Empty<Subnet>(), "::0:0:0::" };

            // explicit valid subnets
            yield return new object[] { new[] { Subnet.Parse("::/128") }, "::/128" };
            yield return new object[] { new[] { Subnet.Parse("2001:db8:85a3:42::/64") }, "2001:db8:85a3:42::/64" };
            yield return new object[]
            {
                new[] { Subnet.Parse("2001:0db8:85a3:0042:1000:0001:0370:7334/128") },
                "2001:0db8:85a3:0042:1000:0001:0370:7334/128",
            };

            for (var hextetCount = 0; hextetCount <= 8; hextetCount++)
            {
                var sb = new StringBuilder();

                sb.Append(string.Join(":", Enumerable.Repeat("0", hextetCount)));

                if (hextetCount < 8)
                {
                    sb.Append("::");
                }

                var subnets = new List<Subnet>();
                for (var i = 0; i <= 8 - hextetCount; i++)
                {
                    subnets.Add(Subnet.Parse($"::/{128 - (16 * i)}"));
                }

                yield return new object[] { subnets, sb.ToString() };
            }

            var hextets = "2001:0db8:85a3:0042:1000:0001:0370:7334".Split(':');

            for (var hextetCount = 1; hextetCount <= 8; hextetCount++)
            {
                var subnets = new List<Subnet>();
                for (var i = 8 - hextetCount; i >= 0; i--)
                {
                    var enumerable = hextets.Take(hextetCount).Select(s => new string(s.SkipWhile(c => c == '0').ToArray()));
                    var trimmedLeadingZero = string.Join(":", enumerable);
                    var subnet =
                        hextetCount < 8
                            ? Subnet.Parse($"{trimmedLeadingZero}::/{128 - (16 * i)}")
                            : Subnet.Parse($"{trimmedLeadingZero}");

                    subnets.Add(subnet);
                }

                var inputString = string.Join(":", hextets.Take(hextetCount));

                if (hextetCount < 8)
                {
                    yield return new object[] { subnets, $"{inputString}::" };
                    yield return new object[] { subnets, $"{inputString}:" };
                }

                if (
                    !string.IsNullOrEmpty(inputString) // no match for an empty string
                    && inputString != hextets[0]
                ) // TODO should this work, a hextet w/o any ':'? Unsure
                {
                    yield return new object[] { subnets, inputString };
                }
            }
        }

        [Theory]
        [MemberData(nameof(TryIPv6FromPartial_Test_Values))]
        public void TryIPv6FromPartial_Test(IEnumerable<Subnet> expected, string input)
        {
            // Arrange
            // Act
#pragma warning disable CS0618 // testing a known obsolete method; ignoring the fact that it is obsolete
            var success = Subnet.TryIPv6FromPartial(input, out var subnets);
#pragma warning restore CS0618

            // Assert
            var expectedList = expected.ToList();
            var subnetList = subnets.ToList();

            Assert.Equal(expectedList.Any(), success);
            Assert.Equal(expectedList.Count, subnetList.Count);
            Assert.All(subnetList, subnet => Assert.Contains(subnet, expectedList));
        }

        #endregion // end: TryIPv6FromPartial

        #region Ctor

        #region Ctor(IPAddress)

        public static IEnumerable<object[]> Ctor_IPAddress_Test_Values()
        {
            foreach (var ipAddress in IPv4Addresses())
            {
                var subnet = new Subnet(ipAddress, 32);
                yield return new object[] { subnet, ipAddress };
            }

            foreach (var ipAddress in IPv6Addresses())
            {
                var subnet = new Subnet(ipAddress, 128);
                yield return new object[] { subnet, ipAddress };
            }

            IEnumerable<IPAddress> IPv4Addresses()
            {
                yield return IPAddress.Any;
                yield return IPAddress.Loopback;
                yield return IPAddress.None;
                yield return IPAddress.Parse("192.168.1.1");
            }

            IEnumerable<IPAddress> IPv6Addresses()
            {
                yield return IPAddress.IPv6Any;
                yield return IPAddress.IPv6Loopback;
                yield return IPAddress.Parse("2001:0db8:85a3:0042:1000:8a2e:0370:7334");
            }
        }

        [Theory]
        [MemberData(nameof(Ctor_IPAddress_Test_Values))]
        public void Ctor_IPAddress_Test(Subnet expected, IPAddress address)
        {
            // Arrange
            // Act
            var subnet = new Subnet(address);

            // Assert
            Assert.Equal(expected, subnet);
        }

        [Fact]
        public void Ctor_IPAddress_NullIPAddress_Throws_ArgumentNullException_Test()
        {
            Assert.Throws<ArgumentNullException>(() => new Subnet(null));
        }

        #endregion // end: Ctor(IPAddress)

        #region Ctor(IPAddress, int)

        [Fact]
        public void Ctor_IPAddress_Int_NullIPAddress_Throws_ArgumentNullException_Test()
        {
            Assert.Throws<ArgumentNullException>(() => new Subnet(null, 42));
        }

        [Theory]
        [InlineData("192.168.1.1", -1)]
        [InlineData("192.168.1.1", 33)]
        [InlineData("::", -1)]
        [InlineData("::", 129)]
        public void Ctor_IPAddress_Int_IntOutOfRange_Throws_ArgumentNullException_Test(string address, int routingPrefix)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Subnet(IPAddress.Parse(address), routingPrefix));
        }

        #endregion

        #region Ctor(IPAddress, IPAddress)

        public static IEnumerable<object[]> Ctor_IPAddress_IPAddress_Test_Values()
        {
            yield return new object[]
            {
                new Subnet(IPAddress.Parse("0.0.0.0"), 0),
                IPAddress.Parse("0.0.0.0"),
                IPAddress.Parse("255.255.255.255"),
            };
            yield return new object[]
            {
                new Subnet(IPAddress.Parse("::"), 0),
                IPAddress.Parse("::"),
                IPAddress.Parse("FFFF:FFFF:FFFF:FFFF:FFFF:FFFF:FFFF:FFFF"),
            };

            foreach (var address in IPv4Addresses())
            {
                for (var routePrefix = 0; routePrefix <= 32; routePrefix++)
                {
                    var subnet = new Subnet(address, routePrefix);
                    yield return new object[] { subnet, subnet.Head, subnet.Tail };
                }
            }

            foreach (var address in IPv6Addresses())
            {
                for (var routePrefix = 0; routePrefix <= 128; routePrefix++)
                {
                    var subnet = new Subnet(address, routePrefix);
                    yield return new object[] { subnet, subnet.Head, subnet.Tail };
                }
            }

            IEnumerable<IPAddress> IPv4Addresses()
            {
                yield return IPAddress.Any;
                yield return IPAddress.Loopback;
                yield return IPAddress.None;
                yield return IPAddress.Parse("192.168.1.1");
            }

            IEnumerable<IPAddress> IPv6Addresses()
            {
                yield return IPAddress.IPv6Any;
                yield return IPAddress.IPv6Loopback;
                yield return IPAddress.Parse("2001:0db8:85a3:0042:1000:8a2e:0370:7334");
            }
        }

        [Theory]
        [MemberData(nameof(Ctor_IPAddress_IPAddress_Test_Values))]
        public void Ctor_IPAddress_IPAddress_Test(Subnet expected, IPAddress primary, IPAddress secondary)
        {
            // Arrange
            // Act
            var result = new Subnet(primary, secondary);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("192.168.3.25", null)]
        [InlineData(null, "192.168.3.25")]
        [InlineData("::", null)]
        [InlineData(null, "::")]
        [InlineData(null, null)]
        public void Ctor_IPAddress_IPAddress_Null_Input_Throws_ArgumentNullException_Test(string primary, string secondary)
        {
            // Arrange
            _ = IPAddress.TryParse(primary, out var primaryAddress);
            _ = IPAddress.TryParse(secondary, out var secondaryAddress);

            // Act
            // Assert
            Assert.Throws<ArgumentNullException>(() => new Subnet(primaryAddress, secondaryAddress));
        }

        [Theory]
        [InlineData("192.168.3.25", "192.168.3.0")]
        [InlineData("ff::dc", "::")]
        public void Ctor_IPAddress_IPAddress_Input_Invalid_Ordering_Throws_InvalidOperationException_Test(
            string primary,
            string secondary
        )
        {
            // Arrange
            var primaryAddress = IPAddress.Parse(primary);
            var secondaryAddress = IPAddress.Parse(secondary);

            // Act
            // Assert
            Assert.Throws<InvalidOperationException>(() => new Subnet(primaryAddress, secondaryAddress));
        }

        [Theory]
        [InlineData("192.168.3.25", "2001:0db8:85a3:0042:1000:8a2e:0370:7334")]
        [InlineData("2001:0db8:85a3:0042:1000:8a2e:0370:7334", "192.168.3.25")]
        public void Ctor_IPAddress_IPAddress_MismatchAddressFamily_Throws_ArgumentException_Test(
            string primary,
            string secondary
        )
        {
            // Arrange
            var primaryAddress = IPAddress.Parse(primary);
            var secondaryAddress = IPAddress.Parse(secondary);

            // Act
            // Assert
            Assert.Throws<ArgumentException>(() => new Subnet(primaryAddress, secondaryAddress));
        }

        #endregion // end: Ctor(IPAddress, IPAddress)

        #endregion // end: Ctor

        #region ISerializable
#if NET48   // maintained for .NET 4.8 compatibility
        public static IEnumerable<object[]> CanSerializable_Test_Values()
        {
            yield return new object[] { new Subnet(IPAddress.Parse("192.168.1.0")) };
            yield return new object[] { new Subnet(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("192.168.1.255")) };
            yield return new object[] { new Subnet(IPAddress.Parse("::"), IPAddress.Parse("::FFFF")) };
        }

        [Theory]
        [MemberData(nameof(CanSerializable_Test_Values))]
        public void CanSerializable_Test(Subnet subnet)
        {
            // Arrange
            var formatter = new BinaryFormatter();

            // Act
            using (var writeStream = new MemoryStream())
            {
                formatter.Serialize(writeStream, subnet);
                writeStream.Seek(0, SeekOrigin.Begin);

                // Deserialize the object from the stream
                var result = formatter.Deserialize(writeStream);

                // Assert
                var actual = Assert.IsType<Subnet>(result);

                // using explicit EqualityComparer to avoid comparing elements of enumerable
                Assert.Equal(subnet, actual, SubnetEqualityComparer.Instance);
            }
        }
#endif
        #endregion end: ISerializable

        #region Static Factory Methods

        #region FromBytes

        public static IEnumerable<object[]> FromBytes_Bytes_Bytes_Test_Values()
        {
            foreach (var ipAddress in IPv4Addresses())
            {
                for (var i = 0; i <= 32; i++)
                {
                    var subnet = new Subnet(ipAddress, i);
                    yield return new object[] { subnet, subnet.Head.GetAddressBytes(), subnet.Tail.GetAddressBytes() };
                }
            }

            foreach (var ipAddress in IPv6Addresses())
            {
                for (var i = 0; i <= 128; i++)
                {
                    var subnet = new Subnet(ipAddress, i);
                    yield return new object[] { subnet, subnet.Head.GetAddressBytes(), subnet.Tail.GetAddressBytes() };
                }
            }

            IEnumerable<IPAddress> IPv4Addresses()
            {
                yield return IPAddress.Any;
                yield return IPAddress.Loopback;
                yield return IPAddress.None;
                yield return IPAddress.Parse("192.168.1.1");
            }

            IEnumerable<IPAddress> IPv6Addresses()
            {
                yield return IPAddress.IPv6Any;
                yield return IPAddress.IPv6Loopback;
                yield return IPAddress.Parse("2001:0db8:85a3:0042:1000:8a2e:0370:7334");
            }
        }

        [Theory]
        [MemberData(nameof(FromBytes_Bytes_Bytes_Test_Values))]
        public void FromBytes_Bytes_Bytes_Test(Subnet expected, byte[] lowAddressBytes, byte[] highAddressBytes)
        {
            // Arrange
            // Act
            var subnet = Subnet.FromBytes(lowAddressBytes, highAddressBytes);

            // Assert
            Assert.Equal(expected, subnet);
        }

        [Theory]
        [InlineData(null, new byte[] { 0x01, 0x01, 0xA8, 0xC0 })]
        [InlineData(new byte[] { 0x01, 0x01, 0xA8, 0xC0 }, null)]
        [InlineData(null, new byte[] { })]
        public void FromBytes_Null_Input_Throws_ArgumentNullException_Test(byte[] lowAddressBytes, byte[] highAddressBytes)
        {
            // Arrange
            // Act
            // Assert
            Assert.Throws<ArgumentNullException>(() => Subnet.FromBytes(lowAddressBytes, highAddressBytes));
        }

        [Theory]
        [InlineData(new byte[] { 0x01, 0x01, 0xA8, 0xC0, 0xFF }, new byte[] { 0x01, 0x01, 0xA8, 0xC0 })]
        [InlineData(new byte[] { 0x01, 0x01, 0xA8, 0xC0 }, new byte[] { 0x01, 0x01, 0xA8, 0xC0, 0xFF })]
        [InlineData(new byte[] { }, new byte[] { 0x01, 0x01, 0xA8, 0xC0, 0xFF })]
        [InlineData(new byte[] { 0x01, 0x01, 0xA8, 0xC0, 0xFF }, new byte[] { })]
        public void FromBytes_Invalid_Input_Throws_ArgumentException_Test(byte[] lowAddressBytes, byte[] highAddressBytes)
        {
            // Arrange
            // Act
            // Assert
            var exception = Assert.Throws<ArgumentException>(() => Subnet.FromBytes(lowAddressBytes, highAddressBytes));
            Assert.IsType<ArgumentException>(exception.InnerException);
        }

        #endregion // end: FromBytes

        #region TryFromBytes

        public static IEnumerable<object[]> TryFromBytes_Bytes_Bytes_Test_Values()
        {
            yield return new object[] { false, null, null, null };
            yield return new object[] { false, null, IPAddress.Any.GetAddressBytes(), IPAddress.IPv6Any.GetAddressBytes() };
            yield return new object[] { false, null, Array.Empty<byte>(), IPAddress.IPv6Any.GetAddressBytes() };
            yield return new object[] { false, null, Array.Empty<byte>(), IPAddress.Any.GetAddressBytes() };
            yield return new object[] { false, null, IPAddress.IPv6Any.GetAddressBytes(), Array.Empty<byte>() };
            yield return new object[] { false, null, IPAddress.Any.GetAddressBytes(), Array.Empty<byte>() };

            yield return new object[] { false, null, new byte[] { 0x00 }, IPAddress.IPv6Any.GetAddressBytes() };
            yield return new object[] { false, null, new byte[] { 0x00 }, IPAddress.Any.GetAddressBytes() };
            yield return new object[] { false, null, IPAddress.IPv6Any.GetAddressBytes(), new byte[] { 0x00 } };
            yield return new object[] { false, null, IPAddress.Any.GetAddressBytes(), new byte[] { 0x00 } };

            foreach (var ipAddress in IPv4Addresses())
            {
                for (var i = 0; i <= 32; i++)
                {
                    var subnet = new Subnet(ipAddress, i);
                    yield return new object[] { true, subnet, subnet.Head.GetAddressBytes(), subnet.Tail.GetAddressBytes() };
                }
            }

            foreach (var ipAddress in IPv6Addresses())
            {
                for (var i = 0; i <= 128; i++)
                {
                    var subnet = new Subnet(ipAddress, i);
                    yield return new object[] { true, subnet, subnet.Head.GetAddressBytes(), subnet.Tail.GetAddressBytes() };
                }
            }

            IEnumerable<IPAddress> IPv4Addresses()
            {
                yield return IPAddress.Any;
                yield return IPAddress.Loopback;
                yield return IPAddress.None;
                yield return IPAddress.Parse("192.168.1.1");
            }

            IEnumerable<IPAddress> IPv6Addresses()
            {
                yield return IPAddress.IPv6Any;
                yield return IPAddress.IPv6Loopback;
                yield return IPAddress.Parse("2001:0db8:85a3:0042:1000:8a2e:0370:7334");
            }
        }

        [Theory]
        [MemberData(nameof(TryFromBytes_Bytes_Bytes_Test_Values))]
        public void TryFromBytes_Bytes_Bytes_Test(
            bool expectedSuccess,
            Subnet expectedSubnet,
            byte[] lowAddressBytes,
            byte[] highAddressBytes
        )
        {
            // Arrange
            // Act
            var success = Subnet.TryFromBytes(lowAddressBytes, highAddressBytes, out var subnet);

            // Assert
            Assert.Equal(expectedSuccess, success);
            Assert.Equal(expectedSubnet, subnet);
        }

        #endregion // end: TryFromBytes

        #region Parse(string)

        public static IEnumerable<object[]> Parse_String_Test_Values()
        {
            foreach (var ipAddress in IPv4Addresses())
            {
                for (var i = 0; i <= 32; i++)
                {
                    var subnet = new Subnet(ipAddress, i);
                    yield return new object[] { subnet, $"{ipAddress}/{i}" };
                }
            }

            foreach (var ipAddress in IPv6Addresses())
            {
                for (var i = 0; i <= 128; i++)
                {
                    var subnet = new Subnet(ipAddress, i);
                    yield return new object[] { subnet, $"{subnet}" };
                }
            }

            IEnumerable<IPAddress> IPv4Addresses()
            {
                yield return IPAddress.Any;
                yield return IPAddress.Loopback;
                yield return IPAddress.None;
                yield return IPAddress.Parse("192.168.1.1");
            }

            IEnumerable<IPAddress> IPv6Addresses()
            {
                yield return IPAddress.IPv6Any;
                yield return IPAddress.IPv6Loopback;
                yield return IPAddress.Parse("2001:0db8:85a3:0042:1000:8a2e:0370:7334");
            }
        }

        [Theory]
        [MemberData(nameof(Parse_String_Test_Values))]
        public void Parse_String_Test(Subnet expected, string input)
        {
            // Arrange

            // Act
            var subnet = Subnet.Parse(input);

            // Assert
            Assert.Equal(expected, subnet);
        }

        [Fact]
        public void Parse_Failure_Throws_FormatException_Test()
        {
            // Arrange
            // Act
            // Assert
            Assert.Throws<FormatException>(() => Subnet.Parse("potato"));
        }

        #endregion // end: Parse(string)

        #region TryParse(string)

        public static IEnumerable<object[]> TryParse_String_Test_Values()
        {
            yield return new object[] { null, null };
            yield return new object[] { null, string.Empty };
            yield return new object[] { null, "potato" };
            yield return new object[] { null, "2001:0db8:85a3:0042:1000:8a2e:0370:7334/129" };
            yield return new object[] { null, "0.0.0.0/33" };
            yield return new object[] { null, "0.0.0.0/potato" };
            yield return new object[] { null, "potato/16" };
            yield return new object[] { null, "0.0.0.0/" };
            yield return new object[] { null, "/32" };

            foreach (var ipAddress in IPv4Addresses())
            {
                for (var i = 0; i <= 32; i++)
                {
                    var subnet = new Subnet(ipAddress, i);
                    yield return new object[] { subnet, $"{ipAddress}/{i}" };
                }

                yield return new object[] { new Subnet(ipAddress, ipAddress), ipAddress.ToString() };
            }

            foreach (var ipAddress in IPv6Addresses())
            {
                for (var i = 0; i <= 128; i++)
                {
                    var subnet = new Subnet(ipAddress, i);
                    yield return new object[] { subnet, $"{subnet}" };
                }

                yield return new object[] { new Subnet(ipAddress, ipAddress), ipAddress.ToString() };
            }

            IEnumerable<IPAddress> IPv4Addresses()
            {
                yield return IPAddress.Any;
                yield return IPAddress.Loopback;
                yield return IPAddress.None;
                yield return IPAddress.Parse("192.168.1.1");
            }

            IEnumerable<IPAddress> IPv6Addresses()
            {
                yield return IPAddress.IPv6Any;
                yield return IPAddress.IPv6Loopback;
                yield return IPAddress.Parse("2001:0db8:85a3:0042:1000:8a2e:0370:7334");
            }
        }

        [Theory]
        [MemberData(nameof(TryParse_String_Test_Values))]
        public void TryParse_String_Test(Subnet expected, string input)
        {
            // Arrange

            // Act
            var success = Subnet.TryParse(input, out var subnet);

            // Assert
            Assert.Equal(expected != null, success);
            Assert.Equal(expected, subnet);
        }

        #endregion // end: TryParse(string)

        #region Parse(string, int)

        public static IEnumerable<object[]> Parse_String_Int_Test_Values()
        {
            foreach (var ipAddress in IPv4Addresses())
            {
                for (var i = 0; i <= 32; i++)
                {
                    var subnet = new Subnet(ipAddress, i);
                    yield return new object[] { subnet, subnet.Head.ToString(), i };
                }
            }

            foreach (var ipAddress in IPv6Addresses())
            {
                for (var i = 0; i <= 128; i++)
                {
                    var subnet = new Subnet(ipAddress, i);
                    yield return new object[] { subnet, subnet.Head.ToString(), i };
                }
            }

            IEnumerable<IPAddress> IPv4Addresses()
            {
                yield return IPAddress.Any;
                yield return IPAddress.Loopback;
                yield return IPAddress.None;
                yield return IPAddress.Parse("192.168.1.1");
            }

            IEnumerable<IPAddress> IPv6Addresses()
            {
                yield return IPAddress.IPv6Any;
                yield return IPAddress.IPv6Loopback;
                yield return IPAddress.Parse("2001:0db8:85a3:0042:1000:8a2e:0370:7334");
            }
        }

        [Theory]
        [MemberData(nameof(Parse_String_Int_Test_Values))]
        public void Parse_String_Int_Test(Subnet expected, string addressString, int routePrefix)
        {
            // Arrange
            // Act
            var subnet = Subnet.Parse(addressString, routePrefix);

            // Assert
            Assert.Equal(expected, subnet);
        }

        #endregion // end: Parse(string, int)

        #region TryParse(string, int)

        public static IEnumerable<object[]> TryParse_String_Int_Test_Values()
        {
            yield return new object[] { false, null, null, 0 };
            yield return new object[] { false, null, "potato", 0 };
            yield return new object[] { false, null, IPAddress.Any.ToString(), -5 };
            yield return new object[] { false, null, IPAddress.IPv6Any.ToString(), -5 };
            yield return new object[] { false, null, IPAddress.Any.ToString(), 33 };
            yield return new object[] { false, null, IPAddress.IPv6Any.ToString(), 129 };
            yield return new object[] { false, null, null, 0 };

            foreach (var ipAddress in IPv4Addresses())
            {
                for (var i = 0; i <= 32; i++)
                {
                    var subnet = new Subnet(ipAddress, i);
                    yield return new object[] { true, subnet, subnet.Head.ToString(), i };
                }
            }

            foreach (var ipAddress in IPv6Addresses())
            {
                for (var i = 0; i <= 128; i++)
                {
                    var subnet = new Subnet(ipAddress, i);
                    yield return new object[] { true, subnet, subnet.Head.ToString(), i };
                }
            }

            IEnumerable<IPAddress> IPv4Addresses()
            {
                yield return IPAddress.Any;
                yield return IPAddress.Loopback;
                yield return IPAddress.None;
                yield return IPAddress.Parse("192.168.1.1");
            }

            IEnumerable<IPAddress> IPv6Addresses()
            {
                yield return IPAddress.IPv6Any;
                yield return IPAddress.IPv6Loopback;
                yield return IPAddress.Parse("2001:0db8:85a3:0042:1000:8a2e:0370:7334");
            }
        }

        [Theory]
        [MemberData(nameof(TryParse_String_Int_Test_Values))]
        public void TryParse_String_Int_Test(bool expectedSuccess, Subnet expectedSubnet, string addressString, int routePrefix)
        {
            // Arrange
            // Act
            var success = Subnet.TryParse(addressString, routePrefix, out var subnet);

            // Assert
            Assert.Equal(expectedSuccess, success);
            Assert.Equal(expectedSubnet, subnet);
        }

        #endregion // end: TryParse(string, int)

        #region Parse(string, string)

        public static IEnumerable<object[]> Parse_String_String_Test_Values()
        {
            foreach (var ipAddress in IPv4Addresses())
            {
                for (var i = 0; i <= 32; i++)
                {
                    var subnet = new Subnet(ipAddress, i);
                    yield return new object[] { subnet, subnet.Head.ToString(), subnet.Tail.ToString() };
                }
            }

            foreach (var ipAddress in IPv6Addresses())
            {
                for (var i = 0; i <= 128; i++)
                {
                    var subnet = new Subnet(ipAddress, i);
                    yield return new object[] { subnet, subnet.Head.ToString(), subnet.Tail.ToString() };
                }
            }

            IEnumerable<IPAddress> IPv4Addresses()
            {
                yield return IPAddress.Any;
                yield return IPAddress.Loopback;
                yield return IPAddress.None;
                yield return IPAddress.Parse("192.168.1.1");
            }

            IEnumerable<IPAddress> IPv6Addresses()
            {
                yield return IPAddress.IPv6Any;
                yield return IPAddress.IPv6Loopback;
                yield return IPAddress.Parse("2001:0db8:85a3:0042:1000:8a2e:0370:7334");
            }
        }

        [Theory]
        [MemberData(nameof(Parse_String_String_Test_Values))]
        public void Parse_String_String_Test(Subnet expected, string low, string high)
        {
            // Arrange
            // Act
            var subnet = Subnet.Parse(low, high);

            // Assert
            Assert.Equal(expected, subnet);
        }

        [Theory]
        [InlineData("::", null)]
        [InlineData(null, "::")]
        [InlineData("192.168.1.1", null)]
        [InlineData(null, "192.168.1.1")]
        [InlineData(null, null)]
        public void Parse_String_String_NullAddressString_Throws_ArgumentNullException_Test(string low, string high)
        {
            // Arrange
            // Act
            // Assert
            Assert.Throws<ArgumentNullException>(() => Subnet.Parse(low, high));
        }

        [Theory]
        [InlineData("::", "potato")]
        [InlineData("potato", "::")]
        [InlineData("192.168.1.1", "potato")]
        [InlineData("potato", "192.168.1.1")]
        public void Parse_String_String_BadAddressFormat_Throws_ArgumentException_Test(string low, string high)
        {
            // Arrange
            // Act
            // Assert
            Assert.Throws<FormatException>(() => Subnet.Parse(low, high));
        }

        [Theory]
        [InlineData("192.168.1.1", "::")]
        [InlineData("::", "192.168.1.1")]
        public void Parse_String_String_MisMatchAddressFamily_Throws_ArgumentException_Test(string low, string high)
        {
            // Arrange
            // Act
            // Assert
            Assert.Throws<ArgumentException>(() => Subnet.Parse(low, high));
        }

        [Theory]
        [InlineData("192.168.1.32", "192.168.1.0")]
        [InlineData("::20", "::")]
        public void Parse_String_String_InvalidRange_Throws_InvalidOperationException_Test(string low, string high)
        {
            // Arrange
            // Act
            // Assert
            Assert.Throws<InvalidOperationException>(() => Subnet.Parse(low, high));
        }

        #endregion Parse(string, string)

        #region TryParse(string, string)

        public static IEnumerable<object[]> TryParse_String_String_Test_Values()
        {
            foreach (var s in new[] { null, string.Empty, "\t", "potato" })
            {
                yield return new object[] { null, "192.168.1.1", s };
                yield return new object[] { null, s, "192.168.1.1" };
                yield return new object[] { null, "2001:0db8:85a3:0042:1000:8a2e:0370:7334", s };
                yield return new object[] { null, s, "2001:0db8:85a3:0042:1000:8a2e:0370:7334" };
            }

            foreach (var ipAddress in IPv4Addresses())
            {
                for (var i = 0; i <= 32; i++)
                {
                    var subnet = new Subnet(ipAddress, i);
                    yield return new object[] { subnet, subnet.Head.ToString(), subnet.Tail.ToString() };
                }
            }

            foreach (var ipAddress in IPv6Addresses())
            {
                for (var i = 0; i <= 128; i++)
                {
                    var subnet = new Subnet(ipAddress, i);
                    yield return new object[] { subnet, subnet.Head.ToString(), subnet.Tail.ToString() };
                }
            }

            IEnumerable<IPAddress> IPv4Addresses()
            {
                yield return IPAddress.Any;
                yield return IPAddress.Loopback;
                yield return IPAddress.None;
                yield return IPAddress.Parse("192.168.1.1");
            }

            IEnumerable<IPAddress> IPv6Addresses()
            {
                yield return IPAddress.IPv6Any;
                yield return IPAddress.IPv6Loopback;
                yield return IPAddress.Parse("2001:0db8:85a3:0042:1000:8a2e:0370:7334");
            }
        }

        [Theory]
        [MemberData(nameof(TryParse_String_String_Test_Values))]
        public void TryParse_String_String_Test(Subnet expected, string low, string high)
        {
            // Arrange

            // Act
            var success = Subnet.TryParse(low, high, out var subnet);

            // Assert
            Assert.Equal(expected != null, success);
            Assert.Equal(expected, subnet);
        }

        #endregion // end: TryParse(string, string)

        #endregion // end: Static Factory Methods

        #region Length / TryGetLength

        public static IEnumerable<object[]> Length_Test_Values()
        {
            foreach (var ipAddress in IPv4Addresses())
            {
                for (var i = 0; i <= 32; i++)
                {
                    var routePrefix = 32 - i;
                    var length = BigInteger.Pow(2, routePrefix);

                    var subnet = new Subnet(ipAddress, i);
                    yield return new object[] { length, subnet };
                }
            }

            foreach (var ipAddress in IPv6Addresses())
            {
                for (var i = 0; i <= 128; i++)
                {
                    var routePrefix = 128 - i;
                    var length = BigInteger.Pow(2, routePrefix);

                    var subnet = new Subnet(ipAddress, i);
                    yield return new object[] { length, subnet };
                }
            }

            IEnumerable<IPAddress> IPv4Addresses()
            {
                yield return IPAddress.Parse("192.168.1.1");
            }

            IEnumerable<IPAddress> IPv6Addresses()
            {
                yield return IPAddress.Parse("2001:0db8:85a3:0042:1000:8a2e:0370:7334");
            }
        }

        [Theory]
        [MemberData(nameof(Length_Test_Values))]
        public static void Length_Test(BigInteger expected, Subnet subnet)
        {
            // Arrange
            // Act

            var result = subnet.Length;

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [MemberData(nameof(Length_Test_Values))]
        public static void TryGetLength_Integer_Test(BigInteger expected, Subnet subnet)
        {
            // Arrange
            // Act
            var success = subnet.TryGetLength(out int length);

            // Assert
            Assert.Equal(expected <= int.MaxValue, success);
            Assert.Equal(expected <= int.MaxValue ? (int)expected : -1, length);
        }

        [Theory]
        [MemberData(nameof(Length_Test_Values))]
        public static void TryGetLength_Long_Test(BigInteger expected, Subnet subnet)
        {
            // Arrange
            // Act
            var success = subnet.TryGetLength(out long length);

            // Assert
            Assert.Equal(expected <= long.MaxValue, success);
            Assert.Equal(expected <= long.MaxValue ? (long)expected : -1, length);
        }

        #endregion // end: Length

        #region Equals

        #region Equals(Subnet)

        public static IEnumerable<object[]> Equals_Subnet_Test_Values()
        {
            foreach (var ipAddress in IPv4Addresses())
            {
                for (var i = 0; i <= 32; i++)
                {
                    yield return new object[] { true, new Subnet(ipAddress, i), new Subnet(ipAddress, i) }; // equivalent

                    yield return new object[] { false, new Subnet(ipAddress, i), new Subnet(ipAddress, (i + 2) % 32) }; // differing routes equivalent

                    var subnet = new Subnet(ipAddress, i);
                    yield return new object[] { true, subnet, subnet }; // same

                    foreach (var ipv6Address in IPv6Addresses().Take(2))
                    {
                        yield return new object[] { false, new Subnet(ipAddress, i), new Subnet(ipv6Address, i) }; // different families
                    }
                }
            }

            foreach (var ipAddress in IPv6Addresses())
            {
                for (var i = 0; i <= 128; i++)
                {
                    yield return new object[] { true, new Subnet(ipAddress, i), new Subnet(ipAddress, i) }; // equivalent

                    yield return new object[] { false, new Subnet(ipAddress, i), new Subnet(ipAddress, (i + 2) % 128) }; // differing routes equivalent

                    var subnet = new Subnet(ipAddress, i);
                    yield return new object[] { true, subnet, subnet }; // same
                }
            }

            IEnumerable<IPAddress> IPv4Addresses()
            {
                yield return IPAddress.Any;
                yield return IPAddress.Loopback;
                yield return IPAddress.None;
                yield return IPAddress.Parse("192.168.1.1");
            }

            IEnumerable<IPAddress> IPv6Addresses()
            {
                yield return IPAddress.IPv6Any;
                yield return IPAddress.IPv6Loopback;
                yield return IPAddress.Parse("2001:0db8:85a3:0042:1000:8a2e:0370:7334");
            }
        }

        [Theory]
        [MemberData(nameof(Equals_Subnet_Test_Values))]
        public void Equals_Subnet_Test(bool expected, Subnet subnetA, Subnet subnetB)
        {
            // Arrange

            // Act
            var result = subnetA.Equals(subnetB);

            // Assert
            Assert.Equal(expected, result);
            Assert.Equal(result, expected);
        }

        #endregion // end: Equals(Subnet)

        #region Equals(object)

        [Theory]
        [MemberData(nameof(Equals_Subnet_Test_Values))]
        public void Equals_Object_Test(bool expected, Subnet subnetA, object subnetB)
        {
            // Arrange

            // Act
            var result = subnetA.Equals(subnetB);

            // Assert
            Assert.Equal(expected, result);
        }

        #endregion // end: Equals(object)

        #endregion // end: Equals

        #region FromNetMask

        public static IEnumerable<object[]> FromNetMask_Test_Values()
        {
            var networkPrefix = IPAddress.Parse("192.168.1.1");

            for (var i = 0; i <= 32; i++)
            {
                var netmaskBytes = Enumerable.Repeat((byte)0xFF, 4).ToArray().ShiftBitsLeft(32 - i);

                var netmask = new IPAddress(netmaskBytes);

                var expected = new Subnet(networkPrefix, i);

                yield return new object[] { expected, networkPrefix, netmask };
            }
        }

        [Theory]
        [MemberData(nameof(FromNetMask_Test_Values))]
        public void FromNetMask_Test(Subnet expected, IPAddress networkPrefix, IPAddress netmask)
        {
            // Arrange
            // Act
            var result = Subnet.FromNetMask(networkPrefix, netmask);

            // Assert
            Assert.Equal(expected, result);
            Assert.Equal(netmask, result.Netmask);
        }

        [Fact]
        public void FromNetMask_AddressNull_Throws_ArgumentNullException_Test()
        {
            // Act
            // Arrange
            // Assert

            Assert.Throws<ArgumentNullException>(() => Subnet.FromNetMask(null, IPAddress.Any));
        }

        [Fact]
        public void FromNetMask_InvalidNetMask_Throws_ArgumentNullException_Test()
        {
            // Act
            // Arrange
            // Assert
            Assert.Throws<ArgumentException>(() => Subnet.FromNetMask(IPAddress.Any, IPAddress.IPv6Any));
        }

        [Fact]
        public void FromNetMask_IPv6Address_Throws_ArgumentNullException_Test()
        {
            // Act
            // Arrange
            // Assert
            Assert.Throws<ArgumentException>(() => Subnet.FromNetMask(IPAddress.IPv6Any, IPAddress.Any));
        }

        [Fact]
        public void FromNetMaskNetMask_NullNetMask_Throws_ArgumentNullException_Test()
        {
            // Act
            // Arrange
            // Assert

            Assert.Throws<ArgumentNullException>(() => Subnet.FromNetMask(IPAddress.Any, null));
        }

        public static IEnumerable<object[]> TryFromNetMask_Test_Values()
        {
            var networkPrefix = IPAddress.Parse("192.168.1.1");

            for (var i = 0; i <= 32; i++)
            {
                var netmaskBytes = Enumerable.Repeat((byte)0xFF, 4).ToArray().ShiftBitsLeft(32 - i);

                var netmask = new IPAddress(netmaskBytes);

                var expected = new Subnet(networkPrefix, i);

                yield return new object[] { true, expected, networkPrefix, netmask };
            }

            yield return new object[] { false, null, null, IPAddress.Parse("192.168.1.1") };
            yield return new object[] { false, null, IPAddress.Parse("192.168.1.1"), null };
            yield return new object[] { false, null, null, IPAddress.Parse("::") };
            yield return new object[] { false, null, IPAddress.Parse("::"), null };
            yield return new object[] { false, null, null, null };
        }

        [Theory]
        [MemberData(nameof(TryFromNetMask_Test_Values))]
        public void TryFromNetMask_Test(bool expectedSuccess, Subnet expectedSubnet, IPAddress networkPrefix, IPAddress netmask)
        {
            // Arrange
            // Act
            var success = Subnet.TryFromNetMask(networkPrefix, netmask, out var subnet);

            // Assert
            Assert.Equal(expectedSuccess, success);
            Assert.Equal(expectedSubnet, subnet);
        }

        #endregion

        #region Formatting

        #region ToString

        [Theory]
        [InlineData("192.168.1.1", "192.168.1.42")]
        [InlineData("::beef", "0123::dead")]
        public void ToString_MatchesGeneralFormat_Test(string headString, string tailString)
        {
            // Arrange
            var head = IPAddress.Parse(headString);
            var tail = IPAddress.Parse(tailString);

            var iPAddressRange = new IPAddressRange(head, tail);

            // Act
            var result = iPAddressRange.ToString();

            // Assert
            Assert.Equal($"{iPAddressRange:G}", result);
        }

        #endregion // end: ToString

        #region ToString(string, IFormatProvider)

        public static IEnumerable<object[]> ToString_Format_Test_Values()
        {
            // general formats
            foreach (var format in new[] { null, string.Empty, "g", "G" })
            {
                foreach (var subnet in Ipv4AddressSubnets().Concat(Ipv6AddressSubnets()))
                {
                    yield return new object[]
                    {
                        $"{subnet.NetworkPrefixAddress}/{subnet.RoutingPrefix}",
                        format,
                        CultureInfo.CurrentCulture,
                        subnet,
                    };
                }
            }

            // "friendly" formats
            foreach (var format in new[] { "f", "F" })
            {
                foreach (var subnet in Ipv4AddressSubnets().Concat(Ipv6AddressSubnets()))
                {
                    yield return new object[]
                    {
                        subnet.IsSingleIP
                            ? $"{subnet.NetworkPrefixAddress}"
                            : $"{subnet.NetworkPrefixAddress}/{subnet.RoutingPrefix}",
                        format,
                        CultureInfo.CurrentCulture,
                        subnet,
                    };
                }
            }

            // range formats
            foreach (var format in new[] { "r", "R" })
            {
                foreach (var subnet in Ipv4AddressSubnets().Concat(Ipv6AddressSubnets()))
                {
                    yield return new object[] { $"{subnet.Head} - {subnet.Tail}", format, CultureInfo.CurrentCulture, subnet };
                }
            }

            IEnumerable<Subnet> Ipv4AddressSubnets()
            {
                yield return new Subnet(IPAddress.Parse("192.168.1.1"), 16);
                yield return new Subnet(IPAddress.Parse("192.168.1.1"), 32);
            }

            IEnumerable<Subnet> Ipv6AddressSubnets()
            {
                yield return new Subnet(IPAddress.Parse("abc:123::beef"), 16);
                yield return new Subnet(IPAddress.Parse("abc:123::beef"), 128);
            }
        }

        [Theory]
        [MemberData(nameof(ToString_Format_Test_Values))]
        public void ToString_Format_Test(string expected, string format, IFormatProvider formatProvider, Subnet subnet)
        {
            // Arrange
            // Act
            var result = subnet.ToString(format, formatProvider);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ToString_UnknownFormat_Throws_FormatException_Test()
        {
            // Arrange
            var range = new Subnet(IPAddress.Parse("192.168.1.1"), 16);

            // Act
            // Assert
            Assert.Throws<FormatException>(() => range.ToString("potato", CultureInfo.CurrentCulture));
        }

        #endregion // end: ToString(string, IFormatProvider)

        #endregion // end: Formatting

        #region GetHashCode

        public static IEnumerable<object[]> GetHashCode_Test_Values()
        {
            // equal
            yield return new object[] { true, new Subnet(IPAddress.Any, 16), new Subnet(IPAddress.Any, 16) };
            yield return new object[] { true, new Subnet(IPAddress.IPv6Any, 16), new Subnet(IPAddress.IPv6Any, 16) };

            // reference equal
            var ipv4Subnet = new Subnet(IPAddress.Any, 16);
            yield return new object[] { true, ipv4Subnet, ipv4Subnet };

            var ipv6Subnet = new Subnet(IPAddress.IPv6Any, 16);
            yield return new object[] { true, ipv6Subnet, ipv6Subnet };

            // expected different
            yield return new object[] { false, ipv4Subnet, ipv6Subnet };
            yield return new object[] { false, new Subnet(IPAddress.Any, 8), new Subnet(IPAddress.Any, 16) };
            yield return new object[] { false, new Subnet(IPAddress.IPv6Any, 8), new Subnet(IPAddress.IPv6Any, 16) };
            yield return new object[] { false, new Subnet(IPAddress.Any, 16), new Subnet(IPAddress.Broadcast, 16) };
            yield return new object[] { false, new Subnet(IPAddress.Parse("::")), new Subnet(IPAddress.Parse("ab::"), 16) };
        }

        [Theory]
        [MemberData(nameof(GetHashCode_Test_Values))]
        public void GetHashCode_Test(bool expectedEqual, Subnet left, Subnet right)
        {
            // Arrange
            // Act
            var leftHash = left.GetHashCode();
            var rightHash = right.GetHashCode();

            // Assert
            Assert.Equal(expectedEqual, leftHash == rightHash);
        }

        #endregion // end: GetHashCode

        #region Deconstruct

        public static IEnumerable<object[]> Deconstruct_Values()
        {
            foreach (var ipAddress in IPv4Addresses())
            {
                for (var i = 0; i <= 32; i++)
                {
                    yield return new object[] { new Subnet(ipAddress, i) };
                }
            }

            foreach (var ipAddress in IPv6Addresses())
            {
                for (var i = 0; i <= 128; i++)
                {
                    yield return new object[] { new Subnet(ipAddress, i) };
                }
            }

            IEnumerable<IPAddress> IPv4Addresses()
            {
                yield return IPAddress.Parse("192.168.1.1");
                yield return IPAddress.Parse("255.255.0.0");
            }

            IEnumerable<IPAddress> IPv6Addresses()
            {
                yield return IPAddress.Parse("2001:0db8:85a3:0042:1000:8a2e:0370:7334");
                yield return IPAddress.Parse("dead:beef::");
            }
        }

        #region Deconstruct(IPAddress, IPAddress, IPAddress, int)

        [Theory]
        [MemberData(nameof(Deconstruct_Values))]
        public void Deconstruct_IPAddress_IPAddress_IPAddress_Int_Test(Subnet subnet)
        {
            // Arrange
            // Act
            var (networkPrefixAddress, broadcastAddress, netmask, routingPrefix) = subnet;

            // Assert
            Assert.Equal(subnet.NetworkPrefixAddress, networkPrefixAddress);
            Assert.Equal(subnet.BroadcastAddress, broadcastAddress);
            Assert.Equal(subnet.Netmask, netmask);
            Assert.Equal(subnet.RoutingPrefix, routingPrefix);
        }

        #endregion // end: Deconstruct(IPAddress, IPAddress, IPAddress, int)

        #region Deconstruct(IPAddress, int)

        [Theory]
        [MemberData(nameof(Deconstruct_Values))]
        public void Deconstruct_IPAddress_Int_Test(Subnet subnet)
        {
            // Arrange
            // Act
            var (networkPrefixAddress, routingPrefix) = subnet;

            // Assert
            Assert.Equal(subnet.NetworkPrefixAddress, networkPrefixAddress);
            Assert.Equal(subnet.RoutingPrefix, routingPrefix);
        }

        #endregion // end: Deconstruct(IPAddress, int)

        #endregion // end: Deconstruct

        internal class SubnetEqualityComparer : IEqualityComparer<Subnet>
        {
            public static readonly SubnetEqualityComparer Instance = new SubnetEqualityComparer();

            public bool Equals(Subnet x, Subnet y)
            {
                if (ReferenceEquals(x, y))
                {
                    return true;
                }

                if (x is null && y is null)
                {
                    return true;
                }

                if (x is null || y is null)
                {
                    return false;
                }

                return x.Equals(y);
            }

            public int GetHashCode(Subnet obj)
            {
                return obj is null ? -1 : obj.GetHashCode();
            }
        }
    }
}
