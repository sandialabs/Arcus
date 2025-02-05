using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using Arcus.Comparers;
using Arcus.Math;

namespace Arcus
{
    /// <summary>
    ///     A basic implementation of a IIPAddressRange us to represent an inclusive range of arbitrary IP Addresses of the
    ///     same address family
    /// </summary>
    [Serializable]
    public class IPAddressRange
        : AbstractIPAddressRange,
            IEquatable<IPAddressRange>,
            IComparable<IPAddressRange>,
            IComparable,
            ISerializable
    {
        #region From Interface ISerializable

        /// <inheritdoc />
        /// <exception cref="ArgumentNullException"><paramref name="info" /> is <see langword="null" /></exception>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            info.AddValue(nameof(Head), Head.GetAddressBytes());
            info.AddValue(nameof(Tail), Tail.GetAddressBytes());
        }

        #endregion

        #region From Interface IComparable

        /// <inheritdoc />
        public int CompareTo(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return 1;
            }

            if (ReferenceEquals(this, obj))
            {
                return 0;
            }

            return obj is IPAddressRange other
                ? this.CompareTo(other)
                : throw new ArgumentException($"Object must be of type {nameof(IPAddressRange)}");
        }

        #endregion

        #region From Interface IComparable<IPAddressRange>

        /// <inheritdoc />
        public int CompareTo(IPAddressRange other)
        {
            return new DefaultIPAddressRangeComparer().Compare(this, other);
        }

        #endregion

        #region From Interface IEquatable<IPAddressRange>

        /// <inheritdoc />
        public bool Equals(IPAddressRange other)
        {
            return !ReferenceEquals(null, other)
                && (ReferenceEquals(this, other) || (Equals(Head, other.Head) && Equals(Tail, other.Tail)));
        }

        #endregion

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return !ReferenceEquals(null, obj)
                && (ReferenceEquals(this, obj) || (obj.GetType() == GetType() && this.Equals((IPAddressRange)obj)));
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return (Head.GetHashCode() * 397) ^ Tail.GetHashCode();
            }
        }

        #region operators

        /// <summary>
        ///     Compares two <see cref="IPAddressRange" /> objects for equality
        /// </summary>
        /// <param name="left">left hand operand</param>
        /// <param name="right">right hand operand</param>
        /// <returns><see langword="true" /> when both sides are equal</returns>
        public static bool operator ==(IPAddressRange left, IPAddressRange right)
        {
            return Equals(left, right);
        }

        /// <summary>
        ///     Compares two <see cref="IPAddressRange" /> objects for non-equality
        /// </summary>
        /// <param name="left">left hand operand</param>
        /// <param name="right">right hand operand</param>
        /// <returns><see langword="true" /> when both sides are not equal</returns>
        public static bool operator !=(IPAddressRange left, IPAddressRange right)
        {
            return !Equals(left, right);
        }

        /// <summary>
        ///     Compares two <see cref="IPAddressRange" /> objects for <paramref name="left" /> being less than
        ///     <paramref name="right" />
        /// </summary>
        /// <param name="left">left hand operand</param>
        /// <param name="right">right hand operand</param>
        /// <returns><see langword="true" /> when <paramref name="left" /> is less than <paramref name="right" /></returns>
        public static bool operator <(IPAddressRange left, IPAddressRange right)
        {
            return Comparer<IPAddressRange>.Default.Compare(left, right) < 0;
        }

        /// <summary>
        ///     Compares two <see cref="IPAddressRange" /> objects for <paramref name="left" /> being greater than
        ///     <paramref name="right" />
        /// </summary>
        /// <param name="left">left hand operand</param>
        /// <param name="right">right hand operand</param>
        /// <returns><see langword="true" /> when <paramref name="left" /> is greater than <paramref name="right" /></returns>
        public static bool operator >(IPAddressRange left, IPAddressRange right)
        {
            return Comparer<IPAddressRange>.Default.Compare(left, right) > 0;
        }

        /// <summary>
        ///     Compares two <see cref="IPAddressRange" /> objects for <paramref name="left" /> being less than or equal
        ///     <paramref name="right" />
        /// </summary>
        /// <param name="left">left hand operand</param>
        /// <param name="right">right hand operand</param>
        /// <returns>
        ///     <see langword="true" /> when <paramref name="left" /> is less than or equal to <paramref name="right" />
        /// </returns>
        public static bool operator <=(IPAddressRange left, IPAddressRange right)
        {
            return Comparer<IPAddressRange>.Default.Compare(left, right) <= 0;
        }

        /// <summary>
        ///     Compares two <see cref="IPAddressRange" /> objects for <paramref name="left" /> being greater than or equal to
        ///     <paramref name="right" />
        /// </summary>
        /// <param name="left">left hand operand</param>
        /// <param name="right">right hand operand</param>
        /// <returns>
        ///     <see langword="true" /> when <paramref name="left" /> is greater than or equal to <paramref name="right" />
        /// </returns>
        public static bool operator >=(IPAddressRange left, IPAddressRange right)
        {
            return Comparer<IPAddressRange>.Default.Compare(left, right) >= 0;
        }

        #endregion end operators

        #region Ctor

        /// <summary>
        ///     Initializes a new instance of the <see cref="IPAddressRange" /> class.
        /// </summary>
        /// <param name="address">the <see cref="IPAddress" /></param>
        public IPAddressRange(IPAddress address)
            : base(address, address)
        {
            // nothing more to do
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="IPAddressRange" /> class.
        /// </summary>
        /// <param name="head">head <see cref="IPAddress" /></param>
        /// <param name="tail">tail <see cref="IPAddress" /></param>
        public IPAddressRange(IPAddress head, IPAddress tail)
            : base(head, tail)
        {
            // nothing more to do
        }

        /// <summary>Initializes a new instance of the <see cref="IPAddressRange"/> class.</summary>
        /// <param name="info">serialization info</param>
        /// <param name="context">serialization context</param>
        /// <exception cref="ArgumentNullException"><paramref name="info"/> is <see langword="null"/></exception>
        protected IPAddressRange(SerializationInfo info, StreamingContext context)
            : this(
                new IPAddress(
                    (byte[])(info ?? throw new ArgumentNullException(nameof(info))).GetValue(nameof(Head), typeof(byte[]))
                ),
                new IPAddress((byte[])info.GetValue(nameof(Tail), typeof(byte[])))
            ) { }

        #endregion // end: Ctor

        #region static methods

        /// <summary>
        ///     Attempt collapse the given input of ranges into fewer ranges thus optimizing
        ///     Ranges that overlap, or butt against each other may be collapsed into a single range
        /// </summary>
        /// <param name="ranges">ranges to collapse</param>
        /// <param name="result">resulting ranges post collapse</param>
        /// <returns>true on success</returns>
        public static bool TryCollapseAll(IEnumerable<IPAddressRange> ranges, out IEnumerable<IPAddressRange> result)
        {
            var rangeList = (ranges ?? Enumerable.Empty<IPAddressRange>()).ToList();

            // item null check
            if (rangeList.Contains(null))
            {
                result = Enumerable.Empty<IPAddressRange>();
                return false;
            }

            // no ranges provided
            if (!rangeList.Any()) // no ranges
            {
                result = Enumerable.Empty<IPAddressRange>();
                return true; // assume success
            }

            // all families don't match match
            if (rangeList.Any(r => r.AddressFamily != rangeList[0].AddressFamily))
            {
                result = Enumerable.Empty<IPAddressRange>();
                return false;
            }

            // sort range list, has to be done post validation check, as invalid cannot be sorted
            rangeList = rangeList.OrderBy(r => r).ToList();

            var resultList = new List<IPAddressRange>
            {
                rangeList[0], // start with first item in the sorted list of ranges
            };

            // iterate over items in range list, no need to iterate over first as it is included by default
            foreach (var range in rangeList.Skip(1))
            {
                var last = resultList[resultList.Count - 1]; // take end of result to process over (is first in first iteration)

                // can be assumed that assume that the range head is greater than or equal to last head because everything is ordered

                // if TryMerge succeeded then overlap exists, merge overlap and re-assign to end of results
                if (TryMerge(last, range, out var merge))
                {
                    resultList[resultList.Count - 1] = merge; // overwrite with merged values
                    continue;
                }

                // could not merge, simply add to end and iterate to next
                resultList.Add(range);
            }

            result = resultList;
            return true;
        }

        /// <summary>
        ///     Rebuild the initial range as an <see cref="IEnumerable{T}" /> of ranges excluding the excluded ranges
        ///     excluded ranges are expected to each be sub ranges of the initial range
        ///     an attempt will be made to TryCollapseAll the excluded
        /// </summary>
        /// <param name="initialRange">the initial <see cref="IPAddressRange" /> to exclude from</param>
        /// <param name="excludedRanges">
        ///     the various <see cref="IPAddressRange" /> to exclude from the
        ///     <paramref name="initialRange" />
        /// </param>
        /// <param name="result">the resulting  <see cref="IPAddressRange" /> <see cref="IEnumerable{T}" /></param>
        /// <exception cref="InvalidOperationException">unexpected invalid operation.</exception>
        /// <returns>true on success</returns>
        public static bool TryExcludeAll(
            IPAddressRange initialRange,
            IEnumerable<IPAddressRange> excludedRanges,
            out IEnumerable<IPAddressRange> result
        )
        {
            // TODO the logical flow here is confusing, see if it can be cleaned up a bit

            if (initialRange == null || excludedRanges == null)
            {
                result = Enumerable.Empty<IPAddressRange>();
                return false;
            }

            var excludedRangesList = excludedRanges as IList<IPAddressRange> ?? excludedRanges.ToList();

            // item null check
            if (excludedRangesList.Any(r => r == null))
            {
                result = Enumerable.Empty<IPAddressRange>();
                return false;
            }

            // no rangers to exclude, out copy of original
            if (!excludedRangesList.Any())
            {
                result = new List<IPAddressRange> { new IPAddressRange(initialRange.Head, initialRange.Tail) };

                return true;
            }

            // all families don't match match
            if (excludedRangesList.Any(r => r.AddressFamily != initialRange.AddressFamily))
            {
                result = Enumerable.Empty<IPAddressRange>();
                return false;
            }

            // results is initialized with a *copy* of initialRange
            var resultList = new List<IPAddressRange> { new IPAddressRange(initialRange.Head, initialRange.Tail) };

            foreach (var exclusion in excludedRangesList)
            {
                var last = resultList[resultList.Count - 1];
                var lastIndex = resultList.Count - 1;

                if (exclusion.Contains(last))
                {
                    resultList.RemoveAt(lastIndex);
                    break; // can't loop any more
                }

                if (exclusion.Contains(initialRange.Tail))
                {
                    var head = resultList[lastIndex].Head;
                    var tail = exclusion.Head.Increment(-1);
                    resultList[lastIndex] = new IPAddressRange(head, tail);
                    continue; // can't loop any more
                }

                // exclusion contains head
                if (exclusion.Contains(last.Head))
                {
                    // push head one point beyond tail of exclusion
                    var head = exclusion.Tail.Increment();
                    var tail = resultList[lastIndex].Tail;
                    resultList[lastIndex] = new IPAddressRange(head, tail);
                    continue; // next iteration
                }

                // exclusion is within last, carve exclusion to two pieces
                if (!last.Overlaps(exclusion))
                {
                    throw new InvalidOperationException("An unexpected overlap check operation occurred"); // should not have made it here
                }

                resultList[lastIndex] = new IPAddressRange(resultList[lastIndex].Head, exclusion.Head.Increment(-1));

                // carve out a piece at starting at exclusion head, ending at exclusion tail
                // thus meaning one less than head at previous, and one more at head at next
                resultList.Add(new IPAddressRange(exclusion.Tail.Increment(), initialRange.Tail));
            }

            result = resultList;
            return true;
        }

        /// <summary>
        ///     Merge two touching or overlapping address ranges
        /// </summary>
        /// <param name="left">the left operand</param>
        /// <param name="right">the right operand</param>
        /// <param name="mergedRange">the resulting <see cref="IPAddressRange" /></param>
        /// <returns>true on success</returns>
        public static bool TryMerge(IPAddressRange left, IPAddressRange right, out IPAddressRange mergedRange)
        {
            if (left == null || right == null || left.AddressFamily != right.AddressFamily)
            {
                mergedRange = null;
                return false;
            }

            // overlap or touch occurs
            if (left.Overlaps(right) || left.Touches(right))
            {
                var newHead = IPAddressMath.Min(left.Head, right.Head);
                var newTail = IPAddressMath.Max(left.Tail, right.Tail);
                mergedRange = new IPAddressRange(newHead, newTail);

                return true;
            }

            mergedRange = null;
            return false;
        }

        #endregion // end: static methods
    }
}
