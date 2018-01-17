using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Arcus.Comparers;
using NUnit.Framework;
using Rhino.Mocks;

namespace Arcus.Tests.Comparers
{
    [TestFixture]
    public class DefaultIPAddressComparerTests
    {
        [TestCase("", "", ExpectedResult = 0)]
        [TestCase("", "192.168.1.1", ExpectedResult = -1)]
        [TestCase("", "ffff::ca75", ExpectedResult = -1)]
        [TestCase("0.0.0.0", "255.255.255.255", ExpectedResult = -1)]
        [TestCase("0.0.0.0", "::", ExpectedResult = -1)]
        [TestCase("0.0.255.255", "::ff", ExpectedResult = -1)]
        [TestCase("1010::ca75", "::ca75", ExpectedResult = 1)]
        [TestCase("192.168.1.1", "", ExpectedResult = 1)]
        [TestCase("192.168.1.1", "192.168.1.1", ExpectedResult = 0)]
        [TestCase("192.168.1.1", "192.168.1.2", ExpectedResult = -1)]
        [TestCase("192.168.1.1", null, ExpectedResult = 1)]
        [TestCase("192.168.1.2", "192.168.1.1", ExpectedResult = 1)]
        [TestCase("255.255.255.255", "0.0.0.0", ExpectedResult = 1)]
        [TestCase("255.255.255.255", "::ffff:ffff", ExpectedResult = -1)]
        [TestCase("255.255.255.255", "ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff", ExpectedResult = -1)]
        [TestCase("::", "0.0.0.0", ExpectedResult = 1)]
        [TestCase("::", "ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff", ExpectedResult = -1)]
        [TestCase("::ca75", "1010::ca75", ExpectedResult = -1)]
        [TestCase("::ff", "0.0.255.255", ExpectedResult = 1)]
        [TestCase("::ffff:ffff", "255.255.255.255", ExpectedResult = 1)]
        [TestCase("ab::cd:ef01", "ab::cd:ef01", ExpectedResult = 0)]
        [TestCase("ab::cd:ef01", "ab::cd:ff01", ExpectedResult = -1)]
        [TestCase("ab::cd:ef01", null, ExpectedResult = 1)]
        [TestCase("ab::cd:ff01", "ab::cd:ef01", ExpectedResult = 1)]
        [TestCase("ffff::ca75", "", ExpectedResult = 1)]
        [TestCase("ffff::ca75", "ffff::ca75", ExpectedResult = 0)]
        [TestCase("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff", "255.255.255.255", ExpectedResult = 1)]
        [TestCase("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff", "::", ExpectedResult = 1)]
        [TestCase(null, "192.168.1.1", ExpectedResult = -1)]
        [TestCase(null, "ab::cd:ef01", ExpectedResult = -1)]
        [TestCase(null, null, ExpectedResult = 0)]
        public int CompareTest(string x,
                               string y)
        {
            // Arrange
            IPAddress addressX;
            if (!IPAddress.TryParse(x, out addressX))
            {
                addressX = null;
            }

            IPAddress addressY;
            if (!IPAddress.TryParse(y, out addressY))
            {
                addressY = null;
            }

            var comparer = new DefaultIPAddressComparer();

            // Act

            var result = comparer.Compare(addressX, addressY);

            // Assert
            return result;
        }

        [Test]
        public void DeferToAddressFamilyComparerTest()
        {
            // Arrange
            var address1 = IPAddress.Any;
            var address2 = IPAddress.IPv6Any;

            var mockAddressFamilyComparer = MockRepository.GenerateStub<IComparer<AddressFamily>>();
            mockAddressFamilyComparer.Expect(c => c.Compare(Arg<AddressFamily>.Is.Same(address1.AddressFamily), Arg<AddressFamily>.Is.Same(address2.AddressFamily)))
                                     .Return(0);

            var comparer = new DefaultIPAddressComparer(mockAddressFamilyComparer);

            // Act

            var result = comparer.Compare(address1, address2);

            // Assert
            mockAddressFamilyComparer.VerifyAllExpectations();
        }

        [Test]
        public void ImplementationTest()
        {
            Assert.That(typeof (IComparer<IPAddress>).IsAssignableFrom(typeof (DefaultIPAddressComparer)));
        }

        [Test]
        public void NullConstructionTest()
        {
            Assert.Throws<ArgumentNullException>(() => new DefaultIPAddressComparer(null));
        }
    }
}
