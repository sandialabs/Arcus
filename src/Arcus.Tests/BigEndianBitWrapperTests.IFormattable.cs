using System.Globalization;

namespace Arcus.Tests
{
    /// <content>
    ///     <see cref="BigEndianBitWrapper"/> tests for <see cref="System.IFormattable"/> and string conversion methods.
    /// </content>
    public partial class BigEndianBitWrapperTests
    {
        #region ToHexString / ToString("HC")

        /// <summary>
        ///     Gets theory data for ToHexString known-value tests.
        /// </summary>
        /// <value>
        ///     Rows: input bytes (byte[]), expected uppercase hex string (string).
        /// </value>
        public static TheoryData<byte[], string> ToHexString_KnownValues_Test_Data =>
            new()
            {
                { new byte[] { 0, 0, 0, 0 }, "00000000" },
                { new byte[] { 255, 255, 255, 255 }, "FFFFFFFF" },
                { new byte[] { 192, 168, 1, 1 }, "C0A80101" },
                { new byte[] { 10, 0, 0, 1 }, "0A000001" },
                {
                    new byte[] { 0x20, 0x01, 0x0D, 0xB8, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
                    "20010DB8000000000000000000000001"
                },
            };

        /// <summary>Verifies ToHexString produces an uppercase two-character-per-byte hex string for known inputs.</summary>
        /// <param name="input">The raw bytes to wrap.</param>
        /// <param name="expected">The expected uppercase hex string.</param>
        [Theory]
        [MemberData(nameof(ToHexString_KnownValues_Test_Data))]
        public void ToHexString_KnownValues_ReturnsUppercaseHex_Test(byte[] input, string expected)
        {
            // Act
            var result = Wrap(input).ToHexString();

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>Verifies ToHexString produces a string of exactly twice the byte width.</summary>
        [Fact]
        public void ToHexString_LengthIsDoubleByteWidth_Test()
        {
            // Arrange / Act
            var result = WrapIPv4("192.168.1.1").ToHexString();

            // Assert
            Assert.Equal(8, result.Length); // 4 bytes × 2
        }

        /// <summary>Verifies ToString("HC", …) produces the same output as ToHexString().</summary>
        [Fact]
        public void ToString_FormatHC_MatchesToHexString_Test()
        {
            // Arrange
            var w = WrapIPv4("192.168.1.1");

            // Act / Assert
            Assert.Equal(w.ToHexString(), w.ToString("HC", CultureInfo.InvariantCulture));
        }

        /// <summary>Verifies ToString(null, null) falls back to the hex representation.</summary>
        [Fact]
        public void ToString_NullFormat_ReturnsHex_Test()
        {
            // Arrange
            var w = WrapIPv4("192.168.1.1");

            // Act / Assert
            Assert.Equal(w.ToHexString(), w.ToString(null, null));
        }

        /// <summary>Verifies ToString("G", null) returns the hex representation.</summary>
        [Fact]
        public void ToString_FormatG_ReturnsHex_Test()
        {
            // Arrange
            var w = WrapIPv4("192.168.1.1");

            // Act / Assert
            Assert.Equal(w.ToHexString(), w.ToString("G", null));
        }

        /// <summary>Verifies ToString() with no arguments returns the uppercase hex string.</summary>
        [Fact]
        public void ToString_NoArgs_ReturnsHex_Test()
        {
            // Arrange
            var w = WrapIPv4("192.168.1.1");

            // Act / Assert
            Assert.Equal("C0A80101", w.ToString());
        }

        #endregion // end: ToHexString / ToString("HC")

        #region ToDecimalString / ToString("IBE")

        /// <summary>
        ///     Gets theory data for ToDecimalString known-value tests.
        /// </summary>
        /// <value>
        ///     Rows: input bytes (byte[]), expected decimal string (string).
        /// </value>
        public static TheoryData<byte[], string> ToDecimalString_KnownValues_Test_Data =>
            new()
            {
                { new byte[] { 0, 0, 0, 0 }, "0" },
                { new byte[] { 0, 0, 0, 1 }, "1" },
                { new byte[] { 255, 255, 255, 255 }, "4294967295" },
                { new byte[] { 192, 168, 1, 1 }, "3232235777" },
            };

        /// <summary>Verifies ToDecimalString produces the correct unsigned base-10 representation for known inputs.</summary>
        /// <param name="input">The raw bytes to wrap.</param>
        /// <param name="expected">The expected decimal string.</param>
        [Theory]
        [MemberData(nameof(ToDecimalString_KnownValues_Test_Data))]
        public void ToDecimalString_KnownValues_ReturnsDecimalString_Test(byte[] input, string expected)
        {
            // Act
            var result = Wrap(input).ToDecimalString();

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>Verifies ToString("IBE", …) produces the same output as ToDecimalString().</summary>
        [Fact]
        public void ToString_FormatIBE_MatchesToDecimalString_Test()
        {
            // Arrange
            var w = WrapIPv4("192.168.1.1");

            // Act / Assert
            Assert.Equal(w.ToDecimalString(), w.ToString("IBE", CultureInfo.InvariantCulture));
        }

        #endregion // end: ToDecimalString / ToString("IBE")

        #region ToBinaryString / ToString("b")

        /// <summary>Verifies ToBinaryString returns an 8-character string of '0' and '1' for a single byte.</summary>
        [Fact]
        public void ToBinaryString_SingleByte_ReturnsEightCharacters_Test()
        {
            // Arrange
            var w = Wrap(0x0F);

            // Act
            var result = w.ToBinaryString();

            // Assert
            Assert.Equal("00001111", result);
        }

        /// <summary>Verifies ToBinaryString returns 32 zeros for the all-zero IPv4 address.</summary>
        [Fact]
        public void ToBinaryString_AllZerosIPv4_AllZeroChars_Test()
        {
            // Act
            var result = WrapIPv4("0.0.0.0").ToBinaryString();

            // Assert
            Assert.Equal(new string('0', 32), result);
        }

        /// <summary>Verifies ToBinaryString returns 32 ones for the all-ones IPv4 address.</summary>
        [Fact]
        public void ToBinaryString_AllOnesIPv4_AllOneChars_Test()
        {
            // Act
            var result = WrapIPv4("255.255.255.255").ToBinaryString();

            // Assert
            Assert.Equal(new string('1', 32), result);
        }

        /// <summary>Verifies ToBinaryString length is exactly 8 times the byte width for both IPv4 and IPv6.</summary>
        [Fact]
        public void ToBinaryString_LengthIsEightTimesWidth_Test()
        {
            // Arrange / Act
            var ipv4Result = WrapIPv4("10.0.0.1").ToBinaryString();
            var ipv6Result = WrapIPv6("::1").ToBinaryString();

            // Assert
            Assert.Equal(32, ipv4Result.Length); // 4 × 8
            Assert.Equal(128, ipv6Result.Length); // 16 × 8
        }

        /// <summary>Verifies ToBinaryString output contains only '0' and '1' characters.</summary>
        [Fact]
        public void ToBinaryString_ContainsOnlyZeroAndOne_Test()
        {
            // Act
            var result = WrapIPv4("192.168.1.1").ToBinaryString();

            // Assert
            Assert.All(result, c => Assert.True(c is '0' or '1'));
        }

        /// <summary>Verifies ToString("b", null) produces the same output as ToBinaryString().</summary>
        [Fact]
        public void ToString_FormatB_MatchesToBinaryString_Test()
        {
            // Arrange
            var w = WrapIPv4("192.168.1.1");

            // Act / Assert
            Assert.Equal(w.ToBinaryString(), w.ToString("b", null));
        }

        #endregion // end: ToBinaryString / ToString("b")

        #region ToString unknown format

        /// <summary>Verifies ToString throws FormatException for an unrecognized format string.</summary>
        [Fact]
        public void ToString_UnknownFormat_ThrowsFormatException_Test()
        {
            // Arrange
            var w = WrapIPv4("10.0.0.1");

            // Act / Assert
            Assert.Throws<System.FormatException>(() => w.ToString("X", null));
        }

        #endregion // end: ToString unknown format
    }
}
