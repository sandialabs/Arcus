using System.Diagnostics;
using System.Net;
#if NETSTANDARD2_0
using System.Runtime.Serialization;
#endif

namespace Arcus
{
    /// <summary>
    ///     A basic implementation of a IIPAddressRange used to represent an inclusive range of arbitrary IP Addresses of the
    ///     same address family
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Represents an arbitrary inclusive range of addresses within the IPv4 32-bit address space
    ///         (<see href="https://www.rfc-editor.org/rfc/rfc791#section-2.3">RFC 791 §2.3</see>) or the IPv6 128-bit address
    ///         space (<see href="https://www.rfc-editor.org/rfc/rfc4291#section-2.1">RFC 4291 §2.1</see>). Unlike
    ///         <see cref="Subnet"/>, ranges are not constrained to power-of-two boundaries or valid network addresses.
    ///     </para>
    /// </remarks>
    [DebuggerDisplay("{Head} - {Tail} ({Length})")]
    [System.Serializable]
    public partial class IPAddressRange : AbstractIPAddressRange
    {
        #region Ctor

        /// <summary>
        ///     Initializes a new instance of the <see cref="IPAddressRange" /> class.
        /// </summary>
        /// <param name="address">the <see cref="IPAddress" /></param>
        /// <param name="maxEnumerationExponent">the maximum enumeration exponent (0-128, default 12)</param>
        public IPAddressRange(IPAddress address, int maxEnumerationExponent = DefaultMaxEnumerationExponent)
            : base(address, address, maxEnumerationExponent)
        {
            // nothing more to do
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="IPAddressRange" /> class.
        /// </summary>
        /// <param name="head">head <see cref="IPAddress" /></param>
        /// <param name="tail">tail <see cref="IPAddress" /></param>
        /// <param name="maxEnumerationExponent">the maximum enumeration exponent (0-128, default 12)</param>
        public IPAddressRange(IPAddress head, IPAddress tail, int maxEnumerationExponent = DefaultMaxEnumerationExponent)
            : base(head, tail, maxEnumerationExponent)
        {
            // nothing more to do
        }

#if NETSTANDARD2_0
        /// <summary>Initializes a new instance of the <see cref="IPAddressRange"/> class.</summary>
        /// <param name="info">serialization info</param>
        /// <param name="context">serialization context</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="info"/> is <see langword="null"/></exception>
        protected IPAddressRange(SerializationInfo info, StreamingContext context)
            : this(
                new IPAddress(
                    (byte[])
                        (info ?? throw new System.ArgumentNullException(nameof(info))).GetValue(nameof(Head), typeof(byte[]))
                ),
                new IPAddress((byte[])info.GetValue(nameof(Tail), typeof(byte[]))),
                DeserializeMaxEnumerationExponent(info)
            ) { }

        private static int DeserializeMaxEnumerationExponent(SerializationInfo info)
        {
            try
            {
                return info.GetInt32(nameof(MaxEnumerationExponent));
            }
            catch (SerializationException)
            {
                // Old format (version 1): fall back to default exponent
                return DefaultMaxEnumerationExponent;
            }
        }
#endif

        #endregion // end: Ctor
    }
}
