using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Text.RegularExpressions;
using Arcus.Utilities;
using JetBrains.Annotations;

namespace Arcus.Converters
{
    /// <summary>
    ///     Static utility class containing conversion methods for converting <see cref="IPAddress" /> objects into something
    ///     else
    /// </summary>
    public static class IPAddressConverters
    {
        private const int IPv4BitCount = 32;

        #region byte[] conversion

        /// <summary>
        ///     Get bytes of address in proper Little Endian form
        /// </summary>
        /// <param name="ipAddress">the ip address to convert</param>
        /// <returns>the ip address as a little endian byte array</returns>
        /// <exception cref="ArgumentNullException"><paramref name="ipAddress" /> is <see langword="null" />.</exception>
        [NotNull]
        public static byte[] GetLittleEndianBytes([NotNull] this IPAddress ipAddress)
        {
            if (ipAddress == null)
            {
                throw new ArgumentNullException(nameof(ipAddress));
            }

            // ipAddress may contain an IPv6 address which is a 8 byte array (128 bits)
            var addressBytes = ipAddress.GetAddressBytes()
                                        .ToList();

            // take into account the littleendianness of the constructor call reversing the byte order
            addressBytes.Reverse();
            return addressBytes.ToArray();
        }

        #endregion

        #region int conversion

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
            if (netmask == null)
            {
                throw new ArgumentNullException(nameof(netmask));
            }

            if (!netmask.IsValidNetMask())
            {
                throw new InvalidOperationException("not a valid netmask");
            }

            var netmaskBytes = netmask.GetAddressBytes();
            var routingPrefix = IPv4BitCount;

            for (var i = 0; i < netmaskBytes.Length * 8; i++)
            {
                var bitMask = (byte) (0x80 >> i % 8); // bit mask built from current bit position in byte

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
        ///     
        ///     <remarks>The RFC is an April Fools Day Joke, but we implemented it anyhow</remarks>
        /// </summary>
        /// <param name="ipAddress">the ip address to convert</param>
        /// <returns>Ascii85/Base85 representation of IPv6 Address, or an empty string on failure</returns>
        [NotNull]
        public static string ToBase85String([CanBeNull] this IPAddress ipAddress) => ipAddress == null || !ipAddress.IsIPv6()
                                                                                         ? string.Empty
                                                                                         : string.Concat(ipAddress.ToUnsignedBigInteger()
                                                                                                                  .ToBase85Chars()
                                                                                                                  .Reverse())
                                                                                                 .PadLeft(18, '0');

        /// <summary>
        ///     Represent address as dotted quad (if not ipv6 simply return stringified version of input)
        /// </summary>
        /// <param name="ipAddress">the ip address to convert</param>
        /// <returns>dotted quad version of the given address</returns>
        /// <exception cref="ArgumentNullException"><paramref name="ipAddress" /> is <see langword="null" />.</exception>
        [NotNull]
        public static string ToDottedQuadString([NotNull] this IPAddress ipAddress)
        {
            if (ipAddress == null)
            {
                throw new ArgumentNullException(nameof(ipAddress));
            }

            if (!ipAddress.IsIPv6())
            {
                return ipAddress.ToString();
            }

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
                                            }) // get bytes in pairs with leading 0's to force unsigned form
                                    .Select(bs => BitConverter.ToInt32(bs, 0)) // convert byte pairs to 16 bit ints
                                    .Select(i => string.Format("{0:x}", i)); // combine bytes to a hex string

            var hextetString = string.Join(":", hextets); // join the hextets on a colon

            var longestMatch = new Regex(@"((:|\b)0\b)+") // find 0's surrounded by colons or word breaks
                .Matches(hextetString) // match across the hextet string
                .Cast<Match>()
                .Select(match => match.Value) // get the match value
                .OrderByDescending(s => s.StartsWith("0")
                                            ? s.Length + 1
                                            : s.Length)
                // order by length accounting for matches at beginning of string
                .FirstOrDefault(); // find the longest span of 0 valued hextets, or null if one does not exist

            if (longestMatch != null)
            {
                // get the first index of the longest match
                var index = hextetString.IndexOf(longestMatch, StringComparison.Ordinal);
                hextetString = hextetString.Remove(index, longestMatch.Length)
                                           .Insert(index, ":");
                // replace first occurrence with a ":" char
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
        [NotNull]
        public static string ToHexString([NotNull] this IPAddress ipAddress)
        {
            if (ipAddress == null)
            {
                throw new ArgumentNullException(nameof(ipAddress));
            }

            return BitConverter.ToString(ipAddress.GetAddressBytes())
                               .Replace("-", string.Empty);
        }

        /// <summary>
        ///     Convert an <see cref="IPAddress" /> to a numeric representation
        /// </summary>
        /// <param name="ipAddress">The ip address to convert</param>
        /// <returns>the BigInteger representation of the IP address</returns>
        /// <exception cref="ArgumentNullException"><paramref name="ipAddress" /> is <see langword="null" />.</exception>
        [NotNull]
        public static string ToNumericString([NotNull] this IPAddress ipAddress)
        {
            if (ipAddress == null)
            {
                throw new ArgumentNullException(nameof(ipAddress));
            }

            return ipAddress.ToUnsignedBigInteger()
                            .ToString();
        }

        /// <summary>
        ///     Convert to uncompressed IPv4/IPv6, adding zeros or explaing '::' where appropriate
        /// </summary>
        /// <param name="ipAddress">the address to expand</param>
        /// <returns>the expanded for of IPv4/IPv6, or ToString() otherwise</returns>
        /// <exception cref="ArgumentNullException"><paramref name="ipAddress" /> is <see langword="null" />.</exception>
        [NotNull]
        public static string ToUncompressedString([NotNull] this IPAddress ipAddress)
        {
            if (ipAddress == null)
            {
                throw new ArgumentNullException(nameof(ipAddress));
            }

            switch (ipAddress.AddressFamily)
            {
                case AddressFamily.InterNetwork:
                {
                    var strings = ipAddress.GetAddressBytes()
                                           .Select(b => string.Format("{0:D3}", b)); // format bytes with padded 0s

                    return string.Join(".", strings); // join padded strings with '.' character
                }
                case AddressFamily.InterNetworkV6:
                {
                    var strings = Enumerable.Range(0, 8) // create index
                                            .Select(i => ipAddress.GetAddressBytes()
                                                                  .ToList()
                                                                  .GetRange(i * 2, 2))
                        // get 8 chunks of bytes
                                            .Select(i =>
                                                    {
                                                        i.Reverse();
                                                        return i;
                                                    }) // reverse bytes for endianness
                                            .Select(bytes => BitConverter.ToInt16(bytes.ToArray(), 0))
                        // convert bytes to 16 bit int
                                            .Select(int16 => string.Format("{0:x4}", int16));
                    // format int as a 4 digit hex 

                    return string.Join(":", strings); // join hex integers with ':'
                }
                default:
                    return ipAddress.ToString(); // all else treat as to string
            }
        }

        #endregion

        #region BigInteger conversion

        /// <summary>
        ///     Convert an <see cref="IPAddress" /> to a <see cref="BigInteger" />
        /// </summary>
        /// <param name="ipAddress">The ip address to convert</param>
        /// <returns>the unsigned BigInteger representation of the IP address</returns>
        /// <exception cref="ArgumentNullException"><paramref name="ipAddress" /> is <see langword="null" />.</exception>
        public static BigInteger ToUnsignedBigInteger([NotNull] this IPAddress ipAddress)
        {
            if (ipAddress == null)
            {
                throw new ArgumentNullException(nameof(ipAddress));
            }

            // ipAddress may contain an IPv6 address which is a 8 byte array (128 bits)
            var addressBytes = ipAddress.GetAddressBytes()
                                        .ToList();

            // take into account the littleendianness of the constructor call reversing the byte order
            addressBytes.Reverse();

            addressBytes.Add(0x00); // append a 00 byte to the end of an array (thus forcing unsigned value)

            return new BigInteger(addressBytes.ToArray());
        }

        /// <summary>
        ///     Convert an <see cref="IPAddress" /> to a <see cref="BigInteger" />
        /// </summary>
        /// <param name="ipAddress">The ip address to convert</param>
        /// <returns>the BigInteger representation of the IP address</returns>
        /// <exception cref="ArgumentNullException"><paramref name="ipAddress" /> is <see langword="null" />.</exception>
        public static BigInteger ToBigInteger([NotNull] this IPAddress ipAddress)
        {
            if (ipAddress == null)
            {
                throw new ArgumentNullException(nameof(ipAddress));
            }

            // ipAddress may contain an IPv6 address which is a 8 byte array (128 bits)
            var addressBytes = ipAddress.GetAddressBytes()
                                        .ToList();

            // take into account the littleendianness of the constructor call reversing the byte order
            addressBytes.Reverse();

            return new BigInteger(addressBytes.ToArray());
        }

        #endregion
    }
}
