using System.Net;
using Xunit;

namespace Arcus.Tests
{
    // Trusting, but verifying the compatibility of IPAddress object with Microsoft System.Net.IPAddress object
    public class IPAddressCompatibilityTests
    {
        [Theory]
        // IPv4 Addresses

        // - min IPv4 address
        [InlineData(true, "0", "0.0.0.0")] // IPv4 minimum single 0
        [InlineData(true, "0000", "0.0.0.0")] // IPv4 minimum 0000
        [InlineData(true, "0.0", "0.0.0.0")] // IPv4 minimum double 0 octets
        [InlineData(true, "0.0.0", "0.0.0.0")] // IPv4 minimum triple 0 octets
        [InlineData(true, "0.0.0.0", "0.0.0.0")] // IPv4 minimum full 0 octets
        // - max IPv4 address
        [InlineData(true, "255.255.255.255", "255.255.255.255")] // IPv4 max value full 255 octets
        // - generic IPv4 address
        [InlineData(true, "192.168.1.100", "192.168.1.100")] // IPv4 generic
        // - integer input
        [InlineData(true, "1024", "0.0.4.0")] // integer 1024
        [InlineData(true, "2048", "0.0.8.0")] // integer 2048
        [InlineData(true, "4096", "0.0.16.0")] // integer 4096
        [InlineData(true, "2147483647", "127.255.255.255")] // integer max int-1 (2147483647)
        [InlineData(true, "2147483648", "128.0.0.0")] // integer max positive
        [InlineData(true, "4294967295", "255.255.255.255")] // integer max uint
        // - fewer than 4 octet IPv4 addresses
        [InlineData(true, "192", "0.0.0.192")] // IPv4 single octet
        [InlineData(true, "192.100", "192.0.0.100")] // IPv4 first and last octets
        [InlineData(true, "192.1.100", "192.1.0.100")] // IPv4 three octets
        [InlineData(true, "064.0100.004.017", "52.64.4.15")] // IPV4 leading 00 octal notation"
        // octal notation IPv4
        [InlineData(true, "064", "0.0.0.52")] // IPv4 octal single octet
        [InlineData(true, "064.0100.04.017", "52.64.4.15")] // IPv4 full octal
        [InlineData(true, "064.100.4.18", "52.100.4.18")] // IPv4 mixed octal and decimal
        // IPv4-like invalid address string
        [InlineData(false, "256.0.0.0", null)] // IPv4-like first octet too large
        [InlineData(false, "0.256.0.0", null)] // IPv4-like second octet too large
        [InlineData(false, "0.0.256.0", null)] // IPv4-like third octet too large
        [InlineData(false, "0.0.0.256", null)] // IPv4-like fourth octet too large
        [InlineData(false, "255.255.255.255.0", null)] // IPv4-like extra 0 tail octet
        [InlineData(false, "0.255.255.255.255", null)] // IPv4-like extra 0 head octet
        [InlineData(false, "064.0100.04.018", null)] // invalid 8 in octal notation
        [InlineData(false, "192.168.1.1.", null)] // IPv4-like trailing period
        [InlineData(false, "192.168.1.1/16", null)] // invalid input IPv4 basic cidr
        [InlineData(false, "192.168.1.1/", null)] // invalid input IPv4 illegal tail slash
        [InlineData(false, "/192.168.1.16", null)] // invalid input IPv4 basic illegal head slash
        // IPv6 addresses
        [InlineData(true, "::", "::")] // IPv6 minimum double colon
        [InlineData(true, "0::", "::")] // IPv6 minimum double colon leading 0
        [InlineData(true, "::0", "::")] // IPv6 minimum double colon following 0
        [InlineData(true, "0::0", "::")] // IPv6 minimum double colon leading and following 0
        [InlineData(true, "::192.168.1.100", "::192.168.1.100")] // IPv6 encoded IPv4
        [InlineData(true, "ab::192.168.1.100", "ab::c0a8:164")] // IPv6 parse encoded IPv4 like
        [InlineData(false, "ab::80000", null)] // IPv6 with IPv4 integer
        [InlineData(false, "ab::064.0100.04.017", null)] // IPv6 with IPv4 full octal
        [InlineData(true, "ab::0:192.168.1.100", "ab::c0a8:164")] // IPv6 parse encoded IPv4 like
        [InlineData(true, "ffff:ffff:ffff:ffff:ffff:ffff:192.168.1.100", "ffff:ffff:ffff:ffff:ffff:ffff:c0a8:164")] // IPv6 parse encoded IPv4 like with leading max
        [InlineData(true, "ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff", "ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff")] // IPv6 maximum all ffff hextets
        [InlineData(true, "FFFF:FFFF:FFFF:FFFF:FFFF:FFFF:FFFF:FFFF", "ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff")] // IPv6 maximum all FFFF hextets
        [InlineData(true, "abcd::", "abcd::")] // IPv6 basic input
        [InlineData(true, "[abcd::12]", "abcd::12")] // IPv6 encapsulated in brackets
        // IPv6-like invalid address string
        [InlineData(false, "abcd::/64", null)] // invalid input IPv6 basic cidr
        [InlineData(false, "abcd::/", null)] // invalid input IPv6 illegal trailing slash
        [InlineData(false, "/abcd::", null)] // invalid input IPv6 illegal prefix slash
        [InlineData(false, "ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff:", null)] // IPv6 trailing colon
        [InlineData(false, "ffff::::abcd", null)] // IPv6 double double colons
        [InlineData(false, "ffff::0::abcd", null)] // IPv6 double multiple collapse around hextets
        [InlineData(false, "%abcd::", null)] // invalid input IPv6 leading %
        [InlineData(false, "f", null)] // single hex char
        [InlineData(false, "fc", null)] // double hex char
        [InlineData(false, "abcd:abc", null)] // IPv6 incomplete
        // invalid input
        [InlineData(false, "", null)] // invalid input empty string
        [InlineData(false, null, null)] // invalid input null
        [InlineData(false, " ", null)] // invalid input whitespace
        [InlineData(false, "\t", null)] // invalid input tab character
        [InlineData(false, "-1", null)] // integer -1
        [InlineData(false, "4294967296", null)] // integer unsigned max input
        [InlineData(false, "8589934591", null)] // integer bigger than max uint
#if NET48
        /*
         * In .NET versions up to and including .NET 4.8 (which corresponds to .NET Standard 2.0), stricter
         * parsing rules were enforced for IPAddress according to the IPv6 specification.Specifically,
         * the presence of a terminal '%' character without a valid zone index is considered invalid in
         * these versions. As a result, the input "abcd::%" fails to parse, leading to a null or failed
         * address parsing depending on Parse/TryParse.This behavior represents a breaking change from
         * Arcus's previous target of .NET Standard 1.3. and may provide confusion for .NET 4.8 / .NET
         * Standard 2.0 versions.
         *
         * In contrast, in newer versions of. NET, including .NET 8, .NET 9, and .NET 10, the parsing rules have been
         * relaxed. The trailing '%' character is now ignored during parsing, allowing for inputs that
         * would have previously failed.
         *
         * It is important to note that this scenario appears to be an extreme edge case, and developers
         * should ensure that their applications handle IPAddress parsing appropriately across different
         * target frameworks as expected.If in doubt it is suggested that IP Address based user input
         * should be sanitized to meet your needs.
         */
        [InlineData(false, "abcd::%", null)]
#else
        [InlineData(true, "abcd::%", "abcd::")]
#endif
        public void IPAddressTryParseSuccessTest(bool expected, string input, string expectedParseResult)
        {
            // Arrange

            // Act
            var success = IPAddress.TryParse(input, out var address);

            // Assert
            if (success)
            {
                Assert.NotNull(address); // expecting non-null result on successful parse
            }
            else
            {
                Assert.Null(address); // expecting null result on unsuccessful parse
            }

            Assert.Equal(expectedParseResult, address?.ToString());
            Assert.Equal(expected, success);
        }
    }
}
