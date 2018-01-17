using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using NUnit.Framework;

namespace Arcus.Tests
{
    [TestFixture]
    public class IPAddressRangeTests
    {
        [TestCase("", "", "", "", ExpectedResult = true)]
        [TestCase("192.168.1.1", "192.168.1.10", "192.168.1.1", "192.168.1.10", ExpectedResult = true)]
        [TestCase("", "192.168.1.1", "", "192.168.1.1", ExpectedResult = true)]
        [TestCase("192.168.1.1", "", "192.168.1.1", "", ExpectedResult = true)]
        [TestCase("192.168.1.1", "192.168.1.5", "192.168.1.1", "", ExpectedResult = false)]
        [TestCase("192.168.1.1", "", "192.168.1.1", "192.168.1.5", ExpectedResult = false)]
        [TestCase("192.168.1.1", "192.168.1.10", "192.168.1.1", "192.168.1.11", ExpectedResult = false)]
        [TestCase("12.168.1.1", "12.168.1.10", "192.18.1.1", "192.18.1.11", ExpectedResult = false)]
        public bool EqualsExplicitTest(string xHead,
                                       string xTail,
                                       string yHead,
                                       string yTail)
        {
            // Arrange
            IPAddress xHeadAddress;
            if (!IPAddress.TryParse(xHead, out xHeadAddress))
            {
                xHeadAddress = null;
            }

            IPAddress xTailAddress;
            if (!IPAddress.TryParse(xTail, out xTailAddress))
            {
                xTailAddress = null;
            }

            IPAddress yHeadAddress;
            if (!IPAddress.TryParse(yHead, out yHeadAddress))
            {
                yHeadAddress = null;
            }

            IPAddress yTailAddress;
            if (!IPAddress.TryParse(yTail, out yTailAddress))
            {
                yTailAddress = null;
            }

            var xAddressRange = new IPAddressRange
                                {
                                    Head = xHeadAddress,
                                    Tail = xTailAddress
                                };

            var yAddressRange = new IPAddressRange
                                {
                                    Head = yHeadAddress,
                                    Tail = yTailAddress
                                };

            // Act
            var result = xAddressRange.Equals(yAddressRange);

            // Assert
            return result;
        }

        [TestCase("", "", "", "", ExpectedResult = true)]
        [TestCase("192.168.1.1", "192.168.1.10", "192.168.1.1", "192.168.1.10", ExpectedResult = true)]
        [TestCase("", "192.168.1.1", "", "192.168.1.1", ExpectedResult = true)]
        [TestCase("192.168.1.1", "", "192.168.1.1", "", ExpectedResult = true)]
        [TestCase("192.168.1.1", "192.168.1.5", "192.168.1.1", "", ExpectedResult = false)]
        [TestCase("192.168.1.1", "", "192.168.1.1", "192.168.1.5", ExpectedResult = false)]
        [TestCase("192.168.1.1", "192.168.1.10", "192.168.1.1", "192.168.1.11", ExpectedResult = false)]
        [TestCase("12.168.1.1", "12.168.1.10", "192.18.1.1", "192.18.1.11", ExpectedResult = false)]
        public bool EqualsTest(string xHead,
                               string xTail,
                               string yHead,
                               string yTail)
        {
            // Arrange
            IPAddress xHeadAddress;
            if (!IPAddress.TryParse(xHead, out xHeadAddress))
            {
                xHeadAddress = null;
            }

            IPAddress xTailAddress;
            if (!IPAddress.TryParse(xTail, out xTailAddress))
            {
                xTailAddress = null;
            }

            IPAddress yHeadAddress;
            if (!IPAddress.TryParse(yHead, out yHeadAddress))
            {
                yHeadAddress = null;
            }

            IPAddress yTailAddress;
            if (!IPAddress.TryParse(yTail, out yTailAddress))
            {
                yTailAddress = null;
            }

            var xAddressRange = new IPAddressRange
                                {
                                    Head = xHeadAddress,
                                    Tail = xTailAddress
                                };

            var yAddressRange = new IPAddressRange
                                {
                                    Head = yHeadAddress,
                                    Tail = yTailAddress
                                };

            // Act
            var result = xAddressRange.Equals((object) yAddressRange);

            // Assert
            return result;
        }

        [TestCase("", "", "", "", ExpectedResult = false)]
        [TestCase("", "255.255.255.255", "0.0.0.0", "255.255.255.255", ExpectedResult = false)]
        [TestCase("192.168.1.0", "255.255.255.255", "", "", ExpectedResult = false)]
        [TestCase("192.168.1.0", "255.255.255.255", "::", "ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff:", ExpectedResult = false)]
        [TestCase("192.168.1.0", "255.255.255.255", "192.168.1.1", "255.255.255.255", ExpectedResult = false)]
        [TestCase("192.168.1.0", "255.255.255.255", "0.0.0.0", "192.168.0.255", ExpectedResult = false)]
        [TestCase("192.168.1.0", "255.255.255.255", "0.0.0.0", "255.255.255.255", ExpectedResult = true)]
        [TestCase("192.168.1.0", "255.255.255.255", "192.168.1.0", "192.168.1.0", ExpectedResult = true)]
        [TestCase("192.168.1.0", "255.255.255.255", "192.168.1.0", "", ExpectedResult = true)]
        [TestCase("192.168.1.0", "255.255.255.255", "", "192.168.1.0", ExpectedResult = true)]
        public bool HeadOverlappedByTest(string thisHead,
                                         string thisTail,
                                         string thatHead,
                                         string thatTail)
        {
            // Arrange
            // this
            IPAddress thisHeadAddress;
            if (!IPAddress.TryParse(thisHead, out thisHeadAddress))
            {
                thisHeadAddress = null;
            }

            IPAddress thisTailAddress;
            if (!IPAddress.TryParse(thisTail, out thisTailAddress))
            {
                thisTailAddress = null;
            }

            var thisAddressRange = new IPAddressRange
                                   {
                                       Head = thisHeadAddress,
                                       Tail = thisTailAddress
                                   };

            // that
            IPAddress thatHeadAddress;
            if (!IPAddress.TryParse(thatHead, out thatHeadAddress))
            {
                thatHeadAddress = null;
            }

            IPAddress thatTailAddress;
            if (!IPAddress.TryParse(thatTail, out thatTailAddress))
            {
                thatTailAddress = null;
            }

            var thatAddressRange = new IPAddressRange
                                   {
                                       Head = thatHeadAddress,
                                       Tail = thatTailAddress
                                   };

            // Act
            var result = thisAddressRange.HeadOverlappedBy(thatAddressRange);

            // Assert 
            return result;
        }

        [TestCase("", "", "", "", ExpectedResult = false)]
        [TestCase("255.255.255.255", "", "0.0.0.0", "255.255.255.255", ExpectedResult = false)]
        [TestCase("0.0.0.0", "192.168.1.0", "", "", ExpectedResult = false)]
        [TestCase("0.0.0.0", "192.168.1.0", "::", "ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff:", ExpectedResult = false)]
        [TestCase("0.0.0.0", "192.168.1.0", "192.168.1.1", "255.255.255.255", ExpectedResult = false)]
        [TestCase("0.0.0.0", "192.168.1.0", "0.0.0.0", "192.168.0.255", ExpectedResult = false)]
        [TestCase("0.0.0.0", "192.168.1.0", "0.0.0.0", "255.255.255.255", ExpectedResult = true)]
        [TestCase("0.0.0.0", "192.168.1.0", "192.168.1.0", "192.168.1.0", ExpectedResult = true)]
        [TestCase("0.0.0.0", "192.168.1.0", "192.168.1.0", "", ExpectedResult = true)]
        [TestCase("0.0.0.0", "192.168.1.0", "", "192.168.1.0", ExpectedResult = true)]
        public bool TailOverlappedByTest(string thisHead,
                                         string thisTail,
                                         string thatHead,
                                         string thatTail)
        {
            // Arrange
            // this
            IPAddress thisHeadAddress;
            if (!IPAddress.TryParse(thisHead, out thisHeadAddress))
            {
                thisHeadAddress = null;
            }

            IPAddress thisTailAddress;
            if (!IPAddress.TryParse(thisTail, out thisTailAddress))
            {
                thisTailAddress = null;
            }

            var thisAddressRange = new IPAddressRange
                                   {
                                       Head = thisHeadAddress,
                                       Tail = thisTailAddress
                                   };

            // that
            IPAddress thatHeadAddress;
            if (!IPAddress.TryParse(thatHead, out thatHeadAddress))
            {
                thatHeadAddress = null;
            }

            IPAddress thatTailAddress;
            if (!IPAddress.TryParse(thatTail, out thatTailAddress))
            {
                thatTailAddress = null;
            }

            var thatAddressRange = new IPAddressRange
                                   {
                                       Head = thatHeadAddress,
                                       Tail = thatTailAddress
                                   };

            // Act
            var result = thisAddressRange.TailOverlappedBy(thatAddressRange);

            // Assert 
            return result;
        }

        [TestCase("", "", "", "", ExpectedResult = true)]
        [TestCase("192.168.1.1", "", "", "", ExpectedResult = false)]
        [TestCase("", "192.168.1.1", "", "", ExpectedResult = false)]
        [TestCase("192.168.1.1", "", "192.168.1.1", "", ExpectedResult = true)]
        [TestCase("", "192.168.1.1", "", "192.168.1.1", ExpectedResult = true)]
        [TestCase("192.168.1.1", "192.168.1.1", "192.168.1.1", "", ExpectedResult = false)]
        [TestCase("192.168.1.1", "192.168.1.1", "", "192.168.1.1", ExpectedResult = false)]
        [TestCase("192.168.1.5", "192.168.1.100", "192.168.1.5", "192.168.1.100", ExpectedResult = true)]
        public bool GetHashCodeTest(string xHead,
                                    string xTail,
                                    string yHead,
                                    string yTail)
        {
            // Arrange
            IPAddress xHeadAddress;
            if (!IPAddress.TryParse(xHead, out xHeadAddress))
            {
                xHeadAddress = null;
            }

            IPAddress xTailAddress;
            if (!IPAddress.TryParse(xTail, out xTailAddress))
            {
                xTailAddress = null;
            }

            IPAddress yHeadAddress;
            if (!IPAddress.TryParse(yHead, out yHeadAddress))
            {
                yHeadAddress = null;
            }

            IPAddress yTailAddress;
            if (!IPAddress.TryParse(yTail, out yTailAddress))
            {
                yTailAddress = null;
            }

            var xAddressRange = new IPAddressRange
                                {
                                    Head = xHeadAddress,
                                    Tail = xTailAddress
                                };

            var yAddressRange = new IPAddressRange
                                {
                                    Head = yHeadAddress,
                                    Tail = yTailAddress
                                };

            // Act
            var xHash = xAddressRange.GetHashCode();
            var yHash = yAddressRange.GetHashCode();

            // Assert
            return xHash.Equals(yHash);
        }

        [TestCase("", "", "", "", ExpectedResult = false)]
        [TestCase("192.168.1.1", "", "", "", ExpectedResult = false)]
        [TestCase("", "192.168.1.1", "", "", ExpectedResult = false)]
        [TestCase("", "", "192.168.1.1", "", ExpectedResult = false)]
        [TestCase("", "", "", "192.168.1.1", ExpectedResult = false)]
        [TestCase("::", "", "", "", ExpectedResult = false)]
        [TestCase("", "::", "", "", ExpectedResult = false)]
        [TestCase("", "", "::", "", ExpectedResult = false)]
        [TestCase("", "", "", "::", ExpectedResult = false)]
        [TestCase("192.168.1.1", "192.168.1.9", "192.168.1.11", "192.168.1.20", ExpectedResult = false)]
        [TestCase("192.168.1.1", "192.168.1.10", "192.168.1.11", "192.168.1.20", ExpectedResult = true)]
        [TestCase("192.168.1.1", "192.168.1.10", "192.168.1.10", "192.168.1.20", ExpectedResult = true)]
        [TestCase("192.168.1.11", "192.168.1.20", "192.168.1.1", "192.168.1.10", ExpectedResult = true)]
        [TestCase("192.168.1.10", "192.168.1.20", "192.168.1.1", "192.168.1.10", ExpectedResult = true)]
        [TestCase("192.168.1.10", "192.168.1.20", "192.168.1.10", "192.168.1.20", ExpectedResult = true)]
        [TestCase("::", "::", "::", "::", ExpectedResult = true)]
        [TestCase("::1", "::2", "::3", "::4", ExpectedResult = true)]
        public bool TryMergeSuccessTest(string alphaHead,
                                        string alphaTail,
                                        string betaHead,
                                        string betaTail)
        {
            // Arrange
            IPAddress alphaHeadAddress;
            if (!IPAddress.TryParse(alphaHead, out alphaHeadAddress))
            {
                alphaHeadAddress = null;
            }

            IPAddress alphaTailAddress;
            if (!IPAddress.TryParse(alphaTail, out alphaTailAddress))
            {
                alphaTailAddress = null;
            }

            var alphaAddressRange = new IPAddressRange(alphaHeadAddress, alphaTailAddress);

            // Arrange
            IPAddress betaHeadAddress;
            if (!IPAddress.TryParse(betaHead, out betaHeadAddress))
            {
                betaHeadAddress = null;
            }

            IPAddress betaTailAddress;
            if (!IPAddress.TryParse(betaTail, out betaTailAddress))
            {
                betaTailAddress = null;
            }

            var betaAddressRange = new IPAddressRange(betaHeadAddress, betaTailAddress);

            // Act
            IPAddressRange mergeResult;
            var success = IPAddressRange.TryMerge(alphaAddressRange, betaAddressRange, out mergeResult);

            // Assert
            return success;
        }

        [TestCase("", "", "", "", ExpectedResult = null)]
        [TestCase("192.168.1.1", "", "", "", ExpectedResult = null)]
        [TestCase("", "192.168.1.1", "", "", ExpectedResult = null)]
        [TestCase("", "", "192.168.1.1", "", ExpectedResult = null)]
        [TestCase("", "", "", "192.168.1.1", ExpectedResult = null)]
        [TestCase("::", "", "", "", ExpectedResult = null)]
        [TestCase("", "::", "", "", ExpectedResult = null)]
        [TestCase("", "", "::", "", ExpectedResult = null)]
        [TestCase("", "", "", "::", ExpectedResult = null)]
        [TestCase("192.168.1.1", "192.168.1.9", "192.168.1.11", "192.168.1.20", ExpectedResult = null)]
        [TestCase("192.168.1.1", "192.168.1.10", "192.168.1.11", "192.168.1.20", ExpectedResult = "192.168.1.1-192.168.1.20")]
        [TestCase("192.168.1.1", "192.168.1.10", "192.168.1.10", "192.168.1.20", ExpectedResult = "192.168.1.1-192.168.1.20")]
        [TestCase("192.168.1.11", "192.168.1.20", "192.168.1.1", "192.168.1.10", ExpectedResult = "192.168.1.1-192.168.1.20")]
        [TestCase("192.168.1.10", "192.168.1.20", "192.168.1.1", "192.168.1.10", ExpectedResult = "192.168.1.1-192.168.1.20")]
        [TestCase("192.168.1.10", "192.168.1.20", "192.168.1.10", "192.168.1.20", ExpectedResult = "192.168.1.10-192.168.1.20")]
        [TestCase("::", "::", "::", "::", ExpectedResult = "::-::")]
        [TestCase("::1", "::2", "::3", "::4", ExpectedResult = "::1-::4")]
        public string TryMergeResultTest(string alphaHead,
                                         string alphaTail,
                                         string betaHead,
                                         string betaTail)
        {
            // Arrange
            IPAddress alphaHeadAddress;
            if (!IPAddress.TryParse(alphaHead, out alphaHeadAddress))
            {
                alphaHeadAddress = null;
            }

            IPAddress alphaTailAddress;
            if (!IPAddress.TryParse(alphaTail, out alphaTailAddress))
            {
                alphaTailAddress = null;
            }

            var alphaAddressRange = new IPAddressRange(alphaHeadAddress, alphaTailAddress);

            // Arrange
            IPAddress betaHeadAddress;
            if (!IPAddress.TryParse(betaHead, out betaHeadAddress))
            {
                betaHeadAddress = null;
            }

            IPAddress betaTailAddress;
            if (!IPAddress.TryParse(betaTail, out betaTailAddress))
            {
                betaTailAddress = null;
            }

            var betaAddressRange = new IPAddressRange(betaHeadAddress, betaTailAddress);

            // Act
            IPAddressRange mergeResult;
            IPAddressRange.TryMerge(alphaAddressRange, betaAddressRange, out mergeResult);

            // Assert
            return mergeResult == null
                       ? null
                       : string.Format("{0}-{1}", mergeResult.Head, mergeResult.Tail);
        }

        [Test]
        public void ConstructEmptyTest()
        {
            // Act
            var addressRange = new IPAddressRange();

            // Assert
            Assert.IsNull(addressRange.Head);
            Assert.IsNull(addressRange.Tail);
        }

        [Test]
        public void ConstructHeadAndTailTest()
        {
            // Act
            var head = IPAddress.Any;
            var tail = IPAddress.Broadcast;

            var addressRange = new IPAddressRange(head, tail);

            // Assert
            Assert.AreEqual(head, addressRange.Head);
            Assert.AreEqual(tail, addressRange.Tail);
        }

        [Test]
        public void ConstructSingleIPAddressTest()
        {
            // Act
            var address = IPAddress.Any;

            var addressRange = new IPAddressRange(address);

            // Assert
            Assert.AreEqual(address, addressRange.Head);
            Assert.AreEqual(address, addressRange.Tail);
        }

        [Test]
        public void ImplementationTest()
        {
            Assert.That(typeof (IIPAddressRange).IsAssignableFrom(typeof (IPAddressRange)));
            Assert.That(typeof (IComparable<IPAddressRange>).IsAssignableFrom(typeof (IPAddressRange)));
            Assert.That(typeof (IEquatable<IPAddressRange>).IsAssignableFrom(typeof (IPAddressRange)));
        }

        [Test]
        public void SetHeadGtTailTest()
        {
            // Arrange
            var range = new IPAddressRange();

            // Act
            range.Tail = IPAddress.Any;

            // Assert
            Assert.Throws<InvalidOperationException>(() => range.Head = IPAddress.Broadcast);
        }

        [Test]
        public void SetHeadWhenTailIsNullTest()
        {
            // Arrange
            var head = IPAddress.Any;
            var range = new IPAddressRange(null, null);

            // Act
            range.Head = head;

            // Assert
            Assert.AreEqual(head, range.Head);
            Assert.IsNull(range.Tail);
        }

        [Test]
        public void SetTailDifferentAddressFamilyThanHeadTest()
        {
            // Arrange
            var range = new IPAddressRange(IPAddress.Any, null);

            // Assert
            Assert.Throws<InvalidOperationException>(() => range.Tail = IPAddress.IPv6Loopback);
        }

        [Test]
        public void SetTailLtHeadTest()
        {
            // Arrange
            var range = new IPAddressRange();

            // Act
            range.Head = IPAddress.Broadcast;

            // Assert
            Assert.Throws<InvalidOperationException>(() => range.Tail = IPAddress.Any);
        }

        [Test]
        public void SetTailWhenHeadIsNullTest()
        {
            // Arrange
            var tail = IPAddress.Broadcast;

            var range = new IPAddressRange(null, null);

            // Act
            range.Tail = tail;

            // Assert
            Assert.AreEqual(tail, range.Tail);
            Assert.IsNull(range.Head);
        }

        [Test]
        public void ThrowInvalidOperationExceptionOnAssignHeadWithDifferentAddressFamilyThanTailTest()
        {
            // Arrange
            var range = new IPAddressRange(null, IPAddress.Broadcast);

            // Assert
            Assert.Throws<InvalidOperationException>(() => range.Head = IPAddress.IPv6Any);
        }

        [Test]
        public void TryCollapseAllConsecutiveTest()
        {
            var ranges = new[]
                         {
                             new IPAddressRange(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("192.168.1.5")),
                             new IPAddressRange(IPAddress.Parse("192.168.1.6"), IPAddress.Parse("192.168.1.7")),
                             new IPAddressRange(IPAddress.Parse("192.168.1.8"), IPAddress.Parse("192.168.1.20"))
                         };

            IList<IPAddressRange> results;
            var success = IPAddressRange.TryCollapseAll(ranges, out results);

            Assert.IsTrue(success);
            Assert.AreEqual(1, results.Count);

            var result = results.First();

            Assert.AreEqual(IPAddress.Parse("192.168.1.0"), result.Head);
            Assert.AreEqual(IPAddress.Parse("192.168.1.20"), result.Tail);
        }

        [Test]
        public void TryCollapseAllDifferentAddressFamiliesTest()
        {
            // Arrange
            var ranges = new[]
                         {
                             new IPAddressRange(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("192.168.1.5")),
                             new IPAddressRange(IPAddress.Parse("::"), IPAddress.Parse("abcd::ef00")),
                             new IPAddressRange(IPAddress.Parse("192.168.1.30"), IPAddress.Parse("192.168.1.35"))
                         };

            // Act
            IList<IPAddressRange> results;
            var success = IPAddressRange.TryCollapseAll(ranges, out results);

            // Assert
            Assert.IsFalse(success);
            Assert.IsFalse(results.Any());
        }

        [Test]
        public void TryCollapseAllEmptyInputTest()
        {
            // Act
            IList<IPAddressRange> results;
            var success = IPAddressRange.TryCollapseAll(Enumerable.Empty<IPAddressRange>(), out results);

            // Assert
            Assert.IsTrue(success);
            Assert.IsFalse(results.Any());
        }

        [Test]
        public void TryCollapseAllInvalidInputTest()
        {
            // Arrange
            var ranges = new[]
                         {
                             new IPAddressRange(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("192.168.1.5")),
                             new IPAddressRange(IPAddress.Parse("192.168.1.7"), null),
                             new IPAddressRange(IPAddress.Parse("192.168.1.30"), IPAddress.Parse("192.168.1.35"))
                         };

            // Act
            IList<IPAddressRange> results;
            var success = IPAddressRange.TryCollapseAll(ranges, out results);

            // Assert
            Assert.IsFalse(success);
            Assert.IsFalse(results.Any());
        }

        [Test]
        public void TryCollapseAllOverlapTest()
        {
            var ranges = new[]
                         {
                             new IPAddressRange(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("192.168.1.5")),
                             new IPAddressRange(IPAddress.Parse("192.168.1.5"), IPAddress.Parse("192.168.1.10")),
                             new IPAddressRange(IPAddress.Parse("192.168.1.8"), IPAddress.Parse("192.168.1.20"))
                         };

            IList<IPAddressRange> results;
            var success = IPAddressRange.TryCollapseAll(ranges, out results);

            Assert.IsTrue(success);
            Assert.AreEqual(1, results.Count);

            var result = results.First();

            Assert.AreEqual(IPAddress.Parse("192.168.1.0"), result.Head);
            Assert.AreEqual(IPAddress.Parse("192.168.1.20"), result.Tail);
        }

        [Test]
        public void TryCollapseAllSubsetContainsAllTest()
        {
            var ranges = new[]
                         {
                             new IPAddressRange(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("192.168.1.5")),
                             new IPAddressRange(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("192.168.1.20")),
                             new IPAddressRange(IPAddress.Parse("192.168.1.8"), IPAddress.Parse("192.168.1.20"))
                         };

            IList<IPAddressRange> results;
            var success = IPAddressRange.TryCollapseAll(ranges, out results);

            Assert.IsTrue(success);
            Assert.AreEqual(1, results.Count);

            var result = results.First();

            Assert.AreEqual(IPAddress.Parse("192.168.1.0"), result.Head);
            Assert.AreEqual(IPAddress.Parse("192.168.1.20"), result.Tail);
        }

        [Test]
        public void TryCollapseAllWithGapsTest()
        {
            var ranges = new[]
                         {
                             new IPAddressRange(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("192.168.1.5")),
                             new IPAddressRange(IPAddress.Parse("192.168.1.7"), IPAddress.Parse("192.168.1.20")),
                             new IPAddressRange(IPAddress.Parse("192.168.1.30"), IPAddress.Parse("192.168.1.35"))
                         };

            IList<IPAddressRange> results;
            var success = IPAddressRange.TryCollapseAll(ranges, out results);

            Assert.IsTrue(success);
            Assert.AreEqual(3, results.Count);
            CollectionAssert.AreEquivalent(results, ranges.ToList());
        }

        [Test]
        public void TryExcludeAllCarveTest()
        {
            var initialRange = new IPAddressRange(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("192.168.1.200"));

            var ranges = new[]
                         {
                             new IPAddressRange(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("192.168.1.10")),
                             new IPAddressRange(IPAddress.Parse("192.168.1.50"), IPAddress.Parse("192.168.1.100")),
                             new IPAddressRange(IPAddress.Parse("192.168.1.150"), IPAddress.Parse("192.168.1.200"))
                         };

            IList<IPAddressRange> results;
            var success = IPAddressRange.TryExcludeAll(initialRange, ranges, out results);

            Assert.IsTrue(success);
            Assert.AreEqual(2, results.Count);

            CollectionAssert.AreEquivalent(results, new[]
                                                    {
                                                        new IPAddressRange(IPAddress.Parse("192.168.1.11"), IPAddress.Parse("192.168.1.49")),
                                                        new IPAddressRange(IPAddress.Parse("192.168.1.101"), IPAddress.Parse("192.168.1.149"))
                                                    }.ToList());
        }

        [Test]
        public void TryExcludeAllConsecutiveCarveTest()
        {
            var initialRange = new IPAddressRange(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("192.168.1.200"));

            var ranges = new[]
                         {
                             new IPAddressRange(IPAddress.Parse("192.168.1.1"), IPAddress.Parse("192.168.1.100")),
                             new IPAddressRange(IPAddress.Parse("192.168.1.101"), IPAddress.Parse("192.168.1.199"))
                         };

            IList<IPAddressRange> results;
            var success = IPAddressRange.TryExcludeAll(initialRange, ranges, out results);

            Assert.IsTrue(success);
            Assert.AreEqual(2, results.Count);

            CollectionAssert.AreEquivalent(results, new[]
                                                    {
                                                        new IPAddressRange(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("192.168.1.0")),
                                                        new IPAddressRange(IPAddress.Parse("192.168.1.200"), IPAddress.Parse("192.168.1.200"))
                                                    }.ToList());
        }

        [Test]
        public void TryExcludeAllHeadTest()
        {
            var initialRange = new IPAddressRange(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("192.168.1.100"));

            var ranges = new[]
                         {
                             new IPAddressRange(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("192.168.1.50"))
                         };

            IList<IPAddressRange> results;
            var success = IPAddressRange.TryExcludeAll(initialRange, ranges, out results);

            Assert.IsTrue(success);
            Assert.AreEqual(1, results.Count);

            var result = results.First();

            Assert.AreEqual(IPAddress.Parse("192.168.1.51"), result.Head);
            Assert.AreEqual(IPAddress.Parse("192.168.1.100"), result.Tail);
        }

        [Test]
        public void TryExcludeAllOverlapTest()
        {
            var initialRange = new IPAddressRange(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("192.168.1.100"));

            var ranges = new[]
                         {
                             new IPAddressRange(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("192.168.1.49")),
                             new IPAddressRange(IPAddress.Parse("192.168.1.50"), IPAddress.Parse("192.168.1.75")),
                             new IPAddressRange(IPAddress.Parse("192.168.1.75"), IPAddress.Parse("192.168.1.100"))
                         };

            IList<IPAddressRange> results;
            var success = IPAddressRange.TryExcludeAll(initialRange, ranges, out results);

            Assert.IsTrue(success);
            Assert.AreEqual(0, results.Count);
        }

        [Test]
        public void TryExcludeAllTailTest()
        {
            var initialRange = new IPAddressRange(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("192.168.1.100"));

            var ranges = new[]
                         {
                             new IPAddressRange(IPAddress.Parse("192.168.1.50"), IPAddress.Parse("192.168.1.100"))
                         };

            IList<IPAddressRange> results;
            var success = IPAddressRange.TryExcludeAll(initialRange, ranges, out results);

            Assert.IsTrue(success);
            Assert.AreEqual(1, results.Count);

            var result = results.First();

            Assert.AreEqual(IPAddress.Parse("192.168.1.0"), result.Head);
            Assert.AreEqual(IPAddress.Parse("192.168.1.49"), result.Tail);
        }

        [Test]
        public void AddressesReturnsOneAddress()
        {
            var address = "192.168.1.1";
            var oneAddressRange = new IPAddressRange(IPAddress.Parse(address), IPAddress.Parse(address));

            var addresses = oneAddressRange.Addresses.ToArray();

            Assert.AreEqual(1, addresses.Length);
            Assert.AreEqual(address, addresses.First().ToString());
        }

        public void TryExcludeInitialMissMatchAddressFamilyCarveTest()
        {
            var initialRange = new IPAddressRange(IPAddress.Parse("::"), IPAddress.Parse("ffff::ffff"));

            var ranges = new[]
                         {
                             new IPAddressRange(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("192.168.1.10"))
                         };

            IList<IPAddressRange> results;
            var success = IPAddressRange.TryExcludeAll(initialRange, ranges, out results);

            Assert.IsFalse(success);
            Assert.IsFalse(results.Any());
        }

        [Test]
        public void TryExcludeNoExclusionsTest()
        {
            var initialRange = new IPAddressRange(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("192.168.1.200"));

            IList<IPAddressRange> results;
            var success = IPAddressRange.TryExcludeAll(initialRange, Enumerable.Empty<IPAddressRange>(), out results);

            Assert.IsTrue(success);
            Assert.AreEqual(1, results.Count);

            CollectionAssert.AreEquivalent(results, new[]
                                                    {
                                                        initialRange
                                                    }.ToList());
        }
    }
}
