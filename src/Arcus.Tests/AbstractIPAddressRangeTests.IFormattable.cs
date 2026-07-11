using System.Globalization;
using System.Net;

namespace Arcus.Tests
{
    /// <content>
    ///     <see cref="AbstractIPAddressRange"/> tests for <see cref="IFormattable"/>
    /// </content>
    public partial class AbstractIPAddressRangeTests
    {
        #region Formatting

        /// <summary>Verifies ToString with a general format specifier returns the Head-dash-Tail representation.</summary>
        /// <param name="headString">Head address string.</param>
        /// <param name="tailString">Tail address string.</param>
        /// <param name="format">Format string for the output representation.</param>
        /// <param name="expected">Expected formatted string.</param>
        [Theory]
        [InlineData("192.168.1.1", "192.168.1.5", null, "192.168.1.1 - 192.168.1.5")]
        [InlineData("192.168.1.1", "192.168.1.5", "", "192.168.1.1 - 192.168.1.5")]
        [InlineData("192.168.1.1", "192.168.1.5", "g", "192.168.1.1 - 192.168.1.5")]
        [InlineData("192.168.1.1", "192.168.1.5", "G", "192.168.1.1 - 192.168.1.5")]
        [InlineData("::beef", "::dead", null, "::beef - ::dead")]
        [InlineData("::beef", "::dead", "", "::beef - ::dead")]
        [InlineData("::beef", "::dead", "g", "::beef - ::dead")]
        [InlineData("::beef", "::dead", "G", "::beef - ::dead")]
        public void ToString_Format_ReturnsHeadDashTail_Test(
            string headString,
            string tailString,
            string format,
            string expected
        )
        {
            // Arrange
            var head = IPAddress.Parse(headString);
            var tail = IPAddress.Parse(tailString);
            var iPAddressRange = new IPAddressRange(head, tail);

            // Act
            var result = iPAddressRange.ToString(format, CultureInfo.InvariantCulture);

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>Verifies the no-arg ToString returns the Head-dash-Tail representation.</summary>
        [Fact]
        public void ToString_NoArgs_ReturnsHeadDashTail_Test()
        {
            // Arrange
            var head = IPAddress.Parse("10.0.0.1");
            var tail = IPAddress.Parse("10.0.0.255");
            var iPAddressRange = new IPAddressRange(head, tail);

            // Act
            var result = iPAddressRange.ToString();

            // Assert
            Assert.Equal("10.0.0.1 - 10.0.0.255", result);
        }

        /// <summary>Verifies ToString throws FormatException for an unrecognized format specifier.</summary>
        /// <param name="headString">Head address string.</param>
        /// <param name="tailString">Tail address string.</param>
        /// <param name="format">Unrecognized format specifier.</param>
        [Theory]
        [InlineData("192.168.1.1", "192.168.1.5", "X")]
        [InlineData("192.168.1.1", "192.168.1.5", "R")]
        [InlineData("::beef", "::dead", "Z")]
        public void ToString_UnknownFormat_Throws_FormatException_Test(string headString, string tailString, string format)
        {
            // Arrange
            var head = IPAddress.Parse(headString);
            var tail = IPAddress.Parse(tailString);
            var iPAddressRange = new IPAddressRange(head, tail);

            // Act
            // Assert
            Assert.Throws<FormatException>(() => iPAddressRange.ToString(format, CultureInfo.InvariantCulture));
        }

        #endregion // end: Formatting
    }
}
