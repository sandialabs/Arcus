namespace Arcus
{
    /// <content>
    ///     <see cref="IPAddressRange"/> operators
    /// </content>
    public partial class IPAddressRange
    {
        #region operators

        /// <summary>
        ///     Determines whether two <see cref="IPAddressRange"/> instances are equal.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns><see langword="true" /> if the instances are equal; otherwise, <see langword="false" />.</returns>
        public static bool operator ==(IPAddressRange left, IPAddressRange right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (left is null || right is null)
            {
                return false;
            }

            return left.Equals(right);
        }

        /// <summary>
        ///     Determines whether two <see cref="IPAddressRange"/> instances are not equal.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns><see langword="true" /> if the instances are not equal; otherwise, <see langword="false" />.</returns>
        public static bool operator !=(IPAddressRange left, IPAddressRange right) => !(left == right);

        /// <summary>
        ///     Determines whether one <see cref="IPAddressRange"/> is strictly less than another.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns><see langword="true" /> if <paramref name="left"/> is less than <paramref name="right"/>.</returns>
        public static bool operator <(IPAddressRange left, IPAddressRange right)
        {
            if (left is null)
            {
                return right is not null;
            }

            return left.CompareTo(right) < 0;
        }

        /// <summary>
        ///     Determines whether one <see cref="IPAddressRange"/> is strictly greater than another.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns><see langword="true" /> if <paramref name="left"/> is greater than <paramref name="right"/>.</returns>
        public static bool operator >(IPAddressRange left, IPAddressRange right)
        {
            if (left is null)
            {
                return false;
            }

            return left.CompareTo(right) > 0;
        }

        /// <summary>
        ///     Determines whether one <see cref="IPAddressRange"/> is less than or equal to another.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns><see langword="true" /> if <paramref name="left"/> is less than or equal to <paramref name="right"/>.</returns>
        public static bool operator <=(IPAddressRange left, IPAddressRange right) => left < right || left == right;

        /// <summary>
        ///     Determines whether one <see cref="IPAddressRange"/> is greater than or equal to another.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns><see langword="true" /> if <paramref name="left"/> is greater than or equal to <paramref name="right"/>.</returns>
        public static bool operator >=(IPAddressRange left, IPAddressRange right) => left > right || left == right;

        #endregion operators
    }
}
