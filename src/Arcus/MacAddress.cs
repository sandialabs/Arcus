using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using Gulliver;
using JetBrains.Annotations;

namespace Arcus
{
    /// <summary>Representation of a 48-bit MAC Address (EUI-48 &amp; MAC-48)</summary>
    /// <remarks>
    ///     <para>
    ///         see IEEE "Guidelines for Use of Extended Unique Identifier (EUI), Organizationally Unique Identifier (OUI), and
    ///         Company ID (CID)":
    ///         https://standards.ieee.org/content/dam/ieee-standards/standards/web/documents/tutorials/eui.pdf
    ///     </para>
    /// </remarks>
    [PublicAPI]
    [Serializable]
    public class MacAddress : IEquatable<MacAddress>,
                              IComparable<MacAddress>,
                              IComparable,
                              IFormattable,
                              ISerializable
    {
        /// <summary>
        ///     <para>MAC Address Regular Expression pattern for matching:</para>
        ///     <list type="bullet">
        ///         <item>
        ///             <description>
        ///                 IEEE 802 format for printing EUI-48 &amp; MAC-48 addresses in six groups of two hexadecimal digits,
        ///                 separated by a dash ("-")
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <description>six groups of two hexadecimal digits separated by colons (":")</description>
        ///         </item>
        ///         <item>
        ///             <description>six groups of two hexadecimal digits separated by spaces (" ")</description>
        ///         </item>
        ///         <item>
        ///             <description>12 hexadecimal digits with no separation</description>
        ///         </item>
        ///         <item>
        ///             <description>Cisco three groups of four hexadecimal digits separated by dots (".")</description>
        ///         </item>
        ///     </list>
        /// </summary>
        /// <remarks>
        ///     <para>this regular expression pattern is expected to be used in matches ignoring case</para>
        /// </remarks>
        public const string AllFormatMacAddressPattern =
            @"^[\dA-F]{2}([ -:]?)(?:[\dA-F]{2}\1){4}[\dA-F]{2}$|^(?:[\dA-F]{4}\.){2}[\dA-F]{4}$|^(?:[\dA-F]{3}\.){3}[\dA-F]{3}$";

        /// <summary>
        ///     MAC Address Regular Expression pattern for matching the "common" six groups of two uppercase hexadecimal digits
        ///     separated by colons(":")
        /// </summary>
        /// <remarks>
        ///     <para>this regular expression pattern is expected to be used in matches ignoring case</para>
        /// </remarks>
        public const string CommonFormatMacAddressPattern = @"^(?:[\dA-F]{2}:){5}[\dA-F]{2}$";

        /// <summary>A MAC Address that represents the default or <see langword="null" /> case. Equal to <c>FF:FF:FF:FF:FF:FF</c>.</summary>
        public static readonly MacAddress DefaultMacAddress = new MacAddress(Enumerable.Repeat((byte) 0xFF, 6));

        /// <summary>
        ///     <para>MAC Address Regular Expression matching</para>
        ///     <list type="bullet">
        ///         <item>
        ///             <description>
        ///                 IEEE 802 format for printing EUI-48 &amp; MAC-48 addresses in six groups of two hexadecimal digits,
        ///                 separated by a dash ("-")
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <description>six groups of two hexadecimal digits separated by colons (":")</description>
        ///         </item>
        ///         <item>
        ///             <description>six groups of two hexadecimal digits separated by spaces (" ")</description>
        ///         </item>
        ///         <item>
        ///             <description>12 hexadecimal digits with no separation</description>
        ///         </item>
        ///         <item>
        ///             <description>Cisco three groups of four hexadecimal digits separated by dots (".")</description>
        ///         </item>
        ///     </list>
        /// </summary>
        /// <remarks>
        ///     <para>see <see cref="AllFormatMacAddressPattern" /> for pattern.</para>
        /// </remarks>
        public static readonly Regex AllFormatMacAddressRegularExpression =
            new Regex(AllFormatMacAddressPattern,
                      RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        /// <summary>
        ///     MAC Address Regular Expression for matching the "common" six groups of two hexadecimal digits separated by colons
        ///     (":"). uses
        /// </summary>
        /// <remarks>
        ///     <para>see <see cref="CommonFormatMacAddressPattern" /> for pattern.</para>
        /// </remarks>
        public static readonly Regex CommonFormatMacAddressRegularExpression =
            new Regex(CommonFormatMacAddressPattern,
                      RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        /// <summary>The inner byte representation fo the MAC Address</summary>
        private readonly byte[] _address;

        /// <summary>Checks if the MAC Address is unicast</summary>
        /// <value>returns <see true="true" /> if the MAC Address is unicast</value>
        public bool IsUnicast => (byte) (this._address[0] & 1) == 0;

        /// <summary>Checks if the MAC Address is multicast</summary>
        /// <value>returns <see true="true" /> if the MAC Address is multicast</value>
        public bool IsMulticast => (byte) (this._address[0] & 1) != 0;

        /// <summary>Checks if the MAC Address is globally unique (OUI enforced)</summary>
        /// <value>returns <see true="true" /> if the MAC Address is globally unique</value>
        public bool IsGloballyUnique => (byte) (this._address[0] & (1 << 1)) == 0;

        /// <summary>Checks if the MAC Address is locally administered</summary>
        /// <value>returns <see true="true" /> if the MAC Address is locally administered</value>
        public bool IsLocallyAdministered => (byte) (this._address[0] & (1 << 1)) != 0;

        /// <summary>Checks if the MAC Address is the EUI-48 default</summary>
        /// <value>returns <see true="true" /> if all bits of the MAC Address are set</value>
        public bool IsDefault => this.Equals(DefaultMacAddress);

        /// <summary>Checks if the MAC Address is "unusable"</summary>
        /// <value>returns <see true="true" /> if all the OUI bits of the MAC Address are unset</value>
        public bool IsUnusable => this._address.Take(3)
                                      .Any(b => b != 0);

        #region From Interface IComparable

        /// <inheritdoc />
        public int CompareTo(object obj)
        {
            return ReferenceEquals(null, obj)
                       ? 1
                       : ReferenceEquals(this, obj)
                           ? 0
                           : obj is MacAddress other
                               ? this.CompareTo(other)
                               : throw new ArgumentException($"Object must be of type {nameof(MacAddress)}");
        }

        #endregion

        #region From Interface IComparable<MacAddress>

        /// <inheritdoc />
        public int CompareTo(MacAddress other)
        {
            return ReferenceEquals(null, other)
                       ? 1
                       : ByteArrayUtils.CompareUnsignedBigEndian(this._address, other._address);
        }

        #endregion

        #region From Interface IEquatable<MacAddress>

        /// <inheritdoc />
        public bool Equals(MacAddress other)
        {
            return !ReferenceEquals(null, other)
                   && (ReferenceEquals(this, other)
                       || ByteArrayUtils.CompareUnsignedBigEndian(this._address, other._address) == 0);
        }

        #endregion

        #region From Interface IFormattable

        /// <inheritdoc cref="IFormattable" />
        /// <summary>
        ///     <para>Express <see cref="MacAddress" /> as a string</para>
        ///     <para>The following formats are provided</para>
        ///     <list type="bullet">
        ///         <item>
        ///             <term>g, empty string, or <see langword="null" /></term>
        ///             <description>
        ///                 General format of uppercase hexadecimal encoded bytes separated by colons. eg
        ///                 <c>AA:BB:CC:DD:EE:FF</c>
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <term>H</term>
        ///             <description>formats as contiguous upper case hexadecimal digits. eg <c>AABBCCDDEEFF</c></description>
        ///         </item>
        ///         <item>
        ///             <term>h</term>
        ///             <description>formats as contiguous lower case hexadecimal digits. eg <c>aabbccddeeff</c></description>
        ///         </item>
        ///         <item>
        ///             <term>c</term>
        ///             <description>
        ///                 Cisco three groups of four uppercase hexadecimal digits separated by dots format. eg
        ///                 <c>AAAA.BBBB.CCCC</c>
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <term>s</term>
        ///             <description>hexadecimal encoded bytes separated by a space character. eg <c>AA BB CC DD EE FF</c></description>
        ///         </item>
        ///         <item>
        ///             <term>d</term>
        ///             <description>hexadecimal encoded bytes separated by a dash character. eg <c>AA-BB-CC-DD-EE-FF</c></description>
        ///         </item>
        ///         <item>
        ///             <term>i</term> <description>formatted as a big-endian integer value</description>
        ///         </item>
        ///     </list>
        /// </summary>
        /// <param name="format">the format specifier</param>
        /// <param name="formatProvider">the format provider</param>
        /// <returns>a byte array represented as a string</returns>
        /// <exception cref="ArgumentException">Invalid base conversion.</exception>
        /// <exception cref="FormatException">provided format is not supported.</exception>
        public string ToString(string format,
                               IFormatProvider formatProvider)
        {
            if (formatProvider == null)
            {
                formatProvider = CultureInfo.InvariantCulture;
            }

            switch (format?.Trim())
            {
                case null: // general formats
                case "":
                case "g":
                    return DelimitedBaseConverter(this._address, 16, ":")
                        .ToUpperInvariant();
                case "x": // lower hexadecimal
                    return DelimitedBaseConverter(this._address, 16);
                case "X": // upper hexadecimal contiguous
                    return DelimitedBaseConverter(this._address, 16)
                        .ToUpperInvariant();
                case "c": // Cisco
                    return AsCisco();
                case "s": // space delimited
                    return DelimitedBaseConverter(this._address, 16, " ")
                        .ToUpperInvariant();
                case "i": // integer value
                    return this._address.ToString("I", formatProvider);
                case "d": // dash delimited
                    return DelimitedBaseConverter(this._address, 16, "-")
                        .ToUpperInvariant();
                default:
                    throw new FormatException($"The \"{format}\" format string is not supported.");
            }

            string AsCisco()
            {
                return string.Join(".",
                                   Enumerable.Range(0, 3)
                                             .Select(i =>
                                                     {
                                                         var j = i * 2;
                                                         return $"{this._address[j]:X2}{this._address[j + 1]:X2}";
                                                     }));
            }

            string DelimitedBaseConverter(byte[] input,
                                          int @base,
                                          string delimiter = "",
                                          int? paddingWidth = null,
                                          char paddingChar = '0')
            {
                if (!new[] {2, 8, 10, 16}.Contains(@base))
                {
                    throw new
                        ArgumentException($"{nameof(@base)} must be 2, 8, 10, or 16 (binary, octal, decimal, or hexadecimal respectively)",
                                          nameof(@base));
                }

                if (paddingWidth.HasValue
                    && paddingWidth.Value < 0)
                {
                    throw new ArgumentException($"{nameof(paddingWidth)} may not be negative", nameof(paddingWidth));
                }

                // use padding width if defined, otherwise use a width of 8 for binary, 3 for octal and decimal, and 2 for hexadecimal
                var width = paddingWidth
                            ?? (@base == 2
                                    ? 8
                                    : @base == 8 || @base == 10
                                        ? 3
                                        : 2);

                return string.Join(delimiter ?? string.Empty,
                                   input.Select(b => Convert.ToString(b, @base)
                                                            .PadLeft(width, paddingChar)));
            }
        }

        #endregion

        #region From Interface ISerializable

        /// <inheritdoc />
        /// <exception cref="ArgumentNullException"><paramref name="info" /> is <see langword="null" /></exception>
        public void GetObjectData([NotNull] SerializationInfo info,
                                  StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            info.AddValue(nameof(this._address), this._address);
        }

        #endregion

        /// <summary>Gets a copy of the underlying big-endian bytes of the MAC Address</summary>
        /// <returns>The bytes of the MAC Address</returns>
        [NotNull]
        public byte[] GetAddressBytes()
        {
            return this._address.ToArray();
        }

        /// <summary>Gets the Organizationally Unique Identifier (OUI) of the address</summary>
        /// <returns>A copy of the first 3-bytes (24-bits) of the MAC Address</returns>
        [NotNull]
        public byte[] GetOuiBytes()
        {
            return this._address.Take(3)
                       .ToArray();
        }

        /// <summary>Gets the Company ID (CID) of the address</summary>
        /// <returns>A copy of the last 3-bytes (24-bits) of the MAC Address</returns>
        [NotNull]
        public byte[] GetCidBytes()
        {
            return this._address.Skip(3)
                       .Take(3)
                       .ToArray();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return this.ToString("g", CultureInfo.InvariantCulture);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return !ReferenceEquals(null, obj)
                   && (ReferenceEquals(this, obj)
                       || obj.GetType() == GetType() && this.Equals((MacAddress) obj));
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return this._address.Aggregate(0,
                                           (i,
                                            b) => unchecked((i * 17) + b));
        }

        #region Ctors

        /// <summary>Initializes a new instance of the <see cref="MacAddress" /> class.</summary>
        /// <exception cref="ArgumentNullException"><paramref name="info" /> is <see langword="null" /></exception>
        protected MacAddress([NotNull] SerializationInfo info,
                             StreamingContext context)
            : this((byte[]) info.GetValue(nameof(_address), typeof(byte[]))) { }

        /// <summary>Initializes a new instance of the <see cref="MacAddress" /> class.</summary>
        /// <param name="bytes">a collection of bytes to be copied as the byte of the MAC Address</param>
        /// <exception cref="ArgumentException"><paramref name="bytes" /> must be exactly 6 bytes long</exception>
        /// <exception cref="ArgumentNullException"><paramref name="bytes" /> is <see langword="null" /></exception>
        public MacAddress([NotNull] IEnumerable<byte> bytes)
        {
            if (bytes is null)
            {
                throw new ArgumentNullException(nameof(bytes));
            }

            var address = bytes.ToArray(); // explicit copy

            if (address.Length != 6)
            {
                throw new ArgumentException("must be exactly 6 bytes long", nameof(bytes));
            }

            this._address = address;
        }

        #endregion end: Ctors

        #region factory

        /// <summary>Attempt to extract hex characters from string and attempt to identify exactly 6 hextets to make a MAC Address</summary>
        /// <param name="input">the string input</param>
        /// <returns>a mac address</returns>
        /// <exception cref="ArgumentException">could not create 6 hextets</exception>
        /// <exception cref="RegexMatchTimeoutException">A time-out occurred</exception>
        private static MacAddress ExtractHextetsAndCreateMacAddress(string input)
        {
            // match and gather all hex characters
            const string hexCharactersPattern = @"([\dA-F]+)+";
            var matchValues = new Regex(hexCharactersPattern,
                                        RegexOptions.Compiled
                                        | RegexOptions.CultureInvariant
                                        | RegexOptions.IgnoreCase);

            // remove all non hexadecimal characters from the input, upper case the result
            var matches = matchValues.Matches(input)
                                     .OfType<Match>()
                                     .Select(match => match.Value);

            var join = string.Join(string.Empty, matches);

            if (join.Length != 12)
            {
                throw new ArgumentException("expecting 6 hextets", nameof(input));
            }

            var bytes = Enumerable.Range(0, 6)
                                  .Select(i => join.Substring(2 * i, 2))
                                  .Select(sb => byte.Parse(sb, NumberStyles.HexNumber, CultureInfo.InvariantCulture));

            return new MacAddress(bytes);
        }

        #region Parse

        #region Parse string

        /// <summary>
        ///     <para>Parse a string input, given one of the following formats, in order to create a MAC Address</para>
        ///     <list type="bullet">
        ///         <item>
        ///             <description>
        ///                 IEEE 802 format for printing EUI-48 &amp; MAC-48 addresses in six groups of two hexadecimal digits,
        ///                 separated by a dash ("-")
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <description>six groups of two hexadecimal digits separated by colons (":")</description>
        ///         </item>
        ///         <item>
        ///             <description>six groups of two hexadecimal digits separated by spaces (" ")</description>
        ///         </item>
        ///         <item>
        ///             <description>12 hexadecimal digits with no separation</description>
        ///         </item>
        ///         <item>
        ///             <description>Cisco three groups of four hexadecimal digits separated by dots (".")</description>
        ///         </item>
        ///     </list>
        /// </summary>
        /// <param name="input">the input to parse</param>
        /// <returns>a <see cref="MacAddress" /></returns>
        /// <exception cref="ArgumentException"><paramref name="input" /> is <see langword="null" /> or whitespace.</exception>
        /// <exception cref="RegexMatchTimeoutException">A time-out occurred</exception>
        [NotNull]
        public static MacAddress Parse([NotNull] string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(input));
            }

            if (!AllFormatMacAddressRegularExpression.IsMatch(input))
            {
                throw new ArgumentException("does not match known MAC Address formats", nameof(input));
            }

            return ExtractHextetsAndCreateMacAddress(input);
        }

        /// <summary>
        ///     <para>Try to parse a string input, given one of the following formats, in order to create a MAC Address</para>
        ///     <list type="bullet">
        ///         <item>
        ///             <description>
        ///                 IEEE 802 format for printing EUI-48 &amp; MAC-48 addresses in six groups of two hexadecimal digits,
        ///                 separated by a dash ("-")
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <description>six groups of two hexadecimal digits separated by colons (":")</description>
        ///         </item>
        ///         <item>
        ///             <description>six groups of two hexadecimal digits separated by spaces (" ")</description>
        ///         </item>
        ///         <item>
        ///             <description>12 hexadecimal digits with no separation</description>
        ///         </item>
        ///         <item>
        ///             <description>Cisco three groups of four hexadecimal digits separated by dots (".")</description>
        ///         </item>
        ///     </list>
        /// </summary>
        /// <param name="input">the input to parse</param>
        /// <param name="macAddress">a <see cref="MacAddress" /> created on successful parse</param>
        /// <returns><see langword="true" /> iff the the parse was successful</returns>
        public static bool TryParse(string input,
                                    [CanBeNull] out MacAddress macAddress)
        {
            try
            {
                macAddress = Parse(input);
                return true;
            }
#pragma warning disable CA1031 // Modify 'TryParseAny' to catch a more specific exception type, or rethrow the exception.
            catch // explicitly catching all for the sake of standard bool Try*(out) pattern
#pragma warning restore CA1031
            {
                macAddress = null;
                return false;
            }
        }

        #endregion end: Parse string

        #region ParseAny string

        /// <summary>Parse a string disregarding all non hexadecimal digits in order to create a MAC Address</summary>
        /// <param name="input">the input to parse</param>
        /// <returns>a <see cref="MacAddress" /></returns>
        /// <exception cref="ArgumentException"><paramref name="input" /> is <see langword="null" /> or whitespace.</exception>
        /// <exception cref="RegexMatchTimeoutException">
        ///     A time-out occurred. For more information about time-outs, see the Remarks
        ///     section.
        /// </exception>
        [NotNull]
        public static MacAddress ParseAny([NotNull] string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(input));
            }

            return ExtractHextetsAndCreateMacAddress(input);
        }

        /// <summary>Try to parse a string disregarding all non hexadecimal digits in order to create a MAC Address</summary>
        /// <param name="input">the input to parse</param>
        /// <param name="macAddress">a <see cref="MacAddress" /> created on successful parse</param>
        /// <returns><see langword="true" /> if the the parse was successful</returns>
        public static bool TryParseAny(string input,
                                       [CanBeNull] out MacAddress macAddress)
        {
            try
            {
                macAddress = ParseAny(input);
                return true;
            }
#pragma warning disable CA1031 // Modify 'TryParseAny' to catch a more specific exception type, or rethrow the exception.
            catch // explicitly catching all for the sake of standard bool Try*(out) pattern
#pragma warning restore CA1031
            {
                macAddress = null;
                return false;
            }
        }

        #endregion end: ParseAny

        #endregion end: Parse

        #endregion end: factory

        #region operators

        /// <summary>Check equality of <paramref name="left" /> and <paramref name="right" /></summary>
        /// <param name="left">the left operand</param>
        /// <param name="right">the right operand</param>
        /// <returns><see langword="true" /> iff both <paramref name="left" /> and <paramref name="right" /> are equal</returns>
        public static bool operator ==(MacAddress left,
                                       MacAddress right)
        {
            return Equals(left, right);
        }

        /// <summary>Check inequality of <paramref name="left" /> and <paramref name="right" /></summary>
        /// <param name="left">the left operand</param>
        /// <param name="right">the right operand</param>
        /// <returns><see langword="true" /> iff both <paramref name="left" /> and <paramref name="right" /> are not equal</returns>
        public static bool operator !=(MacAddress left,
                                       MacAddress right)
        {
            return !Equals(left, right);
        }

        /// <summary>Check if <paramref name="left" /> is less than <paramref name="right" /></summary>
        /// <param name="left">the left operand</param>
        /// <param name="right">the right operand</param>
        /// <returns><see langword="true" /> iff <paramref name="left" /> is less than <paramref name="right" /></returns>
        public static bool operator <(MacAddress left,
                                      MacAddress right)
        {
            return Comparer<MacAddress>.Default.Compare(left, right) < 0;
        }

        /// <summary>Check if <paramref name="left" /> is greater than <paramref name="right" /></summary>
        /// <param name="left">the left operand</param>
        /// <param name="right">the right operand</param>
        /// <returns><see langword="true" /> iff <paramref name="left" /> is greater than <paramref name="right" /></returns>
        public static bool operator >(MacAddress left,
                                      MacAddress right)
        {
            return Comparer<MacAddress>.Default.Compare(left, right) > 0;
        }

        /// <summary>Check if <paramref name="left" /> is less than or equal to<paramref name="right" /></summary>
        /// <param name="left">the left operand</param>
        /// <param name="right">the right operand</param>
        /// <returns>
        ///     <see langword="true" /> iff <paramref name="left" /> is less than or equal to <paramref name="right" />
        /// </returns>
        public static bool operator <=(MacAddress left,
                                       MacAddress right)
        {
            return Comparer<MacAddress>.Default.Compare(left, right) <= 0;
        }

        /// <summary>Check if <paramref name="left" /> is greater than or equal to<paramref name="right" /></summary>
        /// <param name="left">the left operand</param>
        /// <param name="right">the right operand</param>
        /// <returns>
        ///     <see langword="true" /> iff <paramref name="left" /> is greater than or equal to <paramref name="right" />
        /// </returns>
        public static bool operator >=(MacAddress left,
                                       MacAddress right)
        {
            return Comparer<MacAddress>.Default.Compare(left, right) >= 0;
        }

        #endregion end: operators
    }
}
