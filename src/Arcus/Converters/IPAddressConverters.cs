using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Text.RegularExpressions;
using Arcus.Utilities;
using Gulliver;
using JetBrains.Annotations;

namespace Arcus.Converters
{
    /// <summary>
    ///     Static utility class containing conversion methods for converting <see cref="IPAddress" /> objects into something
    ///     else
    /// </summary>
    public static class IPAddressConverters
    {
        #region integral conversion

        /// <summary>
        ///     Convert a valid netmask (encoded as <see cref="IPAddress" />) into a CIDR route prefix
        ///     Only valid for IPv4 netmasks
        /// </summary>
        /// <param name="netmask">the netmask to convert</param>
        /// <returns>the route prefix</returns>
        /// <exception cref="InvalidOperationException"><paramref name="netmask" /> is <see langword="null" />.</exception>
        /// <exception cref="InvalidOperationException">not a valid netmask</exception>
        public static int NetmaskToCidrRoutePrefix([NotNull] this IPAddress netmask)
        {
            #region defense

            if (netmask == null)
            {
                throw new ArgumentNullException(nameof(netmask));
            }

            if (!netmask.IsValidNetMask())
            {
                throw new InvalidOperationException("not a valid netmask");
            }

            #endregion // end: defense

            var netmaskBytes = netmask.GetAddressBytes();
            var routingPrefix = IPAddressUtilities.IPv4BitCount;

            for (var i = 0; i < IPAddressUtilities.IPv4BitCount; i++)
            {
                var bitMask = (byte) (0x80 >> (i % 8)); // bit mask built from current bit position in byte

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
        ///     IPv6 to to Base85 (will return empty string for non ipv6 addresses) AKA Ascii85
        ///     from RFC 1924 ( http://tools.ietf.org/html/rfc1924 )
        ///     <remarks>
        ///         <para>The RFC is an April Fools Day Joke, but we implemented it anyhow</para>
        ///     </remarks>
        /// </summary>
        /// <param name="ipAddress">the ip address to convert</param>
        /// <returns>Ascii85/Base85 representation of IPv6 Address, or an empty string on failure</returns>
        [CanBeNull]
        public static string ToBase85String([CanBeNull] this IPAddress ipAddress)
        {
            if (ipAddress == null
                || !ipAddress.IsIPv6())
            {
                return null;
            }

            return string.Concat(GetBase85Chars(ipAddress)
                                     .Reverse())
                         .PadLeft(20, '0');

            IEnumerable<char> GetBase85Chars(IPAddress input)
            {
                const string alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz!#$%&()*+-;<=>?@^_`{|}~";

                // get little endian unsigned byte value
                var addressBytes = input.GetAddressBytes()
                                        .Reverse()
                                        .ToList();

                addressBytes.Add(0x00);

                // either rely on BigInteger, or implement byte based modulus and division
                var bigInteger = new BigInteger(addressBytes.ToArray());

                do
                {
                    bigInteger = BigInteger.DivRem(bigInteger, 85, out var charIndex);
                    yield return alphabet[(int) charIndex];
                }
                while (bigInteger > 0);
            }
        }

        /// <summary>
        ///     Represent address as dotted quad (if not ipv6 simply return stringified version of input)
        /// </summary>
        /// <param name="ipAddress">the ip address to convert</param>
        /// <returns>dotted quad version of the given address</returns>
        /// <exception cref="ArgumentNullException"><paramref name="ipAddress" /> is <see langword="null" />.</exception>
        [CanBeNull]
        public static string ToDottedQuadString([CanBeNull] this IPAddress ipAddress)
        {
            if (ipAddress == null)
            {
                return null;
            }

            if (!ipAddress.IsIPv6())
            {
                return ipAddress.ToString();
            }

            // TODO candidate for clean up / simplification
            var bytes = ipAddress.GetAddressBytes(); // get the bytes of the ip address

            var leadingBytes = bytes.Take(12)
                                    .ToArray(); // capture the non ipv4 bytes

            var hextets = Enumerable.Range(0, 6)
                                    .Select(i =>
                                            {
                                                var index = i * 2;
                                                return new byte[]
                                                       {
                                                           leadingBytes[index + 1],
                                                           leadingBytes[index],
                                                           0x0,
                                                           0x0
                                                       };
                                            })                                 // get bytes in pairs with leading 0's to force unsigned form
                                    .Select(bs => BitConverter.ToInt32(bs, 0)) // convert byte pairs to 16 bit integers
                                    .Select(i => $"{i:x}");                    // combine bytes to a hex string

            var hextetString = string.Join(":", hextets); // join the hextets on a colon

            var longestMatch = new Regex(@"((:|\b)0\b)+") // find 0's surrounded by colons or word breaks
                               .Matches(hextetString)     // match across the hextet string
                               .Cast<Match>()
                               .Select(match => match.Value) // get the match value
                               .OrderByDescending(s => s?.StartsWith("0", StringComparison.OrdinalIgnoreCase) == true
                                                           ? s.Length + 1
                                                           : s.Length) // order by length accounting for matches at beginning of string
                               .FirstOrDefault();                      // find the longest span of 0 valued hextets, or null if one does not exist

            if (longestMatch != null)
            {
                // get the first index of the longest match
                var index = hextetString.IndexOf(longestMatch, StringComparison.Ordinal);
                hextetString = hextetString.Remove(index, longestMatch.Length)
                                           .Insert(index, ":"); // replace first occurrence with a ":" char
            }

            var followingBytes = bytes.Skip(12)
                                      .ToArray(); // capture IPv4 bytes (last 4)
            var ipv4Address = new IPAddress(followingBytes).ToString();

            return hextetString + ":" + ipv4Address;
        }

        /// <summary>
        ///     Short hex value
        /// </summary>
        /// <param name="ipAddress">the ip address to convert</param>
        /// <returns>Hex version of the given IP Address</returns>
        /// <exception cref="ArgumentNullException"><paramref name="ipAddress" /> is <see langword="null" />.</exception>
        [CanBeNull]
        public static string ToHexString([CanBeNull] this IPAddress ipAddress)
        {
            return ipAddress?.GetAddressBytes()
                            .ToString("HC", CultureInfo.InvariantCulture);
        }

        /// <summary>
        ///     Convert an <see cref="IPAddress" /> to a numeric representation
        /// </summary>
        /// <param name="ipAddress">The ip address to convert</param>
        /// <returns>an integral representation of the IP address</returns>
        /// <exception cref="ArgumentNullException"><paramref name="ipAddress" /> is <see langword="null" />.</exception>
        [CanBeNull]
        public static string ToNumericString([CanBeNull] this IPAddress ipAddress)
        {
            return ipAddress?.GetAddressBytes()
                            .ToString("IBE", CultureInfo.InvariantCulture);
        }

        /// <summary>
        ///     Convert to uncompressed IPv4/IPv6, adding zeros or expanding '::' where appropriate
        /// </summary>
        /// <param name="ipAddress">the address to expand</param>
        /// <returns>the expanded for of IPv4/IPv6, or ToString() otherwise</returns>
        /// <exception cref="ArgumentNullException"><paramref name="ipAddress" /> is <see langword="null" />.</exception>
        [CanBeNull]
        public static string ToUncompressedString([CanBeNull] this IPAddress ipAddress)
        {
            if (ipAddress == null)
            {
                return null;
            }

            switch (ipAddress.AddressFamily)
            {
                case AddressFamily.InterNetwork:
                    return IPv4ToString();
                case AddressFamily.InterNetworkV6:
                    return IPv6ToString();
                default:
                    return ipAddress.ToString(); // all else treat as to string
            }

            string IPv4ToString()
            {
                var octets = ipAddress.GetAddressBytes()
                                      .Select(b => $"{b:D3}");

                return string.Join(".", octets); // join padded strings with '.' character
            }

            string IPv6ToString()
            {
                var addressBytes = ipAddress.GetAddressBytes();

                var hextets = Enumerable.Range(0, IPAddressUtilities.IPv6HextetCount)
                                        .Select(i => i * 2)
                                        .Select(i => $"{addressBytes[i]:x2}{addressBytes[i + 1]:x2}");

                return string.Join(":", hextets);
            }
        }

        #endregion
    }
}
