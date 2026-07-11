namespace Arcus
{
    /// <content><see cref="BigEndianBitWrapper"/> operators</content>
    internal readonly partial struct BigEndianBitWrapper
    {
        /// <summary>Computes the bitwise AND of two wrappers; the result inherits the <see cref="ByteWidth" /> of <paramref name="left" />.</summary>
        /// <param name="left">The left operand; its <see cref="ByteWidth" /> is used for the result.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>A new wrapper containing the bitwise AND of the two values.</returns>
        public static BigEndianBitWrapper operator &(BigEndianBitWrapper left, BigEndianBitWrapper right)
        {
            if (left.ByteWidth != right.ByteWidth)
            {
                throw new ArgumentException("Operands must have the same ByteWidth.");
            }
#if NET8_0_OR_GREATER
            var masked = left._value & right._value & left.MaxValueForWidth;
            return new BigEndianBitWrapper(masked, left.ByteWidth);
#else
            var (maxHi, maxLo) = left.MaxHiLoForWidth;
            return new BigEndianBitWrapper(left._hi & right._hi & maxHi, left._lo & right._lo & maxLo, left.ByteWidth);
#endif
        }

        /// <summary>Computes the bitwise OR of two wrappers; the result inherits the <see cref="ByteWidth" /> of <paramref name="left" />.</summary>
        /// <param name="left">The left operand; its <see cref="ByteWidth" /> is used for the result.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>A new wrapper containing the bitwise OR of the two values.</returns>
        public static BigEndianBitWrapper operator |(BigEndianBitWrapper left, BigEndianBitWrapper right)
        {
            if (left.ByteWidth != right.ByteWidth)
            {
                throw new ArgumentException("Operands must have the same ByteWidth.");
            }
#if NET8_0_OR_GREATER
            var masked = (left._value | right._value) & left.MaxValueForWidth;
            return new BigEndianBitWrapper(masked, left.ByteWidth);
#else
            var (maxHi, maxLo) = left.MaxHiLoForWidth;
            return new BigEndianBitWrapper((left._hi | right._hi) & maxHi, (left._lo | right._lo) & maxLo, left.ByteWidth);
#endif
        }

        /// <summary>Computes the bitwise XOR of two wrappers; the result inherits the <see cref="ByteWidth" /> of <paramref name="left" />.</summary>
        /// <param name="left">The left operand; its <see cref="ByteWidth" /> is used for the result.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>A new wrapper containing the bitwise XOR of the two values.</returns>
        public static BigEndianBitWrapper operator ^(BigEndianBitWrapper left, BigEndianBitWrapper right)
        {
            if (left.ByteWidth != right.ByteWidth)
            {
                throw new ArgumentException("Operands must have the same ByteWidth.");
            }
#if NET8_0_OR_GREATER
            var masked = (left._value ^ right._value) & left.MaxValueForWidth;
            return new BigEndianBitWrapper(masked, left.ByteWidth);
#else
            var (maxHi, maxLo) = left.MaxHiLoForWidth;
            return new BigEndianBitWrapper((left._hi ^ right._hi) & maxHi, (left._lo ^ right._lo) & maxLo, left.ByteWidth);
#endif
        }

        /// <summary>
        ///     Computes the bitwise NOT of <paramref name="value" />, bounded to <see cref="ByteWidth" />. Bits above the
        ///     byte-width boundary are never set in the result.
        /// </summary>
        /// <param name="value">The operand to invert.</param>
        /// <returns>A new wrapper with all bits in the <see cref="ByteWidth" /> range flipped.</returns>
        public static BigEndianBitWrapper operator ~(BigEndianBitWrapper value)
        {
#if NET8_0_OR_GREATER
            return new BigEndianBitWrapper(value.MaxValueForWidth ^ value._value, value.ByteWidth);
#else
            var (maxHi, maxLo) = value.MaxHiLoForWidth;
            return new BigEndianBitWrapper(maxHi ^ value._hi, maxLo ^ value._lo, value.ByteWidth);
#endif
        }

        /// <summary>Returns <see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> are equal in both numeric value and <see cref="ByteWidth" />.</summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns><see langword="true" /> if both operands are equal; otherwise <see langword="false" />.</returns>
        public static bool operator ==(BigEndianBitWrapper left, BigEndianBitWrapper right) => left.Equals(right);

        /// <summary>Returns <see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> differ in numeric value or <see cref="ByteWidth" />.</summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns><see langword="true" /> if the operands are not equal; otherwise <see langword="false" />.</returns>
        public static bool operator !=(BigEndianBitWrapper left, BigEndianBitWrapper right) => !left.Equals(right);

        /// <summary>Returns <see langword="true" /> if <paramref name="left" /> is less than <paramref name="right" />.</summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns><see langword="true" /> if <paramref name="left" /> is less than <paramref name="right" />; otherwise <see langword="false" />.</returns>
        public static bool operator <(BigEndianBitWrapper left, BigEndianBitWrapper right) => left.CompareTo(right) < 0;

        /// <summary>Returns <see langword="true" /> if <paramref name="left" /> is greater than <paramref name="right" />.</summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns><see langword="true" /> if <paramref name="left" /> is greater than <paramref name="right" />; otherwise <see langword="false" />.</returns>
        public static bool operator >(BigEndianBitWrapper left, BigEndianBitWrapper right) => left.CompareTo(right) > 0;

        /// <summary>Returns <see langword="true" /> if <paramref name="left" /> is less than or equal to <paramref name="right" />.</summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns><see langword="true" /> if <paramref name="left" /> is less than or equal to <paramref name="right" />; otherwise <see langword="false" />.</returns>
        public static bool operator <=(BigEndianBitWrapper left, BigEndianBitWrapper right) => left.CompareTo(right) <= 0;

        /// <summary>Returns <see langword="true" /> if <paramref name="left" /> is greater than or equal to <paramref name="right" />.</summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns><see langword="true" /> if <paramref name="left" /> is greater than or equal to <paramref name="right" />; otherwise <see langword="false" />.</returns>
        public static bool operator >=(BigEndianBitWrapper left, BigEndianBitWrapper right) => left.CompareTo(right) >= 0;
    }
}
