#if NETSTANDARD2_0
using System.Runtime.Serialization;

namespace Arcus
{
    /// <content>
    ///     <see cref="IPAddressRange"/> implementation of <see cref="ISerializable"/>
    /// </content>
    public partial class IPAddressRange : ISerializable
    {
        #region From Interface ISerializable

        /// <inheritdoc />
        /// <exception cref="ArgumentNullException"><paramref name="info" /> is <see langword="null" /></exception>
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info is null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            info.AddValue(nameof(Head), Head.GetAddressBytes());
            info.AddValue(nameof(Tail), Tail.GetAddressBytes());
            info.AddValue("SerVersion", 2);
            info.AddValue(nameof(this.MaxEnumerationExponent), this.MaxEnumerationExponent);
        }

        #endregion
    }
}
#endif
