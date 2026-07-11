using System.Net;
using System.Text.RegularExpressions;
using Arcus.Converters;
using Arcus.Math;
using Arcus.Utilities;
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

namespace Arcus
{
    /// <content>
    ///     <see cref="Subnet"/> static factory methods
    /// </content>
    public partial class Subnet
    {
        private const string CouldNotInstantiateSubnetMessage = "could not instantiate subnet";

        /// <summary>
        ///     Regex pattern that matches valid partial IPv4 octet strings (1-4 dot-separated octets, each 0-255).
        ///     Applied with <see cref="RegexOptions.CultureInvariant"/>.
        /// </summary>
        public const string Ipv4OctetPartialPattern =
            @"^(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)(\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)){0,2}(\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)?)?$";

        /// <summary>
        ///     Regex pattern matching the rough shape of a subnet string: an address part followed by an optional slash-prefixed integer.
        ///     Applied with <see cref="RegexOptions.CultureInvariant"/> and <see cref="RegexOptions.IgnoreCase"/>.
        /// </summary>
        public const string RoughSubnetStringPattern = @"^([\da-fA-F:.]+)(?:/(-?[\d]+))?$";

#if !NET7_0_OR_GREATER
        private static readonly Regex IPv4OctetPartialRegex = new(
            Ipv4OctetPartialPattern,
            RegexOptions.Compiled | RegexOptions.CultureInvariant
        );
#else
        private static Regex IPv4OctetPartialRegex => GetIPv4OctetPartialRegex();

        [GeneratedRegex(Ipv4OctetPartialPattern, RegexOptions.CultureInvariant)]
        private static partial Regex GetIPv4OctetPartialRegex();
#endif

#if !NET7_0_OR_GREATER
        private static readonly Regex RoughSubnetRegex = new(
            RoughSubnetStringPattern,
            RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase
        );
#else
        private static Regex RoughSubnetRegex => GetRoughSubnetRegex();

        [GeneratedRegex(RoughSubnetStringPattern, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase)]
        private static partial Regex GetRoughSubnetRegex();
#endif

        #region static factory methods

        #region FromNetMask

        /// <summary>
        ///     Creates a subnet from an IP address and subnet mask.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         The netmask must be a valid IPv4 subnet mask (contiguous leading 1-bits) per
        ///         <see href="https://www.rfc-editor.org/rfc/rfc950#section-2">RFC 950 §2</see>.
        ///         IPv4 only; use <see cref="Subnet.Subnet(IPAddress, int, int)"/> for IPv6.
        ///     </para>
        /// </remarks>
        /// <param name="address">The IP address.</param>
        /// <param name="netmask">The subnet mask.</param>
        /// <param name="maxEnumerationExponent">The maximum enumeration exponent (0-128).</param>
        /// <returns>The created <see cref="Subnet"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="address"/> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="netmask"/> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException"><paramref name="address"/> is not IPv4.</exception>
        /// <exception cref="ArgumentException"><paramref name="netmask"/> is not a valid subnet mask.</exception>
        public static Subnet FromNetMask(
            IPAddress address,
            IPAddress netmask,
            int maxEnumerationExponent = DefaultMaxEnumerationExponent
        )
        {
            if (address is null)
            {
                throw new ArgumentNullException(nameof(address));
            }

            if (netmask is null)
            {
                throw new ArgumentNullException(nameof(netmask));
            }

            if (!address.IsIPv4())
            {
                throw new ArgumentException($"{nameof(address)} must be IPv4", nameof(address));
            }

            if (!netmask.IsValidNetMask())
            {
                throw new ArgumentException($"{nameof(netmask)} must be a valid netmask", nameof(netmask));
            }

            try
            {
                return new Subnet(address, netmask.NetmaskToCidrRoutePrefix(), maxEnumerationExponent);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException(CouldNotInstantiateSubnetMessage, e);
            }
        }

        /// <summary>
        ///     Attempts to create a subnet from an IP address and subnet mask.
        /// </summary>
        /// <param name="address">The IP address.</param>
        /// <param name="netmask">The subnet mask.</param>
        /// <param name="subnet">The created <see cref="Subnet" />, or <see langword="null" /> on failure.</param>
        /// <param name="maxEnumerationExponent">The maximum enumeration exponent (0-128).</param>
        /// <returns><see langword="true" /> if the subnet was created successfully.</returns>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
        public static bool TryFromNetMask(
            IPAddress address,
            IPAddress netmask,
            [NotNullWhen(true)] out Subnet subnet,
            int maxEnumerationExponent = DefaultMaxEnumerationExponent
        )
#else
        public static bool TryFromNetMask(
            IPAddress address,
            IPAddress netmask,
            out Subnet subnet,
            int maxEnumerationExponent = DefaultMaxEnumerationExponent
        )
#endif
        {
            try
            {
                subnet = FromNetMask(address, netmask, maxEnumerationExponent);
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
        ///     Creates the smallest subnet that contains both addresses encoded as byte arrays.
        /// </summary>
        /// <param name="lowAddressBytes">The lower address as a byte array.</param>
        /// <param name="highAddressBytes">The higher address as a byte array.</param>
        /// <param name="maxEnumerationExponent">The maximum enumeration exponent (0-128).</param>
        /// <returns>The created <see cref="Subnet"/>.</returns>
        public static Subnet FromBytes(
            byte[] lowAddressBytes,
            byte[] highAddressBytes,
            int maxEnumerationExponent = DefaultMaxEnumerationExponent
        )
        {
            if (lowAddressBytes is null)
            {
                throw new ArgumentNullException(nameof(lowAddressBytes));
            }

            if (highAddressBytes is null)
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
                return new Subnet(lowAddress, highAddress, maxEnumerationExponent);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException(CouldNotInstantiateSubnetMessage, e);
            }
        }

        /// <summary>
        ///     Attempts to create the smallest subnet containing both addresses encoded as byte arrays.
        /// </summary>
        /// <param name="lowAddressBytes">The lower address as a byte array.</param>
        /// <param name="highAddressBytes">The higher address as a byte array.</param>
        /// <param name="subnet">The created <see cref="Subnet" />, or <see langword="null" /> on failure.</param>
        /// <param name="maxEnumerationExponent">The maximum enumeration exponent (0-128).</param>
        /// <returns><see langword="true" /> if the subnet was created successfully.</returns>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
        public static bool TryFromBytes(
            byte[] lowAddressBytes,
            byte[] highAddressBytes,
            [NotNullWhen(true)] out Subnet subnet,
            int maxEnumerationExponent = DefaultMaxEnumerationExponent
        )
#else
        public static bool TryFromBytes(
            byte[] lowAddressBytes,
            byte[] highAddressBytes,
            out Subnet subnet,
            int maxEnumerationExponent = DefaultMaxEnumerationExponent
        )
#endif
        {
            try
            {
                subnet = FromBytes(lowAddressBytes, highAddressBytes, maxEnumerationExponent);
                return true;
            }
            catch
            {
                subnet = null;
                return false;
            }
        }

        /// <summary>
        ///     Creates a single-IP subnet from an <see cref="IPAddress"/> with a custom max enumeration exponent.
        /// </summary>
        /// <param name="address">the ip address</param>
        /// <param name="maxEnumerationExponent">the maximum enumeration exponent (0-128); enumeration yields at most 2<sup>maxEnumerationExponent</sup> addresses</param>
        /// <returns>A <see cref="Subnet"/> representing a single address.</returns>
        public static Subnet FromIPAddress(IPAddress address, int maxEnumerationExponent = DefaultMaxEnumerationExponent)
        {
            if (address is null)
            {
                throw new ArgumentNullException(nameof(address));
            }

            return new Subnet(address, address, maxEnumerationExponent);
        }

        #region Parse / TryParse

        /// <summary>
        ///     Parses a subnet from a CIDR-notation string.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         Accepts CIDR notation (<c>a.b.c.d/n</c> or <c>addr::x/n</c>) as defined in
        ///         <see href="https://www.rfc-editor.org/rfc/rfc4632#section-2">RFC 4632 §2</see>.
        ///         If no prefix is provided, a host route (/32 for IPv4, /128 for IPv6) is assumed.
        ///     </para>
        /// </remarks>
        /// <param name="subnetString">The string to parse.</param>
        /// <returns>The parsed <see cref="Subnet"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="subnetString"/> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException"><paramref name="subnetString"/> is empty or whitespace.</exception>
        /// <exception cref="FormatException">The string could not be parsed as a valid subnet.</exception>
        public static Subnet Parse(string subnetString)
        {
            subnetString = subnetString?.Trim();

            if (subnetString is null)
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

                if (routingPrefix < 0)
                {
                    throw new FormatException("Routing prefix must be non-negative.");
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
                throw new FormatException(CouldNotInstantiateSubnetMessage, e);
            }
        }

        /// <summary>
        ///     Parses a subnet from an address string and a CIDR prefix length.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         Parses a subnet from an address string and a separate CIDR prefix integer per
        ///         <see href="https://www.rfc-editor.org/rfc/rfc4632#section-2">RFC 4632 §2</see>.
        ///     </para>
        /// </remarks>
        /// <param name="addressString">The address string to parse.</param>
        /// <param name="routingPrefix">The CIDR routing prefix.</param>
        /// <param name="maxEnumerationExponent">The maximum enumeration exponent (0-128).</param>
        /// <returns>The parsed <see cref="Subnet"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="addressString" /> is <see langword="null" />.</exception>
        /// <exception cref="FormatException"><paramref name="addressString" /> could not be parsed as an <see cref="IPAddress"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="addressString" /> has an unsupported address family.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="routingPrefix" /> is out of range for the address family.</exception>
        public static Subnet Parse(
            string addressString,
            int routingPrefix,
            int maxEnumerationExponent = DefaultMaxEnumerationExponent
        )
        {
            if (addressString is null)
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
                return new Subnet(address, routingPrefix, maxEnumerationExponent);
            }
            catch (Exception e)
            {
                throw new FormatException(CouldNotInstantiateSubnetMessage, e);
            }
        }

        /// <summary>
        ///     Parses a subnet from two address strings (low and high bounds).
        /// </summary>
        /// <param name="lowAddressString">The low address string.</param>
        /// <param name="highAddressString">The high address string.</param>
        /// <param name="maxEnumerationExponent">The maximum enumeration exponent (0-128).</param>
        /// <returns>The parsed <see cref="Subnet"/>.</returns>
        public static Subnet Parse(
            string lowAddressString,
            string highAddressString,
            int maxEnumerationExponent = DefaultMaxEnumerationExponent
        )
        {
            if (lowAddressString is null)
            {
                throw new ArgumentNullException(nameof(lowAddressString));
            }

            if (highAddressString is null)
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
                return new Subnet(lowAddress, highAddress, maxEnumerationExponent);
            }
            catch (Exception e)
            {
                throw new FormatException(CouldNotInstantiateSubnetMessage, e);
            }
        }

        /// <summary>
        ///     Attempts to parse a subnet from a CIDR-notation string.
        /// </summary>
        /// <param name="subnetString">The string to parse.</param>
        /// <param name="subnet">The parsed <see cref="Subnet" />, or <see langword="null" /> on failure.</param>
        /// <returns><see langword="true" /> if parsing succeeded.</returns>
        public static bool TryParse(string subnetString,
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
            [NotNullWhen(true)]
#endif
            out Subnet subnet)
        {
            subnetString = subnetString?.Trim();

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

        /// <summary>
        ///     Attempts to parse a subnet from an address string and CIDR prefix length.
        /// </summary>
        /// <param name="addressString">The address string to parse.</param>
        /// <param name="routingPrefix">The CIDR routing prefix.</param>
        /// <param name="subnet">The parsed <see cref="Subnet" />, or <see langword="null" /> on failure.</param>
        /// <param name="maxEnumerationExponent">The maximum enumeration exponent (0-128).</param>
        /// <returns><see langword="true" /> if parsing succeeded.</returns>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
        public static bool TryParse(
            string addressString,
            int routingPrefix,
            [NotNullWhen(true)] out Subnet subnet,
            int maxEnumerationExponent = DefaultMaxEnumerationExponent
        )
#else
        public static bool TryParse(
            string addressString,
            int routingPrefix,
            out Subnet subnet,
            int maxEnumerationExponent = DefaultMaxEnumerationExponent
        )
#endif
        {
            try
            {
                subnet = Parse(addressString, routingPrefix, maxEnumerationExponent);
                return true;
            }
            catch
            {
                subnet = null;
                return false;
            }
        }

        /// <summary>
        ///     Attempts to parse a subnet from two address strings (low and high bounds).
        /// </summary>
        /// <param name="lowAddressString">The low address string.</param>
        /// <param name="highAddressString">The high address string.</param>
        /// <param name="subnet">The parsed <see cref="Subnet" />, or <see langword="null" /> on failure.</param>
        /// <param name="maxEnumerationExponent">The maximum enumeration exponent (0-128).</param>
        /// <returns><see langword="true" /> if parsing succeeded.</returns>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
        public static bool TryParse(
            string lowAddressString,
            string highAddressString,
            [NotNullWhen(true)] out Subnet subnet,
            int maxEnumerationExponent = DefaultMaxEnumerationExponent
        )
#else
        public static bool TryParse(
            string lowAddressString,
            string highAddressString,
            out Subnet subnet,
            int maxEnumerationExponent = DefaultMaxEnumerationExponent
        )
#endif
        {
            try
            {
                subnet = Parse(lowAddressString, highAddressString, maxEnumerationExponent);

                return true;
            }
            catch
            {
                subnet = null;
                return false;
            }
        }

        #region From Partial

        /// <summary>
        ///     Attempts to convert a partial IPv4 address into a subnet.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         Each supplied octet contributes 8 bits to the routing prefix per
        ///         <see href="https://www.rfc-editor.org/rfc/rfc791#section-2.3">RFC 791 §2.3</see>.
        ///         For example, <c>"10.0"</c> produces a /16 subnet, <c>"192.168.1"</c> produces a /24 subnet.
        ///     </para>
        /// </remarks>
        /// <param name="input">The partial IP address string (e.g., <c>"10.0"</c>, <c>"192.168"</c>).</param>
        /// <param name="subnet">The created <see cref="Subnet" />, or <see langword="null" /> on failure.</param>
        /// <param name="maxEnumerationExponent">The maximum enumeration exponent (0-128).</param>
        /// <returns><see langword="true" /> if the partial address was parsed successfully.</returns>
        public static bool TryIPv4FromPartial(
            string input,
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
            [NotNullWhen(true)]
#endif
            out Subnet subnet,
            int maxEnumerationExponent = DefaultMaxEnumerationExponent
        )
        {
            if (string.IsNullOrWhiteSpace(input) || !IPv4OctetPartialRegex.IsMatch(input))
            {
                subnet = null;
                return false;
            }

            input = input.TrimEnd('.');
            var octetCount = input.Count(ch => ch == '.') + 1;
            var addressString = input + string.Concat(Enumerable.Repeat(".0", IPAddressUtilities.IPv4OctetCount - octetCount));

            try
            {
                subnet = Parse(addressString, octetCount * 8, maxEnumerationExponent);
                return true;
            }
            catch
            {
                subnet = null;
                return false;
            }
        }

        /// <summary>
        ///     Given a partial IPv6 address (with or without CIDR notation), enumerate all possible
        ///     valid subnets the input could represent, ordered from most-specific to broadest
        ///     (descending prefix length).
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         This method handles several distinct input patterns, each delegated to a dedicated
        ///         handler. See the individual handler documentation for details.
        ///     </para>
        ///     <list type="table">
        ///         <listheader><term>Input pattern</term><description>Example</description></listheader>
        ///         <item><term>Exact CIDR</term><description><c>"2001:db8::/32"</c> - returned as-is.</description></item>
        ///         <item><term>Bare hex word</term><description><c>"2001"</c> or <c>"abba"</c> - treated as a single hextet with implicit <c>::</c>.</description></item>
        ///         <item><term>Colon-separated partial</term><description><c>"2001:db8::"</c>, <c>"2001:db8:"</c>, <c>"2001:db8:0:0:0:0:"</c> - all possible prefix-length expansions.</description></item>
        ///         <item><term>Bracketed</term><description><c>"[2001:db8::1]"</c> - brackets are stripped before processing.</description></item>
        ///         <item><term>Whitespace-wrapped</term><description><c>" 2001:db8:: "</c> - leading/trailing whitespace is trimmed.</description></item>
        ///     </list>
        ///     <para>
        ///         The permutation-based disambiguation treats each specified hextet as part of the
        ///         routing prefix. For each possible count of zero hextets the <c>::</c> collapse
        ///         could expand to (0 through the remaining hextet slots), a subnet is generated.
        ///         Results are returned in descending prefix length order so the most specific
        ///         (tightest) match appears first.
        ///     </para>
        ///     <para>
        ///         For example, <c>"2001:db8::"</c> has 2 specified hextets and one <c>::</c> collapse.
        ///         The collapse can expand to consume 0 through 5 hextet positions, producing
        ///         subnets at prefix lengths /128, /112, /96, /80, /64, /48, /32.
        ///     </para>
        ///     <para>
        ///         IPv6 colon-hex notation with <c>::</c> zero-group collapse per
        ///         <see href="https://www.rfc-editor.org/rfc/rfc4291#section-2.2">RFC 4291 §2.2</see>.
        ///     </para>
        ///     <para>
        ///         This method is <see cref="ObsoleteAttribute"/> because the permutation-based
        ///         disambiguation is a very specific behaviour that may not match the caller's
        ///         intent. It is likely to be replaced by more explicit factory methods.
        ///     </para>
        /// </remarks>
        /// <param name="input">
        ///     A partial IPv6 address string. The following forms are accepted:
        ///     <list type="bullet">
        ///         <item>
        ///             <description>Exact CIDR notation - <c>"2001:db8::/32"</c>, <c>"::/0"</c>.</description>
        ///         </item>
        ///         <item>
        ///             <description>Fully-qualified address - <c>"2001:db8::1"</c>, <c>"2001:db8:0:0:0:0:0:1"</c>.</description>
        ///         </item>
        ///         <item>
        ///             <description>Partial hextets with trailing <c>::</c> - <c>"2001:db8::"</c>, <c>"abba::"</c>.</description>
        ///         </item>
        ///         <item>
        ///             <description>Partial hextets with trailing <c>:</c> - <c>"2001:db8:"</c>, <c>"2001:db8:0:0:0:0:"</c>.</description>
        ///         </item>
        ///         <item>
        ///             <description>Bare hex word (no colons) - <c>"2001"</c>, <c>"abba"</c>.</description>
        ///         </item>
        ///         <item>
        ///             <description>URL-bracketed - <c>"[2001:db8::1]"</c>.</description>
        ///         </item>
        ///     </list>
        ///     Leading/trailing whitespace is automatically trimmed.
        /// </param>
        /// <param name="subnets">
        ///     When this method returns <see langword="true" />, a collection of all possible
        ///     matching subnets ordered from most-specific (largest prefix) to broadest (smallest
        ///     prefix); when <see langword="false" />, an empty collection.
        /// </param>
        /// <param name="maxEnumerationExponent">the maximum enumeration exponent</param>
        /// <returns><see langword="true" /> when at least one valid subnet interpretation was found.</returns>
#pragma warning disable S1133 // Do not forget to remove this deprecated code someday
        [Obsolete(
            "the needs for this method are very specialized and may not be what the developer is expecting; this is likely to be replaced by a host of other more explicit and useful methods"
        )]
#pragma warning restore S1133
        public static bool TryIPv6FromPartial(
            string input,
            out IEnumerable<Subnet> subnets,
            int maxEnumerationExponent = DefaultMaxEnumerationExponent
        )
        {
            subnets = [];

            if (string.IsNullOrWhiteSpace(input))
            {
                return false;
            }

            input = NormalizeIPv6PartialInput(input);

            if (!IsPotentiallyValidIPv6Partial(input))
            {
                return false;
            }

            if (TryParseExactCidr(input, out subnets))
            {
                return true;
            }

            if (ContainsOnlyHexChars(input))
            {
                return TryParseBareHextet(input, out subnets, maxEnumerationExponent);
            }

            return TryParseAmbiguousHextets(input, out subnets, maxEnumerationExponent);
        }

        /// <summary>
        ///     Normalizes a raw user input string for partial IPv6 parsing.
        ///     Trims leading/trailing whitespace and removes URL-style square brackets.
        /// </summary>
        /// <param name="input">The raw input string (must not be null).</param>
        /// <returns>The normalized input string.</returns>
        /// <remarks>
        ///     <para>
        ///         Non-developer users may inadvertently include whitespace or wrap the
        ///         address in brackets (e.g. from copying a URL). Normalizing upfront
        ///         prevents these from causing spurious parse failures.
        ///     </para>
        /// </remarks>
        private static string NormalizeIPv6PartialInput(string input)
        {
            input = input.Trim();

            if (input.Length >= 2 && input[0] == '[' && input[input.Length - 1] == ']')
            {
                input = input.Substring(1, input.Length - 2).Trim();
            }

            return input;
        }

        /// <summary>
        ///     Fast validation to reject inputs that cannot possibly be a valid IPv6 partial.
        /// </summary>
        /// <param name="input">The normalized input string.</param>
        /// <returns><see langword="true" /> if the input <em>could</em> be a valid IPv6 partial.</returns>
        /// <remarks>
        ///     Rejects:
        ///     <list type="bullet">
        ///         <item>Lone <c>":"</c> - meaningless.</item>
        ///         <item>Triple-colon <c>":::"</c> - invalid syntax.</item>
        ///         <item>Multiple discrete <c>"::"</c> occurrences - RFC 4291 §2.2 allows at most one zero-collapse.</item>
        ///         <item>More than 8 non-empty hextets - exceeds IPv6 address width.</item>
        ///         <item>8+ hextets with trailing colon - over-full; no room for a collapse.</item>
        ///     </list>
        ///     Does NOT validate individual hextet values (hex format is checked by downstream parsers).
        /// </remarks>
        private static bool IsPotentiallyValidIPv6Partial(string input)
        {
            if (input.Equals(":", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (input.Contains(":::"))
            {
                return false;
            }

            if (DoubleColonsAppearsMultipleTimes(input))
            {
                return false;
            }

            var nonEmptyHextetCount = input.Split([':'], StringSplitOptions.RemoveEmptyEntries).Length;

            if (nonEmptyHextetCount > IPAddressUtilities.IPv6HextetCount)
            {
                return false;
            }

            return nonEmptyHextetCount < IPAddressUtilities.IPv6HextetCount
                || !input.EndsWith(":", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        ///     Checks if a string contains only hexadecimal characters (0-9, a-f, A-F).
        /// </summary>
        /// <param name="input">The string to check.</param>
        /// <returns><see langword="true" /> if the string is non-empty and contains only hex chars.</returns>
        /// <remarks>
        ///     Used to detect bare hextet inputs like <c>"2001"</c> or <c>"abba"</c>
        ///     that lack colons entirely. Without this check, such inputs would be
        ///     misinterpreted by <c>IPAddress.TryParse</c> as IPv4 addresses
        ///     (e.g. <c>"2001"</c> is parsed as decimal 2001 = <c>0.0.7.209</c>).
        ///     <para>
        ///         This helper is intentionally permissive (any length of hex), allowing
        ///         future input patterns to flow through to the permutation engine.
        ///     </para>
        /// </remarks>
        private static bool ContainsOnlyHexChars(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return false;
            }

            return input.All(ch => ch is >= '0' and <= '9' or >= 'a' and <= 'f' or >= 'A' and <= 'F');
        }

        /// <summary>
        ///     Attempts to parse the input as an exact IPv6 CIDR subnet.
        /// </summary>
        /// <param name="input">The normalized input string.</param>
        /// <param name="subnets">
        ///     When successful, a single-element collection with the parsed subnet;
        ///     when unsuccessful, an empty collection.
        /// </param>
        /// <returns><see langword="true" /> if the input is a valid IPv6 CIDR subnet.</returns>
        /// <remarks>
        ///     Handles inputs containing a <c>/</c> character, e.g. <c>"2001:db8::/32"</c>
        ///     or <c>"::/0"</c>.  The input must not end with <c>/</c> (missing prefix)
        ///     and must not be an IPv4 CIDR (filtered via <c>Subnet.IsIPv6</c>).
        ///     <para>
        ///         This is the least-ambiguous input pattern - the user explicitly states
        ///         the route prefix length. A single result is returned.
        ///     </para>
        /// </remarks>
        private static bool TryParseExactCidr(string input, out IEnumerable<Subnet> subnets)
        {
            subnets = [];

            if (!input.Contains('/'))
            {
                return false;
            }

#if NETSTANDARD2_0
            if (input.EndsWith("/"))
#else
            if (input.EndsWith('/'))
#endif
            {
                return false;
            }

            if (TryParse(input, out var subnet) && subnet.IsIPv6)
            {
                subnets = [subnet];
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Attempts to parse a bare hex word (no colons) as a single-hextet IPv6 partial.
        /// </summary>
        /// <param name="input">A string containing only hexadecimal characters.</param>
        /// <param name="subnets">
        ///     When successful, all possible subnet interpretations for the hextet,
        ///     ordered from most-specific to broadest; when unsuccessful, an empty collection.
        /// </param>
        /// <param name="exponent">the maximum enumeration exponent</param>
        /// <returns><see langword="true" /> when the bare hextet produced at least one valid subnet.</returns>
        /// <remarks>
        ///     <para>
        ///         A bare hex word like <c>"2001"</c> is treated as a single IPv6 hextet
        ///         with an implicit <c>::</c> collapse appended. The method delegates to
        ///         <see cref="ExpandHextetPermutations"/> which generates all valid prefix-length
        ///         expansions.
        ///     </para>
        ///     <para>
        ///         This handler exists because <c>IPAddress.TryParse</c> would
        ///         misinterpret bare hex strings as IPv4 addresses (e.g. <c>"2001"</c> as
        ///         decimal 2001 = <c>0.0.7.209</c>).
        ///     </para>
        ///     <para>
        ///         <strong>Future:</strong> This method is a strong candidate for public exposure
        ///         (e.g. <c>TryParseBareHextet</c>) for users who explicitly want to resolve
        ///         a single hex value to all covering subnets.
        ///     </para>
        /// </remarks>
        private static bool TryParseBareHextet(
            string input,
            out IEnumerable<Subnet> subnets,
            int exponent = DefaultMaxEnumerationExponent
        )
        {
            return ExpandHextetPermutations(input, out subnets, exponent);
        }

        /// <summary>
        ///     Attempts to parse a colon-separated partial IPv6 string and enumerate all
        ///     possible subnet interpretations.
        /// </summary>
        /// <param name="input">The normalized input string (must contain colons).</param>
        /// <param name="subnets">
        ///     When successful, all possible subnet interpretations in descending prefix
        ///     length order; when unsuccessful, an empty collection.
        /// </param>
        /// <param name="exponent">the maximum enumeration exponent</param>
        /// <returns><see langword="true" /> when at least one valid subnet was found.</returns>
        /// <remarks>
        ///     <para>
        ///         Uses a three-stage fallback to coerce the input into a form that
        ///         <c>IPAddress.TryParse</c> can accept:
        ///     </para>
        ///     <list type="number">
        ///         <item>Parse the input as-is (succeeds for valid addresses like <c>"2001:db8::1"</c>).</item>
        ///         <item>If input contains <c>"::"</c>, trim trailing colons and retry
        ///             (handles edge cases where <c>::</c> is followed by extra colons).</item>
        ///         <item>Append <c>"::"</c> to the colon-trimmed input
        ///             (handles incomplete forms like <c>"2001:db8:"</c> → <c>"2001:db8::"</c>).</item>
        ///     </list>
        ///     <para>
        ///         On success, delegates to <see cref="ExpandHextetPermutations"/>.
        ///     </para>
        ///     <para>
        ///         <strong>Future:</strong> This method is a strong candidate for public exposure
        ///         (e.g. <c>TryParseAmbiguousHextets</c>) for users who want to resolve
        ///         colon-separated partials without the CIDR or bare-hexet paths.
        ///     </para>
        /// </remarks>
        private static bool TryParseAmbiguousHextets(
            string input,
            out IEnumerable<Subnet> subnets,
            int exponent = DefaultMaxEnumerationExponent
        )
        {
            subnets = [];

            if (
                !(
                    IPAddress.TryParse(input, out var address)
                    || (input.Contains("::") && IPAddress.TryParse(input.TrimEnd(':'), out address))
                    || IPAddress.TryParse(input.TrimEnd(':') + "::", out address)
                ) || !address.IsIPv6()
            )
            {
                return false;
            }

            return ExpandHextetPermutations(input, out subnets, exponent);
        }

        /// <summary>
        ///     Core permutation engine. Given a partial IPv6 address string, finds or synthesises
        ///     the <c>::</c> collapse point and generates all valid prefix-length interpretations.
        /// </summary>
        /// <param name="input">A colon-separated partial IPv6 address.</param>
        /// <param name="subnets">
        ///     All possible subnet interpretations in descending prefix length order
        ///     (most-specific first), or an empty collection on failure.
        /// </param>
        /// <param name="exponent">the maximum enumeration exponent</param>
        /// <returns><see langword="true" /> when at least one valid subnet was generated.</returns>
        /// <remarks>
        ///     <para>
        ///         The algorithm:
        ///     </para>
        ///     <list type="number">
        ///         <item>Split the colon-trimmed input on <c>':'</c> to obtain individual hextets.
        ///             An empty-string entry marks the <c>::</c> collapse position.</item>
        ///         <item>If no collapse is found and fewer than 8 hextets are present, an implicit
        ///             collapse is added at the end (the partial is assumed to omit trailing zeros).</item>
        ///         <item>For each possible expansion of the collapse (replacing it with 0 through the
        ///             remaining slots), construct a full address and compute the route prefix as
        ///             <c>(expandedHextets) * 16</c>.</item>
        ///     </list>
        ///     <para>
        ///         Results are emitted in <strong>descending</strong> prefix length order so the
        ///         most specific (largest prefix) match appears first in the output collection.
        ///     </para>
        ///     <para>
        ///         Example: input <c>"2001:db8::"</c>.
        ///         Hextets after trim: <c>["2001", "db8"]</c>, no collapse → add implicit end collapse:
        ///         <c>["2001", "db8", ""]</c>.  Three hextets → 7 permutations:
        ///     </para>
        ///     <list type="table">
        ///         <listheader><term>Permutation</term><description>Address</description><term>Prefix</term></listheader>
        ///         <item><term>6</term><description><c>2001:db8:0:0:0:0:0:0</c></description><term>/128</term></item>
        ///         <item><term>5</term><description><c>2001:db8:0:0:0:0:0::</c></description><term>/112</term></item>
        ///         <item><term>...</term><description>...</description><term>...</term></item>
        ///         <item><term>0</term><description><c>2001:db8::</c></description><term>/32</term></item>
        ///     </list>
        /// </remarks>
        private static bool ExpandHextetPermutations(
            string input,
            out IEnumerable<Subnet> subnets,
            int exponent = DefaultMaxEnumerationExponent
        )
        {
            subnets = [];

            var trimmedPartial = input.TrimEnd(':');

            var hextets = trimmedPartial.Split([':'], StringSplitOptions.None).ToList();

            var collapseIndex = -1;
            for (var i = 0; i < hextets.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(hextets[i]))
                {
                    collapseIndex = i;
                    break;
                }
            }

            if (collapseIndex == -1 && hextets.Count == 8)
            {
                subnets = [Parse(input, IPAddressUtilities.IPv6BitCount, exponent)];
                return true;
            }

            if (collapseIndex == -1)
            {
                hextets.Add(string.Empty);
                collapseIndex = hextets.Count - 1;
            }

            var hextetCount = hextets.Count;
            var permutationLimit = (IPAddressUtilities.IPv6HextetCount + 1) - hextetCount;
            var subnetsList = new List<Subnet>(permutationLimit + 1);
            var hextetsCopy = new List<string>(hextetCount + permutationLimit);

            // Iterate in DESCENDING order so the most-specific (largest prefix) result
            // appears first - this matches user expectations when scanning the output list.
            for (var permutation = permutationLimit; permutation >= 0; permutation--)
            {
                hextetsCopy.Clear();
                hextetsCopy.AddRange(hextets);
                hextetsCopy.RemoveAt(collapseIndex);
                for (var j = 0; j < permutation; j++)
                {
                    hextetsCopy.Insert(collapseIndex, "0");
                }

                var addressString = string.Join(":", hextetsCopy);

                if (
                    !IPAddress.TryParse(addressString + "::", out var subnetAddress)
                    && !IPAddress.TryParse(addressString, out subnetAddress)
                )
                {
                    return false;
                }

                var routePrefix = ((permutation + hextetCount) - 1) * 16;
                subnetsList.Add(new Subnet(subnetAddress, routePrefix, exponent));
            }

            subnets = subnetsList;
            return true;
        }

        /// <summary>
        ///     Checks if a string contains multiple discrete <c>"::"</c> substrings,
        ///     which is syntactically invalid in IPv6 (RFC 4291 §2.2 allows at most one
        ///     zero-group collapse).
        /// </summary>
        /// <param name="input">The string to check.</param>
        /// <returns>
        ///     <see langword="true" /> if two or more independent <c>"::"</c> occurrences
        ///     are found.
        /// </returns>
        /// <remarks>
        ///     <para>
        ///         Note that <c>":::"</c> is caught earlier by <see cref="IsPotentiallyValidIPv6Partial"/>
        ///         via <see cref="string.Contains(string)"/>. However, inputs like <c>"::1::2"</c>
        ///         pass the triple-colon check while having two discrete <c>"::"</c> occurrences
        ///         that must be rejected.
        ///     </para>
        /// </remarks>
        private static bool DoubleColonsAppearsMultipleTimes(string input)
        {
            const string colons = "::";
            var firstIndex = input.IndexOf(colons, StringComparison.Ordinal);
            return firstIndex >= 0 && input.IndexOf(colons, firstIndex + colons.Length, StringComparison.Ordinal) >= 0;
        }

        #endregion // end: From Partial

        #endregion // end: Parse / TryParse

        #endregion // end: Static Factory Methods
    }
}
