namespace Arcus.Tests
{
    /// <content>
    ///     <see cref="BigEndianBitWrapper"/> tests for <see cref="System.IEquatable{BigEndianBitWrapper}"/>, <c>==</c>, and <c>!=</c>.
    /// </content>
    public partial class BigEndianBitWrapperTests
    {
        #region Equals / == / !=

        /// <summary>Verifies that Equals, ==, and != all agree when two wrappers have the same value and byte width.</summary>
        [Fact]
        public void Equals_SameValueAndWidth_ReturnsTrue_Test()
        {
            // Arrange
            var a = WrapIPv4("192.168.1.1");
            var b = WrapIPv4("192.168.1.1");

            // Act / Assert
            Assert.True(a.Equals(b));
            Assert.True(a == b); // intentional == operator test; Assert.Equal would bypass the operator
            Assert.False(a != b); // intentional != operator test; Assert.NotEqual would bypass the operator
        }

        /// <summary>Verifies that Equals, ==, and != all agree when two wrappers have different values.</summary>
        [Fact]
        public void Equals_DifferentValues_ReturnsFalse_Test()
        {
            // Arrange
            var a = WrapIPv4("192.168.1.1");
            var b = WrapIPv4("192.168.1.2");

            // Act / Assert
            Assert.False(a.Equals(b));
            Assert.False(a == b); // intentional == operator test; Assert.Equal would bypass the operator
            Assert.True(a != b); // intentional != operator test; Assert.NotEqual would bypass the operator
        }

        /// <summary>Verifies that wrappers with the same numeric value but different byte widths are not equal.</summary>
        [Fact]
        public void Equals_DifferentByteWidth_ReturnsFalse_Test()
        {
            // Arrange: same numeric value 1, but different byte widths
            var ipv4 = BigEndianBitWrapper.FromBytes([0, 0, 0, 1]);
            var ipv6 = BigEndianBitWrapper.FromBytes([0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1]);

            // Act / Assert
            Assert.False(ipv4.Equals(ipv6));
            Assert.False(ipv4 == ipv6); // intentional == operator test; Assert.Equal would bypass the operator
            Assert.True(ipv4 != ipv6); // intentional != operator test; Assert.NotEqual would bypass the operator
        }

        /// <summary>Verifies that Equals(object) returns true when the boxed object is an equal wrapper.</summary>
        [Fact]
        public void Equals_ObjectBoxed_ReturnsTrueWhenEqual_Test()
        {
            // Arrange
            var a = WrapIPv4("10.0.0.1");
            object b = WrapIPv4("10.0.0.1");

            // Act / Assert
            Assert.True(a.Equals(b));
        }

        /// <summary>Verifies that Equals(object) returns false when the argument is a non-wrapper type.</summary>
        [Fact]
        public void Equals_NonWrapper_ReturnsFalse_Test()
        {
            // Arrange
            var a = WrapIPv4("10.0.0.1");

            // Act / Assert
            // codeql[cs/equals-on-unrelated-types] Intentionally verifying that Equals(object) returns false when comparing to a different type — correct behavior
            Assert.False(a.Equals("not a wrapper"));
        }

        /// <summary>Verifies that equal wrappers produce identical hash codes.</summary>
        [Fact]
        public void GetHashCode_EqualValues_HaveSameHashCode_Test()
        {
            // Arrange
            var a = WrapIPv4("192.168.1.1");
            var b = WrapIPv4("192.168.1.1");

            // Act / Assert
            Assert.Equal(a.GetHashCode(), b.GetHashCode());
        }

        #endregion // end: Equals / == / !=
    }
}
