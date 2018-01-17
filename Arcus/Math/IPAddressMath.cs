using System;
using System.Net;
using System.Net.Sockets;
using Arcus.Converters;
using Arcus.Utilities;
using JetBrains.Annotations;

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
        /// <param name="ipAddress">the ip address to affect</param>
        /// <param name="delta">the increment value, may be negative</param>
        /// <returns>the incremented ip address</returns>
        /// <exception cref="InvalidOperationException">could not increment input</exception>
        /// <exception cref="InvalidOperationException">Increment caused address underflow</exception>
        /// <exception cref="InvalidOperationException">Increment caused address overflow</exception>
        /// <exception cref="InvalidOperationException"><paramref name="ipAddress" /> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="ipAddress" /> is <see langword="null" />.</exception>
        [NotNull]
        public static IPAddress Increment([NotNull] this IPAddress ipAddress,
                                          long delta = 1)
        {
            if (ipAddress == null)
            {
                throw new ArgumentNullException(nameof(ipAddress));
            }

            if (delta == 0)
            {
                return ipAddress;
            }

            var addressAsBigInt = ipAddress.ToUnsignedBigInteger();
            var incrementAddressAsBigInt = addressAsBigInt + delta;

            // underflow prevention
            if (incrementAddressAsBigInt < 0)
            {
                throw new InvalidOperationException("Increment caused address underflow");
            }

            // overflow prevention
            if ((ipAddress.AddressFamily == AddressFamily.InterNetwork && incrementAddressAsBigInt > IPAddressUtilities.IPv4MaxAddress.ToUnsignedBigInteger())
                || (ipAddress.AddressFamily == AddressFamily.InterNetworkV6 && incrementAddressAsBigInt > IPAddressUtilities.IPv6MaxAddress.ToUnsignedBigInteger()))
            {
                throw new InvalidOperationException("Increment caused address overflow");
            }

            IPAddress newAddress;
            if (IPAddressUtilities.TryParse(incrementAddressAsBigInt, ipAddress.AddressFamily, out newAddress))
            {
                return newAddress;
            }

            throw new InvalidOperationException("could not increment input");
        }

        #endregion

        #region comparisons

        /// <summary>
        ///     Equal to
        /// </summary>
        /// <param name="alpha">first operand</param>
        /// <param name="beta">sencond operand</param>
        /// <returns>true if values are equal</returns>
        /// <exception cref="InvalidOperationException">Address families must be InterNetwork or InternetworkV6</exception>
        public static bool IsEqualTo([CanBeNull] this IPAddress alpha,
                                     [CanBeNull] IPAddress beta)
        {
            if (alpha == null
                && beta == null)
            {
                return true;
            }

            if (alpha == null
                || beta == null
                || beta.AddressFamily != alpha.AddressFamily)
            {
                return false;
            }

            if (!beta.IsIPv4()
                && !beta.IsIPv6())
            {
                throw new InvalidOperationException("Address families must be InterNetwork or InternetworkV6");
            }

            return alpha.Equals(beta)
                   || alpha.ToUnsignedBigInteger() == beta.ToUnsignedBigInteger();
        }

        /// <summary>
        ///     Greater Than
        /// </summary>
        /// <param name="alpha">first operand</param>
        /// <param name="beta">sencond operand</param>
        /// <returns>true if alpha is greater than beta</returns>
        /// <exception cref="InvalidOperationException">Address families must be InterNetwork or InternetworkV6</exception>
        public static bool IsGreaterThan([CanBeNull] this IPAddress alpha,
                                         [CanBeNull] IPAddress beta)
        {
            if ((alpha == null && beta == null)
                || alpha == null
                || beta == null
                || beta.AddressFamily != alpha.AddressFamily)
            {
                return false;
            }

            if (!beta.IsIPv4()
                && !beta.IsIPv6())
            {
                throw new InvalidOperationException("Address families must be InterNetwork or InternetworkV6");
            }

            return !alpha.Equals(beta)
                   && alpha.ToUnsignedBigInteger() > beta.ToUnsignedBigInteger();
        }

        /// <summary>
        ///     Greater Than or equal
        /// </summary>
        /// <param name="alpha">first operand</param>
        /// <param name="beta">sencond operand</param>
        /// <returns>true if alpha is greater than or equal beta</returns>
        /// <exception cref="InvalidOperationException">Address families must be InterNetwork or InternetworkV6</exception>
        public static bool IsGreaterThanOrEqualTo([CanBeNull] this IPAddress alpha,
                                                  [CanBeNull] IPAddress beta)
        {
            if (alpha == null
                && beta == null)
            {
                return true;
            }

            if (alpha == null
                || beta == null
                || beta.AddressFamily != alpha.AddressFamily)
            {
                return false;
            }

            if (!beta.IsIPv4()
                && !beta.IsIPv6())
            {
                throw new InvalidOperationException("Address families must be InterNetwork or InternetworkV6");
            }

            return alpha.Equals(beta)
                   || alpha.ToUnsignedBigInteger() >= beta.ToUnsignedBigInteger();
        }

        /// <summary>
        ///     Determine if the tested IP Address occurs numerically between the given high and low IP addresses
        ///     Inclusivity contingent on inclusive bit
        /// </summary>
        /// <param name="input">IP address to test</param>
        /// <param name="low">low value</param>
        /// <param name="high">high value</param>
        /// <param name="inclusive">true if bounds are inclusive (defaults to true)</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"><paramref name="input" /> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="low" /> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="high" /> is <see langword="null" />.</exception>
        public static bool IsBetween([NotNull] this IPAddress input,
                                     [NotNull] IPAddress low,
                                     [NotNull] IPAddress high,
                                     bool inclusive = true)
        {
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

            return low.AddressFamily == high.AddressFamily
                   && input.AddressFamily == low.AddressFamily
                   && input.ToUnsignedBigInteger()
                           .Between(low.ToUnsignedBigInteger(), high.ToUnsignedBigInteger(), inclusive);
        }

        /// <summary>
        ///     Less Than
        /// </summary>
        /// <param name="alpha">first operand</param>
        /// <param name="beta">sencond operand</param>
        /// <returns>true if alpha is less than beta</returns>
        /// <exception cref="InvalidOperationException">Address families must be InterNetwork or InternetworkV6</exception>
        public static bool IsLessThan([CanBeNull] this IPAddress alpha,
                                      [CanBeNull] IPAddress beta)
        {
            if ((alpha == null && beta == null)
                || alpha == null
                || beta == null
                || beta.AddressFamily != alpha.AddressFamily)
            {
                return false;
            }

            if (!beta.IsIPv4()
                && !beta.IsIPv6())
            {
                throw new InvalidOperationException("Address families must be InterNetwork or InternetworkV6");
            }

            return !alpha.Equals(beta)
                   && alpha.ToUnsignedBigInteger() < beta.ToUnsignedBigInteger();
        }

        /// <summary>
        ///     Less Than or equal
        /// </summary>
        /// <param name="alpha">first operand</param>
        /// <param name="beta">sencond operand</param>
        /// <returns>true if alpha is less than or equal beta</returns>
        /// <exception cref="InvalidOperationException">Address families must be InterNetwork or InternetworkV6</exception>
        public static bool IsLessThanOrEqualTo([CanBeNull] this IPAddress alpha,
                                               [CanBeNull] IPAddress beta)
        {
            if (alpha == null
                && beta == null)
            {
                return true;
            }

            if (alpha == null
                || beta == null
                || beta.AddressFamily != alpha.AddressFamily)
            {
                return false;
            }

            if (!beta.IsIPv4()
                && !beta.IsIPv6())
            {
                throw new InvalidOperationException("Address families must be InterNetwork or InternetworkV6");
            }

            return alpha.Equals(beta)
                   || alpha.ToUnsignedBigInteger() <= beta.ToUnsignedBigInteger();
        }

        /// <summary>
        ///     Get Maximum <see cref="IPAddress" /> (based on bytes)
        /// </summary>
        /// <param name="alpha">first operand</param>
        /// <param name="beta">sencond operand</param>
        /// <returns>the largest of the two operands</returns>
        /// <exception cref="ArgumentNullException"><paramref name="alpha" /> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="beta" /> is <see langword="null" />.</exception>
        /// <exception cref="InvalidOperationException">Address families must match</exception>
        /// <exception cref="InvalidOperationException">Address families must be InterNetwork or InternetworkV6</exception>
        public static IPAddress Max([NotNull] IPAddress alpha,
                                    [NotNull] IPAddress beta)
        {
            if (alpha == null)
            {
                throw new ArgumentNullException(nameof(alpha));
            }

            if (beta == null)
            {
                throw new ArgumentNullException(nameof(beta));
            }

            if (beta.AddressFamily != alpha.AddressFamily)
            {
                throw new InvalidOperationException("Address families must match");
            }

            if (!beta.IsIPv4()
                && !beta.IsIPv6())
            {
                throw new InvalidOperationException("Address families must be InterNetwork or InternetworkV6");
            }

            return alpha.IsGreaterThan(beta)
                       ? alpha
                       : beta;
        }

        /// <summary>
        ///     Get Minimum IPAddress (based on bytes)
        /// </summary>
        /// <param name="alpha">first operand</param>
        /// <param name="beta">sencond operand</param>
        /// <returns>the smallest of the two operands</returns>
        /// <exception cref="ArgumentNullException"><paramref name="alpha" /> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="beta" /> is <see langword="null" />.</exception>
        /// <exception cref="InvalidOperationException">Address families must match</exception>
        /// <exception cref="InvalidOperationException">Address families must be InterNetwork or InternetworkV6</exception>
        public static IPAddress Min([NotNull] IPAddress alpha,
                                    [NotNull] IPAddress beta)
        {
            if (alpha == null)
            {
                throw new ArgumentNullException(nameof(alpha));
            }

            if (beta == null)
            {
                throw new ArgumentNullException(nameof(beta));
            }

            if (beta.AddressFamily != alpha.AddressFamily)
            {
                throw new InvalidOperationException("Address families must match");
            }

            if (!beta.IsIPv4()
                && !beta.IsIPv6())
            {
                throw new InvalidOperationException("Address families must be InterNetwork or InternetworkV6");
            }

            return alpha.IsLessThan(beta)
                       ? alpha
                       : beta;
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
            if (address == null)
            {
                throw new ArgumentNullException(nameof(address));
            }

            switch (address.AddressFamily)
            {
                case AddressFamily.InterNetwork:
                    return address.Equals(IPAddressUtilities.IPv4MaxAddress);
                case AddressFamily.InterNetworkV6:
                    return address.Equals(IPAddressUtilities.IPv6MaxAddress);
                default:
                    throw new InvalidOperationException("Address families must be InterNetwork or InternetworkV6");
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
            if (address == null)
            {
                throw new ArgumentNullException(nameof(address));
            }

            switch (address.AddressFamily)
            {
                case AddressFamily.InterNetwork:
                    return address.Equals(IPAddressUtilities.IPv4MinAddress);
                case AddressFamily.InterNetworkV6:
                    return address.Equals(IPAddressUtilities.IPv6MinAddress);
                default:
                    throw new InvalidOperationException("Address families must be InterNetwork or InternetworkV6");
            }
        }

        #endregion
    }
}
