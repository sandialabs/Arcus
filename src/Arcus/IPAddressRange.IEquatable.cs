namespace Arcus
{
    /// <content>
    ///     <see cref="IPAddressRange"/> implementation of <see cref="IEquatable{IPAddressRange}"/>
    /// </content>
    public partial class IPAddressRange : IEquatable<IPAddressRange>
    {
        #region From Interface IEquatable<IPAddressRange>

        /// <inheritdoc />
        public virtual bool Equals(IPAddressRange other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return this.Head.Equals(other.Head) && this.Tail.Equals(other.Tail);
        }

        #endregion

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj is IPAddressRange other)
            {
                return Equals(other);
            }

            return false;
        }

        /// <inheritdoc />
        public override int GetHashCode() => base.GetHashCode();
    }
}
