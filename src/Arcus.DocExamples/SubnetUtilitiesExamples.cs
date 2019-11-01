using System.Linq;
using System.Net;
using Arcus.Utilities;
using Xunit;

namespace Arcus.DocExamples
{
    [Trait("Category", "Doc.Examples")]
    public class SubnetUtilitiesExamples
    {
        [Fact]
        public void FewestConsecutiveSubnetsFor_Example()
        {
            // Arrange
            var left = IPAddress.Parse("192.168.1.3");
            var right = IPAddress.Parse("192.168.1.5");

            // Act
            var result = SubnetUtilities.FewestConsecutiveSubnetsFor(left, right)
                                        .ToArray();

            // Assert
            Assert.Equal(2, result.Length);
            Assert.Contains(Subnet.Parse("192.168.1.4/31"), result);
            Assert.Contains(Subnet.Parse("192.168.1.3/32"), result);
        }

        [Fact]
        public void LargestSubnet_Example()
        {
            // Arrange
            var tall = Subnet.Parse("255.255.255.254/31"); // 2^1 = 2
            var grande = Subnet.Parse("192.168.1.0/24");   // 2^8 = 256
            var vente = Subnet.Parse("10.10.0.0/16");      // 2^16 = 65536
            var trenta = Subnet.Parse("16.240.0.0/12");    // 2^20 = 1048576

            var subnets = new[] {tall, grande, vente, trenta};

            // Act
            var result = SubnetUtilities.LargestSubnet(subnets);

            // Assert
            Assert.Equal(trenta, result);
        }

        [Fact]
        public void SmallestSubnet_Example()
        {
            // Arrange
            var tall = Subnet.Parse("255.255.255.254/31"); // 2^1 = 2
            var grande = Subnet.Parse("192.168.1.0/24");   // 2^8 = 256
            var vente = Subnet.Parse("10.10.0.0/16");      // 2^16 = 65536
            var trenta = Subnet.Parse("16.240.0.0/12");    // 2^20 = 1048576

            var subnets = new[] {tall, grande, vente, trenta};

            // Act
            var result = SubnetUtilities.SmallestSubnet(subnets);

            // Assert
            Assert.Equal(tall, result);
        }
    }
}
