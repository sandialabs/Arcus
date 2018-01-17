using System.Numerics;

namespace Arcus.Math
{
    /// <summary>
    ///     Static utility class containing mathematical methods for <see cref="BigInteger" /> objects
    /// </summary>
    public static class BigIntegerMath
    {
        /// <summary>
        ///     Big Integer method to check if the given big integer value is between the other two (with optional
        ///     <paramref name="inclusive" />)
        /// </summary>
        /// <param name="num">BigInteger to test</param>
        /// <param name="lower">upper bounds</param>
        /// <param name="upper">lower bounds</param>
        /// <param name="inclusive">set to true if bounds are inclusive (default false)</param>
        /// <returns></returns>
        public static bool Between(this BigInteger num,
                                   BigInteger lower,
                                   BigInteger upper,
                                   bool inclusive = false) => inclusive
                                                                  ? lower <= num && num <= upper
                                                                  : lower < num && num < upper;
    }
}
