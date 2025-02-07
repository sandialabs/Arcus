using System;
using System.Collections.Generic;
using System.Net;

namespace Arcus.Comparers
{
    /// <summary>
    ///     Default <see cref="IIPAddressRange" /> <see cref="Comparer{T}" />
    ///     Compares by <see cref="IIPAddressRange.Head" /> and then by range length ordinal
    /// </summary>
    [Obsolete("Use DefaultIIPAddressRangeComparer")]
    public class DefaultIPAddressRangeComparer : Comparer<IIPAddressRange>
    {
        private readonly DefaultIIPAddressRangeComparer _comparer;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DefaultIPAddressRangeComparer" /> class.
        /// </summary>
        /// <param name="ipAddressComparer">comparer used to compare <see cref="IPAddress" /></param>
        /// <exception cref="ArgumentNullException"><paramref name="ipAddressComparer" /> is <see langword="null" />.</exception>
        public DefaultIPAddressRangeComparer(IComparer<IPAddress> ipAddressComparer)
        {
            if (ipAddressComparer is null)
            {
                throw new ArgumentNullException(nameof(ipAddressComparer));
            }

            this._comparer = new DefaultIIPAddressRangeComparer(ipAddressComparer);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DefaultIPAddressRangeComparer" /> class.
        ///     Defaults to use the DefaultIPAddressComparer
        /// </summary>
        public DefaultIPAddressRangeComparer()
            : this(DefaultIPAddressComparer.Instance) { }

        /// <inheritdoc />
        public override int Compare(IIPAddressRange x, IIPAddressRange y)
        {
            return _comparer.Compare(x, y);
        }
    }
}
