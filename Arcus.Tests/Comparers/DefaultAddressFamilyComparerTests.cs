using System.Collections.Generic;
using System.Net.Sockets;
using Arcus.Comparers;
using NUnit.Framework;

namespace Arcus.Tests.Comparers
{
    [TestFixture]
    public class DefaultAddressFamilyComparerTests
    {
        [TestCase(AddressFamily.InterNetwork, AddressFamily.InterNetwork, ExpectedResult = 0)]
        [TestCase(AddressFamily.InterNetworkV6, AddressFamily.InterNetworkV6, ExpectedResult = 0)]
        [TestCase(AddressFamily.InterNetwork, AddressFamily.InterNetworkV6, ExpectedResult = -1)]
        [TestCase(AddressFamily.InterNetworkV6, AddressFamily.InterNetwork, ExpectedResult = 1)]
        [TestCase(AddressFamily.InterNetwork, AddressFamily.InterNetwork, ExpectedResult = 0)]
        [TestCase(AddressFamily.InterNetworkV6, AddressFamily.InterNetworkV6, ExpectedResult = 0)]
        [TestCase(AddressFamily.InterNetwork, AddressFamily.InterNetworkV6, ExpectedResult = -1)]
        [TestCase(AddressFamily.InterNetworkV6, AddressFamily.InterNetwork, ExpectedResult = 1)]
        [TestCase(AddressFamily.InterNetwork, AddressFamily.InterNetwork, ExpectedResult = 0)]
        [TestCase(AddressFamily.InterNetworkV6, AddressFamily.InterNetworkV6, ExpectedResult = 0)]
        public int CompareTest(AddressFamily x,
                               AddressFamily y)
        {
            // Arrange
            var comparer = new DefaultAddressFamilyComparer();

            // Act
            var compare = comparer.Compare(x, y);

            // Assert
            return compare;
        }

        [Test]
        public void ImplementationTest()
        {
            Assert.That(typeof (IComparer<AddressFamily>).IsAssignableFrom(typeof (DefaultAddressFamilyComparer)));
        }
    }
}
