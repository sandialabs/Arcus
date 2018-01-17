using System;
using Arcus.Converters;
using Arcus.Utilities;
using NUnit.Framework;

namespace Arcus.Tests.Converters
{
    [TestFixture]
    public class ByteArrayConvertersTests
    {
        [TestCase(new byte[] {0xaa, 0xbb, 0x01, 0x10, 0xf0}, ExpectedResult = "AABB0110F0")]
        [TestCase(new byte[] {0xaa}, ExpectedResult = "AA")]
        [TestCase(new byte[] {0x00}, ExpectedResult = "00")]
        [TestCase(new byte[] {0x00, 0x00}, ExpectedResult = "0000")]
        [TestCase(new byte[] {}, ExpectedResult = "")]
        public string ToHexStringTest(byte[] input)
        {
            // Act
            var result = input.ToHexString();

            // Assert
            return result;
        }

        [TestCase(new byte[] {0xf0}, false, ExpectedResult = "-16", TestName = "ToBigIntegerTest BE 0xf0")]
        [TestCase(new byte[] {0x0f}, false, ExpectedResult = "15", TestName = "ToBigIntegerTest BE 0x0f")]
        [TestCase(new byte[] {0xff, 0xaa, 0xbb, 0x01, 0x10, 0xf0}, false, ExpectedResult = "-17523437491457", TestName = "ToBigIntegerTest BE 0xff, 0xaa, 0xbb, 0x01, 0x10, 0xf0")]
        [TestCase(new byte[] {0xaa, 0xbb, 0x01, 0x10, 0xf0, 0xff}, false, ExpectedResult = "-68450927702", TestName = "ToBigIntegerTest BE 0xaa, 0xbb, 0x01, 0x10, 0xf0, 0xff")]
        [TestCase(new byte[] {0xaa, 0xbb, 0x01, 0x10, 0xf0}, false, ExpectedResult = "-68450927702", TestName = "ToBigIntegerTest BE 0xaa, 0xbb, 0x01, 0x10, 0xf0 ")]
        [TestCase(new byte[] {0xaa}, false, ExpectedResult = "-86", TestName = "ToBigIntegerTest BE 0xaa")]
        [TestCase(new byte[] {}, false, ExpectedResult = "0", TestName = "ToBigIntegerTest BE []")]
        [TestCase(new byte[] {0xf0}, true, ExpectedResult = "-16", TestName = "ToBigIntegerTest 0xf0")]
        [TestCase(new byte[] {0x0f}, true, ExpectedResult = "15", TestName = "ToBigIntegerTest 0x0f")]
        [TestCase(new byte[] {0xff, 0xaa, 0xbb, 0x01, 0x10, 0xf0}, true, ExpectedResult = "-366229778192", TestName = "ToBigIntegerTest 0xff, 0xaa, 0xbb, 0x01, 0x10, 0xf0  ")]
        [TestCase(new byte[] {0xaa, 0xbb, 0x01, 0x10, 0xf0, 0xff}, true, ExpectedResult = "-93754823216897", TestName = "ToBigIntegerTest  0xaa, 0xbb, 0x01, 0x10, 0xf0, 0xff")]
        [TestCase(new byte[] {0xaa, 0xbb, 0x01, 0x10, 0xf0}, true, ExpectedResult = "-366229778192", TestName = "ToBigIntegerTest 0xaa, 0xbb, 0x01, 0x10, 0xf0")]
        [TestCase(new byte[] {0xaa}, true, ExpectedResult = "-86", TestName = "ToBigIntegerTest 0xaa")]
        [TestCase(new byte[] {}, true, ExpectedResult = "0", TestName = "ToBigIntegerTest []")]
        public string ToBigIntegerTest(byte[] input,
                                       bool littleEndian)
        {
            // Act
            var result = input.ToBigInteger(littleEndian);

            // Assert
            return result.ToString();
        }

        [TestCase(new byte[] {0xf0}, false, ExpectedResult = "240", TestName = "ToUnsignedBigIntegerTest BE 0xf0")]
        [TestCase(new byte[] {0x0f}, false, ExpectedResult = "15", TestName = "ToUnsignedBigIntegerTest BE 0x0f")]
        [TestCase(new byte[] {0xff, 0xaa, 0xbb, 0x01, 0x10, 0xf0}, false, ExpectedResult = "263951539219199", TestName = "ToUnsignedBigIntegerTest BE 0xff, 0xaa, 0xbb, 0x01, 0x10, 0xf0")]
        [TestCase(new byte[] {0xaa, 0xbb, 0x01, 0x10, 0xf0, 0xff}, false, ExpectedResult = "281406525782954", TestName = "ToUnsignedBigIntegerTest BE 0xaa, 0xbb, 0x01, 0x10, 0xf0, 0xff")]
        [TestCase(new byte[] {0xaa, 0xbb, 0x01, 0x10, 0xf0}, false, ExpectedResult = "1031060700074", TestName = "ToUnsignedBigIntegerTest BE 0xaa, 0xbb, 0x01, 0x10, 0xf0 ")]
        [TestCase(new byte[] {0xaa}, false, ExpectedResult = "170", TestName = "ToUnsignedBigIntegerTest BE 0xaa")]
        [TestCase(new byte[] {}, false, ExpectedResult = "0", TestName = "ToUnsignedBigIntegerTest BE []")]
        [TestCase(new byte[] {0xf0}, true, ExpectedResult = "240", TestName = "ToUnsignedBigIntegerTest 0xf0")]
        [TestCase(new byte[] {0x0f}, true, ExpectedResult = "15", TestName = "ToUnsignedBigIntegerTest 0x0f")]
        [TestCase(new byte[] {0xff, 0xaa, 0xbb, 0x01, 0x10, 0xf0}, true, ExpectedResult = "281108746932464", TestName = "ToUnsignedBigIntegerTest 0xff, 0xaa, 0xbb, 0x01, 0x10, 0xf0  ")]
        [TestCase(new byte[] {0xaa, 0xbb, 0x01, 0x10, 0xf0, 0xff}, true, ExpectedResult = "187720153493759", TestName = "ToUnsignedBigIntegerTest  0xaa, 0xbb, 0x01, 0x10, 0xf0, 0xff")]
        [TestCase(new byte[] {0xaa, 0xbb, 0x01, 0x10, 0xf0}, true, ExpectedResult = "733281849584", TestName = "ToUnsignedBigIntegerTest 0xaa, 0xbb, 0x01, 0x10, 0xf0")]
        [TestCase(new byte[] {0xaa}, true, ExpectedResult = "170", TestName = "ToUnsignedBigIntegerTest 0xaa")]
        [TestCase(new byte[] {}, true, ExpectedResult = "0", TestName = "ToUnsignedBigIntegerTest []")]
        public string ToUnsignedBigInteger(byte[] input,
                                           bool littleEndian)
        {
            // Act
            var result = input.ToUnsignedBigInteger(littleEndian);

            // Assert
            return result.ToString();
        }

        [TestCase(new byte[] {}, 0x00, ExpectedResult = "")]
        [TestCase(new byte[] {0xab, 0xbc}, 0x00, ExpectedResult = "0000")]
        [TestCase(new byte[] {0xab, 0xbc}, 0x0F, ExpectedResult = "0F0F")]
        [TestCase(new byte[] {0xab, 0xbc}, 0xF0, ExpectedResult = "F0F0")]
        [TestCase(new byte[] {0xab, 0xbc}, 0xFF, ExpectedResult = "FFFF")]
        public string FillByteArrayTest(byte[] input,
                                        byte fillValue)
        {
            // Act
            var result = input.FillByteArray(fillValue);

            // Assert
            return result.ToHexString();
        }

        [Test]
        public void ToBigIntegerNullInputTest()
        {
            Assert.Throws<ArgumentNullException>(() => ((byte[]) null).ToBigInteger());
        }

        [Test]
        public void ToHexStringNullInputTest()
        {
            Assert.Throws<ArgumentNullException>(() => ((byte[]) null).ToHexString());
        }

        [Test]
        public void ToUnsignedBigIntegerNullInputTest()
        {
            Assert.Throws<ArgumentNullException>(() => ((byte[]) null).ToUnsignedBigInteger());
        }
    }
}
