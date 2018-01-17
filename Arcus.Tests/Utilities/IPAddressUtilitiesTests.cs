using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using Arcus.Converters;
using Arcus.Utilities;
using NUnit.Framework;

namespace Arcus.Tests.Utilities
{
    [TestFixture]
    public class IPAddressUtilitiesTests
    {
        private static IEnumerable<TestCaseData> InvalidAddressFamilies
        {
            get
            {
                return Enum.GetValues(typeof (AddressFamily))
                           .Cast<AddressFamily>()
                           .Where(addressFamily => addressFamily != AddressFamily.InterNetworkV6)
                           .Where(addressFamily => addressFamily != AddressFamily.InterNetwork)
                           .Select(addressFamily => new TestCaseData(addressFamily));
            }
        }

        private static IEnumerable<TestCaseData> ValidAddressFamilies => new[]
                                                                         {
                                                                             new TestCaseData(AddressFamily.InterNetwork),
                                                                             new TestCaseData(AddressFamily.InterNetworkV6)
                                                                         };

        [TestCase("10.0.0.0")]
        [TestCase("10.0.0.128")]
        [TestCase("0.0.0.0")]
        [TestCase("255.255.255.255")]
        [TestCase("10.0.0.128")]
        [TestCase("192.168.1.1")]
        [TestCase("::")]
        [TestCase("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff")]
        [TestCase("ffff:ffff:ffff:ffff::")]
        [TestCase("a:b:c::")]
        public void TryParseBigIntegerTest(string input)
        {
            var address = IPAddress.Parse(input);
            IPAddress parsedAddress;

            Assert.IsTrue(IPAddressUtilities.TryParse(address.ToUnsignedBigInteger(), address.AddressFamily, out parsedAddress));
            Assert.AreEqual(address, parsedAddress);
        }

        [TestCase("10.0.0.0")]
        [TestCase("10.0.0.128")]
        [TestCase("0.0.0.0")]
        [TestCase("255.255.255.255")]
        [TestCase("10.0.0.128")]
        [TestCase("192.168.1.1")]
        [TestCase("::")]
        [TestCase("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff")]
        [TestCase("ffff:ffff:ffff:ffff::")]
        [TestCase("a:b:c::")]
        public void TryParseBytesTest(string input)
        {
            var address = IPAddress.Parse(input);
            IPAddress parsedAddress;

            var byteArray = address.ToUnsignedBigInteger()
                                   .ToByteArray();

            Assert.IsTrue(IPAddressUtilities.TryParse(byteArray, address.AddressFamily, out parsedAddress));
            Assert.AreEqual(address, parsedAddress);
        }

        [TestCase(null, ExpectedResult = false)]
        [TestCase("::", ExpectedResult = false)]
        [TestCase("ffff::", ExpectedResult = false)]
        [TestCase("0.0.0.0", ExpectedResult = true)]
        [TestCase("255.255.255.255", ExpectedResult = true)]
        [TestCase("255.0.0.0", ExpectedResult = true)]
        [TestCase("255.128.0.0", ExpectedResult = true)]
        [TestCase("255.255.255.128", ExpectedResult = true)]
        [TestCase("255.255.0.255", ExpectedResult = false)]
        [TestCase("255.0.255.255", ExpectedResult = false)]
        [TestCase("0.255.255.255", ExpectedResult = false)]
        [TestCase("0.255.255.255", ExpectedResult = false)]
        [TestCase("0.0.0.255", ExpectedResult = false)]
        [TestCase("0.0.0.1", ExpectedResult = false)]
        public bool IsValidNetMaskTestTest(string input)
        {
            IPAddress netmaskAddress;
            if (!IPAddress.TryParse(input, out netmaskAddress))
            {
                netmaskAddress = null;
            }

            return netmaskAddress.IsValidNetMask();
        }

        [TestCase("::", ExpectedResult = false)]
        [TestCase("192.168.1.1", ExpectedResult = false)]
        [TestCase("::ffff:222.1.41.90", ExpectedResult = true)]
        [TestCase("::ffff:ab:cd", ExpectedResult = true)]
        [TestCase("1234::ffff:222.1.41.90", ExpectedResult = true)]
        [TestCase("1234::ffff:ab:cd", ExpectedResult = true)]
        public bool IsIPv4MappedIPv6Test(string input)
        {
            // Arrange
            var address = IPAddress.Parse(input);

            // Act
            var result = address.IsIPv4MappedIPv6();

            // Assert
            return result;
        }

        [TestCase(null, ExpectedResult = false)]
        [TestCase("", ExpectedResult = false)]
        [TestCase("potato", ExpectedResult = false)]
        [TestCase("192", ExpectedResult = true)]
        [TestCase("192.168.1.1", ExpectedResult = true)]
        [TestCase("192.168.001.001", ExpectedResult = true)]
        [TestCase("032.032.032.032", ExpectedResult = true)]
        [TestCase("0.09.009.009", ExpectedResult = true)]
        [TestCase("::", ExpectedResult = true)]
        public bool TryParseIgnoreOctalInIPv4ParseSuccessTest(string input)
        {
            IPAddress address;
            return IPAddressUtilities.TryParseIgnoreOctalInIPv4(input, out address);
        }

        [TestCase(null, ExpectedResult = null)]
        [TestCase("", ExpectedResult = null)]
        [TestCase("potato", ExpectedResult = null)]
        [TestCase("192", ExpectedResult = "0.0.0.192")]
        [TestCase("192.168.1.1", ExpectedResult = "192.168.1.1")]
        [TestCase("192.168.001.001", ExpectedResult = "192.168.1.1")]
        [TestCase("032.032.032.032", ExpectedResult = "32.32.32.32")]
        [TestCase("0.09.009.009", ExpectedResult = "0.9.9.9")]
        [TestCase("::", ExpectedResult = "::")]
        public string TryParseIgnoreOctalInIPv4ParseResultTest(string input)
        {
            IPAddress address;
            IPAddressUtilities.TryParseIgnoreOctalInIPv4(input, out address);
            return address?.ToString();
        }

        [TestCase(null, ExpectedResult = null)]
        [TestCase("", ExpectedResult = null)]
        [TestCase("potato", ExpectedResult = null)]
        [TestCase("192", ExpectedResult = "0.0.0.192")]
        [TestCase("192.168.1.1", ExpectedResult = "192.168.1.1")]
        [TestCase("192.168.001.001", ExpectedResult = "192.168.1.1")]
        [TestCase("032.032.032.032", ExpectedResult = "32.32.32.32")]
        [TestCase("0.09.009.009", ExpectedResult = "0.9.9.9")]
        [TestCase("::", ExpectedResult = "::")]
        public string ParseIgnoreOctalInIPv4Test(string input)
        {
            return IPAddressUtilities.ParseIgnoreOctalInIPv4(input)
                ?.ToString();
        }

        [TestCase("potato", AddressFamily.InterNetwork, ExpectedResult = false)]
        [TestCase("", AddressFamily.InterNetwork, ExpectedResult = false)]
        [TestCase("0x0", AddressFamily.InterNetwork, ExpectedResult = true)]
        [TestCase("0", AddressFamily.InterNetwork, ExpectedResult = true)]
        [TestCase("0x32", AddressFamily.InterNetwork, ExpectedResult = true)]
        [TestCase("32", AddressFamily.InterNetwork, ExpectedResult = true)]
        [TestCase("0x0", AddressFamily.InterNetworkV6, ExpectedResult = true)]
        [TestCase("0", AddressFamily.InterNetworkV6, ExpectedResult = true)]
        [TestCase("0x32", AddressFamily.InterNetworkV6, ExpectedResult = true)]
        [TestCase("32", AddressFamily.InterNetworkV6, ExpectedResult = true)]
        [TestCase("0x00000000000000000000000000000000", AddressFamily.InterNetworkV6, ExpectedResult = true)]
        [TestCase("00000000000000000000000000000000", AddressFamily.InterNetworkV6, ExpectedResult = true)]
        [TestCase("0xFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF", AddressFamily.InterNetworkV6, ExpectedResult = true)]
        [TestCase("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF", AddressFamily.InterNetworkV6, ExpectedResult = true)]
        [TestCase("0xFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF", AddressFamily.InterNetwork, ExpectedResult = true)]
        [TestCase("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF", AddressFamily.InterNetwork, ExpectedResult = true)]
        [TestCase("0x0000000000000000000000000000ABCD", AddressFamily.InterNetworkV6, ExpectedResult = true)]
        [TestCase("0000000000000000000000000000ABCD", AddressFamily.InterNetworkV6, ExpectedResult = true)]
        [TestCase("0xABCD000000000000000000000000ABCD", AddressFamily.InterNetworkV6, ExpectedResult = true)]
        [TestCase("ABCD000000000000000000000000ABCD", AddressFamily.InterNetworkV6, ExpectedResult = true)]
        [TestCase("0xABCD0000000000000000000000000000", AddressFamily.InterNetworkV6, ExpectedResult = true)]
        [TestCase("ABCD0000000000000000000000000000", AddressFamily.InterNetworkV6, ExpectedResult = true)]
        [TestCase("0x00000000", AddressFamily.InterNetwork, ExpectedResult = true)]
        [TestCase("00000000", AddressFamily.InterNetwork, ExpectedResult = true)]
        [TestCase("0xFFFFFFFF", AddressFamily.InterNetwork, ExpectedResult = true)]
        [TestCase("FFFFFFFF", AddressFamily.InterNetwork, ExpectedResult = true)]
        [TestCase("0x80808080", AddressFamily.InterNetwork, ExpectedResult = true)]
        [TestCase("80808080", AddressFamily.InterNetwork, ExpectedResult = true)]
        [TestCase("0x00808080", AddressFamily.InterNetwork, ExpectedResult = true)]
        [TestCase("00808080", AddressFamily.InterNetwork, ExpectedResult = true)]
        [TestCase("80808000", AddressFamily.InterNetwork, ExpectedResult = true)]
        public bool TryParseFromHexStringParseSuccessTest(string input,
                                                          AddressFamily addressFamily)
        {
            IPAddress address;
            return IPAddressUtilities.TryParseFromHexString(input, addressFamily, out address);
        }

        [TestCase("potato", AddressFamily.InterNetwork, ExpectedResult = null)]
        [TestCase("", AddressFamily.InterNetwork, ExpectedResult = null)]
        [TestCase("0x0", AddressFamily.InterNetwork, ExpectedResult = "0.0.0.0")]
        [TestCase("0", AddressFamily.InterNetwork, ExpectedResult = "0.0.0.0")]
        [TestCase("0x32", AddressFamily.InterNetwork, ExpectedResult = "0.0.0.50")]
        [TestCase("32", AddressFamily.InterNetwork, ExpectedResult = "0.0.0.50")]
        [TestCase("0x0", AddressFamily.InterNetworkV6, ExpectedResult = "::")]
        [TestCase("0", AddressFamily.InterNetworkV6, ExpectedResult = "::")]
        [TestCase("0x32", AddressFamily.InterNetworkV6, ExpectedResult = "::32")]
        [TestCase("32", AddressFamily.InterNetworkV6, ExpectedResult = "::32")]
        [TestCase("0x00000000000000000000000000000000", AddressFamily.InterNetworkV6, ExpectedResult = "::")]
        [TestCase("00000000000000000000000000000000", AddressFamily.InterNetworkV6, ExpectedResult = "::")]
        [TestCase("0xFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF", AddressFamily.InterNetworkV6, ExpectedResult = "ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff")]
        [TestCase("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF", AddressFamily.InterNetworkV6, ExpectedResult = "ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff")]
        [TestCase("0xFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF", AddressFamily.InterNetwork, ExpectedResult = "255.255.255.255")]
        [TestCase("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF", AddressFamily.InterNetwork, ExpectedResult = "255.255.255.255")]
        [TestCase("0x0000000000000000000000000000ABCD", AddressFamily.InterNetworkV6, ExpectedResult = "::abcd")]
        [TestCase("0000000000000000000000000000ABCD", AddressFamily.InterNetworkV6, ExpectedResult = "::abcd")]
        [TestCase("0xABCD000000000000000000000000ABCD", AddressFamily.InterNetworkV6, ExpectedResult = "abcd::abcd")]
        [TestCase("ABCD000000000000000000000000ABCD", AddressFamily.InterNetworkV6, ExpectedResult = "abcd::abcd")]
        [TestCase("0xABCD0000000000000000000000000000", AddressFamily.InterNetworkV6, ExpectedResult = "abcd::")]
        [TestCase("ABCD0000000000000000000000000000", AddressFamily.InterNetworkV6, ExpectedResult = "abcd::")]
        [TestCase("0x00000000", AddressFamily.InterNetwork, ExpectedResult = "0.0.0.0")]
        [TestCase("00000000", AddressFamily.InterNetwork, ExpectedResult = "0.0.0.0")]
        [TestCase("0xFFFFFFFF", AddressFamily.InterNetwork, ExpectedResult = "255.255.255.255")]
        [TestCase("FFFFFFFF", AddressFamily.InterNetwork, ExpectedResult = "255.255.255.255")]
        [TestCase("0x80808080", AddressFamily.InterNetwork, ExpectedResult = "128.128.128.128")]
        [TestCase("80808080", AddressFamily.InterNetwork, ExpectedResult = "128.128.128.128")]
        [TestCase("0x00808080", AddressFamily.InterNetwork, ExpectedResult = "0.128.128.128")]
        [TestCase("00808080", AddressFamily.InterNetwork, ExpectedResult = "0.128.128.128")]
        [TestCase("80808000", AddressFamily.InterNetwork, ExpectedResult = "128.128.128.0")]
        public string TryParseFromHexStringParseResultTest(string input,
                                                           AddressFamily addressFamily)
        {
            IPAddress address;
            IPAddressUtilities.TryParseFromHexString(input, addressFamily, out address);
            return address?.ToString();
        }

        [TestCase("potato", AddressFamily.InterNetwork, ExpectedResult = null)]
        [TestCase("", AddressFamily.InterNetwork, ExpectedResult = null)]
        [TestCase("0x0", AddressFamily.InterNetwork, ExpectedResult = "0.0.0.0")]
        [TestCase("0", AddressFamily.InterNetwork, ExpectedResult = "0.0.0.0")]
        [TestCase("0x32", AddressFamily.InterNetwork, ExpectedResult = "0.0.0.50")]
        [TestCase("32", AddressFamily.InterNetwork, ExpectedResult = "0.0.0.50")]
        [TestCase("0x0", AddressFamily.InterNetworkV6, ExpectedResult = "::")]
        [TestCase("0", AddressFamily.InterNetworkV6, ExpectedResult = "::")]
        [TestCase("0x32", AddressFamily.InterNetworkV6, ExpectedResult = "::32")]
        [TestCase("32", AddressFamily.InterNetworkV6, ExpectedResult = "::32")]
        [TestCase("0x00000000000000000000000000000000", AddressFamily.InterNetworkV6, ExpectedResult = "::")]
        [TestCase("00000000000000000000000000000000", AddressFamily.InterNetworkV6, ExpectedResult = "::")]
        [TestCase("0xFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF", AddressFamily.InterNetworkV6, ExpectedResult = "ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff")]
        [TestCase("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF", AddressFamily.InterNetworkV6, ExpectedResult = "ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff")]
        [TestCase("0xFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF", AddressFamily.InterNetwork, ExpectedResult = "255.255.255.255")]
        [TestCase("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF", AddressFamily.InterNetwork, ExpectedResult = "255.255.255.255")]
        [TestCase("0x0000000000000000000000000000ABCD", AddressFamily.InterNetworkV6, ExpectedResult = "::abcd")]
        [TestCase("0000000000000000000000000000ABCD", AddressFamily.InterNetworkV6, ExpectedResult = "::abcd")]
        [TestCase("0xABCD000000000000000000000000ABCD", AddressFamily.InterNetworkV6, ExpectedResult = "abcd::abcd")]
        [TestCase("ABCD000000000000000000000000ABCD", AddressFamily.InterNetworkV6, ExpectedResult = "abcd::abcd")]
        [TestCase("0xABCD0000000000000000000000000000", AddressFamily.InterNetworkV6, ExpectedResult = "abcd::")]
        [TestCase("ABCD0000000000000000000000000000", AddressFamily.InterNetworkV6, ExpectedResult = "abcd::")]
        [TestCase("0x00000000", AddressFamily.InterNetwork, ExpectedResult = "0.0.0.0")]
        [TestCase("00000000", AddressFamily.InterNetwork, ExpectedResult = "0.0.0.0")]
        [TestCase("0xFFFFFFFF", AddressFamily.InterNetwork, ExpectedResult = "255.255.255.255")]
        [TestCase("FFFFFFFF", AddressFamily.InterNetwork, ExpectedResult = "255.255.255.255")]
        [TestCase("0x80808080", AddressFamily.InterNetwork, ExpectedResult = "128.128.128.128")]
        [TestCase("80808080", AddressFamily.InterNetwork, ExpectedResult = "128.128.128.128")]
        [TestCase("0x00808080", AddressFamily.InterNetwork, ExpectedResult = "0.128.128.128")]
        [TestCase("00808080", AddressFamily.InterNetwork, ExpectedResult = "0.128.128.128")]
        [TestCase("80808000", AddressFamily.InterNetwork, ExpectedResult = "128.128.128.0")]
        public string ParseFromHexStringTest(string input,
                                             AddressFamily addressFamily)
        {
            return IPAddressUtilities.ParseFromHexString(input, addressFamily)
                ?.ToString();
        }

        [TestCase("", ExpectedResult = false)]
        [TestCase("potato", ExpectedResult = false)]
        [TestCase("::", ExpectedResult = false)]
        [TestCase("ffff::", ExpectedResult = false)]
        [TestCase("::", ExpectedResult = false)]
        [TestCase("192.168.1.1", ExpectedResult = true)]
        [TestCase("0.0.0.0", ExpectedResult = true)]
        public bool IsIPv4Test(string input)
        {
            // Arrange
            IPAddress address;
            if (!IPAddress.TryParse(input, out address))
            {
                address = null;
            }

            // Act
            var result = address.IsIPv4();

            // Assert
            return result;
        }

        [TestCase("", ExpectedResult = false)]
        [TestCase("potato", ExpectedResult = false)]
        [TestCase("::", ExpectedResult = true)]
        [TestCase("ffff::", ExpectedResult = true)]
        [TestCase("::", ExpectedResult = true)]
        [TestCase("192.168.1.1", ExpectedResult = false)]
        [TestCase("0.0.0.0", ExpectedResult = false)]
        public bool IsIPv6Test(string input)
        {
            // Arrange
            IPAddress address;
            if (!IPAddress.TryParse(input, out address))
            {
                address = null;
            }

            // Act
            var result = address.IsIPv6();

            // Assert
            return result;
        }

        [Test]
        public void IsIPv4MappedIPv6NullInputTest()
        {
            Assert.Throws<ArgumentNullException>(() => ((IPAddress) null).IsIPv4MappedIPv6());
        }

        [Test, TestCaseSource(nameof(InvalidAddressFamilies))]
        public void TryParseBBytesInvalidAddressFamilyTest(AddressFamily addressFamily)
        {
            // Arrange
            var bytes = new byte[] {0x42};

            // Assert
            IPAddress address;
            var success = IPAddressUtilities.TryParse(bytes, addressFamily, out address);

            // Assert
            Assert.IsFalse(success);
            Assert.IsNull(address);
        }

        [Test, TestCaseSource(nameof(InvalidAddressFamilies))]
        public void TryParseBigIntegerInvalidAddressFamilyTest(AddressFamily addressFamily)
        {
            // Arrange
            var negativeBigInteger = new BigInteger(42);

            // Assert
            IPAddress address;
            var success = IPAddressUtilities.TryParse(negativeBigInteger, addressFamily, out address);

            // Assert
            Assert.IsFalse(success);
            Assert.IsNull(address);
        }

        [Test, TestCaseSource(nameof(ValidAddressFamilies))]
        public void TryParseBigIntegerNegativeInputTest(AddressFamily addressFamily)
        {
            // Arrange
            IPAddress address;
            var negativeBigInteger = new BigInteger(-1);

            // Act
            var success = IPAddressUtilities.TryParse(negativeBigInteger, addressFamily, out address);

            // Assert
            Assert.IsFalse(success);
            Assert.IsNull(address);
        }

        [Test]
        public void IPv4MaxAddressTest()
        {
            // Arrange
            var address = IPAddressUtilities.IPv4MaxAddress;

            // Assert
            Assert.IsNotNull(address);
            Assert.IsTrue(address.IsIPv4());
            Assert.AreEqual("255.255.255.255", address.ToString());
        }

        [Test]
        public void IPv4MinAddressTest()
        {
            // Arrange
            var address = IPAddressUtilities.IPv4MinAddress;

            // Assert
            Assert.IsNotNull(address);
            Assert.IsTrue(address.IsIPv4());
            Assert.AreEqual("0.0.0.0", address.ToString());
        }

        [Test]
        public void IPv6MaxAddressTest()
        {
            // Arrange
            var address = IPAddressUtilities.IPv6MaxAddress;

            // Assert
            Assert.IsNotNull(address);
            Assert.IsTrue(address.IsIPv6());
            Assert.AreEqual("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff", address.ToString());
        }

        [Test]
        public void IPv6MinAddressTest()
        {
            // Arrange
            var address = IPAddressUtilities.IPv6MinAddress;

            // Assert
            Assert.IsNotNull(address);
            Assert.IsTrue(address.IsIPv6());
            Assert.AreEqual("::", address.ToString());
        }
    }
}
