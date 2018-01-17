using System;
using System.Collections.Generic;
using System.Net;
using Arcus.Comparers;
using NUnit.Framework;
using Rhino.Mocks;

namespace Arcus.Tests.Comparers
{
    [TestFixture]
    public class DefaultSubnetComparerTests
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
            Subnet subnetX;
            if (!Subnet.TryParse(x, out subnetX))
            {
                subnetX = null;
            }

            Subnet subnetY;
            if (!Subnet.TryParse(y, out subnetY))
            {
                subnetY = null;
            }

            var comparer = new DefaultSubnetComparer();

            // Act

            var result = comparer.Compare(subnetX, subnetY);

            // Assert
            return result;
        }

        [Test]
        public void DeferToIPAddressComparerTest()
        {
            // Arrange
            var address1 = IPAddress.Any;
            var address2 = IPAddress.IPv6Any;

            var subnet1 = new Subnet(address1, 16);
            var subnet2 = new Subnet(address2, 64);

            var mockIPAddressComparer = MockRepository.GenerateStub<IComparer<IPAddress>>();
            mockIPAddressComparer.Expect(c => c.Compare(Arg<IPAddress>.Is.Same(address1), Arg<IPAddress>.Is.Same(address2)))
                                 .Return(0);

            var comparer = new DefaultSubnetComparer(mockIPAddressComparer);

            // Act
            var result = comparer.Compare(subnet1, subnet2);

            // Assert
            mockIPAddressComparer.VerifyAllExpectations();
        }

        [Test]
        public void ImplementationTest()
        {
            Assert.That(typeof (IComparer<Subnet>).IsAssignableFrom(typeof (DefaultSubnetComparer)));
        }

        [Test]
        public void NullConstructionTest()
        {
            Assert.Throws<ArgumentNullException>(() => new DefaultSubnetComparer(null));
        }
    }
}
