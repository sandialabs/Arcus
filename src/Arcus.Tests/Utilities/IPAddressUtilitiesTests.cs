using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using Arcus.Math;
using Arcus.Utilities;

namespace Arcus.Tests.Utilities
{
    /// <summary>Unit tests for <see cref="IPAddressUtilities"/>.</summary>
    public partial class IPAddressUtilitiesTests
    {
        #region IPv4MaxAddress

        /// <summary>Verifies that <see cref="IPAddressUtilities.IPv4MaxAddress"/> returns 255.255.255.255.</summary>
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

        /// <summary>Verifies that <see cref="IPAddressUtilities.IPv4MinAddress"/> returns 0.0.0.0.</summary>
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

        /// <summary>Verifies that <see cref="IPAddressUtilities.IPv4OctetCount"/> equals 4.</summary>
        [Fact]
        public void IPv4OctetCount_Test()
        {
            // Arrange
            // Act
            // Assert
            Assert.Equal(4, IPAddressUtilities.IPv4OctetCount);
        }

        #endregion // end: IPv4OctetCount

        #region IPv6HextetCount

        /// <summary>Verifies that <see cref="IPAddressUtilities.IPv6HextetCount"/> equals 8.</summary>
        [Fact]
        public void IPv6HextetCount_Test()
        {
            // Arrange
            // Act
            // Assert
            Assert.Equal(8, IPAddressUtilities.IPv6HextetCount);
        }

        #endregion // end: IPv6HextetCount

        #region IPv6MaxAddress

        /// <summary>Verifies that <see cref="IPAddressUtilities.IPv6MaxAddress"/> returns ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff.</summary>
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

        /// <summary>Verifies that <see cref="IPAddressUtilities.IPv6MinAddress"/> returns the all-zeros IPv6 address.</summary>
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

        /// <summary>Gets theory data for <see cref="IsIPv4_ReturnsExpected_Test"/>.</summary>
        /// <value>Parameters: expected (bool), input (string).</value>
        public static TheoryData<bool, string> IsIPv4_Test_Data =>
            new()
            {
                { false, null },
                { false, "::" },
                { false, "ffff::" },
                { true, "192.168.1.1" },
                { true, "0.0.0.0" },
            };

        /// <summary>Verifies that <see cref="IPAddressUtilities.IsIPv4"/> returns the expected result for the given input.</summary>
        /// <param name="expected">Expected result of the <see cref="IPAddressUtilities.IsIPv4"/> call.</param>
        /// <param name="input">String representation of the IP address to test.</param>
        [Theory]
        [MemberData(nameof(IsIPv4_Test_Data))]
        public void IsIPv4_ReturnsExpected_Test(bool expected, string input)
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

        /// <summary>Gets theory data for <see cref="IsIPv4MappedIPv6_ReturnsExpected_Test"/>.</summary>
        /// <value>Parameters: expected (bool), input (string).</value>
        public static TheoryData<bool, string> IsIPv4MappedIPv6_Test_Data =>
            new()
            {
                { false, null },
                { false, "::" },
                { false, "192.168.1.1" },
                { true, "::ffff:222.1.41.90" },
                { true, "::ffff:ab:cd" },
                { false, "1234::ffff:222.1.41.90" },
                { false, "1234::ffff:ab:cd" },
            };

        /// <summary>Verifies that <see cref="IPAddressUtilities.IsIPv4MappedIPv6"/> returns the expected result for the given input.</summary>
        /// <param name="expected">Expected result of the <see cref="IPAddressUtilities.IsIPv4MappedIPv6"/> call.</param>
        /// <param name="input">String representation of the IP address to test.</param>
        [Theory]
        [MemberData(nameof(IsIPv4MappedIPv6_Test_Data))]
        public void IsIPv4MappedIPv6_ReturnsExpected_Test(bool expected, string input)
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

        /// <summary>Gets theory data for <see cref="IsIPv6_ReturnsExpected_Test"/>.</summary>
        /// <value>Parameters: expected (bool), input (string).</value>
        public static TheoryData<bool, string> IsIPv6_Test_Data =>
            new()
            {
                { false, null },
                { true, "::" },
                { true, "ffff::" },
                { false, "192.168.1.1" },
                { false, "0.0.0.0" },
            };

        /// <summary>Verifies that <see cref="IPAddressUtilities.IsIPv6"/> returns the expected result for the given input.</summary>
        /// <param name="expected">Expected result of the <see cref="IPAddressUtilities.IsIPv6"/> call.</param>
        /// <param name="input">String representation of the IP address to test.</param>
        [Theory]
        [MemberData(nameof(IsIPv6_Test_Data))]
        public void IsIPv6_ReturnsExpected_Test(bool expected, string input)
        {
            // Arrange
            _ = IPAddress.TryParse(input, out var address);

            // Act
            var result = address.IsIPv6();

            // Assert
            Assert.Equal(expected, result);
        }

        #endregion // end: IsIPv6

        #region shared helpers (private)

        private static IEnumerable<AddressFamily> NonStandardAddressFamilies()
        {
#if NET48
            return Enum.GetValues(typeof(AddressFamily))
                .Cast<AddressFamily>()
                .Except([AddressFamily.InterNetwork, AddressFamily.InterNetworkV6]);
#else
            return Enum.GetValues<AddressFamily>().Except([AddressFamily.InterNetwork, AddressFamily.InterNetworkV6]);
#endif
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

        #endregion // end: shared helpers (private)

        #region IsValidNetMask

        /// <summary>Gets theory data for <see cref="IsValidNetMask_ReturnsExpected_Test"/>.</summary>
        /// <returns>Parameters: expected (bool), input (IPAddress).</returns>
        public static TheoryData<bool, IPAddress> IsValidNetMask_Test_Data()
        {
            var data = new TheoryData<bool, IPAddress>();

            // all valid netmask values
            for (var i = 0; i <= 32; i++)
            {
                var netmaskBytes = BigEndianBitWrapper.CreateMask(4, i).ToBytes();
                data.Add(true, new IPAddress(netmaskBytes));
            }

            data.Add(false, null);

            var invalidNetmaskAddressStrings = new[]
            {
                "::",
                "ffff::",
                "255.255.0.255",
                "255.0.255.255",
                "0.255.255.255",
                "0.0.0.255",
                "0.0.0.1",
            };

            foreach (var s in invalidNetmaskAddressStrings)
            {
                data.Add(false, IPAddress.Parse(s));
            }

            return data;
        }

        /// <summary>Verifies that <see cref="IPAddressUtilities.IsValidNetMask"/> returns the expected result for the given input.</summary>
        /// <param name="expected">Expected result of the <see cref="IPAddressUtilities.IsValidNetMask"/> call.</param>
        /// <param name="input">IP address to test.</param>
        [Theory]
        [MemberData(nameof(IsValidNetMask_Test_Data))]
        public void IsValidNetMask_ReturnsExpected_Test(bool expected, IPAddress input)
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

        /// <summary>Gets theory data for hex-string parse tests with valid inputs.</summary>
        /// <returns>Parameters: expected (IPAddress), addressString (string), addressFamily (AddressFamily).</returns>
        public static TheoryData<IPAddress, string, AddressFamily> ParseFromHexString_Valid_Test_Data()
        {
            var data = new TheoryData<IPAddress, string, AddressFamily>();

            foreach (var address in HexParseAddresses())
            {
                var asHex = AddressToHexString(address);

                data.Add(address, asHex.ToUpperInvariant(), address.AddressFamily);
                data.Add(address, asHex.ToLowerInvariant(), address.AddressFamily);
                data.Add(address, $"0x{asHex}".ToUpperInvariant(), address.AddressFamily);
                data.Add(address, $"0x{asHex}".ToLowerInvariant(), address.AddressFamily);

                var msbZeroTrim = new string([.. asHex.SkipWhile(c => c == '0')]);

                if (!string.IsNullOrEmpty(msbZeroTrim))
                {
                    data.Add(address, msbZeroTrim.ToUpperInvariant(), address.AddressFamily);
                    data.Add(address, msbZeroTrim.ToLowerInvariant(), address.AddressFamily);
                    data.Add(address, $"0x{msbZeroTrim}".ToUpperInvariant(), address.AddressFamily);
                    data.Add(address, $"0x{msbZeroTrim}".ToLowerInvariant(), address.AddressFamily);
                }
            }

            data.Add(IPAddress.Parse("128.128.128.128"), "00000000080808080", AddressFamily.InterNetwork);

            return data;
        }

        /// <summary>Gets theory data for hex-string parse tests with invalid address families.</summary>
        /// <returns>Parameters: addressFamily (AddressFamily).</returns>
        public static TheoryData<AddressFamily> ParseFromHexString_InvalidAddressFamily_Test_Data()
        {
            var data = new TheoryData<AddressFamily>();
            foreach (var af in NonStandardAddressFamilies())
            {
                data.Add(af);
            }

            return data;
        }

        /// <summary>Verifies that <see cref="IPAddressUtilities.ParseFromHexString"/> returns the expected address for valid hex inputs.</summary>
        /// <param name="expected">Expected parsed IP address.</param>
        /// <param name="addressString">Hex string representation of the address.</param>
        /// <param name="addressFamily">Address family used to parse the hex string.</param>
        [Theory]
        [MemberData(nameof(ParseFromHexString_Valid_Test_Data))]
        public void ParseFromHexString_ValidInput_ReturnsExpected_Test(
            IPAddress expected,
            string addressString,
            AddressFamily addressFamily
        )
        {
            // Arrange
            // Act
            var result = IPAddressUtilities.ParseFromHexString(addressString, addressFamily);

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>Verifies that <see cref="IPAddressUtilities.ParseFromHexString"/> parses all-zero input as the zero address rather than throwing.</summary>
        /// <param name="input">An all-zero hex string (optionally "0x"-prefixed).</param>
        /// <param name="addressFamily">The target address family.</param>
        [Theory]
        [InlineData("0", AddressFamily.InterNetwork)]
        [InlineData("00", AddressFamily.InterNetwork)]
        [InlineData("0x0", AddressFamily.InterNetwork)]
        [InlineData("0x00", AddressFamily.InterNetwork)]
        [InlineData("0", AddressFamily.InterNetworkV6)]
        [InlineData("0x0", AddressFamily.InterNetworkV6)]
        public void ParseFromHexString_AllZeroInput_ReturnsZeroAddress_Test(string input, AddressFamily addressFamily)
        {
            // Arrange
            var expected = addressFamily == AddressFamily.InterNetwork ? IPAddress.Any : IPAddress.IPv6Any;

            // Act
            var result = IPAddressUtilities.ParseFromHexString(input, addressFamily);

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>Verifies that <see cref="IPAddressUtilities.ParseFromHexString"/> throws <see cref="ArgumentNullException"/> when given a null input.</summary>
        [Fact]
        public void ParseFromHexString_NullInput_Throws_ArgumentNullException_Test()
        {
            // Arrange
            // Act
            // Assert
            Assert.Throws<ArgumentNullException>(() => IPAddressUtilities.ParseFromHexString(null, default));
        }

        /// <summary>Verifies that <see cref="IPAddressUtilities.ParseFromHexString"/> throws <see cref="ArgumentException"/> for empty or whitespace input.</summary>
        /// <param name="input">Empty or whitespace string to test.</param>
        [Theory]
        [InlineData("")]
        [InlineData("\t")]
        [InlineData(" ")]
        public void ParseFromHexString_EmptyOrWhitespaceInput_Throws_ArgumentException_Test(string input)
        {
            // Arrange
            // Act
            // Assert
            Assert.Throws<ArgumentException>(() => IPAddressUtilities.ParseFromHexString(input, AddressFamily.InterNetwork));
        }

        /// <summary>Verifies that <see cref="IPAddressUtilities.ParseFromHexString"/> throws <see cref="ArgumentException"/> for non-standard address families.</summary>
        /// <param name="addressFamily">Non-standard address family to test.</param>
        [Theory]
        [MemberData(nameof(ParseFromHexString_InvalidAddressFamily_Test_Data))]
        public void ParseFromHexString_InvalidAddressFamily_Throws_ArgumentException_Test(AddressFamily addressFamily)
        {
            // Arrange
            // Act
            // Assert
            Assert.Throws<ArgumentException>(() => IPAddressUtilities.ParseFromHexString("abc123", addressFamily));
        }

        /// <summary>Verifies that <see cref="IPAddressUtilities.ParseFromHexString"/> throws <see cref="ArgumentException"/> for non-hex input strings.</summary>
        /// <param name="input">Non-hex string to test.</param>
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

        /// <summary>Verifies that <see cref="IPAddressUtilities.ParseFromHexString"/> throws <see cref="ArgumentOutOfRangeException"/> when an IPv6-length hex string is parsed as IPv4.</summary>
        [Fact]
        public void ParseFromHexString_IPv6HexTooLargeForIPv4_Throws_ArgumentOutOfRangeException_Test()
        {
            // Arrange
            var ipv6HexString = AddressToHexString(IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff"));

            // Act
            // Assert
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                IPAddressUtilities.ParseFromHexString(ipv6HexString, AddressFamily.InterNetwork)
            );
        }

        /// <summary>Verifies that <see cref="IPAddressUtilities.TryParseFromHexString"/> returns <c>true</c> and the expected address for valid hex inputs.</summary>
        /// <param name="expected">Expected parsed IP address.</param>
        /// <param name="addressString">Hex string representation of the address.</param>
        /// <param name="addressFamily">Address family used to parse the hex string.</param>
        [Theory]
        [MemberData(nameof(ParseFromHexString_Valid_Test_Data))]
        public void TryParseFromHexString_ValidInput_ReturnsTrue_Test(
            IPAddress expected,
            string addressString,
            AddressFamily addressFamily
        )
        {
            // Arrange
            // Act
            var success = IPAddressUtilities.TryParseFromHexString(addressString, addressFamily, out var result);

            // Assert
            Assert.True(success);
            Assert.Equal(expected, result);
        }

        /// <summary>Verifies that <see cref="IPAddressUtilities.TryParseFromHexString"/> returns <c>false</c> and a null result when given null input.</summary>
        [Fact]
        public void TryParseFromHexString_NullInput_ReturnsFalse_Test()
        {
            // Arrange
            // Act
            var success = IPAddressUtilities.TryParseFromHexString(null, AddressFamily.InterNetwork, out var result);

            // Assert
            Assert.False(success);
            Assert.Null(result);
        }

        /// <summary>Verifies that <see cref="IPAddressUtilities.TryParseFromHexString"/> returns <c>false</c> for non-standard address families.</summary>
        /// <param name="addressFamily">Non-standard address family to test.</param>
        [Theory]
        [MemberData(nameof(ParseFromHexString_InvalidAddressFamily_Test_Data))]
        public void TryParseFromHexString_InvalidAddressFamily_ReturnsFalse_Test(AddressFamily addressFamily)
        {
            // Arrange
            // Act
            var success = IPAddressUtilities.TryParseFromHexString("abc123", addressFamily, out var result);

            // Assert
            Assert.False(success);
            Assert.Null(result);
        }

        private static IEnumerable<IPAddress> HexParseAddresses()
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

        private static string AddressToHexString(IPAddress address)
        {
            return string.Concat(address.GetAddressBytes().Select(b => Convert.ToString(b, 16).PadLeft(2, '0')));
        }

        #endregion // end: ParseFromHexString / TryParseFromHexString

        #region ParseIgnoreOctalInIPv4 / TryParseIgnoreOctalInIPv4

        /// <summary>Gets theory data for <see cref="ParseIgnoreOctalInIPv4_ValidInput_ReturnsExpected_Test"/> and related tests.</summary>
        /// <returns>Parameters: expected (IPAddress), input (string).</returns>
        public static TheoryData<IPAddress, string> ParseIgnoreOctalInIPv4_Valid_Test_Data()
        {
            var data = new TheoryData<IPAddress, string>();

            foreach (var address in OctalParseAddresses())
            {
                data.Add(address, address.ToString());

                if (address.AddressFamily == AddressFamily.InterNetwork)
                {
                    var quads = AddressToQuads(address);
                    if (quads != address.ToString())
                    {
                        data.Add(address, quads);
                    }
                }
            }

            data.Add(IPAddress.Parse("0.0.0.192"), "192");
            data.Add(IPAddress.Parse("1.0.0.192"), "1.192");
            data.Add(IPAddress.Parse("1.255.0.192"), "1.255.192");

            // octal case
            data.Add(IPAddress.Parse("7.7.7.0"), "007.007.7.0");

            return data;
        }

        /// <summary>Gets theory data for <see cref="ParseIgnoreOctalInIPv4_InvalidInput_Throws_Test"/> and related tests.</summary>
        /// <value>Parameters: input (string).</value>
        public static TheoryData<string> ParseIgnoreOctalInIPv4_Invalid_Test_Data =>
            new() { { "potato" }, { "255.255.255.255.255" } };

        /// <summary>Verifies that <see cref="IPAddressUtilities.ParseIgnoreOctalInIPv4"/> returns the expected address for valid inputs.</summary>
        /// <param name="expected">Expected parsed IP address.</param>
        /// <param name="input">String representation of the address to parse.</param>
        [Theory]
        [MemberData(nameof(ParseIgnoreOctalInIPv4_Valid_Test_Data))]
        public void ParseIgnoreOctalInIPv4_ValidInput_ReturnsExpected_Test(IPAddress expected, string input)
        {
            // Arrange
            // Act
            var result = IPAddressUtilities.ParseIgnoreOctalInIPv4(input);

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>Verifies that <see cref="IPAddressUtilities.ParseIgnoreOctalInIPv4"/> throws <see cref="ArgumentNullException"/> for null input.</summary>
        [Fact]
        public void ParseIgnoreOctalInIPv4_NullInput_Throws_ArgumentNullException_Test()
        {
            // Arrange
            // Act
            // Assert
            Assert.Throws<ArgumentNullException>(() => IPAddressUtilities.ParseIgnoreOctalInIPv4(null));
        }

        /// <summary>Verifies that <see cref="IPAddressUtilities.ParseIgnoreOctalInIPv4"/> throws <see cref="ArgumentException"/> for empty or whitespace input.</summary>
        /// <param name="input">Empty or whitespace string to test.</param>
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

        /// <summary>Verifies that <see cref="IPAddressUtilities.ParseIgnoreOctalInIPv4"/> throws for invalid input strings.</summary>
        /// <param name="input">Invalid address string to test.</param>
        [Theory]
        [MemberData(nameof(ParseIgnoreOctalInIPv4_Invalid_Test_Data))]
        public void ParseIgnoreOctalInIPv4_InvalidInput_Throws_Test(string input)
        {
            // Arrange
            // Act
            // Assert
            Assert.ThrowsAny<Exception>(() => IPAddressUtilities.ParseIgnoreOctalInIPv4(input));
        }

        /// <summary>Verifies that <see cref="IPAddressUtilities.TryParseIgnoreOctalInIPv4"/> returns <c>true</c> and the expected address for valid inputs.</summary>
        /// <param name="expected">Expected parsed IP address.</param>
        /// <param name="input">String representation of the address to parse.</param>
        [Theory]
        [MemberData(nameof(ParseIgnoreOctalInIPv4_Valid_Test_Data))]
        public void TryParseIgnoreOctalInIPv4_ValidInput_ReturnsTrue_Test(IPAddress expected, string input)
        {
            // Arrange
            // Act
            var success = IPAddressUtilities.TryParseIgnoreOctalInIPv4(input, out var result);

            // Assert
            Assert.True(success);
            Assert.Equal(expected, result);
        }

        /// <summary>Verifies that <see cref="IPAddressUtilities.TryParseIgnoreOctalInIPv4"/> returns <c>false</c> and a null result when given null input.</summary>
        [Fact]
        public void TryParseIgnoreOctalInIPv4_NullInput_ReturnsFalse_Test()
        {
            // Arrange
            // Act
            var success = IPAddressUtilities.TryParseIgnoreOctalInIPv4(null, out var result);

            // Assert
            Assert.False(success);
            Assert.Null(result);
        }

        /// <summary>Verifies that <see cref="IPAddressUtilities.TryParseIgnoreOctalInIPv4"/> returns <c>false</c> for invalid input strings.</summary>
        /// <param name="input">Invalid address string to test.</param>
        [Theory]
        [MemberData(nameof(ParseIgnoreOctalInIPv4_Invalid_Test_Data))]
        public void TryParseIgnoreOctalInIPv4_InvalidInput_ReturnsFalse_Test(string input)
        {
            // Arrange
            // Act
            var success = IPAddressUtilities.TryParseIgnoreOctalInIPv4(input, out var result);

            // Assert
            Assert.False(success);
            Assert.Null(result);
        }

        private static IEnumerable<IPAddress> OctalParseAddresses()
        {
            yield return IPAddress.Any;
            yield return IPAddress.Loopback;
            yield return IPAddress.None;
            yield return IPAddress.Parse("7.7.7.7");
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

        private static string AddressToQuads(IPAddress address)
        {
            return string.Join(".", address.GetAddressBytes().Select(b => Convert.ToString(b, 10).PadLeft(3, '0')));
        }

        #endregion // end: ParseIgnoreOctalInIPv4 / TryParseIgnoreOctalInIPv4

        #region HexLikePattern

        /// <summary>Gets theory data for <see cref="HexLikePattern_IsMatch_ReturnsExpected_Test"/>.</summary>
        /// <value>Parameters: expected (bool), input (string).</value>
        public static TheoryData<bool, string> HexLikePattern_Test_Data =>
            new()
            {
                // matching - lowercase hex digits
                { true, "0123456789abcdef" },
                // matching - uppercase hex digits (IgnoreCase)
                { true, "0123456789ABCDEF" },
                // matching - mixed case
                { true, "DeAdBeEf" },
                // matching - digits only
                { true, "0000" },
                // matching - empty string (pattern uses '*', allows zero chars)
                { true, string.Empty },
                // non-matching - 'g' and beyond are not hex
                { false, "abcdefg" },
                { false, "xyz" },
                // non-matching - '0x' prefix contains 'x'
                { false, "0x1A" },
                // non-matching - space or punctuation
                { false, "12 34" },
                { false, "!" },
            };

        /// <summary>Verifies that <see cref="IPAddressUtilities.HexLikePattern"/> matches expected inputs.</summary>
        /// <param name="expected">Whether the pattern is expected to match the input.</param>
        /// <param name="input">Input string to match against the hex pattern.</param>
        [Theory]
        [MemberData(nameof(HexLikePattern_Test_Data))]
        public void HexLikePattern_IsMatch_ReturnsExpected_Test(bool expected, string input)
        {
            // Arrange
            var regex = MyRegex();

            // Act
            var result = regex.IsMatch(input);

            // Assert
            Assert.Equal(expected, result);
        }

        #endregion // end: HexLikePattern

        #region DottedQuadRegularExpressionPattern

        /// <summary>Gets theory data for <see cref="DottedQuadRegularExpressionPattern_IsMatch_ReturnsExpected_Test"/>.</summary>
        /// <value>Parameters: expected (bool), input (string).</value>
        public static TheoryData<bool, string> DottedQuadRegularExpressionPattern_Test_Data =>
            new()
            {
                // matching - well-formed dotted quads (pattern checks format, not address validity)
                { true, "192.168.1.1" },
                { true, "0.0.0.0" },
                { true, "255.255.255.255" },
                // matching - out-of-range values pass (pattern is format-only)
                { true, "999.999.999.999" },
                // non-matching - too few groups
                { false, "192.168.1" },
                { false, "192.168" },
                { false, "192" },
                // non-matching - too many groups
                { false, "192.168.1.1.5" },
                // non-matching - empty or non-numeric
                { false, string.Empty },
                { false, "::" },
                { false, "potato" },
                // non-matching - 4-digit group exceeds {1,3}
                { false, "1234.1.1.1" },
            };

        /// <summary>Verifies that <see cref="IPAddressUtilities.DottedQuadRegularExpressionPattern"/> matches expected dotted-quad inputs.</summary>
        /// <param name="expected">Whether the pattern is expected to match the input.</param>
        /// <param name="input">Input string to match against the dotted-quad pattern.</param>
        [Theory]
        [MemberData(nameof(DottedQuadRegularExpressionPattern_Test_Data))]
        public void DottedQuadRegularExpressionPattern_IsMatch_ReturnsExpected_Test(bool expected, string input)
        {
            // Arrange
            var regex = new Regex(IPAddressUtilities.DottedQuadRegularExpressionPattern, RegexOptions.CultureInvariant);

            // Act
            var result = regex.IsMatch(input);

            // Assert
            Assert.Equal(expected, result);
        }

        #endregion // end: DottedQuadRegularExpressionPattern

        #region DottedQuadLeadingZerosPattern

        /// <summary>Gets theory data for <see cref="DottedQuadLeadingZerosPattern_Replace_ReturnsExpected_Test"/>.</summary>
        /// <value>Parameters: expected (string), input (string).</value>
        public static TheoryData<string, string> DottedQuadLeadingZerosPattern_Replace_Test_Data =>
            new()
            {
                // leading zeros stripped from each octet
                { "7.7.7.0", "007.007.7.0" },
                { "1.2.3.4", "001.002.003.004" },
                // lone-zero octets preserved (lookahead prevents stripping the only '0')
                { "0.0.0.0", "000.000.000.000" },
                { "0.0.0.0", "0.0.0.0" },
                // no leading zeros - no change
                { "192.168.1.0", "192.168.1.0" },
                { "0.1.0.1", "0.1.0.1" },
            };

        /// <summary>Verifies that <see cref="IPAddressUtilities.DottedQuadLeadingZerosPattern"/> strips leading zeros correctly.</summary>
        /// <param name="expected">Expected string after replacing leading zeros.</param>
        /// <param name="input">Input dotted-quad string to process.</param>
        [Theory]
        [MemberData(nameof(DottedQuadLeadingZerosPattern_Replace_Test_Data))]
        public void DottedQuadLeadingZerosPattern_Replace_ReturnsExpected_Test(string expected, string input)
        {
            // Arrange
            var regex = new Regex(IPAddressUtilities.DottedQuadLeadingZerosPattern, RegexOptions.CultureInvariant);

            // Act
            var result = regex.Replace(input, string.Empty);

            // Assert
            Assert.Equal(expected, result);
        }

        #endregion // end: DottedQuadLeadingZerosPattern

        #region Parse(byte[]) / TryParse(byte[])

        /// <summary>Gets theory data for byte-array parse tests with valid inputs.</summary>
        /// <returns>Parameters: expected (IPAddress), bytes (byte[]), addressFamily (AddressFamily).</returns>
        public static TheoryData<IPAddress, byte[], AddressFamily> Parse_ByteArray_Valid_Test_Data()
        {
            var data = new TheoryData<IPAddress, byte[], AddressFamily>();

            foreach (var address in GeneralPurposeIPv4Addresses().Concat(GeneralPurposeIPv6Addresses()))
            {
                data.Add(address, [.. address.GetAddressBytes()], address.AddressFamily);
            }

            // underflow - pad with MSB zeros
            data.Add(IPAddress.Parse("0.0.0.0"), [], AddressFamily.InterNetwork);
            data.Add(IPAddress.Parse("::"), [], AddressFamily.InterNetworkV6);
            data.Add(IPAddress.Parse("0.0.0.255"), [0x00, 0xff], AddressFamily.InterNetwork);
            data.Add(IPAddress.Parse("::acca"), [0x00, 0x00, 0xac, 0xca], AddressFamily.InterNetworkV6);

            return data;
        }

        /// <summary>Verifies that <see cref="IPAddressUtilities.Parse(byte[], System.Net.Sockets.AddressFamily)"/> returns the expected address for valid byte array inputs.</summary>
        /// <param name="expected">Expected parsed IP address.</param>
        /// <param name="bytes">Byte array to parse.</param>
        /// <param name="addressFamily">Address family used to interpret the byte array.</param>
        [Theory]
        [MemberData(nameof(Parse_ByteArray_Valid_Test_Data))]
        public void Parse_ByteArray_ValidInput_ReturnsExpected_Test(
            IPAddress expected,
            byte[] bytes,
            AddressFamily addressFamily
        )
        {
            // Arrange
            // Act
            var result = IPAddressUtilities.Parse(bytes, addressFamily);

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>Verifies that <see cref="IPAddressUtilities.Parse(byte[], System.Net.Sockets.AddressFamily)"/> throws <see cref="ArgumentNullException"/> when given a null byte array.</summary>
        [Fact]
        public void Parse_ByteArray_NullInput_Throws_ArgumentNullException_Test()
        {
            // Arrange
            // Act
            // Assert
            Assert.Throws<ArgumentNullException>(() => IPAddressUtilities.Parse(null, AddressFamily.InterNetwork));
        }

        /// <summary>Verifies that <see cref="IPAddressUtilities.Parse(byte[], System.Net.Sockets.AddressFamily)"/> throws <see cref="ArgumentOutOfRangeException"/> for non-standard address families.</summary>
        /// <param name="addressFamily">Non-standard address family to test.</param>
        [Theory]
        [MemberData(nameof(ParseFromHexString_InvalidAddressFamily_Test_Data))]
        public void Parse_ByteArray_InvalidAddressFamily_Throws_ArgumentOutOfRangeException_Test(AddressFamily addressFamily)
        {
            // Arrange
            // Act
            // Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => IPAddressUtilities.Parse([0x42], addressFamily));
        }

        /// <summary>Verifies that <see cref="IPAddressUtilities.Parse(byte[], System.Net.Sockets.AddressFamily)"/> throws <see cref="ArgumentOutOfRangeException"/> when the byte array is longer than the address family supports.</summary>
        /// <param name="count">Number of bytes in the oversized array.</param>
        /// <param name="addressFamily">Address family for which the byte array is too long.</param>
        [Theory]
        [InlineData(17, AddressFamily.InterNetworkV6)]
        [InlineData(5, AddressFamily.InterNetwork)]
        public void Parse_ByteArray_InputTooLong_Throws_ArgumentOutOfRangeException_Test(int count, AddressFamily addressFamily)
        {
            // Arrange
            var bytes = Enumerable.Repeat((byte)0x00, count).ToArray();

            // Act
            // Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => IPAddressUtilities.Parse(bytes, addressFamily));
        }

        /// <summary>Verifies that <see cref="IPAddressUtilities.TryParse(byte[], System.Net.Sockets.AddressFamily, out IPAddress)"/> returns <c>true</c> and the expected address for valid byte array inputs.</summary>
        /// <param name="expected">Expected parsed IP address.</param>
        /// <param name="bytes">Byte array to parse.</param>
        /// <param name="addressFamily">Address family used to interpret the byte array.</param>
        [Theory]
        [MemberData(nameof(Parse_ByteArray_Valid_Test_Data))]
        public void TryParse_ByteArray_ValidInput_ReturnsTrue_Test(
            IPAddress expected,
            byte[] bytes,
            AddressFamily addressFamily
        )
        {
            // Arrange
            // Act
            var success = IPAddressUtilities.TryParse(bytes, addressFamily, out var result);

            // Assert
            Assert.True(success);
            Assert.Equal(expected, result);
        }

        /// <summary>Verifies that <see cref="IPAddressUtilities.TryParse(byte[], System.Net.Sockets.AddressFamily, out IPAddress)"/> returns <c>false</c> and a null result when given a null byte array.</summary>
        [Fact]
        public void TryParse_ByteArray_NullInput_ReturnsFalse_Test()
        {
            // Arrange
            // Act
            var success = IPAddressUtilities.TryParse(null, AddressFamily.InterNetwork, out var result);

            // Assert
            Assert.False(success);
            Assert.Null(result);
        }

        /// <summary>Verifies that <see cref="IPAddressUtilities.TryParse(byte[], System.Net.Sockets.AddressFamily, out IPAddress)"/> returns <c>false</c> for non-standard address families.</summary>
        /// <param name="addressFamily">Non-standard address family to test.</param>
        [Theory]
        [MemberData(nameof(ParseFromHexString_InvalidAddressFamily_Test_Data))]
        public void TryParse_ByteArray_InvalidAddressFamily_ReturnsFalse_Test(AddressFamily addressFamily)
        {
            // Arrange
            // Act
            var success = IPAddressUtilities.TryParse([0x42], addressFamily, out var result);

            // Assert
            Assert.False(success);
            Assert.Null(result);
        }

        #endregion // end: Parse(byte[]) / TryParse(byte[])

        #endregion // end: Parse / TryParse

        #region MaxIPAddress

        /// <summary>Verifies that <see cref="IPAddressUtilities.MaxIPAddress"/> returns <see cref="IPAddressUtilities.IPv4MaxAddress"/> for <see cref="AddressFamily.InterNetwork"/>.</summary>
        [Fact]
        public void MaxIPAddress_IPv4_ReturnsIPv4MaxAddress_Test()
        {
            // Arrange
            // Act
            var result = AddressFamily.InterNetwork.MaxIPAddress();

            // Assert
            Assert.Same(IPAddressUtilities.IPv4MaxAddress, result);
        }

        /// <summary>Verifies that <see cref="IPAddressUtilities.MaxIPAddress"/> returns <see cref="IPAddressUtilities.IPv6MaxAddress"/> for <see cref="AddressFamily.InterNetworkV6"/>.</summary>
        [Fact]
        public void MaxIPAddress_IPv6_ReturnsIPv6MaxAddress_Test()
        {
            // Arrange
            // Act
            var result = AddressFamily.InterNetworkV6.MaxIPAddress();

            // Assert
            Assert.Same(IPAddressUtilities.IPv6MaxAddress, result);
        }

        /// <summary>Verifies that <see cref="IPAddressUtilities.MaxIPAddress"/> throws <see cref="ArgumentException"/> for non-standard address families.</summary>
        /// <param name="addressFamily">Non-standard address family to test.</param>
        [Theory]
        [MemberData(nameof(ParseFromHexString_InvalidAddressFamily_Test_Data))]
        public void MaxIPAddress_InvalidAddressFamily_Throws_ArgumentException_Test(AddressFamily addressFamily)
        {
            // Arrange
            // Act
            // Assert
            Assert.Throws<ArgumentException>(() => addressFamily.MaxIPAddress());
        }

        #endregion // end: MaxIPAddress

        #region MinIPAddress

        /// <summary>Verifies that <see cref="IPAddressUtilities.MinIPAddress"/> returns <see cref="IPAddressUtilities.IPv4MinAddress"/> for <see cref="AddressFamily.InterNetwork"/>.</summary>
        [Fact]
        public void MinIPAddress_IPv4_ReturnsIPv4MinAddress_Test()
        {
            // Arrange
            // Act
            var result = AddressFamily.InterNetwork.MinIPAddress();

            // Assert
            Assert.Same(IPAddressUtilities.IPv4MinAddress, result);
        }

        /// <summary>Verifies that <see cref="IPAddressUtilities.MinIPAddress"/> returns <see cref="IPAddressUtilities.IPv6MinAddress"/> for <see cref="AddressFamily.InterNetworkV6"/>.</summary>
        [Fact]
        public void MinIPAddress_IPv6_ReturnsIPv6MinAddress_Test()
        {
            // Arrange
            // Act
            var result = AddressFamily.InterNetworkV6.MinIPAddress();

            // Assert
            Assert.Same(IPAddressUtilities.IPv6MinAddress, result);
        }

        /// <summary>Verifies that <see cref="IPAddressUtilities.MinIPAddress"/> throws <see cref="ArgumentException"/> for non-standard address families.</summary>
        /// <param name="addressFamily">Non-standard address family to test.</param>
        [Theory]
        [MemberData(nameof(ParseFromHexString_InvalidAddressFamily_Test_Data))]
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

        /// <summary>Verifies that <see cref="IPAddressUtilities.IPv4BitCount"/> equals 32.</summary>
        [Fact]
        public void IPv4BitCount_Value_IsThirtyTwo_Test()
        {
            // Arrange
            // Act
            // Assert
            Assert.Equal(32, IPAddressUtilities.IPv4BitCount);
        }

        /// <summary>Verifies that <see cref="IPAddressUtilities.IPv4BitCount"/> matches the actual bit count of an IPv4 address.</summary>
        [Fact]
        public void IPv4BitCount_MatchesIPv4ByteCount_Test()
        {
            // Arrange
            // Act
            // Assert
            Assert.Equal(IPAddressUtilities.IPv4BitCount, IPAddress.Any.GetAddressBytes().Length * 8);
        }

        /// <summary>Verifies that <see cref="IPAddressUtilities.IPv6BitCount"/> equals 128.</summary>
        [Fact]
        public void IPv6BitCount_Value_IsOneTwentyEight_Test()
        {
            // Arrange
            // Act
            // Assert
            Assert.Equal(128, IPAddressUtilities.IPv6BitCount);
        }

        /// <summary>Verifies that <see cref="IPAddressUtilities.IPv6BitCount"/> matches the actual bit count of an IPv6 address.</summary>
        [Fact]
        public void IPv6BitCount_MatchesIPv6ByteCount_Test()
        {
            // Arrange
            // Act
            // Assert
            Assert.Equal(IPAddressUtilities.IPv6BitCount, IPAddress.IPv6Any.GetAddressBytes().Length * 8);
        }

        #endregion // end: BitCount

        #region ByteCount

        /// <summary>Verifies that <see cref="IPAddressUtilities.IPv4ByteCount"/> equals 4.</summary>
        [Fact]
        public void IPv4ByteCount_Value_IsFour_Test()
        {
            // Arrange
            // Act
            // Assert
            Assert.Equal(4, IPAddressUtilities.IPv4ByteCount);
        }

        /// <summary>Verifies that <see cref="IPAddressUtilities.IPv4ByteCount"/> matches the actual byte count of an IPv4 address.</summary>
        [Fact]
        public void IPv4ByteCount_MatchesIPv4AddressBytes_Test()
        {
            // Arrange
            // Act
            // Assert
            Assert.Equal(IPAddressUtilities.IPv4ByteCount, IPAddress.Any.GetAddressBytes().Length);
        }

        /// <summary>Verifies that <see cref="IPAddressUtilities.IPv6ByteCount"/> equals 16.</summary>
        [Fact]
        public void IPv6ByteCount_Value_IsSixteen_Test()
        {
            // Arrange
            // Act
            // Assert
            Assert.Equal(16, IPAddressUtilities.IPv6ByteCount);
        }

        /// <summary>Verifies that <see cref="IPAddressUtilities.IPv6ByteCount"/> matches the actual byte count of an IPv6 address.</summary>
        [Fact]
        public void IPv6ByteCount_MatchesIPv6AddressBytes_Test()
        {
            // Arrange
            // Act
            // Assert
            Assert.Equal(IPAddressUtilities.IPv6ByteCount, IPAddress.IPv6Any.GetAddressBytes().Length);
        }

        #endregion // end: ByteCount

        #region MaxAddress

        /// <summary>Verifies that <see cref="IPAddressUtilities.IPv6MaxAddress"/> has all bytes set to 0xFF and is an IPv6 address.</summary>
        [Fact]
        public void IPv6MaxAddress_Value_IsAllFF_Test()
        {
            // Arrange
            // Act
            var address = IPAddressUtilities.IPv6MaxAddress;

            // Assert
            Assert.Equal(new IPAddress([.. Enumerable.Repeat((byte)0xff, 16)]), address);
            Assert.Equal(AddressFamily.InterNetworkV6, address.AddressFamily);
        }

        /// <summary>Verifies that <see cref="IPAddressUtilities.IPv4MaxAddress"/> has all bytes set to 0xFF and is an IPv4 address.</summary>
        [Fact]
        public void IPv4MaxAddress_Value_IsAllFF_Test()
        {
            // Arrange
            // Act
            var address = IPAddressUtilities.IPv4MaxAddress;

            // Assert
            Assert.Equal(new IPAddress([.. Enumerable.Repeat((byte)0xff, 4)]), address);
            Assert.Equal(AddressFamily.InterNetwork, address.AddressFamily);
        }

        #endregion // end: MaxAddress

        #region MinAddress

        /// <summary>Verifies that <see cref="IPAddressUtilities.IPv6MinAddress"/> has all bytes set to 0x00 and is an IPv6 address.</summary>
        [Fact]
        public void IPv6MinAddress_Value_IsAllZero_Test()
        {
            // Arrange
            // Act
            var address = IPAddressUtilities.IPv6MinAddress;

            // Assert
            Assert.Equal(new IPAddress([.. Enumerable.Repeat((byte)0x00, 16)]), address);
            Assert.Equal(AddressFamily.InterNetworkV6, address.AddressFamily);
        }

        /// <summary>Verifies that <see cref="IPAddressUtilities.IPv4MinAddress"/> has all bytes set to 0x00 and is an IPv4 address.</summary>
        [Fact]
        public void IPv4MinAddress_Value_IsAllZero_Test()
        {
            // Arrange
            // Act
            var address = IPAddressUtilities.IPv4MinAddress;

            // Assert
            Assert.Equal(new IPAddress([.. Enumerable.Repeat((byte)0x00, 4)]), address);
            Assert.Equal(AddressFamily.InterNetwork, address.AddressFamily);
        }

        #endregion // end: MinAddress

        #region ValidAddressFamilies

        /// <summary>Verifies that <see cref="IPAddressUtilities.ValidAddressFamilies"/> is a read-only collection containing both IPv4 and IPv6 families.</summary>
        [Fact]
        public void ValidAddressFamilies_IsReadOnlyCollection_ContainsBothFamilies_Test()
        {
            // Arrange
            var validAddressFamilies = IPAddressUtilities.ValidAddressFamilies;

            // Act

            // Assert
            Assert.IsType<IReadOnlyCollection<AddressFamily>>(validAddressFamilies, exactMatch: false);
            Assert.Equal(2, validAddressFamilies.Length);
            Assert.Contains(AddressFamily.InterNetworkV6, validAddressFamilies);
            Assert.Contains(AddressFamily.InterNetwork, validAddressFamilies);
        }

        /// <summary>Verifies that <see cref="IPAddressUtilities.ValidAddressFamilies"/> is backed by an immutable array rather than a wrapped <see cref="List{AddressFamily}"/>.</summary>
        [Fact]
        public void ValidAddressFamilies_IsImmutable_Test()
        {
            // Arrange
            // Act
            var validAddressFamilies = IPAddressUtilities.ValidAddressFamilies;

            // Assert
            Assert.IsType<AddressFamily[]>(validAddressFamilies);
        }

        #endregion // end: ValidAddressFamilies

        #endregion // end: Constant Values

        #region IsPrivate

        /// <summary>Gets theory data for <see cref="IsPrivate_ReturnsExpected_Test"/>.</summary>
        /// <returns>Parameters: expected (bool), address (IPAddress).</returns>
        public static TheoryData<bool, IPAddress> IsPrivate_Test_Data()
        {
            var data = new TheoryData<bool, IPAddress>();

            foreach (var subnet in SubnetUtilities.PrivateIPAddressRangesList)
            {
                data.Add(true, subnet.NetworkPrefixAddress);
                data.Add(true, subnet.NetworkPrefixAddress.Increment(2));
                data.Add(true, subnet.BroadcastAddress);
                data.Add(true, subnet.BroadcastAddress.Increment(-2));
            }

            data.Add(false, IPAddressUtilities.IPv4MaxAddress);
            data.Add(false, IPAddressUtilities.IPv4MinAddress);
            data.Add(false, IPAddressUtilities.IPv6MaxAddress);
            data.Add(false, IPAddressUtilities.IPv6MinAddress);

            return data;
        }

        /// <summary>Verifies that <see cref="IPAddressUtilities.IsPrivate"/> returns the expected result for the given address.</summary>
        /// <param name="expected">Expected result of the <see cref="IPAddressUtilities.IsPrivate"/> call.</param>
        /// <param name="address">IP address to test.</param>
        [Theory]
        [MemberData(nameof(IsPrivate_Test_Data))]
        public void IsPrivate_ReturnsExpected_Test(bool expected, IPAddress address)
        {
            // Arrange
            // Act
            var isPrivate = address.IsPrivate();

            // Assert
            Assert.Equal(expected, isPrivate);
        }

        /// <summary>Verifies that <see cref="IPAddressUtilities.IsPrivate"/> throws <see cref="ArgumentNullException"/> when given a null address.</summary>
        [Fact]
        public void IsPrivate_NullAddress_Throws_ArgumentNullException_Test()
        {
            // Arrange
            IPAddress address = null;

            // Act
            // Assert
            Assert.Throws<ArgumentNullException>(() => address.IsPrivate());
        }

#if NET48
        private static readonly Regex MyRegexField = new(
            IPAddressUtilities.HexLikePattern,
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant
        );

        private static Regex MyRegex() => MyRegexField;
#else
        [GeneratedRegex(IPAddressUtilities.HexLikePattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
        private static partial Regex MyRegex();
#endif

        #endregion // end: IsPrivate
    }
}
