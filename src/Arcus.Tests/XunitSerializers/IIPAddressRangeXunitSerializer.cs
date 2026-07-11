using System.Net;

namespace Arcus.Tests.XunitSerializers
{
    /// <summary>
    ///     <see cref="IXunitSerializer"/> for <see cref="IIPAddressRange"/>
    /// </summary>
    /// <remarks>
    ///     Serializes concrete <see cref="Subnet"/> as <c>Subnet:&lt;cidr&gt;</c>
    ///     and <see cref="IPAddressRange"/> as <c>Range:&lt;head&gt; - &lt;tail&gt;</c>.
    /// </remarks>
#pragma warning disable S101
    public class IIPAddressRangeXunitSerializer : IXunitSerializer
#pragma warning restore S101
    {
        private const string SubnetPrefix = "Subnet:";
        private const string RangePrefix = "Range:";

        /// <summary>
        /// Initializes a new instance of the <see cref="IIPAddressRangeXunitSerializer"/> class.
        /// </summary>
        /// <remarks>from <see href="https://xunit.net/docs/getting-started/v3/custom-serialization">xUnit Serialization support in v3</see></remarks>
        public IIPAddressRangeXunitSerializer() { }

        /// <inheritdoc/>
        public bool IsSerializable(Type type, object value, out string failureReason)
        {
            if (typeof(IIPAddressRange).IsAssignableFrom(type) && value is IIPAddressRange)
            {
                failureReason = null;
                return true;
            }

            failureReason = $"Type {type.FullName} is not supported by {nameof(IIPAddressRangeXunitSerializer)}.";
            return false;
        }

        /// <inheritdoc/>
        public string Serialize(object value)
        {
            if (value is Subnet subnet)
            {
                return SubnetPrefix + subnet.ToString("f", null);
            }

            if (value is IPAddressRange range)
            {
                return RangePrefix + range.ToString("G", null);
            }

            throw new InvalidOperationException(
                $"Invalid type for serialization: {value.GetType().FullName} is not supported by {nameof(IIPAddressRangeXunitSerializer)}."
            );
        }

        /// <inheritdoc/>
        public object Deserialize(Type type, string serializedValue)
        {
            if (!typeof(IIPAddressRange).IsAssignableFrom(type))
            {
                throw new ArgumentException(
                    $"Invalid type for deserialization: {type.FullName} is not supported by {nameof(IIPAddressRangeXunitSerializer)}"
                );
            }

            if (serializedValue.StartsWith(SubnetPrefix, StringComparison.Ordinal))
            {
                return Subnet.Parse(serializedValue.Substring(SubnetPrefix.Length));
            }

            if (serializedValue.StartsWith(RangePrefix, StringComparison.Ordinal))
            {
                var parts = serializedValue.Substring(RangePrefix.Length).Split('-').Select(s => s.Trim()).ToList();

                if (parts.Count != 2)
                {
                    throw new InvalidOperationException($"Could not parse serialized IIPAddressRange \"{serializedValue}\"");
                }

                return new IPAddressRange(IPAddress.Parse(parts[0]), IPAddress.Parse(parts[1]));
            }

            throw new InvalidOperationException($"Could not parse serialized IIPAddressRange \"{serializedValue}\"");
        }
    }
}
