using System.Globalization;

namespace Arcus.Tests.XunitSerializers
{
    /// <summary>
    ///     <see cref="IXunitSerializer"/> for <see cref="IFormatProvider"/>
    /// </summary>
    /// <remarks>
    ///     Handles <see cref="CultureInfo"/> instances, serialized as <c>CultureInfo:&lt;name&gt;</c>.
    /// </remarks>
    public class FormatProviderXunitSerializer : IXunitSerializer
    {
        private const string CultureInfoPrefix = "CultureInfo:";

        /// <summary>
        /// Initializes a new instance of the <see cref="FormatProviderXunitSerializer"/> class.
        /// </summary>
        /// <remarks>from <see href="https://xunit.net/docs/getting-started/v3/custom-serialization">xUnit Serialization support in v3</see></remarks>
        public FormatProviderXunitSerializer() { }

        /// <inheritdoc/>
        public bool IsSerializable(Type type, object value, out string failureReason)
        {
            if (typeof(IFormatProvider).IsAssignableFrom(type) && value is CultureInfo)
            {
                failureReason = null;
                return true;
            }

            failureReason =
                $"Type {type.FullName} with value type {value?.GetType().FullName ?? "null"} is not supported by {nameof(FormatProviderXunitSerializer)}.";
            return false;
        }

        /// <inheritdoc/>
        public string Serialize(object value)
        {
            if (value is CultureInfo cultureInfo)
            {
                return CultureInfoPrefix + cultureInfo.Name;
            }

            throw new InvalidOperationException(
                $"Invalid type for serialization: {value?.GetType().FullName ?? "null"} is not supported by {nameof(FormatProviderXunitSerializer)}."
            );
        }

        /// <inheritdoc/>
        public object Deserialize(Type type, string serializedValue)
        {
            if (!typeof(IFormatProvider).IsAssignableFrom(type))
            {
                throw new ArgumentException(
                    $"Invalid type for deserialization: {type.FullName} is not supported by {nameof(FormatProviderXunitSerializer)}"
                );
            }

            if (serializedValue.StartsWith(CultureInfoPrefix, StringComparison.Ordinal))
            {
                return new CultureInfo(serializedValue.Substring(CultureInfoPrefix.Length));
            }

            throw new InvalidOperationException($"Could not parse serialized IFormatProvider \"{serializedValue}\"");
        }
    }
}
