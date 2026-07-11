using System.Net;
using System.Numerics;
using Arcus.Utilities;
using static System.Net.Sockets.AddressFamily;
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

namespace Arcus.Math
{
    /// <summary>
    ///     Static utility class containing mathematical methods on <see cref="IPAddress" /> objects
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         IPv4 address arithmetic operates within the 32-bit unsigned address space per
    ///         <see href="https://www.rfc-editor.org/rfc/rfc791#section-2.3">RFC 791 §2.3</see>.
    ///         IPv6 address arithmetic operates within the 128-bit unsigned address space per
    ///         <see href="https://www.rfc-editor.org/rfc/rfc4291#section-2.1">RFC 4291 §2.1</see>.
    ///         Overflow or underflow beyond the respective address space boundaries throws
    ///         <see cref="InvalidOperationException"/>.
    ///     </para>
    /// </remarks>
    public static class IPAddressMath
    {
        #region basic arithmetic operations

        /// <summary>
        ///     Increments or decrements an IPv4 or IPv6 address by a delta.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         For IPv4, overflow beyond <c>255.255.255.255</c> (upper bound per
        ///         <see href="https://www.rfc-editor.org/rfc/rfc791#section-2.3">RFC 791 §2.3</see>) throws
        ///         <see cref="InvalidOperationException"/>.
        ///         For IPv6, overflow beyond <c>ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff</c> (upper bound per
        ///         <see href="https://www.rfc-editor.org/rfc/rfc4291#section-2.1">RFC 4291 §2.1</see>) throws
        ///         <see cref="InvalidOperationException"/>.
        ///     </para>
        /// </remarks>
        /// <param name="input">The IP address to adjust.</param>
        /// <param name="delta">The signed increment value; negative for decrement.</param>
        /// <returns>The adjusted IP address.</returns>
        /// <remarks>
        ///     <para>
        ///         <paramref name="delta"/> is a signed 64-bit integer (<see cref="long"/>).
        ///         For IPv6 ranges exceeding <c>2^63 - 1</c>, callers must issue multiple
        ///         increment calls; there is no <see cref="BigInteger"/>-delta overload.
        ///     </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">Increment would overflow or underflow the address space.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="input" /> is <see langword="null" />.</exception>
        public static IPAddress Increment(this IPAddress input, long delta = 1)
        {
            #region defense

            if (input is null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            if (!IPAddressUtilities.ValidAddressFamilies.Contains(input.AddressFamily))
            {
                throw new ArgumentException(
                    $"must have an address family equal to {string.Join(", ", IPAddressUtilities.ValidAddressFamilies)}",
                    nameof(input)
                );
            }

            #endregion // end: defense

            if (delta == 0)
            {
                return input;
            }

            var wrapper = BigEndianBitWrapper.FromBytes(input.GetAddressBytes());
            if (!wrapper.TryAdd(delta, out var result))
            {
                throw new InvalidOperationException(
                    delta > 0 ? "increment would overflow maximum size of ip address" : "could not increment address"
                );
            }

            return new IPAddress(result.ToBytes());
        }

        /// <summary>
        ///     Attempts to increment an IPv4 or IPv6 address, returning <see langword="false" /> instead of throwing on overflow or underflow.
        /// </summary>
        /// <param name="input">The IP address to adjust.</param>
        /// <param name="address">The resulting <see cref="IPAddress"/>, or <see langword="null" /> on failure.</param>
        /// <param name="delta">The signed increment value; negative for decrement.</param>
        /// <returns><see langword="true" /> if the increment succeeded.</returns>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
        public static bool TryIncrement(IPAddress input, [NotNullWhen(true)] out IPAddress address, long delta = 1)
#else
        public static bool TryIncrement(IPAddress input, out IPAddress address, long delta = 1)
#endif
        {
            if (input == null)
            {
                address = null;
                return false;
            }

            try
            {
                address = input.Increment(delta);
                return true;
            }
            catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
            {
                address = null;
                return false;
            }
        }

        #endregion

        #region comparisons

        /// <summary>
        ///     Determines whether two <see cref="IPAddress"/> instances have equal numeric values.
        /// </summary>
        /// <param name="left">The first address to compare.</param>
        /// <param name="right">The second address to compare.</param>
        /// <returns><see langword="true" /> if the addresses are equal.</returns>
        public static bool IsEqualTo(this IPAddress left, IPAddress right)
        {
            return ReferenceEquals(left, right) || (left?.Equals(right) == true);
        }

        /// <summary>
        ///     Determines whether one <see cref="IPAddress"/> is numerically greater than another.
        /// </summary>
        /// <param name="left">The first address to compare.</param>
        /// <param name="right">The second address to compare.</param>
        /// <returns><see langword="true" /> if <paramref name="left"/> is greater than <paramref name="right"/>.</returns>
        public static bool IsGreaterThan(this IPAddress left, IPAddress right)
        {
            if (
                ReferenceEquals(left, right)
                || (left?.Equals(right) == true)
                || left == null
                || right == null
                || left.AddressFamily != right.AddressFamily
            )
            {
                return false;
            }

            return BigEndianBitWrapper
                    .FromBytes(left.GetAddressBytes())
                    .CompareTo(BigEndianBitWrapper.FromBytes(right.GetAddressBytes())) > 0;
        }

        /// <summary>
        ///     Determines whether one <see cref="IPAddress"/> is numerically greater than or equal to another.
        /// </summary>
        /// <param name="left">The first address to compare.</param>
        /// <param name="right">The second address to compare.</param>
        /// <returns><see langword="true" /> if <paramref name="left"/> is greater than or equal to <paramref name="right"/>.</returns>
        public static bool IsGreaterThanOrEqualTo(this IPAddress left, IPAddress right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (left == null || right == null || left.AddressFamily != right.AddressFamily)
            {
                return false;
            }

            if (left.Equals(right))
            {
                return true;
            }

            return BigEndianBitWrapper
                    .FromBytes(left.GetAddressBytes())
                    .CompareTo(BigEndianBitWrapper.FromBytes(right.GetAddressBytes())) >= 0;
        }

        /// <summary>
        ///     Determines whether one <see cref="IPAddress"/> is numerically less than another.
        /// </summary>
        /// <param name="left">The first address to compare.</param>
        /// <param name="right">The second address to compare.</param>
        /// <returns><see langword="true" /> if <paramref name="left"/> is less than <paramref name="right"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="left"/> or <paramref name="right"/> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">Address families must match.</exception>
        public static bool IsLessThan(this IPAddress left, IPAddress right)
        {
            if (
                ReferenceEquals(left, right)
                || (left?.Equals(right) == true)
                || left == null
                || right == null
                || left.AddressFamily != right.AddressFamily
            )
            {
                return false;
            }

            return BigEndianBitWrapper
                    .FromBytes(left.GetAddressBytes())
                    .CompareTo(BigEndianBitWrapper.FromBytes(right.GetAddressBytes())) < 0;
        }

        /// <summary>
        ///     Determines whether one <see cref="IPAddress"/> is numerically less than or equal to another.
        /// </summary>
        /// <param name="left">The first address to compare.</param>
        /// <param name="right">The second address to compare.</param>
        /// <returns><see langword="true" /> if <paramref name="left"/> is less than or equal to <paramref name="right"/>.</returns>
        public static bool IsLessThanOrEqualTo(this IPAddress left, IPAddress right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (left == null || right == null || left.AddressFamily != right.AddressFamily)
            {
                return false;
            }

            if (left.Equals(right))
            {
                return true;
            }

            return BigEndianBitWrapper
                    .FromBytes(left.GetAddressBytes())
                    .CompareTo(BigEndianBitWrapper.FromBytes(right.GetAddressBytes())) <= 0;
        }

        /// <summary>
        ///     Determines whether an address falls within a numeric range, inclusive by default.
        /// </summary>
        /// <param name="input">The address to test.</param>
        /// <param name="low">The lower bound of the range.</param>
        /// <param name="high">The upper bound of the range.</param>
        /// <param name="inclusive"><see langword="true" /> to include the bounds (default).</param>
        /// <returns><see langword="true" /> if <paramref name="input"/> is between <paramref name="low"/> and <paramref name="high"/>.</returns>
        /// <exception cref="ArgumentNullException">Any argument is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">Address families must match.</exception>
        /// <exception cref="InvalidOperationException"><paramref name="low"/> is greater than <paramref name="high"/>.</exception>
        public static bool IsBetween(this IPAddress input, IPAddress low, IPAddress high, bool inclusive = true)
        {
            #region defense

            if (input is null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            if (low is null)
            {
                throw new ArgumentNullException(nameof(low));
            }

            if (high is null)
            {
                throw new ArgumentNullException(nameof(high));
            }

            if (low.AddressFamily != high.AddressFamily || input.AddressFamily != low.AddressFamily)
            {
                throw new ArgumentException("address families do not match");
            }

            var lowAddressBytes = low.GetAddressBytes();
            var highAddressBytes = high.GetAddressBytes();
            var lowWrap = BigEndianBitWrapper.FromBytes(lowAddressBytes);
            var highWrap = BigEndianBitWrapper.FromBytes(highAddressBytes);
            if (lowWrap.CompareTo(highWrap) > 0)
            {
                throw new InvalidOperationException($"{nameof(low)} must not be greater than {nameof(high)}");
            }

            #endregion // end: defense

            if (ReferenceEquals(input, low) || input.Equals(low) || ReferenceEquals(input, high) || input.Equals(high))
            {
                return inclusive;
            }

            var inputWrap = BigEndianBitWrapper.FromBytes(input.GetAddressBytes());
            return inputWrap.CompareTo(lowWrap) > 0 && inputWrap.CompareTo(highWrap) < 0;
        }

        /// <summary>
        ///     Returns the numerically greater of two <see cref="IPAddress"/> values.
        /// </summary>
        /// <param name="left">The first address.</param>
        /// <param name="right">The second address.</param>
        /// <returns>The larger of the two addresses.</returns>
        /// <exception cref="ArgumentNullException">Either argument is <see langword="null" />.</exception>
        /// <exception cref="InvalidOperationException">Address families must match.</exception>
        public static IPAddress Max(IPAddress left, IPAddress right)
        {
            #region defense

            if (left is null)
            {
                throw new ArgumentNullException(nameof(left));
            }

            if (right is null)
            {
                throw new ArgumentNullException(nameof(right));
            }

            if (right.AddressFamily != left.AddressFamily)
            {
                throw new InvalidOperationException("Address families must match");
            }

            #endregion // end: defense

            return left.IsGreaterThan(right) ? left : right;
        }

        /// <summary>
        ///     Returns the numerically lesser of two <see cref="IPAddress"/> values.
        /// </summary>
        /// <param name="left">The first address.</param>
        /// <param name="right">The second address.</param>
        /// <returns>The smaller of the two addresses.</returns>
        /// <exception cref="ArgumentNullException">Either argument is <see langword="null" />.</exception>
        /// <exception cref="InvalidOperationException">Address families must match.</exception>
        public static IPAddress Min(IPAddress left, IPAddress right)
        {
            #region defense

            if (left is null)
            {
                throw new ArgumentNullException(nameof(left));
            }

            if (right is null)
            {
                throw new ArgumentNullException(nameof(right));
            }

            if (right.AddressFamily != left.AddressFamily)
            {
                throw new InvalidOperationException("Address families must match");
            }

            #endregion // end: defense

            return left.IsLessThan(right) ? left : right;
        }

        #endregion

        #region limit deduction

        /// <summary>
        ///     Determines whether an address is at the maximum value for its address family.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         IPv4 maximum is <c>255.255.255.255</c> per
        ///         <see href="https://www.rfc-editor.org/rfc/rfc791#section-2.3">RFC 791 §2.3</see>.
        ///         IPv6 maximum is <c>ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff</c> per
        ///         <see href="https://www.rfc-editor.org/rfc/rfc4291#section-2.1">RFC 4291 §2.1</see>.
        ///     </para>
        /// </remarks>
        /// <param name="address">The address to test.</param>
        /// <returns><see langword="true" /> if the address is the maximum value for its family.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="address" /> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Unsupported address family.</exception>
        public static bool IsAtMax(this IPAddress address)
        {
            #region defense

            if (address is null)
            {
                throw new ArgumentNullException(nameof(address));
            }

            #endregion // end: defense

            switch (address.AddressFamily)
            {
                case InterNetwork:
                    return address.Equals(IPAddressUtilities.IPv4MaxAddress);
                case InterNetworkV6:
                    return address.Equals(IPAddressUtilities.IPv6MaxAddress);
                default:
                    throw new ArgumentOutOfRangeException(nameof(address), address.AddressFamily, "unexpected address family");
            }
        }

        /// <summary>
        ///     Determines whether an address is at the minimum value for its address family.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         IPv4 minimum is <c>0.0.0.0</c> per
        ///         <see href="https://www.rfc-editor.org/rfc/rfc791#section-2.3">RFC 791 §2.3</see>.
        ///         IPv6 minimum is <c>::</c> per
        ///         <see href="https://www.rfc-editor.org/rfc/rfc4291#section-2.1">RFC 4291 §2.1</see>.
        ///     </para>
        /// </remarks>
        /// <param name="address">The address to test.</param>
        /// <returns><see langword="true" /> if the address is the minimum value for its family.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="address" /> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Unsupported address family.</exception>
        public static bool IsAtMin(this IPAddress address)
        {
            #region defense

            if (address is null)
            {
                throw new ArgumentNullException(nameof(address));
            }

            #endregion // end: defense

            switch (address.AddressFamily)
            {
                case InterNetwork:
                    return address.Equals(IPAddressUtilities.IPv4MinAddress);
                case InterNetworkV6:
                    return address.Equals(IPAddressUtilities.IPv6MinAddress);
                default:
                    throw new ArgumentOutOfRangeException(nameof(address), address.AddressFamily, "unexpected address family");
            }
        }

        #endregion
    }
}
