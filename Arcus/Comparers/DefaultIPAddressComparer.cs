using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Arcus.Converters;
using JetBrains.Annotations;

namespace Arcus.Comparers
{
    /// <summary>
    ///     Address <see cref="Comparer{T}" />
    /// </summary>
    public class DefaultIPAddressComparer : Comparer<IPAddress>
    {
        private readonly IComparer<AddressFamily> _addressFamilyComparer;

        /// <summary>
        ///     Initializes a new instance of the <see cref="T:System.Collections.Generic.Comparer`1" /> class.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="addressFamilyComparer" /> is <see langword="null" />.</exception>
        public DefaultIPAddressComparer([NotNull] IComparer<AddressFamily> addressFamilyComparer)
        {
            if (addressFamilyComparer == null)
            {
                throw new ArgumentNullException(nameof(addressFamilyComparer));
            }

            this._addressFamilyComparer = addressFamilyComparer;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="T:System.Collections.Generic.Comparer`1" /> class.
        ///     Defaults such to use the DefaultAddressFamilyComparer
        /// </summary>
        public DefaultIPAddressComparer()
            : this(new DefaultAddressFamilyComparer()) {}

        /// <summary>
        ///     Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the
        ///     other.
        /// </summary>
        /// <returns>
        ///     A signed integer that indicates the relative values of <paramref name="x" /> and <paramref name="y" />, as
        ///     shown in the following table.Value Meaning Less than zero<paramref name="x" /> is less than <paramref name="y" />
        ///     .Zero<paramref name="x" /> equals <paramref name="y" />.Greater than zero<paramref name="x" /> is greater than
        ///     <paramref name="y" />.
        /// </returns>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        public override int Compare(IPAddress x,
                                    IPAddress y)
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
                       ? x.ToUnsignedBigInteger()
                          .CompareTo(y.ToUnsignedBigInteger())
                       : addressFamilyComparison;
        }
    }
}
