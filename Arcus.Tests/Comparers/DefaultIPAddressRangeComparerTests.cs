using System;
using System.Collections.Generic;
using System.Net;
using Arcus.Comparers;
using NUnit.Framework;
using Rhino.Mocks;

namespace Arcus.Tests.Comparers
{
    [TestFixture]
    public class DefaultIPAddressRangeComparerTests
    {
        [TestCase(null, null, ExpectedResult = 0)]
        [TestCase("192.168.0.0/16", "192.168.0.0/16", ExpectedResult = 0)]
        [TestCase("ab:cd::/64", "ab:cd::/64", ExpectedResult = 0)]
        [TestCase(null, "192.168.0.0/16", ExpectedResult = -1)]
        [TestCase("192.168.0.0/16", null, ExpectedResult = 1)]
        [TestCase(null, "ab:cd::/64", ExpectedResult = -1)]
        [TestCase("ab:cd::/64", null, ExpectedResult = 1)]
        [TestCase("192.168.0.0/16", "192.168.0.0/20", ExpectedResult = 1)]
        [TestCase("192.168.0.0/20", "192.168.0.0/16", ExpectedResult = -1)]
        [TestCase("ab:cd::/64", "ab:cd::/96", ExpectedResult = 1)]
        [TestCase("ab:cd::/96", "ab:cd::/64", ExpectedResult = -1)]
        [TestCase("0.0.0.0/0", "::/0", ExpectedResult = -1)]
        [TestCase("::/0", "0.0.0.0/0", ExpectedResult = 1)]
        [TestCase("0.0.0.0/32", "::/128", ExpectedResult = -1)]
        [TestCase("::/128", "0.0.0.0/32", ExpectedResult = 1)]
        public int CompareTest(string x,
                               string y)
        {
            // Arrange
            IPAddressRange addressRangeX = null;
            Subnet subnetX;
            if (Subnet.TryParse(x, out subnetX))
            {
                addressRangeX = new IPAddressRange(subnetX.Head, subnetX.Tail);
            }

            IPAddressRange addressRangeY = null;
            Subnet subnetY;
            if (Subnet.TryParse(y, out subnetY))
            {
                addressRangeY = new IPAddressRange(subnetY.Head, subnetY.Tail);
            }

            var comparer = new DefaultIPAddressRangeComparer();

            // Act

            var result = comparer.Compare(addressRangeX, addressRangeY);

            // Assert
            return result;
        }

        [Test]
        public void DeferToIPAddressComparerTest()
        {
            // Arrange
            var address1 = IPAddress.Any;
            var address2 = IPAddress.IPv6Any;

            var subnet1 = new IPAddressRange(address1, address1);
            var subnet2 = new IPAddressRange(address2, address2);

            var mockIPAddressComparer = MockRepository.GenerateStub<IComparer<IPAddress>>();
            mockIPAddressComparer.Expect(c => c.Compare(Arg<IPAddress>.Is.Same(address1), Arg<IPAddress>.Is.Same(address2)))
                                 .Return(0);

            var comparer = new DefaultIPAddressRangeComparer(mockIPAddressComparer);

            // Act
            var result = comparer.Compare(subnet1, subnet2);

            // Assert
            mockIPAddressComparer.VerifyAllExpectations();
        }

        [Test]
        public void ImplementationTest()
        {
            Assert.That(typeof (IComparer<IPAddressRange>).IsAssignableFrom(typeof (DefaultIPAddressRangeComparer)));
        }

        [Test]
        public void NullConstructionTest()
        {
            Assert.Throws<ArgumentNullException>(() => new DefaultIPAddressRangeComparer(null));
        }
    }
}
