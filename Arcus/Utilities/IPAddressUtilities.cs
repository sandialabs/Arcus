using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Text.RegularExpressions;
using Arcus.Converters;
using JetBrains.Annotations;

namespace Arcus.Utilities
{
    /// <summary>
    ///     Static utility class containing miscellaneous operations for <see cref="IPAddress" /> objects
    /// </summary>
    public static class IPAddressUtilities
    {
        private const string DottedQuadLeadingZerosString = @"(?<=^|\.)0+(?!\.|$)";
        private const string DottedQuadRegularExpressionString = @"^[0-9]{1,3}(\.[0-9]{1,3}){3}$"; // checks dotted quad format (does not verify validity of address)
        private const int IPv4BitCount = 32;
        private const int IPv4ByteCount = IPv4BitCount / 8;
        private const int IPv6BitCount = 128;
        private const int IPv6ByteCount = IPv6BitCount / 8;

        private static readonly Regex DottedQuadLeadingZerosRegularExpression = new Regex(DottedQuadLeadingZerosString);
        private static readonly Regex DottedQuadStringRegularExpression = new Regex(DottedQuadRegularExpressionString);

        /// <summary>
        ///     Maximum IPv4 Address value (255.255.255.255)
        /// </summary>
        public static readonly IPAddress IPv4MaxAddress = new IPAddress(uint.MaxValue);

        /// <summary>
        ///     Minimum IPv4 Address value (0.0.0.0)
        /// </summary>
        public static readonly IPAddress IPv4MinAddress = new IPAddress(0);

        /// <summary>
        ///     Maximum IPv6 value (ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff)
        /// </summary>
        public static readonly IPAddress IPv6MaxAddress = new IPAddress(new byte[] {0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff}, 0L);

        /// <summary>
        ///     Minimum IPv6 value (::)
        /// </summary>
        public static readonly IPAddress IPv6MinAddress = new IPAddress(new byte[16], 0L);

        #region address family detection

        /// <summary>
        ///     Test if address is IPv4
        /// </summary>
        /// <param name="ipAddress">the IPAddress to test</param>
        /// <returns>true if ipv4</returns>
        public static bool IsIPv4([CanBeNull] this IPAddress ipAddress) => ipAddress != null && ipAddress.AddressFamily == AddressFamily.InterNetwork;

        /// <summary>
        ///     Test if address is IPv6
        /// </summary>
        /// <param name="ipAddress">the IPAddress to test</param>
        /// <returns>true if ipv6</returns>
        public static bool IsIPv6([CanBeNull] this IPAddress ipAddress) => ipAddress != null && ipAddress.AddressFamily == AddressFamily.InterNetworkV6;

        #endregion

        #region address format detection

        /// <summary>
        ///     Test if address is IPv4 mapped to IPv6
        /// </summary>
        /// <param name="ipAddress">the IPAddress to test</param>
        /// <returns>true if is an IPv5 address mapperd to IPv6</returns>
        /// <exception cref="ArgumentNullException"><paramref name="ipAddress" /> is <see langword="null" />.</exception>
        public static bool IsIPv4MappedIPv6([NotNull] this IPAddress ipAddress)
        {
            if (ipAddress == null)
            {
                throw new ArgumentNullException(nameof(ipAddress));
            }

            if (!ipAddress.IsIPv6())
            {
                return false;
            }

            var littleEndianBytes = ipAddress.GetLittleEndianBytes();
            return littleEndianBytes[4] == 0xff && littleEndianBytes[5] == 0xff;
        }

        /// <summary>
        ///     Determine if the given <see cref="IPAddress" /> is a valid net mask
        /// </summary>
        /// <param name="netmask">the netmask to test</param>
        /// <returns>true if the given input is a valid netmask</returns>
        public static bool IsValidNetMask([CanBeNull] this IPAddress netmask)
        {
            if (netmask == null
                || netmask.AddressFamily != AddressFamily.InterNetwork)
            {
                return false;
            }

            var set = true;
            var netmaskBytes = netmask.GetAddressBytes();
            for (var i = 0; i < netmaskBytes.Length * 8; i++)
            {
                var bitMask = (byte) (0x80 >> i % 8);
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
        ///     Attempt to parse a hex input string as an IP Address of the given family
        /// </summary>
        /// <param name="input">hex input</param>
        /// <param name="addressFamily">address family</param>
        /// <returns>IP Address, or <see langword="null" /> if parse fails</returns>
        [CanBeNull]
        public static IPAddress ParseFromHexString([CanBeNull] string input,
                                                   AddressFamily addressFamily)
        {
            IPAddress address;
            return TryParseFromHexString(input, addressFamily, out address)
                       ? address
                       : null;
        }

        /// <summary>
        ///     Try to parse an IPv4 or IPv6 <paramref name="address" /> from string containing hex numbers (possibly prefixed by
        ///     "0x")
        /// </summary>
        /// <param name="input">the string to attempt to parse</param>
        /// <param name="addressFamily">the desired address family</param>
        /// <param name="address">the address that was parsed</param>
        /// <returns>true on success</returns>
        public static bool TryParseFromHexString([CanBeNull] string input,
                                                 AddressFamily addressFamily,
                                                 out IPAddress address)
        {
            BigInteger addressBytesBigInteger;

            if (string.IsNullOrWhiteSpace(input))
            {
                address = null;
                return false;
            }

            // prefixing input w/ 0 forces BigInteger to be unsigned
            if (!(input.StartsWith("0x", StringComparison.OrdinalIgnoreCase) && BigInteger.TryParse("0" + input.Remove(0, 2), NumberStyles.HexNumber, null, out addressBytesBigInteger))
                && !BigInteger.TryParse("0" + input, NumberStyles.HexNumber, null, out addressBytesBigInteger))
            {
                address = null;
                return false;
            }

            var bytes = addressBytesBigInteger.ToByteArray()
                                              .ToArray();

            return TryParse(bytes, addressFamily, out address);
        }

        #endregion

        #region octal parsing

        /// <summary>
        ///     Converts an IP address string to an <see cref="System.Net.IPAddress" /> instance ignoring leading zeros (octal
        ///     notation) of dotted
        ///     quad format.
        /// </summary>
        /// <param name="input">
        ///     A string that contains an IP address in dotted-quad notation for IPv4 and in colon-hexadecimal
        ///     notation for IPv6
        /// </param>
        /// <returns>An <see cref="System.Net.IPAddress" /> instance</returns>
        [CanBeNull]
        public static IPAddress ParseIgnoreOctalInIPv4([CanBeNull] string input)
        {
            IPAddress address;
            return TryParseIgnoreOctalInIPv4(input, out address)
                       ? address
                       : null;
        }

        /// <summary>
        ///     Converts an IP address string to an <see cref="System.Net.IPAddress" /> instance ignoring leading zeros (octal
        ///     notation) of dotted
        ///     quad format.
        /// </summary>
        /// <param name="input">The string to validate.</param>
        /// <param name="address">The <see cref="System.Net.IPAddress" /> version of the string.</param>
        /// <returns>true if ipString is a valid IP address; otherwise, false.</returns>
        public static bool TryParseIgnoreOctalInIPv4([CanBeNull] string input,
                                                     out IPAddress address)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                address = null;
                return false;
            }

            if (DottedQuadStringRegularExpression.IsMatch(input))
            {
                input = DottedQuadLeadingZerosRegularExpression.Replace(input, ""); // remove leading zeros
            }

            IPAddress ip;
            var parseSuccess = IPAddress.TryParse(input, out ip);

            address = parseSuccess
                          ? ip
                          : null;

            return parseSuccess;
        }

        #endregion

        /// <summary>
        ///     Try to get an IP address from an (expected unsigned) big integer
        ///     If too few bytes add until enough are present
        /// </summary>
        /// <param name="input">a BigInteger to parse an IP address from</param>
        /// <param name="addressFamily">the desired address family</param>
        /// <param name="address">the address on success</param>
        /// <returns>true on success</returns>
        public static bool TryParse(BigInteger input,
                                    AddressFamily addressFamily,
                                    out IPAddress address)
        {
            address = null;

            if (input.Sign < 0)
            {
                return false;
            }

            int expectedByteCount;

            switch (addressFamily)
            {
                case AddressFamily.InterNetwork:
                    expectedByteCount = IPv4ByteCount;
                    break;

                case AddressFamily.InterNetworkV6:
                    expectedByteCount = IPv6ByteCount;
                    break;
                default:
                    return false;
            }

            var bytes = input.ToByteArray()
                             .AffixByteLength(expectedByteCount)
                             .Reverse()
                             .ToArray();

            address = new IPAddress(bytes);
            return true;
        }

        /// <summary>
        ///     Attempt to convert bytes (big endian) to IP address
        /// </summary>
        /// <param name="input">the big endian IPAddress</param>
        /// <param name="addressFamily">the desired address family</param>
        /// <param name="address">the address on sucess</param>
        /// <returns>true on success</returns>
        /// <exception cref="ArgumentNullException"><paramref name="input" /> is <see langword="null" />.</exception>
        public static bool TryParse([CanBeNull] byte[] input,
                                    AddressFamily addressFamily,
                                    out IPAddress address)
        {
            address = null;
            int expectedByteCount;

            switch (addressFamily)
            {
                case AddressFamily.InterNetwork:
                    expectedByteCount = IPv4ByteCount;
                    break;

                case AddressFamily.InterNetworkV6:
                    expectedByteCount = IPv6ByteCount;
                    break;
                default:
                    return false;
            }

            var bytes = input.AffixByteLength(expectedByteCount)
                             .Reverse()
                             .ToArray();

            address = new IPAddress(bytes);
            if (addressFamily.Equals(address.AddressFamily))
            {
                return true;
            }

            address = null;
            return false;
        }

        #endregion
    }
}
