using System.Linq;
using System.Net;
using Xunit;

namespace Arcus.DocExamples
{
    [Trait("Category", "Doc.Examples")]
    public class IPAddressRangeExamples
    {
        [Fact]
        public void TryCollapseAll_Example()
        {
            // Arrange
            var ranges = new[]
                         {
                             new IPAddressRange(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("192.168.1.5")),
                             new IPAddressRange(IPAddress.Parse("192.168.1.6"), IPAddress.Parse("192.168.1.7")),
                             new IPAddressRange(IPAddress.Parse("192.168.1.8"), IPAddress.Parse("192.168.1.20"))
                         };

            // Act
            var success = IPAddressRange.TryCollapseAll(ranges, out var results);
            var resultList = results?.ToList();

            // Assert
            Assert.True(success);
            Assert.NotNull(results);
            Assert.Single(resultList);

            var result = resultList.Single();

            Assert.Equal(IPAddress.Parse("192.168.1.0"), result.Head);
            Assert.Equal(IPAddress.Parse("192.168.1.20"), result.Tail);
        }
    }
}
