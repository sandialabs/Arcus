using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Arcus.Math;
using JetBrains.Annotations;

namespace Arcus.Utilities
{
    /// <summary>
    ///     Static utility class containing miscellaneous operations for <see cref="Subnet" /> objects
    /// </summary>
    public static class SubnetUtilities
    {
        /// <summary>
        ///     Get The fewest consecutive subnets that would fill the range between the given addresses (inclusive)
        /// </summary>
        /// <param name="left">lowest order IP Address</param>
        /// <param name="right">highest order IP Address</param>
        /// <returns>an enumerable of Subnet</returns>
        /// <exception cref="ArgumentNullException"><paramref name="left" /> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="right" /> is <see langword="null" />.</exception>
        /// <exception cref="InvalidOperationException">Address families must match</exception>
        /// <exception cref="InvalidOperationException">Address families must be InterNetwork or InternetworkV6</exception>
        [NotNull]
        [ItemNotNull]
        public static IEnumerable<Subnet> FewestConsecutiveSubnetsFor([NotNull] IPAddress left,
                                                                      [NotNull] IPAddress right)
        {
            #region defense

            if (left == null)
            {
                throw new ArgumentNullException(nameof(left));
            }

            if (right == null)
            {
                throw new ArgumentNullException(nameof(right));
            }

            if (!IPAddressUtilities.ValidAddressFamilies.Contains(left.AddressFamily))
            {
                throw new ArgumentException($"{nameof(left)} must have an address family equal to {string.Join(", ", IPAddressUtilities.ValidAddressFamilies)}", nameof(left));
            }

            if (!IPAddressUtilities.ValidAddressFamilies.Contains(right.AddressFamily))
            {
                throw new ArgumentException($"{nameof(right)} must have an address family equal to {string.Join(", ", IPAddressUtilities.ValidAddressFamilies)}", nameof(right));
            }

            if (left.AddressFamily != right.AddressFamily)
            {
                throw new InvalidOperationException($"{nameof(left)} and {nameof(right)} must have matching address families");
            }

            #endregion // end: defense

            var minHead = IPAddressMath.Min(left, right);
            var maxTail = IPAddressMath.Max(left, right);

            return FilledSubnets(minHead, maxTail, new Subnet(minHead, maxTail));

            // recursive function call
            // Works by verifying that passed subnet isn't bounded by head, tail IP Addresses
            // if not breaks subnet in half and recursively tests, building in essence a binary tree of testable subnet paths
            IEnumerable<Subnet> FilledSubnets(IPAddress head,
                                              IPAddress tail,
                                              Subnet subnet)
            {
                var networkPrefixAddress = subnet.NetworkPrefixAddress;
                var broadcastAddress = subnet.BroadcastAddress;

                // the given subnet is the perfect size for the head/tail (not papa bear, not mama bear, but just right with baby bear)
                if (networkPrefixAddress.IsGreaterThanOrEqualTo(head)
                    && broadcastAddress.IsLessThanOrEqualTo(tail))
                {
                    return new[] {subnet};
                }

                // increasing the route prefix by 1 creates a subnet of half the initial size (due 2^(max-n) route prefix sizing)
                var nextSmallestRoutePrefix = subnet.RoutingPrefix + 1;

                // over-iterated route prefix, no valid subnet beyond this point; end search on this branch
                if ((subnet.IsIPv6 && nextSmallestRoutePrefix > IPAddressUtilities.IPv6BitCount)
                    || (subnet.IsIPv4 && nextSmallestRoutePrefix > IPAddressUtilities.IPv4BitCount))
                {
                    return Enumerable.Empty<Subnet>(); // no subnets to be found here, stop investigating branch of tree
                }

                // build head subnet
                var headSubnet = new Subnet(networkPrefixAddress, nextSmallestRoutePrefix);

                // use the next address after the end of the head subnet as the first address for the tail subnet
                if (!IPAddressMath.TryIncrement(headSubnet.BroadcastAddress, out var tailStartingAddress))
                {
                    throw new InvalidOperationException($"unable to increment {headSubnet.BroadcastAddress}");
                }

                // ReSharper disable once AssignNullToNotNullAttribute
                var tailSubnet = new Subnet(tailStartingAddress, nextSmallestRoutePrefix);

                // break into binary search tree, searching both head subnet and tail subnet for ownership of head and tail ip
                return FilledSubnets(head, tail, headSubnet)
                    .Concat(FilledSubnets(head, tail, tailSubnet));
            }
        }

        /// <summary>
        ///     Return the largest subnet (smallest route prefix value)
        ///     if more than one "largest" return is not predictable beyond that one will be returned
        ///     Consider usage of DefaultSubnetComparer
        /// </summary>
        /// <param name="subnets">the subnets to search</param>
        /// <returns>
        ///     The first largest subnet by routing prefix, or <see langword="null" /> if no <paramref name="subnets" /> to
        ///     choose from
        /// </returns>
        [CanBeNull]
        public static Subnet LargestSubnet([CanBeNull] IEnumerable<Subnet> subnets)
        {
            var enumerable = (subnets ?? Enumerable.Empty<Subnet>()).ToList();

            return !enumerable.Any()
                       ? null
                       : enumerable
                         .Where(s => s != null)
                         .Aggregate((s1,
                                     s2) => s1.RoutingPrefix < s2.RoutingPrefix
                                                ? s1
                                                : s2);
        }

        /// <summary>
        ///     Return the smallest subnet (largest route prefix value)
        ///     if more than one "smallest" return is not predictable beyond that one will be returned
        ///     Consider usage of DefaultSubnetComparer
        /// </summary>
        /// <param name="subnets">the list of subnets</param>
        /// <returns>The first smallest subnet by routing prefix, or null if no subnets to choose from</returns>
        [CanBeNull]
        public static Subnet SmallestSubnet([CanBeNull] IEnumerable<Subnet> subnets)
        {
            var enumerable = (subnets ?? Enumerable.Empty<Subnet>()).ToList();

            return !enumerable.Any()
                       ? null
                       : enumerable.Where(s => s != null)
                                   .Aggregate((s1,
                                               s2) => s1.RoutingPrefix > s2.RoutingPrefix
                                                          ? s1
                                                          : s2);
        }
    }
}
