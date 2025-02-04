using System;
using System.Collections.Generic;
using System.Net;

namespace Arcus.Comparers
{
    /// <summary>
    ///     Default <see cref="IIPAddressRange" /> <see cref="Comparer{T}" />
    ///     Compares by <see cref="IIPAddressRange.Head" /> and then by range length ordinal
    /// </summary>
    public class DefaultIPAddressRangeComparer : Comparer<IIPAddressRange>
    {
        private readonly IComparer<IPAddress> _ipAddressComparer;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DefaultIPAddressRangeComparer" /> class.
        /// </summary>
        /// <param name="ipAddressComparer">comparer used to compare <see cref="IPAddress" /></param>
        /// <exception cref="ArgumentNullException"><paramref name="ipAddressComparer" /> is <see langword="null" />.</exception>
        public DefaultIPAddressRangeComparer(IComparer<IPAddress> ipAddressComparer)
        {
            this._ipAddressComparer = ipAddressComparer ?? throw new ArgumentNullException(nameof(ipAddressComparer));
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DefaultIPAddressRangeComparer" /> class.
        ///     Defaults to use the DefaultIPAddressComparer
        /// </summary>
        public DefaultIPAddressRangeComparer()
            : this(new DefaultIPAddressComparer()) { }

        /// <inheritdoc />
        public override int Compare(IIPAddressRange x, IIPAddressRange y)
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
            return headComparison != 0 ? headComparison : x.Length.CompareTo(y.Length);
        }
    }
}
