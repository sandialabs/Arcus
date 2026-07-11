namespace Arcus.Tests.XunitSerializers
{
    /// <summary>
    ///     <see cref="IXunitSerializer"/> for <see cref="IEnumerable{T}"/> of <see cref="Subnet"/>
    /// </summary>
    public class SubnetEnumerableXunitSerializer : IXunitSerializer
    {
        private const string Separator = "|";

        /// <summary>
        /// Initializes a new instance of the <see cref="SubnetEnumerableXunitSerializer"/> class.
        /// </summary>
        /// <remarks>from <see href="https://xunit.net/docs/getting-started/v3/custom-serialization">xUnit Serialization support in v3</see></remarks>
        public SubnetEnumerableXunitSerializer() { }

        /// <inheritdoc/>
        public bool IsSerializable(Type type, object value, out string failureReason)
        {
            if (type == typeof(IEnumerable<Subnet>) && value is IEnumerable<Subnet>)
            {
                failureReason = null;
                return true;
            }

            failureReason = $"Type {type.FullName} is not supported by {nameof(SubnetEnumerableXunitSerializer)}.";
            return false;
        }

        /// <inheritdoc/>
        public string Serialize(object value)
        {
            if (value is IEnumerable<Subnet> subnets)
            {
                return string.Join(Separator, subnets.Select(s => s.ToString("f", null)));
            }

            throw new InvalidOperationException(
                $"Invalid type for serialization: {value.GetType().FullName} is not supported by {nameof(SubnetEnumerableXunitSerializer)}."
            );
        }

        /// <inheritdoc/>
        public object Deserialize(Type type, string serializedValue)
        {
            if (type == typeof(IEnumerable<Subnet>))
            {
                if (serializedValue.Length == 0)
                {
                    return Enumerable.Empty<Subnet>();
                }

                return serializedValue.Split(new[] { Separator }, StringSplitOptions.None).Select(Subnet.Parse).ToList();
            }

            throw new ArgumentException(
                $"Invalid type for deserialization: {type.FullName} is not supported by {nameof(SubnetEnumerableXunitSerializer)}"
            );
        }
    }
}
