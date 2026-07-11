namespace Arcus.Tests
{
    /// <content>
    ///     <see cref="BigEndianBitWrapper"/> tests for factory methods: <c>FromBytes</c> and <c>CreateMask</c>.
    /// </content>
    public partial class BigEndianBitWrapperTests
    {
        #region FromBytes

        /// <summary>
        ///     Gets theory data for round-trip byte array tests.
        /// </summary>
        /// <value>
        ///     One row per byte array input (IPv4, IPv6, and leading-zero patterns).
        /// </value>
        public static TheoryData<byte[]> FromBytes_ValidInput_RoundTrips_Test_Data =>
            new()
            {
                // IPv4
                { new byte[] { 0, 0, 0, 0 } },
                { new byte[] { 255, 255, 255, 255 } },
                { new byte[] { 192, 168, 1, 1 } },
                { new byte[] { 10, 0, 0, 1 } },
                // IPv6
                { new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } },
                {
                    new byte[]
                    {
                        0xFF,
                        0xFF,
                        0xFF,
                        0xFF,
                        0xFF,
                        0xFF,
                        0xFF,
                        0xFF,
                        0xFF,
                        0xFF,
                        0xFF,
                        0xFF,
                        0xFF,
                        0xFF,
                        0xFF,
                        0xFF,
                    }
                },
                {
                    new byte[]
                    {
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
                    }
                },
                // Leading zeros preserved
                { new byte[] { 0x00, 0x00, 0xFF, 0xFF } },
            };

        /// <summary>Verifies FromBytes round-trips the input bytes through ByteWidth and ToBytes without loss.</summary>
        /// <param name="input">The raw byte array to wrap.</param>
        [Theory]
        [MemberData(nameof(FromBytes_ValidInput_RoundTrips_Test_Data))]
        public void FromBytes_ValidInput_RoundTrips_Test(byte[] input)
        {
            // Arrange / Act
            var wrapper = Wrap(input);

            // Assert
            Assert.Equal(input.Length, wrapper.ByteWidth);
            Assert.Equal(input, wrapper.ToBytes());
        }

        /// <summary>Verifies FromBytes throws ArgumentNullException when passed a null byte array.</summary>
        [Fact]
        public void FromBytes_NullInput_ThrowsArgumentNullException_Test()
        {
            // Act / Assert
            Assert.Throws<ArgumentNullException>(() => BigEndianBitWrapper.FromBytes(null));
        }

        /// <summary>Verifies FromBytes throws ArgumentException when passed an empty byte array.</summary>
        [Fact]
        public void FromBytes_EmptyArray_ThrowsArgumentException_Test()
        {
            // Act / Assert
            Assert.Throws<ArgumentException>(() => BigEndianBitWrapper.FromBytes([]));
        }

        /// <summary>Verifies FromBytes throws ArgumentException when the byte array is longer than 16 bytes.</summary>
        [Fact]
        public void FromBytes_ArrayLongerThan16_ThrowsArgumentException_Test()
        {
            // Act / Assert
            Assert.Throws<ArgumentException>(() => BigEndianBitWrapper.FromBytes(new byte[17]));
        }

        #endregion // end: FromBytes

        #region FromBytes(bytes, targetWidth)

        /// <summary>Verifies the target-width overload throws ArgumentNullException when bytes is null.</summary>
        [Fact]
        public void FromBytesWithTarget_NullBytes_ThrowsArgumentNullException_Test()
        {
            Assert.Throws<ArgumentNullException>(() => BigEndianBitWrapper.FromBytes(null, 4));
        }

        /// <summary>Verifies the target-width overload throws ArgumentOutOfRangeException for invalid target widths.</summary>
        /// <param name="targetWidth">An invalid target width (zero, negative, or greater than 16).</param>
        [Theory]
        [InlineData(0)]
        [InlineData(17)]
        [InlineData(-1)]
        public void FromBytesWithTarget_InvalidTargetWidth_ThrowsArgumentOutOfRangeException_Test(int targetWidth)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => BigEndianBitWrapper.FromBytes([0x01], targetWidth));
        }

        /// <summary>Verifies the target-width overload throws ArgumentException when bytes is longer than the target width.</summary>
        [Fact]
        public void FromBytesWithTarget_BytesLongerThanTarget_ThrowsArgumentException_Test()
        {
            // Arrange
            var bytes = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05 };

            // Act / Assert
            Assert.Throws<ArgumentException>(() => BigEndianBitWrapper.FromBytes(bytes, 4));
        }

        /// <summary>Verifies that FromBytes(bytes, targetWidth) produces the same result as FromBytes(bytes) when lengths match.</summary>
        [Fact]
        public void FromBytesWithTarget_SameLength_MatchesFromBytes_Test()
        {
            // Arrange
            var bytes = new byte[] { 192, 168, 1, 1 };

            // Act
            var withTarget = BigEndianBitWrapper.FromBytes(bytes, 4);
            var direct = BigEndianBitWrapper.FromBytes(bytes);

            // Assert
            Assert.Equal(direct, withTarget);
        }

        /// <summary>Verifies that a shorter byte array is zero-padded on the left to fill the target width.</summary>
        [Fact]
        public void FromBytesWithTarget_ShorterBytes_ZeroPadsOnLeft_Test()
        {
            // Arrange: one-byte value 0x01, padded to IPv4 width
            var bytes = new byte[] { 0x01 };

            // Act
            var wrapper = BigEndianBitWrapper.FromBytes(bytes, 4);

            // Assert
            Assert.Equal(4, wrapper.ByteWidth);
            Assert.Equal(new byte[] { 0x00, 0x00, 0x00, 0x01 }, wrapper.ToBytes());
        }

        /// <summary>Verifies that an empty byte array produces an all-zero wrapper of the target width.</summary>
        [Fact]
        public void FromBytesWithTarget_EmptyBytes_ProducesAllZeros_Test()
        {
            // Act
            var wrapper = BigEndianBitWrapper.FromBytes([], 4);

            // Assert
            Assert.Equal(new byte[] { 0, 0, 0, 0 }, wrapper.ToBytes());
        }

        #endregion // end: FromBytes(bytes, targetWidth)

        #region CreateMask

        /// <summary>
        ///     Gets theory data for IPv4 CreateMask tests.
        /// </summary>
        /// <value>
        ///     Rows: byteWidth (int), prefixLength (int), expected bytes (byte[]).
        /// </value>
        public static TheoryData<int, int, byte[]> CreateMask_IPv4_Test_Data =>
            new()
            {
                { 4, 0, new byte[] { 0x00, 0x00, 0x00, 0x00 } },
                { 4, 8, new byte[] { 0xFF, 0x00, 0x00, 0x00 } },
                { 4, 16, new byte[] { 0xFF, 0xFF, 0x00, 0x00 } },
                { 4, 24, new byte[] { 0xFF, 0xFF, 0xFF, 0x00 } },
                { 4, 28, new byte[] { 0xFF, 0xFF, 0xFF, 0xF0 } },
                { 4, 31, new byte[] { 0xFF, 0xFF, 0xFF, 0xFE } },
                { 4, 32, new byte[] { 0xFF, 0xFF, 0xFF, 0xFF } },
            };

        /// <summary>Verifies CreateMask generates the correct byte pattern for standard IPv4 prefix lengths.</summary>
        /// <param name="byteWidth">The byte width (4 for IPv4).</param>
        /// <param name="prefix">The prefix length in bits.</param>
        /// <param name="expected">The expected mask byte array.</param>
        [Theory]
        [MemberData(nameof(CreateMask_IPv4_Test_Data))]
        public void CreateMask_IPv4_ReturnsCorrectMask_Test(int byteWidth, int prefix, byte[] expected)
        {
            AssertCreateMask(byteWidth, prefix, expected);
        }

        /// <summary>
        ///     Gets theory data for IPv6 CreateMask tests.
        /// </summary>
        /// <value>
        ///     Rows: byteWidth (int), prefixLength (int), expected bytes (byte[]).
        /// </value>
        public static TheoryData<int, int, byte[]> CreateMask_IPv6_Test_Data =>
            new()
            {
                { 16, 0, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } },
                { 16, 64, new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0, 0, 0, 0, 0, 0, 0, 0 } },
                { 16, 96, new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0, 0, 0, 0 } },
                {
                    16,
                    128,
                    new byte[]
                    {
                        0xFF,
                        0xFF,
                        0xFF,
                        0xFF,
                        0xFF,
                        0xFF,
                        0xFF,
                        0xFF,
                        0xFF,
                        0xFF,
                        0xFF,
                        0xFF,
                        0xFF,
                        0xFF,
                        0xFF,
                        0xFF,
                    }
                },
                { 16, 32, new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } },
            };

        /// <summary>Verifies CreateMask generates the correct byte pattern for standard IPv6 prefix lengths.</summary>
        /// <param name="byteWidth">The byte width (16 for IPv6).</param>
        /// <param name="prefix">The prefix length in bits.</param>
        /// <param name="expected">The expected mask byte array.</param>
        [Theory]
        [MemberData(nameof(CreateMask_IPv6_Test_Data))]
        public void CreateMask_IPv6_ReturnsCorrectMask_Test(int byteWidth, int prefix, byte[] expected)
        {
            AssertCreateMask(byteWidth, prefix, expected);
        }

        /// <summary>Verifies CreateMask throws ArgumentOutOfRangeException for invalid byte widths.</summary>
        /// <param name="byteWidth">An invalid byte width (zero, negative, or greater than 16).</param>
        [Theory]
        [InlineData(0)]
        [InlineData(17)]
        [InlineData(-1)]
        public void CreateMask_InvalidByteWidth_ThrowsArgumentOutOfRangeException_Test(int byteWidth)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => BigEndianBitWrapper.CreateMask(byteWidth, 0));
        }

        /// <summary>Verifies CreateMask throws ArgumentOutOfRangeException when the prefix length exceeds the bit capacity of the given byte width.</summary>
        /// <param name="byteWidth">The byte width.</param>
        /// <param name="prefix">An invalid prefix length.</param>
        [Theory]
        [InlineData(4, -1)]
        [InlineData(4, 33)]
        [InlineData(16, 129)]
        [InlineData(16, -1)]
        public void CreateMask_InvalidPrefixLength_ThrowsArgumentOutOfRangeException_Test(int byteWidth, int prefix)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => BigEndianBitWrapper.CreateMask(byteWidth, prefix));
        }

        #endregion // end: CreateMask

        private static void AssertCreateMask(int byteWidth, int prefix, byte[] expected)
        {
            var mask = BigEndianBitWrapper.CreateMask(byteWidth, prefix);

            Assert.Equal(byteWidth, mask.ByteWidth);
            Assert.Equal(expected, mask.ToBytes());
        }
    }
}
