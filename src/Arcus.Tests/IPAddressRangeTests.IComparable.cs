using System.Net;

namespace Arcus.Tests
{
    /// <content>
    ///     <see cref="IPAddressRange"/> tests for <see cref="System.IComparable{IPAddressRange}"/>
    /// </content>
    public partial class IPAddressRangeTests
    {
        #region CompareTo / Operators

        /// <summary>
        ///     Gets test data for <see cref="CompareTo_Test" /> and operator tests.
        ///     Covers equal ranges, null right operands, cross-family ordering, and length-based ordering within a family.
        ///     <para>Parameters: expected comparison result (int), left (IPAddressRange), right (IPAddressRange).</para>
        /// </summary>
        /// <value>
        ///     Test data for <see cref="CompareTo_Test" /> and operator tests.
        ///     Covers equal ranges, null right operands, cross-family ordering, and length-based ordering within a family.
        ///     <para>Parameters: expected comparison result (int), left (IPAddressRange), right (IPAddressRange).</para>
        /// </value>
        public static TheoryData<int, IPAddressRange, IPAddressRange> Comparison_Values
        {
            get
            {
                var ipv4Slash16 = new IPAddressRange(IPAddress.Parse("192.168.0.0"), IPAddress.Parse("192.168.255.255"));
                var ipv6Slash64 = new IPAddressRange(IPAddress.Parse("ab:cd::"), IPAddress.Parse("ab:cd::ffff:ffff:ffff:ffff"));
                var ipv4Slash20 = new IPAddressRange(IPAddress.Parse("192.168.0.0"), IPAddress.Parse("192.168.15.255"));
                var ipv6Slash96 = new IPAddressRange(IPAddress.Parse("ab:cd::"), IPAddress.Parse("ab:cd::ffff:ffff"));
                var ipv4All = new IPAddressRange(IPAddress.Parse("0.0.0.0"), IPAddress.Parse("255.255.255.255"));
                var ipv6All = new IPAddressRange(
                    IPAddress.Parse("::"),
                    IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff")
                );
                var ipv4Single = new IPAddressRange(IPAddress.Parse("0.0.0.0"), IPAddress.Parse("0.0.0.0"));
                var ipv6Single = new IPAddressRange(IPAddress.Parse("::"), IPAddress.Parse("::"));

                var data = new TheoryData<int, IPAddressRange, IPAddressRange>
                {
                    {
                        0,
                        new IPAddressRange(ipv4Slash16.Head, ipv4Slash16.Tail),
                        new IPAddressRange(ipv4Slash16.Head, ipv4Slash16.Tail)
                    },
                    {
                        0,
                        new IPAddressRange(ipv6Slash64.Head, ipv6Slash64.Tail),
                        new IPAddressRange(ipv6Slash64.Head, ipv6Slash64.Tail)
                    },
                    { 1, new IPAddressRange(ipv4Slash16.Head, ipv4Slash16.Tail), null },
                    { 1, new IPAddressRange(ipv6Slash64.Head, ipv6Slash64.Tail), null },
                    {
                        1,
                        new IPAddressRange(ipv4Slash16.Head, ipv4Slash16.Tail),
                        new IPAddressRange(ipv4Slash20.Head, ipv4Slash20.Tail)
                    },
                    {
                        -1,
                        new IPAddressRange(ipv4Slash20.Head, ipv4Slash20.Tail),
                        new IPAddressRange(ipv4Slash16.Head, ipv4Slash16.Tail)
                    },
                    {
                        1,
                        new IPAddressRange(ipv6Slash64.Head, ipv6Slash64.Tail),
                        new IPAddressRange(ipv6Slash96.Head, ipv6Slash96.Tail)
                    },
                    {
                        -1,
                        new IPAddressRange(ipv6Slash96.Head, ipv6Slash96.Tail),
                        new IPAddressRange(ipv6Slash64.Head, ipv6Slash64.Tail)
                    },
                    { -1, new IPAddressRange(ipv4All.Head, ipv4All.Tail), new IPAddressRange(ipv6All.Head, ipv6All.Tail) },
                    { 1, new IPAddressRange(ipv6All.Head, ipv6All.Tail), new IPAddressRange(ipv4All.Head, ipv4All.Tail) },
                    {
                        -1,
                        new IPAddressRange(ipv4Single.Head, ipv4Single.Tail),
                        new IPAddressRange(ipv6Single.Head, ipv6Single.Tail)
                    },
                    {
                        1,
                        new IPAddressRange(ipv6Single.Head, ipv6Single.Tail),
                        new IPAddressRange(ipv4Single.Head, ipv4Single.Tail)
                    },
                };

                return data;
            }
        }

        /// <summary>Verifies CompareTo returns the expected result for all ordered range pairs.</summary>
        /// <param name="expected">Expected comparison result.</param>
        /// <param name="left">The left operand range.</param>
        /// <param name="right">The right operand range.</param>
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

        #endregion // end: CompareTo / Operators

        #region CompareTo(object)

        /// <summary>Verifies that <see cref="IPAddressRange.CompareTo(object)"/> throws <see cref="ArgumentException"/> when the argument is not an <see cref="IPAddressRange"/>.</summary>
        [Fact]
        public void CompareTo_Object_NonRange_Throws_ArgumentException_Test()
        {
            // Arrange
            var range = new IPAddressRange(IPAddress.Parse("192.168.0.0"), IPAddress.Parse("192.168.255.255"));

            // Act / Assert
            Assert.Throws<ArgumentException>(() => range.CompareTo("not a range"));
        }

        /// <summary>Verifies that <see cref="IPAddressRange.CompareTo(object)"/> returns a positive value when compared to null.</summary>
        [Fact]
        public void CompareTo_Object_Null_ReturnsPositive_Test()
        {
            // Arrange
            var range = new IPAddressRange(IPAddress.Parse("192.168.0.0"), IPAddress.Parse("192.168.255.255"));

            // Act
            var result = range.CompareTo((object)null);

            // Assert
            Assert.Equal(1, result);
        }

        /// <summary>Verifies that <see cref="IPAddressRange.CompareTo(object)"/> returns zero when compared to a boxed equivalent range.</summary>
        [Fact]
        public void CompareTo_Object_RangeBox_ReturnsZero_Test()
        {
            // Arrange
            var range = new IPAddressRange(IPAddress.Parse("192.168.0.0"), IPAddress.Parse("192.168.255.255"));
            object boxed = new IPAddressRange(IPAddress.Parse("192.168.0.0"), IPAddress.Parse("192.168.255.255"));

            // Act
            var result = range.CompareTo(boxed);

            // Assert
            Assert.Equal(0, result);
        }

        #endregion // end: CompareTo(object)
    }
}
