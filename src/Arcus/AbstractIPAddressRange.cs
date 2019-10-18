using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using Arcus.Math;
using Arcus.Utilities;
using Gulliver;
using JetBrains.Annotations;

namespace Arcus
{
    /// <summary>
    ///     An <see langword="abstract" /> implementation of <see cref="IIPAddressRange" /> built to work with IPv4 and IPv6
    /// </summary>
    [PublicAPI]
    public abstract class AbstractIPAddressRange : IIPAddressRange
    {
        /// <summary>
        ///     <see langword="true" /> if the subnet describes a single ip address
        /// </summary>
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
        public BigInteger Length { get; }

        #region From Interface IIPAddressRange

        #region Deconstructors

        /// <inheritdoc />
        public void Deconstruct(out IPAddress head,
                                out IPAddress tail)
        {
            head = this.Head;
            tail = this.Tail;
        }

        #endregion // end: Deconstruct

        #endregion

        #region AddressTuple

        /// <summary>
        ///     AddressTuple for moving around a pair of <see cref="IPAddress" /> objects as a unit
        /// </summary>
        protected struct AddressTuple : IEquatable<AddressTuple>
        {
            /// <summary>
            ///     Initializes a new instance of the <see cref="AddressTuple" /> struct.
            /// </summary>
            /// <param name="head">the head address</param>
            /// <param name="tail">the tail address</param>
            public AddressTuple([NotNull] IPAddress head,
                                [NotNull] IPAddress tail)
            {
                this.Head = head ?? throw new ArgumentNullException(nameof(head));
                this.Tail = tail ?? throw new ArgumentNullException(nameof(tail));
            }

            /// <summary>
            ///     Head
            /// </summary>
            [NotNull]
            public IPAddress Head { get; }

            /// <summary>
            ///     Tail
            /// </summary>
            [NotNull]
            public IPAddress Tail { get; }

            /// <inheritdoc />
            public bool Equals(AddressTuple other)
            {
                return this.Head.Equals(other.Head)
                       && this.Tail.Equals(other.Tail);
            }

            /// <inheritdoc />
            public override bool Equals(object obj)
            {
                return obj is AddressTuple other && this.Equals(other);
            }

            /// <inheritdoc />
            public override int GetHashCode()
            {
                unchecked
                {
                    return (this.Head.GetHashCode() * 397) ^ this.Tail.GetHashCode();
                }
            }

            /// <summary>
            ///     Equals operation
            /// </summary>
            /// <param name="left">left operand</param>
            /// <param name="right">right operand</param>
            /// <returns><see langword="true" /> if <paramref name="left"></paramref> and <paramref name="right" />are equal</returns>
            public static bool operator ==(AddressTuple left,
                                           AddressTuple right)
            {
                return left.Equals(right);
            }

            /// <summary>
            ///     Not Equals operation
            /// </summary>
            /// <param name="left">left operand</param>
            /// <param name="right">right operand</param>
            /// <returns><see langword="true" /> if <paramref name="left"></paramref> and <paramref name="right" />are equal</returns>
            public static bool operator !=(AddressTuple left,
                                           AddressTuple right)
            {
                return !(left == right);
            }
        }

        #endregion // end: AddressTuple

        #region Ctor

        /// <summary>
        ///     Initializes a new instance of the <see cref="AbstractIPAddressRange"/> class.
        /// </summary>
        /// <param name="head">the range head (lowest valued <see cref="IPAddress" />)</param>
        /// <param name="tail">the range tail (highest valued <see cref="IPAddress" />)</param>
        protected AbstractIPAddressRange([NotNull] IPAddress head,
                                         [NotNull] IPAddress tail)
        {
            #region defense

            if (head == null)
            {
                throw new ArgumentNullException(nameof(head));
            }

            if (tail == null)
            {
                throw new ArgumentNullException(nameof(tail));
            }

            if (!IPAddressUtilities.ValidAddressFamilies.Contains(head.AddressFamily))
            {
                throw new ArgumentException($"{nameof(head)} must have an address family equal to {string.Join(", ", IPAddressUtilities.ValidAddressFamilies)}", nameof(head));
            }

            if (!IPAddressUtilities.ValidAddressFamilies.Contains(tail.AddressFamily))
            {
                throw new ArgumentException($"{nameof(tail)} must have an address family equal to {string.Join(", ", IPAddressUtilities.ValidAddressFamilies)}", nameof(tail));
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
                // convert big endian (network byte order) difference result to a 0x00 prefixed byte array for converting to an unsigned BigInteger
                var differenceBytes = ByteArrayUtils.SubtractUnsignedBigEndian(tail.GetAddressBytes(), head.GetAddressBytes());
                var differenceBytesLength = differenceBytes.Length;
                var unsignedLittleEndianBytes = new byte[differenceBytesLength + 1]; // one greater so trailing byte is always 0x00

                for (var i = 0; i < differenceBytesLength; i++)
                {
                    unsignedLittleEndianBytes[differenceBytesLength - 1 - i] = differenceBytes[i];
                }

                return new BigInteger(unsignedLittleEndianBytes) + 1; // a rare but valid use of BigInteger;
            }
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="AbstractIPAddressRange"/> class.
        /// </summary>
        /// <param name="addressTuple">an <see cref="AddressTuple"/> representing two addresses</param>
        protected AbstractIPAddressRange(AddressTuple addressTuple)
            : this(addressTuple.Head, addressTuple.Tail)
        {
            // nothing additional to do
        }

        #endregion // end: Ctor

        #region TryGetLength

        /// <inheritdoc />
        public bool TryGetLength(out int length)
        {
            var actualLength = this.Length;

            if (actualLength <= int.MaxValue)
            {
                length = (int) actualLength;
                return true;
            }

            length = -1;
            return false;
        }

        /// <inheritdoc />
        public bool TryGetLength(out long length)
        {
            var actualLength = this.Length;

            if (actualLength <= long.MaxValue)
            {
                length = (long) actualLength;
                return true;
            }

            length = -1;
            return false;
        }

        #endregion // end: TryGetLength

        #region IEnumerable / IEnumerable<IPAddress>

        /// <inheritdoc />
        public IEnumerator<IPAddress> GetEnumerator()
        {
            // determine maximum possible address for iteration
            var addressLimit = IPAddressMath.Min(this.Tail,
                                                 this.IsIPv4
                                                     ? IPAddressUtilities.IPv4MaxAddress
                                                     : IPAddressUtilities.IPv6MaxAddress)
                                            .GetAddressBytes();

            // determine the width of the bye array for the address address
            var addressByteWidth = this.IsIPv4
                                       ? IPAddressUtilities.IPv4ByteCount
                                       : IPAddressUtilities.IPv6ByteCount;

            var currentAddressBytes = this.Head.GetAddressBytes();

            //  iterate appropriately as long as the current address isn't beyond the limit
            while (ByteArrayUtils.CompareUnsignedBigEndian(currentAddressBytes, addressLimit) <= 0)
            {
                yield return new IPAddress(currentAddressBytes);

                // determine next address
                if (!ByteArrayUtils.TrySumBigEndian(currentAddressBytes, 1, out var nextAddressBytes))
                {
                    break;
                }

                var nextAddressByteWidth = nextAddressBytes.Length;
                if (nextAddressByteWidth > addressByteWidth)
                {
                    break;
                }

                // copy appropriate portion of next address with prefixed 0x00 bytes
                currentAddressBytes = new byte[addressByteWidth];
                Array.Copy(nextAddressBytes, 0, currentAddressBytes, addressByteWidth - nextAddressByteWidth, nextAddressByteWidth);
            }
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion // end: IEnumerable

        #region Formatting

        /// <inheritdoc />
        public override string ToString()
        {
            return this.ToString("G", CultureInfo.InvariantCulture);
        }

        /// <inheritdoc />
        public virtual string ToString(string format,
                                       IFormatProvider formatProvider)
        {
            switch (format?.Trim())
            {
                case null:
                case "":

                // general formats
                case "g":
                case "G":
                    return $"{this.Head} - {this.Tail}";
                default:
                    throw new FormatException($"The format \"{format}\" is not supported.");
            }
        }

        #endregion // end: Formatting

        #region Set Operations

        #region Contains

        /// <inheritdoc />
        public bool Contains(IIPAddressRange that)
        {
            return ReferenceEquals(this, that)
                   || Equals(this, that)
                   || (that != null
                   && this.Contains(that.Head)
                   && this.Contains(that.Tail));
        }

        /// <inheritdoc />
        public bool Contains(IPAddress address)
        {
            return address != null
                   && address.AddressFamily == this.AddressFamily
                   && address.IsBetween(this.Head, this.Tail);
        }

        #endregion // end: Contains

        #region Ovelap and Touches

        /// <inheritdoc />
        public bool HeadOverlappedBy(IIPAddressRange that)
        {
            return ReferenceEquals(this, that)
                   || Equals(this, that)
                   || (that != null
                   && that.Contains(this.Head));
        }

        /// <inheritdoc />
        public bool TailOverlappedBy(IIPAddressRange that)
        {
            return ReferenceEquals(this, that)
                   || Equals(this, that)
                   || (that != null
                   && that.Contains(this.Tail));
        }

        /// <inheritdoc />
        public bool Overlaps(IIPAddressRange that)
        {
            return ReferenceEquals(this, that)
                   || Equals(this, that)
                   || (that != null
                   && (this.Contains(that)
                       || this.Contains(that.Head)
                       || this.Contains(that.Tail)));
        }

        /// <inheritdoc />
        public bool Touches(IIPAddressRange that)
        {
            return that != null
                   && this.AddressFamily == that.AddressFamily
                   && ((this.Tail.IsLessThan(this.Tail.AddressFamily.MaxIPAddress())    // prevent overflow
                       && Equals(this.Tail.Increment(), that.Head))                     // this tail appears directly before that head
                       || (that.Tail.IsLessThan(that.Tail.AddressFamily.MaxIPAddress()) // prevent overflow
                       && Equals(that.Tail.Increment(), this.Head)));                   // that tail appears directly before this head
        }

        #endregion // end: Ovelap and Touches

        #endregion // end: Set Operations
    }
}
