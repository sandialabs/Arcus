using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace Arcus.Comparers
{
    /// <summary>
    ///     Default <see cref="AddressFamily" /> <see cref="Comparer{T}" />
    ///     Executes the <see cref="Enum.CompareTo" /> of <see cref="AddressFamily" /> method
    /// </summary>
    public class DefaultAddressFamilyComparer : Comparer<AddressFamily>
    {
        /// <inheritdoc />
        public override int Compare(AddressFamily x,
                                    AddressFamily y)
        {
            return x.CompareTo(y);
        }
    }
}
