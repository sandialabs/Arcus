using System.Collections.Generic;
using System.Net.Sockets;
using Arcus.Comparers;
using Xunit;

namespace Arcus.Tests.Comparers
{
    public class DefaultAddressFamilyComparerTests
    {
        [Fact]
        public void Assignability_Test()
        {
            // Arrange
            var type = typeof(DefaultAddressFamilyComparer);

            // Act
            var isAssignableFrom = typeof(IComparer<AddressFamily>).IsAssignableFrom(type);

            // Assert
            Assert.True(isAssignableFrom);
        }

        #region Compare

        public static IEnumerable<object[]> Compare_Test_Values()
        {
            var concernedAddressFamilies = new[] { AddressFamily.InterNetwork, AddressFamily.InterNetworkV6 };

            foreach (var i in concernedAddressFamilies)
            {
                foreach (var j in concernedAddressFamilies)
                {
                    yield return new object[] { i.CompareTo(j), i, j };
                }
            }
        }

        [Theory]
        [MemberData(nameof(Compare_Test_Values))]
        public void Compare_Test(int expected, AddressFamily x, AddressFamily y)
        {
            // Arrange
            var comparer = new DefaultAddressFamilyComparer();

            // Act
            var result = comparer.Compare(x, y);

            // Assert
            Assert.Equal(expected, result);
        }

        #endregion // end: Compare
    }
}
