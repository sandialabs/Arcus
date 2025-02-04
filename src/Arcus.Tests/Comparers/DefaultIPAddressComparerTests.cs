using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Arcus.Comparers;
using Moq;
using Xunit;

namespace Arcus.Tests.Comparers
{
    public class DefaultIPAddressComparerTests
    {
        [Fact]
        public void Assignability_Test()
        {
            // Arrange
            var type = typeof(DefaultIPAddressComparer);

            // Act
            var isAssignableFrom = typeof(IComparer<IPAddress>).IsAssignableFrom(type);

            // Assert
            Assert.True(isAssignableFrom);
        }

        #region Ctor

        [Fact]
        public void Ctor_NullInput_Throws_ArgumentNullException_Test()
        {
            // Arrange
            // Act
            // Assert

            Assert.Throws<ArgumentNullException>(() => new DefaultIPAddressComparer(null));
        }

        #endregion // end: Ctor

        #region Compare

        public static IEnumerable<object[]> Compare_Test_Values()
        {
            // equal addresses
            yield return new object[] { 0, IPAddress.Parse("192.168.1.1"), IPAddress.Parse("192.168.1.1") };
            yield return new object[] { 0, IPAddress.Parse("dead::beef"), IPAddress.Parse("dead::beef") };

            // same address
            var ipv4Same = IPAddress.Parse("192.168.1.1");
            yield return new object[] { 0, ipv4Same, ipv4Same };

            var ipv6Same = IPAddress.Parse("dead::beef");
            yield return new object[] { 0, ipv6Same, ipv6Same };

            // null compare
            yield return new object[] { 0, null, null };
            yield return new object[] { -1, null, IPAddress.Parse("192.168.1.1") };
            yield return new object[] { 1, IPAddress.Parse("192.168.1.1"), null };
            yield return new object[] { -1, null, IPAddress.Parse("dead::beef") };
            yield return new object[] { 1, IPAddress.Parse("dead::beef"), null };

            // numerically equivalent, different address families
            yield return new object[] { -1, IPAddress.Parse("0.0.0.0"), IPAddress.Parse("::") };
            yield return new object[] { 1, IPAddress.Parse("::"), IPAddress.Parse("0.0.0.0") };

            // satisfies ordinal ordering
            yield return new object[] { -1, IPAddress.Parse("192.168.1.1"), IPAddress.Parse("192.168.1.2") };
            yield return new object[] { 1, IPAddress.Parse("192.168.1.2"), IPAddress.Parse("192.168.1.1") };
            yield return new object[] { -1, IPAddress.Parse("abc::123"), IPAddress.Parse("abc::124") };
            yield return new object[] { 1, IPAddress.Parse("abc::124"), IPAddress.Parse("abc::123") };
        }

        [Theory]
        [MemberData(nameof(Compare_Test_Values))]
        public void Compare_Test(int expected, IPAddress x, IPAddress y)
        {
            // Arrange
            var comparer = new DefaultIPAddressComparer();

            // Act

            var result = comparer.Compare(x, y);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void Compare_DeferToAddressFamilyComparer_Test()
        {
            // Arrange
            const int expectedResult = 42;
            var x = IPAddress.Any;
            var y = IPAddress.IPv6Any;

            var mockAddressFamilyComparer = new Mock<IComparer<AddressFamily>>();

            mockAddressFamilyComparer.Setup(c => c.Compare(x.AddressFamily, y.AddressFamily)).Returns(expectedResult);

            var comparer = new DefaultIPAddressComparer(mockAddressFamilyComparer.Object);

            // Act
            var result = comparer.Compare(x, y);

            // Assert
            Assert.Equal(expectedResult, result);
            mockAddressFamilyComparer.Verify(c => c.Compare(x.AddressFamily, y.AddressFamily), Times.Once);
        }

        #endregion // end: Compare
    }
}
