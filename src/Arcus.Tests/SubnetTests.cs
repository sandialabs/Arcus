using System.Globalization;
using System.Net;
using System.Numerics;
using Arcus.Tests.XunitSerializers;
#if NET48   // maintained for .NET 4.8 compatibility
using System.Runtime.Serialization;
#endif

[assembly: RegisterXunitSerializer(typeof(SubnetXunitSerializer), typeof(Subnet))]
[assembly: RegisterXunitSerializer(typeof(SubnetEnumerableXunitSerializer), typeof(IEnumerable<Subnet>))]
[assembly: RegisterXunitSerializer(typeof(FormatProviderXunitSerializer), typeof(IFormatProvider))]

namespace Arcus.Tests
{
    /// <summary>Unit tests for <see cref="Subnet"/>.</summary>
    public partial class SubnetTests
    {
        #region Addresses

        /// <summary>Verifies that enumerating a subnet's addresses matches converting it to an array.</summary>
        /// <param name="input">CIDR notation string for the subnet under test.</param>
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

        /// <summary>Verifies that <see cref="Subnet"/> is assignable to the expected base types and interfaces.</summary>
        /// <param name="assignableFromType">The type that <see cref="Subnet"/> should be assignable to.</param>
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

        /// <summary>Gets theory data for comparison operator and <see cref="Subnet.CompareTo(Subnet)"/> tests.</summary>
        /// <returns>Parameters: expected comparison sign (int), left subnet (<see cref="Subnet"/>), right subnet (<see cref="Subnet"/>).</returns>
        public static TheoryData<int, Subnet, Subnet> Comparison_Values()
        {
            return new TheoryData<int, Subnet, Subnet>
            {
                { 0, Subnet.Parse("192.168.0.0/16"), Subnet.Parse("192.168.0.0/16") },
                { 0, Subnet.Parse("ab:cd::/64"), Subnet.Parse("ab:cd::/64") },
                { 1, Subnet.Parse("192.168.0.0/16"), null },
                { 1, Subnet.Parse("ab:cd::/64"), null },
                { 1, Subnet.Parse("192.168.0.0/16"), Subnet.Parse("192.168.0.0/20") },
                { -1, Subnet.Parse("192.168.0.0/20"), Subnet.Parse("192.168.0.0/16") },
                { 1, Subnet.Parse("ab:cd::/64"), Subnet.Parse("ab:cd::/96") },
                { -1, Subnet.Parse("ab:cd::/96"), Subnet.Parse("ab:cd::/64") },
                { -1, Subnet.Parse("0.0.0.0/0"), Subnet.Parse("::/0") },
                { 1, Subnet.Parse("::/0"), Subnet.Parse("0.0.0.0/0") },
                { -1, Subnet.Parse("0.0.0.0/32"), Subnet.Parse("::/128") },
                { 1, Subnet.Parse("::/128"), Subnet.Parse("0.0.0.0/32") },
            };
        }

        #endregion // end: CompareTo / Operators

        #region S6 - CompareTo typo fix

        /// <summary>Verifies that <see cref="Subnet.CompareTo(object)"/> uses the correct article ("a" not "an") in the exception message.</summary>
        [Fact]
        public void CompareTo_WithNonSubnet_ExceptionMessageUsesCorrectArticle()
        {
            var subnet = Subnet.Parse("192.168.1.0/24");
            var ex = Assert.Throws<ArgumentException>(() => subnet.CompareTo((object)"not a subnet"));
            Assert.Equal("Object is not a Subnet", ex.Message);
        }

        #endregion // end: S6 - CompareTo typo fix

        #region Netmask

        /// <summary>Verifies that <see cref="Subnet.Netmask"/> is null for an IPv6 subnet.</summary>
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

        /// <summary>Verifies that <see cref="Subnet.Netmask"/> is non-null and correct for an IPv4 subnet.</summary>
        [Fact]
        public void Netmask_NotNull_ForIPv4_Test()
        {
            // Arrange
            var subnet = new Subnet(IPAddress.Parse("192.168.1.0"), 24);

            // Act
            var netmask = subnet.Netmask;

            // Assert
            Assert.NotNull(netmask);
            Assert.Equal(IPAddress.Parse("255.255.255.0"), netmask);
        }

        #endregion // end: Netmask

        #region S9 - IPv6 netmask skip

        /// <summary>Verifies that constructing an IPv6 subnet does not allocate a netmask.</summary>
        [Fact]
        public void IPv6Subnet_Construction_DoesNotAllocateNetmask()
        {
            var subnet = new Subnet(IPAddress.IPv6Loopback, 64);
            Assert.Null(subnet.Netmask);
        }

        #endregion // end: S9 - IPv6 netmask skip

        #region NetworkPrefixAddress / BroadcastAddress

        /// <summary>Verifies that <see cref="Subnet.NetworkPrefixAddress"/> equals the subnet's <see cref="AbstractIPAddressRange.Head"/>.</summary>
        [Fact]
        public void NetworkPrefixAddress_EqualsHead_Test()
        {
            // Arrange
            var subnet = Subnet.Parse("192.168.1.0/24");

            // Act
            var networkPrefix = subnet.NetworkPrefixAddress;

            // Assert
            Assert.Equal(subnet.Head, networkPrefix);
            Assert.Equal(IPAddress.Parse("192.168.1.0"), networkPrefix);
        }

        /// <summary>Verifies that <see cref="Subnet.BroadcastAddress"/> equals the subnet's <see cref="AbstractIPAddressRange.Tail"/>.</summary>
        [Fact]
        public void BroadcastAddress_EqualsTail_Test()
        {
            // Arrange
            var subnet = Subnet.Parse("192.168.1.0/24");

            // Act
            var broadcast = subnet.BroadcastAddress;

            // Assert
            Assert.Equal(subnet.Tail, broadcast);
            Assert.Equal(IPAddress.Parse("192.168.1.255"), broadcast);
        }

        #endregion // end: NetworkPrefixAddress / BroadcastAddress

        #region Overlaps

        /// <summary>Verifies that <see cref="Subnet.Overlaps(Subnet)"/> returns the expected result for various subnet pairs.</summary>
        /// <param name="expected">Expected overlap result.</param>
        /// <param name="subnetAString">CIDR string for the first subnet.</param>
        /// <param name="subnetBString">CIDR string for the second subnet.</param>
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

        /// <summary>Verifies that <see cref="AbstractIPAddressRange.Overlaps(IIPAddressRange)"/> is symmetric when a range is wholly contained within a subnet.</summary>
        [Fact]
        public void Overlaps_WhollyContainedIIPAddressRange_IsSymmetric_Test()
        {
            // When a range is wholly contained inside a subnet, both directions must return true.
            // This exercises AbstractIPAddressRange.Overlaps(IIPAddressRange) - the Subnet-typed
            // overload already handled this correctly; the base-class path had an asymmetry bug.
            var outer = Subnet.Parse("192.168.0.0/16");
            var inner = new IPAddressRange(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("192.168.1.255"));

            Assert.True(outer.Overlaps((IIPAddressRange)inner), "outer.Overlaps(inner) should be true");
            Assert.True(inner.Overlaps((IIPAddressRange)outer), "inner.Overlaps(outer) should be true - symmetry");
        }

        /// <summary>Verifies that the subnet-typed <see cref="Subnet.Overlaps(Subnet)"/> overload is symmetric when one subnet is wholly inside the other.</summary>
        [Fact]
        public void Overlaps_WhollyContainedSubnet_IsSymmetric_Test()
        {
            // Subnet-typed overload: both directions must return true when one subnet is inside the other.
            var outer = Subnet.Parse("10.0.0.0/8");
            var inner = Subnet.Parse("10.10.0.0/16");

            Assert.True(outer.Overlaps(inner), "outer.Overlaps(inner) should be true");
            Assert.True(inner.Overlaps(outer), "inner.Overlaps(outer) should be true - symmetry");
        }

        #endregion // end: Overlaps

        #region ToString

        /// <summary>Verifies that <see cref="Subnet.ToString(string, System.IFormatProvider)"/> returns the canonical CIDR representation.</summary>
        /// <param name="expected">Expected canonical CIDR string.</param>
        /// <param name="input">Input CIDR string to parse.</param>
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

        #endregion // end: ToString

        #region UsableHostAddressCount

        /// <summary>Gets theory data for <see cref="UsableHostAddressCount_Test"/>.</summary>
        /// <returns>Parameters: expected usable host address count (<see cref="BigInteger"/>), subnet under test (<see cref="Subnet"/>).</returns>
        public static TheoryData<BigInteger, Subnet> UsableHostAddressCount_Test_Values()
        {
            var data = new TheoryData<BigInteger, Subnet>();

            foreach (var ipAddress in IPv4Addresses())
            {
                for (var i = 0; i <= 32; i++)
                {
                    var routePrefix = 32 - i;
                    var count = routePrefix < 2 ? BigInteger.Zero : BigInteger.Subtract(BigInteger.Pow(2, routePrefix), 2);

                    var subnet = new Subnet(ipAddress, i);
                    data.Add(count, subnet);
                }
            }

            foreach (var ipAddress in IPv6Addresses())
            {
                for (var i = 0; i <= 128; i++)
                {
                    var routePrefix = 128 - i;
                    var count = routePrefix < 2 ? BigInteger.Zero : BigInteger.Subtract(BigInteger.Pow(2, routePrefix), 2);

                    var subnet = new Subnet(ipAddress, i);
                    data.Add(count, subnet);
                }
            }

            return data;

            IEnumerable<IPAddress> IPv4Addresses()
            {
                yield return IPAddress.Parse("192.168.1.1");
            }

            IEnumerable<IPAddress> IPv6Addresses()
            {
                yield return IPAddress.Parse("2001:0db8:85a3:0042:1000:8a2e:0370:7334");
            }
        }

        /// <summary>Verifies that <see cref="Subnet.UsableHostAddressCount"/> returns the expected count for a given subnet.</summary>
        /// <param name="expected">Expected usable host address count.</param>
        /// <param name="subnet">The subnet under test.</param>
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

        /// <summary>Verifies that <see cref="Subnet.UsableHostAddressCount"/> returns zero for a single-host /32 subnet.</summary>
        [Fact]
        public void UsableHostAddressCount_SingleHost_ReturnsZero_Test()
        {
            // Arrange
            var subnet = new Subnet(IPAddress.Parse("10.0.0.1"), 32);

            // Act
            var result = subnet.UsableHostAddressCount;

            // Assert
            Assert.Equal(BigInteger.Zero, result);
        }

        /// <summary>Verifies that <see cref="Subnet.UsableHostAddressCount"/> returns the total length minus two for a /0 subnet.</summary>
        [Fact]
        public void UsableHostAddressCount_SlashZero_ReturnsLengthMinusTwo_Test()
        {
            // Arrange
            var subnet = new Subnet(IPAddress.Any, 0);

            // Act
            var result = subnet.UsableHostAddressCount;

            // Assert
            Assert.Equal(BigInteger.Pow(2, 32) - 2, result);
        }

        #endregion // end: UsableHostAddressCount

        #region Contains

        #region Contains(IPAddress)

        /// <summary>Verifies that <see cref="AbstractIPAddressRange.Contains(System.Net.IPAddress)"/> returns the expected result for various address inputs.</summary>
        /// <param name="expected">Expected result.</param>
        /// <param name="subnetString">CIDR string for the subnet under test.</param>
        /// <param name="containsIPAddressString">The IP address string to test containment.</param>
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

        /// <summary>Verifies that <see cref="Subnet.Contains(Subnet)"/> returns the expected result for various subnet inputs.</summary>
        /// <param name="expected">Expected result.</param>
        /// <param name="subnetString">CIDR string for the outer subnet.</param>
        /// <param name="containsSubnetString">CIDR string for the inner subnet to test.</param>
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

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>Verifies that an IPv4 subnet does not contain an IPv6 address.</summary>
        [Fact]
        public void Contains_IPAddress_IPv4SubnetDoesNotContainIPv6_Test()
        {
            // Arrange
            var subnet = Subnet.Parse("0.0.0.0/0");

            // Act
            var result = subnet.Contains(IPAddress.Parse("::"));

            // Assert
            Assert.False(result);
        }

        #endregion // end: Contains(Subnet)

        #region S2 - Contains delegation

        /// <summary>Verifies that <see cref="Subnet.Contains(Subnet)"/> delegates to the base class <see cref="AbstractIPAddressRange.Contains(IIPAddressRange)"/>.</summary>
        /// <param name="expected">Expected result.</param>
        /// <param name="subnetAString">CIDR string for the outer subnet.</param>
        /// <param name="subnetBString">CIDR string for the inner subnet.</param>
        [Theory]
        [InlineData(true, "192.168.0.0/16", "192.168.0.0/24")]
        [InlineData(true, "192.168.0.0/16", "192.168.0.0/16")]
        [InlineData(false, "192.168.0.0/24", "192.168.0.0/16")]
        [InlineData(false, "10.0.0.0/8", "::/0")]
        public void Contains_IsDelegatedToBaseContains(bool expected, string subnetAString, string subnetBString)
        {
            var subnetA = Subnet.Parse(subnetAString);
            var subnetB = Subnet.Parse(subnetBString);
            Assert.Equal(expected, subnetA.Contains(subnetB));
            Assert.Equal(((IIPAddressRange)subnetA).Contains(subnetB), subnetA.Contains(subnetB));
        }

        #endregion // end: S2 - Contains delegation

        #endregion // end: Contains

        #region Ctor

        #region Ctor(IPAddress)

        /// <summary>Gets theory data for <see cref="Ctor_IPAddress_Test"/>.</summary>
        /// <returns>Parameters: expected subnet (<see cref="Subnet"/>), address to construct with (<see cref="IPAddress"/>).</returns>
        public static TheoryData<Subnet, IPAddress> Ctor_IPAddress_Test_Values()
        {
            var data = new TheoryData<Subnet, IPAddress>();

            foreach (var ipAddress in IPv4Addresses())
            {
                var subnet = new Subnet(ipAddress, 32);
                data.Add(subnet, ipAddress);
            }

            foreach (var ipAddress in IPv6Addresses())
            {
                var subnet = new Subnet(ipAddress, 128);
                data.Add(subnet, ipAddress);
            }

            return data;

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

        /// <summary>Verifies that constructing a <see cref="Subnet"/> from a single <see cref="IPAddress"/> produces the expected host subnet.</summary>
        /// <param name="expected">Expected subnet.</param>
        /// <param name="address">The IP address to construct from.</param>
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

        /// <summary>Verifies that constructing a <see cref="Subnet"/> with a null <see cref="IPAddress"/> throws <see cref="ArgumentNullException"/>.</summary>
        [Fact]
        public void Ctor_IPAddress_NullIPAddress_Throws_ArgumentNullException_Test()
        {
            // Arrange
            // Act / Assert
            Assert.Throws<ArgumentNullException>(() => new Subnet((IPAddress)null));
        }

        /// <summary>Verifies that constructing a <see cref="Subnet"/> from an IPv4 address sets routing prefix to 32.</summary>
        [Fact]
        public void Ctor_IPAddress_SetsRoutingPrefix32_ForIPv4_Test()
        {
            // Arrange
            var address = IPAddress.Parse("10.0.0.1");

            // Act
            var subnet = new Subnet(address);

            // Assert
            Assert.Equal(32, subnet.RoutingPrefix);
        }

        /// <summary>Verifies that constructing a <see cref="Subnet"/> from an IPv6 address sets routing prefix to 128.</summary>
        [Fact]
        public void Ctor_IPAddress_SetsRoutingPrefix128_ForIPv6_Test()
        {
            // Arrange
            var address = IPAddress.IPv6Loopback;

            // Act
            var subnet = new Subnet(address);

            // Assert
            Assert.Equal(128, subnet.RoutingPrefix);
        }

        #endregion // end: Ctor(IPAddress)

        #region S10 - Single-IP constructor with custom exponent

        /// <summary>Verifies that the single-IP constructor uses the default max enumeration exponent.</summary>
        [Fact]
        public void Subnet_SingleIPConstructor_DefaultExponent()
        {
            var subnet = new Subnet(IPAddress.Parse("192.168.1.1"));
            Assert.Equal(12, subnet.MaxEnumerationExponent);
        }

        /// <summary>Verifies that <see cref="Subnet.FromIPAddress(IPAddress, int)"/> sets the expected exponent.</summary>
        [Fact]
        public void Subnet_SingleIPConstructor_CustomExponent()
        {
            var subnet = Subnet.FromIPAddress(IPAddress.Parse("192.168.1.1"), 5);
            Assert.Equal(5, subnet.MaxEnumerationExponent);
        }

        #endregion // end: S10 - Single-IP constructor with custom exponent

        #region Ctor(IPAddress, int)

        /// <summary>Verifies that constructing a <see cref="Subnet"/> with a null address and routing prefix throws <see cref="ArgumentNullException"/>.</summary>
        [Fact]
        public void Ctor_IPAddress_Int_NullIPAddress_Throws_ArgumentNullException_Test()
        {
            // Arrange
            // Act / Assert
            Assert.Throws<ArgumentNullException>(() => new Subnet(null, 42));
        }

        /// <summary>Verifies that constructing a <see cref="Subnet"/> with an out-of-range routing prefix throws <see cref="ArgumentOutOfRangeException"/>.</summary>
        /// <param name="address">IP address string.</param>
        /// <param name="routingPrefix">The out-of-range routing prefix.</param>
        [Theory]
        [InlineData("192.168.1.1", -1)]
        [InlineData("192.168.1.1", 33)]
        [InlineData("::", -1)]
        [InlineData("::", 129)]
        public void Ctor_IPAddress_Int_IntOutOfRange_Throws_ArgumentOutOfRangeException_Test(string address, int routingPrefix)
        {
            // Arrange
            var ipAddress = IPAddress.Parse(address);

            // Act / Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => new Subnet(ipAddress, routingPrefix));
        }

        #endregion // end: Ctor(IPAddress, int)

        #region Ctor(IPAddress, IPAddress)

        /// <summary>Gets theory data for <see cref="Ctor_IPAddress_IPAddress_Test"/>.</summary>
        /// <returns>Parameters: expected subnet (<see cref="Subnet"/>), primary address (<see cref="IPAddress"/>), secondary address (<see cref="IPAddress"/>).</returns>
        public static TheoryData<Subnet, IPAddress, IPAddress> Ctor_IPAddress_IPAddress_Test_Values()
        {
            var data = new TheoryData<Subnet, IPAddress, IPAddress>();
            var seen = new HashSet<string>();

            foreach (var address in IPv4Addresses())
            {
                for (var routePrefix = 0; routePrefix <= 32; routePrefix++)
                {
                    var subnet = new Subnet(address, routePrefix);
                    var key = $"{subnet.ToString("f", null)}|{subnet.Head}|{subnet.Tail}";
                    if (seen.Add(key))
                    {
                        data.Add(subnet, subnet.Head, subnet.Tail);
                    }
                }
            }

            foreach (var address in IPv6Addresses())
            {
                for (var routePrefix = 0; routePrefix <= 128; routePrefix++)
                {
                    var subnet = new Subnet(address, routePrefix);
                    var key = $"{subnet.ToString("f", null)}|{subnet.Head}|{subnet.Tail}";
                    if (seen.Add(key))
                    {
                        data.Add(subnet, subnet.Head, subnet.Tail);
                    }
                }
            }

            return data;

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

        /// <summary>Verifies that constructing a <see cref="Subnet"/> from two <see cref="IPAddress"/> bounds produces the expected subnet.</summary>
        /// <param name="expected">Expected subnet.</param>
        /// <param name="primary">The low address bound.</param>
        /// <param name="secondary">The high address bound.</param>
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

        /// <summary>Verifies that constructing a <see cref="Subnet"/> with a null address argument throws <see cref="ArgumentNullException"/>.</summary>
        /// <param name="primary">Primary address string.</param>
        /// <param name="secondary">Secondary address string.</param>
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

            // Act / Assert
            Assert.Throws<ArgumentNullException>(() => new Subnet(primaryAddress, secondaryAddress));
        }

        /// <summary>Verifies that constructing a <see cref="Subnet"/> with an invalid address ordering throws <see cref="InvalidOperationException"/>.</summary>
        /// <param name="primary">The higher (invalid low) address string.</param>
        /// <param name="secondary">The lower (invalid high) address string.</param>
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

            // Act / Assert
            Assert.Throws<InvalidOperationException>(() => new Subnet(primaryAddress, secondaryAddress));
        }

        /// <summary>Verifies that constructing a <see cref="Subnet"/> with mismatched address families throws <see cref="ArgumentException"/>.</summary>
        /// <param name="primary">First address string.</param>
        /// <param name="secondary">Second address string of a different family.</param>
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

            // Act / Assert
            Assert.Throws<ArgumentException>(() => new Subnet(primaryAddress, secondaryAddress));
        }

        #endregion // end: Ctor(IPAddress, IPAddress)

        #endregion // end: Ctor

        #region Length / TryGetLength

        /// <summary>Gets theory data for <see cref="Length_Test"/>, <see cref="TryGetLength_Integer_Test"/>, and <see cref="TryGetLength_Long_Test"/>.</summary>
        /// <returns>Parameters: expected length (<see cref="BigInteger"/>), subnet under test (<see cref="Subnet"/>).</returns>
        public static TheoryData<BigInteger, Subnet> Length_Test_Values()
        {
            var data = new TheoryData<BigInteger, Subnet>();

            foreach (var ipAddress in IPv4Addresses())
            {
                for (var i = 0; i <= 32; i++)
                {
                    var routePrefix = 32 - i;
                    var length = BigInteger.Pow(2, routePrefix);

                    var subnet = new Subnet(ipAddress, i);
                    data.Add(length, subnet);
                }
            }

            foreach (var ipAddress in IPv6Addresses())
            {
                for (var i = 0; i <= 128; i++)
                {
                    var routePrefix = 128 - i;
                    var length = BigInteger.Pow(2, routePrefix);

                    var subnet = new Subnet(ipAddress, i);
                    data.Add(length, subnet);
                }
            }

            return data;

            IEnumerable<IPAddress> IPv4Addresses()
            {
                yield return IPAddress.Parse("192.168.1.1");
            }

            IEnumerable<IPAddress> IPv6Addresses()
            {
                yield return IPAddress.Parse("2001:0db8:85a3:0042:1000:8a2e:0370:7334");
            }
        }

        /// <summary>Verifies that <see cref="AbstractIPAddressRange.Length"/> returns the expected address count for a subnet.</summary>
        /// <param name="expected">Expected length.</param>
        /// <param name="subnet">The subnet under test.</param>
        [Theory]
        [MemberData(nameof(Length_Test_Values))]
        public void Length_Test(BigInteger expected, Subnet subnet)
        {
            // Arrange
            // Act

            var result = subnet.Length;

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>Verifies that <see cref="AbstractIPAddressRange.TryGetLength(out int)"/> returns the expected success flag and integer length.</summary>
        /// <param name="expected">Expected length as a <see cref="BigInteger"/>.</param>
        /// <param name="subnet">The subnet under test.</param>
        [Theory]
        [MemberData(nameof(Length_Test_Values))]
        public void TryGetLength_Integer_Test(BigInteger expected, Subnet subnet)
        {
            // Arrange
            // Act
            var success = subnet.TryGetLength(out int length);

            // Assert
            Assert.Equal(expected <= int.MaxValue, success);
            Assert.Equal(expected <= int.MaxValue ? (int)expected : -1, length);
        }

        /// <summary>Verifies that <see cref="AbstractIPAddressRange.TryGetLength(out long)"/> returns the expected success flag and long length.</summary>
        /// <param name="expected">Expected length as a <see cref="BigInteger"/>.</param>
        /// <param name="subnet">The subnet under test.</param>
        [Theory]
        [MemberData(nameof(Length_Test_Values))]
        public void TryGetLength_Long_Test(BigInteger expected, Subnet subnet)
        {
            // Arrange
            // Act
            var success = subnet.TryGetLength(out long length);

            // Assert
            Assert.Equal(expected <= long.MaxValue, success);
            Assert.Equal(expected <= long.MaxValue ? (long)expected : -1, length);
        }

        #endregion // end: Length / TryGetLength

        #region Formatting

        #region ToString

        /// <summary>Verifies that <see cref="AbstractIPAddressRange.ToString()"/> matches the general format string output.</summary>
        /// <param name="headString">Head address string of the range.</param>
        /// <param name="tailString">Tail address string of the range.</param>
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

        /// <summary>Gets theory data for <see cref="ToString_Format_Test"/>.</summary>
        /// <returns>Parameters: expected string result, format specifier string, format provider (<see cref="IFormatProvider"/>), subnet under test (<see cref="Subnet"/>).</returns>
        public static TheoryData<string, string, IFormatProvider, Subnet> ToString_Format_Test_Values()
        {
            var data = new TheoryData<string, string, IFormatProvider, Subnet>();

            // general formats
            foreach (var format in new[] { null, string.Empty, "g", "G" })
            {
                foreach (var subnet in Ipv4AddressSubnets().Concat(Ipv6AddressSubnets()))
                {
                    data.Add(
                        $"{subnet.NetworkPrefixAddress}/{subnet.RoutingPrefix}",
                        format,
                        CultureInfo.CurrentCulture,
                        subnet
                    );
                }
            }

            // "friendly" formats
            foreach (var format in new[] { "f", "F" })
            {
                foreach (var subnet in Ipv4AddressSubnets().Concat(Ipv6AddressSubnets()))
                {
                    data.Add(
                        subnet.IsSingleIP
                            ? $"{subnet.NetworkPrefixAddress}"
                            : $"{subnet.NetworkPrefixAddress}/{subnet.RoutingPrefix}",
                        format,
                        CultureInfo.CurrentCulture,
                        subnet
                    );
                }
            }

            // range formats
            foreach (var format in new[] { "r", "R" })
            {
                foreach (var subnet in Ipv4AddressSubnets().Concat(Ipv6AddressSubnets()))
                {
                    data.Add($"{subnet.Head} - {subnet.Tail}", format, CultureInfo.CurrentCulture, subnet);
                }
            }

            return data;

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

        /// <summary>Verifies that <see cref="Subnet.ToString(string, IFormatProvider)"/> returns the expected formatted string.</summary>
        /// <param name="expected">Expected formatted string.</param>
        /// <param name="format">Format specifier.</param>
        /// <param name="formatProvider">Format provider.</param>
        /// <param name="subnet">The subnet to format.</param>
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

        /// <summary>Verifies that <see cref="Subnet.ToString(string, IFormatProvider)"/> uses invariant culture when the format provider is null.</summary>
        [Fact]
        public void ToString_NullFormatProvider_UsesInvariantCulture_Test()
        {
            // Arrange
            var subnet = new Subnet(IPAddress.Parse("192.168.1.0"), 24);

            // Act
            var result = subnet.ToString("G", null);

            // Assert
            Assert.Equal("192.168.1.0/24", result);
        }

        /// <summary>Verifies that <see cref="Subnet.ToString(string, IFormatProvider)"/> throws <see cref="FormatException"/> for an unknown format specifier.</summary>
        [Fact]
        public void ToString_UnknownFormat_Throws_FormatException_Test()
        {
            // Arrange
            var range = new Subnet(IPAddress.Parse("192.168.1.1"), 16);

            // Act / Assert
            Assert.Throws<FormatException>(() => range.ToString("potato", CultureInfo.CurrentCulture));
        }

        #endregion // end: ToString(string, IFormatProvider)

        #endregion // end: Formatting

        #region Deconstruct

        /// <summary>Gets theory data for deconstruct tests.</summary>
        /// <returns>Parameters: subnet under test (<see cref="Subnet"/>).</returns>
        public static TheoryData<Subnet> Deconstruct_Values()
        {
            var data = new TheoryData<Subnet>();
            var seen = new HashSet<string>();

            foreach (var ipAddress in IPv4Addresses())
            {
                for (var i = 0; i <= 32; i++)
                {
                    var subnet = new Subnet(ipAddress, i);
                    if (seen.Add(subnet.ToString("f", null)))
                    {
                        data.Add(subnet);
                    }
                }
            }

            foreach (var ipAddress in IPv6Addresses())
            {
                for (var i = 0; i <= 128; i++)
                {
                    var subnet = new Subnet(ipAddress, i);
                    if (seen.Add(subnet.ToString("f", null)))
                    {
                        data.Add(subnet);
                    }
                }
            }

            return data;

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

        /// <summary>Verifies that the four-element deconstruct of a <see cref="Subnet"/> yields the expected components.</summary>
        /// <param name="subnet">The subnet to deconstruct.</param>
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

        /// <summary>Verifies that the two-element deconstruct of a <see cref="Subnet"/> yields the network prefix address and routing prefix.</summary>
        /// <param name="subnet">The subnet to deconstruct.</param>
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

        #region Ctor refactoring verification

        /// <summary>Verifies that the Subnet(IPAddress, IPAddress) ctor computes correct prefix and netmask.</summary>
        /// <param name="subnetString">CIDR notation expected after construction from low/high addresses.</param>
        /// <param name="lowString">Low address string.</param>
        /// <param name="highString">High address string.</param>
        [Theory]
        [InlineData("192.168.1.0/24", "192.168.1.0", "192.168.1.255")]
        [InlineData("192.168.1.8/29", "192.168.1.8", "192.168.1.15")]
        [InlineData("10.0.0.0/8", "10.0.0.0", "10.255.255.255")]
        [InlineData("::/112", "::", "::ffff")]
        [InlineData("feed:beef::/32", "feed:beef::", "feed:beef:ffff:ffff:ffff:ffff:ffff:ffff")]
        public void Ctor_FromLowHigh_ComputesPrefixAndNetmask_Test(string subnetString, string lowString, string highString)
        {
            // Arrange
            var expected = Subnet.Parse(subnetString);
            var low = IPAddress.Parse(lowString);
            var high = IPAddress.Parse(highString);

            // Act
            var subnet = new Subnet(low, high);

            // Assert
            Assert.Equal(expected.RoutingPrefix, subnet.RoutingPrefix);
            Assert.Equal(expected.NetworkPrefixAddress, subnet.NetworkPrefixAddress);
            Assert.Equal(expected.BroadcastAddress, subnet.BroadcastAddress);
            Assert.Equal(expected.Netmask, subnet.Netmask);
        }

        #endregion // end: Ctor refactoring verification

        #region maxEnumerationExponent propagation

        /// <summary>Verifies that <see cref="Subnet(IPAddress, int, int)"/> propagates maxEnumerationExponent.</summary>
        [Fact]
        public void Ctor_IPAddress_ExponentPropagates_Test()
        {
            var subnet = new Subnet(IPAddress.Parse("10.0.0.1"), 32, maxEnumerationExponent: 4);
            Assert.Equal(4, subnet.MaxEnumerationExponent);
        }

        #endregion // end: maxEnumerationExponent propagation

        internal class SubnetEqualityComparer : IEqualityComparer<Subnet>
        {
            public static readonly SubnetEqualityComparer Instance = new();

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
