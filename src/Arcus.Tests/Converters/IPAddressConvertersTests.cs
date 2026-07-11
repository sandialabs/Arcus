using System.Net;
using Arcus.Converters;

namespace Arcus.Tests.Converters
{
    /// <summary>Unit tests for <see cref="IPAddressConverters"/>.</summary>
    public class IPAddressConvertersTests
    {
        #region ToUncompressedString

        /// <summary>Gets theory data for <see cref="ToUncompressedString_ValidInput_ReturnsUncompressed_Test"/>.</summary>
        /// <value>Parameters: expected (string), input (string).</value>
        public static TheoryData<string, string> ToUncompressedString_ValidInput_ReturnsUncompressed_Test_Values =>
            new()
            {
                { "192.168.001.001", "192.168.1.1" },
                { "192.168.009.001", "192.168.9.1" },
                { "124.253.063.036", "124.253.63.36" },
                { "124.253.000.000", "124.253.0.0" },
                { "000.253.063.036", "0.253.63.36" },
                { "001.253.063.036", "1.253.63.36" },
                { "001.000.063.036", "1.0.63.36" },
                { "255.255.255.255", "255.255.255.255" },
                { "000.000.000.000", "0.0.0.0" },
                { "100.010.001.000", "100.10.1.0" },
                { "0000:0000:0000:0000:0000:0000:0000:0000", "::" },
                { "0001:0002:ffff:0000:00ab:0000:0000:0123", "1:2:ffff:0:ab:0:0:123" },
                { "ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff", "ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff" },
            };

        /// <summary>Verifies that <c>ToUncompressedString</c> returns the fully-expanded uncompressed form of a valid IP address.</summary>
        /// <param name="expected">The expected uncompressed string representation.</param>
        /// <param name="input">The input IP address string to parse and convert.</param>
        [Theory]
        [MemberData(nameof(ToUncompressedString_ValidInput_ReturnsUncompressed_Test_Values))]
        public void ToUncompressedString_ValidInput_ReturnsUncompressed_Test(string expected, string input)
        {
            // Arrange
            var address = IPAddress.Parse(input);

            // Act
            var result = address.ToUncompressedString();

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>Verifies that <c>ToUncompressedString</c> returns <see langword="null"/> when the input address is <see langword="null"/>.</summary>
        [Fact]
        public void ToUncompressedString_NullInput_ReturnsNull_Test()
        {
            // Arrange
            IPAddress address = null;

            // Act
            // codeql[cs/dereferenced-value-is-always-null] Extension methods are static calls in C# — invoking on null is valid and intentionally tests null-handling behavior
            var result = address.ToUncompressedString();

            // Assert
            Assert.Null(result);
        }

        #endregion // end: ToUncompressedString

        #region ToBase85String

        /// <summary>Gets theory data for <see cref="ToBase85String_ValidIPv6Input_ReturnsBase85_Test"/>.</summary>
        /// <value>Parameters: expected (string), input (string).</value>
        public static TheoryData<string, string> ToBase85String_ValidIPv6Input_ReturnsBase85_Test_Values =>
            new()
            {
                { "$@bLmTEHhx*HIpup2~ix", "dead:beef::" },
                { "=r54lj&NUUO~Hi%c2ym0", "ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff" },
                { "00000000000000000000", "::" },
                { "000000000000000-mSjx", "::dead:beef" },
                { "4)+k&C#VzJ4br>0wv%Yp", "1080:0:0:0:8:800:200C:417A" }, // specific example from RFC 1924
            };

        /// <summary>Verifies that <c>ToBase85String</c> returns the correct RFC 1924 Base-85 encoding for a valid IPv6 address.</summary>
        /// <param name="expected">The expected Base-85 encoded string.</param>
        /// <param name="input">The input IPv6 address string to parse and convert.</param>
        [Theory]
        [MemberData(nameof(ToBase85String_ValidIPv6Input_ReturnsBase85_Test_Values))]
        public void ToBase85String_ValidIPv6Input_ReturnsBase85_Test(string expected, string input)
        {
            // Arrange
            var address = IPAddress.Parse(input);

            // Act
            var result = address.ToBase85String();

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>Verifies that <c>ToBase85String</c> returns <see langword="null"/> when the input address is <see langword="null"/>.</summary>
        [Fact]
        public void ToBase85String_NullInput_ReturnsNull_Test()
        {
            // Arrange
            IPAddress address = null;

            // Act
            // codeql[cs/dereferenced-value-is-always-null] Extension methods are static calls in C# — invoking on null is valid and intentionally tests null-handling behavior
            var result = address.ToBase85String();

            // Assert
            Assert.Null(result);
        }

        /// <summary>Verifies that <c>ToBase85String</c> returns <see langword="null"/> for an IPv4 address, since Base-85 encoding applies only to IPv6.</summary>
        [Fact]
        public void ToBase85String_IPv4Input_ReturnsNull_Test()
        {
            // Arrange
            var address = IPAddress.Parse("192.168.1.1");

            // Act
            var result = address.ToBase85String();

            // Assert
            Assert.Null(result);
        }

        #endregion // end: ToBase85String

        #region ToDottedQuadString

        /// <summary>Gets theory data for <see cref="ToDottedQuadString_ValidInput_ReturnsDottedQuad_Test"/>.</summary>
        /// <value>Parameters: expected (string), input (string).</value>
        public static TheoryData<string, string> ToDottedQuadString_ValidInput_ReturnsDottedQuad_Test_Values =>
            new()
            {
                { "1:2:3:a:b:ffff:255.255.255.255", "1:2:3:a:b:ffff:ffff:ffff" },
                { "ffff:ffff:ffff:ffff:ffff:ffff:1.2.3.4", "ffff:ffff:ffff:ffff:ffff:ffff:0102:0304" },
                { "0:ffff:ffff:ffff::255.255.255.255", "0:ffff:ffff:ffff:0:0:ffff:ffff" },
                { "::ffff:ffff:0:0:255.255.255.255", "0:0:ffff:ffff:0:0:ffff:ffff" },
                { "::ffff:ffff:ffff:0:255.255.255.255", "0:0:ffff:ffff:ffff:0:ffff:ffff" },
                { "ffff:ffff:ffff:ffff:ffff:ffff:255.255.255.255", "ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff" },
                { "ffff:ffff:ffff:ffff:ffff:ffff:0.0.0.0", "ffff:ffff:ffff:ffff:ffff:ffff::" },
                { "ffff::0.0.0.0", "ffff::" },
                { "::ffff:0.0.0.0", "::ffff:0000:0000" },
                { "::ffff:ffff:ffff:ffff:255.255.255.255", "00::ffff:ffff:ffff:ffff:ffff:ffff" },
                { "::255.255.255.255", "::ffff:ffff" },
                { "ffff::ffff:ffff:ffff:0:0.0.0.0", "ffff:0000:ffff:ffff:ffff::" },
                { "ffff::ffff:ffff:ffff:ffff:255.255.255.255", "ffff::ffff:ffff:ffff:ffff:ffff:ffff" },
                { "::0.0.0.0", "::" },
                { "192.168.1.1", "192.168.1.1" },
                { "::0.0.0.1", "::1" },
                { "::ffff:192.168.1.1", "::ffff:192.168.1.1" },
                { "::1:0:0:1:255.255.255.255", "0:0:1:0:0:1:ffff:ffff" },
                { "fe80::0.0.0.1", "fe80::1" },
                { "1::1:1:0.0.0.0", "1:0:0:0:1:1:0:0" },
            };

        /// <summary>Verifies that <c>ToDottedQuadString</c> returns the correct dotted-quad notation string for a valid IP address.</summary>
        /// <param name="expected">The expected dotted-quad string representation.</param>
        /// <param name="input">The input IP address string to parse and convert.</param>
        [Theory]
        [MemberData(nameof(ToDottedQuadString_ValidInput_ReturnsDottedQuad_Test_Values))]
        public void ToDottedQuadString_ValidInput_ReturnsDottedQuad_Test(string expected, string input)
        {
            // Arrange
            var address = IPAddress.Parse(input);

            // Act
            var result = address.ToDottedQuadString();

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>Verifies that <c>ToDottedQuadString</c> returns <see langword="null"/> when the input address is <see langword="null"/>.</summary>
        [Fact]
        public void ToDottedQuadString_NullInput_ReturnsNull_Test()
        {
            // Arrange
            IPAddress address = null;

            // Act
            // codeql[cs/dereferenced-value-is-always-null] Extension methods are static calls in C# — invoking on null is valid and intentionally tests null-handling behavior
            var result = address.ToDottedQuadString();

            // Assert
            Assert.Null(result);
        }

        #endregion // end: ToDottedQuadString

        #region ToHexString

        /// <summary>Gets theory data for <see cref="ToHexString_ValidInput_ReturnsHex_Test"/>.</summary>
        /// <value>Parameters: expected (string), input (string).</value>
        public static TheoryData<string, string> ToHexString_ValidInput_ReturnsHex_Test_Values =>
            new()
            {
                { "00000000000000000000000000000000", "::" },
                { "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF", "ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff" },
                { "0000000000000000000000000000ABCD", "::abcd" },
                { "ABCD000000000000000000000000ABCD", "abcd::abcd" },
                { "ABCD0000000000000000000000000000", "abcd::" },
                { "00000000", "0.0.0.0" },
                { "FFFFFFFF", "255.255.255.255" },
                { "80808080", "128.128.128.128" },
                { "00808080", "0.128.128.128" },
                { "80808000", "128.128.128.0" },
            };

        /// <summary>Verifies that <c>ToHexString</c> returns the uppercase hexadecimal string representation of a valid IP address.</summary>
        /// <param name="expected">The expected hexadecimal string.</param>
        /// <param name="input">The input IP address string to parse and convert.</param>
        [Theory]
        [MemberData(nameof(ToHexString_ValidInput_ReturnsHex_Test_Values))]
        public void ToHexString_ValidInput_ReturnsHex_Test(string expected, string input)
        {
            // Arrange
            var address = IPAddress.Parse(input);

            // Act
            var result = address.ToHexString();

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>Verifies that <c>ToHexString</c> returns <see langword="null"/> when the input address is <see langword="null"/>.</summary>
        [Fact]
        public void ToHexString_NullInput_ReturnsNull_Test()
        {
            // Arrange
            IPAddress address = null;

            // Act
            // codeql[cs/dereferenced-value-is-always-null] Extension methods are static calls in C# — invoking on null is valid and intentionally tests null-handling behavior
            var result = address.ToHexString();

            // Assert
            Assert.Null(result);
        }

        #endregion // end: ToHexString

        #region ToNumericString

        /// <summary>Gets theory data for <see cref="ToNumericString_ValidInput_ReturnsNumeric_Test"/>.</summary>
        /// <value>Parameters: expected (string), input (string).</value>
        public static TheoryData<string, string> ToNumericString_ValidInput_ReturnsNumeric_Test_Values =>
            new()
            {
                { "0", "::" },
                { "340282366920938463463374607431768211455", "ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff" },
                { "43981", "::abcd" },
                { "228362408135220253930399759055429086157", "abcd::abcd" },
                { "228362408135220253930399759055429042176", "abcd::" },
                { "0", "0.0.0.0" },
                { "4294967295", "255.255.255.255" },
                { "2155905152", "128.128.128.128" },
                { "8421504", "0.128.128.128" },
                { "2155905024", "128.128.128.0" },
            };

        /// <summary>Verifies that <c>ToNumericString</c> returns the decimal numeric string representation of a valid IP address.</summary>
        /// <param name="expected">The expected decimal numeric string.</param>
        /// <param name="input">The input IP address string to parse and convert.</param>
        [Theory]
        [MemberData(nameof(ToNumericString_ValidInput_ReturnsNumeric_Test_Values))]
        public void ToNumericString_ValidInput_ReturnsNumeric_Test(string expected, string input)
        {
            // Arrange
            var address = IPAddress.Parse(input);

            // Act
            var result = address.ToNumericString();

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>Verifies that <c>ToNumericString</c> returns <see langword="null"/> when the input address is <see langword="null"/>.</summary>
        [Fact]
        public void ToNumericString_NullInput_ReturnsNull_Test()
        {
            // Arrange
            IPAddress address = null;

            // Act
            // codeql[cs/dereferenced-value-is-always-null] Extension methods are static calls in C# — invoking on null is valid and intentionally tests null-handling behavior
            var result = address.ToNumericString();

            // Assert
            Assert.Null(result);
        }

        #endregion // end: ToNumericString

        #region NetmaskToCidrRoutePrefix

        /// <summary>Gets theory data for <see cref="NetmaskToCidrRoutePrefix_ValidNetmask_ReturnsRoutePrefix_Test"/>.</summary>
        /// <value>Parameters: expected (int), address (IPAddress).</value>
        public static TheoryData<int, IPAddress> NetmaskToCidrRoutePrefix_ValidNetmask_ReturnsRoutePrefix_Test_Values
        {
            get
            {
                var data = new TheoryData<int, IPAddress>();

                for (var i = 0; i <= 32; i++)
                {
                    var netmaskBytes = BigEndianBitWrapper.CreateMask(4, i).ToBytes();
                    data.Add(i, new IPAddress(netmaskBytes));
                }

                return data;
            }
        }

        /// <summary>Verifies that <c>NetmaskToCidrRoutePrefix</c> returns the correct CIDR prefix length for a valid IPv4 netmask.</summary>
        /// <param name="expected">The expected CIDR route prefix integer.</param>
        /// <param name="address">The netmask IP address to convert.</param>
        [Theory]
        [MemberData(nameof(NetmaskToCidrRoutePrefix_ValidNetmask_ReturnsRoutePrefix_Test_Values))]
        public void NetmaskToCidrRoutePrefix_ValidNetmask_ReturnsRoutePrefix_Test(int expected, IPAddress address)
        {
            // Arrange
            // (address provided via theory data)

            // Act
            var result = address.NetmaskToCidrRoutePrefix();

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>Gets theory data for <see cref="NetmaskToCidrRoutePrefix_InvalidNetmask_ThrowsArgumentException_Test"/>.</summary>
        /// <value>Parameters: input (string).</value>
        public static TheoryData<string> NetmaskToCidrRoutePrefix_InvalidNetmask_ThrowsArgumentException_Test_Values =>
            new()
            {
                { "::" },
                { "192.168.1.1" },
                { "0.0.0.255" },
                { "255.0.255.0" }, // non-contiguous mask
            };

        /// <summary>Verifies that <c>NetmaskToCidrRoutePrefix</c> throws <see cref="ArgumentException"/> for an address that is not a valid contiguous netmask.</summary>
        /// <param name="input">The invalid netmask IP address string to parse.</param>
        [Theory]
        [MemberData(nameof(NetmaskToCidrRoutePrefix_InvalidNetmask_ThrowsArgumentException_Test_Values))]
        public void NetmaskToCidrRoutePrefix_InvalidNetmask_ThrowsArgumentException_Test(string input)
        {
            // Arrange
            var address = IPAddress.Parse(input);

            // Act / Assert
            Assert.Throws<ArgumentException>(() => address.NetmaskToCidrRoutePrefix());
        }

        /// <summary>Verifies that <c>NetmaskToCidrRoutePrefix</c> throws <see cref="ArgumentNullException"/> when the input address is <see langword="null"/>.</summary>
        [Fact]
        public void NetmaskToCidrRoutePrefix_NullInput_ThrowsArgumentNullException_Test()
        {
            // Arrange
            IPAddress address = null;

            // Act / Assert
            Assert.Throws<ArgumentNullException>(() => address.NetmaskToCidrRoutePrefix());
        }

        #endregion // end: NetmaskToCidrRoutePrefix
    }
}
