using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using JetBrains.Annotations;

namespace Arcus
{
    /// <summary>
    ///     An <see langword="interface" /> representing a range of IPAddresses
    ///     A range must contain a head (the first address) and a tail (the last address) inclusive
    ///     The tail should NEVER appear numerically previous to a head
    /// </summary>
    public interface IIPAddressRange
    {
        /// <summary>
        ///     returns an enumeration of all addresses within range
        ///     note that in certain scenarios this has the potential to be dangerous
        /// </summary>
        [NotNull]
        IEnumerable<IPAddress> Addresses { get; }

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
        IPAddress Head { get; set; }

        /// <summary>
        ///     The tail of a <see cref="IIPAddressRange" />
        /// </summary>
        IPAddress Tail { get; set; }

        /// <summary>
        ///     determine if a <see cref="IIPAddressRange" /> contains another <see cref="IIPAddressRange" />
        /// </summary>
        bool Contains([CanBeNull] IIPAddressRange addressRange);

        /// <summary>
        ///     determine if a <see cref="IIPAddressRange" /> contains an <paramref name="address" />
        /// </summary>
        bool Contains([CanBeNull] IPAddress address);
    }
}
