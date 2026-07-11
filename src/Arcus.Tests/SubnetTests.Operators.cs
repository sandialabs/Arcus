namespace Arcus.Tests
{
    /// <content>
    ///     <see cref="Subnet"/> tests for operators
    /// </content>
    public partial class SubnetTests
    {
        #region Operator Equality

        /// <summary>Verifies that the <c>==</c> operator returns the expected equality result based on comparison sign.</summary>
        /// <param name="expected">Expected comparison sign; zero means the subnets are equal.</param>
        /// <param name="left">The left-hand subnet.</param>
        /// <param name="right">The right-hand subnet.</param>
        [Theory]
        [MemberData(nameof(Comparison_Values))]
        public void Operator_Equals_Test(int expected, Subnet left, Subnet right)
        {
            // Arrange
            // Act
            var result = left == right;

            // Assert
            Assert.Equal(expected == 0, result);
        }

        /// <summary>Verifies that the <c>!=</c> operator returns the expected inequality result based on comparison sign.</summary>
        /// <param name="expected">Expected comparison sign; non-zero means the subnets are not equal.</param>
        /// <param name="left">The left-hand subnet.</param>
        /// <param name="right">The right-hand subnet.</param>
        [Theory]
        [MemberData(nameof(Comparison_Values))]
        public void Operator_NotEquals_Test(int expected, Subnet left, Subnet right)
        {
            // Arrange
            // Act
            var result = left != right;

            // Assert
            Assert.Equal(expected != 0, result);
        }

        #endregion

        #region Comparison Operators

        /// <summary>Verifies that the <c>&gt;</c> operator returns the expected result based on comparison sign.</summary>
        /// <param name="expected">Expected comparison sign; positive means left is greater.</param>
        /// <param name="left">The left-hand subnet.</param>
        /// <param name="right">The right-hand subnet.</param>
        [Theory]
        [MemberData(nameof(Comparison_Values))]
        public void Operator_GreaterThan_Test(int expected, Subnet left, Subnet right)
        {
            // Arrange
            // Act
            var result = left > right;

            // Assert
            Assert.Equal(expected > 0, result);
        }

        /// <summary>Verifies that the <c>&gt;=</c> operator returns the expected result based on comparison sign.</summary>
        /// <param name="expected">Expected comparison sign; non-negative means left is greater than or equal.</param>
        /// <param name="left">The left-hand subnet.</param>
        /// <param name="right">The right-hand subnet.</param>
        [Theory]
        [MemberData(nameof(Comparison_Values))]
        public void Operator_GreaterThanOrEqual_Test(int expected, Subnet left, Subnet right)
        {
            // Arrange
            // Act
            var result = left >= right;

            // Assert
            Assert.Equal(expected >= 0, result);
        }

        /// <summary>Verifies that the <c>&lt;</c> operator returns the expected result based on comparison sign.</summary>
        /// <param name="expected">Expected comparison sign; negative means left is less.</param>
        /// <param name="left">The left-hand subnet.</param>
        /// <param name="right">The right-hand subnet.</param>
        [Theory]
        [MemberData(nameof(Comparison_Values))]
        public void Operator_LessThan_Test(int expected, Subnet left, Subnet right)
        {
            // Arrange
            // Act
            var result = left < right;

            // Assert
            Assert.Equal(expected < 0, result);
        }

        /// <summary>Verifies that the <c>&lt;=</c> operator returns the expected result based on comparison sign.</summary>
        /// <param name="expected">Expected comparison sign; non-positive means left is less than or equal.</param>
        /// <param name="left">The left-hand subnet.</param>
        /// <param name="right">The right-hand subnet.</param>
        [Theory]
        [MemberData(nameof(Comparison_Values))]
        public void Operator_LessThanOrEqual_Test(int expected, Subnet left, Subnet right)
        {
            // Arrange
            // Act
            var result = left <= right;

            // Assert
            Assert.Equal(expected <= 0, result);
        }

        #endregion

        #region Null Operands

        /// <summary>Verifies that the <c>&lt;</c> operator returns <see langword="true"/> when the left operand is null and the right is non-null.</summary>
        [Fact]
        public void Operator_LessThan_NullLeft_NonNullRight_ReturnsTrue_Test()
        {
            // Arrange
            Subnet left = null;
            var right = Subnet.Parse("192.168.0.0/16");

            // Act
            var result = left < right;

            // Assert
            Assert.True(result);
        }

        /// <summary>Verifies that the <c>&lt;</c> operator returns <see langword="false"/> when both operands are null.</summary>
        [Fact]
        public void Operator_LessThan_BothNull_ReturnsFalse_Test()
        {
            // Arrange
            Subnet left = null;
            Subnet right = null;

            // Act
            var result = left < right;

            // Assert
            Assert.False(result);
        }

        /// <summary>Verifies that the <c>&gt;</c> operator returns <see langword="false"/> when the left operand is null.</summary>
        [Fact]
        public void Operator_GreaterThan_NullLeft_ReturnsFalse_Test()
        {
            // Arrange
            Subnet left = null;
            var right = Subnet.Parse("192.168.0.0/16");

            // Act
            var result = left > right;

            // Assert
            Assert.False(result);
        }

        #endregion
    }
}
