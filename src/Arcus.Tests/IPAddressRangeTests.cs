using System.Globalization;
using System.Net;
using Arcus.Tests.XunitSerializers;
#if NET48
using System.Runtime.Serialization;
#endif

[assembly: RegisterXunitSerializer(typeof(IPAddressRangeXunitSerializer), typeof(IPAddressRange))]

namespace Arcus.Tests
{
    /// <summary>Unit tests for <see cref="IPAddressRange"/>.</summary>
    public partial class IPAddressRangeTests
    {
        #region HeadOverlappedBy

        /// <summary>Verifies HeadOverlappedBy returns the expected result for an IPAddressRange against various ranges.</summary>
        /// <param name="expected">Expected result.</param>
        /// <param name="thisHead">The head address of the range under test.</param>
        /// <param name="thisTail">The tail address of the range under test.</param>
        /// <param name="thatHead">The head address of the other range.</param>
        /// <param name="thatTail">The tail address of the other range.</param>
        [Theory]
        [InlineData(false, "192.168.1.0", "255.255.255.255", null, null)]
        [InlineData(false, "192.168.1.0", "255.255.255.255", "::", "ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff")]
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

        /// <summary>Verifies IPAddressRange is assignable from its expected base types and interfaces.</summary>
        /// <param name="assignableFromType">The type that should be assignable from IPAddressRange.</param>
        [Theory]
        [InlineData(typeof(AbstractIPAddressRange))]
        [InlineData(typeof(IEquatable<IPAddressRange>))]
        [InlineData(typeof(IComparable<IPAddressRange>))]
        [InlineData(typeof(IComparable))]
#if NET48
        [InlineData(typeof(ISerializable))]
#endif
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

        #region TailOverlappedBy

        /// <summary>Verifies TailOverlappedBy returns the expected result for an IPAddressRange against various ranges.</summary>
        /// <param name="expected">Expected result.</param>
        /// <param name="thisHead">The head address of the range under test.</param>
        /// <param name="thisTail">The tail address of the range under test.</param>
        /// <param name="thatHead">The head address of the other range.</param>
        /// <param name="thatTail">The tail address of the other range.</param>
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

        #region Ctor

        /// <summary>Verifies the two-argument constructor correctly sets Head and Tail.</summary>
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

        /// <summary>Verifies the single-address constructor sets both Head and Tail to the same address.</summary>
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

        #region maxEnumerationExponent propagation

        /// <summary>Verifies that <see cref="IPAddressRange(IPAddress, int)"/> propagates maxEnumerationExponent.</summary>
        [Fact]
        public void Ctor_SingleAddress_ExponentPropagates_Test()
        {
            var range = new IPAddressRange(IPAddress.Parse("10.0.0.1"), maxEnumerationExponent: 4);
            Assert.Equal(4, range.MaxEnumerationExponent);
        }

        #endregion

        #region Head set

        /// <summary>Verifies constructing with head greater than tail throws InvalidOperationException.</summary>
        [Fact]
        public void Head_Set_GreaterThanTail_Test()
        {
            // Arrange
            // Act
            // Assert
            Assert.Throws<InvalidOperationException>(() => new IPAddressRange(IPAddress.Broadcast, IPAddress.Any));
        }

        /// <summary>Verifies constructing with head from a different address family than tail throws InvalidOperationException.</summary>
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

        /// <summary>Verifies constructing with tail from a different address family than head throws InvalidOperationException.</summary>
        [Fact]
        public void Tail_Set_DifferentAddressFamilyThanHead_Throw_InvalidOperationException_Test()
        {
            // Arrange
            // Act
            // Assert
            Assert.Throws<InvalidOperationException>(() => new IPAddressRange(IPAddress.Any, IPAddress.IPv6Loopback));
        }

        /// <summary>Verifies constructing with tail less than head throws InvalidOperationException.</summary>
        [Fact]
        public void Tail_Set_LessThanHead_Test()
        {
            // Arrange
            // Act
            // Assert
            Assert.Throws<InvalidOperationException>(() => new IPAddressRange(IPAddress.Broadcast, IPAddress.Any));
        }

        #endregion

        #region Formatting

        #region ToString

        /// <summary>Verifies ToString() produces the same output as the "G" format specifier.</summary>
        /// <param name="headString">The head IP address string.</param>
        /// <param name="tailString">The tail IP address string.</param>
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

        /// <summary>
        ///     Gets test data for <see cref="ToString_Format_Test" />.
        ///     Covers all general format specifiers (<see langword="null" />, empty, "g", "G") for both IPv4 and IPv6 ranges.
        ///     <para>Parameters: expected (string), format (string), formatProvider (IFormatProvider), ipAddressRange (IPAddressRange).</para>
        /// </summary>
        /// <value>
        ///     Test data for <see cref="ToString_Format_Test" />.
        ///     Covers all general format specifiers (<see langword="null" />, empty, "g", "G") for both IPv4 and IPv6 ranges.
        ///     <para>Parameters: expected (string), format (string), formatProvider (IFormatProvider), ipAddressRange (IPAddressRange).</para>
        /// </value>
        public static TheoryData<string, string, IFormatProvider, IPAddressRange> ToString_Format_Test_Values
        {
            get
            {
                var ipv4Range = new IPAddressRange(IPAddress.Parse("192.168.1.1"), IPAddress.Parse("192.168.1.42"));
                var ipv4Single = new IPAddressRange(IPAddress.Parse("192.168.1.1"), IPAddress.Parse("192.168.1.1"));
                var ipv6Range = new IPAddressRange(IPAddress.Parse("::beef"), IPAddress.Parse("0123::dead"));
                var ipv6Single = new IPAddressRange(IPAddress.Parse("::beef"), IPAddress.Parse("::beef"));

                var data = new TheoryData<string, string, IFormatProvider, IPAddressRange>();

                foreach (var format in new[] { null, string.Empty, "g", "G" })
                {
                    foreach (var range in new[] { ipv4Range, ipv4Single, ipv6Range, ipv6Single })
                    {
                        data.Add($"{range.Head} - {range.Tail}", format, CultureInfo.CurrentCulture, range);
                    }
                }

                return data;
            }
        }

        /// <summary>
        ///     Verifies that <see cref="AbstractIPAddressRange.ToString(string, IFormatProvider)" /> returns the expected string
        ///     for various format specifiers and address ranges.
        /// </summary>
        /// <param name="expected">the expected formatted string.</param>
        /// <param name="format">the format specifier to use.</param>
        /// <param name="formatProvider">the format provider to use.</param>
        /// <param name="ipAddressRange">the IP address range to format.</param>
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

        /// <summary>Verifies ToString throws FormatException for an unrecognized format specifier.</summary>
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

        internal class IPAddressRangeEqualityComparer : IEqualityComparer<IPAddressRange>
        {
            public static readonly IPAddressRangeEqualityComparer Instance = new();

            public bool Equals(IPAddressRange x, IPAddressRange y)
            {
                if (ReferenceEquals(x, y))
                {
                    return true;
                }

                if (x is null && y is null)
                {
                    return true;
                }

                if (x is null || y is null)
                {
                    return false;
                }

                return x.Equals(y);
            }

            public int GetHashCode(IPAddressRange obj)
            {
                return obj is null ? -1 : obj.GetHashCode();
            }
        }
    }
}
