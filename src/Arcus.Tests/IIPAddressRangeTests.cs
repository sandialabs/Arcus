using System;
using System.Collections.Generic;
using System.Net;
using Xunit;

namespace Arcus.Tests
{
    public class IIPAddressRangeTests
    {
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
