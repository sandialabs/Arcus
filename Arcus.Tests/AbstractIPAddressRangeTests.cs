using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using NUnit.Framework;
using Rhino.Mocks;

namespace Arcus.Tests
{
    [TestFixture]
    public class AbstractIPAddressRangeTests
    {
        [TestCase("::dead:beef", "192.168.1.1")]
        [TestCase("192.168.1.1", "::dead:beef")]
        public void IsIPv4IsIPv6HeadAndTailNotMatchNotIPv6NotIPv4Test(string headString,
                                                                      string tailString)
        {
            // Arrange
            var headIPAddress = IPAddress.Parse(headString);
            var tailIPAddress = IPAddress.Parse(tailString);

            var mockIPAddressRange = MockRepository.GenerateStub<AbstractIPAddressRange>();
            mockIPAddressRange.Head = headIPAddress;
            mockIPAddressRange.Tail = tailIPAddress;

            // Assert
            Assert.IsFalse(mockIPAddressRange.IsIPv4);
            Assert.IsFalse(mockIPAddressRange.IsIPv6);
        }

        [TestCase("::", "::ABCD", "::1234", ExpectedResult = true, Description = "IPv6 range contains IPv6")]
        [TestCase("::", "::ABCD", "::FFFF", ExpectedResult = false, Description = "IPv6 range  doesn't contains IPv6")]
        [TestCase("::", "::ABCD", "::", ExpectedResult = true, Description = "IPv6 range contains IPv6 head")]
        [TestCase("::", "::ABCD", "::ABCD", ExpectedResult = true, Description = "IPv6 range contains IPv6 tail")]
        [TestCase("::", "::ABCD", "192.168.1.1", ExpectedResult = false, Description = "IPv6 range doesn't contains IPv4")]
        [TestCase("192.168.0.1", "192.168.0.254", "192.168.0.128", ExpectedResult = true, Description = "IPv4 range contains IPv4")]
        [TestCase("192.168.0.1", "192.168.0.254", "10.1.1.1", ExpectedResult = false, Description = "IPv4 range doesn't contains IPv4")]
        [TestCase("192.168.0.1", "192.168.0.254", "192.168.0.1", ExpectedResult = true, Description = "IPv4 range contains IPv4 head")]
        [TestCase("192.168.0.1", "192.168.0.254", "192.168.0.254", ExpectedResult = true, Description = "IPv4 range contains IPv4 tail")]
        [TestCase("192.168.0.1", "192.168.0.254", "::", ExpectedResult = false, Description = "IPv4 range doesn't contains IPv6")]
        public bool ContainsIPAddressTest(string headString,
                                          string tailString,
                                          string subHeadAddress)
        {
            // Arrange
            var headIPAddress = IPAddress.Parse(headString);
            var tailIPAddress = IPAddress.Parse(tailString);

            var mockIPAddressRange = MockRepository.GenerateStub<AbstractIPAddressRange>();
            mockIPAddressRange.Head = headIPAddress;
            mockIPAddressRange.Tail = tailIPAddress;

            var containsAddress = IPAddress.Parse(subHeadAddress);

            // Act
            var result = mockIPAddressRange.Contains(containsAddress);

            // Assert
            return result;
        }

        [TestCase("::", "::ABCD", "::", "::ABCD", ExpectedResult = true, Description = "IPv6 range contains self")]
        [TestCase("::", "::ABCD", "::1", "::1234", ExpectedResult = true, Description = "IPv6 range contains internal IPv6 range")]
        [TestCase("::", "::ABCD", "AB::FFFF", "AA::FFFF", ExpectedResult = false, Description = "IPv6 range doesn't contains external IPv6 range")]
        [TestCase("::", "::ABCD", null, "::1234", ExpectedResult = false, Description = "IPv6 range doesn't contains null head IPv6 range")]
        [TestCase("::", "::ABCD", "::1", null, ExpectedResult = false, Description = "IPv6 range doesn't contains null tail IPv6 range")]
        [TestCase("A::", "A::ABCD", "::", "A::", ExpectedResult = false, Description = "IPv6 range  doesn't contains IPv6 overhang head")]
        [TestCase("::", "::ABCD", "::", "AA::FFFF", ExpectedResult = false, Description = "IPv6 range  doesn't contains IPv6 overhang tail")]
        [TestCase("::", "::ABCD", "192.168.1.1", "192.168.1.10", ExpectedResult = false, Description = "IPv6 range doesn't contains IPv4 range")]
        [TestCase("128.0.0.1", "128.0.0.254", "128.0.0.1", "128.0.0.254", ExpectedResult = true, Description = "IPv4 range contains self")]
        [TestCase("128.0.0.1", "128.0.0.254", "128.0.0.5", "128.0.0.100", ExpectedResult = true, Description = "IPv4 range contains internal IPv4 range")]
        [TestCase("128.0.0.1", "128.0.0.254", "140.0.0.0", "145.0.0.0", ExpectedResult = false, Description = "IPv4 range doesn't contains external IPv4 range")]
        [TestCase("128.0.0.1", "128.0.0.254", "", "128.0.0.100", ExpectedResult = false, Description = "IPv4 range doesn't contains null head IPv4 range")]
        [TestCase("128.0.0.1", "128.0.0.254", "128.0.0.5", "", ExpectedResult = false, Description = "IPv4 range doesn't contains null tail IPv4 range")]
        [TestCase("128.0.0.1", "128.0.0.254", "0.0.0.0", "128.0.0.100", ExpectedResult = false, Description = "IPv4 range  doesn't contains IPv4 overhang head")]
        [TestCase("128.0.0.1", "128.0.0.254", "128.0.0.5", "145.0.0.0", ExpectedResult = false, Description = "IPv4 range  doesn't contains IPv4 overhang tail")]
        [TestCase("128.0.0.1", "128.0.0.254", "::ABCD", "42::ABCD", ExpectedResult = false, Description = "IPv4 range doesn't contains IPv6 range")]
        public bool ContainsIIPAddressRangeTest(string headString,
                                                string tailString,
                                                string subHeadAddress,
                                                string subTailAddress)
        {
            // Arrange
            var headIPAddress = IPAddress.Parse(headString);
            var tailIPAddress = IPAddress.Parse(tailString);

            var mockIPAddressRange = MockRepository.GenerateStub<AbstractIPAddressRange>();
            mockIPAddressRange.Head = headIPAddress;
            mockIPAddressRange.Tail = tailIPAddress;

            var mockIPAddressSubRange = MockRepository.GenerateStub<AbstractIPAddressRange>();

            IPAddress subHeadIPAddress;
            if (!IPAddress.TryParse(subHeadAddress, out subHeadIPAddress))
            {
                subHeadIPAddress = null;
            }

            mockIPAddressSubRange.Head = subHeadIPAddress;

            IPAddress subTailIPAddress;
            if (!IPAddress.TryParse(subTailAddress, out subTailIPAddress))
            {
                subTailIPAddress = null;
            }
            mockIPAddressSubRange.Tail = subTailIPAddress;

            // Act
            var result = mockIPAddressRange.Contains(mockIPAddressSubRange);

            // Assert
            return result;
        }

        [Test(Description = "Test when head is null the Address collection is null")]
        public void AddressesOnNullHeadReturnEmptyTest()
        {
            // Arrange
            const string address = "192.168.1.1";
            var ipAddress = IPAddress.Parse(address);

            var mockIPAddressRange = MockRepository.GenerateStub<AbstractIPAddressRange>();
            mockIPAddressRange.Head = null;
            mockIPAddressRange.Tail = ipAddress;

            // Act
            var addresses = mockIPAddressRange.Addresses.ToList();

            // Assert
            Assert.IsFalse(addresses.Any());
        }

        [Test(Description = "Test when tail is null the Address collection is null")]
        public void AddressesOnNullTailReturnEmptyTest()
        {
            // Arrange
            const string address = "192.168.1.1";
            var ipAddress = IPAddress.Parse(address);

            var mockIPAddressRange = MockRepository.GenerateStub<AbstractIPAddressRange>();
            mockIPAddressRange.Head = ipAddress;
            mockIPAddressRange.Tail = null;

            // Act
            var addresses = mockIPAddressRange.Addresses.ToList();

            // Assert
            Assert.IsFalse(addresses.Any());
        }

        [Test]
        public void AddressFamilyHeadNotMatchingTailThrowsInvalidOperationExceptionTest()
        {
            // Arrange
            const string headString = "192.168.1.1";
            const string tailString = "::dead:beef";
            var headIPAddress = IPAddress.Parse(headString);
            var tailIPAddress = IPAddress.Parse(tailString);

            var mockIPAddressRange = MockRepository.GenerateStub<AbstractIPAddressRange>();
            mockIPAddressRange.Head = headIPAddress;
            mockIPAddressRange.Tail = tailIPAddress;

            // Assert
            Assert.Throws<InvalidOperationException>(() => { var addressFamily = mockIPAddressRange.AddressFamily; });
        }

        [Test]
        public void AddressFamilyIPv4Test()
        {
            // Arrange
            const string headString = "192.168.1.1";
            const string tailString = "192.168.1.5";
            var headIPAddress = IPAddress.Parse(headString);
            var tailIPAddress = IPAddress.Parse(tailString);

            var mockIPAddressRange = MockRepository.GenerateStub<AbstractIPAddressRange>();
            mockIPAddressRange.Head = headIPAddress;
            mockIPAddressRange.Tail = tailIPAddress;

            // Assert
            Assert.AreEqual(AddressFamily.InterNetwork, mockIPAddressRange.AddressFamily);
        }

        [Test]
        public void AddressFamilyIPv6Test()
        {
            // Arrange
            const string headString = "::dead";
            const string tailString = "::beef";
            var headIPAddress = IPAddress.Parse(headString);
            var tailIPAddress = IPAddress.Parse(tailString);

            var mockIPAddressRange = MockRepository.GenerateStub<AbstractIPAddressRange>();
            mockIPAddressRange.Head = headIPAddress;
            mockIPAddressRange.Tail = tailIPAddress;

            // Assert
            Assert.AreEqual(AddressFamily.InterNetworkV6, mockIPAddressRange.AddressFamily);
        }

        [Test]
        public void AddressFamilyNullHeadThrowsNullReferenceExceptionTest()
        {
            // Arrange
            const string addressString = "::a";
            var address = IPAddress.Parse(addressString);

            var mockIPAddressRange = MockRepository.GenerateStub<AbstractIPAddressRange>();
            mockIPAddressRange.Head = null;
            mockIPAddressRange.Tail = address;

            // Assert
            Assert.Throws<NullReferenceException>(() => { var addressFamily = mockIPAddressRange.AddressFamily; });
        }

        [Test]
        public void AddressFamilyNullTailThrowsNullReferenceExceptionTest()
        {
            // Arrange
            const string addressString = "::a";
            var address = IPAddress.Parse(addressString);

            var mockIPAddressRange = MockRepository.GenerateStub<AbstractIPAddressRange>();
            mockIPAddressRange.Head = address;
            mockIPAddressRange.Tail = null;

            // Assert
            Assert.Throws<NullReferenceException>(() => { var addressFamily = mockIPAddressRange.AddressFamily; });
        }

        [Test(Description = "Test that the expected addresses appear in the given IPv4 range")]
        public void AddressIPv4Test()
        {
            // Arrange
            const string headString = "192.168.1.1";
            const string tailString = "192.168.1.3";
            var headIPAddress = IPAddress.Parse(headString);
            var tailIPAddress = IPAddress.Parse(tailString);

            var mockIPAddressRange = MockRepository.GenerateStub<AbstractIPAddressRange>();
            mockIPAddressRange.Head = headIPAddress;
            mockIPAddressRange.Tail = tailIPAddress;

            // Act
            var addresses = mockIPAddressRange.Addresses.ToList();

            // Assert
            Assert.AreEqual(3, addresses.Count);
            CollectionAssert.AreEquivalent(new[] {headIPAddress, IPAddress.Parse("192.168.1.2"), tailIPAddress}, addresses);
        }

        [Test(Description = "Test that the expected addresses appear in the given IPv6 range")]
        public void AddressIPv6Test()
        {
            // Arrange
            const string headString = "::a";
            const string tailString = "::c";
            var headIPAddress = IPAddress.Parse(headString);
            var tailIPAddress = IPAddress.Parse(tailString);

            var mockIPAddressRange = MockRepository.GenerateStub<AbstractIPAddressRange>();
            mockIPAddressRange.Head = headIPAddress;
            mockIPAddressRange.Tail = tailIPAddress;

            // Act
            var addresses = mockIPAddressRange.Addresses.ToList();

            // Assert
            Assert.AreEqual(3, addresses.Count);
            CollectionAssert.AreEquivalent(new[] {headIPAddress, IPAddress.Parse("::b"), tailIPAddress}, addresses);
        }

        [Test(Description = "Test that a range of one returns a single address when Addresses is called")]
        public void AddressSameHeadAndTailReturnsOneAddressTest()
        {
            // Arrange
            const string address = "192.168.1.1";
            var ipAddress = IPAddress.Parse(address);

            var mockIPAddressRange = MockRepository.GenerateStub<AbstractIPAddressRange>();
            mockIPAddressRange.Head = ipAddress;
            mockIPAddressRange.Tail = ipAddress;

            // Act
            var addresses = mockIPAddressRange.Addresses.ToArray();

            // Assert
            Assert.AreEqual(1, addresses.Length);
            Assert.AreEqual(address, addresses.Single()
                                              .ToString());
        }

        [Test]
        public void ImplementationTest()
        {
            Assert.That(typeof (IIPAddressRange).IsAssignableFrom(typeof (AbstractIPAddressRange)));
        }

        [Test]
        public void IsIPv4IsIPv6HeadAndTailIPv4NotIPv6Test()
        {
            // Arrange
            const string headString = "192.168.1.1";
            const string tailString = "192.168.1.5";
            var headIPAddress = IPAddress.Parse(headString);
            var tailIPAddress = IPAddress.Parse(tailString);

            var mockIPAddressRange = MockRepository.GenerateStub<AbstractIPAddressRange>();
            mockIPAddressRange.Head = headIPAddress;
            mockIPAddressRange.Tail = tailIPAddress;

            // Assert
            Assert.IsTrue(mockIPAddressRange.IsIPv4);
            Assert.IsFalse(mockIPAddressRange.IsIPv6);
        }

        [Test]
        public void IsIPv4IsIPv6HeadAndTailIPv6NotIPv4Test()
        {
            // Arrange
            const string headString = "::dead:beef";
            const string tailString = "::f7ee:7ac0";
            var headIPAddress = IPAddress.Parse(headString);
            var tailIPAddress = IPAddress.Parse(tailString);

            var mockIPAddressRange = MockRepository.GenerateStub<AbstractIPAddressRange>();
            mockIPAddressRange.Head = headIPAddress;
            mockIPAddressRange.Tail = tailIPAddress;

            // Assert
            Assert.IsFalse(mockIPAddressRange.IsIPv4);
            Assert.IsTrue(mockIPAddressRange.IsIPv6);
        }

        [Test]
        public void IsIPv4IsIPv6HeadNullIPv4OrIPv6Test()
        {
            // Arrange
            const string ipString = "::7ac0";
            var ipAddress = IPAddress.Parse(ipString);

            var mockIPAddressRange = MockRepository.GenerateStub<AbstractIPAddressRange>();
            mockIPAddressRange.Head = null;
            mockIPAddressRange.Tail = ipAddress;

            // Assert
            Assert.IsFalse(mockIPAddressRange.IsIPv4);
            Assert.IsFalse(mockIPAddressRange.IsIPv6);
        }

        [Test]
        public void IsIPv4IsIPv6TailNullIPv4OrIPv6Test()
        {
            // Arrange
            const string ipString = "::7ac0";
            var ipAddress = IPAddress.Parse(ipString);

            var mockIPAddressRange = MockRepository.GenerateStub<AbstractIPAddressRange>();
            mockIPAddressRange.Head = ipAddress;
            mockIPAddressRange.Tail = null;

            // Assert
            Assert.IsFalse(mockIPAddressRange.IsIPv4);
            Assert.IsFalse(mockIPAddressRange.IsIPv6);
        }
    }
}
