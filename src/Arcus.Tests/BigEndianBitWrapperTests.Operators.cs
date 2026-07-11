using System.Reflection;

namespace Arcus.Tests
{
    /// <content>
    ///     <see cref="BigEndianBitWrapper"/> tests for bitwise operators (<c>&amp;</c>, <c>|</c>, <c>~</c>) and
    ///     comparison operators (<c>&lt;</c>, <c>&gt;</c>, <c>&lt;=</c>, <c>&gt;=</c>).
    /// </content>
    public partial class BigEndianBitWrapperTests
    {
        #region Bitwise AND

        /// <summary>Verifies the &amp; operator produces the correct network address when ANDing a host address with a subnet mask.</summary>
        [Fact]
        public void BitwiseAnd_AddressAndMask_ReturnsNetworkAddress_Test()
        {
            // Arrange: 192.168.1.100 & 255.255.255.0 = 192.168.1.0
            var address = WrapIPv4("192.168.1.100");
            var mask = BigEndianBitWrapper.CreateMask(4, 24);

            // Act
            var network = address & mask;

            // Assert
            Assert.Equal(WrapIPv4("192.168.1.0"), network);
        }

        /// <summary>Verifies the &amp; operator returns the all-zero wrapper when ANDed with an all-zeros mask.</summary>
        [Fact]
        public void BitwiseAnd_WithAllZerosMask_ReturnsZero_Test()
        {
            // Arrange
            var address = WrapIPv4("10.20.30.40");
            var zero = BigEndianBitWrapper.CreateMask(4, 0); // all zeros

            // Act
            var result = address & zero;

            // Assert
            Assert.Equal(WrapIPv4("0.0.0.0"), result);
        }

        /// <summary>Verifies the &amp; operator returns the original value when ANDed with an all-ones mask.</summary>
        [Fact]
        public void BitwiseAnd_WithAllOnesMask_ReturnsSameValue_Test()
        {
            // Arrange
            var address = WrapIPv4("10.20.30.40");
            var allOnes = BigEndianBitWrapper.CreateMask(4, 32); // all ones

            // Act
            var result = address & allOnes;

            // Assert
            Assert.Equal(address, result);
        }

        #endregion // end: Bitwise AND

        #region Bitwise OR

        /// <summary>Verifies the | operator produces the correct broadcast address when ORing a network address with the inverse mask.</summary>
        [Fact]
        public void BitwiseOr_AddressOrInverseMask_ReturnsBroadcastAddress_Test()
        {
            // Arrange: 192.168.1.0 | ~255.255.255.0 = 192.168.1.255
            var network = WrapIPv4("192.168.1.0");
            var inverseMask = ~BigEndianBitWrapper.CreateMask(4, 24);

            // Act
            var broadcast = network | inverseMask;

            // Assert
            Assert.Equal(WrapIPv4("192.168.1.255"), broadcast);
        }

        /// <summary>Verifies the | operator returns the original value when ORed with an all-zeros wrapper.</summary>
        [Fact]
        public void BitwiseOr_WithAllZeros_ReturnsSameValue_Test()
        {
            // Arrange
            var address = WrapIPv4("10.20.30.40");
            var zero = BigEndianBitWrapper.CreateMask(4, 0);

            // Act
            var result = address | zero;

            // Assert
            Assert.Equal(address, result);
        }

        /// <summary>Verifies the | operator returns the all-ones wrapper when ORed with an all-ones mask.</summary>
        [Fact]
        public void BitwiseOr_WithAllOnes_ReturnsAllOnes_Test()
        {
            // Arrange
            var address = WrapIPv4("10.20.30.40");
            var allOnes = BigEndianBitWrapper.CreateMask(4, 32);

            // Act
            var result = address | allOnes;

            // Assert
            Assert.Equal(WrapIPv4("255.255.255.255"), result);
        }

        #endregion // end: Bitwise OR

        #region Bitwise NOT

        /// <summary>Verifies the ~ operator inverts a /24 mask to the correct host mask.</summary>
        [Fact]
        public void BitwiseNot_Mask24_ReturnsInverseMask_Test()
        {
            // Arrange: ~255.255.255.0 = 0.0.0.255
            var mask = BigEndianBitWrapper.CreateMask(4, 24);

            // Act
            var inverse = ~mask;

            // Assert
            Assert.Equal(WrapIPv4("0.0.0.255"), inverse);
            Assert.Equal(4, inverse.ByteWidth);
        }

        /// <summary>Verifies the ~ operator inverts an all-zero wrapper to the all-ones wrapper.</summary>
        [Fact]
        public void BitwiseNot_AllZeros_ReturnsMaxForWidth_Test()
        {
            // Arrange
            var zeros = BigEndianBitWrapper.CreateMask(4, 0); // 0.0.0.0

            // Act
            var result = ~zeros;

            // Assert
            Assert.Equal(WrapIPv4("255.255.255.255"), result);
        }

        /// <summary>Verifies the ~ operator inverts an all-ones wrapper to the all-zero wrapper.</summary>
        [Fact]
        public void BitwiseNot_AllOnes_ReturnsZero_Test()
        {
            // Arrange
            var allOnes = BigEndianBitWrapper.CreateMask(4, 32);

            // Act
            var result = ~allOnes;

            // Assert
            Assert.Equal(WrapIPv4("0.0.0.0"), result);
        }

        /// <summary>Verifies the ~ operator correctly inverts a /64 IPv6 mask to the host portion.</summary>
        [Fact]
        public void BitwiseNot_IPv6Mask64_ReturnsCorrectInverse_Test()
        {
            // Arrange: ffff:ffff:ffff:ffff:: → NOT → ::ffff:ffff:ffff:ffff
            var mask = BigEndianBitWrapper.CreateMask(16, 64);

            // Act
            var inverse = ~mask;

            // Assert
            var expected = WrapIPv6("::ffff:ffff:ffff:ffff");
            Assert.Equal(expected, inverse);
        }

        /// <summary>Verifies the ~ operator preserves the byte width of the operand.</summary>
        [Fact]
        public void BitwiseNot_DoesNotAffectByteWidth_Test()
        {
            // Arrange
            var w = WrapIPv4("0.0.0.0");

            // Act
            var result = ~w;

            // Assert - byte width unchanged
            Assert.Equal(w.ByteWidth, result.ByteWidth);
        }

        #endregion // end: Bitwise NOT

        #region B3: ByteWidth validation for binary operators

        /// <summary>Verifies the &amp; operator throws ArgumentException when operands have different ByteWidth.</summary>
        [Fact]
        public void AndOperator_WithDifferentByteWidth_ThrowsArgumentException_Test()
        {
            var ipv4 = BigEndianBitWrapper.FromBytes([192, 168, 1, 1]);
            var ipv6 = BigEndianBitWrapper.CreateMask(16, 64);
            Assert.Throws<ArgumentException>(() => ipv4 & ipv6);
        }

        /// <summary>Verifies the | operator throws ArgumentException when operands have different ByteWidth.</summary>
        [Fact]
        public void OrOperator_WithDifferentByteWidth_ThrowsArgumentException_Test()
        {
            var ipv4 = BigEndianBitWrapper.FromBytes([192, 168, 1, 1]);
            var ipv6 = BigEndianBitWrapper.CreateMask(16, 64);
            Assert.Throws<ArgumentException>(() => ipv4 | ipv6);
        }

        #endregion

        #region B4: Operator result masking

#if NET8_0_OR_GREATER
        private static BigEndianBitWrapper CreateWrapperWithHighBits(int byteWidth)
        {
            var ctor = typeof(BigEndianBitWrapper)
                .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
                .Single(c => c.GetParameters().Length == 2);
            return (BigEndianBitWrapper)ctor.Invoke([UInt128.MaxValue, byteWidth]);
        }
#else
        private static BigEndianBitWrapper CreateWrapperWithHighBits(int byteWidth)
        {
            var ctor = typeof(BigEndianBitWrapper)
                .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
                .Single(c => c.GetParameters().Length == 3);
            var hi = byteWidth > 8 ? ulong.MaxValue : 1UL;
            const ulong lo = ulong.MaxValue;
            return (BigEndianBitWrapper)ctor.Invoke([hi, lo, byteWidth]);
        }
#endif

        /// <summary>
        ///     Verifies the &amp; operator masks the result to the operand's ByteWidth.  The backing
        ///     value of a wrapper should never have bits set beyond its ByteWidth, so a bitwise AND
        ///     between two wrappers with high backing bits produces a result that equals the
        ///     correctly-bounded version.
        /// </summary>
        [Fact]
        public void AndOperator_ResultIsMaskedToByteWidth_Test()
        {
            var a = CreateWrapperWithHighBits(4);
            var b = CreateWrapperWithHighBits(4);
            var result = a & b;
            var expected = BigEndianBitWrapper.FromBytes([0xFF, 0xFF, 0xFF, 0xFF], 4);
            Assert.Equal(expected, result);
        }

        /// <summary>
        ///     Verifies the | operator masks the result to the operand's ByteWidth.  Without masking,
        ///     ORing a high-bits wrapper with a bounded wrapper would produce a result whose backing
        ///     value has bits beyond the byte width.
        /// </summary>
        [Fact]
        public void OrOperator_ResultIsMaskedToByteWidth_Test()
        {
            var a = CreateWrapperWithHighBits(4);
            var normal = BigEndianBitWrapper.FromBytes([0x00, 0x00, 0x00, 0xFF], 4);
            var result = normal | a;
            var expected = BigEndianBitWrapper.FromBytes([0xFF, 0xFF, 0xFF, 0xFF], 4);
            Assert.Equal(expected, result);
        }

        /// <summary>Verifies the ~ operator result is correctly bounded to ByteWidth.</summary>
        [Fact]
        public void NotOperator_ResultIsMaskedToByteWidth_Test()
        {
            var w = BigEndianBitWrapper.FromBytes([0x00, 0x00, 0x00, 0xFF], 4);
            var result = ~w;
            // Inverse of 0x000000FF for 4 bytes = 0xFFFFFF00
            var expected = BigEndianBitWrapper.FromBytes([0xFF, 0xFF, 0xFF, 0x00], 4);
            Assert.Equal(expected, result);
            Assert.Equal(4, result.ByteWidth);
        }

        #endregion

        #region B5: XOR operator

        /// <summary>Verifies the ^ operator produces the correct XOR result for two IPv4 wrappers.</summary>
        [Fact]
        public void XorOperator_ProducesCorrectResult_Test()
        {
            // 0xFF00FF00 ^ 0x0F0F0F0F = 0xF00FF00F
            var a = BigEndianBitWrapper.FromBytes([0xFF, 0x00, 0xFF, 0x00], 4);
            var b = BigEndianBitWrapper.FromBytes([0x0F, 0x0F, 0x0F, 0x0F], 4);
            var result = a ^ b;
            var expected = BigEndianBitWrapper.FromBytes([0xF0, 0x0F, 0xF0, 0x0F], 4);
            Assert.Equal(expected, result);
        }

        /// <summary>Verifies the ^ operator throws ArgumentException when operands have different ByteWidth.</summary>
        [Fact]
        public void XorOperator_WithDifferentByteWidth_ThrowsArgumentException_Test()
        {
            var ipv4 = BigEndianBitWrapper.FromBytes([192, 168, 1, 1]);
            var ipv6 = BigEndianBitWrapper.CreateMask(16, 64);
            Assert.Throws<ArgumentException>(() => ipv4 ^ ipv6);
        }

        /// <summary>Verifies the ^ operator masks the result to the operand's ByteWidth.</summary>
        [Fact]
        public void XorOperator_ResultIsMaskedToByteWidth_Test()
        {
            var a = CreateWrapperWithHighBits(4);
            var normal = BigEndianBitWrapper.FromBytes([0x00, 0x00, 0x00, 0xFF], 4);
            var result = a ^ normal;
            var expected = BigEndianBitWrapper.FromBytes([0xFF, 0xFF, 0xFF, 0x00], 4);
            Assert.Equal(expected, result);
        }

        #endregion

        #region Operators: < > <= >=

        /// <summary>Verifies the &lt; operator returns the expected result for all ordered IPv4 pairs.</summary>
        /// <param name="leftStr">The left IPv4 address string.</param>
        /// <param name="rightStr">The right IPv4 address string.</param>
        /// <param name="expectedSign">The expected comparison result.</param>
        [Theory]
        [MemberData(nameof(CompareTo_Test_Data))]
        public void LessThan_Operator_ReturnsExpected_Test(string leftStr, string rightStr, int expectedSign)
        {
            // Arrange
            var left = WrapIPv4(leftStr);
            var right = WrapIPv4(rightStr);

            // Act / Assert
            Assert.Equal(expectedSign < 0, left < right);
        }

        /// <summary>Verifies the &gt; operator returns the expected result for all ordered IPv4 pairs.</summary>
        /// <param name="leftStr">The left IPv4 address string.</param>
        /// <param name="rightStr">The right IPv4 address string.</param>
        /// <param name="expectedSign">The expected comparison result.</param>
        [Theory]
        [MemberData(nameof(CompareTo_Test_Data))]
        public void GreaterThan_Operator_ReturnsExpected_Test(string leftStr, string rightStr, int expectedSign)
        {
            // Arrange
            var left = WrapIPv4(leftStr);
            var right = WrapIPv4(rightStr);

            // Act / Assert
            Assert.Equal(expectedSign > 0, left > right);
        }

        /// <summary>Verifies the &lt;= operator returns the expected result for all ordered IPv4 pairs.</summary>
        /// <param name="leftStr">The left IPv4 address string.</param>
        /// <param name="rightStr">The right IPv4 address string.</param>
        /// <param name="expectedSign">The expected comparison result.</param>
        [Theory]
        [MemberData(nameof(CompareTo_Test_Data))]
        public void LessThanOrEqual_Operator_ReturnsExpected_Test(string leftStr, string rightStr, int expectedSign)
        {
            // Arrange
            var left = WrapIPv4(leftStr);
            var right = WrapIPv4(rightStr);

            // Act / Assert
            Assert.Equal(expectedSign <= 0, left <= right);
        }

        /// <summary>Verifies the &gt;= operator returns the expected result for all ordered IPv4 pairs.</summary>
        /// <param name="leftStr">The left IPv4 address string.</param>
        /// <param name="rightStr">The right IPv4 address string.</param>
        /// <param name="expectedSign">The expected comparison result.</param>
        [Theory]
        [MemberData(nameof(CompareTo_Test_Data))]
        public void GreaterThanOrEqual_Operator_ReturnsExpected_Test(string leftStr, string rightStr, int expectedSign)
        {
            // Arrange
            var left = WrapIPv4(leftStr);
            var right = WrapIPv4(rightStr);

            // Act / Assert
            Assert.Equal(expectedSign >= 0, left >= right);
        }

        #endregion // end: Operators: < > <= >=
    }
}
