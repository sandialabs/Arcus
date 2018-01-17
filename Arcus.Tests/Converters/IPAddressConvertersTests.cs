using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Numerics;
using Arcus.Converters;
using NUnit.Framework;

namespace Arcus.Tests.Converters
{
    [TestFixture]
    public class IPAddressConvertersTests
    {
        [TestCase("", ExpectedResult = "")]
        [TestCase("192.168.1.1", ExpectedResult = "")]
        [TestCase("1080:0:0:0:8:800:200C:417A", ExpectedResult = "4)+k&C#VzJ4br>0wv%Yp")]
        public string ToBase85Test(string input)
        {
            IPAddress address;
            if (!IPAddress.TryParse(input, out address))
            {
                address = null;
            }

            return address.ToBase85String();
        }

        [TestCase("0.0.0.0", ExpectedResult = 0)]
        [TestCase("128.0.0.0", ExpectedResult = 1)]
        [TestCase("192.0.0.0", ExpectedResult = 2)]
        [TestCase("224.0.0.0", ExpectedResult = 3)]
        [TestCase("240.0.0.0", ExpectedResult = 4)]
        [TestCase("248.0.0.0", ExpectedResult = 5)]
        [TestCase("252.0.0.0", ExpectedResult = 6)]
        [TestCase("254.0.0.0", ExpectedResult = 7)]
        [TestCase("255.0.0.0", ExpectedResult = 8)]
        [TestCase("255.128.0.0", ExpectedResult = 9)]
        [TestCase("255.192.0.0", ExpectedResult = 10)]
        [TestCase("255.224.0.0", ExpectedResult = 11)]
        [TestCase("255.240.0.0", ExpectedResult = 12)]
        [TestCase("255.248.0.0", ExpectedResult = 13)]
        [TestCase("255.252.0.0", ExpectedResult = 14)]
        [TestCase("255.254.0.0", ExpectedResult = 15)]
        [TestCase("255.255.0.0", ExpectedResult = 16)]
        [TestCase("255.255.128.0", ExpectedResult = 17)]
        [TestCase("255.255.192.0", ExpectedResult = 18)]
        [TestCase("255.255.224.0", ExpectedResult = 19)]
        [TestCase("255.255.240.0", ExpectedResult = 20)]
        [TestCase("255.255.248.0", ExpectedResult = 21)]
        [TestCase("255.255.252.0", ExpectedResult = 22)]
        [TestCase("255.255.254.0", ExpectedResult = 23)]
        [TestCase("255.255.255.0", ExpectedResult = 24)]
        [TestCase("255.255.255.128", ExpectedResult = 25)]
        [TestCase("255.255.255.192", ExpectedResult = 26)]
        [TestCase("255.255.255.224", ExpectedResult = 27)]
        [TestCase("255.255.255.240", ExpectedResult = 28)]
        [TestCase("255.255.255.248", ExpectedResult = 29)]
        [TestCase("255.255.255.252", ExpectedResult = 30)]
        [TestCase("255.255.255.254", ExpectedResult = 31)]
        [TestCase("255.255.255.255", ExpectedResult = 32)]
        public int FromNetmaskToCidrRoutePrefixTest(string input)
        {
            // Arrange
            var address = IPAddress.Parse(input);
            // Act
            var result = address.NetmaskToCidrRoutePrefix();
            // Assert
            return result;
        }

        [TestCase("::")]
        [TestCase("192.168.1.1")]
        [TestCase("0.0.0.255")]
        public void FromNetmaskToCidrRoutePrefixInvalidNetmaskTest(string input)
        {
            // Arrange
            var address = IPAddress.Parse(input);

            // Assert
            Assert.Throws<InvalidOperationException>(() => address.NetmaskToCidrRoutePrefix());
        }

        [TestCase("192.168.1.1", ExpectedResult = "192.168.001.001")]
        [TestCase("192.168.9.1", ExpectedResult = "192.168.009.001")]
        [TestCase("124.253.63.36", ExpectedResult = "124.253.063.036")]
        [TestCase("124.253.0.0", ExpectedResult = "124.253.000.000")]
        [TestCase("0.253.63.36", ExpectedResult = "000.253.063.036")]
        [TestCase("1.253.63.36", ExpectedResult = "001.253.063.036")]
        [TestCase("1.0.63.36", ExpectedResult = "001.000.063.036")]
        public string IPAddressToUncompressedStringConversionTest(string input)
        {
            // Arrange
            var address = IPAddress.Parse(input);

            // Act
            var result = address.ToUncompressedString();

            // Assert
            return result;
        }

        [TestCase("::", ExpectedResult = "0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0")]
        [TestCase("2001:db8::ff00:42:8329", ExpectedResult = "41,131,66,0,0,255,0,0,0,0,0,0,184,13,1,32")]
        [TestCase("0.0.0.0", ExpectedResult = "0,0,0,0")]
        [TestCase("192.168.1.1", ExpectedResult = "1,1,168,192")]
        public string GetLittleEndianBytesTest(string input)
        {
            // Arrange
            var inputAddress = IPAddress.Parse(input);

            // Act
            var bytes = inputAddress.GetLittleEndianBytes();

            // Assert
            return string.Join(",", bytes.Select(b => b.ToString()));
        }

        [TestCase("1:2:3:a:b:ffff:ffff:ffff", ExpectedResult = "1:2:3:a:b:ffff:255.255.255.255")]
        [TestCase("ffff:ffff:ffff:ffff:ffff:ffff:0102:0304", ExpectedResult = "ffff:ffff:ffff:ffff:ffff:ffff:1.2.3.4")]
        [TestCase("0:ffff:ffff:ffff:0:0:ffff:ffff", ExpectedResult = "0:ffff:ffff:ffff::255.255.255.255")]
        [TestCase("0:0:ffff:ffff:0:0:ffff:ffff", ExpectedResult = "::ffff:ffff:0:0:255.255.255.255")]
        [TestCase("0:0:ffff:ffff:ffff:0:ffff:ffff", ExpectedResult = "::ffff:ffff:ffff:0:255.255.255.255")]
        [TestCase("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff", ExpectedResult = "ffff:ffff:ffff:ffff:ffff:ffff:255.255.255.255")]
        [TestCase("ffff:ffff:ffff:ffff:ffff:ffff::", ExpectedResult = "ffff:ffff:ffff:ffff:ffff:ffff:0.0.0.0")]
        [TestCase("ffff::", ExpectedResult = "ffff::0.0.0.0")]
        [TestCase("::ffff:0000:0000", ExpectedResult = "::ffff:0.0.0.0")]
        [TestCase("00::ffff:ffff:ffff:ffff:ffff:ffff", ExpectedResult = "::ffff:ffff:ffff:ffff:255.255.255.255")]
        [TestCase("::ffff:ffff", ExpectedResult = "::255.255.255.255")]
        [TestCase("ffff:0000:ffff:ffff:ffff::", ExpectedResult = "ffff::ffff:ffff:ffff:0:0.0.0.0")]
        [TestCase("ffff::ffff:ffff:ffff:ffff:ffff:ffff", ExpectedResult = "ffff::ffff:ffff:ffff:ffff:255.255.255.255")]
        [TestCase("::", ExpectedResult = "::0.0.0.0")]
        [TestCase("192.168.1.1", ExpectedResult = "192.168.1.1")]
        public string ToDottedQuadTestTest(string input)
        {
            return IPAddress.Parse(input)
                            .ToDottedQuadString();
        }

        [TestCase("192.168.1.1", ExpectedResult = "192.168.001.001")]
        [TestCase("255.255.255.255", ExpectedResult = "255.255.255.255")]
        [TestCase("0.0.0.0", ExpectedResult = "000.000.000.000")]
        [TestCase("100.10.1.0", ExpectedResult = "100.010.001.000")]
        [TestCase("::", ExpectedResult = "0000:0000:0000:0000:0000:0000:0000:0000")]
        [TestCase("1:2:ffff:0:ab:0:0:123", ExpectedResult = "0001:0002:ffff:0000:00ab:0000:0000:0123")]
        [TestCase("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff", ExpectedResult = "ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff")]
        public string ToUncompressedStringTest(string input)
        {
            return IPAddress.Parse(input)
                            .ToUncompressedString();
        }

        [TestCase("::", ExpectedResult = "0")]
        [TestCase("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff", ExpectedResult = "-1")]
        [TestCase("::abcd", ExpectedResult = "43981")]
        [TestCase("abcd::abcd", ExpectedResult = "-111919958785718209532974848376339125299")]
        [TestCase("abcd::", ExpectedResult = "-111919958785718209532974848376339169280")]
        [TestCase("0.0.0.0", ExpectedResult = "0")]
        [TestCase("255.255.255.255", ExpectedResult = "-1")]
        [TestCase("128.128.128.128", ExpectedResult = "-2139062144")]
        [TestCase("0.128.128.128", ExpectedResult = "8421504")]
        [TestCase("128.128.128.0", ExpectedResult = "-2139062272")]
        public string ToBigIntegerTest(string input)
        {
            // Arrange
            var address = IPAddress.Parse(input);

            // Act
            var result = address.ToBigInteger();

            // Assert
            return result.ToString();
        }

        [TestCase("::", ExpectedResult = "00000000000000000000000000000000")]
        [TestCase("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff", ExpectedResult = "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF")]
        [TestCase("::abcd", ExpectedResult = "0000000000000000000000000000ABCD")]
        [TestCase("abcd::abcd", ExpectedResult = "ABCD000000000000000000000000ABCD")]
        [TestCase("abcd::", ExpectedResult = "ABCD0000000000000000000000000000")]
        [TestCase("0.0.0.0", ExpectedResult = "00000000")]
        [TestCase("255.255.255.255", ExpectedResult = "FFFFFFFF")]
        [TestCase("128.128.128.128", ExpectedResult = "80808080")]
        [TestCase("0.128.128.128", ExpectedResult = "00808080")]
        [TestCase("128.128.128.0", ExpectedResult = "80808000")]
        public string ToHexTest(string input)
        {
            return IPAddress.Parse(input)
                            .ToHexString();
        }

        [TestCase("::", ExpectedResult = "0")]
        [TestCase("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff", ExpectedResult = "340282366920938463463374607431768211455")]
        [TestCase("::abcd", ExpectedResult = "43981")]
        [TestCase("abcd::abcd", ExpectedResult = "228362408135220253930399759055429086157")]
        [TestCase("abcd::", ExpectedResult = "228362408135220253930399759055429042176")]
        [TestCase("0.0.0.0", ExpectedResult = "0")]
        [TestCase("255.255.255.255", ExpectedResult = "4294967295")]
        [TestCase("128.128.128.128", ExpectedResult = "2155905152")]
        [TestCase("0.128.128.128", ExpectedResult = "8421504")]
        [TestCase("128.128.128.0", ExpectedResult = "2155905024")]
        public string ToNumericTest(string input)
        {
            return IPAddress.Parse(input)
                            .ToNumericString();
        }

        [Test]
        public void FromNetmaskToCidrRoutePrefixNullInputTest()
        {
            // Assert
            Assert.Throws<ArgumentNullException>(() => ((IPAddress) null).NetmaskToCidrRoutePrefix());
        }

        [Test]
        public void GetLittleEndianBytesNullInputTest()
        {
            Assert.Throws<ArgumentNullException>(() => ((IPAddress) null).GetLittleEndianBytes());
        }

        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodExpectedResults", MessageId = "System.Net.IPAddress.TryParse(System.String,System.Net.IPAddress@)", Justification = "Safe to ignore ExpectedResult for test")]
        [Test]
        public void ToBigIntegerIPv4ByteOrderTest()
        {
            IPAddress ip;
            IPAddress.TryParse("0.0.0.1", out ip);
            Assert.AreEqual((BigInteger) 1, ip.ToUnsignedBigInteger());
        }

        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodExpectedResults", MessageId = "System.Net.IPAddress.TryParse(System.String,System.Net.IPAddress@)", Justification = "Safe to ignore ExpectedResult for test")]
        [Test]
        public void ToBigIntegerIPv6ByteOrderTest()
        {
            IPAddress ip;
            IPAddress.TryParse("0:0:0:0:0:0:0:1", out ip);

            Assert.AreEqual((BigInteger) 1, ip.ToUnsignedBigInteger());
        }

        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodExpectedResults", MessageId = "System.Net.IPAddress.TryParse(System.String,System.Net.IPAddress@)", Justification = "Safe to ignore ExpectedResult for test")]
        [Test]
        public void ToBigIntegerMaxIPv4Test()
        {
            IPAddress ip;
            IPAddress.TryParse("255.255.255.255", out ip);

            var bArray = new byte[5];
            for (var i = 0; i < bArray.Length - 1; i++)
            {
                bArray[i] = 0xff;
            }
            var maxIPv4 = new BigInteger(bArray);

            Assert.AreEqual(maxIPv4, ip.ToUnsignedBigInteger());
        }

        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodExpectedResults", MessageId = "System.Net.IPAddress.TryParse(System.String,System.Net.IPAddress@)", Justification = "Safe to ignore ExpectedResult for test")]
        [Test]
        public void ToBigIntegerMaxIPv6Test()
        {
            IPAddress ip;
            IPAddress.TryParse("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff", out ip);

            var bArray = new byte[17];
            for (var i = 0; i < bArray.Length - 1; i++)
            {
                bArray[i] = 0xff;
            }
            var maxIPv6 = new BigInteger(bArray);

            Assert.AreEqual(maxIPv6, ip.ToUnsignedBigInteger());
        }

        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodExpectedResults", MessageId = "System.Net.IPAddress.TryParse(System.String,System.Net.IPAddress@)", Justification = "Safe to ignore ExpectedResult for test")]
        [Test]
        public void ToBigIntegerMinIPv4Test()
        {
            IPAddress ip;
            IPAddress.TryParse("0.0.0.0", out ip);

            Assert.AreEqual((BigInteger) 0, ip.ToUnsignedBigInteger());
        }

        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodExpectedResults", MessageId = "System.Net.IPAddress.TryParse(System.String,System.Net.IPAddress@)", Justification = "Safe to ignore ExpectedResult for test")]
        [Test]
        public void ToBigIntegerMinIPv6Test()
        {
            IPAddress ip;
            IPAddress.TryParse("0:0:0:0:0:0:0:0", out ip);

            Assert.AreEqual((BigInteger) 0, ip.ToUnsignedBigInteger());
        }

        [Test]
        public void ToBigIntegerNullInputTest()
        {
            Assert.Throws<ArgumentNullException>(() => ((IPAddress) null).ToBigInteger());
        }

        [Test]
        public void ToDottedQuadNullInputTest()
        {
            Assert.Throws<ArgumentNullException>(() => ((IPAddress) null).ToDottedQuadString());
        }

        [Test]
        public void ToHexNullInputTest()
        {
            Assert.Throws<ArgumentNullException>(() => ((IPAddress) null).ToHexString());
        }

        [Test]
        public void ToNumericNullInputTest()
        {
            Assert.Throws<ArgumentNullException>(() => ((IPAddress) null).ToNumericString());
        }

        [Test]
        public void ToUncompressedStringNullInputTest()
        {
            Assert.Throws<ArgumentNullException>(() => ((IPAddress) null).ToUncompressedString());
        }

        [Test]
        public void ToUnsignedBigIntegerNullInputTest()
        {
            Assert.Throws<ArgumentNullException>(() => ((IPAddress) null).ToUnsignedBigInteger());
        }
    }
}
