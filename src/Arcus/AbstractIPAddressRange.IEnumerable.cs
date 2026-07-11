using System.Collections;
using System.Net;
using System.Numerics;

namespace Arcus
{
    /// <content>
    ///     <see cref="AbstractIPAddressRange"/> implementation of <see cref="IEnumerable{IPAddress}"/>
    /// </content>
    public abstract partial class AbstractIPAddressRange
    {
        private BigInteger? _maxCount;

        #region IEnumerable / IEnumerable<IPAddress>

        /// <inheritdoc />
        public IEnumerable<IPAddress> ToIPAddresses()
        {
            _maxCount ??= BigInteger.One << this.MaxEnumerationExponent;
            return EnumerateCore(_maxCount.Value);
        }

/// <inheritdoc />
        /// <remarks>
        ///     <para>
        ///         Marked <see cref="ObsoleteAttribute"/> to steer callers toward
        ///         <see cref="ToIPAddresses()"/>, which is non-breaking and respects
        ///         <see cref="AbstractIPAddressRange.MaxEnumerationExponent"/>. Direct
        ///         <c>foreach</c> on range types remains supported via this method for
        ///         back-compat but is discouraged for large ranges.
        ///     </para>
        /// </remarks>
        #pragma warning disable S1133 // Do not forget to remove this deprecated code someday
        [Obsolete("Use ToIPAddresses() instead")]
        #pragma warning restore S1133
        public IEnumerator<IPAddress> GetEnumerator()
        {
            return ToIPAddresses().GetEnumerator();
        }

        /// <inheritdoc />
#pragma warning disable CS0618 // Type or member is obsolete
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
#pragma warning restore CS0618

        private IEnumerable<IPAddress> EnumerateCore(BigInteger maxCount)
        {
            var limitWrap = BigEndianBitWrapper.FromBytes(this.Tail.GetAddressBytes());

            var current = BigEndianBitWrapper.FromBytes(this.Head.GetAddressBytes());

#if NET8_0_OR_GREATER
            var buffer = new byte[current.ByteWidth];
#endif

            BigInteger? bigCount = null;
            long longCount = 0;
            var useLong = maxCount <= long.MaxValue;

            while (current.CompareTo(limitWrap) <= 0)
            {
                if (useLong ? longCount >= (long)maxCount : bigCount >= maxCount)
                {
                    throw new InvalidOperationException(
                        $"Enumeration limit of {maxCount} addresses (2^{this.MaxEnumerationExponent}) reached. "
                            + $"The range contains {this.Length} addresses, which exceeds this limit. "
                            + "Construct with a larger maxEnumerationExponent (0-128) to enumerate more."
                    );
                }

#if NET8_0_OR_GREATER
                current.ToBytes(buffer);
                yield return new IPAddress(buffer);
#else
                yield return new IPAddress(current.ToBytes());
#endif

                if (!current.TryAdd(1, out var next))
                {
                    break;
                }

                current = next;
                if (useLong)
                {
                    longCount++;
                }
                else
                {
                    bigCount = (bigCount ?? BigInteger.Zero) + 1;
                }
            }
        }

        #endregion // end: IEnumerable
    }
}
