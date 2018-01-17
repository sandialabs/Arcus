using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Arcus.Converters;
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
        /// <param name="alpha">lowest order IP Address</param>
        /// <param name="beta">highest order IP Address</param>
        /// <returns>an enumerable of Subnet</returns>
        /// <exception cref="ArgumentNullException"><paramref name="alpha" /> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="beta" /> is <see langword="null" />.</exception>
        /// <exception cref="InvalidOperationException">Address families must match</exception>
        /// <exception cref="InvalidOperationException">Address families must be InterNetwork or InternetworkV6</exception>
        public static IEnumerable<Subnet> FewestConsecutiveSubnetsFor([NotNull] IPAddress alpha,
                                                                      [NotNull] IPAddress beta)
        {
            if (alpha == null)
            {
                throw new ArgumentNullException(nameof(alpha));
            }

            if (beta == null)
            {
                throw new ArgumentNullException(nameof(beta));
            }

            if (beta.AddressFamily != alpha.AddressFamily)
            {
                throw new InvalidOperationException("Address families must match");
            }

            if (!beta.IsIPv4()
                && !beta.IsIPv6())
            {
                throw new InvalidOperationException("Address families must be InterNetwork or InternetworkV6");
            }

            var ipHead = IPAddressMath.Min(alpha, beta);
            var ipTail = IPAddressMath.Max(alpha, beta);

            return FilledSubnets(ipHead, ipTail, new Subnet(ipHead, ipTail));
        }

        /// <summary>
        ///     Function to be called recursively for FewestConsecutiveSubnetsFor
        ///     Works by verifying that passed subnet isn't bounded by head, tail
        ///     if not breaks subnet in half and recursively tests, building in essence a binary tree of testable subnet paths
        /// </summary>
        /// <param name="headIP">the head IP</param>
        /// <param name="tailIP">the tail IP</param>
        /// <param name="subnet">The subnet found on success</param>
        /// <returns>true on success</returns>
        private static IEnumerable<Subnet> FilledSubnets([NotNull] IPAddress headIP,
                                                         [NotNull] IPAddress tailIP,
                                                         [NotNull] Subnet subnet)
        {
            var networkPrefixAddress = subnet.NetworkPrefixAddress;
            var broadcastAddress = subnet.BroadcastAddress;

            // the given subnet is the perfect size for the head/tail (not papa bear, not mama bear, but just right with baby bear)
            if (networkPrefixAddress.ToUnsignedBigInteger() >= headIP.ToUnsignedBigInteger()
                && broadcastAddress.ToUnsignedBigInteger() <= tailIP.ToUnsignedBigInteger())
            {
                return new[] {subnet};
            }

            // increasing the route prefix by 1 creates a subnet of half the initial size (due 2^(max-n) route prefix sizing)
            var nextSmallestRoutePrefix = subnet.RoutingPrefix + 1;

            // over-iterated route prefix, no valid subnet beyond this point; end search on this branch
            if ((subnet.IsIPv6 && nextSmallestRoutePrefix > 128)
                || (subnet.IsIPv4 && nextSmallestRoutePrefix > 32))
            {
                return Enumerable.Empty<Subnet>(); // no subnets to be found here, stop investigating branch of tree
            }

            // build head subnet
            var headSubnet = new Subnet(networkPrefixAddress, nextSmallestRoutePrefix);

            // use the next address after the end of the head subnet as the first address for the tail subnet
            var tailStartingAddressBigInteger = headSubnet.BroadcastAddress.ToUnsignedBigInteger() + 1;
            IPAddress tailStartingAddress;
            if (!IPAddressUtilities.TryParse(tailStartingAddressBigInteger, headIP.AddressFamily, out tailStartingAddress))
            {
                throw new ArgumentException();
            }

            var tailSubnet = new Subnet(tailStartingAddress, nextSmallestRoutePrefix);

            // break into binary search tree, searching both head subnet and tail subnet for ownership of head and tail ip
            return FilledSubnets(headIP, tailIP, headSubnet)
                .Concat(FilledSubnets(headIP, tailIP, tailSubnet));
        }

        /// <summary>
        ///     Return the largest subnet (smallest route prefix value)
        ///     if more than one "largest" return is not predictable beyond that one will be returned
        ///     Consider usage of DefaultSubnetComparer
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="subnets">the subnets to search</param>
        /// <returns>
        ///     The first largest subnet by routing prefix, or <see langword="null" /> if no <paramref name="subnets" /> to
        ///     choose from
        /// </returns>
        [CanBeNull]
        public static Subnet LargestSubnet<T>([CanBeNull] IEnumerable<T> subnets) where T : Subnet
        {
            var enumerable = (subnets ?? Enumerable.Empty<T>()).ToList();

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
        /// <typeparam name="T"></typeparam>
        /// <param name="subnets">the list of subnets</param>
        /// <returns>The first smallest subnet by routing prefix, or null if no subnets to choose from</returns>
        [CanBeNull]
        public static Subnet SmallestSubnet<T>([CanBeNull] IEnumerable<T> subnets) where T : Subnet
        {
            var enumerable = (subnets ?? Enumerable.Empty<T>()).ToList();

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
