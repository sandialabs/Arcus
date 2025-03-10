using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Gulliver;

namespace Arcus.Comparers
{
    /// <summary>
    ///     Default <see cref="IPAddress" /> <see cref="Comparer{T}" />
    ///     Compares the <see cref="AddressFamily" /> then the integer equivalent value of an <see cref="IPAddress" /> in
    ///     ordinal order
    /// </summary>
    public class DefaultIPAddressComparer : Comparer<IPAddress>
    {
        /// <summary>
        ///     Default instance of <see cref="DefaultIPAddressComparer"/> using <see cref="DefaultAddressFamilyComparer.Instance"/>
        /// </summary>
        public static readonly DefaultIPAddressComparer Instance = new DefaultIPAddressComparer();

        private readonly IComparer<AddressFamily> _addressFamilyComparer;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DefaultIPAddressComparer" /> class.
        /// </summary>
        /// <param name="addressFamilyComparer">the <see cref="AddressFamily" /> comparer</param>
        /// <exception cref="ArgumentNullException"><paramref name="addressFamilyComparer" /> is <see langword="null" />.</exception>
        public DefaultIPAddressComparer(IComparer<AddressFamily> addressFamilyComparer)
        {
            if (addressFamilyComparer == null)
            {
                throw new ArgumentNullException(nameof(addressFamilyComparer));
            }

            this._addressFamilyComparer =
                addressFamilyComparer ?? throw new ArgumentNullException(nameof(addressFamilyComparer));
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DefaultIPAddressComparer" /> class.
        /// </summary>
        public DefaultIPAddressComparer()
            : this(DefaultAddressFamilyComparer.Instance) { }

        /// <inheritdoc />
        public override int Compare(IPAddress x, IPAddress y)
        {
            if (ReferenceEquals(x, y))
            {
                return 0;
            }

            if (x == null)
            {
                return -1;
            }

            if (y == null)
            {
                return 1;
            }

            var addressFamilyComparison = this._addressFamilyComparer.Compare(x.AddressFamily, y.AddressFamily);

            return addressFamilyComparison == 0
                ? ByteArrayUtils.CompareUnsignedBigEndian(x.GetAddressBytes(), y.GetAddressBytes())
                : addressFamilyComparison;
        }
    }
}
