using Arcus.Math;
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

namespace Arcus
{
    /// <content>
    ///     <see cref="IPAddressRange"/> static factory methods
    /// </content>
    public partial class IPAddressRange
    {
        #region static methods

        /// <summary>
        ///     Attempts to collapse overlapping or adjacent ranges into the minimum number of contiguous ranges.
        /// </summary>
        /// <param name="ranges">The ranges to collapse.</param>
        /// <param name="result">The collapsed ranges.</param>
        /// <param name="maxEnumerationExponent">The maximum enumeration exponent (0-128).</param>
        /// <returns><see langword="true" /> if the ranges were collapsed successfully.</returns>
        public static bool TryCollapseAll(
            IEnumerable<IPAddressRange> ranges,
            out IEnumerable<IPAddressRange> result,
            int maxEnumerationExponent = DefaultMaxEnumerationExponent
        )
        {
            if (ranges is null)
            {
                result = [];
                return true;
            }

            var rangeList = ranges.ToList();

            // item null check
            if (rangeList.Contains(null))
            {
                result = [];
                return false;
            }

            // no ranges provided
            if (rangeList.Count == 0)
            {
                result = [];
                return true; // assume success
            }

            // all families don't match match
            if (rangeList.Any(range => range.AddressFamily != rangeList[0].AddressFamily))
            {
                result = [];
                return false;
            }

            // sort range list, has to be done post validation check, as invalid cannot be sorted
            rangeList.Sort();

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
                if (TryMerge(last, range, out var merge, maxEnumerationExponent))
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
        ///     Rebuild the initial range as an <see cref="IEnumerable{IPAddressRange}" /> of ranges excluding the excluded ranges.
        ///     Excluded ranges are expected to each be sub-ranges of the initial range.
        /// </summary>
        /// <param name="initialRange">the initial <see cref="IPAddressRange" /> to exclude from</param>
        /// <param name="excludedRanges">
        ///     the various <see cref="IPAddressRange" /> to exclude from the
        ///     <paramref name="initialRange" />
        /// </param>
        /// <param name="result">the resulting  <see cref="IPAddressRange" /> <see cref="IEnumerable{IPAddressRange}" /></param>
        /// <param name="maxEnumerationExponent">the maximum enumeration exponent</param>
        /// <returns>true on success</returns>
        /// <remarks>
        ///     <para>
        ///         A return value of <see langword="false" /> indicates an error condition (null input, address-family mismatch,
        ///         or null elements in <paramref name="excludedRanges" />), not an empty result set. A <see langword="true" />
        ///         return with an empty <paramref name="result" /> means the exclusions cover the entire
        ///         <paramref name="initialRange" />.
        ///     </para>
        ///     <para>
        ///         <b>Family-maximum boundary:</b> when an exclusion ends at the family maximum address
        ///         (e.g., <c>255.255.255.255</c> for IPv4 or <c>ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff</c> for IPv6),
        ///         no trailing segment can be produced after it. The method returns <see langword="true" /> with only the
        ///         leading segment in <paramref name="result" /> (which may itself be empty if the exclusion also covers
        ///         the start of the range).
        ///     </para>
        ///     <para>
        ///         <b>Family-minimum boundary:</b> when an exclusion starts at the family minimum address
        ///         (e.g., <c>0.0.0.0</c> for IPv4 or <c>::</c> for IPv6), no leading segment can be produced before it.
        ///         The method returns <see langword="true" /> with only the trailing segment in <paramref name="result" />
        ///         (which may itself be empty if the exclusion also covers the end of the range).
        ///     </para>
        ///     <para>
        ///         A non-overlapping exclusion in <paramref name="excludedRanges" /> causes the method
        ///         to return <see langword="false" /> rather than throwing.
        ///     </para>
        /// </remarks>
        public static bool TryExcludeAll(
            IPAddressRange initialRange,
            IEnumerable<IPAddressRange> excludedRanges,
            out IEnumerable<IPAddressRange> result,
            int maxEnumerationExponent = DefaultMaxEnumerationExponent
        )
        {
            if (initialRange is null || excludedRanges is null)
            {
                result = [];
                return false;
            }

            // Materialize once: copying ensures we do not sort the caller's original collection.
            var excludedList = excludedRanges.ToList();

            if (excludedList.Any(range => range is null || range.AddressFamily != initialRange.AddressFamily))
            {
                result = [];
                return false;
            }

            if (excludedList.Count == 0)
            {
                result = [new IPAddressRange(initialRange.Head, initialRange.Tail, maxEnumerationExponent)];
                return true;
            }

            var resultList = new List<IPAddressRange> { new(initialRange.Head, initialRange.Tail, maxEnumerationExponent) };

            // Exclusions are processed in ascending order. Because each exclusion is to the right of
            // all previous ones, it can only ever affect the rightmost not-yet-trimmed segment -
            // earlier segments are entirely left of the current exclusion and are permanently settled.
            excludedList.Sort();
            foreach (var exclusion in excludedList)
            {
                var lastIndex = resultList.Count - 1;
                var (error, done, segments) = ApplyExclusion(resultList[lastIndex], exclusion);

                if (error)
                {
                    result = [];
                    return false;
                }

                resultList.RemoveAt(lastIndex);
                resultList.AddRange(segments);

                // done=true means the exclusion consumed the segment's tail boundary. Any remaining
                // exclusions start at or after the current one's head, so they cannot produce
                // additional output - short-circuit to avoid redundant work.
                if (done)
                {
                    break;
                }
            }

            result = resultList;
            return true;

            (bool Error, bool Done, IPAddressRange[] Segments) ApplyExclusion(IPAddressRange segment, IPAddressRange exclusion)
            {
                // exclusion covers the entire segment
                if (exclusion.Contains(segment))
                {
                    return (false, true, []);
                }

                // exclusion covers the tail; no trailing portion can exist after this point
                if (exclusion.Contains(segment.Tail))
                {
                    return (false, true, LeadingFragment(segment, exclusion));
                }

                // exclusion covers the head; advance the segment's head past the exclusion's tail
                if (exclusion.Contains(segment.Head))
                {
                    var trailing = TrailingFragment(segment, exclusion);
                    return (false, trailing.Length == 0, trailing);
                }

                // Non-overlapping exclusion: signal an error to the caller instead of throwing.
                // Per the documented contract, an exclusion that does not intersect the initial range
                // causes TryExcludeAll to return false rather than failing at runtime.
                if (!segment.Overlaps(exclusion))
                {
                    return (true, false, []);
                }

                // Strictly interior exclusion: split into leading and trailing pieces.
                // The collection-expression spread is bounded: at most one leading and one trailing
                // fragment (<=2 elements), so there is no unbounded allocation concern here.
                return (false, false, [.. LeadingFragment(segment, exclusion), .. TrailingFragment(segment, exclusion)]);
            }

            // The boundary guards IsAtMin() and IsAtMax() below must remain synchronized with
            // IPAddressMath.Increment's underflow/overflow detection: when the exclusion's head
            // sits at the family minimum (or its tail sits at the family maximum), the
            // Increment(-1)/Increment() step would otherwise underflow/overflow and produce
            // an out-of-range address. The guards short-circuit to an empty fragment instead.
            IPAddressRange[] LeadingFragment(IPAddressRange segment, IPAddressRange exclusion) =>
                exclusion.Head.IsAtMin()
                    ? []
                    : [new IPAddressRange(segment.Head, exclusion.Head.Increment(-1), maxEnumerationExponent)];

            IPAddressRange[] TrailingFragment(IPAddressRange segment, IPAddressRange exclusion) =>
                exclusion.Tail.IsAtMax()
                    ? []
                    : [new IPAddressRange(exclusion.Tail.Increment(), segment.Tail, maxEnumerationExponent)];
        }

        /// <summary>
        ///     Attempts to merge two touching or overlapping address ranges into a single range.
        /// </summary>
        /// <param name="left">The left range.</param>
        /// <param name="right">The right range.</param>
        /// <param name="mergedRange">The merged <see cref="IPAddressRange" /> on success.</param>
        /// <param name="maxEnumerationExponent">The maximum enumeration exponent (0-128).</param>
        /// <returns><see langword="true" /> if the ranges were merged successfully.</returns>
        public static bool TryMerge(
            IPAddressRange left,
            IPAddressRange right,
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
            [NotNullWhen(true)]
#endif
            out IPAddressRange mergedRange,
            int maxEnumerationExponent = DefaultMaxEnumerationExponent
        )
        {
            if (left is null || right is null || left.AddressFamily != right.AddressFamily)
            {
                mergedRange = null;
                return false;
            }

            // overlap or touch occurs
            if (left.Overlaps(right) || left.Touches(right))
            {
                var newHead = IPAddressMath.Min(left.Head, right.Head);
                var newTail = IPAddressMath.Max(left.Tail, right.Tail);
                mergedRange = new IPAddressRange(newHead, newTail, maxEnumerationExponent);

                return true;
            }

            mergedRange = null;
            return false;
        }

        #endregion // end: static methods
    }
}
