#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

namespace Arcus
{
    /// <content><see cref="BigEndianBitWrapper"/> implementation of <see cref="IFormattable"/></content>
    internal readonly partial struct BigEndianBitWrapper
    {
        /// <summary>
        ///     Formats the value using the specified format specifier.
        /// </summary>
        /// <param name="format">
        ///     <list type="bullet">
        ///         <item>
        ///             <term><c>"HC"</c></term>
        ///             <description>Hex compact - uppercase, no separators (e.g. "C0A80101").</description>
        ///         </item>
        ///         <item>
        ///             <term><c>"IBE"</c></term>
        ///             <description>Integer big-endian - decimal string of the unsigned value.</description>
        ///         </item>
        ///         <item>
        ///             <term><c>"b"</c></term>
        ///             <description>Binary string, MSB first, length ByteWidth × 8.</description>
        ///         </item>
        ///         <item>
        ///             <term><c>null</c> or <c>"G"</c></term>
        ///             <description>Default - same as "HC".</description>
        ///         </item>
        ///     </list>
        /// </param>
        /// <param name="formatProvider">Ignored; present to satisfy <see cref="IFormattable" />.</param>
        /// <returns>The formatted string.</returns>
        /// <exception cref="FormatException">An unrecognised format specifier was supplied.</exception>
        public string ToString(
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
            [AllowNull]
#endif
            string format,
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
            [AllowNull]
#endif
            IFormatProvider formatProvider)
        {
            return string.IsNullOrEmpty(format) || format == "G"
                ? ToHexString()
                : format switch
                {
                    "HC" => ToHexString(),
                    "IBE" => ToDecimalString(),
                    "b" => ToBinaryString(),
                    _ => throw new FormatException($"Unknown format specifier '{format}' for {nameof(BigEndianBitWrapper)}."),
                };
        }
    }
}
