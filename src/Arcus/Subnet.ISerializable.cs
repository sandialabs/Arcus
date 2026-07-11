#if NETSTANDARD2_0
using System.Runtime.Serialization;

namespace Arcus
{
    /// <content>
    ///     <see cref="Subnet"/> implementation of <see cref="ISerializable"/>
    /// </content>
    public partial class Subnet : ISerializable
    {
        /// <summary>
        ///     The serialization format version written by <see cref="GetObjectData"/>.
        ///     Deserialization rejects data carrying a different version.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         Version 3: writes <see cref="NetworkPrefixAddress"/> (Head) instead of the
        ///         previously-written <see cref="BroadcastAddress"/> (Tail). Head is the
        ///         semantically correct seed value for reconstruction; the broadcast address
        ///         was an implementation artefact of the legacy <c>CtorFactory</c>.
        ///     </para>
        /// </remarks>
        internal const int SerializationFormatVersion = 3;

        /// <inheritdoc />
        /// <exception cref="ArgumentNullException"><paramref name="info" /> is <see langword="null" /></exception>
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info is null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            info.AddValue(nameof(this.NetworkPrefixAddress), this.NetworkPrefixAddress.GetAddressBytes());
            info.AddValue(nameof(this.RoutingPrefix), this.RoutingPrefix);
            info.AddValue("SerVersion", SerializationFormatVersion);
            info.AddValue(nameof(this.MaxEnumerationExponent), this.MaxEnumerationExponent);
        }

        /// <summary>Reconstructs a <see cref="Subnet"/> from serialization info - test affordance.</summary>
        /// <param name="info">serialization info populated by <see cref="GetObjectData"/> (or hand-built for version tests).</param>
        /// <returns>The deserialized <see cref="Subnet"/>.</returns>
        internal static Subnet DeserializeFromInfo(SerializationInfo info) => new(DeserializeSubnet(info));
    }
}
#endif
