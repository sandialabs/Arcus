using System.Net;

namespace Arcus.Tests.XunitSerializers
{
    /// <summary>
    ///     <see cref="IXunitSerializer"/> for <see cref="IPAddress"/>
    /// </summary>
    public class IPAddressXunitSerializer : IXunitSerializer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IPAddressXunitSerializer"/> class.
        /// </summary>
        /// <remarks>from <see href="https://xunit.net/docs/getting-started/v3/custom-serialization">xUnit Serialization support in v3</see></remarks>
        public IPAddressXunitSerializer() { }

        /// <inheritdoc/>
        public bool IsSerializable(Type type, object value, out string failureReason)
        {
            if (type == typeof(IPAddress) && value is IPAddress)
            {
                failureReason = null;
                return true;
            }

            failureReason = $"Type {type.FullName} is not supported by {nameof(IPAddressXunitSerializer)}.";
            return false;
        }

        /// <inheritdoc/>
        public string Serialize(object value)
        {
            if (value is IPAddress ipAddress)
            {
                return ipAddress.ToString();
            }

            throw new InvalidOperationException(
                $"Invalid type for serialization: {value.GetType().FullName} is not supported by {nameof(IPAddressXunitSerializer)}."
            );
        }

        /// <inheritdoc/>
        public object Deserialize(Type type, string serializedValue)
        {
            if (type == typeof(IPAddress))
            {
                return IPAddress.Parse(serializedValue);
            }

            throw new ArgumentException(
                $"Invalid type for deserialization: {type.FullName} is not supported by {nameof(IPAddressXunitSerializer)}"
            );
        }
    }
}
