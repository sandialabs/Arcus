using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Text.RegularExpressions;
using Arcus.Comparers;
using Arcus.Converters;
using Arcus.Math;
using Arcus.Utilities;
using JetBrains.Annotations;

namespace Arcus
{
    /// <summary>
    ///     An IPv4 or IPv6 subnetwork representation - the work horse and original intention of the Arcus library
    /// </summary>
    public class Subnet : AbstractIPAddressRange,
                          IEquatable<Subnet>,
                          IComparable<Subnet>
    {
        /// <summary>
        ///     Bits in an IPv4 address
        /// </summary>
        private const int IPv4BitCount = 32;

        /// <summary>
        ///     number of octets in IPv4 address
        /// </summary>
        private const int IPv4OctetCount = 4;

        /// <summary>
        ///     Pattern that passes on valid IPv4 octet partials
        /// </summary>
        private const string IPv4OctetPartialPattern = @"^(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)(\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)){0,2}(\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)?)?$";

        /// <summary>
        ///     Bits in IPv6 address
        /// </summary>
        private const int IPv6BitCount = 128;

        /// <summary>
        ///     Hextets in IPv6 Address
        /// </summary>
        private const int IPv6HextetCount = 8;

        /// <summary>
        ///     the length of the subnet
        /// </summary>
        private BigInteger _length;

        /// <summary>
        ///     <see langword="true" /> if the subnet describes a single ip address
        /// </summary>
        public bool IsSingleIP => this.Length == 1;

        /// <summary>
        ///     The length of a <see cref="IIPAddressRange" />
        /// </summary>
        public override BigInteger Length => this._length;

        /// <summary>
        ///     the number of usable addresses in the subnet (ignores Broadcast and Network)
        /// </summary>
        public BigInteger UsableHostAddressCount => this.Length >= 2
                                                        ? this.Length - 2
                                                        : 0;

        /// <summary>
        ///     the broad cast of the subnet (highest order ip address)
        /// </summary>
        public IPAddress BroadcastAddress { get; private set; }

        /// <summary>
        ///     The head of a <see cref="IIPAddressRange" />
        /// </summary>
        /// <exception cref="NotSupportedException" accessor="set">head may only be assigned during construction</exception>
        public override IPAddress Head
        {
            get { return this.NetworkPrefixAddress; }

            set { throw new NotSupportedException("head may only be assigned during construction"); }
        }

        /// <summary>
        ///     the calculated Netmask of the subnet
        /// </summary>
        public IPAddress Netmask { get; private set; }

        /// <summary>
        ///     the network prefix of the subnet (lowest order IP address)
        /// </summary>
        public IPAddress NetworkPrefixAddress { get; private set; }

        /// <summary>
        ///     the routing prefix used to specify the ip address
        /// </summary>
        public int RoutingPrefix { get; private set; }

        /// <summary>
        ///     The tail of a <see cref="IIPAddressRange" />
        /// </summary>
        /// <exception cref="NotSupportedException" accessor="set">tail may only be assigned during construction</exception>
        public override IPAddress Tail
        {
            get { return this.BroadcastAddress; }
            set { throw new NotSupportedException("tail may only be assigned during construction"); }
        }

        #region From Interface IComparable<Subnet>

        /// <summary>
        ///     Compares the current object with another object of the same type.
        /// </summary>
        /// <returns>
        ///     A value that indicates the relative order of the objects being compared. The return value has the following
        ///     meanings: Value Meaning Less than zero This object is less than the <paramref name="other" /> parameter.Zero This
        ///     object is equal to <paramref name="other" />. Greater than zero This object is greater than
        ///     <paramref name="other" />.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public int CompareTo(Subnet other) => new DefaultSubnetComparer().Compare(this, other);

        #endregion

        /// <summary>
        ///     check if a given subnet falls within the specified subnet
        /// </summary>
        /// <param name="subnet"></param>
        /// <returns>true if the passed subnet is contained within this</returns>
        public bool Contains(Subnet subnet) => subnet != null && Contains(subnet.NetworkPrefixAddress) && Contains(subnet.BroadcastAddress);

        /// <exception cref="ArgumentException">Routing prefix is out of range</exception>
        private void InitSubnetFromIPAndRoute([NotNull] IPAddress ipAddress,
                                              [CanBeNull] int? routingPrefix)
        {
            if (ipAddress == null)
            {
                throw new ArgumentNullException(nameof(ipAddress));
            }

            // determine address type
            var isIPv4 = ipAddress.AddressFamily == AddressFamily.InterNetwork;
            var isIPv6 = ipAddress.AddressFamily == AddressFamily.InterNetworkV6;

            if (!isIPv4
                && !isIPv6)
            {
                throw new ArgumentException("IP Address must be IPv4 or IPv6", nameof(ipAddress));
            }

            this.RoutingPrefix = routingPrefix ?? (isIPv4
                                                       ? IPv4BitCount
                                                       : IPv6BitCount); // assign routing prefix

            // validate routing prefix
            if (this.RoutingPrefix < 0
                || (isIPv4 && this.RoutingPrefix > IPv4BitCount)
                || (isIPv6 && this.RoutingPrefix > IPv6BitCount))
            {
                throw new ArgumentException("Routing prefix is out of range", nameof(routingPrefix));
            }

            var addressBytes = ipAddress.GetAddressBytes(); // get the byte of the ip address
            var addressByteLength = addressBytes.Length * 8; // get the number of bytes in the address

            // calculate net mask
            var netmaskBytes = ByteArrayUtilities.CreateFilledByteArray(isIPv6
                                                                            ? 16
                                                                            : 4)
                                                 .ShiftBitsLeft(addressByteLength - this.RoutingPrefix);

            this.Netmask = new IPAddress(netmaskBytes); // assign net mask as ip address

            var networkBytes = addressBytes.BitwiseAnd(netmaskBytes);
            this.NetworkPrefixAddress = new IPAddress(networkBytes);

            var broadcastBytes = addressBytes.BitwiseOr(netmaskBytes.BitwiseNot());
            this.BroadcastAddress = new IPAddress(broadcastBytes);

            this._length = BigInteger.Pow(2, (isIPv4
                                                  ? IPv4BitCount
                                                  : IPv6BitCount) - this.RoutingPrefix);
        }

        /// <summary>
        ///     check if the given subnets overlaps this
        /// </summary>
        /// <param name="subnet">the subnet to check the overlap of</param>
        /// <returns>true if there is an overlap</returns>
        public bool Overlaps(Subnet subnet) => subnet != null && (subnet.Contains(this) || this.Contains(subnet));

        /// <summary>
        ///     To string method that will treat a single IP subnet as a single ip without CIDR notation
        /// </summary>
        /// <returns>the friendly version of the subnet as a CIDR or single address</returns>
        [NotNull]
        public string ToFriendlyString()
        {
            return this.IsSingleIP
                       ? string.Format("{0}", this.NetworkPrefixAddress)
                       : string.Format("{0}/{1}", this.NetworkPrefixAddress, this.RoutingPrefix);
        }

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return string.Format("{0}/{1}", this.NetworkPrefixAddress, this.RoutingPrefix);
        }

        #region constructors

        /// <summary>
        ///     Construct the smallest possible subnet that would contain both IP addresses encoded as bytes typically the address
        ///     specified are the Network and Broadcast addresses (lower and higher bounds) but this is not necessary. Addresses
        ///     *MUST* be the same address family (either Internetwork or InternetworkV6)
        /// </summary>
        /// <param name="primary">byte representation of primary address</param>
        /// <param name="secondary">byte representation of secondary address</param>
        /// <exception cref="ArgumentException"></exception>
        public Subnet([NotNull] byte[] primary,
                      [NotNull] byte[] secondary)
            : this(new IPAddress(primary), new IPAddress(secondary))
        {
            // no more work need be done.
        }

        /// <summary>
        ///     Construct the smallest possible subnet that would contain both IP addresses
        ///     typically the address specified are the Network and Broadcast addresses
        ///     (lower and higher bounds) but this is not necessary.
        ///     Addresses *MUST* be the same address family (either Internetwork or InternetworkV6)
        /// </summary>
        /// <param name="primary">a address to be contained within the subnet</param>
        /// <param name="secondary">another address to be contained within the subnet</param>
        /// <exception cref="ArgumentException"></exception>
        public Subnet([NotNull] IPAddress primary,
                      [NotNull] IPAddress secondary)
        {
            if (primary == null
                || secondary == null
                || primary.AddressFamily != secondary.AddressFamily
                || !(primary.AddressFamily == AddressFamily.InterNetwork || primary.AddressFamily == AddressFamily.InterNetworkV6))
            {
                throw new ArgumentException();
            }

            var primaryBytes = primary.GetAddressBytes();
            var secondaryBytes = secondary.GetAddressBytes();

            // XNOR to find common bytes between primary and secondary
            var commonBytes = primaryBytes.BitwiseXNor(secondaryBytes);

            var routingPrefix = primary.AddressFamily == AddressFamily.InterNetwork
                                    ? IPv4BitCount
                                    : IPv6BitCount;

            // find the first most significant bit that is not common to both addresses
            for (var i = 0; i < commonBytes.Length * 8; i++)
            {
                var bitMask = (byte) (0x80 >> i % 8); // bit mask built from current bit position in byte

                if ((commonBytes[i / 8] & bitMask) != 0) // if set bits are common
                {
                    continue;
                }

                routingPrefix = i;
                break;
            }

            this.InitSubnetFromIPAndRoute(primary, routingPrefix);
        }

        /// <summary>
        ///     Construct a new Subnet
        /// </summary>
        /// <param name="ipAddress">the ip address</param>
        /// <param name="routingPrefix">the routing prefix</param>
        /// <exception cref="ArgumentException">IP Address must be IPv4 or IPv6</exception>
        /// <exception cref="ArgumentException">Routing prefix is out of range</exception>
        public Subnet([NotNull] IPAddress ipAddress,
                      int routingPrefix)
        {
            this.InitSubnetFromIPAndRoute(ipAddress, routingPrefix);
        }

        /// <summary>
        ///     Construct using an IP Address String
        /// </summary>
        /// <param name="ipAddressString"></param>
        /// <param name="routingPrefix"></param>
        public Subnet([NotNull] string ipAddressString,
                      int routingPrefix)
            : this(IPAddress.Parse(ipAddressString), routingPrefix) {}

        /// <summary>
        ///     Construct a new Singleton Subnet (contains only a single ip address)
        /// </summary>
        /// <param name="ipAddress">the ip address</param>
        public Subnet([NotNull] IPAddress ipAddress)
        {
            this.InitSubnetFromIPAndRoute(ipAddress, null);
        }

        #endregion

        #region static constructors

        /// <summary>
        ///     Create a subnet from an IP Address and netmask
        /// </summary>
        /// <param name="ipAddress">the ip address</param>
        /// <param name="netmask">the net mask</param>
        /// <returns>The created subnet</returns>
        /// <exception cref="ArgumentNullException">ipAddress</exception>
        /// <exception cref="ArgumentNullException">netmask</exception>
        /// <exception cref="InvalidOperationException">the given IP Address is not IPv4</exception>
        /// <exception cref="InvalidOperationException">the given netmask is invalid</exception>
        public static Subnet FromNetMask([NotNull] IPAddress ipAddress,
                                         [NotNull] IPAddress netmask)
        {
            if (ipAddress == null)
            {
                throw new ArgumentNullException(nameof(ipAddress));
            }

            if (netmask == null)
            {
                throw new ArgumentNullException(nameof(netmask));
            }

            if (!ipAddress.IsIPv4())
            {
                throw new InvalidOperationException("IP Address must be IPv4");
            }

            if (!netmask.IsValidNetMask())
            {
                throw new InvalidOperationException(string.Format("the given netmask of '{0}' is invalid", netmask));
            }

            return new Subnet(ipAddress, netmask.NetmaskToCidrRoutePrefix());
        }

        #region parsing methods

        #region from string

        /// <summary>
        ///     Unsafe parsing of a string into a subnet
        /// </summary>
        /// <param name="input">the string to parse a subnet from</param>
        /// <returns>the parsed subnet</returns>
        /// <exception cref="ArgumentException">could not parse input</exception>
        public static Subnet Parse([CanBeNull] string input)
        {
            Subnet subnet;
            if (!TryParse(input, out subnet))
            {
                throw new ArgumentException(string.Format("Could not parse input '{0}' as IP Address", input));
            }
            return subnet;
        }

        /// <summary>
        ///     Attempt to parse a string into a subnet
        /// </summary>
        /// <param name="input">the string to parse</param>
        /// <param name="subnet">the resulting subnet</param>
        /// <returns>true on successful parse</returns>
        public static bool TryParse([CanBeNull] string input,
                                    out Subnet subnet)
        {
            IPAddress ipAddress;
            return TryParse(input, out subnet, out ipAddress);
        }

        /// <summary>
        ///     Try parse two strings as addresses with the same address family
        /// </summary>
        /// <param name="low">the low address string</param>
        /// <param name="high">the high address string</param>
        /// <param name="subnet">the resulting subnet</param>
        /// <returns>true on success</returns>
        public static bool TryParse([CanBeNull] string low,
                                    [CanBeNull] string high,
                                    out Subnet subnet)
        {
            IPAddress lowAddress = null;
            IPAddress highAddress = null;

            var success = low != null
                          && high != null
                          && IPAddress.TryParse(low, out lowAddress) // low parses
                          && IPAddress.TryParse(high, out highAddress) // high parses
                          && lowAddress.AddressFamily == highAddress.AddressFamily // address families the same
                          && (lowAddress.IsIPv4() || lowAddress.IsIPv6()) // IPv4 or IPv6
                          && highAddress.IsGreaterThanOrEqualTo(lowAddress); // high >= low

            if (!success)
            {
                subnet = null;
                return false;
            }

            subnet = new Subnet(lowAddress, highAddress);
            return true;
        }

        /// <summary>
        ///     Attempt to parse a string into a subnet
        /// </summary>
        /// <param name="input">the string to parse</param>
        /// <param name="subnet">the resulting subnet</param>
        /// <param name="ipAddress">the resulting ip address</param>
        /// <returns>true on successful parse</returns>
        public static bool TryParse([CanBeNull] string input,
                                    out Subnet subnet,
                                    out IPAddress ipAddress)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                subnet = null;
                ipAddress = null;
                return false;
            }

            var subnetParts = input.IndexOf('/') == -1
                                  ? new[] {input, null}
                                  : input.Split('/');

            // parse ip address
            IPAddress ipOut;
            if (!IPAddress.TryParse(subnetParts.First(), out ipOut))
            {
                ipAddress = null;
                subnet = null;
                return false;
            }

            int routingPrefix;
            if (subnetParts.Last() == null)
            {
                routingPrefix = ipOut.AddressFamily == AddressFamily.InterNetwork
                                    ? IPv4BitCount
                                    : IPv6BitCount; // assign routing prefix
            }
            else
            {
                // failed parsing ip address, route prefix
                if (!int.TryParse(subnetParts.Last(), out routingPrefix))
                {
                    subnet = null;
                    ipAddress = null;
                    return false;
                }
            }

            try
            {
                subnet = new Subnet(ipOut, routingPrefix);
            }
            catch // purposeful catch all
            {
                subnet = null;
                ipAddress = null;
                return false;
            }

            ipAddress = ipOut;
            return true;
        }

        #endregion

        #region from partial string

        /// <summary>
        ///     Try to convert a partial IPv4 address into a subnet based on found evedent octets
        /// </summary>
        /// <param name="ipPartial">the partial IP address to pase</param>
        /// <param name="subnet">the subnet created</param>
        /// <returns>true on success</returns>
        public static bool TryIPv4FromPartial([CanBeNull] string ipPartial,
                                              out Subnet subnet)
        {
            if (string.IsNullOrWhiteSpace(ipPartial)
                || !new Regex(IPv4OctetPartialPattern).IsMatch(ipPartial))
            {
                subnet = null;
                return false;
            }

            ipPartial = ipPartial.TrimEnd('.');
            var octetCount = ipPartial.Count(c => c == '.') + 1;

            subnet = new Subnet(string.Format("{0}{1}", ipPartial, string.Join("", Enumerable.Repeat(".0", IPv4OctetCount - octetCount))), octetCount * 8);
            return true;
        }

        /// <summary>
        ///     Try to convert an assumed partial IPv6 address into possible subnet based on given hextets
        /// </summary>
        /// <param name="cidrPartial">the partial ipv6 cidr</param>
        /// <param name="subnets">possible matching subnets</param>
        /// <returns>true on success</returns>
        public static bool TryIPv6FromPartial([CanBeNull] string cidrPartial,
                                              out IEnumerable<Subnet> subnets)
        {
            IPAddress address;
            if (string.IsNullOrWhiteSpace(cidrPartial))
            {
                subnets = Enumerable.Empty<Subnet>();
                return false;
            }

            // a IPv6 cidr partial is provided
            if ((IPAddress.TryParse(cidrPartial, out address) // treat as a complete address, may contain a '::' or not
                 || (cidrPartial.Contains("::") && IPAddress.TryParse(cidrPartial.TrimEnd(':'), out address))
                 || IPAddress.TryParse(cidrPartial.TrimEnd(':') + "::", out address))
                && address.IsIPv6()) // no collapse, but incomplete address
            {
                var trimmedPartial = cidrPartial.TrimEnd(':'); // remove trailing ":" or "::" if exists

                // break up entry on hextets, an empty hextet implies a collapse
                var hextets = trimmedPartial.Split(new[] {':'}, StringSplitOptions.None) // DO NOT remove empty splits
                                            .ToList();

                // should contain an empty, pump the first with appropriate values

                // get the index of the collapse 
                var collapse = hextets.Select((value,
                                               index) => new
                                                         {
                                                             value,
                                                             index
                                                         })
                                      .FirstOrDefault(pair => string.IsNullOrWhiteSpace(pair.value));

                int collapseIndex;
                if (collapse == null
                    && hextets.Count == 8) // no collapse - fully fledged address, all 8 hextets present
                {
                    subnets = new[] {new Subnet(cidrPartial, 128)};
                    return true;
                }

                // introduce a collapse at the end if one does not exist
                if (collapse == null) // not fully fledged, add a collapse to the end
                {
                    hextets.Add(string.Empty);
                    collapseIndex = hextets.Count - 1;
                }
                else // a collapse is present 
                {
                    collapseIndex = collapse.index;
                }

                var subnetsList = new List<Subnet>();

                var hextetCount = hextets.Count;
                var permutationLimit = IPv6HextetCount + 1 - hextetCount; // number of permutations of IP partial, every hextet available removes a permutation
                for (var permutation = 0; permutation <= permutationLimit; permutation++)
                {
                    var hextetsCopy = hextets.ToList();
                    hextetsCopy.RemoveAt(collapseIndex); // collapsed empty item
                    hextetsCopy.InsertRange(collapseIndex, Enumerable.Repeat("0", permutation)); // add zeros as appropriate

                    var addressString = string.Join(":", hextetsCopy); // re join string

                    IPAddress subnetAddress;
                    if (!IPAddress.TryParse(addressString + "::", out subnetAddress)
                        && !IPAddress.TryParse(addressString, out subnetAddress))
                    {
                        subnets = Enumerable.Empty<Subnet>();
                        return false;
                    }
                    var routePrefix = (permutation + hextetCount - 1) * 16;
                    subnetsList.Add(new Subnet(subnetAddress, routePrefix));
                }

                subnets = subnetsList;
                return true;
            }

            // a single subnet is explicitly defined
            Subnet singleSubnet;
            if (TryParse(cidrPartial, out singleSubnet)
                && singleSubnet.IsIPv6)
            {
                subnets = new[] {singleSubnet};
                return true;
            }

            subnets = Enumerable.Empty<Subnet>();
            return false;
        }

        #endregion

        #endregion

        #endregion

        #region equality and hashcode

        /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
        /// <returns>true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.</returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(Subnet other)
        {
            return !ReferenceEquals(null, other)
                   && (ReferenceEquals(this, other)
                       || this.NetworkPrefixAddress.Equals(other.NetworkPrefixAddress)
                       && this.RoutingPrefix == other.RoutingPrefix);
        }

        /// <summary>Determines whether the specified object is equal to the current object.</summary>
        /// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
        /// <param name="obj">The object to compare with the current object. </param>
        public override bool Equals(object obj)
        {
            return !ReferenceEquals(null, obj)
                   && (ReferenceEquals(this, obj)
                       || obj.GetType() == GetType()
                       && this.Equals((Subnet) obj));
        }

        /// <summary>Serves as the default hash function. </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return this.Head.GetHashCode() * 29
                       * AddressFamily.GetHashCode() * 187
                       * this.RoutingPrefix.GetHashCode();
            }
        }

        #endregion
    }
}
