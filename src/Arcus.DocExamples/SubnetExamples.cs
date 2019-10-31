using System.Linq;
using System.Net;
using Xunit;

namespace Arcus.DocExamples
{
    /// <summary>
    ///     Subnet Examples
    /// </summary>
    [Trait("Category", "Doc.Examples")]
    public class SubnetExamples
    {
        [Fact]
        public void Address_RoutePrefix_Subnet_Example()
        {
            // Arrange
            var ipAddress = IPAddress.Parse("192.168.1.1");
            const int routePrefix = 24;

            // Act
            var subnet = new Subnet(ipAddress, routePrefix);

            // Assert
            Assert.False(subnet.IsSingleIP);

            Assert.Equal(256, subnet.Length);
            Assert.Equal("192.168.1.0", subnet.Head.ToString());
            Assert.Equal("192.168.1.255", subnet.Tail.ToString());
            Assert.Equal(24, subnet.RoutingPrefix);
            Assert.Equal("192.168.1.0/24", subnet.ToString());
        }

        [Fact]
        public void Contains_Example()
        {
            // Arrange
            var subnetA = Subnet.Parse("192.168.1.0", 8);  // 192.0.0.0 - 192.255.255.255
            var subnetB = Subnet.Parse("192.168.0.0", 16); // 192.168.0.0 - 192.168.255.255
            var subnetC = Subnet.Parse("255.0.0.0", 8);    // 255.0.0.0 - 255.255.255.255

            // Act
            // Assert
            Assert.True(subnetA.Contains(subnetB));
            Assert.False(subnetA.Contains(subnetC));
        }

        [Fact]
        public void Overlaps_Example()
        {
            // Arrange
            var ipv4SubnetA = Subnet.Parse("255.255.0.0", 16);
            var ipv4SubnetB = Subnet.Parse("0.0.0.0", 0);

            var ipv6SubnetA = Subnet.Parse("::", 0);
            var ipv6SubnetB = Subnet.Parse("abcd:ef01::", 64);

            // Act
            Assert.True(ipv4SubnetA.Overlaps(ipv4SubnetB));
            Assert.True(ipv4SubnetB.Overlaps(ipv4SubnetA));

            Assert.True(ipv6SubnetA.Overlaps(ipv6SubnetB));

            Assert.False(ipv6SubnetA.Overlaps(ipv4SubnetA));
        }

        [Fact]
        public void Single_Address_Subnet_Example()
        {
            // Arrange
            var ipAddress = IPAddress.Parse("192.168.1.1");

            // Act
            var subnet = new Subnet(ipAddress);

            // Assert
            Assert.Equal(1, subnet.Length);
            Assert.Equal(ipAddress, subnet.Single());
            Assert.True(subnet.IsSingleIP);
            Assert.Equal("192.168.1.1/32", subnet.ToString());
        }
    }
}
