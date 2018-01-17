using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Arcus.Utilities
{
    /// <summary>
    ///     Static utility class containing miscellaneous operations for generic <see cref="IEnumerable{T}" />
    /// </summary>
    public static class EnumerableUtilities
    {
        /// <summary>
        ///     Reverse based on predicate <see langword="bool" /> value
        /// </summary>
        /// <param name="input">the input to possibly reverse</param>
        /// <param name="reversePredicate">predicate for reversal</param>
        /// <returns>reverse the given input if the predicate is true, otherwise return the original value</returns>
        [NotNull]
        [LinqTunnel]
        public static IEnumerable<T> ReverseIf<T>([CanBeNull] [NoEnumeration] this IEnumerable<T> input,
                                                  bool reversePredicate)
        {
            return input == null
                       ? Enumerable.Empty<T>()
                       : (reversePredicate
                              ? input.Reverse()
                              : input);
        }

        /// <summary>
        ///     Reverse based on predicate
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input">the input to possibly reverse</param>
        /// <param name="predicate">predicate for reversal</param>
        /// <returns>reverse the given input if the predicate is true, otherwise return the original value</returns>
        [NotNull]
        [LinqTunnel]
        public static IEnumerable<T> ReverseIf<T>([CanBeNull] [NoEnumeration] this IEnumerable<T> input
            , Func<T, bool> predicate)
        {
            return input == null 
                ? Enumerable.Empty<T>()
                : ReverseIf(input, input.Any(predicate));
        }

    }
}
