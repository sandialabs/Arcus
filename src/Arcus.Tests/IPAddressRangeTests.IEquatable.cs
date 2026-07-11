using System.Net;

namespace Arcus.Tests
{
    /// <content>
    ///     <see cref="IPAddressRange"/> tests for <see cref="System.IEquatable{IPAddressRange}"/>
    /// </content>
    public partial class IPAddressRangeTests
    {
        #region Equals

        #region Equals(IPAddressRange)

        /// <summary>Verifies that <see cref="IPAddressRange.Equals(IPAddressRange)"/> returns <see langword="true"/> for self-comparison via the <see cref="object.ReferenceEquals"/> fast-path.</summary>
        [Fact]
        public void Equals_ReferenceEqualsSelf_ReturnsTrue_Test()
        {
            // Arrange
            var range = new IPAddressRange(IPAddress.Parse("192.168.1.1"), IPAddress.Parse("192.168.1.10"));

            // Act
            var result = range.Equals(range);

            // Assert
            Assert.True(result);
        }

        /// <summary>Verifies Equals(IPAddressRange) returns the expected result for equal, null, and differing ranges.</summary>
        /// <param name="expected">Expected result.</param>
        /// <param name="xHead">The head address of the left range.</param>
        /// <param name="xTail">The tail address of the left range.</param>
        /// <param name="yHead">The head address of the right range.</param>
        /// <param name="yTail">The tail address of the right range.</param>
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

        /// <summary>Verifies Equals(object) returns the expected result when the argument is boxed as object.</summary>
        /// <param name="expected">Expected result.</param>
        /// <param name="xHead">The head address of the left range.</param>
        /// <param name="xTail">The tail address of the left range.</param>
        /// <param name="yHead">The head address of the right range.</param>
        /// <param name="yTail">The tail address of the right range.</param>
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

        /// <summary>Verifies that <see cref="IPAddressRange.Equals(object)"/> returns <see langword="false"/> when the argument is not an <see cref="IPAddressRange"/>.</summary>
        [Fact]
        public void Equals_Object_NonIPAddressRangeType_ReturnsFalse_Test()
        {
            // Arrange
            var range = new IPAddressRange(IPAddress.Parse("192.168.1.1"), IPAddress.Parse("192.168.1.10"));

            // Act
            // codeql[cs/equals-on-unrelated-types] Intentionally verifying that Equals(object) returns false when comparing to a different type — correct behavior
            var result = range.Equals("not an IP address range");

            // Assert
            Assert.False(result);
        }

        #endregion // end: Equals(object)

        #region GetHashCode

        /// <summary>Verifies equal ranges produce the same hash code and differing ranges produce different hash codes.</summary>
        /// <param name="expected">Whether the two hash codes should be equal.</param>
        /// <param name="xHead">The head address of the left range.</param>
        /// <param name="xTail">The tail address of the left range.</param>
        /// <param name="yHead">The head address of the right range.</param>
        /// <param name="yTail">The tail address of the right range.</param>
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

        #region Equals edge cases specific to direct Head/Tail comparison

        /// <summary>Verifies that two ranges with the same Head and Tail are equal regardless of construction path.</summary>
        /// <param name="headString">The head IP address string for both ranges.</param>
        /// <param name="tailString">The tail IP address string for both ranges.</param>
        [Theory]
        [InlineData("192.168.1.1", "192.168.1.10")]
        [InlineData("::abcd", "::abcf")]
        [InlineData("10.0.0.0", "10.0.0.1")]
        [InlineData("2001:db8::", "2001:db8::ff")]
        public void Equals_SameHeadTail_RangesEqual_Test(string headString, string tailString)
        {
            var head = IPAddress.Parse(headString);
            var tail = IPAddress.Parse(tailString);
            var range1 = new IPAddressRange(head, tail);
            var range2 = new IPAddressRange(head, tail);

            var result = range1.Equals(range2);

            Assert.True(result);
            Assert.Equal(range1.GetHashCode(), range2.GetHashCode());
        }

        /// <summary>Verifies that Equals correctly distinguishes ranges with same Head but different Tail.</summary>
        [Fact]
        public void Equals_SameHead_DifferentTail_ReturnsFalse_Test()
        {
            var head = IPAddress.Parse("192.168.1.0");
            var range1 = new IPAddressRange(head, IPAddress.Parse("192.168.1.10"));
            var range2 = new IPAddressRange(head, IPAddress.Parse("192.168.1.20"));

            Assert.False(range1.Equals(range2));
        }

        #endregion // end: Equals edge cases

        #endregion // end: Equals
    }
}
