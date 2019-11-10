using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using Gulliver;
using JetBrains.Annotations;

namespace Arcus.Utilities
{
    /// <summary>
    ///     Static utility class containing miscellaneous operations for <see cref="IPAddress" /> objects
    /// </summary>
    public static class IPAddressUtilities
    {
        private const int BitsPerByte = 8;
        private const string DottedQuadLeadingZerosPattern = @"(?<=^|\.)0+(?!\.|$)";
        private const string DottedQuadRegularExpressionPattern = @"^[0-9]{1,3}(\.[0-9]{1,3}){3}$"; // checks dotted quad format (does not verify validity of address)
        private const string HexLikePattern = "^[0-9a-f]*$";

        /// <summary>
        ///     number of bits in an IPv4 address
        /// </summary>
        public const int IPv4BitCount = 32;

        /// <summary>
        ///     number of bytes in an IPv4 address (4)
        /// </summary>
        public const int IPv4ByteCount = IPv4BitCount / BitsPerByte;

        /// <summary>
        ///     number of octets in an IPv4 address
        /// </summary>
        public const int IPv4OctetCount = 4;

        /// <summary>
        ///     number of bits in an IPv6 address
        /// </summary>
        public const int IPv6BitCount = 128;

        /// <summary>
        ///     number of bytes in an IPv6 address (16)
        /// </summary>
        public const int IPv6ByteCount = IPv6BitCount / BitsPerByte;

        /// <summary>
        ///     number of hextets in an IPv6 address
        /// </summary>
        public const int IPv6HextetCount = 8;

        private static readonly Regex HexLikeRegularExpression = new Regex(HexLikePattern, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        private static readonly Regex DottedQuadLeadingZerosRegularExpression = new Regex(DottedQuadLeadingZerosPattern, RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static readonly Regex DottedQuadStringRegularExpression = new Regex(DottedQuadRegularExpressionPattern, RegexOptions.Compiled | RegexOptions.CultureInvariant);

        /// <summary>
        ///     Maximum IPv4 Address value (255.255.255.255)
        /// </summary>
        [NotNull]
        public static readonly IPAddress IPv4MaxAddress = new IPAddress(uint.MaxValue);

        /// <summary>
        ///     Minimum IPv4 Address value (0.0.0.0)
        /// </summary>
        [NotNull]
        public static readonly IPAddress IPv4MinAddress = new IPAddress(0);

        /// <summary>
        ///     Maximum IPv6 value (ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff)
        /// </summary>
        [NotNull]
        public static readonly IPAddress IPv6MaxAddress = new IPAddress(new byte[] {0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff}, 0L);

        /// <summary>
        ///     Standard valid <see cref="AddressFamily" />
        /// </summary>
        [NotNull]
        public static readonly IReadOnlyCollection<AddressFamily> ValidAddressFamilies = new List<AddressFamily> {AddressFamily.InterNetwork, AddressFamily.InterNetworkV6}.AsReadOnly();

        /// <summary>
        ///     Minimum IPv6 value (::)
        /// </summary>
        [NotNull]
        public static readonly IPAddress IPv6MinAddress = new IPAddress(new byte[IPv6ByteCount], 0L);

        #region Address Max / Minimum

        /// <summary>
        ///     Get the Max Address for the given address family. (supports only <see cref="AddressFamily.InterNetwork" /> and
        ///     <see cref="AddressFamily.InterNetworkV6" />)
        /// </summary>
        /// <param name="addressFamily">the <see cref="AddressFamily"/></param>
        [NotNull]
        public static IPAddress MaxIPAddress(this AddressFamily addressFamily)
        {
            switch (addressFamily)
            {
                case AddressFamily.InterNetwork:
                    return IPv4MaxAddress;
                case AddressFamily.InterNetworkV6:
                    return IPv6MaxAddress;
                default:
                    throw new ArgumentException($"Unsupported address family \"{addressFamily}\"", nameof(addressFamily));
            }
        }

        /// <summary>
        ///     Get the Min Address for the given address family. (supports only <see cref="AddressFamily.InterNetwork" /> and
        ///     <see cref="AddressFamily.InterNetworkV6" />)
        /// </summary>
        /// <param name="addressFamily">the <see cref="AddressFamily"/></param>
        [NotNull]
        public static IPAddress MinIPAddress(this AddressFamily addressFamily)
        {
            switch (addressFamily)
            {
                case AddressFamily.InterNetwork:
                    return IPv4MinAddress;
                case AddressFamily.InterNetworkV6:
                    return IPv6MinAddress;
                default:
                    throw new ArgumentException($"Unsupported address family \"{addressFamily}\"", nameof(addressFamily));
            }
        }

        #endregion // end: Address Max / Minimum

        #region address family detection

        /// <summary>
        ///     Test if address is IPv4
        /// </summary>
        /// <param name="ipAddress">the IPAddress to test</param>
        /// <returns>true if ipv4</returns>
        public static bool IsIPv4([CanBeNull] this IPAddress ipAddress)
        {
            return ipAddress != null && ipAddress.AddressFamily == AddressFamily.InterNetwork;
        }

        /// <summary>
        ///     Test if address is IPv6
        /// </summary>
        /// <param name="ipAddress">the IPAddress to test</param>
        /// <returns>true if ipv6</returns>
        public static bool IsIPv6([CanBeNull] this IPAddress ipAddress)
        {
            return ipAddress != null && ipAddress.AddressFamily == AddressFamily.InterNetworkV6;
        }

        #endregion

        #region address format detection

        /// <summary>
        ///     Check if address is IPv4 mapped to IPv6
        ///     in accordance to rfc 4291 (https://tools.ietf.org/html/rfc4291) - 2.5.5.2.  IPv4-Mapped IPv6 Address
        /// </summary>
        /// <param name="ipAddress">the IPAddress to test</param>
        /// <returns>true if is an IPv4 address mapped to IPv6</returns>
        /// <exception cref="ArgumentNullException"><paramref name="ipAddress" /> is <see langword="null" />.</exception>
        public static bool IsIPv4MappedIPv6([CanBeNull] this IPAddress ipAddress)
        {
            if (ipAddress == null
                || !ipAddress.IsIPv6())
            {
                return false;
            }

            var addressBytes = ipAddress.GetAddressBytes();

            return addressBytes.Take(10)
                               .All(b => b == 0x00)
                   && addressBytes[10] == 0xff
                   && addressBytes[11] == 0xff;
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
                var bitMask = (byte) (0x80 >> (i % 8));
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
        [NotNull]
        public static IPAddress ParseFromHexString([NotNull] string input,
                                                   AddressFamily addressFamily)
        {
            #region defense

            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            if (string.IsNullOrWhiteSpace(input))
            {
                throw new ArgumentException($"{nameof(input)} is in an invalid format", nameof(input));
            }

            if (!ValidAddressFamilies.Contains(addressFamily))
            {
                throw new ArgumentException($"{nameof(addressFamily)} must be in {string.Join(", ", ValidAddressFamilies)}", nameof(addressFamily));
            }

            #endregion // end: defense

            // ignore "0x" prefix, and trim most significant 0s
            var byteString = (input.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
                                  ? input.Substring(2, input.Length - 2)
                                  : input).TrimStart('0');

            // if the byte string has an odd number of characters provide a single significant 0
            if (byteString.Length % 2 != 0)
            {
                byteString = "0" + byteString; // ensure string has an even number of characters, prefix with MSB 0
            }

            // fail if the string is composed of non-valid hex characters
            if (!HexLikeRegularExpression.IsMatch(byteString))
            {
                throw new ArgumentException($"{nameof(input)} is in an unexpected format", nameof(input));
            }

            // for each i%2, convert i, i+1 into a byte, reverse, and convert into an array
            var byteArray = Enumerable.Range(0, byteString.Length / 2)
                                      .Select(i => i * 2)
                                      .Select(i => Convert.ToByte(byteString.Substring(i, 2), 16))
                                      .ToArray();

            return Parse(byteArray, addressFamily);
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
                                                 [CanBeNull] out IPAddress address)
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
#pragma warning disable CA1031 // catch is purposely general
            catch
#pragma warning restore CA1031
            {
                address = null;
                return false;
            }
        }

        #endregion

        #region octal parsing

        /// <summary>
        ///     Converts an IP address string to an <see cref="System.Net.IPAddress" /> instance ignoring leading zeros (octal
        ///     notation) of dotted quad format.
        /// </summary>
        /// <param name="input">
        ///     A string that contains an IP address in dotted-quad notation for IPv4 and in colon-hexadecimal
        ///     notation for IPv6
        /// </param>
        /// <returns>An <see cref="System.Net.IPAddress" /> instance</returns>
        [NotNull]
        public static IPAddress ParseIgnoreOctalInIPv4([NotNull] string input)
        {
            #region defense

            if (input == null)
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
        ///     Converts an IP address string to an <see cref="System.Net.IPAddress" /> instance ignoring leading zeros (octal
        ///     notation) of dotted
        ///     quad format.
        /// </summary>
        /// <param name="input">The string to validate.</param>
        /// <param name="address">The <see cref="System.Net.IPAddress" /> version of the string.</param>
        /// <returns>true if ipString is a valid IP address; otherwise, false.</returns>
        public static bool TryParseIgnoreOctalInIPv4([CanBeNull] string input,
                                                     [CanBeNull] out IPAddress address)
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
#pragma warning disable CA1031 // catch is purposely general
            catch
#pragma warning restore CA1031
            {
                address = null;
                return false;
            }
        }

        #endregion

        #region Parse byte[]

        /// <summary>
        ///     Convert bytes (big endian) to an IP Address providing 0x00 MSB bytes as needed
        /// </summary>
        /// <param name="input">the big endian IPAddress</param>
        /// <param name="addressFamily">the desired address family</param>
        public static IPAddress Parse([NotNull] byte[] input,
                                      AddressFamily addressFamily)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
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
                    throw new ArgumentOutOfRangeException(nameof(addressFamily));
            }

            if (input.Length > expectedByteCount)
            {
                throw new ArgumentOutOfRangeException(nameof(input), $"{nameof(input)} length is greater than the expected byte count of {expectedByteCount} for {addressFamily}");
            }

            return new IPAddress(input.PadBigEndianMostSignificantBytes(expectedByteCount));
        }

        /// <summary>
        ///     Attempt to convert bytes (big endian) to IP address providing 0x00 MSB bytes as needed
        /// </summary>
        /// <param name="input">the big endian IPAddress</param>
        /// <param name="addressFamily">the desired address family</param>
        /// <param name="address">the address on success</param>
        /// <returns>true on success</returns>
        public static bool TryParse(byte[] input,
                                    AddressFamily addressFamily,
                                    [CanBeNull] out IPAddress address)
        {
            try
            {
                address = Parse(input, addressFamily);
                return true;
            }
#pragma warning disable CA1031 // catch is purposely general
            catch
#pragma warning restore CA1031
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
        /// <param name="address">the input address</param>
        /// <returns><see langword="true"/> iff the <paramref name="address"/> is private.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="address"/> is <see langword="null"/></exception>
        public static bool IsPrivate([NotNull] this IPAddress address)
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
