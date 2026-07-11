using System.Net.Sockets;

namespace Arcus.Comparers
{
    /// <summary>
    ///     Default <see cref="Comparer{AddressFamily}" /> for <see cref="AddressFamily" />.
    ///     Delegates to the <see cref="Enum.CompareTo" /> implementation of <see cref="AddressFamily" />.
    /// </summary>
    public sealed class DefaultAddressFamilyComparer : Comparer<AddressFamily>
    {
        /// <summary>
        ///     Default singleton instance.
        /// </summary>
        public static readonly DefaultAddressFamilyComparer Instance = new();

        /// <inheritdoc />
        public override int Compare(AddressFamily x, AddressFamily y)
        {
            return x.CompareTo(y);
        }
    }
}
