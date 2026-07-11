namespace Arcus.Tests
{
    /// <content>
    ///     <see cref="Subnet"/> tests for <see cref="IComparable{Subnet}"/> and <see cref="IComparable"/>
    /// </content>
    public partial class SubnetTests
    {
        /// <summary>Verifies that <see cref="Subnet.CompareTo(Subnet)"/> returns a value whose sign matches the expected ordering.</summary>
        /// <param name="expected">Expected sign of the comparison result.</param>
        /// <param name="left">The left-hand subnet.</param>
        /// <param name="right">The right-hand subnet.</param>
        [Theory]
        [MemberData(nameof(Comparison_Values))]
        public void CompareTo_Subnet_ReturnsExpectedSign_Test(int expected, Subnet left, Subnet right)
        {
            // Arrange
            // Act
            var result = left.CompareTo(right);

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>Verifies that <see cref="Subnet.CompareTo(object)"/> throws <see cref="ArgumentException"/> when the argument is not a <see cref="Subnet"/>.</summary>
        [Fact]
        public void CompareTo_Object_NonSubnet_Throws_ArgumentException_Test()
        {
            // Arrange
            var subnet = Subnet.Parse("192.168.0.0/16");

            // Act / Assert
            Assert.Throws<ArgumentException>(() => subnet.CompareTo("not a subnet"));
        }

        /// <summary>Verifies that <see cref="Subnet.CompareTo(object)"/> returns a positive value when compared to null.</summary>
        [Fact]
        public void CompareTo_Object_Null_ReturnsPositive_Test()
        {
            // Arrange
            var subnet = Subnet.Parse("192.168.0.0/16");

            // Act
            var result = subnet.CompareTo((object)null);

            // Assert
            Assert.Equal(1, result);
        }

        /// <summary>Verifies that <see cref="Subnet.CompareTo(object)"/> returns zero when compared to a boxed equivalent subnet.</summary>
        [Fact]
        public void CompareTo_Object_SubnetBox_ReturnsZero_Test()
        {
            // Arrange
            var subnet = Subnet.Parse("192.168.0.0/16");
            object boxed = Subnet.Parse("192.168.0.0/16");

            // Act
            var result = subnet.CompareTo(boxed);

            // Assert
            Assert.Equal(0, result);
        }
    }
}
