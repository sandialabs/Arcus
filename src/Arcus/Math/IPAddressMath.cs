using System;
using System.Linq;
using System.Net;
using Arcus.Utilities;
using Gulliver;
using JetBrains.Annotations;
using static System.Net.Sockets.AddressFamily;

namespace Arcus.Math
{
    /// <summary>
    ///     Static utility class containing mathematical methods on <see cref="IPAddress" /> objects
    /// </summary>
    public static class IPAddressMath
    {
        #region basic arithmetic operations

        /// <summary>
        ///     Increment IPv4 or IPv6 value
        /// </summary>
        /// <param name="input">the ip address to affect</param>
        /// <param name="delta">the increment value, may be negative</param>
        /// <returns>the incremented ip address</returns>
        /// <exception cref="InvalidOperationException">could not increment input</exception>
        /// <exception cref="InvalidOperationException">Increment caused address underflow</exception>
        /// <exception cref="InvalidOperationException">Increment caused address overflow</exception>
        /// <exception cref="ArgumentNullException"><paramref name="input" /> is <see langword="null" />.</exception>
        [NotNull]
        public static IPAddress Increment([NotNull] this IPAddress input,
                                          long delta = 1)
        {
            #region defense

            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            if (!IPAddressUtilities.ValidAddressFamilies.Contains(input.AddressFamily))
            {
                throw new ArgumentException($"must have an address family equal to {string.Join(", ", IPAddressUtilities.ValidAddressFamilies)}", nameof(input));
            }

            #endregion // end: defense

            if (delta == 0)
            {
                return input;
            }

            if (!ByteArrayUtils.TrySumBigEndian(input.GetAddressBytes(), delta, out var byteResult))
            {
                throw new InvalidOperationException("could not increment address");
            }

            var addressByteWidth = input.IsIPv4()
                                       ? IPAddressUtilities.IPv4ByteCount
                                       : IPAddressUtilities.IPv6ByteCount;

            if (byteResult.Length > addressByteWidth)
            {
                throw new InvalidOperationException("increment would overflow maximum size of ip address");
            }

            var paddedBytes = byteResult.PadBigEndianMostSignificantBytes(addressByteWidth);
            return new IPAddress(paddedBytes);
        }

        /// <summary>
        ///     Try to increment the given address
        /// </summary>
        /// <param name="input">the <see cref="IPAddress"/> to increment</param>
        /// <param name="address">the resulting <see cref="IPAddress"/> post increment</param>
        /// <param name="delta">the amount to increment by</param>
        public static bool TryIncrement([CanBeNull] IPAddress input,
                                        [CanBeNull] out IPAddress address,
                                        long delta = 1)
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
#pragma warning disable CA1031 // catch is purposely general
            catch
#pragma warning restore CA1031
            {
                address = null;
                return false;
            }
        }

        #endregion

        #region comparisons

        /// <summary>
        ///     Is Equal
        /// </summary>
        /// <param name="left">first operand</param>
        /// <param name="right">second operand</param>
        public static bool IsEqualTo([CanBeNull] this IPAddress left,
                                     [CanBeNull] IPAddress right)
        {
            return ReferenceEquals(left, right)
                   || (!ReferenceEquals(left, null)
                   && left.Equals(right));
        }

        /// <summary>
        ///     Greater Than
        /// </summary>
        /// <param name="left">first operand</param>
        /// <param name="right">second operand</param>
        public static bool IsGreaterThan([CanBeNull] this IPAddress left,
                                         [CanBeNull] IPAddress right)
        {
            if (ReferenceEquals(left, right)
                || (!ReferenceEquals(left, null)
                && left.Equals(right))
                || left == null
                || right == null
                || left.AddressFamily != right.AddressFamily)
            {
                return false;
            }

            return ByteArrayUtils.CompareUnsignedBigEndian(left.GetAddressBytes(), right.GetAddressBytes()) > 0;
        }

        /// <summary>
        ///     Greater Than or equal
        /// </summary>
        /// <param name="left">first operand</param>
        /// <param name="right">second operand</param>
        public static bool IsGreaterThanOrEqualTo([CanBeNull] this IPAddress left,
                                                  [CanBeNull] IPAddress right)
        {
            return ReferenceEquals(left, right)
                   || (!ReferenceEquals(left, null) && left.Equals(right))
                   || (left?.AddressFamily == right?.AddressFamily && ByteArrayUtils.CompareUnsignedBigEndian(left?.GetAddressBytes(), right?.GetAddressBytes()) >= 0);
        }

        /// <summary>
        ///     Less Than
        /// </summary>
        /// <param name="left">first operand</param>
        /// <param name="right">second operand</param>
        /// <returns>true if alpha is less than beta</returns>
        /// <exception cref="InvalidOperationException">Address families must be InterNetwork or InternetworkV6</exception>
        public static bool IsLessThan([CanBeNull] this IPAddress left,
                                      [CanBeNull] IPAddress right)
        {
            if (ReferenceEquals(left, right)
                || left == null
                || right == null
                || left.AddressFamily != right.AddressFamily)
            {
                return false;
            }

            return ByteArrayUtils.CompareUnsignedBigEndian(left.GetAddressBytes(), right.GetAddressBytes()) < 0;
        }

        /// <summary>
        ///     Less Than or equal
        /// </summary>
        /// <param name="left">first operand</param>
        /// <param name="right">second operand</param>
        public static bool IsLessThanOrEqualTo([CanBeNull] this IPAddress left,
                                               [CanBeNull] IPAddress right)
        {
            return ReferenceEquals(left, right)
                   || (!ReferenceEquals(left, null) && left.Equals(right))
                   || (left?.AddressFamily == right?.AddressFamily && ByteArrayUtils.CompareUnsignedBigEndian(left?.GetAddressBytes(), right?.GetAddressBytes()) <= 0);
        }

        /// <summary>
        ///     Determine if the tested IP Address occurs numerically between the given high and low IP addresses
        ///     Inclusivity contingent on inclusive bit
        /// </summary>
        /// <param name="input">IP address to test</param>
        /// <param name="low">low value</param>
        /// <param name="high">high value</param>
        /// <param name="inclusive">true if bounds are inclusive (defaults to true)</param>
        /// <exception cref="ArgumentNullException"><paramref name="input" /> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="low" /> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="high" /> is <see langword="null" />.</exception>
        public static bool IsBetween([NotNull] this IPAddress input,
                                     [NotNull] IPAddress low,
                                     [NotNull] IPAddress high,
                                     bool inclusive = true)
        {
            #region defense

            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            if (low == null)
            {
                throw new ArgumentNullException(nameof(low));
            }

            if (high == null)
            {
                throw new ArgumentNullException(nameof(high));
            }

            if (low.AddressFamily != high.AddressFamily
                || input.AddressFamily != low.AddressFamily)
            {
                throw new InvalidOperationException("address families do not match");
            }

            var lowAddressBytes = low.GetAddressBytes();
            var highAddressBytes = high.GetAddressBytes();
            if (ByteArrayUtils.CompareUnsignedBigEndian(lowAddressBytes, highAddressBytes) > 0)
            {
                throw new InvalidOperationException($"{nameof(low)} must not be greater than {nameof(high)}");
            }

            #endregion // end: defense

            if (ReferenceEquals(input, low)
                || input.Equals(low)
                || ReferenceEquals(input, high)
                || input.Equals(high))
            {
                return inclusive;
            }

            var inputBytes = input.GetAddressBytes();
            return ByteArrayUtils.CompareUnsignedBigEndian(inputBytes, lowAddressBytes) > 0
                   && ByteArrayUtils.CompareUnsignedBigEndian(inputBytes, highAddressBytes) < 0;
        }

        /// <summary>
        ///     Get Maximum <see cref="IPAddress" /> (based on bytes)
        /// </summary>
        /// <param name="left">first operand</param>
        /// <param name="right">second operand</param>
        /// <returns>the largest of the two operands</returns>
        /// <exception cref="ArgumentNullException"><paramref name="left" /> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="right" /> is <see langword="null" />.</exception>
        /// <exception cref="InvalidOperationException">Address families must match</exception>
        [NotNull]
        public static IPAddress Max([NotNull] IPAddress left,
                                    [NotNull] IPAddress right)
        {
            #region defense

            if (left == null)
            {
                throw new ArgumentNullException(nameof(left));
            }

            if (right == null)
            {
                throw new ArgumentNullException(nameof(right));
            }

            if (right.AddressFamily != left.AddressFamily)
            {
                throw new InvalidOperationException("Address families must match");
            }

            #endregion // end: defense

            return left.IsGreaterThan(right)
                       ? left
                       : right;
        }

        /// <summary>
        ///     Get Minimum IPAddress (based on bytes)
        /// </summary>
        /// <param name="left">first operand</param>
        /// <param name="right">second operand</param>
        /// <returns>the smallest of the two operands</returns>
        /// <exception cref="ArgumentNullException"><paramref name="left" /> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="right" /> is <see langword="null" />.</exception>
        /// <exception cref="InvalidOperationException">Address families must match</exception>
        [NotNull]
        public static IPAddress Min([NotNull] IPAddress left,
                                    [NotNull] IPAddress right)
        {
            #region defense

            if (left == null)
            {
                throw new ArgumentNullException(nameof(left));
            }

            if (right == null)
            {
                throw new ArgumentNullException(nameof(right));
            }

            if (right.AddressFamily != left.AddressFamily)
            {
                throw new InvalidOperationException("Address families must match");
            }

            #endregion // end: defense

            return left.IsLessThan(right)
                       ? left
                       : right;
        }

        #endregion

        #region limit deduction

        /// <summary>
        ///     determine if IP address is at maximum value
        /// </summary>
        /// <param name="address">the IP Address to test</param>
        /// <returns>true if the address is the maximum value</returns>
        /// <exception cref="InvalidOperationException">Address families must be InterNetwork or InternetworkV6</exception>
        /// <exception cref="ArgumentNullException"><paramref name="address" /> is <see langword="null" />.</exception>
        public static bool IsAtMax([NotNull] this IPAddress address)
        {
            #region defense

            if (address == null)
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
        ///     determine if IP address is at minimum value
        /// </summary>
        /// <param name="address">the IP Address to test</param>
        /// <returns>true if the address is the minimum value</returns>
        /// <exception cref="InvalidOperationException">Address families must be InterNetwork or InternetworkV6</exception>
        /// <exception cref="ArgumentNullException"><paramref name="address" /> is <see langword="null" />.</exception>
        public static bool IsAtMin([NotNull] this IPAddress address)
        {
            #region defense

            if (address == null)
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
