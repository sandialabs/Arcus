using System.Net;

namespace Arcus.Tests
{
    /// <content>
    ///     <see cref="IPAddressRange"/> tests for operators
    /// </content>
    public partial class IPAddressRangeTests
    {
        #region Operators

        /// <summary>Verifies the == operator returns true only when the comparison result is zero.</summary>
        /// <param name="expected">Expected comparison result.</param>
        /// <param name="left">Left operand range.</param>
        /// <param name="right">Right operand range.</param>
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

        /// <summary>Verifies the != operator returns true when the comparison result is non-zero.</summary>
        /// <param name="expected">Expected comparison result.</param>
        /// <param name="left">Left operand range.</param>
        /// <param name="right">Right operand range.</param>
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

        /// <summary>Verifies the &gt; operator returns true only when the comparison result is positive.</summary>
        /// <param name="expected">Expected comparison result.</param>
        /// <param name="left">Left operand range.</param>
        /// <param name="right">Right operand range.</param>
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

        /// <summary>Verifies the &gt;= operator returns true when the comparison result is zero or positive.</summary>
        /// <param name="expected">Expected comparison result.</param>
        /// <param name="left">Left operand range.</param>
        /// <param name="right">Right operand range.</param>
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

        /// <summary>Verifies the &lt; operator returns true only when the comparison result is negative.</summary>
        /// <param name="expected">Expected comparison result.</param>
        /// <param name="left">Left operand range.</param>
        /// <param name="right">Right operand range.</param>
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

        /// <summary>Verifies the &lt;= operator returns true when the comparison result is zero or negative.</summary>
        /// <param name="expected">Expected comparison result.</param>
        /// <param name="left">Left operand range.</param>
        /// <param name="right">Right operand range.</param>
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

        #endregion // end: Operators

        #region Null Operands

        /// <summary>Verifies that the &lt; operator returns <see langword="true"/> when the left operand is null and the right is non-null.</summary>
        [Fact]
        public void Operator_LessThan_NullLeft_NonNullRight_ReturnsTrue_Test()
        {
            // Arrange
            IPAddressRange left = null;
            var right = new IPAddressRange(IPAddress.Parse("192.168.0.0"), IPAddress.Parse("192.168.255.255"));

            // Act
            var result = left < right;

            // Assert
            Assert.True(result);
        }

        /// <summary>Verifies that the &lt; operator returns <see langword="false"/> when both operands are null.</summary>
        [Fact]
        public void Operator_LessThan_BothNull_ReturnsFalse_Test()
        {
            // Arrange
            IPAddressRange left = null;
            IPAddressRange right = null;

            // Act
            var result = left < right;

            // Assert
            Assert.False(result);
        }

        /// <summary>Verifies that the &gt; operator returns <see langword="false"/> when the left operand is null.</summary>
        [Fact]
        public void Operator_GreaterThan_NullLeft_ReturnsFalse_Test()
        {
            // Arrange
            IPAddressRange left = null;
            var right = new IPAddressRange(IPAddress.Parse("192.168.0.0"), IPAddress.Parse("192.168.255.255"));

            // Act
            var result = left > right;

            // Assert
            Assert.False(result);
        }

        /// <summary>Verifies that the == operator returns <see langword="false"/> when one operand is null and the other is non-null.</summary>
        [Fact]
        public void Operator_Equals_NullLeft_NonNullRight_ReturnsFalse_Test()
        {
            // Arrange
            IPAddressRange left = null;
            var right = new IPAddressRange(IPAddress.Parse("192.168.0.0"), IPAddress.Parse("192.168.255.255"));

            // Act
            var result = left == right;

            // Assert
            Assert.False(result);
        }

        /// <summary>Verifies that the == operator returns <see langword="true"/> when both operands are null.</summary>
        [Fact]
        public void Operator_Equals_BothNull_ReturnsTrue_Test()
        {
            // Arrange
            IPAddressRange left = null;
            IPAddressRange right = null;

            // Act
            var result = left == right;

            // Assert
            Assert.True(result);
        }

        #endregion // end: Null Operands
    }
}
