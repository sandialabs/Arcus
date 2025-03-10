using System;
using Xunit.Sdk;

namespace Arcus.Tests.XunitSerializers
{
    /// <summary>
    ///     <see cref="IXunitSerializer"/> for <see cref="Subnet"/>
    /// </summary>
    public class SubnetXunitSerializer : IXunitSerializer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SubnetXunitSerializer"/> class.
        /// </summary>
        /// <remarks>from <see href="https://xunit.net/docs/getting-started/v3/custom-serialization">xUnit Serialization support in v3</see></remarks>
        public SubnetXunitSerializer() { }

        /// <inheritdoc/>
        public bool IsSerializable(Type type, object value, out string failureReason)
        {
            if (type == typeof(Subnet) && value is Subnet)
            {
                failureReason = null;
                return true;
            }

            failureReason = $"Type {type.FullName} is not supported by {nameof(SubnetXunitSerializer)}.";
            return false;
        }

        /// <inheritdoc/>
        public string Serialize(object value)
        {
            if (value is Subnet subnet)
            {
                return subnet.ToString("f", null);
            }

            throw new InvalidOperationException(
                $"Invalid type for serialization: {value.GetType().FullName} is not supported by {nameof(SubnetXunitSerializer)}."
            );
        }

        /// <inheritdoc/>
        public object Deserialize(Type type, string serializedValue)
        {
            if (type == typeof(Subnet))
            {
                return Subnet.Parse(serializedValue);
            }

            throw new ArgumentException(
                $"Invalid type for deserialization: {type.FullName} is not supported by {nameof(SubnetXunitSerializer)}"
            );
        }
    }
}
