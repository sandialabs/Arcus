using System.Net;
using System.Net.Sockets;
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

namespace Arcus.Comparers
{
    /// <summary>
    ///     Default <see cref="Comparer{IPAddress}" /> for <see cref="IPAddress" />.
    ///     Compares by <see cref="AddressFamily" /> first, then by the numeric value of the address.
    /// </summary>
    public sealed class DefaultIPAddressComparer : Comparer<IPAddress>
    {
        /// <summary>
        ///     Default singleton instance using <see cref="DefaultAddressFamilyComparer.Instance"/>.
        /// </summary>
        public static readonly DefaultIPAddressComparer Instance = new();

        private readonly IComparer<AddressFamily> _addressFamilyComparer;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DefaultIPAddressComparer" /> class.
        /// </summary>
        /// <param name="addressFamilyComparer">the <see cref="AddressFamily" /> comparer</param>
        /// <exception cref="ArgumentNullException"><paramref name="addressFamilyComparer" /> is <see langword="null" />.</exception>
        public DefaultIPAddressComparer(IComparer<AddressFamily> addressFamilyComparer)
        {
            if (addressFamilyComparer is null)
            {
                throw new ArgumentNullException(nameof(addressFamilyComparer));
            }

            this._addressFamilyComparer = addressFamilyComparer;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DefaultIPAddressComparer" /> class.
        /// </summary>
        public DefaultIPAddressComparer()
            : this(DefaultAddressFamilyComparer.Instance) { }

        /// <inheritdoc />
        public override int Compare(
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
            [AllowNull]
#endif
            IPAddress x,
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
            [AllowNull]
#endif
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
                ? BigEndianBitWrapper
                    .FromBytes(x.GetAddressBytes())
                    .CompareTo(BigEndianBitWrapper.FromBytes(y.GetAddressBytes()))
                : addressFamilyComparison;
        }
    }
}
