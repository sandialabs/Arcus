using System.Numerics;
using Arcus.Math;
using NUnit.Framework;

namespace Arcus.Tests.Math
{
    [TestFixture]
    public class BigIntegerMathTests
    {
        [TestCase(5, 0, 10, true, ExpectedResult = true)]
        [TestCase(5, 0, 10, false, ExpectedResult = true)]
        [TestCase(0, 0, 10, true, ExpectedResult = true)]
        [TestCase(0, 0, 10, false, ExpectedResult = false)]
        [TestCase(10, 0, 10, true, ExpectedResult = true)]
        [TestCase(10, 0, 10, false, ExpectedResult = false)]
        [TestCase(-10, 0, 10, true, ExpectedResult = false)]
        [TestCase(-10, 0, 10, false, ExpectedResult = false)]
        [TestCase(20, 0, 10, true, ExpectedResult = false)]
        [TestCase(20, 0, 10, false, ExpectedResult = false)]
        public bool BetweenTest(int num,
                                int lower,
                                int upper,
                                bool inclusive) => new BigInteger(num).Between(new BigInteger(lower), new BigInteger(upper), inclusive);
    }
}
