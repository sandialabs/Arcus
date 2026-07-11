using Arcus.Comparers;
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

namespace Arcus
{
    /// <content>
    ///     <see cref="Subnet"/> implementation of <see cref="IComparable{Subnet}"/> and <see cref="IComparable"/>
    /// </content>
    public partial class Subnet : IComparable<Subnet>, IComparable
    {
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

            if (obj is Subnet other)
            {
                return CompareTo(other);
            }

            throw new ArgumentException("Object is not a Subnet");
        }

        /// <inheritdoc />
        public int CompareTo(
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
            [AllowNull]
#endif
            Subnet other)
        {
            if (other is null)
            {
                return 1;
            }

            return DefaultIIPAddressRangeComparer.Instance.Compare(this, other);
        }
    }
}
