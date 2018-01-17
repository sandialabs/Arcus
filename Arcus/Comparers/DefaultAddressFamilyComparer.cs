using System.Collections.Generic;
using System.Net.Sockets;

namespace Arcus.Comparers
{
    /// <summary>
    ///     Default Address Family comparer
    /// </summary>
    public class DefaultAddressFamilyComparer : Comparer<AddressFamily>
    {
        public override int Compare(AddressFamily x,
                                    AddressFamily y)
        {
            return x.CompareTo(y);
        }
    }
}
