using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Numerics;
using Arcus.Comparers;
using Arcus.Converters;
using Arcus.Math;
using JetBrains.Annotations;

namespace Arcus
{
    /// <summary>
    ///     A basic implementation of a IIPAddressRange us to represent an inclusive range of arbitrary IP Addresses of the same address family
    /// </summary>
    public class IPAddressRange : AbstractIPAddressRange,
                                  IEquatable<IPAddressRange>,
                                  IComparable<IPAddressRange>

    {
        private IPAddress _head;    // the lowerst value IP address in the range
        private IPAddress _tail;    // the highest valud IP address in the range

        /// <summary>
        ///     Initializes a new instance of the <see cref="T:System.Object" /> class.
        /// </summary>
        public IPAddressRange() {}

        /// <summary>
        ///     Initializes a new instance of the <see cref="T:System.Object" /> class.
        /// </summary>
        /// <param name="address"></param>
        public IPAddressRange(IPAddress address)
            : this(address, address)
        {
            // nothing more to do
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="T:System.Object" /> class.
        /// </summary>
        public IPAddressRange(IPAddress head,
                              IPAddress tail)
            : this()
        {
            this.Head = head;
            this.Tail = tail;
        }

        /// <summary>
        ///     Tests that
        ///     -head and tail are non <see langword="null" />
        ///     -address families are the same
        ///     -tail is equal to, or after head
        /// </summary>
        protected internal bool IsValid => this.Head != null
                                           && this.Tail != null
                                           && this.Head.AddressFamily == this.Tail.AddressFamily
                                           && this.Tail.IsGreaterThanOrEqualTo(this.Head);

        /// <summary>
        ///     Length of range
        ///     <returns>if the range is invalid the result witll be 0, otherwise it will be the length of the range</returns>
        /// </summary>
        public override BigInteger Length => this.IsValid 
                                                 ? this.Tail.ToUnsignedBigInteger() - this.Head.ToUnsignedBigInteger()
                                                 : 0;

        /// <summary>
        ///     Head of range
        /// </summary>
        /// <exception cref="InvalidOperationException" accessor="set">on set Address family must match</exception>
        /// <exception cref="InvalidOperationException" accessor="set">on set Head must be less than tail</exception>
        [CanBeNull]
        public sealed override IPAddress Head
        {
            get { return this._head; }
            set
            {
                if (value != null
                    && this.Tail != null)
                {
                    if (value.AddressFamily != this.Tail.AddressFamily)
                    {
                        throw new InvalidOperationException("on set Address family must match");
                    }

                    if (value.IsGreaterThan(this.Tail))
                    {
                        throw new InvalidOperationException("on set Head must be less than tail");
                    }
                }
                this._head = value;
            }
        }

        /// <summary>
        ///     Tail of range
        /// </summary>
        /// <exception cref="InvalidOperationException" accessor="set">on set Address family must match</exception>
        /// <exception cref="InvalidOperationException" accessor="set">on set Tail must be greater than head</exception>
        [CanBeNull]
        public sealed override IPAddress Tail
        {
            get { return this._tail; }
            set
            {
                if (value != null
                    && this.Head != null)
                {
                    if (value.AddressFamily != this.Head.AddressFamily)
                    {
                        throw new InvalidOperationException("Address family must match");
                    }

                    if (value.IsLessThan(this.Head))
                    {
                        throw new InvalidOperationException("Tail must be greater than head");
                    }
                }

                this._tail = value;
            }
        }

        #region From Interface IComparable<IPAddressRange>

        /// <summary>
        ///     Compares the current object with another object of the same type.
        /// </summary>
        /// <returns>
        ///     A value that indicates the relative order of the objects being compared. The return value has the following
        ///     meanings: Value Meaning Less than zero This object is less than the <paramref name="other" /> parameter.Zero This
        ///     object is equal to <paramref name="other" />. Greater than zero This object is greater than
        ///     <paramref name="other" />.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public int CompareTo([CanBeNull] IPAddressRange other) => new DefaultIPAddressRangeComparer().Compare(this, other);

        #endregion

        #region From Interface IEquatable<IPAddressRange>

        /// <summary>
        ///     Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        ///     <see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise,
        ///     false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(IPAddressRange other)
        {
            return !ReferenceEquals(null, other)
                   && (ReferenceEquals(this, other)
                       || Equals(this.Head, other.Head)
                       && Equals(this.Tail, other.Tail));
        }

        #endregion

        /// <summary>
        ///     Attempt collapse the given input of ranges into fewer ranges thus optimizing
        ///     Ranges that overlap, or butt against each other may be collapsed into a single range
        /// </summary>
        /// <param name="ranges">ranges to collapse</param>
        /// <param name="result">resulting ranges post collapse</param>
        /// <returns></returns>
        public static bool TryCollapseAll([CanBeNull] [ItemNotNull] IEnumerable<IPAddressRange> ranges,
                                          out IList<IPAddressRange> result)
        {
            var rangeList = (ranges ?? Enumerable.Empty<IPAddressRange>()).ToList();

            if (!rangeList.Any()) // no ranges
            {
                result = Enumerable.Empty<IPAddressRange>()
                                   .ToList();
                return true; // assume success
            }

            if (rangeList.Any(r => !r.IsValid) // one or more invalid ranges
                || rangeList.Any(r => !(r.IsIPv4 || r.IsIPv6)) // all using IPv4 or IPv6
                || rangeList.Any(r => r.AddressFamily != rangeList.First()
                                                                  .AddressFamily)) // and all families match
            {
                result = Enumerable.Empty<IPAddressRange>()
                                   .ToList();
                return false;
            }

            // sort range list, has to be done post validation check, as invalid cannot be sorted
            rangeList = rangeList.OrderBy(r => r)
                                 .ToList();

            result = new List<IPAddressRange>
                     {
                         rangeList.First() // start with first item in the sorted list of ranges
                     };

            // iterate over items in range list, no need to iterate over first as it is included by default
            foreach (var range in rangeList.Skip(1))
            {
                var last = result.Last(); // take end of result to process over (is first in first iteration)
                // can be assumed that assume that the range head is greater than or equal to last head because everything is ordered

                // if TryMerge succeeded then overlap exists, merge overlap and re-assign to end of results
                IPAddressRange merge;
                if (TryMerge(last, range, out merge))
                {
                    result[result.Count - 1] = merge; // overwrite with merged values
                    continue;
                }

                // could not merge, simply add to end and iterate to next
                result.Add(range);
            }

            return true;
        }

        /// <summary>
        ///     Rebuild the initial range as an <see cref="IList{T}" /> of ranges excluding the excluded ranges
        ///     excluded ranges are expected to each be sub ranges of the initial range
        ///     an attempt will be made to TryCollapseAll the excluded
        /// </summary>
        /// <param name="initialRange"></param>
        /// <param name="excludedRanges"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">unexpected invalid operation.</exception>
        public static bool TryExcludeAll([NotNull] IPAddressRange initialRange,
                                         [CanBeNull] [ItemNotNull] IEnumerable<IPAddressRange> excludedRanges,
                                         out IList<IPAddressRange> result)
        {
            var excludedRangesList = (excludedRanges ?? Enumerable.Empty<IPAddressRange>()).ToList();

            if (!excludedRangesList.Any())
            {
                result = new List<IPAddressRange>
                         {
                             new IPAddressRange(initialRange.Head, initialRange.Tail)
                         };
                return true;
            }

            if (excludedRangesList.Any(r => !r.IsValid) // one or more invalid ranges
                || excludedRangesList.Any(r => !(r.IsIPv4 || r.IsIPv6)) // all using IPv4 or IPv6
                || excludedRangesList.Any(r => r.AddressFamily != initialRange.AddressFamily)) // and all families match
            {
                result = Enumerable.Empty<IPAddressRange>()
                                   .ToList();
                return false;
            }

            IList<IPAddressRange> exclusionList;
            if (!TryCollapseAll(excludedRangesList, out exclusionList) // don't attempt if not collapsible
                || !exclusionList.All(initialRange.Contains)) // don't attempt if initial range doesn't contain all excluded ranges
            {
                result = Enumerable.Empty<IPAddressRange>()
                                   .ToList();
                return false;
            }

            // results is initialized with a *copy* of initialRange
            result = new List<IPAddressRange>
                     {
                         new IPAddressRange(initialRange.Head, initialRange.Tail)
                     };

            foreach (var exclusion in exclusionList)
            {
                var last = result.Last();

                // exclusion has tail, remove last element from result
                //if (exclusion.Contains(initialRange.Tail))
                //{
                //    result.RemoveAt(result.Count - 1);
                //    break; // can't loop any more
                //}
                if (exclusion.Contains(last))
                {
                    result.RemoveAt(result.Count - 1);
                    break; // can't loop any more
                }

                if (exclusion.Contains(initialRange.Tail))
                {
                    Debug.Assert(exclusion.Head != null, "exclusion.Head != null");
                    result[result.Count - 1].Tail = exclusion.Head.Increment(-1);
                    continue; // can't loop any more
                }

                // exclusion contains head
                if (exclusion.Contains(last.Head))
                {
                    // push head one point beyond tail of exclusion 
                    Debug.Assert(exclusion.Tail != null, "exclusion.Tail != null");
                    result[result.Count - 1].Head = exclusion.Tail.Increment();
                    continue; // next iteration
                }

                // exclusion is within last, carve exclusion to two pieces
                if (!last.Overlaps(exclusion))
                {
                    throw new InvalidOperationException(); // should not have made it here
                }

                Debug.Assert(exclusion.Head != null, "exclusion.Head != null");
                result[result.Count - 1].Tail = exclusion.Head.Increment(-1);
                // carve out a piece at starting at exclusion head, ending at exclusion tail
                // thus meaning one less than head at previous, and one more at head at next
                Debug.Assert(exclusion.Tail != null, "exclusion.Tail != null");
                result.Add(new IPAddressRange(exclusion.Tail.Increment(), initialRange.Tail));
            }

            return true;
        }

        /// <summary>
        ///     Merge two touching or overlapiing address ranges
        /// </summary>
        /// <param name="alpha"></param>
        /// <param name="beta"></param>
        /// <param name="mergedRange"></param>
        /// <returns></returns>
        public static bool TryMerge([CanBeNull] IPAddressRange alpha,
                                    [CanBeNull] IPAddressRange beta,
                                    out IPAddressRange mergedRange)
        {
            if (alpha == null
                || beta == null
                || !alpha.IsValid
                || !beta.IsValid
                || !alpha.AddressFamily.Equals(beta.AddressFamily))
            {
                mergedRange = null;
                return false;
            }

            if (alpha.Overlaps(beta)
                || alpha.Touches(beta))
            {
                Debug.Assert(alpha.Head != null, "alpha.Head != null");
                Debug.Assert(beta.Head != null, "beta.Head != null");
                var newHead = IPAddressMath.Min(alpha.Head, beta.Head);

                Debug.Assert(alpha.Tail != null, "alpha.Tail != null");
                Debug.Assert(beta.Tail != null, "beta.Tail != null");
                var newTail = IPAddressMath.Max(alpha.Tail, beta.Tail);
                mergedRange = new IPAddressRange(newHead, newTail);
                return true;
            }

            mergedRange = null;
            return false;
        }

        /// <summary>
        ///     Determines whether the specified <see cref="T:System.Object" /> is equal to the current
        ///     <see cref="T:System.Object" />.
        /// </summary>
        /// <returns>
        ///     <see langword="true" /> if the specified <see cref="T:System.Object" /> is equal to the current
        ///     <see cref="T:System.Object" />;
        ///     otherwise, false.
        /// </returns>
        /// <param name="obj">The <see cref="T:System.Object" /> to compare with the current <see cref="T:System.Object" />. </param>
        public override bool Equals(object obj)
        {
            return !ReferenceEquals(null, obj)
                   && (ReferenceEquals(this, obj) || obj.GetType() == GetType()
                       && this.Equals((IPAddressRange) obj));
        }

        /// <summary>
        ///     Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        ///     A hash code for the current <see cref="T:System.Object" />.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return ((this.Head?.GetHashCode() ?? 0) * 397) ^ (this.Tail?.GetHashCode() ?? 0);
            }
        }

        /// <summary>
        ///     Return <see langword="true" /> if the head of this is within the range of that
        /// </summary>
        /// <param name="that"></param>
        /// <returns></returns>
        public bool HeadOverlappedBy([CanBeNull] IPAddressRange that)
        {
            return this.Head != null
                   && that != null
                   && (this.Head.IsEqualTo(that.Head)
                       || this.Head.IsEqualTo(that.Tail)
                       || that.Head != null
                       && that.Tail != null
                       && that.Contains(this.Head));
        }

        /// <summary>
        ///     return <see langword="true" /> if this overlaps that (totally contained, or contains either the head or the tail)
        /// </summary>
        /// <param name="that"></param>
        /// <returns></returns>
        public bool Overlaps([CanBeNull] IPAddressRange that)
        {
            return that != null && (this.Head.IsLessThanOrEqualTo(that.Head)
                                    && this.Tail.IsGreaterThanOrEqualTo(that.Tail) || Contains(that.Head) || Contains(that.Tail));
        }

        /// <summary>
        ///     Return <see langword="true" /> if the tail of this is within the range of that
        /// </summary>
        /// <param name="that"></param>
        /// <returns></returns>
        public bool TailOverlappedBy([CanBeNull] IPAddressRange that)
        {
            return this.Tail != null
                   && that != null
                   && (this.Tail.IsEqualTo(that.Head)
                       || this.Tail.IsEqualTo(that.Tail)
                       || that.Head != null
                       && that.Tail != null
                       && that.Contains(this.Tail));
        }

        /// <summary>
        ///     Returns a <see cref="T:System.String" /> that represents the current <see cref="T:System.Object" />.
        /// </summary>
        /// <returns>
        ///     A <see cref="T:System.String" /> that represents the current <see cref="T:System.Object" />.
        /// </returns>
        public override string ToString()
            => string.Format("IPAddress Range: Head: {0}, Tail: {1}", this.Head, this.Tail);

        /// <summary>
        ///     <see langword="true" /> if tail of one item is consecutively in order of head of other item
        /// </summary>
        /// <param name="that"></param>
        /// <returns></returns>
        private bool Touches([CanBeNull] IPAddressRange that)
        {
            return that != null
                   && ((this.Tail != null && Equals(this.Tail.Increment(), that.Head))
                       || (that.Tail != null && Equals(that.Tail.Increment(), this.Head)));
        }
    }
}
