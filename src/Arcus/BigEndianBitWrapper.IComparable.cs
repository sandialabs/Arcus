using System.Diagnostics;

namespace Arcus
{
    /// <content><see cref="BigEndianBitWrapper"/> implementation of <see cref="System.IComparable{BigEndianBitWrapper}"/></content>
    internal readonly partial struct BigEndianBitWrapper
    {
        /// <summary>
        ///     Compares this instance to <paramref name="other" /> as unsigned integers.
        ///     When numeric values are equal, <see cref="ByteWidth" /> is used as a tiebreaker so
        ///     that the comparison order is consistent with <see cref="Equals(BigEndianBitWrapper)" />.
        /// </summary>
        /// <param name="other">The value to compare against.</param>
        /// <returns>A negative integer if less than, zero if equal, positive if greater than.</returns>
        public int CompareTo(BigEndianBitWrapper other)
        {
            Debug.Assert(ByteWidth > 0, "ByteWidth must be greater than zero.");
#if NET8_0_OR_GREATER
            var result = _value.CompareTo(other._value);
            return result != 0 ? result : ByteWidth.CompareTo(other.ByteWidth);
#else
            if (_hi != other._hi)
            {
                return _hi.CompareTo(other._hi);
            }

            if (_lo != other._lo)
            {
                return _lo.CompareTo(other._lo);
            }

            return ByteWidth.CompareTo(other.ByteWidth);
#endif
        }
    }
}
