using System;
using System.Net;
using Arcus.Math;
using NUnit.Framework;

namespace Arcus.Tests.Math
{
    [TestFixture]
    public class IPAddressMathTests
    {
        [TestCase("192.168.1.0", "192.168.1.0", ExpectedResult = "192.168.1.0")]
        [TestCase("192.168.1.0", "192.168.1.1", ExpectedResult = "192.168.1.1")]
        [TestCase("192.168.1.1", "192.168.1.0", ExpectedResult = "192.168.1.1")]
        [TestCase("::", "::", ExpectedResult = "::")]
        [TestCase("::", "::ffff", ExpectedResult = "::ffff")]
        [TestCase("::ffff", "::ffff", ExpectedResult = "::ffff")]
        public string MaxTest(string alpha,
                              string beta)
        {
            // Arrange
            IPAddress alphaAddress;
            IPAddress.TryParse(alpha, out alphaAddress);

            IPAddress betaAddress;
            IPAddress.TryParse(beta, out betaAddress);

            // Act
            var result = IPAddressMath.Max(alphaAddress, betaAddress)
                                      .ToString();

            // Assert
            return result;
        }

        [TestCase("192.168.1.0", "192.168.1.0", ExpectedResult = "192.168.1.0")]
        [TestCase("192.168.1.0", "192.168.1.1", ExpectedResult = "192.168.1.0")]
        [TestCase("192.168.1.1", "192.168.1.0", ExpectedResult = "192.168.1.0")]
        [TestCase("::", "::", ExpectedResult = "::")]
        [TestCase("::", "::ffff", ExpectedResult = "::")]
        [TestCase("::ffff", "::", ExpectedResult = "::")]
        public string MinTest(string alpha,
                              string beta)
        {
            // Arrange
            IPAddress alphaAddress;
            IPAddress.TryParse(alpha, out alphaAddress);

            IPAddress betaAddress;
            IPAddress.TryParse(beta, out betaAddress);

            // Act
            var result = IPAddressMath.Min(alphaAddress, betaAddress)
                                      .ToString();

            // Assert
            return result;
        }

        [TestCase("192.168.1.1", "192.168.1.1", ExpectedResult = true)]
        [TestCase("192.168.1.1", "192.168.1.2", ExpectedResult = false)]
        [TestCase("192.168.1.2", "192.168.1.1", ExpectedResult = false)]
        [TestCase("a::", "a::", ExpectedResult = true)]
        [TestCase("::", "a::", ExpectedResult = false)]
        [TestCase("a::", "::", ExpectedResult = false)]
        [TestCase("0.0.0.0", "::", ExpectedResult = false)]
        [TestCase("::", "0.0.0.0", ExpectedResult = false)]
        [TestCase("", "::", ExpectedResult = false)]
        [TestCase("::", "", ExpectedResult = false)]
        [TestCase("0.0.0.0", "", ExpectedResult = false)]
        [TestCase("", "0.0.0.0", ExpectedResult = false)]
        [TestCase("", "", ExpectedResult = true)]
        public bool EqualToTest(string alpha,
                           string beta)
        {
            // Arrange
            IPAddress alphaAddress;
            if (!IPAddress.TryParse(alpha, out alphaAddress))
            {
                alphaAddress = null;
            }

            IPAddress betaAddress;
            if (!IPAddress.TryParse(beta, out betaAddress))
            {
                betaAddress = null;
            }

            // Act
            var result = alphaAddress.IsEqualTo(betaAddress);

            // Assert
            return result;
        }

        [TestCase("192.168.1.1", "192.168.1.1", ExpectedResult = false)]
        [TestCase("192.168.1.1", "192.168.1.2", ExpectedResult = false)]
        [TestCase("192.168.1.2", "192.168.1.1", ExpectedResult = true)]
        [TestCase("a::", "a::", ExpectedResult = false)]
        [TestCase("::", "a::", ExpectedResult = false)]
        [TestCase("a::", "::", ExpectedResult = true)]
        [TestCase("0.0.0.0", "::", ExpectedResult = false)]
        [TestCase("::", "0.0.0.0", ExpectedResult = false)]
        [TestCase("", "::", ExpectedResult = false)]
        [TestCase("::", "", ExpectedResult = false)]
        [TestCase("0.0.0.0", "", ExpectedResult = false)]
        [TestCase("", "0.0.0.0", ExpectedResult = false)]
        [TestCase("", "", ExpectedResult = false)]
        public bool GreaterThanTest(string alpha,
                           string beta)
        {
            // Arrange
            IPAddress alphaAddress;
            if (!IPAddress.TryParse(alpha, out alphaAddress))
            {
                alphaAddress = null;
            }

            IPAddress betaAddress;
            if (!IPAddress.TryParse(beta, out betaAddress))
            {
                betaAddress = null;
            }

            // Act
            var result = alphaAddress.IsGreaterThan(betaAddress);

            // Assert
            return result;
        }

        [TestCase("192.168.1.1", "192.168.1.1", ExpectedResult = true)]
        [TestCase("192.168.1.1", "192.168.1.2", ExpectedResult = false)]
        [TestCase("192.168.1.2", "192.168.1.1", ExpectedResult = true)]
        [TestCase("a::", "a::", ExpectedResult = true)]
        [TestCase("::", "a::", ExpectedResult = false)]
        [TestCase("a::", "::", ExpectedResult = true)]
        [TestCase("0.0.0.0", "::", ExpectedResult = false)]
        [TestCase("::", "0.0.0.0", ExpectedResult = false)]
        [TestCase("", "::", ExpectedResult = false)]
        [TestCase("::", "", ExpectedResult = false)]
        [TestCase("0.0.0.0", "", ExpectedResult = false)]
        [TestCase("", "0.0.0.0", ExpectedResult = false)]
        [TestCase("", "", ExpectedResult = true)]
        public bool GreaterThanOrEqualToTest(string alpha,
                            string beta)
        {
            // Arrange
            IPAddress alphaAddress;
            if (!IPAddress.TryParse(alpha, out alphaAddress))
            {
                alphaAddress = null;
            }

            IPAddress betaAddress;
            if (!IPAddress.TryParse(beta, out betaAddress))
            {
                betaAddress = null;
            }

            // Act
            var result = alphaAddress.IsGreaterThanOrEqualTo(betaAddress);

            // Assert
            return result;
        }

        [TestCase("192.168.1.1", "192.168.1.1", ExpectedResult = false)]
        [TestCase("192.168.1.1", "192.168.1.2", ExpectedResult = true)]
        [TestCase("192.168.1.2", "192.168.1.1", ExpectedResult = false)]
        [TestCase("a::", "a::", ExpectedResult = false)]
        [TestCase("::", "a::", ExpectedResult = true)]
        [TestCase("a::", "::", ExpectedResult = false)]
        [TestCase("0.0.0.0", "::", ExpectedResult = false)]
        [TestCase("::", "0.0.0.0", ExpectedResult = false)]
        [TestCase("", "::", ExpectedResult = false)]
        [TestCase("::", "", ExpectedResult = false)]
        [TestCase("0.0.0.0", "", ExpectedResult = false)]
        [TestCase("", "0.0.0.0", ExpectedResult = false)]
        [TestCase("", "", ExpectedResult = false)]
        public bool LessThanTest(string alpha,
                           string beta)
        {
            // Arrange
            IPAddress alphaAddress;
            if (!IPAddress.TryParse(alpha, out alphaAddress))
            {
                alphaAddress = null;
            }

            IPAddress betaAddress;
            if (!IPAddress.TryParse(beta, out betaAddress))
            {
                betaAddress = null;
            }

            // Act
            var result = alphaAddress.IsLessThan(betaAddress);

            // Assert
            return result;
        }

        [TestCase("192.168.1.1", "192.168.1.1", ExpectedResult = true)]
        [TestCase("192.168.1.1", "192.168.1.2", ExpectedResult = true)]
        [TestCase("192.168.1.2", "192.168.1.1", ExpectedResult = false)]
        [TestCase("a::", "a::", ExpectedResult = true)]
        [TestCase("::", "a::", ExpectedResult = true)]
        [TestCase("a::", "::", ExpectedResult = false)]
        [TestCase("0.0.0.0", "::", ExpectedResult = false)]
        [TestCase("::", "0.0.0.0", ExpectedResult = false)]
        [TestCase("", "::", ExpectedResult = false)]
        [TestCase("::", "", ExpectedResult = false)]
        [TestCase("0.0.0.0", "", ExpectedResult = false)]
        [TestCase("", "0.0.0.0", ExpectedResult = false)]
        [TestCase("", "", ExpectedResult = true)]
        public bool LessThanOrEqualToTest(string alpha,
                            string beta)
        {
            // Arrange
            IPAddress alphaAddress;
            if (!IPAddress.TryParse(alpha, out alphaAddress))
            {
                alphaAddress = null;
            }

            IPAddress betaAddress;
            if (!IPAddress.TryParse(beta, out betaAddress))
            {
                betaAddress = null;
            }

            // Act
            var result = alphaAddress.IsLessThanOrEqualTo(betaAddress);

            // Assert
            return result;
        }

        [TestCase("::", 0, ExpectedResult = "::")]
        [TestCase("::", 1, ExpectedResult = "::1")]
        [TestCase("abcd::7ac0", -2048, ExpectedResult = "abcd::72c0")]
        [TestCase("abcd::7ac0", 2048, ExpectedResult = "abcd::82c0")]
        [TestCase("abcd::7ac0", 10000, ExpectedResult = "abcd::a1d0")]
        [TestCase("abcd::7ac0", -10000, ExpectedResult = "abcd::53b0")]
        [TestCase("ffff:ffff:ffff:ffff:ffff:ffff:ffff:fffe", 1, ExpectedResult = "ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff")]
        [TestCase("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff", 0, ExpectedResult = "ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff")]
        [TestCase("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff", -1, ExpectedResult = "ffff:ffff:ffff:ffff:ffff:ffff:ffff:fffe")]
        [TestCase("192.168.1.0", 0, ExpectedResult = "192.168.1.0")]
        [TestCase("192.168.1.0", 1, ExpectedResult = "192.168.1.1")]
        [TestCase("192.168.1.0", 2, ExpectedResult = "192.168.1.2")]
        [TestCase("192.168.1.0", -1, ExpectedResult = "192.168.0.255")]
        [TestCase("192.168.1.0", 255, ExpectedResult = "192.168.1.255")]
        [TestCase("192.168.1.255", -255, ExpectedResult = "192.168.1.0")]
        [TestCase("192.168.1.0", 1024, ExpectedResult = "192.168.5.0")]
        [TestCase("192.168.1.0", -1024, ExpectedResult = "192.167.253.0")]
        [TestCase("255.255.255.254", 1, ExpectedResult = "255.255.255.255")]
        [TestCase("255.255.255.255", 0, ExpectedResult = "255.255.255.255")]
        [TestCase("255.255.255.255", -1, ExpectedResult = "255.255.255.254")]
        public string IncrementTest(string input,
                                    long amount)
        {
            // Arrange
            var address = IPAddress.Parse(input);

            // Act
            var result = address.Increment(amount);

            // Assert
            return result.ToString();
        }

        [TestCase("255.255.255.255", 1)]
        [TestCase("255.255.255.0", 1024)]
        [TestCase("0.0.0.0", -1)]
        [TestCase("0.0.0.255", -1024)]
        [TestCase("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff", 1)]
        [TestCase("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ff", 65535)]
        [TestCase("::", -1)]
        [TestCase("::FF", -1024)]
        public void IncrementOverflowUnderflowTest(string input,
                                                   long amount)
        {
            // Arrange
            var address = IPAddress.Parse(input);

            // Assert
            Assert.Throws<InvalidOperationException>(() => address.Increment(amount));
        }

        [TestCase("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff", ExpectedResult = true)]
        [TestCase("255.255.255.255", ExpectedResult = true)]
        [TestCase("::", ExpectedResult = false)]
        [TestCase("0.0.0.0", ExpectedResult = false)]
        public bool IsAtMaxTest(string input)
        {
            return IPAddress.Parse(input)
                            .IsAtMax();
        }

        [TestCase("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff", ExpectedResult = false)]
        [TestCase("255.255.255.255", ExpectedResult = false)]
        [TestCase("::", ExpectedResult = true)]
        [TestCase("0.0.0.0", ExpectedResult = true)]
        public bool IsAtMinTest(string input)
        {
            return IPAddress.Parse(input)
                            .IsAtMin();
        }

        [Test]
        public void IncrementNullInputTest()
        {
            Assert.Throws<ArgumentNullException>(() => ((IPAddress) null).Increment());
        }

        [Test]
        public void IsAtMaxNullInputTest()
        {
            Assert.Throws<ArgumentNullException>(() => ((IPAddress) null).IsAtMax());
        }

        [Test]
        public void IsAtMinNullInputTest()
        {
            Assert.Throws<ArgumentNullException>(() => ((IPAddress) null).IsAtMin());
        }

        [Test]
        public void IsBetweenNullHighTest()
        {
            Assert.Throws<ArgumentNullException>(() => IPAddress.Any.IsBetween(IPAddress.Any, null));
        }

        [Test]
        public void IsBetweenNullInputTest()
        {
            Assert.Throws<ArgumentNullException>(() => ((IPAddress) null).IsBetween(IPAddress.Any, IPAddress.Any));
        }

        [Test]
        public void IsBetweenNullLowTest()
        {
            Assert.Throws<ArgumentNullException>(() => IPAddress.Any.IsBetween(null, IPAddress.Any));
        }

        // IPv4
        [Test]
        [TestCase("10.0.0.0", "10.0.0.0", "10.0.0.255", true, ExpectedResult = true)]
        [TestCase("10.0.0.0", "10.0.0.0", "10.0.0.255", false, ExpectedResult = false)]
        [TestCase("10.0.0.255", "10.0.0.0", "10.0.0.255", true, ExpectedResult = true)]
        [TestCase("10.0.0.255", "10.0.0.0", "10.0.0.255", false, ExpectedResult = false)]
        [TestCase("10.0.0.128", "10.0.0.0", "10.0.0.255", true, ExpectedResult = true)]
        [TestCase("10.0.0.128", "10.0.0.0", "10.0.0.255", true, ExpectedResult = true)]
        [TestCase("0.0.0.0", "0.0.0.0", "255.255.255.255", true, ExpectedResult = true)]
        [TestCase("0.0.0.0", "0.0.0.0", "255.255.255.255", false, ExpectedResult = false)]
        [TestCase("255.255.255.255", "0.0.0.0", "255.255.255.255", true, ExpectedResult = true)]
        [TestCase("255.255.255.255", "0.0.0.0", "255.255.255.255", false, ExpectedResult = false)]
        [TestCase("10.0.0.128", "0.0.0.0", "255.255.255.255", true, ExpectedResult = true)]
        [TestCase("10.0.0.128", "0.0.0.0", "255.255.255.255", true, ExpectedResult = true)]

        // IPv6
        [TestCase("::", "::", "ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff", true, ExpectedResult = true)]
        [TestCase("::", "::", "ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff", false, ExpectedResult = false)]
        [TestCase("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff", "::", "ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff", true, ExpectedResult = true)]
        [TestCase("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff", "::", "ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff", false, ExpectedResult = false)]
        [TestCase("ffff:ffff:ffff:ffff::", "::", "ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff", true, ExpectedResult = true)]
        [TestCase("ffff:ffff:ffff:ffff::", "::", "ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff", true, ExpectedResult = true)]
        public bool IsBetweenTest(string input,
                                  string low,
                                  string high,
                                  bool inclusive)
        {
            return IPAddress.Parse(input)
                            .IsBetween(IPAddress.Parse(low), IPAddress.Parse(high), inclusive);
        }

        [Test]
        public void MaxMissMatchedAddressFamilyTest()
        {
            Assert.Throws<InvalidOperationException>(() => IPAddressMath.Max(IPAddress.Any, IPAddress.IPv6Any));
            Assert.Throws<InvalidOperationException>(() => IPAddressMath.Max(IPAddress.IPv6Any, IPAddress.Any));
        }

        [Test]
        public void MaxNullAlphaTest()
        {
            Assert.Throws<ArgumentNullException>(() => IPAddressMath.Max(null, IPAddress.Any));
        }

        [Test]
        public void MaxNullBetaTest()
        {
            Assert.Throws<ArgumentNullException>(() => IPAddressMath.Max(IPAddress.Any, null));
        }

        [Test]
        public void MinMissMatchedAddressFamilyTest()
        {
            Assert.Throws<InvalidOperationException>(() => IPAddressMath.Min(IPAddress.Any, IPAddress.IPv6Any));
            Assert.Throws<InvalidOperationException>(() => IPAddressMath.Min(IPAddress.IPv6Any, IPAddress.Any));
        }

        [Test]
        public void MinNullAlphaTest()
        {
            Assert.Throws<ArgumentNullException>(() => IPAddressMath.Min(null, IPAddress.Any));
        }

        [Test]
        public void MinNullBetaTest()
        {
            Assert.Throws<ArgumentNullException>(() => IPAddressMath.Min(IPAddress.Any, null));
        }

    }
}
