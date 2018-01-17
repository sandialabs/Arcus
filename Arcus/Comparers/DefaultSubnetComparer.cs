using System;
using System.Collections.Generic;
using System.Net;
using JetBrains.Annotations;

namespace Arcus.Comparers
{
    /// <summary>
    ///     Default <see cref="Subnet" /> comparer
    /// </summary>
    public class DefaultSubnetComparer : Comparer<Subnet>
    {
        private readonly IComparer<IPAddress> _ipAddressComparer;

        /// <exception cref="ArgumentNullException"><paramref name="ipAddressComparer" /> is <see langword="null" />.</exception>
        public DefaultSubnetComparer([NotNull] IComparer<IPAddress> ipAddressComparer)
        {
            if (ipAddressComparer == null)
            {
                throw new ArgumentNullException(nameof(ipAddressComparer));
            }

            this._ipAddressComparer = ipAddressComparer;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="T:System.Collections.Generic.Comparer`1" /> class.
        ///     Defaults to use the DefaultIPAddressComparer
        /// </summary>
        public DefaultSubnetComparer()
            : this(new DefaultIPAddressComparer()) {}

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
        public override int Compare(Subnet x,
                                    Subnet y)
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

            var headComparison = this._ipAddressComparer.Compare(x.Head, y.Head);
            return headComparison != 0
                       ? headComparison
                       : x.Length.CompareTo(y.Length);
        }
    }
}
