using System.Net;
using System.Net.Sockets;
using System.Numerics;
using Arcus.Math;
using Arcus.Utilities;

namespace Arcus
{
    /// <summary>
    ///     An <see langword="abstract" /> implementation of <see cref="IIPAddressRange" /> built to work with IPv4 and IPv6
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         IPv4 addresses are represented as 32-bit unsigned integers per
    ///         <see href="https://www.rfc-editor.org/rfc/rfc791#section-2.3">RFC 791 §2.3</see>.
    ///         IPv6 addresses are represented as 128-bit unsigned integers per
    ///         <see href="https://www.rfc-editor.org/info/rfc4291/#section-2">RFC 4291 §2</see>;
    ///         <see cref="BigInteger"/> is used throughout because IPv6 ranges can exceed <see cref="long.MaxValue"/>.
    ///     </para>
    ///     <para>
    ///         This class is <see langword="abstract"/> to enforce a concrete subtype
    ///         (<see cref="Subnet"/> or <see cref="IPAddressRange"/>) for instantiation.
    ///         Although it has no <see langword="abstract"/> members, the abstract modifier
    ///         prevents direct construction, ensures each instance carries a meaningful
    ///         concrete type for equality, comparison, and serialization, and reserves
    ///         the <see cref="IIPAddressRange"/> contract for the two supported implementations.
    ///     </para>
    /// </remarks>
    public abstract partial class AbstractIPAddressRange : IIPAddressRange
    {
        /// <summary>
        ///     The default maximum enumeration exponent (12), limiting enumeration to 4096 addresses.
        /// </summary>
        internal const int DefaultMaxEnumerationExponent = 12;

        /// <summary>
        ///     Gets the maximum enumeration exponent for this range. Enumeration via <see cref="ToIPAddresses"/> (and <c>foreach</c> /
        ///     <see cref="System.Collections.Generic.IEnumerable{T}"/> for backwards compatibility) yields at most
        ///     2<sup>MaxEnumerationExponent</sup> addresses. Defaults to <see cref="DefaultMaxEnumerationExponent"/>
        ///     (2<sup>12</sup> = 4096) to prevent accidental enumeration of enormous address spaces.
        /// </summary>
        /// <value>The maximum enumeration exponent (0-128).</value>
        public int MaxEnumerationExponent { get; }

        /// <summary>
        ///     Gets a value indicating whether this range contains exactly one address.
        /// </summary>
        /// <value><see langword="true" /> when <see cref="Length"/> equals 1.</value>
        public bool IsSingleIP => this.Length == 1;

        /// <inheritdoc />
        public AddressFamily AddressFamily => this.Head.AddressFamily;

        /// <inheritdoc />
        public bool IsIPv4 => this.Head.IsIPv4();

        /// <inheritdoc />
        public bool IsIPv6 => this.Head.IsIPv6();

        /// <inheritdoc />
        public IPAddress Head { get; }

        /// <inheritdoc />
        public IPAddress Tail { get; }

        /// <inheritdoc />
        /// <remarks>
        ///     <para>
        ///         <see cref="Length"/> is computed once in the constructor and cached by the readonly
        ///         backing field; subsequent reads are O(1) and allocation-free.
        ///     </para>
        /// </remarks>
        public BigInteger Length { get; }

        #region AddressTuple

        /// <summary>
        ///     Holds a pair of <see cref="IPAddress" /> objects as an immutable unit.
        /// </summary>
        /// <param name="head">The head address (must not be <see langword="null" />).</param>
        /// <param name="tail">The tail address (must not be <see langword="null" />).</param>
        private protected readonly struct AddressTuple(IPAddress head, IPAddress tail) : IEquatable<AddressTuple>
        {
            /// <summary>
            ///     Gets the head address of the pair.
            /// </summary>
            /// <value>The first (numerically lower) address.</value>
            public IPAddress Head { get; } = head ?? throw new ArgumentNullException(nameof(head));

            /// <summary>
            ///     Gets the tail address of the pair.
            /// </summary>
            /// <value>The second (numerically higher) address.</value>
            public IPAddress Tail { get; } = tail ?? throw new ArgumentNullException(nameof(tail));

            /// <inheritdoc />
            public bool Equals(AddressTuple other)
            {
                if (other.Head is null || other.Tail is null)
                {
                    return false;
                }

                return this.Head.Equals(other.Head) && this.Tail.Equals(other.Tail);
            }

            /// <inheritdoc />
            public override bool Equals(object obj) => obj is AddressTuple other && this.Equals(other);

            /// <summary>
            ///     Determines whether two <see cref="AddressTuple"/> instances are equal.
            /// </summary>
            /// <param name="left">The first instance.</param>
            /// <param name="right">The second instance.</param>
            /// <returns><see langword="true" /> if the two pairs contain equal addresses.</returns>
            public static bool operator ==(AddressTuple left, AddressTuple right) => left.Equals(right);

            /// <summary>
            ///     Determines whether two <see cref="AddressTuple"/> instances are not equal.
            /// </summary>
            /// <param name="left">The first instance.</param>
            /// <param name="right">The second instance.</param>
            /// <returns><see langword="true" /> if the two pairs differ in either address.</returns>
            public static bool operator !=(AddressTuple left, AddressTuple right) => !(left == right);

            /// <inheritdoc />
            public override int GetHashCode() => HashCode.Combine(this.Head ?? IPAddress.None, this.Tail ?? IPAddress.None);
        }

        #endregion // end: AddressTuple

        #region Ctor

        /// <summary>
        ///     Initializes a new instance of the <see cref="AbstractIPAddressRange"/> class.
        /// </summary>
        /// <param name="head">the range head (lowest valued <see cref="IPAddress" />)</param>
        /// <param name="tail">the range tail (highest valued <see cref="IPAddress" />)</param>
        /// <param name="maxEnumerationExponent">the maximum enumeration exponent (0-128); enumeration yields at most 2<sup>maxEnumerationExponent</sup> addresses</param>
        protected AbstractIPAddressRange(
            IPAddress head,
            IPAddress tail,
            int maxEnumerationExponent = DefaultMaxEnumerationExponent
        )
        {
            if (head is null)
            {
                throw new ArgumentNullException(nameof(head));
            }

            if (tail is null)
            {
                throw new ArgumentNullException(nameof(tail));
            }

            if (maxEnumerationExponent is < 0 or > 128)
            {
                throw new ArgumentOutOfRangeException(nameof(maxEnumerationExponent));
            }

            var maxExponentForFamily = head.AddressFamily == AddressFamily.InterNetwork ? 32 : 128;
            this.MaxEnumerationExponent =
                maxEnumerationExponent < maxExponentForFamily ? maxEnumerationExponent : maxExponentForFamily;

            #region defense

            if (!IPAddressUtilities.ValidAddressFamilies.Contains(head.AddressFamily))
            {
                throw new ArgumentException(
                    $"{nameof(head)} must have an address family equal to {string.Join(", ", IPAddressUtilities.ValidAddressFamilies)}",
                    nameof(head)
                );
            }

            if (!IPAddressUtilities.ValidAddressFamilies.Contains(tail.AddressFamily))
            {
                throw new ArgumentException(
                    $"{nameof(tail)} must have an address family equal to {string.Join(", ", IPAddressUtilities.ValidAddressFamilies)}",
                    nameof(tail)
                );
            }

            if (head.AddressFamily != tail.AddressFamily)
            {
                throw new InvalidOperationException($"{nameof(head)} and {nameof(tail)} must have matching address families");
            }

            if (!tail.IsGreaterThanOrEqualTo(head))
            {
                throw new InvalidOperationException($"{nameof(tail)} must be greater or equal to {nameof(head)}");
            }

            #endregion // end: defense

            this.Head = head;
            this.Tail = tail;
            this.Length = CalculateLength();

            BigInteger CalculateLength()
            {
                var tailWrap = BigEndianBitWrapper.FromBytes(tail.GetAddressBytes());
                var headWrap = BigEndianBitWrapper.FromBytes(head.GetAddressBytes());
                return tailWrap.Subtract(headWrap).ToBigInteger() + 1;
            }
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="AbstractIPAddressRange"/> class.
        /// </summary>
        /// <param name="addressTuple">an <see cref="AddressTuple"/> representing two addresses</param>
        /// <param name="maxEnumerationExponent">the maximum enumeration exponent (0-128)</param>
        private protected AbstractIPAddressRange(
            AddressTuple addressTuple,
            int maxEnumerationExponent = DefaultMaxEnumerationExponent
        )
            : this(addressTuple.Head, addressTuple.Tail, maxEnumerationExponent)
        {
            // nothing additional to do
        }

        #endregion // end: Ctor

        #region Equals / GetHashCode

        /// <summary>
        ///     Determines whether the specified object is equal to the current range by comparing
        ///     <see cref="Head"/> and <see cref="Tail"/>. Two ranges of different concrete types
        ///     (e.g. a <see cref="Subnet"/> and an <see cref="IPAddressRange"/>) with identical
        ///     <see cref="Head"/> and <see cref="Tail"/> are considered equal by this base implementation.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         Subclasses (<see cref="Subnet"/>, <see cref="IPAddressRange"/>) override this method
        ///         to enforce type-specific equality; the shared algorithm lives here so that the
        ///         <see cref="GetHashCode"/> contract is uniform across implementations.
        ///     </para>
        /// </remarks>
        /// <param name="obj">The object to compare.</param>
        /// <returns><see langword="true"/> if <paramref name="obj"/> is an <see cref="IIPAddressRange"/> with matching Head and Tail.</returns>
        public override bool Equals(object obj)
        {
            if (obj is IIPAddressRange other)
            {
                return this.Head.Equals(other.Head) && this.Tail.Equals(other.Tail);
            }

            return false;
        }

        /// <summary>
        ///     Returns a hash code based on <see cref="Head"/> and <see cref="Tail"/>. All
        ///     <see cref="AbstractIPAddressRange"/> subclasses use this same function so that
        ///     value-equal ranges hash identically regardless of concrete type.
        /// </summary>
        /// <returns>A hash code for the current range.</returns>
        public override int GetHashCode() => HashCode.Combine(this.Head, this.Tail);

        #endregion // end: Equals / GetHashCode
    }
}
