using Arcus.Comparers;
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

namespace Arcus
{
    /// <content>
    ///     <see cref="IPAddressRange"/> implementation of <see cref="IComparable{IPAddressRange}"/>
    /// </content>
    public partial class IPAddressRange : IComparable<IPAddressRange>, IComparable
    {
        #region From Interface IComparable

        /// <inheritdoc />
        public int CompareTo(
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
            [AllowNull]
#endif
            object obj)
        {
            if (obj is null)
            {
                return 1;
            }

            if (obj is IPAddressRange other)
            {
                return CompareTo(other);
            }

            throw new ArgumentException("Object is not an IPAddressRange");
        }

        #endregion

        #region From Interface IComparable<IPAddressRange>

        /// <inheritdoc />
        /// <remarks>
        ///     Comparison delegates to <see cref="DefaultIIPAddressRangeComparer" />. Direct
        ///     <see cref="BigEndianBitWrapper" /> head-address comparison could reduce allocations
        ///     if performance becomes a concern.
        /// </remarks>
        public int CompareTo(
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
            [AllowNull]
#endif
            IPAddressRange other)
        {
            if (other is null)
            {
                return 1;
            }

            return DefaultIIPAddressRangeComparer.Instance.Compare(this, other);
        }

        #endregion
    }
}
