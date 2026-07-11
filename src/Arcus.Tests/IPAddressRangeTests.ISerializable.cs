#if NET48
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Arcus.Tests
{
    /// <content>
    ///     <see cref="IPAddressRange"/> tests for <see cref="ISerializable"/>
    /// </content>
    public partial class IPAddressRangeTests
    {
        #region ISerializable

        /// <summary>
        ///     Gets theory data for binary serialization round-trip tests.
        /// </summary>
        /// <value>
        ///     A set of <see cref="IPAddressRange"/> instances covering single-address, IPv4, and IPv6 ranges.
        /// </value>
        public static TheoryData<IPAddressRange> CanSerializable_Test_Values =>
            [
                new IPAddressRange(IPAddress.Parse("192.168.1.0")),
                new IPAddressRange(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("192.168.1.255")),
                new IPAddressRange(IPAddress.Parse("::"), IPAddress.Parse("::FFFF:4321")),
            ];

        /// <summary>Verifies an IPAddressRange round-trips through binary serialization and deserialization unchanged.</summary>
        /// <param name="ipAddressRange">The range to serialize.</param>
        [Theory]
        [MemberData(nameof(CanSerializable_Test_Values))]
        public void CanSerializable_Test(IPAddressRange ipAddressRange)
        {
            // Arrange
            var formatter = new BinaryFormatter();

            // Act
            using var writeStream = new MemoryStream();
            // Serialize the object to the stream
            formatter.Serialize(writeStream, ipAddressRange);
            writeStream.Seek(0, SeekOrigin.Begin);

            // Deserialize the object from the stream
            var result = formatter.Deserialize(writeStream);

            // Assert
            var actual = Assert.IsType<IPAddressRange>(result);

            // using explicit EqualityComparer to avoid comparing elements of enumerable
            Assert.Equal(ipAddressRange, actual, IPAddressRangeEqualityComparer.Instance);
        }

        #endregion // end: ISerializable
    }
}
#endif
