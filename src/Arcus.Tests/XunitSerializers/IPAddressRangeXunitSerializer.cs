using System;
using System.Linq;
using System.Net;
using Xunit.Sdk;

namespace Arcus.Tests.XunitSerializers
{
    /// <summary>
    ///     <see cref="IXunitSerializer"/> for <see cref="IPAddressRange"/>
    /// </summary>
    public class IPAddressRangeXunitSerializer : IXunitSerializer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IPAddressRangeXunitSerializer"/> class.
        /// </summary>
        /// <remarks>from <see href="https://xunit.net/docs/getting-started/v3/custom-serialization">xUnit Serialization support in v3</see></remarks>
        public IPAddressRangeXunitSerializer() { }

        /// <inheritdoc/>
        public bool IsSerializable(Type type, object value, out string failureReason)
        {
            if (type == typeof(IPAddressRange) && value is IPAddressRange)
            {
                failureReason = null;
                return true;
            }

            failureReason = $"Type {type.FullName} is not supported by {nameof(IPAddressRangeXunitSerializer)}.";
            return false;
        }

        /// <inheritdoc/>
        public string Serialize(object value)
        {
            if (value is IPAddressRange ipAddressRange)
            {
                return ipAddressRange.ToString("G", null);
            }

            throw new InvalidOperationException(
                $"Invalid type for serialization: {value.GetType().FullName} is not supported by {nameof(IPAddressRangeXunitSerializer)}."
            );
        }

        /// <inheritdoc/>
        public object Deserialize(Type type, string serializedValue)
        {
            if (type == typeof(IPAddressRange))
            {
                var substrings = serializedValue.Split('-').Select(s => s.Trim()).ToList();

                if (substrings.Count > 2)
                {
                    throw new InvalidOperationException("Could not parse serialized IP Address range \"{serializedValue}\"");
                }

                return new IPAddressRange(IPAddress.Parse(substrings[0]), IPAddress.Parse(substrings[1]));
            }

            throw new ArgumentException(
                $"Invalid type for deserialization: {type.FullName} is not supported by {nameof(IPAddressRangeXunitSerializer)}"
            );
        }
    }
}
