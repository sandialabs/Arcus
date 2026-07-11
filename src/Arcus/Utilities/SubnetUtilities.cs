using System.Net;
using Arcus.Math;

namespace Arcus.Utilities
{
    /// <summary>
    ///     Static utility class containing miscellaneous operations for <see cref="Subnet" /> objects
    /// </summary>
    public static class SubnetUtilities
    {
        private static readonly Lazy<IReadOnlyList<Subnet>> PrivateRanges = new(() => new[]
            {
                // IPv4 RFC 1918
#pragma warning disable S1313 // IP addresses should not be hardcoded
                Subnet.Parse("10.0.0.0", 8),
                Subnet.Parse("172.16.0.0", 12),
                Subnet.Parse("192.168.0.0", 16),
                // IPv6 RFC 4193
                Subnet.Parse("fd00::", 8),
#pragma warning restore S1313 // IP addresses should not be hardcoded
            }.ToList().AsReadOnly());

        private static readonly Lazy<IReadOnlyList<Subnet>> LinkLocalRanges = new(() => new[]
            {
                // RFC 3927
#pragma warning disable S1313 // IP addresses should not be hardcoded
                Subnet.Parse("169.254.0.0", 16),
                // RFC 4291
                Subnet.Parse("fe80::", 10),
#pragma warning restore S1313 // IP addresses should not be hardcoded
            }.ToList().AsReadOnly());

        /// <summary>
        ///     Gets a collection of all known private IP Address ranges.
        /// </summary>
        /// <value>A <see cref="IReadOnlyList{Subnet}" /> of the RFC 1918 / RFC 4193 private ranges.</value>
        /// <remarks>
        ///     <para>
        ///         Contains the four RFC-defined private/ULA address blocks:
        ///         <c>10.0.0.0/8</c>, <c>172.16.0.0/12</c>, and <c>192.168.0.0/16</c> per
        ///         <see href="https://www.rfc-editor.org/rfc/rfc1918#section-3">RFC 1918 §3</see>;
        ///         and <c>fd00::/8</c> (IPv6 Unique Local Addresses) per
        ///         <see href="https://www.rfc-editor.org/rfc/rfc4193#section-8">RFC 4193 §8</see>.
        ///     </para>
        ///     <para>
        ///         <b>Breaking change (lazy):</b> This member is now a lazily-initialized static
        ///         property backed by <see cref="Lazy{T}"/>. Prior to this version it was a
        ///         <see langword="static readonly"/> field whose reference could be reassigned
        ///         by external code. Code that assigned to
        ///         <c>SubnetUtilities.PrivateIPAddressRangesList = ...</c> will no longer compile.
        ///         The <see cref="IReadOnlyList{Subnet}" /> contract already prevented mutation of
        ///         the list contents; this change extends that guarantee to the backing reference
        ///         and defers construction until first access.
        ///     </para>
        /// </remarks>
        public static IReadOnlyList<Subnet> PrivateIPAddressRangesList => PrivateRanges.Value;

        /// <summary>
        ///     Gets a collection of all known Link Local IP Address ranges.
        /// </summary>
        /// <value>A <see cref="IReadOnlyList{Subnet}" /> of the RFC 3927 / RFC 4291 link-local ranges.</value>
        /// <remarks>
        ///     <para>
        ///         Contains <c>169.254.0.0/16</c> (IPv4 link-local) per
        ///         <see href="https://www.rfc-editor.org/rfc/rfc3927#section-2.1">RFC 3927 §2.1</see>
        ///         and <c>fe80::/10</c> (IPv6 link-local) per
        ///         <see href="https://www.rfc-editor.org/rfc/rfc4291#section-2.4">RFC 4291 §2.4</see>.
        ///     </para>
        ///     <para>
        ///         <b>Breaking change (lazy):</b> This member is now a lazily-initialized static
        ///         property backed by <see cref="Lazy{T}"/>. Prior to this version it was a
        ///         <see langword="static readonly"/> field whose reference could be reassigned
        ///         by external code. Code that assigned to
        ///         <c>SubnetUtilities.LinkLocalIPAddressRangesList = ...</c> will no longer compile.
        ///     </para>
        /// </remarks>
        public static IReadOnlyList<Subnet> LinkLocalIPAddressRangesList => LinkLocalRanges.Value;

        /// <summary>
        ///     Computes the fewest consecutive CIDR subnets that exactly cover the range from <paramref name="left"/> to <paramref name="right"/> (inclusive).
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         Uses a binary-tree decomposition algorithm: the range is progressively divided
        ///         into the largest possible CIDR blocks. Each increment of the prefix length halves
        ///         the block size (2<sup>max−n</sup> addresses) per
        ///         <see href="https://www.rfc-editor.org/rfc/rfc4632#section-2">RFC 4632 §2</see>.
        ///     </para>
        /// </remarks>
        /// <param name="left">The lower bound of the range.</param>
        /// <param name="right">The upper bound of the range.</param>
        /// <param name="maxEnumerationExponent">The maximum enumeration exponent (0-128).</param>
        /// <returns>An enumerable of <see cref="Subnet"/> values covering the range.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException">Address families must match.</exception>
        public static IEnumerable<Subnet> FewestConsecutiveSubnetsFor(
            IPAddress left,
            IPAddress right,
            int maxEnumerationExponent = AbstractIPAddressRange.DefaultMaxEnumerationExponent
        )
        {
            #region defense

            if (left is null)
            {
                throw new ArgumentNullException(nameof(left));
            }

            if (right is null)
            {
                throw new ArgumentNullException(nameof(right));
            }

            if (!IPAddressUtilities.ValidAddressFamilies.Contains(left.AddressFamily))
            {
                throw new ArgumentException(
                    $"{nameof(left)} must have an address family equal to {string.Join(", ", IPAddressUtilities.ValidAddressFamilies)}",
                    nameof(left)
                );
            }

            if (!IPAddressUtilities.ValidAddressFamilies.Contains(right.AddressFamily))
            {
                throw new ArgumentException(
                    $"{nameof(right)} must have an address family equal to {string.Join(", ", IPAddressUtilities.ValidAddressFamilies)}",
                    nameof(right)
                );
            }

            if (left.AddressFamily != right.AddressFamily)
            {
                throw new InvalidOperationException($"{nameof(left)} and {nameof(right)} must have matching address families");
            }

            #endregion // end: defense

            var minHead = IPAddressMath.Min(left, right);
            var maxTail = IPAddressMath.Max(left, right);

            return FilledSubnets(
                minHead,
                maxTail,
                new Subnet(minHead, maxTail, maxEnumerationExponent),
                maxEnumerationExponent
            );

            // recursive function call
            // Works by verifying that passed subnet isn't bounded by head, tail IP Addresses
            // if not breaks subnet in half and recursively tests, building in essence a binary tree of testable subnet paths
            //
            // Implemented as a yield-return iterator rather than a recursive Concat chain. The previous
            // `return a.Concat(b)` form produced an O(N) iterator-wrapper depth — each level wrapped its
            // predecessor — so enumerating the full result was O(N^2) (one Concat MoveNext per emitted
            // element, multiplied across the recursion depth). Yielding directly lets the compiler emit a
            // single state machine per branch with O(1) amortized work per element and no intermediate
            // Concat allocations, while preserving the exact same subnet sequence (ascending by
            // NetworkPrefixAddress, no gaps, no overlaps).

            static IEnumerable<Subnet> FilledSubnets(IPAddress head, IPAddress tail, Subnet subnet, int exponent)
            {
                var networkPrefixAddress = subnet.NetworkPrefixAddress;
                var broadcastAddress = subnet.BroadcastAddress;

                // the given subnet is the perfect size for the head/tail (not papa bear, not mama bear, but just right with baby bear)
                if (networkPrefixAddress.IsGreaterThanOrEqualTo(head) && broadcastAddress.IsLessThanOrEqualTo(tail))
                {
                    yield return subnet;
                    yield break;
                }

                // increasing the route prefix by 1 creates a subnet of half the initial size (due 2^(max-n) route prefix sizing)
                var nextSmallestRoutePrefix = subnet.RoutingPrefix + 1;

                // over-iterated route prefix, no valid subnet beyond this point; end search on this branch
                if (
                    (subnet.IsIPv6 && nextSmallestRoutePrefix > IPAddressUtilities.IPv6BitCount)
                    || (subnet.IsIPv4 && nextSmallestRoutePrefix > IPAddressUtilities.IPv4BitCount)
                )
                {
                    yield break; // no subnets to be found here, stop investigating branch of tree
                }

                // build head subnet
                var headSubnet = new Subnet(networkPrefixAddress, nextSmallestRoutePrefix, exponent);

                // use the next address after the end of the head subnet as the first address for the tail subnet
                if (!IPAddressMath.TryIncrement(headSubnet.BroadcastAddress, out var tailStartingAddress))
                {
                    throw new InvalidOperationException($"unable to increment {headSubnet.BroadcastAddress}");
                }

                var tailSubnet = new Subnet(tailStartingAddress, nextSmallestRoutePrefix, exponent);

                // break into binary search tree, searching both head subnet and tail subnet for ownership of head and tail ip
                foreach (var s in FilledSubnets(head, tail, headSubnet, exponent))
                {
                    yield return s;
                }

                foreach (var s in FilledSubnets(head, tail, tailSubnet, exponent))
                {
                    yield return s;
                }
            }
        }

        /// <summary>
        ///     Return the largest subnet (smallest routing prefix value).
        ///     When multiple subnets tie for largest, which one is returned is not guaranteed beyond that one will be returned;
        ///     consider <see cref="Comparers.DefaultIIPAddressRangeComparer" /> for a stable sort.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         A smaller routing prefix means a larger address block per
        ///         <see href="https://www.rfc-editor.org/rfc/rfc4632#section-2">RFC 4632 §2</see>
        ///         (block size = 2<sup>max−prefix</sup>).
        ///     </para>
        /// </remarks>
        /// <param name="subnets">the subnets to search</param>
        /// <returns>The first largest subnet by routing prefix.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="subnets" /> is <see langword="null" />.</exception>
        /// <exception cref="InvalidOperationException"><paramref name="subnets" /> contains no elements.</exception>
        /// <exception cref="ArgumentException"><paramref name="subnets" /> contains a <see langword="null" /> element.</exception>
        public static Subnet LargestSubnet(IEnumerable<Subnet> subnets)
        {
            return SelectSubnet(subnets, (candidate, current) => candidate.RoutingPrefix < current.RoutingPrefix);
        }

        /// <summary>
        ///     Return the smallest subnet (largest routing prefix value).
        ///     When multiple subnets tie for smallest, which one is returned is not guaranteed beyond that one will be returned;
        ///     consider <see cref="Comparers.DefaultIIPAddressRangeComparer" /> for a stable sort.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         A larger routing prefix means a smaller address block per
        ///         <see href="https://www.rfc-editor.org/rfc/rfc4632#section-2">RFC 4632 §2</see>
        ///         (block size = 2<sup>max−prefix</sup>).
        ///     </para>
        /// </remarks>
        /// <param name="subnets">the subnets to search</param>
        /// <returns>The first smallest subnet by routing prefix.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="subnets" /> is <see langword="null" />.</exception>
        /// <exception cref="InvalidOperationException"><paramref name="subnets" /> contains no elements.</exception>
        /// <exception cref="ArgumentException"><paramref name="subnets" /> contains a <see langword="null" /> element.</exception>
        public static Subnet SmallestSubnet(IEnumerable<Subnet> subnets)
        {
            return SelectSubnet(subnets, (candidate, current) => candidate.RoutingPrefix > current.RoutingPrefix);
        }

        /// <summary>Iterates <paramref name="subnets" /> and returns the element for which <paramref name="shouldReplace" /> never returns <see langword="true" /> when compared against a later element.</summary>
        /// <param name="subnets">the subnets to search; must not be <see langword="null" /> or empty, and must contain no <see langword="null" /> elements.</param>
        /// <param name="shouldReplace">
        ///     A predicate of the form <c>(candidate, currentBest)</c> that returns <see langword="true" /> when
        ///     <c>candidate</c> should displace <c>currentBest</c> as the running winner.
        /// </param>
        /// <returns>The winning subnet after a single pass.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="subnets" /> is <see langword="null" />.</exception>
        /// <exception cref="InvalidOperationException"><paramref name="subnets" /> contains no elements.</exception>
        /// <exception cref="ArgumentException"><paramref name="subnets" /> contains a <see langword="null" /> element.</exception>
        private static Subnet SelectSubnet(IEnumerable<Subnet> subnets, Func<Subnet, Subnet, bool> shouldReplace)
        {
            if (subnets is null)
            {
                throw new ArgumentNullException(nameof(subnets));
            }

            using var enumerator = subnets.GetEnumerator();

            if (!enumerator.MoveNext())
            {
                throw new InvalidOperationException("Sequence contains no elements.");
            }

            var best =
                enumerator.Current
                ?? throw new ArgumentException("The collection cannot contain null elements.", nameof(subnets));

            while (enumerator.MoveNext())
            {
                var candidate =
                    enumerator.Current
                    ?? throw new ArgumentException("The collection cannot contain null elements.", nameof(subnets));

                if (shouldReplace(candidate, best))
                {
                    best = candidate;
                }
            }

            return best;
        }
    }
}
