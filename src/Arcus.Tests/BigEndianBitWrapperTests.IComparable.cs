namespace Arcus.Tests
{
    /// <content>
    ///     <see cref="BigEndianBitWrapper"/> tests for <see cref="System.IComparable"/>.
    /// </content>
    public partial class BigEndianBitWrapperTests
    {
        #region CompareTo

        /// <summary>
        ///     Gets theory data for CompareTo sign tests.
        /// </summary>
        /// <value>
        ///     Rows: leftStr (string), rightStr (string), expectedSign (int).
        /// </value>
        public static TheoryData<string, string, int> CompareTo_Test_Data =>
            new()
            {
                // IPv4
                { "10.0.0.1", "10.0.0.1", 0 },
                { "10.0.0.1", "10.0.0.2", -1 },
                { "10.0.0.2", "10.0.0.1", 1 },
                { "0.0.0.0", "255.255.255.255", -1 },
                { "255.255.255.255", "0.0.0.0", 1 },
                { "192.168.1.0", "192.168.1.255", -1 },
            };

        /// <summary>Verifies CompareTo returns the correct sign for ordered pairs of IPv4 addresses.</summary>
        /// <param name="leftStr">The left IPv4 address string.</param>
        /// <param name="rightStr">The right IPv4 address string.</param>
        /// <param name="expectedSign">The expected comparison result.</param>
        [Theory]
        [MemberData(nameof(CompareTo_Test_Data))]
        public void CompareTo_IPv4Values_ReturnsExpectedSign_Test(string leftStr, string rightStr, int expectedSign)
        {
            // Arrange
            var left = WrapIPv4(leftStr);
            var right = WrapIPv4(rightStr);

            // Act
            var result = left.CompareTo(right);

            // Assert: only the sign matters
            Assert.Equal(expectedSign, System.Math.Sign(result));
        }

        /// <summary>
        ///     Verifies CompareTo considers <see cref="BigEndianBitWrapper.ByteWidth" /> as a tiebreaker
        ///     when numeric values are equal, ensuring consistency with <c>==</c> (which checks ByteWidth via Equals).
        /// </summary>
        [Fact]
        public void CompareTo_EqualValuesWithDifferentByteWidth_ReturnsNonZero_Test()
        {
            // Arrange: same numeric value (1) but different byte widths (1 vs 2)
            var oneByte = Wrap(1);
            var twoByte = Wrap(0, 1);

            // Act
            var compareResult = oneByte.CompareTo(twoByte);

            // Assert: CompareTo must reflect the ByteWidth difference (Equals considers ByteWidth)
            Assert.NotEqual(0, compareResult);
            Assert.False(oneByte == twoByte); // Equals returns false (different ByteWidth)
            Assert.False(oneByte <= twoByte && oneByte >= twoByte); // should NOT be contradictory with ==
        }

        /// <summary>
        ///     Verifies CompareTo treats 0-value wrappers with different byte widths as different,
        ///     consistent with <c>==</c> which considers ByteWidth.
        /// </summary>
        [Fact]
        public void CompareTo_ZeroValuesWithDifferentByteWidth_ReturnsNonZero_Test()
        {
            // Arrange: zero value but different byte widths
            var oneByte = Wrap(0);
            var twoByte = Wrap(0, 0);

            // Act
            var compareResult = oneByte.CompareTo(twoByte);

            // Assert
            Assert.NotEqual(0, compareResult);
            Assert.False(oneByte == twoByte);
            Assert.False(oneByte <= twoByte && oneByte >= twoByte);
        }

        /// <summary>Verifies CompareTo imposes strict ascending order across the IPv6 address space.</summary>
        [Fact]
        public void CompareTo_IPv6_AscendingOrder_Test()
        {
            // Arrange
            var zero = WrapIPv6("::");
            var one = WrapIPv6("::1");
            var max = WrapIPv6("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff");

            // Act / Assert
            Assert.True(zero.CompareTo(one) < 0);
            Assert.True(one.CompareTo(max) < 0);
            // CodeQL [cs/comparison-of-identical-expressions] Intentionally verifying that CompareTo returns 0 when comparing an object to itself — correct behavior
            Assert.Equal(0, zero.CompareTo(zero));
        }

        #endregion // end: CompareTo
    }
}
