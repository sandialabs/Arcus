using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using Arcus.Comparers;
using Arcus.Converters;
using Arcus.Math;
using Arcus.Utilities;
using Gulliver;

namespace Arcus
{
    /// <summary>
    ///     An IPv4 or IPv6 subnetwork representation - the work horse and original intention of the Arcus library
    /// </summary>
    [Serializable]
    public class Subnet : AbstractIPAddressRange,
#if NETSTANDARD2_0
            ISerializable,
#endif
            IEquatable<Subnet>, IComparable<Subnet>, IComparable
    {
        /// <summary>
        ///     Pattern that passes on valid IPv4 octet partials
        /// </summary>
        private const string Ipv4OctetPartialPattern =
            @"^(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)(\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)){0,2}(\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)?)?$";

        /// <summary>
        ///     Pattern for rough shape of a subnet string
        /// </summary>
        private const string RoughSubnetStringPattern = @"^([\da-fA-F:.]+)(?:/([\d]+))?$";

        /// <summary>
        ///     Regex that passes on valid IPv4 octet partials
        /// </summary>
        private static readonly Regex IPv4OctetPartialRegex = new Regex(
            Ipv4OctetPartialPattern,
            RegexOptions.Compiled | RegexOptions.CultureInvariant
        );

        /// <summary>
        ///     Regex for rough shape of a subnet string
        /// </summary>
        private static readonly Regex RoughSubnetRegex = new Regex(
            RoughSubnetStringPattern,
            RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase
        );

        /// <summary>
        ///     Gets the number of usable addresses in the subnet (ignores Broadcast and Network addresses)
        /// </summary>
        /// <value>
        /// the number of usable addresses in the subnet (ignores Broadcast and Network addresses)
        /// </value>
        public BigInteger UsableHostAddressCount => Length >= 2 ? Length - 2 : 0;

        /// <summary>
        ///     Gets the broadcast of the subnet (highest order ip address)
        /// </summary>
        /// <value>
        /// the broadcast of the subnet (highest order ip address)
        /// </value>
        public IPAddress BroadcastAddress => Tail;

        /// <summary>
        ///     Gets the calculated Netmask of the subnet, only valid for IPv4 based subnets will be <see langword="null" /> on IPv6
        ///     subnets
        /// </summary>
        /// <value>
        /// the calculated Netmask of the subnet, only valid for IPv4 based subnets will be <see langword="null" /> on IPv6
        ///     subnets
        /// </value>
        public IPAddress Netmask { get; }

        /// <summary>
        ///     Gets the network prefix of the subnet (lowest order IP address)
        /// </summary>
        /// <value>
        /// the network prefix of the subnet (lowest order IP address)
        /// </value>
        public IPAddress NetworkPrefixAddress => Head;

        /// <summary>
        ///     Gets the routing prefix used to specify the ip address
        /// </summary>
        /// <value>
        /// the routing prefix used to specify the ip address
        /// </value>
        public int RoutingPrefix { get; }

        #region From Interface IComparable

        /// <inheritdoc />
        public int CompareTo(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return 1;
            }

            if (ReferenceEquals(this, obj))
            {
                return 0;
            }

            return obj is Subnet other
                ? this.CompareTo(other)
                : throw new ArgumentException($"Object must be of type {nameof(Subnet)}");
        }

        #endregion

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
        public int CompareTo(Subnet other)
        {
            return new DefaultIPAddressRangeComparer().Compare(this, other);
        }

        #endregion

        #region Formatting

        /// <inheritdoc />
        public override string ToString(string format, IFormatProvider formatProvider)
        {
            if (formatProvider == null)
            {
                formatProvider = CultureInfo.InvariantCulture;
            }

            switch (format?.Trim())
            {
                // unspecified
                case null:
                case "":

                // general formats
                case "g":
                case "G":
                    return $"{this.NetworkPrefixAddress}/{this.RoutingPrefix}";

                // "friendly" formats
                case "f":
                case "F":
                    return IsSingleIP ? $"{this.NetworkPrefixAddress}" : $"{this.NetworkPrefixAddress}/{this.RoutingPrefix}";

                // range formats
                case "r":
                case "R":
                    return $"{this.NetworkPrefixAddress} - {this.BroadcastAddress}";

                // delegate to base
                default:
                    return base.ToString(format, formatProvider);
            }
        }

        #endregion // end: Formatting

        #region set based operations

        /// <summary>
        ///     check if a given subnet falls within the specified subnet
        /// </summary>
        /// <param name="subnet">the subnet to test</param>
        /// <returns>true if the passed subnet is contained within this</returns>
        public bool Contains(Subnet subnet)
        {
            return subnet != null && Contains(subnet.NetworkPrefixAddress) && Contains(subnet.BroadcastAddress);
        }

        /// <summary>
        ///     check if the given subnets overlaps this
        /// </summary>
        /// <param name="subnet">the subnet to check the overlap of</param>
        /// <returns>true if there is an overlap</returns>
        public bool Overlaps(Subnet subnet)
        {
            return subnet != null && (subnet.Contains(this) || this.Contains(subnet));
        }

        #endregion // end: set based operations

        #region Ctor

        /// <summary>
        ///     Initializes a new instance of the <see cref="Subnet" /> class.
        ///     Construct the smallest possible subnet that would contain both IP addresses
        ///     typically the address specified are the Network and Broadcast addresses
        ///     (lower and higher bounds) but this is not necessary.
        ///     Addresses *MUST* be the same address family (either Internetwork or InternetworkV6)
        /// </summary>
        /// <param name="lowAddress">a address to be contained within the subnet</param>
        /// <param name="highAddress">another address to be contained within the subnet</param>
        public Subnet(IPAddress lowAddress, IPAddress highAddress)
            : base(CtorFactory(lowAddress, highAddress))
        {
            var result = NormalizeAndCreateNetMask(Head, Tail); // TODO the execution of this call is logically redundant as it is used in the call of the base constructor
            this.RoutingPrefix = result.Prefix;
            this.Netmask = IsIPv4 ? result.Mask : null; // only set netmask for IPv4 based subnets
        }

        private static AddressTuple CtorFactory(IPAddress lowAddress, IPAddress highAddress)
        {
            #region Defense

            if (lowAddress == null)
            {
                throw new ArgumentNullException(nameof(lowAddress));
            }

            if (highAddress == null)
            {
                throw new ArgumentNullException(nameof(highAddress));
            }

            if (lowAddress.AddressFamily != highAddress.AddressFamily)
            {
                throw new ArgumentException(
                    $"{nameof(lowAddress)} and {nameof(highAddress)} must have matching address families"
                );
            }

            if (!IPAddressUtilities.ValidAddressFamilies.Contains(lowAddress.AddressFamily))
            {
                throw new ArgumentException(
                    $"{nameof(lowAddress)} must have an address family equal to {string.Join(", ", IPAddressUtilities.ValidAddressFamilies)}",
                    nameof(lowAddress)
                );
            }

            if (!IPAddressUtilities.ValidAddressFamilies.Contains(highAddress.AddressFamily))
            {
                throw new ArgumentException(
                    $"{nameof(highAddress)} must have an address family equal to {string.Join(", ", IPAddressUtilities.ValidAddressFamilies)}",
                    nameof(highAddress)
                );
            }

            if (!highAddress.IsGreaterThanOrEqualTo(lowAddress))
            {
                throw new InvalidOperationException($"{nameof(highAddress)} must be greater or equal to {nameof(lowAddress)}");
            }

            #endregion // end: Defense

            var result = NormalizeAndCreateNetMask(lowAddress, highAddress);
            return new AddressTuple(result.Head, result.Tail);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Subnet" /> class.
        /// </summary>
        /// <param name="address">the ip address</param>
        /// <param name="routingPrefix">the routing prefix</param>
        /// <exception cref="ArgumentException">IP Address must be IPv4 or IPv6</exception>
        /// <exception cref="ArgumentException">Routing prefix is out of range</exception>
        public Subnet(IPAddress address, int routingPrefix)
            : base(CtorFactory(address, routingPrefix))
        {
            var result = NormalizeAndCreateNetMask(Head, routingPrefix); // TODO the execution of this call is logically redundant as it is used in the call of the base constructor
            this.RoutingPrefix = routingPrefix;
            this.Netmask = IsIPv4 ? result.Mask : null; // only set netmask for IPv4 based subnets
        }

        private static AddressTuple CtorFactory(IPAddress address, int routingPrefix)
        {
            #region Defense

            if (address == null)
            {
                throw new ArgumentNullException(nameof(address));
            }

            if (!IPAddressUtilities.ValidAddressFamilies.Contains(address.AddressFamily))
            {
                throw new ArgumentException(
                    $"{nameof(address)} must have an address family equal to {string.Join(", ", IPAddressUtilities.ValidAddressFamilies)}",
                    nameof(address)
                );
            }

            if (routingPrefix < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(routingPrefix));
            }

            if (address.IsIPv4() && routingPrefix > IPAddressUtilities.IPv4BitCount)
            {
                throw new ArgumentOutOfRangeException(nameof(routingPrefix));
            }

            if (address.IsIPv6() && routingPrefix > IPAddressUtilities.IPv6BitCount)
            {
                throw new ArgumentOutOfRangeException(nameof(routingPrefix));
            }

            #endregion // end: Defense

            var result = NormalizeAndCreateNetMask(address, routingPrefix);
            return new AddressTuple(result.Head, result.Tail);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Subnet" /> class.
        ///     contains only a single ip address
        /// </summary>
        /// <param name="address">the ip address</param>
        public Subnet(IPAddress address)
            : base(address, address)
        {
            this.RoutingPrefix = IsIPv4 ? IPAddressUtilities.IPv4BitCount : IPAddressUtilities.IPv6BitCount;

            if (IsIPv6)
            {
                return;
            }

            var netmaskBytes = Enumerable.Repeat((byte)0xff, IPAddressUtilities.IPv4ByteCount).ToArray();

            this.Netmask = new IPAddress(netmaskBytes);
        }

        /// <summary>Initializes a new instance of the <see cref="Subnet"/> class.</summary>
        /// <param name="info">serialization info</param>
        /// <param name="context">serialization context</param>
        /// <exception cref="ArgumentNullException"><paramref name="info"/> is <see langword="null"/></exception>
        protected Subnet(SerializationInfo info, StreamingContext context)
            : this(
                new IPAddress(
                    (byte[])
                        (info ?? throw new ArgumentNullException(nameof(info))).GetValue(
                            nameof(BroadcastAddress),
                            typeof(byte[])
                        )
                ),
                (int)info.GetValue(nameof(RoutingPrefix), typeof(int))
            ) { }

        #endregion // end: Ctor

        #region static factory methods

        #region FromNetMask

        /// <summary>
        ///     Create a subnet from an IP Address and netmask
        /// </summary>
        /// <param name="address">the ip address</param>
        /// <param name="netmask">the net mask</param>
        /// <returns>The created subnet</returns>
        /// <exception cref="ArgumentNullException">ipAddress</exception>
        /// <exception cref="ArgumentNullException">netmask</exception>
        /// <exception cref="InvalidOperationException">the given IP Address is not IPv4</exception>
        /// <exception cref="InvalidOperationException">the given netmask is invalid</exception>
        public static Subnet FromNetMask(IPAddress address, IPAddress netmask)
        {
            if (address == null)
            {
                throw new ArgumentNullException(nameof(address));
            }

            if (netmask == null)
            {
                throw new ArgumentNullException(nameof(netmask));
            }

            if (!address.IsIPv4())
            {
                throw new ArgumentException($"{nameof(address)} must be IPv4", nameof(address));
            }

            if (!netmask.IsValidNetMask())
            {
                throw new ArgumentException($"{nameof(netmask)} must be IPv4", nameof(netmask));
            }

            try
            {
                return new Subnet(address, netmask.NetmaskToCidrRoutePrefix());
            }
            catch (Exception e)
            {
                throw new Exception("could not instantiate subnet", e);
            }
        }

        /// <summary>
        ///     Try to create a subnet from an IP Address and netmask
        /// </summary>
        /// <param name="address">the ip address</param>
        /// <param name="netmask">the net mask</param>
        /// <param name="subnet">the created subnet or <see langword="null" /> on failure</param>
        /// <returns><see langword="true" /> on success</returns>
        public static bool TryFromNetMask(IPAddress address, IPAddress netmask, out Subnet subnet)
        {
            try
            {
                subnet = FromNetMask(address, netmask);
                return true;
            }
            catch
            {
                subnet = null;
                return false;
            }
        }

        #endregion // end: FromNetMask

        /// <summary>
        ///     Construct the smallest possible subnet that would contain both IP addresses encoded as bytes typically the address
        ///     specified are the Network and Broadcast addresses (lower and higher bounds) but this is not necessary. Addresses
        ///     *MUST* be the same address family (either Internetwork or InternetworkV6)
        /// </summary>
        /// <param name="lowAddressBytes">the lower address <see cref="byte" /> array</param>
        /// <param name="highAddressBytes">the high address <see cref="byte" /> array</param>
        /// <returns>The created <see cref="Subnet"/></returns>
        public static Subnet FromBytes(byte[] lowAddressBytes, byte[] highAddressBytes)
        {
            if (lowAddressBytes == null)
            {
                throw new ArgumentNullException(nameof(lowAddressBytes));
            }

            if (highAddressBytes == null)
            {
                throw new ArgumentNullException(nameof(highAddressBytes));
            }

            IPAddress lowAddress;
            try
            {
                lowAddress = new IPAddress(lowAddressBytes);
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException(
                    $"could not convert {nameof(lowAddressBytes)} to an IPAddress",
                    nameof(lowAddressBytes),
                    e
                );
            }

            IPAddress highAddress;
            try
            {
                highAddress = new IPAddress(highAddressBytes);
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException(
                    $"could not convert {nameof(highAddressBytes)} to an IPAddress",
                    nameof(highAddressBytes),
                    e
                );
            }

            try
            {
                return new Subnet(lowAddress, highAddress);
            }
            catch (Exception e)
            {
                throw new Exception("could not instantiate subnet", e);
            }
        }

        /// <summary>
        ///     Construct the smallest possible subnet that would contain both IP addresses encoded as bytes typically the address
        ///     specified are the Network and Broadcast addresses (lower and higher bounds) but this is not necessary. Addresses
        ///     *MUST* be the same address family (either Internetwork or InternetworkV6)
        /// </summary>
        /// <param name="lowAddressBytes">the lower address <see cref="byte" /> array</param>
        /// <param name="highAddressBytes">the high address <see cref="byte" /> array</param>
        /// <param name="subnet">the created subnet or <see langword="null" /> on failure</param>
        /// <returns><see langword="true" /> on success</returns>
        public static bool TryFromBytes(byte[] lowAddressBytes, byte[] highAddressBytes, out Subnet subnet)
        {
            try
            {
                subnet = FromBytes(lowAddressBytes, highAddressBytes);
                return true;
            }
            catch
            {
                subnet = null;
                return false;
            }
        }

        #region Parse / TryParse

        #region subnet string

        /// <summary>
        ///     Unsafe parsing of a string into a subnet
        /// </summary>
        /// <param name="subnetString">the string to parse a subnet from</param>
        /// <returns>the parsed subnet</returns>
        /// <exception cref="ArgumentException">could not parse input</exception>
        public static Subnet Parse(string subnetString)
        {
            if (subnetString == null)
            {
                throw new ArgumentNullException(nameof(subnetString));
            }

            if (string.IsNullOrWhiteSpace(subnetString))
            {
                throw new ArgumentException("a non empty value is expected", nameof(subnetString));
            }

            var matches = RoughSubnetRegex.Matches(subnetString);

            if (matches.Count != 1)
            {
                throw new FormatException("unexpected format");
            }

            var match = matches[0];

            var addressString = match.Groups[1].Value;
            var routePrefixString = match.Groups[2].Value;

            if (!IPAddress.TryParse(addressString, out var address)) // attempt to parse IP address portion
            {
                throw new FormatException($"cannot parse ip address \"{addressString}\"");
            }

            int routingPrefix;
            if (!string.IsNullOrWhiteSpace(routePrefixString)) // attempt to parse routing prefix in there is a match
            {
                if (!int.TryParse(routePrefixString, out routingPrefix))
                {
                    throw new FormatException($"cannot parse routing prefix \"{routePrefixString}\"");
                }
            }
            else // no routing prefix match, assume it is a single address
            {
                routingPrefix = address.IsIPv4() ? IPAddressUtilities.IPv4BitCount : IPAddressUtilities.IPv6BitCount;
            }

            if (address.IsIPv4() && routingPrefix > IPAddressUtilities.IPv4BitCount)
            {
                throw new ArgumentException("routing prefix must be 32 or less for an IPv4 subnet", nameof(subnetString));
            }

            if (address.IsIPv6() && routingPrefix > IPAddressUtilities.IPv6BitCount)
            {
                throw new ArgumentException("routing prefix must be 128 or less for an IPv6 subnet", nameof(subnetString));
            }

            try
            {
                return new Subnet(address, routingPrefix);
            }
            catch (Exception e)
            {
                throw new Exception("could not instantiate subnet", e);
            }
        }

        /// <summary>
        ///     Attempt to parse a string into a subnet
        /// </summary>
        /// <param name="subnetString">the string to parse</param>
        /// <param name="subnet">the created subnet or <see langword="null" /> on failure</param>
        /// <returns><see langword="true" /> on success</returns>
        public static bool TryParse(string subnetString, out Subnet subnet)
        {
            try
            {
                subnet = Parse(subnetString);
                return true;
            }
            catch
            {
                subnet = null;
                return false;
            }
        }

        #endregion // end: subnet string

        #region address, routing prefix

        /// <summary>
        ///     Unsafe parsing of a string address and routing prefix into a subnet
        /// </summary>
        /// <param name="addressString">the address string</param>
        /// <param name="routingPrefix">the subnet routing prefix</param>
        /// <exception cref="ArgumentException">
        ///     <paramref name="addressString" /> has an invalid <see cref="AddressFamily" />
        /// </exception>
        /// <exception cref="ArgumentNullException"><paramref name="addressString" /> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="routingPrefix" /> is less than <c>0</c></exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     <paramref name="routingPrefix" /> is out of range of the provided
        ///     <see cref="AddressFamily" />
        /// </exception>
        /// <returns>The parsed <see cref="Subnet"/></returns>
        public static Subnet Parse(string addressString, int routingPrefix)
        {
            if (addressString == null)
            {
                throw new ArgumentNullException(nameof(addressString));
            }

            if (!IPAddress.TryParse(addressString, out var address))
            {
                throw new FormatException($"could not parse {nameof(addressString)} value \"{addressString}\"");
            }

            if (!IPAddressUtilities.ValidAddressFamilies.Contains(address.AddressFamily))
            {
                throw new ArgumentException(
                    $"must have an address family equal to {string.Join(", ", IPAddressUtilities.ValidAddressFamilies)}",
                    nameof(addressString)
                );
            }

            if (routingPrefix < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(routingPrefix));
            }

            if (address.IsIPv4() && routingPrefix > IPAddressUtilities.IPv4BitCount)
            {
                throw new ArgumentOutOfRangeException(nameof(routingPrefix));
            }

            if (address.IsIPv6() && routingPrefix > IPAddressUtilities.IPv6BitCount)
            {
                throw new ArgumentOutOfRangeException(nameof(routingPrefix));
            }

            try
            {
                return new Subnet(address, routingPrefix);
            }
            catch (Exception e)
            {
                throw new Exception("could not instantiate subnet", e);
            }
        }

        /// <summary>
        ///     Try to parse <paramref name="addressString" /> as an <see cref="IPAddress" /> for a Subnet with the given routing
        ///     prefix
        /// </summary>
        /// <param name="addressString">the address string</param>
        /// <param name="routingPrefix">the subnet routing prefix</param>
        /// <param name="subnet">the created subnet or <see langword="null" /> on failure</param>
        /// <returns><see langword="true" /> on success</returns>
        public static bool TryParse(string addressString, int routingPrefix, out Subnet subnet)
        {
            try
            {
                subnet = Parse(addressString, routingPrefix);
                return true;
            }
            catch
            {
                subnet = null;
                return false;
            }
        }

        #endregion // end: address, routing prefix

        #region string string

        /// <summary>
        ///     Unsafe parsing of two string as a new subnet
        /// </summary>
        /// <param name="lowAddressString">the low address string</param>
        /// <param name="highAddressString">the high address string</param>
        /// <returns>The parsed <see cref="Subnet"/></returns>
        public static Subnet Parse(string lowAddressString, string highAddressString)
        {
            if (lowAddressString == null)
            {
                throw new ArgumentNullException(nameof(lowAddressString));
            }

            if (highAddressString == null)
            {
                throw new ArgumentNullException(nameof(highAddressString));
            }

            if (!IPAddress.TryParse(lowAddressString, out var lowAddress))
            {
                throw new FormatException($"could not parse {nameof(lowAddressString)} value \"{lowAddressString}\"");
            }

            if (!IPAddress.TryParse(highAddressString, out var highAddress))
            {
                throw new FormatException($"could not parse {nameof(highAddressString)} value \"{highAddressString}\"");
            }

            if (lowAddress.AddressFamily != highAddress.AddressFamily)
            {
                throw new ArgumentException(
                    $"{nameof(lowAddressString)} and {nameof(highAddressString)} must have matching address families"
                );
            }

            if (!IPAddressUtilities.ValidAddressFamilies.Contains(lowAddress.AddressFamily))
            {
                throw new ArgumentException(
                    $"{nameof(lowAddressString)} must have an address family equal to {string.Join(", ", IPAddressUtilities.ValidAddressFamilies)}",
                    nameof(lowAddressString)
                );
            }

            if (!IPAddressUtilities.ValidAddressFamilies.Contains(highAddress.AddressFamily))
            {
                throw new ArgumentException(
                    $"{nameof(highAddressString)} must have an address family equal to {string.Join(", ", IPAddressUtilities.ValidAddressFamilies)}",
                    nameof(highAddressString)
                );
            }

            if (!highAddress.IsGreaterThanOrEqualTo(lowAddress))
            {
                throw new InvalidOperationException(
                    $"{nameof(highAddressString)} must be greater or equal to {nameof(lowAddressString)}"
                );
            }

            try
            {
                return new Subnet(lowAddress, highAddress);
            }
            catch (Exception e)
            {
                throw new Exception("could not instantiate subnet", e);
            }
        }

        /// <summary>
        ///     Try parse two strings as addresses
        /// </summary>
        /// <param name="lowAddressString">the low address string</param>
        /// <param name="highAddressString">the high address string</param>
        /// <param name="subnet">the created subnet or <see langword="null" /> on failure</param>
        /// <returns><see langword="true" /> on success</returns>
        public static bool TryParse(string lowAddressString, string highAddressString, out Subnet subnet)
        {
            try
            {
                subnet = Parse(lowAddressString, highAddressString);

                return true;
            }
            catch
            {
                subnet = null;
                return false;
            }
        }

        #endregion // end: string string

        #region From Partial

        /// <summary>
        ///     Try to convert a partial IPv4 address into a subnet based on found provided partial octets
        /// </summary>
        /// <param name="input">the partial IP address to parse</param>
        /// <param name="subnet">the subnet created</param>
        /// <returns>true on success</returns>
        public static bool TryIPv4FromPartial(string input, out Subnet subnet)
        {
            if (string.IsNullOrWhiteSpace(input) || !IPv4OctetPartialRegex.IsMatch(input))
            {
                subnet = null;
                return false;
            }

            input = input.TrimEnd('.');
            var octetCount = input.Count(c => c == '.') + 1;

            subnet = Parse(
                input + string.Concat(Enumerable.Repeat(".0", IPAddressUtilities.IPv4OctetCount - octetCount)),
                octetCount * 8
            );
            return true;
        }

        /// <summary>
        ///     Given a IPv6 cidr-like or IPv6 like string build a collection of all possible valid subnets that could be intended
        ///     by the input
        /// </summary>
        /// <param name="input">the partial ipv6 cidr or address</param>
        /// <param name="subnets">a collection of all possible matching subnets on success, or an empty collection on failure</param>
        /// <returns><see langword="true" /> on success</returns>
        [Obsolete(
            "the needs for this method are very specialized and may not be what the developer is expecting; this is likely to be replaced by a host of other more explicit and useful methods"
        )]
        public static bool TryIPv6FromPartial(string input, out IEnumerable<Subnet> subnets)
        {
            // try to discover possible garbage or incomplete input
            int hextetCount;
            if (
                string.IsNullOrWhiteSpace(input)
                || input.Equals(":", StringComparison.OrdinalIgnoreCase)
                || input.Contains(":::")
                || DoubleColonsAppearsMultipleTimes(input)
                || (hextetCount = input.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries).Length)
                    > IPAddressUtilities.IPv6HextetCount
                || (
                    hextetCount >= IPAddressUtilities.IPv6HextetCount && input.EndsWith(":", StringComparison.OrdinalIgnoreCase)
                )
            ) // too many hextets
            {
                subnets = Enumerable.Empty<Subnet>();
                return false;
            }

            // parseable as a well defined IPv6 subnet already
            if (
                input.Contains('/')
                && !input.EndsWith("/", StringComparison.Ordinal)
                && TryParse(input, out var subnet)
                && subnet.IsIPv6
            )
            {
                subnets = new[] { subnet };
                return true;
            }

            // TODO this could probably be done more cleanly, and provide a more expected result

            // a IPv6 cidr partial is provided
            if (
                (
                    IPAddress.TryParse(input, out var address) // treat as a complete address, may contain a '::' or not
                    || (input.Contains("::") && IPAddress.TryParse(input.TrimEnd(':'), out address))
                    || IPAddress.TryParse(input.TrimEnd(':') + "::", out address)
                ) && address.IsIPv6()
            ) // no collapse, but incomplete address
            {
                var trimmedPartial = input.TrimEnd(':'); // remove trailing ":" or "::" if exists

                // break up entry on hextets, an empty hextet implies a collapse
                var hextets = trimmedPartial
                    .Split(new[] { ':' }, StringSplitOptions.None) // DO NOT remove empty splits
                    .ToList();

                // should contain an empty, pump the first with appropriate values

                // get the index of the collapse
                var collapse = hextets
                    .Select((value, index) => new { value, index })
                    .FirstOrDefault(pair => string.IsNullOrWhiteSpace(pair.value));

                int collapseIndex;
                if (collapse == null && hextets.Count == 8) // no collapse - fully fledged address, all 8 hextets present
                {
                    subnets = new[] { Parse(input, IPAddressUtilities.IPv6BitCount) };
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

                hextetCount = hextets.Count;
                var permutationLimit = (IPAddressUtilities.IPv6HextetCount + 1) - hextetCount; // number of permutations of IP partial, every hextet available removes a permutation
                for (var permutation = 0; permutation <= permutationLimit; permutation++)
                {
                    var hextetsCopy = hextets.ToList();
                    hextetsCopy.RemoveAt(collapseIndex); // collapsed empty item
                    hextetsCopy.InsertRange(collapseIndex, Enumerable.Repeat("0", permutation)); // add zeros as appropriate

                    var addressString = string.Join(":", hextetsCopy); // re join string

                    if (
                        !IPAddress.TryParse(addressString + "::", out var subnetAddress)
                        && !IPAddress.TryParse(addressString, out subnetAddress)
                    )
                    {
                        subnets = Enumerable.Empty<Subnet>();
                        return false;
                    }

                    var routePrefix = ((permutation + hextetCount) - 1) * 16;
                    subnetsList.Add(new Subnet(subnetAddress, routePrefix));
                }

                subnets = subnetsList;
                return true;
            }

            // fail; could not parse anything useful from the input
            subnets = Enumerable.Empty<Subnet>();
            return false;

            // checks input for multiple occurrences of discrete "::" substrings
#if NET6_0_OR_GREATER
            static
#endif
            bool DoubleColonsAppearsMultipleTimes(string @in)
            {
                const string colons = "::";
                int firstIndex;

                return @in.Length >= 4
                    && (firstIndex = @in.IndexOf(colons, StringComparison.Ordinal)) != -1
                    && @in.Substring(firstIndex + 2, @in.Length - firstIndex - 2).IndexOf(colons, StringComparison.Ordinal)
                        != -1;
            }
        }

        #endregion // end: From Partial

        #endregion // end: Parse / TryParse

        #endregion // end: Static Factory Methods

        /// <inheritdoc />
        public bool Equals(Subnet other)
        {
            return !ReferenceEquals(null, other)
                && (
                    ReferenceEquals(this, other)
                    || (
                        Equals(this.NetworkPrefixAddress, other.NetworkPrefixAddress)
                        && this.RoutingPrefix == other.RoutingPrefix
                    )
                );
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return !ReferenceEquals(null, obj)
                && (ReferenceEquals(this, obj) || (obj.GetType() == GetType() && this.Equals((Subnet)obj)));
        }

        /// <inheritdoc />
        public override int GetHashCode() => HashCode.Combine(AddressFamily, Head, RoutingPrefix);

#if NETSTANDARD2_0
        #region From Interface ISerializable

        /// <inheritdoc />
        /// <exception cref="ArgumentNullException"><paramref name="info" /> is <see langword="null" /></exception>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            info.AddValue(nameof(this.BroadcastAddress), this.BroadcastAddress.GetAddressBytes());
            info.AddValue(nameof(this.RoutingPrefix), this.RoutingPrefix);
        }

        #endregion
#endif

        #region operators

        /// <summary>
        ///     Compares two <see cref="Subnet" /> objects for equality
        /// </summary>
        /// <param name="left">left hand operand</param>
        /// <param name="right">right hand operand</param>
        /// <returns><see langword="true" /> when both sides are equal</returns>
        public static bool operator ==(Subnet left, Subnet right)
        {
            return Equals(left, right);
        }

        /// <summary>
        ///     Compares two <see cref="Subnet" /> objects for non-equality
        /// </summary>
        /// <param name="left">left hand operand</param>
        /// <param name="right">right hand operand</param>
        /// <returns><see langword="true" /> when both sides are not equal</returns>
        public static bool operator !=(Subnet left, Subnet right)
        {
            return !Equals(left, right);
        }

        /// <summary>
        ///     Compares two <see cref="Subnet" /> objects for <paramref name="left" /> being less than
        ///     <paramref name="right" />
        /// </summary>
        /// <param name="left">left hand operand</param>
        /// <param name="right">right hand operand</param>
        /// <returns><see langword="true" /> when <paramref name="left" /> is less than <paramref name="right" /></returns>
        public static bool operator <(Subnet left, Subnet right)
        {
            return Comparer<Subnet>.Default.Compare(left, right) < 0;
        }

        /// <summary>
        ///     Compares two <see cref="Subnet" /> objects for <paramref name="left" /> being greater than
        ///     <paramref name="right" />
        /// </summary>
        /// <param name="left">left hand operand</param>
        /// <param name="right">right hand operand</param>
        /// <returns><see langword="true" /> when <paramref name="left" /> is greater than <paramref name="right" /></returns>
        public static bool operator >(Subnet left, Subnet right)
        {
            return Comparer<Subnet>.Default.Compare(left, right) > 0;
        }

        /// <summary>
        ///     Compares two <see cref="Subnet" /> objects for <paramref name="left" /> being less than or equal
        ///     <paramref name="right" />
        /// </summary>
        /// <param name="left">left hand operand</param>
        /// <param name="right">right hand operand</param>
        /// <returns>
        ///     <see langword="true" /> when <paramref name="left" /> is less than or equal to <paramref name="right" />
        /// </returns>
        public static bool operator <=(Subnet left, Subnet right)
        {
            return Comparer<Subnet>.Default.Compare(left, right) <= 0;
        }

        /// <summary>
        ///     Compares two <see cref="Subnet" /> objects for <paramref name="left" /> being greater than or equal to
        ///     <paramref name="right" />
        /// </summary>
        /// <param name="left">left hand operand</param>
        /// <param name="right">right hand operand</param>
        /// <returns>
        ///     <see langword="true" /> when <paramref name="left" /> is greater than or equal to <paramref name="right" />
        /// </returns>
        public static bool operator >=(Subnet left, Subnet right)
        {
            return Comparer<Subnet>.Default.Compare(left, right) >= 0;
        }

        #endregion end operators

        #region Static metods, may be appropriate for extracting

        private readonly struct AddressMaskAndPrefixTuple
        {
            public AddressMaskAndPrefixTuple(IPAddress head, IPAddress tail, IPAddress mask, int prefix)
            {
                this.Head = head ?? throw new ArgumentNullException(nameof(head));
                this.Tail = tail ?? throw new ArgumentNullException(nameof(tail));
                this.Mask = mask ?? throw new ArgumentNullException(nameof(mask));

                if (prefix < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(prefix));
                }

                this.Prefix = prefix;
            }

            /// <summary>
            ///     Gets head
            /// </summary>
            public IPAddress Head { get; }

            /// <summary>
            ///     Gets tail
            /// </summary>
            public IPAddress Tail { get; }

            /// <summary>
            ///     Gets mask
            /// </summary>
            public IPAddress Mask { get; }

            /// <summary>
            ///     Gets prefix
            /// </summary>
            public int Prefix { get; }
        }

        private static AddressMaskAndPrefixTuple NormalizeAndCreateNetMask(IPAddress head, IPAddress tail)
        {
            var headBytes = head.GetAddressBytes();
            var tailBytes = tail.GetAddressBytes();

            var routingPrefix = CalculateRoutingPrefix(headBytes, tailBytes);
            var result = NormalizeAndCreateNetMask(head, routingPrefix);
            return new AddressMaskAndPrefixTuple(result.Head, result.Tail, result.Mask, routingPrefix);

#if NET6_0_OR_GREATER
            static
#endif
            int CalculateRoutingPrefix(byte[] hb, byte[] tb)
            {
                var bitCount = hb.Length * 8; // 8 bits per byte

                // iterate in order to find the count of common bits starting at the 0th element of each byte array
                for (var i = 0; i < bitCount; i++)
                {
                    var byteIndex = i / 8;
                    var bitmask = (byte)(0x80 >> (i % 8));
                    if ((hb[byteIndex] & bitmask) != (tb[byteIndex] & bitmask)) // if bits aren't equal break
                    {
                        return i; // return index when matching stops
                    }
                }

                return bitCount; // all match, return length
            }
        }

        private readonly struct AddressAndMaskTuple
        {
            public AddressAndMaskTuple(IPAddress head, IPAddress tail, IPAddress mask)
            {
                this.Head = head ?? throw new ArgumentNullException(nameof(head));
                this.Tail = tail ?? throw new ArgumentNullException(nameof(tail));
                this.Mask = mask ?? throw new ArgumentNullException(nameof(mask));
            }

            /// <summary>
            ///     Gets head
            /// </summary>
            public IPAddress Head { get; }

            /// <summary>
            ///     Gets tail
            /// </summary>
            public IPAddress Tail { get; }

            /// <summary>
            ///     Gets mask
            /// </summary>
            public IPAddress Mask { get; }
        }

        private static AddressAndMaskTuple NormalizeAndCreateNetMask(IPAddress head, int routingPrefix)
        {
            var headBytes = head.GetAddressBytes();
            var addressByteLength = headBytes.Length;

            var maskBytes = Enumerable
                .Repeat((byte)0xFF, addressByteLength)
                .ToArray()
                .ShiftBitsLeft((addressByteLength * 8) - routingPrefix);

            var newHead = new IPAddress(ByteArrayUtils.BitwiseAndBigEndian(headBytes, maskBytes));
            var newTail = new IPAddress(ByteArrayUtils.BitwiseOrBigEndian(headBytes, ByteArrayUtils.BitwiseNot(maskBytes)));
            var netmask = new IPAddress(maskBytes);

            return new AddressAndMaskTuple(newHead, newTail, netmask);
        }

        #endregion // end: Static metods, may be appropriate for extracting

        #region Deconstructors

        /// <summary>
        ///     Deconstruct to Network Prefix Address, Broadcast Address, Netmask, and Routing Prefix
        /// </summary>
        /// <param name="networkPrefixAddress">the subnet <see cref="NetworkPrefixAddress" /></param>
        /// <param name="broadcastAddress">the subnet <see cref="BroadcastAddress" /></param>
        /// <param name="netmask">the subnet <see cref="Netmask" />, will be <see langword="null" /> for non IPv6 subnets</param>
        /// <param name="routingPrefix">the subnet <see cref="RoutingPrefix" /></param>
        public void Deconstruct(
            out IPAddress networkPrefixAddress,
            out IPAddress broadcastAddress,
            out IPAddress netmask,
            out int routingPrefix
        )
        {
            networkPrefixAddress = this.NetworkPrefixAddress;
            broadcastAddress = this.BroadcastAddress;
            netmask = this.Netmask;
            routingPrefix = this.RoutingPrefix;
        }

        /// <summary>
        ///     Deconstruct to Network Prefix Address and Routing Prefix
        /// </summary>
        /// <param name="networkPrefixAddress">the subnet <see cref="NetworkPrefixAddress" /></param>
        /// <param name="routingPrefix">the subnet <see cref="RoutingPrefix" /></param>
        public void Deconstruct(out IPAddress networkPrefixAddress, out int routingPrefix)
        {
            networkPrefixAddress = this.NetworkPrefixAddress;
            routingPrefix = this.RoutingPrefix;
        }

        #endregion // end: Deconstruct
    }
}
