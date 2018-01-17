using System.Numerics;
using Arcus.Converters;
using NUnit.Framework;

namespace Arcus.Tests.Converters
{
    [TestFixture]
    public class BigIntegerConvertersTests
    {
        [TestCase(0, ExpectedResult = "0")]
        [TestCase(1, ExpectedResult = "01")]
        [TestCase(2, ExpectedResult = "010")]
        [TestCase(3, ExpectedResult = "011")]
        [TestCase(4, ExpectedResult = "0100")]
        [TestCase(42, ExpectedResult = "0101010")]
        [TestCase(100, ExpectedResult = "01100100")]
        [TestCase(1000, ExpectedResult = "01111101000")]
        [TestCase(int.MaxValue, ExpectedResult = "01111111111111111111111111111111")]
        public string ToBinaryString(int input) => new BigInteger(input).ToBinaryString();

        [TestCase(0, ExpectedResult = "0x00")]
        [TestCase(1, ExpectedResult = "0x01")]
        [TestCase(2, ExpectedResult = "0x02")]
        [TestCase(3, ExpectedResult = "0x03")]
        [TestCase(4, ExpectedResult = "0x04")]
        [TestCase(42, ExpectedResult = "0x2A")]
        [TestCase(100, ExpectedResult = "0x64")]
        [TestCase(1000, ExpectedResult = "0x03E8")]
        [TestCase(int.MaxValue, ExpectedResult = "0x7FFFFFFF")]
        public string ToHexString(int input) => new BigInteger(input).ToHexString();

        [TestCase(0, ExpectedResult = "0")]
        [TestCase(1, ExpectedResult = "01")]
        [TestCase(2, ExpectedResult = "02")]
        [TestCase(3, ExpectedResult = "03")]
        [TestCase(4, ExpectedResult = "04")]
        [TestCase(5, ExpectedResult = "05")]
        [TestCase(6, ExpectedResult = "06")]
        [TestCase(7, ExpectedResult = "07")]
        [TestCase(8, ExpectedResult = "010")]
        [TestCase(9, ExpectedResult = "011")]
        [TestCase(10, ExpectedResult = "012")]
        [TestCase(11, ExpectedResult = "013")]
        [TestCase(12, ExpectedResult = "014")]
        [TestCase(13, ExpectedResult = "015")]
        [TestCase(14, ExpectedResult = "016")]
        [TestCase(15, ExpectedResult = "017")]
        [TestCase(16, ExpectedResult = "020")]
        [TestCase(17, ExpectedResult = "021")]
        [TestCase(42, ExpectedResult = "052")]
        [TestCase(100, ExpectedResult = "0144")]
        [TestCase(1000, ExpectedResult = "01750")]
        [TestCase(2097151, ExpectedResult = "07777777")]
        [TestCase(16777215, ExpectedResult = "077777777")]
        [TestCase(134217727, ExpectedResult = "0777777777")]
        [TestCase(int.MaxValue, ExpectedResult = "017777777777")]
        public string ToOctalString(int input) => new BigInteger(input).ToOctalString();
    }
}
