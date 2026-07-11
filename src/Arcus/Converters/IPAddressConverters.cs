using System.Net;
using System.Net.Sockets;
using Arcus.Utilities;
#if !NET8_0_OR_GREATER
using System.Numerics;
#endif

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

namespace Arcus.Converters
{
    /// <summary>
    ///     Static utility class providing extension methods for converting <see cref="IPAddress" /> objects.
    /// </summary>
    public static class IPAddressConverters
    {
        #region integral conversion

        /// <summary>
        ///     Converts a valid IPv4 subnet mask to a CIDR route prefix length.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         The netmask must be a contiguous sequence of leading 1-bits per
        ///         <see href="https://www.rfc-editor.org/rfc/rfc950#section-2">RFC 950 §2</see>.
        ///         The resulting integer is the CIDR prefix length per
        ///         <see href="https://www.rfc-editor.org/rfc/rfc4632#section-2">RFC 4632 §2</see>.
        ///     </para>
        /// </remarks>
        /// <param name="netmask">The subnet mask to convert (IPv4 only).</param>
        /// <returns>The CIDR route prefix length (0-32).</returns>
        /// <exception cref="ArgumentNullException"><paramref name="netmask" /> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException"><paramref name="netmask" /> is not a valid subnet mask.</exception>
        public static int NetmaskToCidrRoutePrefix(this IPAddress netmask)
        {
            #region defense

            if (netmask is null)
            {
                throw new ArgumentNullException(nameof(netmask));
            }

            if (!netmask.IsValidNetMask())
            {
                throw new ArgumentException("The provided address is not a valid netmask.", nameof(netmask));
            }

            #endregion // end: defense

            var netmaskBytes = netmask.GetAddressBytes();
            var routingPrefix = IPAddressUtilities.IPv4BitCount;

            for (var i = 0; i < IPAddressUtilities.IPv4BitCount; i++)
            {
                var bitMask = (byte)(0x80 >> (i % 8)); // bit mask built from current bit position in byte

                if ((netmaskBytes[i / 8] & bitMask) != 0) // if set bits are common
                {
                    continue;
                }

                routingPrefix = i;
                break;
            }

            return routingPrefix;
        }

        #endregion

        #region string conversion

        /// <summary>
        ///     Converts an IPv6 address to a Base85 (Ascii85) string per RFC 1924.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         Encoding defined in <see href="https://www.rfc-editor.org/rfc/rfc1924">RFC 1924</see>
        ///         (an April Fools Day joke, but implemented here regardless).
        ///     </para>
        /// </remarks>
        /// <param name="ipAddress">The IPv6 address to convert.</param>
        /// <returns>Base85 representation, or <see langword="null" /> if the address is not IPv6.</returns>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
        [return: MaybeNull]
#endif
        public static string ToBase85String(this IPAddress ipAddress)
        {
            if (ipAddress?.IsIPv6() is not true)
            {
                return null;
            }

            const string base85Alphabet =
                "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz!#$%&()*+-;<=>?@^_`{|}~";
            var chars = new char[20];
            var addressBytes = ipAddress.GetAddressBytes();

#if NET8_0_OR_GREATER
            ulong high = 0;
            ulong low = 0;
            for (var i = 0; i < 8; i++)
            {
                high = (high << 8) | addressBytes[i];
            }

            for (var i = 8; i < 16; i++)
            {
                low = (low << 8) | addressBytes[i];
            }

            var value128 = new UInt128(high, low);
            for (var i = 19; i >= 0; i--)
            {
                (value128, var remainder) = UInt128.DivRem(value128, 85);
                chars[i] = base85Alphabet[(int)remainder];
            }
#else
            var leBytes = new byte[17]; // zero-initialized; index 16 stays 0 (sign byte)
            for (var i = 0; i < 16; i++)
            {
                leBytes[i] = addressBytes[15 - i];
            }

            var value = new BigInteger(leBytes);
            for (var i = 19; i >= 0; i--)
            {
                value = BigInteger.DivRem(value, 85, out var remainder);
                chars[i] = base85Alphabet[(int)remainder];
            }
#endif

            return new string(chars);
        }

        /// <summary>
        ///     Converts an IP address to dotted-quad notation (IPv4), or mixed IPv6/IPv4 notation for IPv4-mapped IPv6 addresses.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         IPv4 dotted-quad notation (four decimal octets separated by dots) per
        ///         <see href="https://www.rfc-editor.org/rfc/rfc791#section-2.3">RFC 791 §2.3</see>.
        ///     </para>
        ///     <para>
        ///         For IPv6 addresses the first 96 bits (6 hextets) are rendered in compressed IPv6 notation -
        ///         the longest consecutive run of zero-valued hextets is collapsed to <c>::</c> per
        ///         <see href="https://www.rfc-editor.org/rfc/rfc5952#section-4">RFC 5952 §4</see> - followed
        ///         by the trailing 32 bits as an IPv4 dotted-quad suffix.
        ///     </para>
        /// </remarks>
        /// <param name="ipAddress">the ip address to convert</param>
        /// <returns>dotted quad version of the given address</returns>
        /// <exception cref="ArgumentNullException"><paramref name="ipAddress" /> is <see langword="null" />.</exception>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
        [return: NotNullIfNotNull(nameof(ipAddress))]
#endif
        public static string ToDottedQuadString(this IPAddress ipAddress)
        {
            if (ipAddress?.IsIPv6() is not true)
            {
                return ipAddress?.ToString();
            }

#if NET8_0_OR_GREATER
            Span<byte> addressBytes = stackalloc byte[16];
            if (!ipAddress.TryWriteBytes(addressBytes, out var bytesWritten) || bytesWritten != 16)
            {
                return ipAddress.ToString();
            }
            Span<ushort> hextets = stackalloc ushort[6];
#else
            var addressBytes = ipAddress.GetAddressBytes();
            var hextets = new ushort[6];
#endif

            for (var i = 0; i < 6; i++)
            {
                hextets[i] = (ushort)((addressBytes[i * 2] << 8) | addressBytes[(i * 2) + 1]);
            }

            var (bestStart, bestLen) = FindLongestZeroRun(hextets);

#if NET8_0_OR_GREATER
            Span<char> hexBuffer = stackalloc char[4];
#endif

            var sb = new System.Text.StringBuilder(45);
#if NET8_0_OR_GREATER
            AppendIPv6Prefix(sb, hextets, bestStart, bestLen, hexBuffer);
#else
            AppendIPv6Prefix(sb, hextets, bestStart, bestLen);
#endif

            sb.Append(addressBytes[12]);
            sb.Append('.');
            sb.Append(addressBytes[13]);
            sb.Append('.');
            sb.Append(addressBytes[14]);
            sb.Append('.');
            sb.Append(addressBytes[15]);

            return sb.ToString();

#if NET8_0_OR_GREATER
            static (int BestStart, int BestLen) FindLongestZeroRun(ReadOnlySpan<ushort> hextets)
#else
            static (int BestStart, int BestLen) FindLongestZeroRun(ushort[] hextets)
#endif
            {
                var bestStart = -1;
                var bestLength = 0;
                var runStart = -1;
                var runLength = 0;

                for (var i = 0; i < 6; i++)
                {
                    if (hextets[i] == 0)
                    {
                        if (runStart < 0)
                        {
                            runStart = i;
                        }

                        runLength++;
                        if (runLength > bestLength)
                        {
                            bestStart = runStart;
                            bestLength = runLength;
                        }
                    }
                    else
                    {
                        runStart = -1;
                        runLength = 0;
                    }
                }

                return (bestStart, bestLength);
            }

#if NET8_0_OR_GREATER
            static void AppendIPv6Prefix(
                System.Text.StringBuilder sb,
                ReadOnlySpan<ushort> hextets,
                int bestStart,
                int bestLen,
                Span<char> hexBuffer
            )
#else
            static void AppendIPv6Prefix(System.Text.StringBuilder sb, ushort[] hextets, int bestStart, int bestLen)
#endif
            {
                if (bestLen <= 0)
                {
#if NET8_0_OR_GREATER
                    AppendHextetRange(sb, hextets, 0, 6, hexBuffer);
#else
                    AppendHextetRange(sb, hextets, 0, 6);
#endif
                    sb.Append(':');
                    return;
                }

#if NET8_0_OR_GREATER
                AppendHextetRange(sb, hextets, 0, bestStart, hexBuffer);
#else
                AppendHextetRange(sb, hextets, 0, bestStart);
#endif
                sb.Append("::");

                var afterRunEnd = bestStart + bestLen;
#if NET8_0_OR_GREATER
                AppendHextetRange(sb, hextets, afterRunEnd, 6, hexBuffer);
#else
                AppendHextetRange(sb, hextets, afterRunEnd, 6);
#endif

                if (afterRunEnd < 6)
                {
                    sb.Append(':');
                }
            }

#if NET8_0_OR_GREATER
            static void AppendHextetRange(
                System.Text.StringBuilder sb,
                ReadOnlySpan<ushort> hextets,
                int from,
                int to,
                Span<char> hexBuffer
            )
#else
            static void AppendHextetRange(System.Text.StringBuilder sb, ushort[] hextets, int from, int to)
#endif
            {
                for (var i = from; i < to; i++)
                {
                    if (i > from)
                    {
                        sb.Append(':');
                    }
#if NET8_0_OR_GREATER
                    hextets[i].TryFormat(hexBuffer, out var charsWritten, "x");
                    sb.Append(hexBuffer[..charsWritten]);
#else
                    sb.Append(hextets[i].ToString("x"));
#endif
                }
            }
        }

        /// <summary>
        ///     Converts an IP address to an uppercase hexadecimal string with no separators.
        /// </summary>
        /// <param name="ipAddress">The address to convert.</param>
        /// <returns>Hex string (e.g., "C0A80101" for 192.168.1.1), or <see langword="null" /> if input is <see langword="null" />.</returns>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
        [return: NotNullIfNotNull(nameof(ipAddress))]
#endif
        public static string ToHexString(this IPAddress ipAddress)
        {
            return ipAddress == null ? null : BigEndianBitWrapper.FromBytes(ipAddress.GetAddressBytes()).ToHexString();
        }

        /// <summary>
        ///     Converts an IP address to its decimal numeric string representation.
        /// </summary>
        /// <param name="ipAddress">The address to convert.</param>
        /// <returns>Decimal string of the unsigned integer value, or <see langword="null" /> if input is <see langword="null" />.</returns>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
        [return: NotNullIfNotNull(nameof(ipAddress))]
#endif
        public static string ToNumericString(this IPAddress ipAddress)
        {
            return ipAddress == null ? null : BigEndianBitWrapper.FromBytes(ipAddress.GetAddressBytes()).ToDecimalString();
        }

        /// <summary>
        ///     Converts an IP address to its fully-expanded (uncompressed) string form.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         For IPv6, produces the fully-expanded form (no <c>::</c> compression, all hextets zero-padded to 4 digits),
        ///         which is the inverse of the compressed form defined in
        ///         <see href="https://www.rfc-editor.org/rfc/rfc5952#section-4">RFC 5952 §4</see>.
        ///         For IPv4, produces three-digit zero-padded dotted-quad per
        ///         <see href="https://www.rfc-editor.org/rfc/rfc791#section-2.3">RFC 791 §2.3</see>.
        ///     </para>
        /// </remarks>
        /// <param name="ipAddress">The address to expand.</param>
        /// <returns>The expanded string form, or <see langword="null" /> if input is <see langword="null" />.</returns>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
        [return: NotNullIfNotNull(nameof(ipAddress))]
#endif
        public static string ToUncompressedString(this IPAddress ipAddress)
        {
            if (ipAddress == null)
            {
                return null;
            }

            return ipAddress.AddressFamily switch
            {
                AddressFamily.InterNetwork => IPv4ToString(),
                AddressFamily.InterNetworkV6 => IPv6ToString(),
                _ => ipAddress.ToString(), // all else treat as to string
            };

            string IPv4ToString()
            {
                var octets = ipAddress.GetAddressBytes().Select(octet => $"{octet:D3}");

                return string.Join(".", octets); // join padded strings with '.' character
            }

            string IPv6ToString()
            {
                var addressBytes = ipAddress.GetAddressBytes();
#if NET8_0_OR_GREATER
                const string hexDigits = "0123456789abcdef";
                Span<char> chars = stackalloc char[(IPAddressUtilities.IPv6HextetCount * 5) - 1];

                var pos = 0;
                for (var i = 0; i < IPAddressUtilities.IPv6HextetCount; i++)
                {
                    if (i > 0)
                    {
                        chars[pos++] = ':';
                    }
                    var hiByte = addressBytes[i * 2];
                    var loByte = addressBytes[(i * 2) + 1];
                    chars[pos++] = hexDigits[hiByte >> 4];
                    chars[pos++] = hexDigits[hiByte & 0x0F];
                    chars[pos++] = hexDigits[loByte >> 4];
                    chars[pos++] = hexDigits[loByte & 0x0F];
                }
                return new string(chars);
#else
                var hextets = Enumerable
                    .Range(0, IPAddressUtilities.IPv6HextetCount)
                    .Select(hextetIndex => hextetIndex * 2)
                    .Select(byteOffset => $"{addressBytes[byteOffset]:x2}{addressBytes[byteOffset + 1]:x2}");

                return string.Join(":", hextets);
#endif
            }
        }

        #endregion
    }
}
