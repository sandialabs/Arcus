namespace Arcus
{
    /// <content><see cref="BigEndianBitWrapper"/> static factory methods</content>
    internal readonly partial struct BigEndianBitWrapper
    {
        /// <summary>
        ///     Creates a <see cref="BigEndianBitWrapper" /> from a big-endian byte array.
        ///     <see cref="ByteWidth" /> is set to <paramref name="bigEndianBytes" />.Length.
        /// </summary>
        /// <param name="bigEndianBytes">A non-empty, non-null big-endian byte array of at most 16 bytes.</param>
        /// <returns>A wrapper whose value and byte width match the supplied array.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="bigEndianBytes" /> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException"><paramref name="bigEndianBytes" /> is empty or longer than 16 bytes.</exception>
        public static BigEndianBitWrapper FromBytes(byte[] bigEndianBytes)
        {
            if (bigEndianBytes is null)
            {
                throw new ArgumentNullException(nameof(bigEndianBytes));
            }

            if (bigEndianBytes.Length == 0)
            {
                throw new ArgumentException("Byte array cannot be empty.", nameof(bigEndianBytes));
            }

            if (bigEndianBytes.Length > 16)
            {
                throw new ArgumentException("Byte array cannot exceed 16 bytes.", nameof(bigEndianBytes));
            }

            return FromBytesCore(bigEndianBytes, bigEndianBytes.Length);
        }

        /// <summary>
        ///     Creates a <see cref="BigEndianBitWrapper" /> from a big-endian byte array, zero-padding
        ///     the most-significant side to reach <paramref name="targetWidth" />.
        ///     <see cref="ByteWidth" /> is set to <paramref name="targetWidth" />.
        /// </summary>
        /// <remarks>
        ///     An empty <paramref name="bigEndianBytes" /> with a positive <paramref name="targetWidth" />
        ///     produces an all-zero wrapper (value 0, ByteWidth = targetWidth).
        /// </remarks>
        /// <param name="bigEndianBytes">
        ///     A non-null big-endian byte array whose length does not exceed
        ///     <paramref name="targetWidth" />.
        /// </param>
        /// <param name="targetWidth">Target byte width (1-16).</param>
        /// <returns>
        ///     A wrapper with the value from <paramref name="bigEndianBytes" /> and
        ///     <see cref="ByteWidth" /> equal to <paramref name="targetWidth" />.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="bigEndianBytes" /> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     <paramref name="targetWidth" /> is less than 1 or greater than 16.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="bigEndianBytes" />.Length exceeds <paramref name="targetWidth" />.
        /// </exception>
        public static BigEndianBitWrapper FromBytes(byte[] bigEndianBytes, int targetWidth)
        {
            if (bigEndianBytes is null)
            {
                throw new ArgumentNullException(nameof(bigEndianBytes));
            }

            if (targetWidth is < 1 or > 16)
            {
                throw new ArgumentOutOfRangeException(nameof(targetWidth), "Target width must be between 1 and 16.");
            }

            if (bigEndianBytes.Length > targetWidth)
            {
                throw new ArgumentException(
                    $"Byte array length ({bigEndianBytes.Length}) exceeds target width ({targetWidth}).",
                    nameof(bigEndianBytes)
                );
            }

            // Leading zero bytes don't affect the integer value, so shorter arrays can be
            // passed directly; FromBytesCore sets ByteWidth = targetWidth regardless.
            return FromBytesCore(bigEndianBytes, targetWidth);
        }

        /// <summary>
        ///     Creates a subnet mask with the top <paramref name="prefixLength" /> bits set to 1 and the
        ///     remaining host bits set to 0. For example, <c>CreateMask(4, 24)</c> produces the IPv4
        ///     netmask 255.255.255.0.
        /// </summary>
        /// <param name="byteWidth">Byte width of the address family (1-16).</param>
        /// <param name="prefixLength">Number of leading network bits to set; must be in [0, byteWidth × 8].</param>
        /// <returns>A <see cref="BigEndianBitWrapper" /> representing the subnet mask.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     <paramref name="byteWidth" /> is not in [1, 16], or <paramref name="prefixLength" /> is not
        ///     in [0, byteWidth × 8].
        /// </exception>
        public static BigEndianBitWrapper CreateMask(int byteWidth, int prefixLength)
        {
            if (byteWidth is < 1 or > 16)
            {
                throw new ArgumentOutOfRangeException(nameof(byteWidth), "Byte width must be between 1 and 16.");
            }

            var totalBits = byteWidth * 8;

            if (prefixLength < 0 || prefixLength > totalBits)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(prefixLength),
                    $"Prefix length must be between 0 and {totalBits} for a {byteWidth}-byte address."
                );
            }

            if (prefixLength == 0)
            {
#if NET8_0_OR_GREATER
                return new BigEndianBitWrapper(UInt128.Zero, byteWidth);
#else
                return new BigEndianBitWrapper(0UL, 0UL, byteWidth);
#endif
            }

            var hostBits = totalBits - prefixLength;

#if NET8_0_OR_GREATER
            var allOnes = byteWidth >= 16 ? UInt128.MaxValue : (UInt128.One << totalBits) - 1;
            // Shift right to drop the low hostBits, then shift back left to restore bit positions -
            // this clears the trailing hostBits while keeping the leading prefixLength bits set.
            var mask = hostBits == 0 ? allOnes : (allOnes >> hostBits) << hostBits;
            return new BigEndianBitWrapper(mask, byteWidth);
#else
            return CreateMaskLegacy(byteWidth, hostBits);
#endif
        }

        /// <summary>
        ///     Converts a validated big-endian byte array into a <see cref="BigEndianBitWrapper" />,
        ///     setting <see cref="ByteWidth" /> to <paramref name="byteWidth" />. Callers are responsible
        ///     for all argument validation.
        /// </summary>
        /// <param name="bytes">
        ///     Big-endian byte array whose length is at most <paramref name="byteWidth" />; bytes absent
        ///     from the leading (most-significant) side are treated as implicit zeros.
        /// </param>
        /// <param name="byteWidth">Byte width to assign to the returned wrapper; must be in [1, 16].</param>
        /// <returns>A wrapper whose numeric value equals <paramref name="bytes" /> interpreted as an unsigned big-endian integer, with <see cref="ByteWidth" /> set to <paramref name="byteWidth" />.</returns>
        private static BigEndianBitWrapper FromBytesCore(byte[] bytes, int byteWidth)
        {
#if NET8_0_OR_GREATER
            UInt128 value = 0;
            foreach (var byteValue in bytes)
            {
                value = (value << 8) | byteValue;
            }

            return new BigEndianBitWrapper(value, byteWidth);
#else
            ulong hi = 0,
                lo = 0;
            var len = bytes.Length;

            if (len <= 8)
            {
                foreach (var byteValue in bytes)
                {
                    lo = (lo << 8) | byteValue;
                }
            }
            else
            {
                // Bytes [0, hiCount) fill hi MSB-first; the remaining 8 bytes fill lo.
                var hiCount = len - 8;
                for (var i = 0; i < hiCount; i++)
                {
                    hi = (hi << 8) | bytes[i];
                }

                for (var i = hiCount; i < len; i++)
                {
                    lo = (lo << 8) | bytes[i];
                }
            }

            return new BigEndianBitWrapper(hi, lo, byteWidth);
#endif
        }

#if !NET8_0_OR_GREATER
        /// <summary>
        ///     Builds a subnet mask for the pre-.NET 8 two-field (hi/lo) representation.
        ///     Called from <see cref="CreateMask" /> on targets that lack <c>UInt128</c>.
        /// </summary>
        /// <param name="byteWidth">Byte width of the address family; must be in [1, 16].</param>
        /// <param name="hostBits">Number of low bits to clear; equals total bits minus prefix length. Must be &gt; 0.</param>
        /// <returns>A wrapper whose high <c>byteWidth × 8 − hostBits</c> bits are set and whose low <c>hostBits</c> bits are cleared.</returns>
        private static BigEndianBitWrapper CreateMaskLegacy(int byteWidth, int hostBits)
        {
            // Establish the all-ones ceiling for this width so that host bits can be cleared below.
            ulong allHi,
                allLo;
            if (byteWidth <= 8)
            {
                allHi = 0UL;
                allLo = byteWidth == 8 ? ulong.MaxValue : (1UL << (byteWidth * 8)) - 1;
            }
            else
            {
                allLo = ulong.MaxValue;
                allHi = byteWidth == 16 ? ulong.MaxValue : (1UL << ((byteWidth - 8) * 8)) - 1;
            }

            if (hostBits == 0)
            {
                return new BigEndianBitWrapper(allHi, allLo, byteWidth);
            }

            ulong maskHi,
                maskLo;

            if (byteWidth <= 8)
            {
                // All bits live in lo. For valid inputs (byteWidth ≤ 8, prefixLength > 0), hostBits < 64,
                // so the >= 64 guard is defensive; the shift-right-then-left clears the low hostBits.
                maskHi = 0UL;
                maskLo = hostBits >= 64 ? 0UL : (allLo >> hostBits) << hostBits;
            }
            else if (hostBits < 64)
            {
                // Host bits fall entirely within lo; the full hi word is the network prefix.
                maskHi = allHi;
                maskLo = (ulong.MaxValue >> hostBits) << hostBits; // clear the low hostBits of lo
            }
            else if (hostBits == 64)
            {
                // Entire lo word is host bits; hi carries the full network prefix.
                maskHi = allHi;
                maskLo = 0UL;
            }
            else
            {
                // Host bits spill into hi; clear hiHostBits low bits of hi and zero lo entirely.
                var hiHostBits = hostBits - 64;
                maskHi = hiHostBits >= 64 ? 0UL : (allHi >> hiHostBits) << hiHostBits;
                maskLo = 0UL;
            }

            return new BigEndianBitWrapper(maskHi, maskLo, byteWidth);
        }
#endif
    }
}
