using System.Net;
using Xunit;

namespace Arcus.DocExamples
{
    [Trait("Category", "Doc.Examples")]
    public class AbstractIPAddressRangeExamples
    {
        [Fact]
        public void IFormattable_Example()
        {
            // Arrange
            var head = IPAddress.Parse("192.168.0.0");
            var tail = IPAddress.Parse("192.168.128.0");
            var ipAddressRange = new IPAddressRange(head, tail);

            const string expected = "192.168.0.0 - 192.168.128.0";

            // Act
            var formattableString = string.Format("{0:g}", ipAddressRange);

            // Assert
            Assert.Equal(expected, formattableString);
        }
    }
}
