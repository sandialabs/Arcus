namespace Arcus
{
    /// <content>
    ///     <see cref="Subnet"/> implementation of <see cref="IEquatable{Subnet}"/>
    /// </content>
    public partial class Subnet : IEquatable<Subnet>
    {
        /// <inheritdoc />
        public virtual bool Equals(Subnet other)
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

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj is Subnet other)
            {
                return Equals(other);
            }

            return false;
        }

        /// <inheritdoc />
        public override int GetHashCode() => base.GetHashCode();
    }
}