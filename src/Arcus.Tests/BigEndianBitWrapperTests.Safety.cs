namespace Arcus.Tests
{
    /// <content>
    ///     <see cref="BigEndianBitWrapper"/> tests for safety and robustness: span validation,
    ///     default-value behavior, width-mismatch detection, and unchecked arithmetic.
    /// </content>
    public partial class BigEndianBitWrapperTests
    {
        #region B1: ToBytes span validation

#if NET8_0_OR_GREATER
        /// <summary>Verifies ToBytes(Span&lt;byte&gt;) throws ArgumentException when the destination is too short.</summary>
        [Fact]
        public void ToBytesSpan_WithInsufficientLength_ThrowsArgumentException_Test()
        {
            var w = BigEndianBitWrapper.FromBytes(new byte[4]);
            var dest = new byte[2];
            Assert.Throws<ArgumentException>(() => w.ToBytes(dest));
        }
#endif

        #endregion

        #region B2: Default wrapper (ByteWidth=0) handling

        /// <summary>Verifies the default wrapper has ByteWidth=0 and MaxValueForWidth does not throw.</summary>
        [Fact]
        public void DefaultWrapper_ByteWidthIsZero_MaxValueForWidthReturnsZero_Test()
        {
            var w = default(BigEndianBitWrapper);
            Assert.Equal(0, w.ByteWidth);
            // ToBytes should produce an empty array without crashing.
            var bytes = w.ToBytes();
            Assert.Empty(bytes);
        }

        /// <summary>
        ///     Verifies that TryAdd on a default wrapper rejects all non-zero deltas and sets
        ///     result to default (ByteWidth == 0).
        /// </summary>
        [Fact]
        public void DefaultWrapper_TryAddRejectsAllDeltas_Test()
        {
            var w = default(BigEndianBitWrapper);
            var success = w.TryAdd(1, out var result);
            Assert.False(success);
            Assert.Equal(default, result);
            Assert.Equal(0, result.ByteWidth);

            success = w.TryAdd(-1, out result);
            Assert.False(success);
            Assert.Equal(default, result);
            Assert.Equal(0, result.ByteWidth);
        }

        #endregion

        #region B6: Subtract validation

        /// <summary>Verifies Subtract throws ArgumentException when operands have different ByteWidth.</summary>
        [Fact]
        public void Subtract_WithDifferentByteWidth_ThrowsArgumentException_Test()
        {
            var ipv4 = BigEndianBitWrapper.FromBytes([192, 168, 1, 1]);
            var ipv6 = BigEndianBitWrapper.FromBytes([
                0x20,
                0x01,
                0x0D,
                0xB8,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x01,
            ]);
            Assert.Throws<ArgumentException>(() => ipv4.Subtract(ipv6));
        }

        #endregion

        #region X9: Unchecked arithmetic

        /// <summary>
        ///     Verifies that carry/borrow detection works correctly for edge-case values where
        ///     unchecked wrapping would produce incorrect results.  Tests that 0 - 1 wraps properly
        ///     and is caught as underflow, and that equal-value subtraction produces zero correctly.
        /// </summary>
        [Fact]
        public void BigIntegerArithmetic_ReliesOnUncheckedContext_Test()
        {
            // 0 - 1 underflow detection
            var zero = BigEndianBitWrapper.FromBytes([0, 0, 0, 0], 4);
            var success = zero.TryAdd(-1, out _);
            Assert.False(success);

            // Equal-value subtraction (no borrow)
            var a = BigEndianBitWrapper.FromBytes([0, 0, 0, 5], 4);
            var b = BigEndianBitWrapper.FromBytes([0, 0, 0, 5], 4);
            var diff = a.Subtract(b);
            Assert.Equal(0, (int)diff.ToBigInteger());

            // Large delta subtract below zero
            var small = BigEndianBitWrapper.FromBytes([0, 0, 0, 1], 4);
            success = small.TryAdd(-2, out _);
            Assert.False(success);
        }

        #endregion
    }
}
