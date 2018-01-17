using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using Arcus.Converters;
using Arcus.Math;
using Arcus.Utilities;

namespace Arcus
{
    /// <summary>
    ///     An <see langword="abstract" /> implementation of <see cref="IIPAddressRange" /> built to work with IPv4 and IPv6
    /// </summary>
    public abstract class AbstractIPAddressRange : IIPAddressRange
    {
        /// <summary>
        ///     returns an enumeration of all addresses within range
        ///     note that in certain scenarios this has the potential to be dangerous
        /// </summary>
        public IEnumerable<IPAddress> Addresses
        {
            get
            {
                if (this.Head == null
                    || this.Tail == null)
                {
                    yield break;
                }

                for (var i = this.Head.ToUnsignedBigInteger(); i < this.Tail.ToUnsignedBigInteger() + 1; i++)
                {
                    // BigInteger to IP address
                    var bytes = i.ToByteArray();
                    Array.Resize(ref bytes, this.IsIPv4 // assume IPv6 if not IPv4, so 4 bytes vs 16 bytes
                                                ? 4
                                                : 16);

                    yield return new IPAddress(bytes.Reverse()
                                                    .ToArray());
                }
            }
        }

        /// <summary>
        ///     The address family of the Address Range
        ///     Typically Internetwork or InternetworkV6
        /// </summary>
        /// <exception cref="InvalidOperationException" accessor="get">
        ///     when Head and is Tail <see langword="null" />, or do not have matching address
        ///     families
        /// </exception>
        public AddressFamily AddressFamily
        {
            get
            {
                if(this.Head == null && this.Tail == null)
                    throw new NullReferenceException(nameof(Head) + " And " + nameof(Tail));
                if(Head == null)
                    throw new NullReferenceException(nameof(Head));
                if(Tail == null)
                    throw new NullReferenceException(nameof(Tail));
                if(Head.AddressFamily != Tail.AddressFamily)
                    throw new InvalidOperationException("Head and Tail do not have matching address families!");

                return this.Head.AddressFamily;
            }
        }

        /// <summary>
        ///     Determine if IIPAddress Range is IPv4
        /// </summary>
        /// <returns></returns>
        public bool IsIPv4 => this.Head != null
                              && this.Tail != null
                              && this.Head.IsIPv4()
                              && this.Tail.IsIPv4();

        /// <summary>
        ///     Determine if IIPAddress Range is IPv6
        /// </summary>
        /// <returns></returns>
        public bool IsIPv6 => this.Head != null
                              && this.Tail != null
                              && this.Head.IsIPv6()
                              && this.Tail.IsIPv6();

        /// <summary>
        ///     The length of a <see cref="IIPAddressRange" />
        /// </summary>
        public abstract BigInteger Length { get; }

        /// <summary>
        ///     The head of a <see cref="IIPAddressRange" />
        /// </summary>
        public abstract IPAddress Head { get; set; }

        /// <summary>
        ///     The tail of a <see cref="IIPAddressRange" />
        /// </summary>
        public abstract IPAddress Tail { get; set; }

        #region From Interface IIPAddressRange

        /// <summary>
        ///     Check for contains address range
        /// </summary>
        /// <param name="addressRange"></param>
        /// <returns><see langword="true" /> if the range contains head and tail of the given range</returns>
        public bool Contains(IIPAddressRange addressRange) => addressRange != null
                                                              && addressRange.Head != null
                                                              && addressRange.Tail != null
                                                              && this.Contains(addressRange.Head)
                                                              && this.Contains(addressRange.Tail);

        /// <summary>
        ///     check for contains specific <paramref name="address" />
        /// </summary>
        /// <param name="address"></param>
        /// <returns>true if range contains the ip address</returns>
        public bool Contains(IPAddress address) => address != null
                                                   && address.AddressFamily == this.AddressFamily
                                                   && address.IsBetween(this.Head, this.Tail);

        #endregion
    }
}
