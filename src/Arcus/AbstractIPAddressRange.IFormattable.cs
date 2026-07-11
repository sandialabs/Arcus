using System.Globalization;
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

namespace Arcus
{
    /// <content>
    ///     <see cref="AbstractIPAddressRange"/> implementation of <see cref="IFormattable"/>
    /// </content>
    public abstract partial class AbstractIPAddressRange
    {
        #region Formatting

        /// <inheritdoc />
        public override string ToString()
        {
            return this.ToString("G", CultureInfo.InvariantCulture);
        }

        /// <inheritdoc />
        public virtual string ToString(
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
            [AllowNull]
#endif
            string format,
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
            [AllowNull]
#endif
            IFormatProvider formatProvider)
        {
            return format?.Trim() switch
            {
                null or { Length: 0 } or "g" or "G" => $"{this.Head} - {this.Tail}",
                _ => throw new FormatException($"The format \"{format}\" is not supported."),
            };
        }

        #endregion // end: Formatting
    }
}
