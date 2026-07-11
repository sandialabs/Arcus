namespace Arcus
{
    /// <content>
    ///     <see cref="Subnet"/> operators
    /// </content>
    public partial class Subnet
    {
        /// <summary>
        ///     Determines whether two <see cref="Subnet"/> instances are equal.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns><see langword="true" /> if the instances are equal; otherwise, <see langword="false" />.</returns>
        public static bool operator ==(Subnet left, Subnet right)
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
        ///     Determines whether two <see cref="Subnet"/> instances are not equal.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns><see langword="true" /> if the instances are not equal; otherwise, <see langword="false" />.</returns>
        public static bool operator !=(Subnet left, Subnet right) => !(left == right);

        /// <summary>
        ///     Determines whether one <see cref="Subnet"/> is strictly less than another.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns><see langword="true" /> if <paramref name="left"/> is less than <paramref name="right"/>.</returns>
        public static bool operator <(Subnet left, Subnet right)
        {
            if (left is null)
            {
                return right is not null;
            }

            return left.CompareTo(right) < 0;
        }

        /// <summary>
        ///     Determines whether one <see cref="Subnet"/> is strictly greater than another.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns><see langword="true" /> if <paramref name="left"/> is greater than <paramref name="right"/>.</returns>
        public static bool operator >(Subnet left, Subnet right)
        {
            if (left is null)
            {
                return false;
            }

            return left.CompareTo(right) > 0;
        }

        /// <summary>
        ///     Determines whether one <see cref="Subnet"/> is less than or equal to another.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns><see langword="true" /> if <paramref name="left"/> is less than or equal to <paramref name="right"/>.</returns>
        public static bool operator <=(Subnet left, Subnet right) => !(left > right);

        /// <summary>
        ///     Determines whether one <see cref="Subnet"/> is greater than or equal to another.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns><see langword="true" /> if <paramref name="left"/> is greater than or equal to <paramref name="right"/>.</returns>
        public static bool operator >=(Subnet left, Subnet right) => !(left < right);
    }
}
