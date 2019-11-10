using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using JetBrains.Annotations;

namespace Arcus
{
    /// <summary>
    ///     An <see langword="interface" /> representing a range of IPAddresses.
    ///     A range must contain a head (the first address) and a tail (the last address) inclusive
    ///     The tail should NEVER appear numerically previous to a head
    /// </summary>
    [PublicAPI]
    public interface IIPAddressRange : IFormattable,
                                       IEnumerable<IPAddress>
    {
        /// <summary>
        ///     The address family of the Address Range
        ///     Typically Internetwork or InternetworkV6
        /// </summary>
        AddressFamily AddressFamily { get; }

        /// <summary>
        ///     The length of a <see cref="IIPAddressRange" />
        /// </summary>
        BigInteger Length { get; }

        /// <summary>
        ///     The head of a <see cref="IIPAddressRange" />
        /// </summary>
        [NotNull]
        IPAddress Head { get; }

        /// <summary>
        ///     The tail of a <see cref="IIPAddressRange" />
        /// </summary>
        [NotNull]
        IPAddress Tail { get; }

        /// <summary>
        ///     <see langword="true" /> if the subnet describes a single ip address
        /// </summary>
        bool IsSingleIP { get; }

        /// <summary>
        ///     Determine if IIPAddress Range is IPv4
        /// </summary>
        bool IsIPv4 { get; }

        /// <summary>
        ///     Determine if IIPAddress Range is IPv6
        /// </summary>
        bool IsIPv6 { get; }

        /// <summary>
        ///     Try to get the length as an <see cref="int" />
        /// </summary>
        /// <param name="length">the length of the AddressRange if it is possible to fit into a <see cref="int" />. Otherwise -1</param>
        /// <returns>true if the length is less than or equal to <see cref="int.MaxValue" /></returns>
        bool TryGetLength(out int length);

        /// <summary>
        ///     Try to get length as an <see cref="long" />
        /// </summary>
        /// <param name="length">the length of the AddressRange if it is possible to fit into a <see cref="long" />. Otherwise -1</param>
        /// <returns>true if the length is less than or equal to <see cref="long.MaxValue" /></returns>
        bool TryGetLength(out long length);

        #region Deconstructors

        /// <summary>
        ///     Deconstruct to Head and Tail addresses
        /// </summary>
        /// <param name="head">the head <see cref="IPAddress"/></param>
        /// <param name="tail">the tail <see cref="IPAddress"/></param>
        void Deconstruct([NotNull] out IPAddress head,
                         [NotNull] out IPAddress tail);

        #endregion // end: Deconstructors

        #region Set Operations

        /// <summary>
        ///     determine if a <see cref="IIPAddressRange" /> contains another <see cref="IIPAddressRange" />
        /// </summary>
        /// <param name="that">the secondary operand</param>
        bool Contains(IIPAddressRange that);

        /// <summary>
        ///     determine if a <see cref="IIPAddressRange" /> contains an <paramref name="address" />
        /// </summary>
        /// <param name="address">the secondary operand</param>
        bool Contains(IPAddress address);

        #region Ovelap and Touches

        /// <summary>
        ///     Return <see langword="true" /> if the head of this is within the range of <paramref name="that" />
        /// </summary>
        /// <param name="that">the secondary operand</param>
        bool HeadOverlappedBy(IIPAddressRange that);

        /// <summary>
        ///     Return <see langword="true" /> if the tail of this is within the range of <paramref name="that" />
        /// </summary>
        /// <param name="that">the secondary operand</param>
        bool TailOverlappedBy(IIPAddressRange that);

        /// <summary>
        ///     return <see langword="true" /> if this overlaps that (totally contained within, or contains either the head or the
        ///     tail)
        /// </summary>
        /// <param name="that">the secondary operand</param>
        bool Overlaps(IIPAddressRange that);

        /// <summary>
        ///     <see langword="true" /> if tail of one item is consecutively in order of head of other item
        ///     eg in sequence with no gaps in between
        /// </summary>
        /// <param name="that">the secondary operand</param>
        bool Touches(IIPAddressRange that);

        #endregion // end: Ovelap and Touches

        #endregion // end: Set Operations

        #region Contains Any/All Public/Private Addresses

        /// <summary>
        ///     Determines if the range contains any private addresses
        /// </summary>
        /// <returns><see lang="true"/> iff the range contains any private addresses</returns>
        bool ContainsAnyPrivateAddresses();

        /// <summary>
        ///     Determines if the range contains all private addresses
        /// </summary>
        /// <returns><see lang="true"/> iff the range contains all private addresses</returns>
        bool ContainsAllPrivateAddresses();

        /// <summary>
        ///     Determines if the range contains any public addresses
        /// </summary>
        /// <returns><see lang="true"/> iff the range contains any public addresses</returns>
        bool ContainsAnyPublicAddresses();

        /// <summary>
        ///     Determines if the range contains all public addresses
        /// </summary>
        /// <returns><see lang="true"/> iff the range contains all public addresses</returns>
        bool ContainsAllPublicAddresses();

        #endregion end: Contains Any/All Public/Private Addresses

    }
}
