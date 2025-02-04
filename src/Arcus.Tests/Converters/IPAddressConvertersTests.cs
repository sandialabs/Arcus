using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Arcus.Converters;
using Gulliver;
using Xunit;

namespace Arcus.Tests.Converters
{
    public class IPAddressConvertersTests
    {
        #region ToUncompressedString

        [Theory]
        [InlineData(null, null)]
        [InlineData("192.168.001.001", "192.168.1.1")]
        [InlineData("192.168.009.001", "192.168.9.1")]
        [InlineData("124.253.063.036", "124.253.63.36")]
        [InlineData("124.253.000.000", "124.253.0.0")]
        [InlineData("000.253.063.036", "0.253.63.36")]
        [InlineData("001.253.063.036", "1.253.63.36")]
        [InlineData("001.000.063.036", "1.0.63.36")]
        [InlineData("255.255.255.255", "255.255.255.255")]
        [InlineData("000.000.000.000", "0.0.0.0")]
        [InlineData("100.010.001.000", "100.10.1.0")]
        [InlineData("0000:0000:0000:0000:0000:0000:0000:0000", "::")]
        [InlineData("0001:0002:ffff:0000:00ab:0000:0000:0123", "1:2:ffff:0:ab:0:0:123")]
        [InlineData("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff", "ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff")]
        public void IPAddressToUncompressedStringConversion_Test(string expected, string input)
        {
            // Arrange
            _ = IPAddress.TryParse(input, out var address);

            // Act
            var result = address.ToUncompressedString();

            // Assert
            Assert.Equal(expected, result);
        }

        #endregion // end: ToUncompressedString

        #region ToBase85String

        [Theory]
        [InlineData(null, null)]
        [InlineData(null, "")]
        [InlineData(null, "192.168.1.1")]
        [InlineData("$@bLmTEHhx*HIpup2~ix", "dead:beef::")]
        [InlineData("=r54lj&NUUO~Hi%c2ym0", "ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff")]
        [InlineData("00000000000000000000", "::")]
        [InlineData("000000000000000-mSjx", "::dead:beef")]
        [InlineData("4)+k&C#VzJ4br>0wv%Yp", "1080:0:0:0:8:800:200C:417A")] // specific example from RFC 1924
        public void ToBase85Test(string expected, string input)
        {
            // Arrange
            _ = IPAddress.TryParse(input, out var address);

            // Act
            var result = address.ToBase85String();

            // Assert
            Assert.Equal(expected, result);
        }

        #endregion // end: ToBase85String

        #region ToDottedQuadString

        [Theory]
        [InlineData(null, null)]
        [InlineData("1:2:3:a:b:ffff:255.255.255.255", "1:2:3:a:b:ffff:ffff:ffff")]
        [InlineData("ffff:ffff:ffff:ffff:ffff:ffff:1.2.3.4", "ffff:ffff:ffff:ffff:ffff:ffff:0102:0304")]
        [InlineData("0:ffff:ffff:ffff::255.255.255.255", "0:ffff:ffff:ffff:0:0:ffff:ffff")]
        [InlineData("::ffff:ffff:0:0:255.255.255.255", "0:0:ffff:ffff:0:0:ffff:ffff")]
        [InlineData("::ffff:ffff:ffff:0:255.255.255.255", "0:0:ffff:ffff:ffff:0:ffff:ffff")]
        [InlineData("ffff:ffff:ffff:ffff:ffff:ffff:255.255.255.255", "ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff")]
        [InlineData("ffff:ffff:ffff:ffff:ffff:ffff:0.0.0.0", "ffff:ffff:ffff:ffff:ffff:ffff::")]
        [InlineData("ffff::0.0.0.0", "ffff::")]
        [InlineData("::ffff:0.0.0.0", "::ffff:0000:0000")]
        [InlineData("::ffff:ffff:ffff:ffff:255.255.255.255", "00::ffff:ffff:ffff:ffff:ffff:ffff")]
        [InlineData("::255.255.255.255", "::ffff:ffff")]
        [InlineData("ffff::ffff:ffff:ffff:0:0.0.0.0", "ffff:0000:ffff:ffff:ffff::")]
        [InlineData("ffff::ffff:ffff:ffff:ffff:255.255.255.255", "ffff::ffff:ffff:ffff:ffff:ffff:ffff")]
        [InlineData("::0.0.0.0", "::")]
        [InlineData("192.168.1.1", "192.168.1.1")]
        public void ToDottedQuadTestTest(string expected, string input)
        {
            // Arrange
            _ = IPAddress.TryParse(input, out var address);

            // Act
            var result = address.ToDottedQuadString();

            // Assert
            Assert.Equal(expected, result);
        }

        #endregion // end: ToDottedQuadString

        #region ToHexString

        [Theory]
        [InlineData(null, null)]
        [InlineData("00000000000000000000000000000000", "::")]
        [InlineData("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF", "ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff")]
        [InlineData("0000000000000000000000000000ABCD", "::abcd")]
        [InlineData("ABCD000000000000000000000000ABCD", "abcd::abcd")]
        [InlineData("ABCD0000000000000000000000000000", "abcd::")]
        [InlineData("00000000", "0.0.0.0")]
        [InlineData("FFFFFFFF", "255.255.255.255")]
        [InlineData("80808080", "128.128.128.128")]
        [InlineData("00808080", "0.128.128.128")]
        [InlineData("80808000", "128.128.128.0")]
        public void ToHexTest(string expected, string input)
        {
            // Arrange
            _ = IPAddress.TryParse(input, out var address);

            // Act
            var result = address.ToHexString();

            // Assert
            Assert.Equal(expected, result);
        }

        #endregion // end: ToHexString

        #region

        [Theory]
        [InlineData(null, null)]
        [InlineData("0", "::")]
        [InlineData("340282366920938463463374607431768211455", "ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff")]
        [InlineData("43981", "::abcd")]
        [InlineData("228362408135220253930399759055429086157", "abcd::abcd")]
        [InlineData("228362408135220253930399759055429042176", "abcd::")]
        [InlineData("0", "0.0.0.0")]
        [InlineData("4294967295", "255.255.255.255")]
        [InlineData("2155905152", "128.128.128.128")]
        [InlineData("8421504", "0.128.128.128")]
        [InlineData("2155905024", "128.128.128.0")]
        public void ToNumericTest(string expected, string input)
        {
            // Arrange
            _ = IPAddress.TryParse(input, out var address);

            // Act
            var result = address.ToNumericString();

            // Assert
            Assert.Equal(expected, result);
        }

        #endregion // end:

        #region NetmaskToCidrRoutePrefix

        public static IEnumerable<object[]> NetmaskToCidrRoutePrefix_Test_Values()
        {
            for (var i = 0; i <= 32; i++)
            {
                var netmaskBytes = Enumerable.Repeat((byte)0xFF, 4).ToArray().ShiftBitsLeft(32 - i);

                var netmask = new IPAddress(netmaskBytes);

                yield return new object[] { i, netmask };
            }
        }

        [Theory]
        [MemberData(nameof(NetmaskToCidrRoutePrefix_Test_Values))]
        public void NetmaskToCidrRoutePrefix_Test(int expected, IPAddress address)
        {
            // Arrange
            // Act
            var result = address.NetmaskToCidrRoutePrefix();

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("::")]
        [InlineData("192.168.1.1")]
        [InlineData("0.0.0.255")]
        public void NetmaskToCidrRoutePrefix_InvalidInput_Throws_InvalidOperationException_Test(string input)
        {
            // Arrange
            var address = IPAddress.Parse(input);

            // Act
            // Assert
            Assert.Throws<InvalidOperationException>(() => address.NetmaskToCidrRoutePrefix());
        }

        [Fact]
        public void NetmaskToCidrRoutePrefix_NullInput_Throws_ArgumentNullException_Test()
        {
            // Arrange
            // Act
            // Assert

            Assert.Throws<ArgumentNullException>(() => ((IPAddress)null).NetmaskToCidrRoutePrefix());
        }

        #endregion // end: NetmaskToCidrRoutePrefix
    }
}
