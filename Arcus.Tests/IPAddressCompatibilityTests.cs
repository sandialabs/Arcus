using System.Net;
using NUnit.Framework;

namespace Arcus.Tests
{
    [TestFixture(Description = "TestFixture for (trusting) but verifying the compatibility of IPAddress object with Microsoft System.Net.IPAddress object")]
    public class IPAddressCompatibilityTests
    {
        // IPv4 Addresses

        // - min IPv4 address
        [TestCase("0", "0.0.0.0", ExpectedResult = true, TestName = "IPv4 minimum single 0")]
        [TestCase("0000", "0.0.0.0", ExpectedResult = true, TestName = "IPv4 minimum 0000")]
        [TestCase("0.0", "0.0.0.0", ExpectedResult = true, TestName = "IPv4 minimum double 0 octets")]
        [TestCase("0.0.0", "0.0.0.0", ExpectedResult = true, TestName = "IPv4 minimum triple 0 octets")]
        [TestCase("0.0.0.0", "0.0.0.0", ExpectedResult = true, TestName = "IPv4 minimum full 0 octets")]

        // - max IPv4 address
        [TestCase("255.255.255.255", "255.255.255.255", ExpectedResult = true, TestName = "IPv4 max value full 255 octets")]

        // - generic IPv4 address
        [TestCase("192.168.1.100", "192.168.1.100", ExpectedResult = true, TestName = "IPv4 generic")]

        // - integer input
        [TestCase("1024", "0.0.4.0", ExpectedResult = true, TestName = "integer 1024")]
        [TestCase("2048", "0.0.8.0", ExpectedResult = true, TestName = "integer 2048")]
        [TestCase("4096", "0.0.16.0", ExpectedResult = true, TestName = "integer 4096")]
        [TestCase("2147483647", "127.255.255.255", ExpectedResult = true, TestName = "integer max int-1 (2147483647)")]
        [TestCase("2147483648", "128.0.0.0", ExpectedResult = true, TestName = "integer max positive")]
        [TestCase("4294967295", "255.255.255.255", ExpectedResult = true, TestName = "integer max uint")]

        // - fewer than 4 octet IPv4 addresses
        [TestCase("192", "0.0.0.192", ExpectedResult = true, TestName = "IPv4 single octet")]
        [TestCase("192.100", "192.0.0.100", ExpectedResult = true, TestName = "IPv4 first and last octets")]
        [TestCase("192.1.100", "192.1.0.100", ExpectedResult = true, TestName = "IPv4 three octets")]
        [TestCase("064.0100.004.017", "52.64.4.15", ExpectedResult = true, Description = "IPV4 leading 00 octal notation")]

        // octal notation IPv4
        [TestCase("064", "0.0.0.52", ExpectedResult = true, TestName = "IPv4 octal single octet")]
        [TestCase("064.0100.04.017", "52.64.4.15", ExpectedResult = true, TestName = "IPv4 full octal")]
        [TestCase("064.100.4.18", "52.100.4.18", ExpectedResult = true, TestName = "IPv4 mixed octal and decimal")]

        // IPv4-like invalid address string
        [TestCase("256.0.0.0", null, ExpectedResult = false, TestName = "IPv4-like first octet too large")]
        [TestCase("0.256.0.0", null, ExpectedResult = false, TestName = "IPv4-like second octet too large")]
        [TestCase("0.0.256.0", null, ExpectedResult = false, TestName = "IPv4-like third octet too large")]
        [TestCase("0.0.0.256", null, ExpectedResult = false, TestName = "IPv4-like fourth octet too large")]
        [TestCase("255.255.255.255.0", null, ExpectedResult = false, TestName = "IPv4-like extra 0 tail octet")]
        [TestCase("0.255.255.255.255", null, ExpectedResult = false, TestName = "IPv4-like extra 0 head octet")]
        [TestCase("064.0100.04.018", null, ExpectedResult = false, Description = "invalid 8 in octal notation")]
        [TestCase("192.168.1.1.", null, ExpectedResult = false, TestName = "IPv4-like trailing period")]
        [TestCase("192.168.1.1/16", null, ExpectedResult = false, TestName = "invalid input IPv4 basic cidr")]
        [TestCase("192.168.1.1/", null, ExpectedResult = false, TestName = "invalid input IPv4 illegal tail slash")]
        [TestCase("/192.168.1.16", null, ExpectedResult = false, TestName = "invalid input IPv4 basic illegal head slash")]

        // IPv6 addresses
        [TestCase("::", "::", ExpectedResult = true, TestName = "IPv6 minimum double colon")]
        [TestCase("0::", "::", ExpectedResult = true, TestName = "IPv6 minimum double colon leading 0")]
        [TestCase("::0", "::", ExpectedResult = true, TestName = "IPv6 minimum double colon following 0")]
        [TestCase("0::0", "::", ExpectedResult = true, TestName = "IPv6 minimum double colon leading and following 0")]
        [TestCase("::192.168.1.100", "::192.168.1.100", ExpectedResult = true, TestName = "IPv6 encoded IPv4")]
        [TestCase("ab::192.168.1.100", "ab::c0a8:164", ExpectedResult = true, TestName = "IPv6 parse encoded IPv4 like")]
        [TestCase("ab::80000", null, ExpectedResult = false, TestName = "IPv6 with IPv4 integer")]
        [TestCase("ab::064.0100.04.017", null, ExpectedResult = false, TestName = "IPv6 with IPv4 full octal")]
        [TestCase("ab::0:192.168.1.100", "ab::c0a8:164", ExpectedResult = true, TestName = "IPv6 parse encoded IPv4 like")]
        [TestCase("ffff:ffff:ffff:ffff:ffff:ffff:192.168.1.100", "ffff:ffff:ffff:ffff:ffff:ffff:c0a8:164", ExpectedResult = true, TestName = "IPv6 parse encoded IPv4 like with leading max")]
        [TestCase("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff", "ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff", ExpectedResult = true, TestName = "IPv6 maximum all ffff hextets")]
        [TestCase("FFFF:FFFF:FFFF:FFFF:FFFF:FFFF:FFFF:FFFF", "ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff", ExpectedResult = true, TestName = "IPv6 maximum all FFFF hextets")]
        [TestCase("abcd::", "abcd::", ExpectedResult = true, TestName = "IPv6 basic input")]
        [TestCase("[abcd::12]", "abcd::12", ExpectedResult = true, TestName = "IPv6 encapsulated in brackets")]

        // IPv6-like invalid address string
        [TestCase("abcd::/64", null, ExpectedResult = false, TestName = "invalid input IPv6 basic cidr")]
        [TestCase("abcd::/", null, ExpectedResult = false, TestName = "invalid input IPv6 illegal trailing slash")]
        [TestCase("/abcd::", null, ExpectedResult = false, TestName = "invalid input IPv6 illegal prefix slash")]
        [TestCase("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff:", null, ExpectedResult = false, TestName = "IPv6 trailing colon")]
        [TestCase("ffff::::abcd", null, ExpectedResult = false, TestName = "IPv6 double double colons")]
        [TestCase("ffff::0::abcd", null, ExpectedResult = false, TestName = "IPv6 double multiple collapse around hextets")]
        [TestCase("abcd::%", null, ExpectedResult = false, TestName = "invalid input IPv6 trailing %")]
        [TestCase("%abcd::", null, ExpectedResult = false, TestName = "invalid input IPv6 leading %")]
        [TestCase("f", null, ExpectedResult = false, TestName = "single hex char")]
        [TestCase("fc", null, ExpectedResult = false, TestName = "double hex char")]
        [TestCase("abcd:abc", null, ExpectedResult = false, TestName = "IPv6 incomplete")]

        // invalid input 
        [TestCase("", null, ExpectedResult = false, TestName = "invalid input empty string")]
        [TestCase(null, null, ExpectedResult = false, TestName = "invalid input null")]
        [TestCase(" ", null, ExpectedResult = false, TestName = "invalid input whitespace")]
        [TestCase("\t", null, ExpectedResult = false, TestName = "invalid input tab character")]
        [TestCase("-1", null, ExpectedResult = false, TestName = "integer -1")]
        [TestCase("4294967296", null, ExpectedResult = false, TestName = "integer unsigned max input")]
        [TestCase("8589934591", null, ExpectedResult = false, TestName = "integer bigger than max uint")]
        public bool IPAddressTryParseSuccessTest(string input,
                                                 string expectedParseResult)
        {
            // Arrange
            IPAddress address;

            // Act
            var success = IPAddress.TryParse(input, out address);

            // Assert
            if (success)
            {
                Assert.IsNotNull(address, "expecting non-null result on successful parse");
            }
            else
            {
                Assert.IsNull(address, "expecting null result on unsuccessful parse");
            }

            Assert.AreEqual(expectedParseResult, address?.ToString());
            return success;
        }
    }
}
