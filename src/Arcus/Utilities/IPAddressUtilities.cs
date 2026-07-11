using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

namespace Arcus.Utilities
{
    /// <summary>
    ///     Static utility class containing miscellaneous operations for <see cref="IPAddress" /> objects
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         IPv4 constants are derived from the 32-bit address definition in
    ///         <see href="https://www.rfc-editor.org/rfc/rfc791#section-2.3">RFC 791 §2.3</see>.
    ///         IPv6 constants are derived from the 128-bit address definition in
    ///         <see href="https://www.rfc-editor.org/rfc/rfc4291#section-2.1">RFC 4291 §2.1</see>.
    ///     </para>
    /// </remarks>
    public static partial class IPAddressUtilities
    {
        private const int BitsPerByte = 8;

        /// <summary>
        ///     Regex pattern matching leading zeros in each dotted-quad octet.
        ///     Lookbehind <c>(?&lt;=^|\.)</c> anchors to start-of-string or a dot; lookahead <c>(?!\.|$)</c> preserves lone-zero octets.
        ///     Applied with <see cref="RegexOptions.CultureInvariant"/>.
        /// </summary>
        public const string DottedQuadLeadingZerosPattern = @"(?<=^|\.)0+(?!\.|$)";

        /// <summary>
        ///     Regex pattern checking dotted-quad format: four groups of 1-3 digits separated by dots.
        ///     Does not verify address validity (octet value range).
        ///     Applied with <see cref="RegexOptions.CultureInvariant"/>.
        /// </summary>
        public const string DottedQuadRegularExpressionPattern = @"^[0-9]{1,3}(\.[0-9]{1,3}){3}$";

        /// <summary>
        ///     Regex pattern matching strings composed entirely of hexadecimal digits (0-9, a-f; allows empty string).
        ///     Applied with <see cref="RegexOptions.IgnoreCase"/> and <see cref="RegexOptions.CultureInvariant"/>.
        /// </summary>
        public const string HexLikePattern = "^[0-9a-f]*$";

        /// <summary>
        ///     Bit width of an IPv4 address (32).
        /// </summary>
        public const int IPv4BitCount = 32;

        /// <summary>
        ///     Byte width of an IPv4 address (4).
        /// </summary>
        public const int IPv4ByteCount = IPv4BitCount / BitsPerByte;

        /// <summary>
        ///     Number of octets in an IPv4 dotted-quad address (4).
        /// </summary>
        public const int IPv4OctetCount = 4;

        /// <summary>
        ///     Bit width of an IPv6 address (128).
        /// </summary>
        public const int IPv6BitCount = 128;

        /// <summary>
        ///     Byte width of an IPv6 address (16).
        /// </summary>
        public const int IPv6ByteCount = IPv6BitCount / BitsPerByte;

        /// <summary>
        ///     Number of hextets in an IPv6 colon-hex address (8).
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         IPv6 colon-hex notation groups 128 bits into 8 16-bit hextets per
        ///         <see href="https://www.rfc-editor.org/rfc/rfc4291#section-2.2">RFC 4291 §2.2</see>.
        ///     </para>
        /// </remarks>
        public const int IPv6HextetCount = 8;

#if !NET7_0_OR_GREATER
        private static readonly Regex HexLikeRegularExpression = new(
            HexLikePattern,
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant
        );
#else
        private static Regex HexLikeRegularExpression => GetHexLikeRegularExpression();

        [GeneratedRegex(HexLikePattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
        private static partial Regex GetHexLikeRegularExpression();
#endif

#if !NET7_0_OR_GREATER
        private static readonly Regex DottedQuadLeadingZerosRegularExpression = new(
            DottedQuadLeadingZerosPattern,
            RegexOptions.Compiled | RegexOptions.CultureInvariant
        );
#else
        private static Regex DottedQuadLeadingZerosRegularExpression => GetDottedQuadLeadingZerosRegularExpression();

        [GeneratedRegex(DottedQuadLeadingZerosPattern, RegexOptions.CultureInvariant)]
        private static partial Regex GetDottedQuadLeadingZerosRegularExpression();
#endif

#if !NET7_0_OR_GREATER
        private static readonly Regex DottedQuadStringRegularExpression = new(
            DottedQuadRegularExpressionPattern,
            RegexOptions.Compiled | RegexOptions.CultureInvariant
        );
#else
        private static Regex DottedQuadStringRegularExpression => GetDottedQuadStringRegularExpression();

        [GeneratedRegex(DottedQuadRegularExpressionPattern, RegexOptions.CultureInvariant)]
        private static partial Regex GetDottedQuadStringRegularExpression();
#endif

        /// <summary>
        ///     Maximum IPv4 address: 255.255.255.255.
        /// </summary>
        public static readonly IPAddress IPv4MaxAddress = new(uint.MaxValue);

        /// <summary>
        ///     Minimum IPv4 address: 0.0.0.0.
        /// </summary>
        public static readonly IPAddress IPv4MinAddress = new(0);

        /// <summary>
        ///     Maximum IPv6 address: ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff.
        /// </summary>
        public static readonly IPAddress IPv6MaxAddress = new(
            [0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff],
            0L
        );

        /// <summary>
        ///     The set of address families supported by Arcus.
        /// </summary>
        public static readonly AddressFamily[] ValidAddressFamilies =
        [
            AddressFamily.InterNetwork,
            AddressFamily.InterNetworkV6,
        ];

        /// <summary>
        ///     Minimum IPv6 address: :: (all zeros).
        /// </summary>
        public static readonly IPAddress IPv6MinAddress = new(new byte[IPv6ByteCount], 0L);

        #region Address Max / Minimum

        /// <summary>
        ///     Get the Max Address for the given address family. (supports only <see cref="AddressFamily.InterNetwork" /> and
        ///     <see cref="AddressFamily.InterNetworkV6" />)
        /// </summary>
        /// <param name="addressFamily">the <see cref="AddressFamily"/></param>
        /// <returns><see cref="IPv4MaxAddress"/> when <paramref name="addressFamily"/> is <see cref="AddressFamily.InterNetwork" /> or <see cref="IPv6MaxAddress"/> when <see cref="AddressFamily.InterNetworkV6" /> </returns>
        public static IPAddress MaxIPAddress(this AddressFamily addressFamily)
        {
            return addressFamily switch
            {
                AddressFamily.InterNetwork => IPv4MaxAddress,
                AddressFamily.InterNetworkV6 => IPv6MaxAddress,
                _ => throw new ArgumentException($"Unsupported address family \"{addressFamily}\"", nameof(addressFamily)),
            };
        }

        /// <summary>
        ///     Get the Min Address for the given address family. (supports only <see cref="AddressFamily.InterNetwork" /> and
        ///     <see cref="AddressFamily.InterNetworkV6" />)
        /// </summary>
        /// <param name="addressFamily">the <see cref="AddressFamily"/></param>
        /// <returns><see cref="IPv4MinAddress"/> when <paramref name="addressFamily"/> is <see cref="AddressFamily.InterNetwork" /> or <see cref="IPv6MinAddress"/> when <see cref="AddressFamily.InterNetworkV6" /> </returns>
        public static IPAddress MinIPAddress(this AddressFamily addressFamily)
        {
            return addressFamily switch
            {
                AddressFamily.InterNetwork => IPv4MinAddress,
                AddressFamily.InterNetworkV6 => IPv6MinAddress,
                _ => throw new ArgumentException($"Unsupported address family \"{addressFamily}\"", nameof(addressFamily)),
            };
        }

        #endregion // end: Address Max / Minimum

        #region address family detection

        /// <summary>
        ///     Determines whether the address is an IPv4 address.
        /// </summary>
        /// <param name="ipAddress">The address to test.</param>
        /// <returns><see langword="true" /> if the address is IPv4.</returns>
        public static bool IsIPv4(this IPAddress ipAddress)
        {
            return ipAddress != null && ipAddress.AddressFamily == AddressFamily.InterNetwork;
        }

        /// <summary>
        ///     Determines whether the address is an IPv6 address.
        /// </summary>
        /// <param name="ipAddress">The address to test.</param>
        /// <returns><see langword="true" /> if the address is IPv6.</returns>
        public static bool IsIPv6(this IPAddress ipAddress)
        {
            return ipAddress != null && ipAddress.AddressFamily == AddressFamily.InterNetworkV6;
        }

        #endregion

        #region address format detection

        /// <summary>
        ///     Determines whether the address is an IPv4-mapped IPv6 address.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         IPv4-Mapped IPv6 Address format per
        ///         <see href="https://www.rfc-editor.org/rfc/rfc4291#section-2.5.5.2">RFC 4291 §2.5.5.2</see>:
        ///         80 zero bits, 16 one-bits (<c>0xFFFF</c>), followed by the 32-bit IPv4 address.
        ///     </para>
        /// </remarks>
        /// <param name="ipAddress">the IPAddress to test</param>
        /// <returns>true if is an IPv4 address mapped to IPv6</returns>
        /// <exception cref="ArgumentNullException"><paramref name="ipAddress" /> is <see langword="null" />.</exception>
        public static bool IsIPv4MappedIPv6(this IPAddress ipAddress)
        {
            if (ipAddress?.IsIPv6() != true)
            {
                return false;
            }

            var addressBytes = ipAddress.GetAddressBytes();

#if NET8_0_OR_GREATER
            return addressBytes[11] == 0xff
                && addressBytes[10] == 0xff
                && addressBytes[0] == 0x00
                && addressBytes[1] == 0x00
                && addressBytes[2] == 0x00
                && addressBytes[3] == 0x00
                && addressBytes[4] == 0x00
                && addressBytes[5] == 0x00
                && addressBytes[6] == 0x00
                && addressBytes[7] == 0x00
                && addressBytes[8] == 0x00
                && addressBytes[9] == 0x00;
#else
            return addressBytes.Take(10).All(byteValue => byteValue == 0x00)
                && addressBytes[10] == 0xff
                && addressBytes[11] == 0xff;
#endif
        }

        /// <summary>
        ///     Determines whether the given <see cref="IPAddress" /> is a valid IPv4 subnet mask.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         A valid subnet mask is a contiguous sequence of leading 1-bits followed by 0-bits per
        ///         <see href="https://www.rfc-editor.org/rfc/rfc950#section-2">RFC 950 §2</see>.
        ///         IPv4 only; returns <see langword="false" /> for IPv6 addresses.
        ///     </para>
        /// </remarks>
        /// <param name="netmask">The mask to test.</param>
        /// <returns><see langword="true" /> if the address is a valid IPv4 subnet mask.</returns>
        public static bool IsValidNetMask(this IPAddress netmask)
        {
            if (netmask == null || netmask.AddressFamily != AddressFamily.InterNetwork)
            {
                return false;
            }

            var set = true;
            var netmaskBytes = netmask.GetAddressBytes();
            for (var i = 0; i < netmaskBytes.Length * 8; i++)
            {
                var bitMask = (byte)(0x80 >> (i % 8));
                var indexSet = (netmaskBytes[i / 8] & bitMask) != 0;

                if (!set && indexSet)
                {
                    return false;
                }

                if (!indexSet)
                {
                    set = false;
                }
            }

            return true;
        }

        #endregion

        #region parsing

        #region hex parsing

        /// <summary>
        ///     Parses a hex string (optionally prefixed with "0x") as an IP address.
        /// </summary>
        /// <param name="input">Hex input.</param>
        /// <param name="addressFamily">Address family.</param>
        /// <returns>
        ///     IP Address, or <see langword="null" /> if parse fails.
        /// </returns>
        public static IPAddress ParseFromHexString(string input, AddressFamily addressFamily)
        {
            #region defense

            if (input is null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            if (string.IsNullOrWhiteSpace(input))
            {
                throw new ArgumentException($"{nameof(input)} is in an invalid format", nameof(input));
            }

            if (!ValidAddressFamilies.Contains(addressFamily))
            {
                throw new ArgumentException(
                    $"{nameof(addressFamily)} must be in {string.Join(", ", ValidAddressFamilies)}",
                    nameof(addressFamily)
                );
            }

            #endregion // end: defense

            // Ignore "0x" prefix and trim most significant zeros.
            var byteString = (
                input.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? input.Substring(2) : input
            ).TrimStart('0');

            // Preserve a single "0" when the input was all zeros (TrimStart would otherwise empty the string).
            if (byteString.Length == 0)
            {
                byteString = "0";
            }

            // If the byte string has an odd number of characters, provide a single significant 0.
            if (byteString.Length % 2 != 0)
            {
                byteString = "0" + byteString;
            }

            // Fail if the string contains non-hex characters.
            if (!HexLikeRegularExpression.IsMatch(byteString))
            {
                throw new ArgumentException($"{nameof(input)} is in an unexpected format", nameof(input));
            }

#if NET8_0_OR_GREATER
            var bytes = Convert.FromHexString(byteString);
            return Parse(bytes, addressFamily);
#else
            // Convert each pair of hex characters into a byte.
            var byteArray = Enumerable
                .Range(0, byteString.Length / 2)
                .Select(bytePairIndex => Convert.ToByte(byteString.Substring(bytePairIndex * 2, 2), 16))
                .ToArray();

            return Parse(byteArray, addressFamily);
#endif
        }

        /// <summary>
        ///     Attempts to parse a hex string as an IP address.
        /// </summary>
        /// <param name="input">The hex string (optionally prefixed with "0x").</param>
        /// <param name="addressFamily">The desired address family.</param>
        /// <param name="address">The parsed <see cref="IPAddress" />, or <see langword="null" /> on failure.</param>
        /// <returns><see langword="true" /> if parsing succeeded.</returns>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
        public static bool TryParseFromHexString(
            string input,
            AddressFamily addressFamily,
            [NotNullWhen(true)] out IPAddress address
        )
#else
        public static bool TryParseFromHexString(string input, AddressFamily addressFamily, out IPAddress address)
#endif
        {
            if (input == null)
            {
                address = null;
                return false;
            }

            try
            {
                address = ParseFromHexString(input, addressFamily);
                return true;
            }
            catch
            {
                address = null;
                return false;
            }
        }

        #endregion

        #region octal parsing

        /// <summary>
        ///     Parses an IP address string, stripping leading zeros from dotted-quad octets to avoid octal interpretation.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         IPv4 dotted-quad notation uses four decimal octets per
        ///         <see href="https://www.rfc-editor.org/rfc/rfc791#section-2.3">RFC 791 §2.3</see>.
        ///         Leading zeros in an octet are stripped before parsing to avoid unintended octal interpretation.
        ///     </para>
        /// </remarks>
        /// <param name="input">The IP address string (dotted-quad for IPv4, colon-hex for IPv6).</param>
        /// <returns>The parsed <see cref="IPAddress" />.</returns>
        public static IPAddress ParseIgnoreOctalInIPv4(string input)
        {
            #region defense

            if (input is null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            if (string.IsNullOrWhiteSpace(input))
            {
                throw new ArgumentException($"{nameof(input)} is in an invalid format", nameof(input));
            }

            #endregion // end: defense

            if (DottedQuadStringRegularExpression.IsMatch(input))
            {
                input = DottedQuadLeadingZerosRegularExpression.Replace(input, string.Empty); // remove leading zeros
            }

            return IPAddress.Parse(input);
        }

        /// <summary>
        ///     Attempts to parse an IP address string, stripping leading zeros from dotted-quad octets.
        /// </summary>
        /// <param name="input">The IP address string.</param>
        /// <param name="address">The parsed <see cref="IPAddress" />, or <see langword="null" /> on failure.</param>
        /// <returns><see langword="true" /> if parsing succeeded.</returns>
        public static bool TryParseIgnoreOctalInIPv4(string input,
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
            [NotNullWhen(true)]
#endif
            out IPAddress address)
        {
            if (input == null)
            {
                address = null;
                return false;
            }

            try
            {
                address = ParseIgnoreOctalInIPv4(input);
                return true;
            }
            catch
            {
                address = null;
                return false;
            }
        }

        #endregion

        #region Parse byte[]

        /// <summary>
        ///     Parses a big-endian byte array into an <see cref="IPAddress" />, zero-padding the MSB side as needed.
        /// </summary>
        /// <param name="input">The big-endian byte array.</param>
        /// <param name="addressFamily">The target address family.</param>
        /// <returns>The parsed <see cref="IPAddress"/>.</returns>
        public static IPAddress Parse(byte[] input, AddressFamily addressFamily)
        {
            if (input is null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            var expectedByteCount = addressFamily switch
            {
                AddressFamily.InterNetwork => IPv4ByteCount,
                AddressFamily.InterNetworkV6 => IPv6ByteCount,
                _ => throw new ArgumentOutOfRangeException(nameof(addressFamily)),
            };

            if (input.Length > expectedByteCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(input),
                    $"{nameof(input)} length is greater than the expected byte count of {expectedByteCount} for {addressFamily}"
                );
            }

            return new IPAddress(BigEndianBitWrapper.FromBytes(input, expectedByteCount).ToBytes());
        }

        /// <summary>
        ///     Attempts to parse a big-endian byte array into an <see cref="IPAddress" />.
        /// </summary>
        /// <param name="input">The big-endian byte array.</param>
        /// <param name="addressFamily">The target address family.</param>
        /// <param name="address">The parsed <see cref="IPAddress"/>, or <see langword="null" /> on failure.</param>
        /// <returns><see langword="true" /> if parsing succeeded.</returns>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
        public static bool TryParse(byte[] input, AddressFamily addressFamily, [NotNullWhen(true)] out IPAddress address)
#else
        public static bool TryParse(byte[] input, AddressFamily addressFamily, out IPAddress address)
#endif
        {
            if (input is null)
            {
                address = null;
                return false;
            }

            try
            {
                address = Parse(input, addressFamily);
                return true;
            }
            catch
            {
                address = null;
                return false;
            }
        }

        #endregion // end: Parse byte[]

        #endregion

        #region IsPrivate

        /// <summary>
        ///     Determines if an <see cref="IPAddress"/> is a private address.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         Private address blocks (<c>10.0.0.0/8</c>, <c>172.16.0.0/12</c>, <c>192.168.0.0/16</c>, <c>fd00::/8</c>)
        ///         are defined in <see href="https://www.rfc-editor.org/rfc/rfc1918#section-3">RFC 1918 §3</see>.
        ///     </para>
        /// </remarks>
        /// <param name="address">the input address</param>
        /// <returns><see langword="true"/> if, and only if, the <paramref name="address"/> is private.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="address"/> is <see langword="null"/></exception>
        public static bool IsPrivate(this IPAddress address)
        {
            if (address is null)
            {
                throw new ArgumentNullException(nameof(address));
            }

            return SubnetUtilities.PrivateIPAddressRangesList.Any(subnet => subnet.Contains(address));
        }

        #endregion end: IsPrivate
    }
}
