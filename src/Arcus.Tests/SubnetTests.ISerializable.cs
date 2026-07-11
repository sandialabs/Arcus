#if NET48   // maintained for .NET 4.8 compatibility
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Arcus;
using Xunit;

namespace Arcus.Tests
{
    /// <content>
    ///     <see cref="Subnet"/> tests for <see cref="ISerializable"/>
    /// </content>
    public partial class SubnetTests
    {
        /// <summary>Gets theory data for <see cref="CanSerializable_Test"/>.</summary>
        /// <returns>Parameters: subnet to serialize (<see cref="Subnet"/>).</returns>
        public static TheoryData<Subnet> CanSerializable_Test_Values()
        {
            return new TheoryData<Subnet>
            {
                { new Subnet(IPAddress.Parse("192.168.1.0")) },
                { new Subnet(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("192.168.1.255")) },
                { new Subnet(IPAddress.Parse("::"), IPAddress.Parse("::FFFF")) },
            };
        }

        /// <summary>Verifies that a <see cref="Subnet"/> can be round-trip serialized and deserialized using <see cref="BinaryFormatter"/>.</summary>
        /// <param name="subnet">The subnet to serialize.</param>
        [Theory]
        [MemberData(nameof(CanSerializable_Test_Values))]
        public void CanSerializable_Test(Subnet subnet)
        {
            // Arrange
            var formatter = new BinaryFormatter();

            // Act
            using (var writeStream = new MemoryStream())
            {
                formatter.Serialize(writeStream, subnet);
                writeStream.Seek(0, SeekOrigin.Begin);

                // Deserialize the object from the stream
                var result = formatter.Deserialize(writeStream);

                // Assert
                var actual = Assert.IsType<Subnet>(result);

                // using explicit EqualityComparer to avoid comparing elements of enumerable
                Assert.Equal(subnet, actual, SubnetEqualityComparer.Instance);
            }
        }

        /// <summary>
        ///     S3 - <see cref="Subnet.GetObjectData"/> writes an entry named
        ///     <c>NetworkPrefixAddress</c> and does NOT write <c>BroadcastAddress</c>.
        /// </summary>
        [Fact]
        public void GetObjectData_WritesNetworkPrefixAddress_Field()
        {
            // Arrange
            var subnet = new Subnet(IPAddress.Parse("192.168.1.0"), 24);
            var info = new SerializationInfo(typeof(Subnet), new FormatterConverter());

            // Act
            subnet.GetObjectData(info, new StreamingContext(StreamingContextStates.All));

            // Assert: the new-format field is present
            var networkBytes = (byte[])info.GetValue("NetworkPrefixAddress", typeof(byte[]));
            Assert.Equal(subnet.NetworkPrefixAddress.GetAddressBytes(), networkBytes);

            // Assert: the legacy BroadcastAddress field must not be present.
            // SerializationInfo has no enumerator helper; attempt a read and expect a
            // SerializationException (entry-not-found).
            Assert.Throws<SerializationException>(
                () => info.GetValue("BroadcastAddress", typeof(byte[]))
            );
        }

        /// <summary>
        ///     S3 - <see cref="Subnet.GetObjectData"/> stamps the format version as 3.
        /// </summary>
        [Fact]
        public void GetObjectData_WritesSerVersion_Three()
        {
            // Arrange
            var subnet = new Subnet(IPAddress.Parse("10.0.0.0"), 8);
            var info = new SerializationInfo(typeof(Subnet), new FormatterConverter());

            // Act
            subnet.GetObjectData(info, new StreamingContext(StreamingContextStates.All));

            // Assert
            Assert.Equal(3, info.GetInt32("SerVersion"));
        }

        /// <summary>
        ///     S3 - round-trip via the in-process <see cref="Subnet.GetObjectData"/> /
        ///     <see cref="Subnet.DeserializeFromInfo"/> helpers reconstructs an equal subnet
        ///     for both IPv4 and IPv6 inputs.
        /// </summary>
        /// <param name="address">The network prefix address (CIDR notation host) to construct.</param>
        /// <param name="routingPrefix">The CIDR routing prefix length.</param>
        [Theory]
        [InlineData("192.168.1.0", 24)]
        [InlineData("10.0.0.0", 8)]
        [InlineData("172.16.0.0", 12)]
        [InlineData("::", 0)]
        [InlineData("2001:db8::", 32)]
        public void Subnet_Serialization_RoundTrip_Succeeds(string address, int routingPrefix)
        {
            // Arrange
            var subnet = new Subnet(IPAddress.Parse(address), routingPrefix);
            var info = new SerializationInfo(typeof(Subnet), new FormatterConverter());

            // Act
            subnet.GetObjectData(info, new StreamingContext(StreamingContextStates.All));
            var actual = Subnet.DeserializeFromInfo(info);

            // Assert
            Assert.Equal(subnet, actual, SubnetEqualityComparer.Instance);
            Assert.Equal(subnet.NetworkPrefixAddress, actual.NetworkPrefixAddress);
            Assert.Equal(subnet.RoutingPrefix, actual.RoutingPrefix);
            Assert.Equal(subnet.MaxEnumerationExponent, actual.MaxEnumerationExponent);
        }

        /// <summary>
        ///     S3 - deserialization rejects data carrying an unsupported (legacy) SerVersion,
        ///     throwing <see cref="SerializationException"/> that mentions the mismatched version.
        /// </summary>
        [Fact]
        public void DeserializeSubnet_WithWrongVersion_ThrowsSerializationException()
        {
            // Arrange - build info that mimics the legacy v2 format
            var info = new SerializationInfo(typeof(Subnet), new FormatterConverter());
            info.AddValue("NetworkPrefixAddress", IPAddress.Parse("192.168.1.0").GetAddressBytes());
            info.AddValue("RoutingPrefix", 24);
            info.AddValue("SerVersion", 2);
            info.AddValue("MaxEnumerationExponent", Subnet.DefaultMaxEnumerationExponent);

            // Act & Assert
            var ex = Assert.Throws<SerializationException>(
                () => Subnet.DeserializeFromInfo(info)
            );
            Assert.Contains("version", ex.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("2", ex.Message);
        }
    }
}
#endif
