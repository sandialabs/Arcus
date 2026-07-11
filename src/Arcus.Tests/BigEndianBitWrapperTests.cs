using System.Globalization;
using System.Net;
using System.Reflection;

namespace Arcus.Tests
{
    /// <summary>Unit tests for <see cref="BigEndianBitWrapper"/>.</summary>
    public partial class BigEndianBitWrapperTests
    {
        // Convenience helpers - keep tests focused on behaviour, not construction noise.
        private static BigEndianBitWrapper Wrap(params byte[] bytes)
        {
            return BigEndianBitWrapper.FromBytes(bytes);
        }

        private static BigEndianBitWrapper WrapIPv4(string dotted)
        {
            return Wrap(IPAddress.Parse(dotted).GetAddressBytes());
        }

        private static BigEndianBitWrapper WrapIPv6(string addr)
        {
            return Wrap(IPAddress.Parse(addr).GetAddressBytes());
        }

        #region TryAdd

        /// <summary>Verifies TryAdd with delta zero returns true and leaves the value unchanged.</summary>
        [Fact]
        public void TryAdd_DeltaZero_ReturnsTrueWithSameValue_Test()
        {
            // Arrange
            var w = WrapIPv4("10.0.0.1");

            // Act
            var success = w.TryAdd(0, out var result);

            // Assert
            Assert.True(success);
            Assert.Equal(w, result);
        }

        /// <summary>Verifies TryAdd with a positive delta of one increments to the next address.</summary>
        [Fact]
        public void TryAdd_PositiveDelta_IncrementsByDelta_Test()
        {
            // Arrange
            var w = WrapIPv4("10.0.0.1");

            // Act
            var success = w.TryAdd(1, out var result);

            // Assert
            Assert.True(success);
            Assert.Equal(WrapIPv4("10.0.0.2"), result);
        }

        /// <summary>Verifies TryAdd with a large positive delta produces the correctly incremented value.</summary>
        [Fact]
        public void TryAdd_PositiveDeltaLarge_IncrementsByDelta_Test()
        {
            // Arrange
            var w = WrapIPv4("10.0.0.1");

            // Act
            var success = w.TryAdd(254, out var result);

            // Assert
            Assert.True(success);
            Assert.Equal(WrapIPv4("10.0.0.255"), result);
        }

        /// <summary>Verifies TryAdd with a negative delta decrements the address by the given amount.</summary>
        [Fact]
        public void TryAdd_NegativeDelta_DecrementsByDelta_Test()
        {
            // Arrange
            var w = WrapIPv4("10.0.0.5");

            // Act
            var success = w.TryAdd(-3, out var result);

            // Assert
            Assert.True(success);
            Assert.Equal(WrapIPv4("10.0.0.2"), result);
        }

        /// <summary>Verifies TryAdd returns false and default when incrementing past the IPv4 maximum address.</summary>
        [Fact]
        public void TryAdd_PositiveDelta_IPv4MaxAddress_ReturnsFalse_Test()
        {
            // Arrange: 255.255.255.255 + 1 overflows IPv4
            var w = WrapIPv4("255.255.255.255");

            // Act
            var success = w.TryAdd(1, out var result);

            // Assert
            Assert.False(success);
            Assert.Equal(default, result);
        }

        /// <summary>Verifies TryAdd returns false and default when decrementing below the IPv4 minimum address.</summary>
        [Fact]
        public void TryAdd_NegativeDelta_OnZeroAddress_ReturnsFalse_Test()
        {
            // Arrange: 0.0.0.0 - 1 underflows
            var w = WrapIPv4("0.0.0.0");

            // Act
            var success = w.TryAdd(-1, out var result);

            // Assert
            Assert.False(success);
            Assert.Equal(default, result);
        }

        /// <summary>Verifies TryAdd returns false when the delta exceeds the maximum value representable by the byte width.</summary>
        [Fact]
        public void TryAdd_PositiveDeltaExceedsWidthBound_ReturnsFalse_Test()
        {
            // Arrange: any IPv4 + 2^32 overflows because delta > max for 4 bytes
            var w = WrapIPv4("0.0.0.0");

            // Act
            var success = w.TryAdd(4294967296L, out _); // 2^32

            // Assert
            Assert.False(success);
        }

        /// <summary>Verifies TryAdd returns false when incrementing past the IPv6 maximum address.</summary>
        [Fact]
        public void TryAdd_IPv6_MaxAddress_Overflow_ReturnsFalse_Test()
        {
            // Arrange
            var maxIPv6 = WrapIPv6("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff");

            // Act
            var success = maxIPv6.TryAdd(1, out _);

            // Assert
            Assert.False(success);
        }

        /// <summary>Verifies TryAdd returns false when decrementing below the IPv6 minimum address.</summary>
        [Fact]
        public void TryAdd_IPv6_ZeroAddress_Underflow_ReturnsFalse_Test()
        {
            // Arrange
            var zeroIPv6 = WrapIPv6("::");

            // Act
            var success = zeroIPv6.TryAdd(-1, out _);

            // Assert
            Assert.False(success);
        }

        /// <summary>
        ///     Verifies TryAdd with <see cref="long.MinValue"/> as delta correctly decrements an IPv6 address
        ///     whose numeric value is >= the magnitude of <c>long.MinValue</c>.
        ///     This exercises the <c>delta == long.MinValue</c> special-case branch in <see cref="BigEndianBitWrapper.TryAdd"/>
        ///     which avoids <c>-(long.MinValue)</c> overflow.
        /// </summary>
        [Fact]
        public void TryAdd_DeltaLongMinValue_IPv6_DecrementsByMagnitude_Test()
        {
            // Arrange: an IPv6 address with numeric value exactly 2^63 (0x8000_0000_0000_0000)
            // Byte layout (big-endian, 16 bytes): 8 zero bytes, 0x80, 7 zero bytes
            var bytes = new byte[16];
            bytes[8] = 0x80;
            var w = Wrap(bytes);

            // Act: decrement by -long.MinValue (magnitude = 2^63)
            var success = w.TryAdd(long.MinValue, out var result);

            // Assert: value goes to 0
            Assert.True(success);
            Assert.Equal(Wrap(new byte[16]), result);
        }

        /// <summary>
        ///     Verifies TryAdd with <see cref="long.MinValue"/> returns false when the value is smaller
        ///     than the magnitude of <c>long.MinValue</c> (i.e. underflow).
        /// </summary>
        [Fact]
        public void TryAdd_DeltaLongMinValue_Underflow_ReturnsFalse_Test()
        {
            // Arrange: IPv4 address has value way less than 2^63
            var w = WrapIPv4("10.0.0.1");

            // Act
            var success = w.TryAdd(long.MinValue, out _);

            // Assert
            Assert.False(success);
        }

        /// <summary>Verifies TryAdd correctly increments across a 16-bit word boundary in an IPv6 address.</summary>
        [Fact]
        public void TryAdd_IPv6_IncrementAcrossWordBoundary_Test()
        {
            // Arrange: ::ffff + 1 = ::1:0000
            var w = WrapIPv6("::ffff");

            // Act
            var success = w.TryAdd(1, out var result);

            // Assert
            Assert.True(success);
            Assert.Equal(WrapIPv6("::1:0"), result);
        }

        /// <summary>Verifies TryAdd correctly increments across a byte rollover boundary in an IPv4 address.</summary>
        [Fact]
        public void TryAdd_IPv4_IncrementAcrossByteRollover_Test()
        {
            // Arrange: 10.0.0.255 + 1 = 10.0.1.0
            var w = WrapIPv4("10.0.0.255");

            // Act
            var success = w.TryAdd(1, out var result);

            // Assert
            Assert.True(success);
            Assert.Equal(WrapIPv4("10.0.1.0"), result);
        }

        #endregion // end: TryAdd

        #region Subtract

        /// <summary>Verifies Subtract returns the correct numeric difference between two IPv4 addresses.</summary>
        [Fact]
        public void Subtract_BasicDifference_ReturnsExpectedValue_Test()
        {
            // Arrange
            var tail = WrapIPv4("10.0.0.10");
            var head = WrapIPv4("10.0.0.1");

            // Act
            var diff = tail.Subtract(head);

            // Assert - difference is 9
            Assert.Equal(9, (int)diff.ToBigInteger());
        }

        /// <summary>Verifies Subtract returns zero when both operands are equal.</summary>
        [Fact]
        public void Subtract_EqualValues_ReturnsZero_Test()
        {
            // Arrange
            var w = WrapIPv4("192.168.1.1");

            // Act
            var diff = w.Subtract(w);

            // Assert
            Assert.Equal(0, (int)diff.ToBigInteger());
            Assert.Equal(new byte[] { 0, 0, 0, 0 }, diff.ToBytes());
        }

        /// <summary>Verifies Subtract throws InvalidOperationException when the result would be negative.</summary>
        [Fact]
        public void Subtract_Underflow_ThrowsInvalidOperationException_Test()
        {
            // Arrange
            var small = WrapIPv4("10.0.0.1");
            var large = WrapIPv4("10.0.0.100");

            // Act / Assert
            Assert.Throws<InvalidOperationException>(() => small.Subtract(large));
        }

        /// <summary>Verifies Subtract returns the correct BigInteger difference for a large IPv6 range.</summary>
        [Fact]
        public void Subtract_IPv6_LargeRange_ReturnsExpectedBigInteger_Test()
        {
            // Arrange: ::ffff:ffff - ::0 = 4294967295
            var tail = WrapIPv6("::ffff:ffff");
            var head = WrapIPv6("::");

            // Act
            var diff = tail.Subtract(head);

            // Assert
            Assert.Equal(System.Numerics.BigInteger.Parse("4294967295", CultureInfo.InvariantCulture), diff.ToBigInteger());
        }

        #endregion // end: Subtract

        #region ToBigInteger

        /// <summary>
        ///     Gets parameters: input (byte[]), expected (long).
        /// </summary>
        /// <value>
        ///     Parameters: input (byte[]), expected (long).
        /// </value>
        public static TheoryData<byte[], long> ToBigInteger_KnownValues_Test_Data =>
            new()
            {
                { new byte[] { 0, 0, 0, 0 }, 0L },
                { new byte[] { 0, 0, 0, 1 }, 1L },
                { new byte[] { 0, 0, 1, 0 }, 256L },
                { new byte[] { 1, 0, 0, 0 }, 16777216L },
                { new byte[] { 255, 255, 255, 255 }, 4294967295L },
                { new byte[] { 192, 168, 1, 1 }, 3232235777L },
            };

        /// <summary>Verifies ToBigInteger returns the expected unsigned big-endian value for known IPv4 byte patterns.</summary>
        /// <param name="input">The raw bytes to wrap.</param>
        /// <param name="expected">The expected integer value.</param>
        [Theory]
        [MemberData(nameof(ToBigInteger_KnownValues_Test_Data))]
        public void ToBigInteger_KnownIPv4Values_ReturnsExpected_Test(byte[] input, long expected)
        {
            // Arrange
            var w = Wrap(input);

            // Act
            var result = w.ToBigInteger();

            // Assert
            Assert.Equal(new System.Numerics.BigInteger(expected), result);
        }

        /// <summary>Verifies ToBigInteger returns zero for the IPv6 all-zeros address.</summary>
        [Fact]
        public void ToBigInteger_ZeroIPv6_ReturnsZero_Test()
        {
            // Act
            var result = WrapIPv6("::").ToBigInteger();

            // Assert
            Assert.Equal(System.Numerics.BigInteger.Zero, result);
        }

        /// <summary>Verifies ToBigInteger always returns a non-negative value (unsigned interpretation).</summary>
        [Fact]
        public void ToBigInteger_IsNonNegative_Test()
        {
            // Arrange: max IPv4 value
            var w = WrapIPv4("255.255.255.255");

            // Act
            var result = w.ToBigInteger();

            // Assert
            Assert.True(result >= 0);
        }

        #endregion // end: ToBigInteger

        #region ByteWidth preservation

        /// <summary>Verifies ByteWidth is preserved through bitwise NOT, TryAdd increment, and bitwise AND operations.</summary>
        /// <param name="byteWidth">The byte width to test.</param>
        [Theory]
        [InlineData(4)]
        [InlineData(6)]
        [InlineData(16)]
        public void ByteWidth_IsPreservedAcrossOperations_Test(int byteWidth)
        {
            // Arrange
            var bytes = new byte[byteWidth];
            bytes[byteWidth - 1] = 0x01;
            var w = Wrap(bytes);

            // Act: put the wrapper through multiple operations
            var notW = ~w;
            w.TryAdd(1, out var incremented);
            var andResult = w & notW;

            // Assert: ByteWidth is preserved through all operations
            Assert.Equal(byteWidth, notW.ByteWidth);
            Assert.Equal(byteWidth, incremented.ByteWidth);
            Assert.Equal(byteWidth, andResult.ByteWidth);
        }

        #endregion // end: ByteWidth preservation

        #region Subnet operations (combined)

        /// <summary>Verifies that applying a /24 mask to a host address correctly computes the network and broadcast addresses for IPv4.</summary>
        [Fact]
        public void SubnetOps_ComputeNetworkAndBroadcast_IPv4_Test()
        {
            // Reproduces the NormalizeAndCreateNetMask pattern from Subnet.cs
            // Arrange: host address 192.168.1.100/24
            var headBytes = IPAddress.Parse("192.168.1.100").GetAddressBytes();
            var head = Wrap(headBytes);
            var mask = BigEndianBitWrapper.CreateMask(4, 24);

            // Act
            var network = (head & mask).ToBytes();
            var broadcast = (head | ~mask).ToBytes();

            // Assert
            Assert.Equal(IPAddress.Parse("192.168.1.0").GetAddressBytes(), network);
            Assert.Equal(IPAddress.Parse("192.168.1.255").GetAddressBytes(), broadcast);
        }

        /// <summary>Verifies that applying a /64 mask to a host address correctly computes the network and broadcast addresses for IPv6.</summary>
        [Fact]
        public void SubnetOps_ComputeNetworkAndBroadcast_IPv6_Test()
        {
            // Arrange: 2001:db8::1/64
            var headBytes = IPAddress.Parse("2001:db8::1").GetAddressBytes();
            var head = Wrap(headBytes);
            var mask = BigEndianBitWrapper.CreateMask(16, 64);

            // Act
            var network = (head & mask).ToBytes();
            var broadcast = (head | ~mask).ToBytes();

            // Assert
            Assert.Equal(IPAddress.Parse("2001:db8::").GetAddressBytes(), network);
            Assert.Equal(IPAddress.Parse("2001:db8::ffff:ffff:ffff:ffff").GetAddressBytes(), broadcast);
        }

        #endregion // end: Subnet operations (combined)

        #region DebuggerDisplay

        /// <summary>Verifies the debugger display format for an IPv4 address produces the expected hex-with-underscore representation.</summary>
        [Fact]
        public void DebuggerDisplay_IPv4_ReturnsExpectedFormat_Test()
        {
            // Arrange
            var w = WrapIPv4("192.168.1.1");
            var prop = typeof(BigEndianBitWrapper).GetProperty(
                "DebuggerDisplay",
                BindingFlags.Instance | BindingFlags.NonPublic
            );

            // Act
            var value = prop!.GetValue(w) as string;

            // Assert
            Assert.Equal("0xC0_A8_01_01 (4 bytes)", value);
        }

        /// <summary>Verifies the debugger display format for an IPv6 address produces the expected hex-with-underscore representation.</summary>
        [Fact]
        public void DebuggerDisplay_IPv6_ReturnsExpectedFormat_Test()
        {
            // Arrange
            var w = WrapIPv6("2001:db8::1");
            var prop = typeof(BigEndianBitWrapper).GetProperty(
                "DebuggerDisplay",
                BindingFlags.Instance | BindingFlags.NonPublic
            );

            // Act
            var value = prop!.GetValue(w) as string;

            // Assert: 16 bytes in a 128-bit IPv6 address
            // Format: "0x" + 47 hex-and-underscore chars + " (16 bytes)" = 60
            Assert.EndsWith("(16 bytes)", value, StringComparison.Ordinal);
            Assert.StartsWith("0x", value, StringComparison.Ordinal);
            Assert.Equal(60, value!.Length);
        }

        #endregion // end: DebuggerDisplay
    }
}
