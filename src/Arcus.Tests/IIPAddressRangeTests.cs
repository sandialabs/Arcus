using System.Net;

namespace Arcus.Tests
{
    /// <summary>Unit tests for <see cref="IIPAddressRange"/>.</summary>
    public class IIPAddressRangeTests
    {
        /// <summary>Verifies IIPAddressRange extends IFormattable and IEnumerable&lt;IPAddress&gt;.</summary>
        [Fact]
        public void Assignability_Test()
        {
            // Arrange
            var type = typeof(IIPAddressRange);

            // Act
            // Assert
            Assert.True(typeof(IFormattable).IsAssignableFrom(type));
            Assert.True(typeof(IEnumerable<IPAddress>).IsAssignableFrom(type));
        }

        /// <summary>Verifies IIPAddressRange is declared as an interface.</summary>
        [Fact]
        public void IsInterface_Test()
        {
            // Arrange
            var type = typeof(IIPAddressRange);

            // Act
            var typeIsInterface = type.IsInterface;

            // Assert
            Assert.True(typeIsInterface);
        }
    }
}
