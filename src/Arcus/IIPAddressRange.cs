using System.Net;
using System.Net.Sockets;
using System.Numerics;

namespace Arcus
{
    /// <summary>
    ///     An <see langword="interface" /> representing a range of IPAddresses.
    ///     A range must contain a head (the first address) and a tail (the last address) inclusive
    ///     The tail should NEVER appear numerically previous to a head
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Covers both the IPv4 32-bit address space per
    ///         <see href="https://www.rfc-editor.org/rfc/rfc791#section-2.3">RFC 791 §2.3</see> and the IPv6 128-bit address
    ///         space per <see href="https://www.rfc-editor.org/rfc/rfc4291#section-2.1">RFC 4291 §2.1</see>.
    ///         <see cref="Length"/> uses <see cref="BigInteger"/> because the IPv6 space (2<sup>128</sup>
    ///         addresses) exceeds <see cref="long.MaxValue"/>.
    ///     </para>
    /// </remarks>
    public interface IIPAddressRange : IFormattable, IEnumerable<IPAddress>
    {
        /// <summary>
        ///     Returns an <see cref="IEnumerable{T}"/> of the <see cref="IPAddress"/> values in this range, from
        ///     <see cref="Head"/> to <see cref="Tail"/>, up to the cap defined by <c>MaxEnumerationExponent</c>.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="IPAddress"/> values in the range.</returns>
        IEnumerable<IPAddress> ToIPAddresses();

        /// <summary>
        ///     Gets the address family of the range.
        /// </summary>
        /// <value>
        ///     <see cref="System.Net.Sockets.AddressFamily.InterNetwork"/> for IPv4 ranges,
        ///     <see cref="System.Net.Sockets.AddressFamily.InterNetworkV6"/> for IPv6 ranges.
        /// </value>
        AddressFamily AddressFamily { get; }

        /// <summary>
        ///     Gets the number of addresses in this range.
        /// </summary>
        /// <value>
        ///     Total count of addresses from <see cref="Head"/> to <see cref="Tail"/>, inclusive.
        ///     Uses <see cref="BigInteger"/> because IPv6 ranges (2<sup>128</sup>) exceed
        ///     <see cref="long.MaxValue"/>.
        /// </value>
        /// <remarks>
        ///     <para>
        ///         Typed as <see cref="BigInteger"/> because the IPv6 address space contains 2<sup>128</sup>
        ///         addresses per <see href="https://www.rfc-editor.org/rfc/rfc4291#section-2.1">RFC 4291 §2.1</see>,
        ///         which exceeds <see cref="long.MaxValue"/>.
        ///     </para>
        /// </remarks>
        BigInteger Length { get; }

        /// <summary>
        ///     Gets the first (numerically lowest) address in the range.
        /// </summary>
        /// <value>The first <see cref="IPAddress"/> in the range, inclusive.</value>
        IPAddress Head { get; }

        /// <summary>
        ///     Gets the last (numerically highest) address in the range.
        /// </summary>
        /// <value>The last <see cref="IPAddress"/> in the range, inclusive.</value>
        IPAddress Tail { get; }

        /// <summary>
        ///     Gets a value indicating whether this range contains exactly one address.
        /// </summary>
        /// <value><see langword="true" /> if <see cref="Head"/> equals <see cref="Tail"/>.</value>
        bool IsSingleIP { get; }

        /// <summary>
        ///     Gets a value indicating whether the range spans only IPv4 addresses.
        /// </summary>
        /// <value><see langword="true" /> if the range uses the IPv4 address family.</value>
        bool IsIPv4 { get; }

        /// <summary>
        ///     Gets a value indicating whether the range spans only IPv6 addresses.
        /// </summary>
        /// <value><see langword="true" /> if the range uses the IPv6 address family.</value>
        bool IsIPv6 { get; }

        /// <summary>
        ///     Attempts to get the range length as an <see cref="int" />.
        /// </summary>
        /// <param name="length">When successful, the range length; otherwise <c>-1</c>.</param>
        /// <returns><see langword="true" /> if the length fits in an <see cref="int" />.</returns>
        bool TryGetLength(out int length);

        /// <summary>
        ///     Attempts to get the range length as an <see cref="long" />.
        /// </summary>
        /// <param name="length">When successful, the range length; otherwise <c>-1</c>.</param>
        /// <returns><see langword="true" /> if the length fits in a <see cref="long" />.</returns>
        bool TryGetLength(out long length);

        #region Deconstructors

        /// <summary>
        ///     Deconstructs the range into its <see cref="Head"/> and <see cref="Tail"/> addresses.
        /// </summary>
        /// <param name="head">Receives the head <see cref="IPAddress"/>.</param>
        /// <param name="tail">Receives the tail <see cref="IPAddress"/>.</param>
        void Deconstruct(out IPAddress head, out IPAddress tail);

        #endregion // end: Deconstructors

        #region Set Operations

        /// <summary>
        ///     Determines whether this range contains another range in full.
        /// </summary>
        /// <param name="addressRange">The range to test.</param>
        /// <returns><see langword="true" /> if <paramref name="addressRange"/> is wholly contained within this range.</returns>
        bool Contains(IIPAddressRange addressRange);

        /// <summary>
        ///     Determines whether this range contains a specific address.
        /// </summary>
        /// <param name="address">The address to test.</param>
        /// <returns><see langword="true" /> if <paramref name="address"/> is within this range.</returns>
        bool Contains(IPAddress address);

        #region Ovelap and Touches

        /// <summary>
        ///     Determines whether the <see cref="Head"/> of this range lies within <paramref name="addressRange" />.
        /// </summary>
        /// <param name="addressRange">The range to test against.</param>
        /// <returns><see langword="true" /> if this range's head is contained by <paramref name="addressRange" />.</returns>
        bool HeadOverlappedBy(IIPAddressRange addressRange);

        /// <summary>
        ///     Determines whether the <see cref="Tail"/> of this range lies within <paramref name="addressRange" />.
        /// </summary>
        /// <param name="addressRange">The range to test against.</param>
        /// <returns><see langword="true" /> if this range's tail is contained by <paramref name="addressRange" />.</returns>
        bool TailOverlappedBy(IIPAddressRange addressRange);

        /// <summary>
        ///     Determines whether this range overlaps another range.
        /// </summary>
        /// <param name="addressRange">The range to test.</param>
        /// <returns><see langword="true" /> if the two ranges share any address in common.</returns>
        bool Overlaps(IIPAddressRange addressRange);

        /// <summary>
        ///     Determines whether this range touches another range without overlap.
        /// </summary>
        /// <param name="addressRange">The range to test.</param>
        /// <returns>
        ///     <see langword="true" /> if the ranges are consecutive with no gap between them
        ///     (e.g., this range's tail immediately precedes the other range's head).
        /// </returns>
        bool Touches(IIPAddressRange addressRange);

        #endregion // end: Ovelap and Touches

        #endregion // end: Set Operations

        #region Contains Any/All Public/Private Addresses

        /// <summary>
        ///     Determines whether the range contains any private (RFC 1918) addresses.
        /// </summary>
        /// <returns><see langword="true"/> if any address in this range is a private address.</returns>
        bool ContainsAnyPrivateAddresses();

        /// <summary>
        ///     Determines whether every address in the range is a private (RFC 1918) address.
        /// </summary>
        /// <returns><see langword="true"/> if all addresses in this range are private addresses.</returns>
        bool ContainsAllPrivateAddresses();

        /// <summary>
        ///     Determines whether the range contains any public (non-private) addresses.
        /// </summary>
        /// <returns><see langword="true"/> if any address in this range is a public address.</returns>
        bool ContainsAnyPublicAddresses();

        /// <summary>
        ///     Determines whether every address in the range is a public (non-private) address.
        /// </summary>
        /// <returns><see langword="true"/> if all addresses in this range are public addresses.</returns>
        bool ContainsAllPublicAddresses();

        #endregion end: Contains Any/All Public/Private Addresses
    }
}
