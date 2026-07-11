using System.Diagnostics;
using System.Globalization;
using System.Numerics;
#if NET9_0_OR_GREATER
using System.Runtime.CompilerServices;
#endif
#if !NET8_0_OR_GREATER
using System.Text;
#endif

namespace Arcus
{
    /// <summary>
    ///     An internal unsigned integer with big-endian byte semantics, used for binary operations on
    ///     network addresses. <see cref="ByteWidth" /> (4 for IPv4, 16 for IPv6) is stored
    ///     alongside the value so that overflow/underflow detection and byte-array round-trips are always
    ///     bounded to the correct address-family width.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         On .NET 8+ the backing store is a <c>UInt128</c>; on earlier targets two
    ///         <see cref="ulong" /> fields (high / low) provide equivalent 128-bit arithmetic. All arithmetic
    ///         is bounded by <see cref="ByteWidth" />, not by the full 128-bit range.
    ///     </para>
    ///     <para>All arithmetic operations rely on unchecked context for carry/borrow detection.</para>
    ///     <para>
    ///         The non-generic <see cref="System.IComparable"/> is intentionally not implemented because
    ///         this type is <see langword="internal"/> and only used in strongly-typed comparison sites.
    ///     </para>
    /// </remarks>
#if NET9_0_OR_GREATER
    [SkipLocalsInit]
#endif
    [DebuggerDisplay("{DebuggerDisplay}")]
    internal readonly partial struct BigEndianBitWrapper
        : IComparable<BigEndianBitWrapper>,
            IEquatable<BigEndianBitWrapper>,
            IFormattable
    {
        /// <summary>Gets the byte width of this value (e.g. 4 for IPv4, 16 for IPv6).</summary>
        public readonly int ByteWidth;

#if NET8_0_OR_GREATER
        private readonly UInt128 _value; // unsigned 128-bit backing store; always bounded to [0, MaxValueForWidth]

        /// <summary>Initializes a new instance of the <see cref="BigEndianBitWrapper" /> struct.</summary>
        /// <param name="value">The unsigned 128-bit value, already bounded to the representable range for <paramref name="byteWidth" /> bytes.</param>
        /// <param name="byteWidth">Byte width of the address family; must be in [1, 16].</param>
        private BigEndianBitWrapper(UInt128 value, int byteWidth)
        {
            _value = value;
            ByteWidth = byteWidth;
        }

        /// <summary>Gets the maximum unsigned integer representable in <see cref="ByteWidth" /> bytes.</summary>
        /// <value>
        ///     <c>UInt128.MaxValue</c> when <see cref="ByteWidth" /> is 16; otherwise
        ///     <c>(1 &lt;&lt; (ByteWidth × 8)) - 1</c>. Serves as the arithmetic ceiling for overflow
        ///     detection and as the all-ones base for bitwise NOT and mask operations.
        /// </value>
        private UInt128 MaxValueForWidth
        {
            get
            {
                if (ByteWidth == 0)
                {
                    return UInt128.Zero;
                }

                return ByteWidth >= 16 ? UInt128.MaxValue : (UInt128.One << (ByteWidth * 8)) - 1;
            }
        }
#else
        private readonly ulong _hi; // most-significant 64 bits
        private readonly ulong _lo; // least-significant 64 bits

        /// <summary>Initializes a new instance of the <see cref="BigEndianBitWrapper" /> struct.</summary>
        /// <param name="hi">Most-significant 64 bits of the value; always <c>0</c> for byte widths 1-8.</param>
        /// <param name="lo">Least-significant 64 bits of the value.</param>
        /// <param name="byteWidth">Byte width of the address family; must be in [1, 16].</param>
        private BigEndianBitWrapper(ulong hi, ulong lo, int byteWidth)
        {
            _hi = hi;
            _lo = lo;
            ByteWidth = byteWidth;
        }

        /// <summary>Gets the maximum unsigned integer for <see cref="ByteWidth" /> bytes, split into high and low 64-bit components.</summary>
        /// <value>
        ///     A tuple where <c>Hi</c> is <c>0</c> for byte widths 1-8 (value fits entirely in <c>Lo</c>)
        ///     and carries the most-significant bits for widths 9-16. Serves as the arithmetic ceiling
        ///     for overflow detection and as the all-ones base for bitwise NOT and mask operations.
        /// </value>
        private (ulong Hi, ulong Lo) MaxHiLoForWidth
        {
            get
            {
                if (ByteWidth == 0)
                {
                    return (0UL, 0UL);
                }

                if (ByteWidth >= 16)
                {
                    return (ulong.MaxValue, ulong.MaxValue);
                }

                if (ByteWidth == 8)
                {
                    return (0UL, ulong.MaxValue);
                }

                // ByteWidth in [1,7]: value fits entirely in lo
                return (0UL, (1UL << (ByteWidth * 8)) - 1);
            }
        }
#endif

        #region Arithmetic

        /// <summary>
        ///     Attempts to add <paramref name="delta" /> to this value. Returns <see langword="false" /> if
        ///     the result would exceed the maximum value for <see cref="ByteWidth" /> (overflow) or fall
        ///     below zero (underflow). On failure <paramref name="result" /> is set to
        ///     <see langword="default" />.
        /// </summary>
        /// <param name="delta">Signed amount to add; may be negative for a decrement.</param>
        /// <param name="result">Receives the result on success.</param>
        /// <returns><see langword="true" /> if the addition succeeded within the byte-width bounds.</returns>
        public bool TryAdd(long delta, out BigEndianBitWrapper result)
        {
            if (delta == 0)
            {
                result = this;
                return true;
            }

#if NET8_0_OR_GREATER
            var maxValue = MaxValueForWidth;

            if (delta > 0)
            {
                // Route through ulong before widening to UInt128 to make unsigned intent explicit.
                var ud = (UInt128)(ulong)delta;
                // Compare against headroom (maxValue - _value) rather than checking _value + ud > maxValue
                // directly: _value can be UInt128.MaxValue when ByteWidth == 16, making the sum overflow.
                if (ud > maxValue - _value)
                {
                    result = default;
                    return false;
                }

                result = new BigEndianBitWrapper(_value + ud, ByteWidth);
            }
            else
            {
                // Avoid -long.MinValue overflow; its magnitude as ulong is 2^63.
                var magnitude = delta == long.MinValue ? 9223372036854775808UL : (ulong)(-delta);
                if ((UInt128)magnitude > _value)
                {
                    result = default;
                    return false;
                }

                result = new BigEndianBitWrapper(_value - magnitude, ByteWidth);
            }
#else
            var (maxHi, maxLo) = MaxHiLoForWidth;

            if (delta > 0)
            {
                var ud = (ulong)delta;
                // Compute headroom = maxValue - _value (safe because _value ≤ maxValue).
                // If remHi > 0, headroom ≥ 2^64 which exceeds any ulong addend, so overflow is
                // impossible; only check remLo when headroom fits entirely in 64 bits.
                var (remHi, remLo) = SubtractHiLo(maxHi, maxLo, _hi, _lo);
                if (remHi == 0 && ud > remLo)
                {
                    result = default;
                    return false;
                }

                var (newHi, newLo) = AddUlongToHiLo(_hi, _lo, ud);
                result = new BigEndianBitWrapper(newHi, newLo, ByteWidth);
            }
            else
            {
                var magnitude = delta == long.MinValue ? 9223372036854775808UL : (ulong)(-delta);
                // Underflow if magnitude > _value. If _hi > 0, _value ≥ 2^64 > magnitude.
                if (_hi == 0 && magnitude > _lo)
                {
                    result = default;
                    return false;
                }

                var (newHi, newLo) = SubtractHiLo(_hi, _lo, 0UL, magnitude);
                result = new BigEndianBitWrapper(newHi, newLo, ByteWidth);
            }
#endif
            return true;
        }

        /// <summary>
        ///     Subtracts <paramref name="other" /> from this value. The caller must ensure
        ///     <c>this &gt;= other</c>; a negative result throws.
        /// </summary>
        /// <param name="other">The value to subtract.</param>
        /// <returns>The non-negative difference, retaining <see cref="ByteWidth" />.</returns>
        /// <exception cref="InvalidOperationException">The subtraction would produce a negative result.</exception>
        public BigEndianBitWrapper Subtract(BigEndianBitWrapper other)
        {
            Debug.Assert(ByteWidth > 0, "ByteWidth must be greater than zero.");
            if (ByteWidth != other.ByteWidth)
            {
                throw new ArgumentException("Operands must have the same ByteWidth.");
            }
            if (CompareTo(other) < 0)
            {
                throw new InvalidOperationException("Subtraction would produce a negative result.");
            }

#if NET8_0_OR_GREATER
            return new BigEndianBitWrapper(_value - other._value, ByteWidth);
#else
            var (newHi, newLo) = SubtractHiLo(_hi, _lo, other._hi, other._lo);
            return new BigEndianBitWrapper(newHi, newLo, ByteWidth);
#endif
        }

#if !NET8_0_OR_GREATER
        /// <summary>Adds a 64-bit <paramref name="addend" /> to a 128-bit value represented as (<paramref name="hi" />, <paramref name="lo" />) fields.</summary>
        /// <param name="hi">Most-significant 64 bits of the operand.</param>
        /// <param name="lo">Least-significant 64 bits of the operand.</param>
        /// <param name="addend">The value to add to the low word; must not overflow the 128-bit total.</param>
        /// <returns>The (hi, lo) sum, with any carry from <paramref name="lo" /> propagated into hi.</returns>
        private static (ulong Hi, ulong Lo) AddUlongToHiLo(ulong hi, ulong lo, ulong addend)
        {
            var newLo = lo + addend;
            var carry = newLo < lo ? 1UL : 0UL; // Unsigned wrap-around in newLo means a carry occurred.
            return (hi + carry, newLo);
        }

        /// <summary>Subtracts a 128-bit value (<paramref name="bHi" />, <paramref name="bLo" />) from (<paramref name="aHi" />, <paramref name="aLo" />). The caller must guarantee a ≥ b.</summary>
        /// <param name="aHi">Most-significant 64 bits of the minuend.</param>
        /// <param name="aLo">Least-significant 64 bits of the minuend.</param>
        /// <param name="bHi">Most-significant 64 bits of the subtrahend.</param>
        /// <param name="bLo">Least-significant 64 bits of the subtrahend.</param>
        /// <returns>The (hi, lo) difference, with any borrow from <paramref name="aLo" /> propagated into hi.</returns>
        private static (ulong Hi, ulong Lo) SubtractHiLo(ulong aHi, ulong aLo, ulong bHi, ulong bLo)
        {
            var newLo = aLo - bLo;
            var borrow = newLo > aLo ? 1UL : 0UL; // Unsigned wrap-around in newLo means a borrow occurred.
            return (aHi - bHi - borrow, newLo);
        }
#endif

        #endregion // end: Arithmetic

        #region Conversion

        /// <summary>
        ///     Returns the value as a big-endian byte array of exactly <see cref="ByteWidth" /> bytes,
        ///     zero-padded on the most-significant side.
        /// </summary>
        /// <returns>A new byte array of length <see cref="ByteWidth" />.</returns>
        /// <remarks>
        ///     <para>
        ///         This overload allocates a fresh byte array. On .NET 8+ where the caller has a fixed-size
        ///         buffer, prefer <c>ToBytes(Span&lt;byte&gt;)</c> to avoid the allocation. Pre-.NET 8
        ///         callers must continue using this allocating overload.
        ///     </para>
        /// </remarks>
        public byte[] ToBytes()
        {
            var result = new byte[ByteWidth];
#if NET8_0_OR_GREATER
            ToBytes(result.AsSpan());
#else
            ToBytesCore(result);
#endif
            return result;
        }

#if NET8_0_OR_GREATER
        /// <summary>
        ///     Writes the value as a big-endian sequence of exactly <see cref="ByteWidth" /> bytes into
        ///     <paramref name="destination" />, zero-padded on the most-significant side.
        ///     Prefer this overload over <see cref="ToBytes()" /> when the caller can supply a
        ///     <c>stackalloc</c> buffer to avoid a heap allocation.
        /// </summary>
        /// <param name="destination">A span of at least <see cref="ByteWidth" /> bytes to write into.</param>
        public void ToBytes(Span<byte> destination)
        {
            if (destination.Length < ByteWidth)
            {
                throw new ArgumentException("Destination span is too short.", nameof(destination));
            }
            var v = _value;
            for (var i = ByteWidth - 1; i >= 0; i--)
            {
                destination[i] = (byte)(v & 0xFF);
                v >>= 8;
            }
        }
#else
        /// <summary>
        ///     Writes the value as a big-endian sequence of exactly <see cref="ByteWidth" /> bytes into
        ///     <paramref name="result" />, zero-padding the most-significant side as needed.
        ///     This is the pre-.NET 8 equivalent of <see cref="ToBytes()" />, operating on the two-field
        ///     <c>(_hi, _lo)</c> representation. All argument validation is the caller's responsibility.
        /// </summary>
        /// <param name="result">Pre-allocated byte array of length <see cref="ByteWidth" /> to write into.</param>
        private void ToBytesCore(byte[] result)
        {
            if (ByteWidth <= 8)
            {
                var lo = _lo;
                for (var i = ByteWidth - 1; i >= 0; i--)
                {
                    result[i] = (byte)(lo & 0xFF);
                    lo >>= 8;
                }
            }
            else
            {
                // Lower 8 bytes come from lo, upper (ByteWidth - 8) bytes come from hi.
                var lo = _lo;
                for (var i = ByteWidth - 1; i >= ByteWidth - 8; i--)
                {
                    result[i] = (byte)(lo & 0xFF);
                    lo >>= 8;
                }

                var hi = _hi;
                var hiByteCount = ByteWidth - 8;
                for (var i = hiByteCount - 1; i >= 0; i--)
                {
                    result[i] = (byte)(hi & 0xFF);
                    hi >>= 8;
                }
            }
        }
#endif

        /// <summary>
        ///     Converts the value to a non-negative <see cref="BigInteger" /> using the unsigned
        ///     big-endian interpretation.
        /// </summary>
        /// <returns>A non-negative <see cref="BigInteger" /> equal to the unsigned integer value.</returns>
        public BigInteger ToBigInteger()
        {
#if NET8_0_OR_GREATER
            Span<byte> bytes = stackalloc byte[ByteWidth];
            ToBytes(bytes);
            return new BigInteger(bytes, isUnsigned: true, isBigEndian: true);
#else
            var bytes = ToBytes();
            // BigInteger(byte[]) expects little-endian with an optional sign byte.
            // Allocate one extra byte (le[bytes.Length] stays 0) so the leading zero
            // forces an unsigned (positive) interpretation regardless of the MSB.
            var le = new byte[bytes.Length + 1];
            for (var i = 0; i < bytes.Length; i++)
            {
                le[i] = bytes[bytes.Length - 1 - i]; // reverse byte order: big-endian → little-endian
            }

            return new BigInteger(le);
#endif
        }

        #endregion // end: Conversion

        #region Formatting

        /// <summary>
        ///     Returns an uppercase hex string with no separators or prefix.
        ///     Length is always <c>ByteWidth × 2</c> characters.
        /// </summary>
        /// <returns>Example: 192.168.1.1 → "C0A80101".</returns>
        public string ToHexString()
        {
#if NET8_0_OR_GREATER
            Span<byte> bytes = stackalloc byte[ByteWidth];
            ToBytes(bytes);
            return Convert.ToHexString(bytes);
#else
            var bytes = ToBytes();
            var sb = new StringBuilder(ByteWidth * 2);
            foreach (var byteValue in bytes)
            {
                sb.Append(byteValue.ToString("X2", CultureInfo.InvariantCulture));
            }

            return sb.ToString();
#endif
        }

        /// <summary>
        ///     Returns the decimal string of the unsigned big-endian integer value.
        /// </summary>
        /// <returns>Example: 192.168.1.1 → "3232235777".</returns>
        public string ToDecimalString()
        {
            return ToBigInteger().ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        ///     Returns a binary string of exactly <c>ByteWidth × 8</c> characters, MSB first.
        /// </summary>
        /// <returns>Example: 0x0F (1 byte) → "00001111".</returns>
        public string ToBinaryString()
        {
#if NET8_0_OR_GREATER
            Span<byte> bytes = stackalloc byte[ByteWidth];
            ToBytes(bytes);
            Span<char> chars = stackalloc char[ByteWidth * 8];
            var pos = 0;
            foreach (var byteValue in bytes)
            {
                for (var bit = 7; bit >= 0; bit--)
                {
                    // Shift the target bit to position 0, mask to isolate it, then map 0→'0' and 1→'1'.
                    chars[pos++] = (char)('0' + ((byteValue >> bit) & 1));
                }
            }

            return new string(chars);
#else
            var bytes = ToBytes();
            var sb = new StringBuilder(ByteWidth * 8);
            foreach (var byteValue in bytes)
            {
                for (var bit = 7; bit >= 0; bit--)
                {
                    // Shift the target bit to position 0, mask to isolate it; Append(int) renders 0 or 1.
                    sb.Append((byteValue >> bit) & 1);
                }
            }

            return sb.ToString();
#endif
        }

        /// <summary>Returns the hex compact representation (same as <see cref="ToHexString" />).</summary>
        /// <returns>An uppercase hex string of length <c>ByteWidth × 2</c>.</returns>
        public override string ToString()
        {
            return ToHexString();
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay
        {
            get
            {
                var bytes = ToBytes();
                var hex = string.Join(
                    "_",
                    Array.ConvertAll(bytes, byteValue => byteValue.ToString("X2", CultureInfo.InvariantCulture))
                );
                return $"0x{hex} ({ByteWidth} bytes)";
            }
        }

        #endregion // end: Formatting
    }
}
