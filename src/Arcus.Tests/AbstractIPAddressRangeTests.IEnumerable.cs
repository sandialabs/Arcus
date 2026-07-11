using System.Collections;
using System.Net;

namespace Arcus.Tests
{
    /// <content>
    ///     <see cref="AbstractIPAddressRange"/> tests for <see cref="IEnumerable{IPAddress}"/>
    /// </content>
    public partial class AbstractIPAddressRangeTests
    {
        #region IEnumerable

        /// <summary>Verifies the generic enumerator yields every address from head to tail inclusive.</summary>
        /// <param name="headAddressString">The head IP address string for the range.</param>
        /// <param name="tailAddressString">The tail IP address string for the range.</param>
        [Theory]
        [InlineData("::", "::")]
        [InlineData("::", "::FF")]
        [InlineData("0.0.0.0", "0.0.0.0")]
        [InlineData("192.168.1.1", "192.168.1.1")]
        [InlineData("192.168.1.1", "192.168.1.5")]
        [InlineData("255.255.255.128", "255.255.255.255")]
        [InlineData("255.255.255.255", "255.255.255.255")]
        [InlineData("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ff00", "ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff")]
        [InlineData("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff", "ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff")]
        public void GetEnumerator_Test(string headAddressString, string tailAddressString)
        {
            // Arrange
            var head = IPAddress.Parse(headAddressString);
            var tail = IPAddress.Parse(tailAddressString);

            var iPAddressRange = CreateIPAddressRange(head, tail);

            // Act
            var ipAddressArray = iPAddressRange.ToArray();

            // Assert
            Assert.Equal(iPAddressRange.Count(), ipAddressArray.Length);
            Assert.Equal(head, ipAddressArray[0]);
            Assert.Equal(tail, ipAddressArray[ipAddressArray.Length - 1]);
        }

        /// <summary>Verifies the non-generic IEnumerable enumerator yields every address from head to tail inclusive.</summary>
        /// <param name="headAddressString">The head IP address string for the range.</param>
        /// <param name="tailAddressString">The tail IP address string for the range.</param>
        [Theory]
        [InlineData("::", "::")]
        [InlineData("::", "::FF")]
        [InlineData("0.0.0.0", "0.0.0.0")]
        [InlineData("192.168.1.1", "192.168.1.1")]
        [InlineData("192.168.1.1", "192.168.1.5")]
        [InlineData("255.255.255.128", "255.255.255.255")]
        [InlineData("255.255.255.255", "255.255.255.255")]
        [InlineData("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ff00", "ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff")]
        [InlineData("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff", "ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff")]
        public void GetIEnumerableEnumerator_Test(string headAddressString, string tailAddressString)
        {
            // Arrange
            var head = IPAddress.Parse(headAddressString);
            var tail = IPAddress.Parse(tailAddressString);

            var iPAddressRange = CreateIPAddressRange(head, tail);

            // Act
            var result = new List<IPAddress>();
            var enumerator = ((IEnumerable)iPAddressRange).GetEnumerator();
            while (enumerator.MoveNext())
            {
                result.Add(enumerator.Current as IPAddress);
            }

            // Assert
            Assert.Equal(iPAddressRange.Count(), result.Count);
            Assert.Equal(head, result[0]);
            Assert.Equal(tail, result[result.Count - 1]);
        }

        #endregion // end: IEnumerable

        #region IEnumerable / IEnumerable<IPAddress>

        /// <summary>Verifies that enumerating an IPv6 range yields exactly the expected addresses.</summary>
        [Fact]
        public void Enumerable_IPv6_ContainsExpected_Test()
        {
            // Arrange
            const string headString = "::a";
            const string tailString = "::c";
            var head = IPAddress.Parse(headString);
            var tail = IPAddress.Parse(tailString);

            var iPAddressRange = CreateIPAddressRange(head, tail);

            // Act
            var result = iPAddressRange.ToList();

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Equal(new[] { head, IPAddress.Parse("::b"), tail }, result);
        }

        /// <summary>Verifies that taking more elements than the IPv6 range contains stops at the tail without error.</summary>
        [Fact]
        public void Enumerable_IPv6_TakePastEnd_Test()
        {
            // Arrange
            var head = IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:ffff:ffff:fff0");
            var tail = IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff");

            var iPAddressRange = CreateIPAddressRange(head, tail);

            // Act
            var result = iPAddressRange.Take(100).ToList();

            // Assert
            Assert.Equal(16, result.Count);
            Assert.Equal(tail, result[result.Count - 1]);
        }

        /// <summary>Verifies that taking more elements than the IPv4 range contains stops at the tail without error.</summary>
        [Fact]
        public void Enumerable_IPv4_TakePastEnd_Test()
        {
            // Arrange
            var head = IPAddress.Parse("255.255.255.240");
            var tail = IPAddress.Parse("255.255.255.255");

            var iPAddressRange = CreateIPAddressRange(head, tail);

            // Act
            var result = iPAddressRange.Take(100).ToList();

            // Assert
            Assert.Equal(16, result.Count);
            Assert.Equal(tail, result[result.Count - 1]);
        }

        /// <summary>Verifies that enumerating an IPv4 range yields exactly the expected addresses.</summary>
        [Fact]
        public void Enumerable_IPv4_ContainsExpected_Test()
        {
            // Arrange
            const string headString = "192.168.1.1";
            const string tailString = "192.168.1.3";
            var head = IPAddress.Parse(headString);
            var tail = IPAddress.Parse(tailString);

            var iPAddressRange = CreateIPAddressRange(head, tail);

            // Act
            var result = iPAddressRange.ToList();

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Equal(new[] { head, IPAddress.Parse("192.168.1.2"), tail }, result);
        }

        /// <summary>Verifies that a range whose head and tail are the same address enumerates to exactly one element.</summary>
        [Fact]
        public void Enumerable_SameHead_And_SameTail_ReturnsSingle_Test()
        {
            // Arrange
            var ipAddress = IPAddress.Parse("192.168.1.1");

            var iPAddressRange = CreateIPAddressRange(ipAddress, ipAddress);

            // Act
            var addresses = iPAddressRange.ToArray();

            // Assert
            Assert.Single(addresses);
            Assert.Equal(ipAddress, addresses.Single());
        }

        /// <summary>Verifies that enumerating across a byte-boundary (address rollover) produces correct results.</summary>
        [Fact]
        public void Enumerable_ByteBoundaryCrossing_Test()
        {
            // Arrange - IPv4 range crossing the 255.255.255.255 → 0.0.0.0 boundary is invalid,
            // but we can test a range that exercises the span-based path near a byte boundary.
            var head = IPAddress.Parse("192.168.1.254");
            var tail = IPAddress.Parse("192.168.2.2");
            var iPAddressRange = CreateIPAddressRange(head, tail);

            // Act
            var result = iPAddressRange.ToList();

            // Assert
            Assert.Equal(5, result.Count);
            Assert.Equal(head, result[0]);
            Assert.Equal(IPAddress.Parse("192.168.1.255"), result[1]);
            Assert.Equal(IPAddress.Parse("192.168.2.0"), result[2]);
            Assert.Equal(IPAddress.Parse("192.168.2.1"), result[3]);
            Assert.Equal(tail, result[4]);
        }

        /// <summary>Verifies that enumerating a small IPv6 range that spans a 16-bit boundary produces correct results.</summary>
        [Fact]
        public void Enumerable_IPv6_BoundaryCrossing_Test()
        {
            // Arrange - IPv6 range crossing ::ffff → ::1:0000
            var head = IPAddress.Parse("::fffe");
            var tail = IPAddress.Parse("::1:0001");
            var iPAddressRange = CreateIPAddressRange(head, tail);

            // Act
            var result = iPAddressRange.ToList();

            // Assert
            Assert.Equal(4, result.Count);
            Assert.Equal(head, result[0]);
            Assert.Equal(IPAddress.Parse("::ffff"), result[1]);
            Assert.Equal(IPAddress.Parse("::1:0"), result[2]);
            Assert.Equal(tail, result[3]);
        }

        /// <summary>Verifies that skipping and taking within a huge IPv6 range completes in finite time (lazy enumeration).</summary>
        [Fact]
        public void Enumerable_ReasonableIteration_Test()
        {
            // Arrange
            var iPAddressRange = CreateIPAddressRange(
                IPAddress.Parse("::"),
                IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff")
            );

            // Act
            var addresses = iPAddressRange.Skip(10).Take(10).ToArray();

            // Assert
            Assert.NotNull(addresses); // if this test completed it likely means that we didn't iterate through 2^128 ip addresses to skip 10 and take 10
        }

        #endregion // end: IEnumerable / IEnumerable<IPAddress>

        #region Constructor maxEnumerationExponent

        /// <summary>Verifies that constructing with a negative exponent throws.</summary>
        [Fact]
        public void Ctor_NegativeExponent_Throws_Test()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new IPAddressRange(IPAddress.Parse("0.0.0.0"), IPAddress.Parse("0.0.0.1"), maxEnumerationExponent: -1)
            );

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new Subnet(IPAddress.Parse("0.0.0.0"), IPAddress.Parse("0.0.0.1"), maxEnumerationExponent: -1)
            );

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new Subnet(IPAddress.Parse("0.0.0.0"), 24, maxEnumerationExponent: -1)
            );
        }

        /// <summary>Verifies that constructing with an exponent over 128 throws.</summary>
        [Fact]
        public void Ctor_ExponentOver128_Throws_Test()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new IPAddressRange(IPAddress.Parse("0.0.0.0"), IPAddress.Parse("0.0.0.1"), maxEnumerationExponent: 129)
            );

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new Subnet(IPAddress.Parse("0.0.0.0"), IPAddress.Parse("0.0.0.1"), maxEnumerationExponent: 129)
            );
        }

        /// <summary>Verifies that constructing with exponent 0 is valid.</summary>
        [Fact]
        public void Ctor_Exponent0_IsValid_Test()
        {
            var range = new IPAddressRange(IPAddress.Parse("10.0.0.0"), IPAddress.Parse("10.0.0.5"), maxEnumerationExponent: 0);
            Assert.Equal(0, range.MaxEnumerationExponent);
        }

        /// <summary>
        ///     Verifies that constructing with exponent 128 is valid for IPv6 (stays 128)
        ///     and for IPv4 is clamped to 32.
        /// </summary>
        [Fact]
        public void Ctor_Exponent128_IsValid_Test()
        {
            // IPv6: exponent 128 should stay 128
            var ipv6Range = new IPAddressRange(
                IPAddress.Parse("::"),
                IPAddress.Parse("::5"),
                maxEnumerationExponent: 128
            );
            Assert.Equal(128, ipv6Range.MaxEnumerationExponent);

            // IPv4: exponent 128 should clamp to 32
            var ipv4Range = new IPAddressRange(
                IPAddress.Parse("10.0.0.0"),
                IPAddress.Parse("10.0.0.5"),
                maxEnumerationExponent: 128
            );
            Assert.Equal(32, ipv4Range.MaxEnumerationExponent);
        }

        /// <summary>Verifies that the default exponent is 12.</summary>
        [Fact]
        public void Ctor_DefaultExponent_Is12_Test()
        {
            var range = new IPAddressRange(IPAddress.Parse("10.0.0.0"), IPAddress.Parse("10.0.0.5"));
            Assert.Equal(12, range.MaxEnumerationExponent);
        }

        #endregion // end: Constructor maxEnumerationExponent

        #region Enumeration Cap

        /// <summary>Verifies that iterating a range within the cap succeeds.</summary>
        [Fact]
        public void GetEnumerator_WithinCap_Succeeds_Test()
        {
            var range = new IPAddressRange(IPAddress.Parse("10.0.0.0"), IPAddress.Parse("10.0.0.5"), maxEnumerationExponent: 4);

            var result = range.ToArray();

            Assert.Equal(6, result.Length);
            Assert.Equal(IPAddress.Parse("10.0.0.0"), result[0]);
            Assert.Equal(IPAddress.Parse("10.0.0.5"), result[result.Length - 1]);
        }

        /// <summary>Verifies that iterating a range exceeding the cap throws.</summary>
        [Fact]
        public void GetEnumerator_ExceedsCap_Throws_Test()
        {
            var range = new IPAddressRange(
                IPAddress.Parse("10.0.0.0"),
                IPAddress.Parse("10.0.0.255"),
                maxEnumerationExponent: 4
            );

            var ex = Assert.Throws<InvalidOperationException>(() => range.ToArray());
            Assert.Contains("2^4", ex.Message);
        }

        /// <summary>Verifies that iterating a range exactly at the cap boundary succeeds.</summary>
        [Fact]
        public void GetEnumerator_AtCapBoundary_Succeeds_Test()
        {
            // 2^4 = 16 addresses
            var range = new IPAddressRange(
                IPAddress.Parse("10.0.0.0"),
                IPAddress.Parse("10.0.0.15"),
                maxEnumerationExponent: 4
            );

            var result = range.ToArray();

            Assert.Equal(16, result.Length);
            Assert.Equal(IPAddress.Parse("10.0.0.15"), result[result.Length - 1]);
        }

        /// <summary>Verifies that Subnet with explicit exponent enforces the cap.</summary>
        [Fact]
        public void Subnet_Enumeration_UsesExponent_Test()
        {
            // /30 = 4 addresses, exponent 2 = cap 4
            var subnet = new Subnet(IPAddress.Parse("10.0.0.0"), 30, maxEnumerationExponent: 2);

            var result = subnet.ToArray();

            Assert.Equal(4, result.Length);
        }

        #endregion // end: Enumeration Cap

        #region ToIPAddresses

        /// <summary>Verifies that <see cref="IIPAddressRange.ToIPAddresses"/> yields the same addresses as <see cref="IEnumerable{IPAddress}"/> for small ranges.</summary>
        /// <param name="headAddressString">The head IP address string for the range.</param>
        /// <param name="tailAddressString">The tail IP address string for the range.</param>
        [Theory]
        [InlineData("::", "::")]
        [InlineData("::", "::FF")]
        [InlineData("0.0.0.0", "0.0.0.0")]
        [InlineData("192.168.1.1", "192.168.1.1")]
        [InlineData("192.168.1.1", "192.168.1.5")]
        [InlineData("255.255.255.128", "255.255.255.255")]
        [InlineData("255.255.255.255", "255.255.255.255")]
        [InlineData("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ff00", "ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff")]
        [InlineData("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff", "ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff")]
        public void ToIPAddresses_MatchesEnumerable_Test(string headAddressString, string tailAddressString)
        {
            // Arrange
            var head = IPAddress.Parse(headAddressString);
            var tail = IPAddress.Parse(tailAddressString);
            var range = CreateIPAddressRange(head, tail);

            // Act
            var enumerateResult = range.ToIPAddresses().ToArray();
            // CodeQL [cs/useless-upcast] Intentionally casting to IEnumerable<IPAddress> to verify that the explicit interface implementation produces the same results as the public ToIPAddresses method
            var enumerableResult = ((IEnumerable<IPAddress>)range).ToArray();

            // Assert
            Assert.Equal(enumerableResult, enumerateResult);
        }

        /// <summary>Verifies that <see cref="IIPAddressRange.ToIPAddresses"/> within the cap succeeds.</summary>
        [Fact]
        public void ToIPAddresses_WithinCap_Succeeds_Test()
        {
            var range = new IPAddressRange(IPAddress.Parse("10.0.0.0"), IPAddress.Parse("10.0.0.5"), maxEnumerationExponent: 4);

            var result = range.ToIPAddresses().ToArray();
            Assert.Equal(6, result.Length);
            Assert.Equal(IPAddress.Parse("10.0.0.0"), result[0]);
            Assert.Equal(IPAddress.Parse("10.0.0.5"), result[result.Length - 1]);
        }

        /// <summary>Verifies that <see cref="IIPAddressRange.ToIPAddresses"/> exceeding the cap throws.</summary>
        [Fact]
        public void ToIPAddresses_ExceedsCap_Throws_Test()
        {
            var range = new IPAddressRange(
                IPAddress.Parse("10.0.0.0"),
                IPAddress.Parse("10.0.0.255"),
                maxEnumerationExponent: 4
            );

            var ex = Assert.Throws<InvalidOperationException>(() => range.ToIPAddresses().ToArray());
            Assert.Contains("2^4", ex.Message);
        }

        /// <summary>Verifies that <see cref="IIPAddressRange.ToIPAddresses"/> at the cap boundary succeeds.</summary>
        [Fact]
        public void ToIPAddresses_AtCapBoundary_Succeeds_Test()
        {
            // 2^4 = 16 addresses
            var range = new IPAddressRange(
                IPAddress.Parse("10.0.0.0"),
                IPAddress.Parse("10.0.0.15"),
                maxEnumerationExponent: 4
            );

            var result = range.ToIPAddresses().ToArray();

            Assert.Equal(16, result.Length);
            Assert.Equal(IPAddress.Parse("10.0.0.15"), result[result.Length - 1]);
        }

        #endregion // end: ToIPAddresses
    }
}
