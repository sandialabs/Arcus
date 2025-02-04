using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using Xunit;
#if NET48   // maintained for .NET 4.8 compatability
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
#endif

namespace Arcus.Tests
{
    public class IPAddressRangeTests
    {
        #region GetHashCode

        [Theory]
        [InlineData(true, "192.168.1.5", "192.168.1.100", "192.168.1.5", "192.168.1.100")]
        [InlineData(false, "192.168.1.5", "192.168.1.100", "10.168.1.0", "10.168.1.100")]
        [InlineData(true, "::abcd", "ff:12::abcd", "::abcd", "ff:12::abcd")]
        [InlineData(false, "::abcd", "ff:12::abcd", "::ef", "ff:12::1234")]
        public void GetHashCode_Test(bool expected, string xHead, string xTail, string yHead, string yTail)
        {
            // Arrange
            _ = IPAddress.TryParse(xHead, out var xHeadAddress);
            _ = IPAddress.TryParse(xTail, out var xTailAddress);

            var xAddressRange = new IPAddressRange(xHeadAddress, xTailAddress);

            _ = IPAddress.TryParse(yHead, out var yHeadAddress);
            _ = IPAddress.TryParse(yTail, out var yTailAddress);

            var yAddressRange = new IPAddressRange(yHeadAddress, yTailAddress);

            // Act
            var xHash = xAddressRange.GetHashCode();
            var yHash = yAddressRange.GetHashCode();
            var result = xHash.Equals(yHash);

            // Assert
            Assert.Equal(expected, result);
        }

        #endregion // end: GetHashCode

        #region HeadOverlappedBy

        [Theory]
        [InlineData(false, "192.168.1.0", "255.255.255.255", null, null)]
        [InlineData(false, "192.168.1.0", "255.255.255.255", "::", "ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff:")]
        [InlineData(false, "192.168.1.0", "255.255.255.255", "192.168.1.1", "255.255.255.255")]
        [InlineData(false, "192.168.1.0", "255.255.255.255", "0.0.0.0", "192.168.0.255")]
        [InlineData(true, "192.168.1.0", "255.255.255.255", "0.0.0.0", "255.255.255.255")]
        [InlineData(true, "192.168.1.0", "255.255.255.255", "192.168.1.0", "192.168.1.0")]
        public void HeadOverlappedBy_Test(bool expected, string thisHead, string thisTail, string thatHead, string thatTail)
        {
            // Arrange
            // this
            _ = IPAddress.TryParse(thisHead, out var thisHeadAddress);
            _ = IPAddress.TryParse(thisTail, out var thisTailAddress);

            var thisAddressRange = new IPAddressRange(thisHeadAddress, thisTailAddress);

            // that
            var thatAddressRange =
                IPAddress.TryParse(thatHead, out var thatHeadAddress) && IPAddress.TryParse(thatTail, out var thatTailAddress)
                    ? new IPAddressRange(thatHeadAddress, thatTailAddress)
                    : null;

            // Act
            var result = thisAddressRange.HeadOverlappedBy(thatAddressRange);

            // Assert
            Assert.Equal(expected, result);
        }

        #endregion // end: HeadOverlappedBy

        #region Class

        [Theory]
        [InlineData(typeof(AbstractIPAddressRange))]
        [InlineData(typeof(IEquatable<IPAddressRange>))]
        [InlineData(typeof(IComparable<IPAddressRange>))]
        [InlineData(typeof(IComparable))]
        [InlineData(typeof(ISerializable))]
        public void Assignability_Test(Type assignableFromType)
        {
            // Arrange
            var type = typeof(IPAddressRange);

            // Act
            var isAssignableFrom = assignableFromType.IsAssignableFrom(type);

            // Assert
            Assert.True(isAssignableFrom);
        }

        #endregion //end: Class

        #region CompareTo / Operators

        public static IEnumerable<object[]> Comparison_Values()
        {
            var ipv4Slash16 = Subnet.Parse("192.168.0.0/16");
            var ipv6Slash64 = Subnet.Parse("ab:cd::/64");
            var ipv4Slash20 = Subnet.Parse("192.168.0.0/20");
            var ipv6Slash96 = Subnet.Parse("ab:cd::/96");
            var ipv4All = Subnet.Parse("0.0.0.0/0");
            var ipv6All = Subnet.Parse("::/0");
            var ipv4Single = Subnet.Parse("0.0.0.0/32");
            var ipv6Single = Subnet.Parse("::/128");

            yield return new object[]
            {
                0,
                new IPAddressRange(ipv4Slash16.Head, ipv4Slash16.Tail),
                new IPAddressRange(ipv4Slash16.Head, ipv4Slash16.Tail),
            };
            yield return new object[]
            {
                0,
                new IPAddressRange(ipv6Slash64.Head, ipv6Slash64.Tail),
                new IPAddressRange(ipv6Slash64.Head, ipv6Slash64.Tail),
            };
            yield return new object[] { 1, new IPAddressRange(ipv4Slash16.Head, ipv4Slash16.Tail), null };
            yield return new object[] { 1, new IPAddressRange(ipv6Slash64.Head, ipv6Slash64.Tail), null };
            yield return new object[]
            {
                1,
                new IPAddressRange(ipv4Slash16.Head, ipv4Slash16.Tail),
                new IPAddressRange(ipv4Slash20.Head, ipv4Slash20.Tail),
            };
            yield return new object[]
            {
                -1,
                new IPAddressRange(ipv4Slash20.Head, ipv4Slash20.Tail),
                new IPAddressRange(ipv4Slash16.Head, ipv4Slash16.Tail),
            };
            yield return new object[]
            {
                1,
                new IPAddressRange(ipv6Slash64.Head, ipv6Slash64.Tail),
                new IPAddressRange(ipv6Slash96.Head, ipv6Slash96.Tail),
            };
            yield return new object[]
            {
                -1,
                new IPAddressRange(ipv6Slash96.Head, ipv6Slash96.Tail),
                new IPAddressRange(ipv6Slash64.Head, ipv6Slash64.Tail),
            };
            yield return new object[]
            {
                -1,
                new IPAddressRange(ipv4All.Head, ipv4All.Tail),
                new IPAddressRange(ipv6All.Head, ipv6All.Tail),
            };
            yield return new object[]
            {
                1,
                new IPAddressRange(ipv6All.Head, ipv6All.Tail),
                new IPAddressRange(ipv4All.Head, ipv4All.Tail),
            };
            yield return new object[]
            {
                -1,
                new IPAddressRange(ipv4Single.Head, ipv4Single.Tail),
                new IPAddressRange(ipv6Single.Head, ipv6Single.Tail),
            };
            yield return new object[]
            {
                1,
                new IPAddressRange(ipv6Single.Head, ipv6Single.Tail),
                new IPAddressRange(ipv4Single.Head, ipv4Single.Tail),
            };
        }

        [Theory]
        [MemberData(nameof(Comparison_Values))]
        public void CompareTo_Test(int expected, IPAddressRange left, IPAddressRange right)
        {
            // Arrange
            // Act
            var result = left.CompareTo(right);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [MemberData(nameof(Comparison_Values))]
        public void Operator_Equals_Test(int expected, IPAddressRange left, IPAddressRange right)
        {
            // Arrange
            // Act
            var result = left == right;

            // Assert
            Assert.Equal(expected == 0, result);
        }

        [Theory]
        [MemberData(nameof(Comparison_Values))]
        public void Operator_NotEquals_Test(int expected, IPAddressRange left, IPAddressRange right)
        {
            // Arrange
            // Act
            var result = left != right;

            // Assert
            Assert.Equal(expected != 0, result);
        }

        [Theory]
        [MemberData(nameof(Comparison_Values))]
        public void Operator_GreaterThan_Test(int expected, IPAddressRange left, IPAddressRange right)
        {
            // Arrange
            // Act
            var result = left > right;

            // Assert
            Assert.Equal(expected > 0, result);
        }

        [Theory]
        [MemberData(nameof(Comparison_Values))]
        public void Operator_GreaterThanOrEqual_Test(int expected, IPAddressRange left, IPAddressRange right)
        {
            // Arrange
            // Act
            var result = left >= right;

            // Assert
            Assert.Equal(expected >= 0, result);
        }

        [Theory]
        [MemberData(nameof(Comparison_Values))]
        public void Operator_LessThan_Test(int expected, IPAddressRange left, IPAddressRange right)
        {
            // Arrange
            // Act
            var result = left < right;

            // Assert
            Assert.Equal(expected < 0, result);
        }

        [Theory]
        [MemberData(nameof(Comparison_Values))]
        public void Operator_LessThanOrEqual_Test(int expected, IPAddressRange left, IPAddressRange right)
        {
            // Arrange
            // Act
            var result = left <= right;

            // Assert
            Assert.Equal(expected <= 0, result);
        }

        #endregion end CompareTo / Operators

        #region TailOverlappedBy

        [Theory]
        [InlineData(false, "0.0.0.0", "192.168.1.0", null, null)]
        [InlineData(false, "0.0.0.0", "192.168.1.0", "::", "ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff:")]
        [InlineData(false, "0.0.0.0", "192.168.1.0", "192.168.1.1", "255.255.255.255")]
        [InlineData(false, "0.0.0.0", "192.168.1.0", "0.0.0.0", "192.168.0.255")]
        [InlineData(true, "0.0.0.0", "192.168.1.0", "0.0.0.0", "255.255.255.255")]
        [InlineData(true, "0.0.0.0", "192.168.1.0", "192.168.1.0", "192.168.1.0")]
        public void TailOverlappedBy_Test(bool expected, string thisHead, string thisTail, string thatHead, string thatTail)
        {
            // Arrange
            // this
            _ = IPAddress.TryParse(thisHead, out var thisHeadAddress);
            _ = IPAddress.TryParse(thisTail, out var thisTailAddress);

            var thisAddressRange = new IPAddressRange(thisHeadAddress, thisTailAddress);

            // that
            var thatAddressRange =
                IPAddress.TryParse(thatHead, out var thatHeadAddress) && IPAddress.TryParse(thatTail, out var thatTailAddress)
                    ? new IPAddressRange(thatHeadAddress, thatTailAddress)
                    : null;

            // Act
            var result = thisAddressRange.TailOverlappedBy(thatAddressRange);

            // Assert
            Assert.Equal(expected, result);
        }

        #endregion // end: TailOverlappedBy

        #region TryMerge

        [Theory]
        [InlineData(null, null, null, null, null)]
        [InlineData(null, "192.168.1.1", "192.168.1.9", "::", "::")]
        [InlineData(null, "192.168.1.1", "192.168.1.9", "192.168.1.11", "192.168.1.20")]
        [InlineData(null, "192.168.1.1", "192.168.1.9", null, null)]
        [InlineData(null, null, null, "192.168.1.1", "192.168.1.9")]
        [InlineData("192.168.1.1-192.168.1.20", "192.168.1.1", "192.168.1.10", "192.168.1.11", "192.168.1.20")]
        [InlineData("192.168.1.1-192.168.1.20", "192.168.1.1", "192.168.1.10", "192.168.1.10", "192.168.1.20")]
        [InlineData("192.168.1.1-192.168.1.20", "192.168.1.11", "192.168.1.20", "192.168.1.1", "192.168.1.10")]
        [InlineData("192.168.1.1-192.168.1.20", "192.168.1.10", "192.168.1.20", "192.168.1.1", "192.168.1.10")]
        [InlineData("192.168.1.10-192.168.1.20", "192.168.1.10", "192.168.1.20", "192.168.1.10", "192.168.1.20")]
        [InlineData("::-::", "::", "::", "::", "::")]
        [InlineData("::1-::4", "::1", "::2", "::3", "::4")]
        [InlineData(null, null, null, "::", "::")]
        [InlineData(null, "::", "::", null, null)]
        public void TryMergeResultTest(string expected, string alphaHead, string alphaTail, string betaHead, string betaTail)
        {
            // Arrange
            var alphaAddressRange =
                IPAddress.TryParse(alphaHead, out var alphaHeadAddress)
                && IPAddress.TryParse(alphaTail, out var alphaTailAddress)
                    ? new IPAddressRange(alphaHeadAddress, alphaTailAddress)
                    : null;

            var betaAddressRange =
                IPAddress.TryParse(betaHead, out var betaHeadAddress) && IPAddress.TryParse(betaTail, out var betaTailAddress)
                    ? new IPAddressRange(betaHeadAddress, betaTailAddress)
                    : null;

            // Act
            var successResult = IPAddressRange.TryMerge(alphaAddressRange, betaAddressRange, out var mergeResult);

            // Assert
            Assert.Equal(expected != null, successResult);

            if (expected == null)
            {
                Assert.Null(mergeResult);
            }
            else
            {
                Assert.NotNull(mergeResult);
                Assert.Equal(expected, $"{mergeResult.Head}-{mergeResult.Tail}");
            }
        }

        #endregion

        #region Ctor

        [Fact]
        public void Ctor_HeadAndTail_Specified_Test()
        {
            // Act
            var head = IPAddress.Any;
            var tail = IPAddress.Broadcast;

            // Act
            var addressRange = new IPAddressRange(head, tail);

            // Assert
            Assert.Equal(head, addressRange.Head);
            Assert.Equal(tail, addressRange.Tail);
        }

        [Fact]
        public void Ctor_SingleAddressRange_Test()
        {
            // Act
            var address = IPAddress.Any;

            // Act
            var addressRange = new IPAddressRange(address);

            // Assert
            Assert.Equal(address, addressRange.Head);
            Assert.Equal(address, addressRange.Tail);
        }

        #endregion

        #region ISerializable

        public static IEnumerable<object[]> CanSerializable_Test_Values()
        {
            yield return new object[] { new IPAddressRange(IPAddress.Parse("192.168.1.0")) };
            yield return new object[] { new IPAddressRange(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("192.168.1.255")) };
            yield return new object[] { new IPAddressRange(IPAddress.Parse("::"), IPAddress.Parse("::FFFF:4321")) };
        }

#if NET48   // maintained for .NET 4.8 compatability
        [Theory]
        [MemberData(nameof(CanSerializable_Test_Values))]
        public void CanSerializable_Test(IPAddressRange ipAddressRange)
        {
            // Arrange
#warning determine alternative formatter for compatability sake
            var formatter = new BinaryFormatter();

            // Act
            using (var writeStream = new MemoryStream())
            {
                formatter.Serialize(writeStream, ipAddressRange);

                var bytes = writeStream.ToArray();
                var readStream = new MemoryStream(bytes);
                var result = formatter.Deserialize(readStream);

                // Assert
                Assert.IsType<IPAddressRange>(result);
                Assert.Equal(ipAddressRange, result);
            }
        }
#endif
        #endregion end: ISerializable

        #region Equals

        #region Equals(IPAddressRange)

        [Theory]
        [InlineData(true, "192.168.1.1", "192.168.1.10", "192.168.1.1", "192.168.1.10")]
        [InlineData(false, "192.168.1.1", "192.168.1.5", null, null)]
        [InlineData(false, "192.168.1.1", "192.168.1.10", "192.168.1.1", "192.168.1.11")]
        [InlineData(false, "12.168.1.1", "12.168.1.10", "192.18.1.1", "192.18.1.11")]
        public void Equals_IPAddressRange_Test(bool expected, string xHead, string xTail, string yHead, string yTail)
        {
            // Arrange
            _ = IPAddress.TryParse(xHead, out var xHeadAddress);
            _ = IPAddress.TryParse(xTail, out var xTailAddress);
            var xAddressRange = new IPAddressRange(xHeadAddress, xTailAddress);

            var yAddressRange =
                IPAddress.TryParse(yHead, out var yHeadAddress) && IPAddress.TryParse(yTail, out var yTailAddress)
                    ? new IPAddressRange(yHeadAddress, yTailAddress)
                    : null;

            // Act
            var result = xAddressRange.Equals(yAddressRange);

            // Assert
            Assert.Equal(expected, result);
        }

        #endregion // end: Equals(IPAddressRange)

        #region Equals(object)

        [Theory]
        [InlineData(true, "192.168.1.1", "192.168.1.10", "192.168.1.1", "192.168.1.10")]
        [InlineData(false, "192.168.1.1", "192.168.1.5", null, null)]
        [InlineData(false, "192.168.1.1", "192.168.1.10", "192.168.1.1", "192.168.1.11")]
        [InlineData(false, "12.168.1.1", "12.168.1.10", "192.18.1.1", "192.18.1.11")]
        public void Equals_Object_Test(bool expected, string xHead, string xTail, string yHead, string yTail)
        {
            // Arrange
            _ = IPAddress.TryParse(xHead, out var xHeadAddress);
            _ = IPAddress.TryParse(xTail, out var xTailAddress);
            var xAddressRange = new IPAddressRange(xHeadAddress, xTailAddress);

            var yAddressRange =
                IPAddress.TryParse(yHead, out var yHeadAddress) && IPAddress.TryParse(yTail, out var yTailAddress)
                    ? new IPAddressRange(yHeadAddress, yTailAddress)
                    : null;

            // Act
            var result = xAddressRange.Equals((object)yAddressRange);

            // Assert
            Assert.Equal(expected, result);
        }

        #endregion // end: Equals(object)

        #endregion // end: Equals

        #region Head set

        [Fact]
        public void Head_Set_GreaterThanTail_Test()
        {
            // Arrange
            // Act
            // Assert
            Assert.Throws<InvalidOperationException>(() => new IPAddressRange(IPAddress.Broadcast, IPAddress.Any));
        }

        [Fact]
        public void Head_Set_DifferentAddressFamilyThanTail_Throw_InvalidOperationException_Test()
        {
            // Arrange
            // Act
            // Assert
            Assert.Throws<InvalidOperationException>(() => new IPAddressRange(IPAddress.IPv6Any, IPAddress.Broadcast));
        }

        #endregion

        #region Tail set

        [Fact]
        public void Tail_Set_DifferentAddressFamilyThanHead_Throw_InvalidOperationException_Test()
        {
            // Arrange
            // Act
            // Assert
            Assert.Throws<InvalidOperationException>(() => new IPAddressRange(IPAddress.Any, IPAddress.IPv6Loopback));
        }

        [Fact]
        public void Tail_Set_LessThanHead_Test()
        {
            // Arrange
            // Act
            // Assert
            Assert.Throws<InvalidOperationException>(() => new IPAddressRange(IPAddress.Broadcast, IPAddress.Any));
        }

        #endregion

        #region TryCollapseAll

        [Fact]
        public void TryCollapseAll_Consecutive_Test()
        {
            // Arrange
            var ranges = new[]
            {
                new IPAddressRange(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("192.168.1.5")),
                new IPAddressRange(IPAddress.Parse("192.168.1.6"), IPAddress.Parse("192.168.1.7")),
                new IPAddressRange(IPAddress.Parse("192.168.1.8"), IPAddress.Parse("192.168.1.20")),
            };

            // Act
            var success = IPAddressRange.TryCollapseAll(ranges, out var results);

            // Assert
            Assert.True(success);
            Assert.NotNull(results);
            var collection = results.ToList();
            Assert.Single(collection);

            var result = collection.Single();
            Assert.Equal(IPAddress.Parse("192.168.1.0"), result.Head);
            Assert.Equal(IPAddress.Parse("192.168.1.20"), result.Tail);
        }

        [Fact]
        public void TryCollapse_MismatchedAddressFamilies_Test()
        {
            // Arrange
            var ranges = new[]
            {
                new IPAddressRange(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("192.168.1.5")),
                new IPAddressRange(IPAddress.Parse("::"), IPAddress.Parse("abcd::ef00")),
            };

            // Act
            var success = IPAddressRange.TryCollapseAll(ranges, out var results);

            // Assert
            Assert.False(success);
            Assert.NotNull(results);
            Assert.False(results.Any());
        }

        [Fact]
        public void TryCollapseAll_EmptyInput_Test()
        {
            // Act
            var success = IPAddressRange.TryCollapseAll(Enumerable.Empty<IPAddressRange>(), out var results);

            // Assert
            Assert.True(success);
            Assert.NotNull(results);
            Assert.False(results.Any());
        }

        [Fact]
        public void TryCollapse_AllInvalidInput_Test()
        {
            // Arrange
            var ranges = new[]
            {
                new IPAddressRange(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("192.168.1.5")),
                null,
                new IPAddressRange(IPAddress.Parse("192.168.1.30"), IPAddress.Parse("192.168.1.35")),
            };

            // Act
            var success = IPAddressRange.TryCollapseAll(ranges, out var results);

            // Assert
            Assert.False(success);
            Assert.NotNull(results);
            Assert.False(results.Any());
        }

        [Fact]
        public void TryCollapse_AllOverlap_Test()
        {
            // Arrange
            var ranges = new[]
            {
                new IPAddressRange(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("192.168.1.5")),
                new IPAddressRange(IPAddress.Parse("192.168.1.5"), IPAddress.Parse("192.168.1.10")),
                new IPAddressRange(IPAddress.Parse("192.168.1.8"), IPAddress.Parse("192.168.1.20")),
            };

            // Act
            var success = IPAddressRange.TryCollapseAll(ranges, out var results);

            // Assert
            Assert.True(success);
            Assert.NotNull(results);
            var collection = results.ToList();
            Assert.Single(collection);

            var result = collection.Single();
            Assert.Equal(IPAddress.Parse("192.168.1.0"), result.Head);
            Assert.Equal(IPAddress.Parse("192.168.1.20"), result.Tail);
        }

        [Fact]
        public void TryCollapseAll_SubsetContainsAll_Test()
        {
            // Arrange
            var ranges = new[]
            {
                new IPAddressRange(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("192.168.1.5")),
                new IPAddressRange(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("192.168.1.20")),
                new IPAddressRange(IPAddress.Parse("192.168.1.8"), IPAddress.Parse("192.168.1.20")),
            };

            // Act
            var success = IPAddressRange.TryCollapseAll(ranges, out var results);

            // Assert
            Assert.True(success);
            Assert.NotNull(results);
            var collection = results.ToList();
            Assert.Single(collection);

            var result = collection.Single();
            Assert.Equal(IPAddress.Parse("192.168.1.0"), result.Head);
            Assert.Equal(IPAddress.Parse("192.168.1.20"), result.Tail);
        }

        [Fact]
        public void TryCollapseAll_WithGaps_Test()
        {
            // Arrange
            var ranges = new[]
            {
                new IPAddressRange(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("192.168.1.5")),
                new IPAddressRange(IPAddress.Parse("192.168.1.7"), IPAddress.Parse("192.168.1.20")),
                new IPAddressRange(IPAddress.Parse("192.168.1.30"), IPAddress.Parse("192.168.1.35")),
            };

            // Act
            var success = IPAddressRange.TryCollapseAll(ranges, out var results);

            // Assert
            Assert.True(success);
            Assert.NotNull(results);
            var enumerable = results.ToList();
            Assert.Equal(3, enumerable.Count);
            Assert.Equal(enumerable, ranges.ToList());
        }

        #endregion // end: TryCollapseAll

        #region TryExcludeAll

        [Fact]
        public void TryExcludeAll_Carve_Test()
        {
            // Arrange
            var initialRange = new IPAddressRange(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("192.168.1.200"));

            var ranges = new[]
            {
                new IPAddressRange(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("192.168.1.10")),
                new IPAddressRange(IPAddress.Parse("192.168.1.50"), IPAddress.Parse("192.168.1.100")),
                new IPAddressRange(IPAddress.Parse("192.168.1.150"), IPAddress.Parse("192.168.1.200")),
            };

            // Act
            var success = IPAddressRange.TryExcludeAll(initialRange, ranges, out var results);

            // Assert
            Assert.True(success);
            Assert.NotNull(results);
            var enumerable = results.ToList();
            Assert.Equal(2, enumerable.Count);

            Assert.Equal(
                enumerable,
                new[]
                {
                    new IPAddressRange(IPAddress.Parse("192.168.1.11"), IPAddress.Parse("192.168.1.49")),
                    new IPAddressRange(IPAddress.Parse("192.168.1.101"), IPAddress.Parse("192.168.1.149")),
                }.ToList()
            );
        }

        [Fact]
        public void TryExcludeAll_ConsecutiveCarve_Test()
        {
            // Arrange
            var initialRange = new IPAddressRange(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("192.168.1.200"));

            var ranges = new[]
            {
                new IPAddressRange(IPAddress.Parse("192.168.1.1"), IPAddress.Parse("192.168.1.100")),
                new IPAddressRange(IPAddress.Parse("192.168.1.101"), IPAddress.Parse("192.168.1.199")),
            };

            // Act
            var success = IPAddressRange.TryExcludeAll(initialRange, ranges, out var results);

            // Assert
            Assert.True(success);
            Assert.NotNull(results);
            var enumerable = results.ToList();
            Assert.Equal(2, enumerable.Count);

            Assert.Equal(
                enumerable,
                new[]
                {
                    new IPAddressRange(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("192.168.1.0")),
                    new IPAddressRange(IPAddress.Parse("192.168.1.200"), IPAddress.Parse("192.168.1.200")),
                }.ToList()
            );
        }

        [Fact]
        public void TryExcludeAll_Head_Test()
        {
            // Arrange
            var initialRange = new IPAddressRange(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("192.168.1.100"));

            var ranges = new[] { new IPAddressRange(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("192.168.1.50")) };

            // Act
            var success = IPAddressRange.TryExcludeAll(initialRange, ranges, out var results);

            // Assert
            Assert.True(success);
            Assert.NotNull(results);
            var collection = results.ToList();
            Assert.Single(collection);

            var result = collection.Single();

            Assert.Equal(IPAddress.Parse("192.168.1.51"), result.Head);
            Assert.Equal(IPAddress.Parse("192.168.1.100"), result.Tail);
        }

        [Fact]
        public void TryExcludeAll_Overlap_Test()
        {
            // Arrange
            var initialRange = new IPAddressRange(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("192.168.1.100"));

            var ranges = new[]
            {
                new IPAddressRange(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("192.168.1.49")),
                new IPAddressRange(IPAddress.Parse("192.168.1.50"), IPAddress.Parse("192.168.1.75")),
                new IPAddressRange(IPAddress.Parse("192.168.1.75"), IPAddress.Parse("192.168.1.100")),
            };

            // Act
            var success = IPAddressRange.TryExcludeAll(initialRange, ranges, out var results);

            //Assert
            Assert.True(success);
            Assert.Empty(results);
        }

        [Fact]
        public void TryExcludeAll_Tail_Test()
        {
            // Arrange
            var initialRange = new IPAddressRange(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("192.168.1.100"));

            var ranges = new[] { new IPAddressRange(IPAddress.Parse("192.168.1.50"), IPAddress.Parse("192.168.1.100")) };

            // Act
            var success = IPAddressRange.TryExcludeAll(initialRange, ranges, out var results);

            // Assert
            Assert.True(success);
            Assert.NotNull(results);
            var collection = results.ToList();
            Assert.Single(collection);

            var result = collection.Single();

            Assert.Equal(IPAddress.Parse("192.168.1.0"), result.Head);
            Assert.Equal(IPAddress.Parse("192.168.1.49"), result.Tail);
        }

        [Fact]
        public void TryExcludeAll_NoExclusions_Test()
        {
            // Arrange
            var initialRange = new IPAddressRange(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("192.168.1.200"));

            // Act
            var success = IPAddressRange.TryExcludeAll(initialRange, Enumerable.Empty<IPAddressRange>(), out var results);

            // Assert
            Assert.True(success);
            Assert.NotNull(results);
            var collection = results.ToList();
            Assert.Single(collection);

            Assert.Equal(initialRange, collection.Single());
        }

        [Fact]
        public void TryExcludeAll_InitialMissMatchedAddressFamily_Test()
        {
            // Arrange
            var initialRange = new IPAddressRange(IPAddress.Parse("::"), IPAddress.Parse("ffff::ffff"));

            var ranges = new[] { new IPAddressRange(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("192.168.1.10")) };

            // Act
            var success = IPAddressRange.TryExcludeAll(initialRange, ranges, out var results);

            // Assert
            Assert.False(success);
            Assert.Empty(results);
        }

        #endregion // end: TryExcludeAll

        #region Formatting

        #region ToString

        [Theory]
        [InlineData("192.168.1.1", "192.168.1.42")]
        [InlineData("::beef", "0123::dead")]
        public void ToString_MatchesGeneralFormat_Test(string headString, string tailString)
        {
            // Arrange
            var head = IPAddress.Parse(headString);
            var tail = IPAddress.Parse(tailString);

            var iPAddressRange = new IPAddressRange(head, tail);

            // Act
            var result = iPAddressRange.ToString();

            // Assert
            Assert.Equal($"{iPAddressRange:G}", result);
        }

        #endregion // end: ToString

        #region ToString(string, IFormatProvider)

        public static IEnumerable<object[]> ToString_Format_Test_Values()
        {
            // general formats
            foreach (var format in new[] { null, string.Empty, "g", "G" })
            {
                foreach (var ipAddressRange in Ipv4AddressRanges().Concat(Ipv6AddressRanges()))
                {
                    yield return new object[]
                    {
                        $"{ipAddressRange.Head} - {ipAddressRange.Tail}",
                        format,
                        CultureInfo.CurrentCulture,
                        ipAddressRange,
                    };
                }
            }

            IEnumerable<IPAddressRange> Ipv4AddressRanges()
            {
                yield return new IPAddressRange(IPAddress.Parse("192.168.1.1"), IPAddress.Parse("192.168.1.42"));
                yield return new IPAddressRange(IPAddress.Parse("192.168.1.1"), IPAddress.Parse("192.168.1.1"));
            }

            IEnumerable<IPAddressRange> Ipv6AddressRanges()
            {
                yield return new IPAddressRange(IPAddress.Parse("::beef"), IPAddress.Parse("0123::dead"));
                yield return new IPAddressRange(IPAddress.Parse("::beef"), IPAddress.Parse("::beef"));
            }
        }

        [Theory]
        [MemberData(nameof(ToString_Format_Test_Values))]
        public void ToString_Format_Test(
            string expected,
            string format,
            IFormatProvider formatProvider,
            IPAddressRange ipAddressRange
        )
        {
            // Arrange
            // Act
            var result = ipAddressRange.ToString(format, formatProvider);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ToString_UnknownFormat_Throws_FormatException_Test()
        {
            // Arrange
            var range = new IPAddressRange(IPAddress.Parse("192.168.1.1"), IPAddress.Parse("192.168.1.42"));

            // Act
            // Assert
            Assert.Throws<FormatException>(() => range.ToString("potato", CultureInfo.CurrentCulture));
        }

        #endregion // end: ToString(string, IFormatProvider)

        #endregion // end: Formatting
    }
}
