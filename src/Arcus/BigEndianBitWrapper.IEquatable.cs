namespace Arcus
{
    /// <content><see cref="BigEndianBitWrapper"/> implementation of <see cref="IEquatable{BigEndianBitWrapper}"/></content>
    internal readonly partial struct BigEndianBitWrapper
    {
        /// <summary>
        ///     Returns <see langword="true" /> when <paramref name="other" /> has the same numeric value
        ///     and the same <see cref="ByteWidth" />.
        /// </summary>
        /// <param name="other">The value to compare.</param>
        /// <returns><see langword="true" /> if equal.</returns>
        public bool Equals(BigEndianBitWrapper other) =>
#if NET8_0_OR_GREATER
            _value == other._value && ByteWidth == other.ByteWidth;
#else
            _hi == other._hi && _lo == other._lo && ByteWidth == other.ByteWidth;
#endif

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is BigEndianBitWrapper other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode() =>
#if NET8_0_OR_GREATER
            HashCode.Combine(_value, ByteWidth);
#else
            HashCode.Combine(_hi, _lo, ByteWidth);
#endif
    }
}
