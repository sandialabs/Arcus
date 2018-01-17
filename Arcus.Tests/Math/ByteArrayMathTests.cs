using System;
using System.Linq;
using Arcus.Converters;
using Arcus.Math;
using NUnit.Framework;

namespace Arcus.Tests.Math
{
    [TestFixture]
    public class ByteArrayMathTests
    {
        [TestCase(new byte[] {}, new byte[] {}, ExpectedResult = "")]
        [TestCase(new byte[] {0x00}, new byte[] {0x00}, ExpectedResult = "00")]
        [TestCase(new byte[] {0x00, 0x00}, new byte[] {0x00, 0x00}, ExpectedResult = "0000")]
        [TestCase(new byte[] {0x00}, new byte[] {0x00, 0xFF}, ExpectedResult = "00FF")]
        [TestCase(new byte[] {0x00}, new byte[] {0xFF, 0x00}, ExpectedResult = "FF00")]
        [TestCase(new byte[] {0x00, 0xFF}, new byte[] {0x00, 0x00}, ExpectedResult = "00FF")]
        [TestCase(new byte[] {0xFF, 0x00}, new byte[] {0x00, 0x00}, ExpectedResult = "FF00")]
        [TestCase(new byte[] {0xFF, 0x00}, new byte[] {0x00, 0x00}, ExpectedResult = "FF00")]
        [TestCase(new byte[] {0x00, 0xFF}, new byte[] {0x00, 0x00}, ExpectedResult = "00FF")]
        [TestCase(new byte[] {0xFF, 0x00}, new byte[] {0x00, 0xFF}, ExpectedResult = "00FF")]
        [TestCase(new byte[] {0x00, 0xFF}, new byte[] {0x00, 0x00}, ExpectedResult = "00FF")]
        public string MaxBigEndianSignedTest(byte[] alpha,
                                             byte[] beta)
        {
            // Act
            var result = ByteArrayMath.Max(alpha, beta, false, true);

            // Assert
            return result.ToHexString();
        }

        [TestCase(new byte[] {}, new byte[] {}, ExpectedResult = "")]
        [TestCase(new byte[] {0x00}, new byte[] {0x00}, ExpectedResult = "00")]
        [TestCase(new byte[] {0x00, 0x00}, new byte[] {0x00, 0x00}, ExpectedResult = "0000")]
        [TestCase(new byte[] {0x00}, new byte[] {0x00, 0xFF}, ExpectedResult = "00FF")]
        [TestCase(new byte[] {0x00}, new byte[] {0xFF, 0x00}, ExpectedResult = "FF00")]
        [TestCase(new byte[] {0x00, 0xFF}, new byte[] {0x00, 0x00}, ExpectedResult = "00FF")]
        [TestCase(new byte[] {0xFF, 0x00}, new byte[] {0x00, 0x00}, ExpectedResult = "FF00")]
        [TestCase(new byte[] {0xFF, 0x00}, new byte[] {0x00, 0x00}, ExpectedResult = "FF00")]
        [TestCase(new byte[] {0x00, 0xFF}, new byte[] {0x00, 0x00}, ExpectedResult = "00FF")]
        [TestCase(new byte[] {0xFF, 0x00}, new byte[] {0x00, 0xFF}, ExpectedResult = "FF00")]
        [TestCase(new byte[] {0x00, 0xFF}, new byte[] {0x00, 0x00}, ExpectedResult = "00FF")]
        public string MaxLittleEndianSignedTest(byte[] alpha,
                                                byte[] beta)
        {
            // Act
            var result = ByteArrayMath.Max(alpha, beta, true, true);

            // Assert
            return result.ToHexString();
        }

        [TestCase(new byte[] {}, new byte[] {}, ExpectedResult = "")]
        [TestCase(new byte[] {0x00}, new byte[] {0x00}, ExpectedResult = "00")]
        [TestCase(new byte[] {0x00, 0x00}, new byte[] {0x00, 0x00}, ExpectedResult = "0000")]
        [TestCase(new byte[] {0x00}, new byte[] {0x00, 0xFF}, ExpectedResult = "00")]
        [TestCase(new byte[] {0x00}, new byte[] {0xFF, 0x00}, ExpectedResult = "FF00")]
        [TestCase(new byte[] {0x00, 0xFF}, new byte[] {0x00, 0x00}, ExpectedResult = "0000")]
        [TestCase(new byte[] {0xFF, 0x00}, new byte[] {0x00, 0x00}, ExpectedResult = "FF00")]
        [TestCase(new byte[] {0xFF, 0x00}, new byte[] {0x00, 0x00}, ExpectedResult = "FF00")]
        [TestCase(new byte[] {0x00, 0xFF}, new byte[] {0x00, 0x00}, ExpectedResult = "0000")]
        [TestCase(new byte[] {0xFF, 0x00}, new byte[] {0x00, 0xFF}, ExpectedResult = "FF00")]
        [TestCase(new byte[] {0x00, 0xFF}, new byte[] {0x00, 0x00}, ExpectedResult = "0000")]
        public string MaxBigEndianUnsignedTest(byte[] alpha,
                                               byte[] beta)
        {
            // Act
            var result = ByteArrayMath.Max(alpha, beta);

            // Assert
            return result.ToHexString();
        }

        [TestCase(new byte[] {}, new byte[] {}, ExpectedResult = "")]
        [TestCase(new byte[] {0x00}, new byte[] {0x00}, ExpectedResult = "00")]
        [TestCase(new byte[] {0x00, 0x00}, new byte[] {0x00, 0x00}, ExpectedResult = "0000")]
        [TestCase(new byte[] {0x00}, new byte[] {0x00, 0xFF}, ExpectedResult = "00FF")]
        [TestCase(new byte[] {0x00}, new byte[] {0xFF, 0x00}, ExpectedResult = "00")]
        [TestCase(new byte[] {0x00, 0xFF}, new byte[] {0x00, 0x00}, ExpectedResult = "00FF")]
        [TestCase(new byte[] {0xFF, 0x00}, new byte[] {0x00, 0x00}, ExpectedResult = "0000")]
        [TestCase(new byte[] {0xFF, 0x00}, new byte[] {0x00, 0x00}, ExpectedResult = "0000")]
        [TestCase(new byte[] {0x00, 0xFF}, new byte[] {0x00, 0x00}, ExpectedResult = "00FF")]
        [TestCase(new byte[] {0xFF, 0x00}, new byte[] {0x00, 0xFF}, ExpectedResult = "00FF")]
        [TestCase(new byte[] {0x00, 0xFF}, new byte[] {0x00, 0x00}, ExpectedResult = "00FF")]
        public string MaxLittleEndianUnsignedTest(byte[] alpha,
                                                  byte[] beta)
        {
            // Act
            var result = ByteArrayMath.Max(alpha, beta, true);

            // Assert
            return result.ToHexString();
        }

        [TestCase(new byte[] {}, new byte[] {}, ExpectedResult = "")]
        [TestCase(new byte[] {0x00}, new byte[] {0x00}, ExpectedResult = "00")]
        [TestCase(new byte[] {0x00, 0x00}, new byte[] {0x00, 0x00}, ExpectedResult = "0000")]
        [TestCase(new byte[] {0x00}, new byte[] {0x00, 0xFF}, ExpectedResult = "00")]
        [TestCase(new byte[] {0x00}, new byte[] {0xFF, 0x00}, ExpectedResult = "00")]
        [TestCase(new byte[] {0x00, 0xFF}, new byte[] {0x00, 0x00}, ExpectedResult = "0000")]
        [TestCase(new byte[] {0xFF, 0x00}, new byte[] {0x00, 0x00}, ExpectedResult = "0000")]
        [TestCase(new byte[] {0xFF, 0x00}, new byte[] {0x00, 0x00}, ExpectedResult = "0000")]
        [TestCase(new byte[] {0x00, 0xFF}, new byte[] {0x00, 0x00}, ExpectedResult = "0000")]
        [TestCase(new byte[] {0xFF, 0x00}, new byte[] {0x00, 0xFF}, ExpectedResult = "FF00")]
        [TestCase(new byte[] {0x00, 0xFF}, new byte[] {0x00, 0x00}, ExpectedResult = "0000")]
        public string MinBigEndianSignedTest(byte[] alpha,
                                             byte[] beta)
        {
            // Act
            var result = ByteArrayMath.Min(alpha, beta, false, true);

            // Assert
            return result.ToHexString();
        }

        [TestCase(new byte[] {}, new byte[] {}, ExpectedResult = "")]
        [TestCase(new byte[] {0x00}, new byte[] {0x00}, ExpectedResult = "00")]
        [TestCase(new byte[] {0x00, 0x00}, new byte[] {0x00, 0x00}, ExpectedResult = "0000")]
        [TestCase(new byte[] {0x00}, new byte[] {0x00, 0xFF}, ExpectedResult = "00")]
        [TestCase(new byte[] {0x00}, new byte[] {0xFF, 0x00}, ExpectedResult = "00")]
        [TestCase(new byte[] {0x00, 0xFF}, new byte[] {0x00, 0x00}, ExpectedResult = "0000")]
        [TestCase(new byte[] {0xFF, 0x00}, new byte[] {0x00, 0x00}, ExpectedResult = "0000")]
        [TestCase(new byte[] {0xFF, 0x00}, new byte[] {0x00, 0x00}, ExpectedResult = "0000")]
        [TestCase(new byte[] {0x00, 0xFF}, new byte[] {0x00, 0x00}, ExpectedResult = "0000")]
        [TestCase(new byte[] {0xFF, 0x00}, new byte[] {0x00, 0xFF}, ExpectedResult = "00FF")]
        [TestCase(new byte[] {0x00, 0xFF}, new byte[] {0x00, 0x00}, ExpectedResult = "0000")]
        public string MinLittleEndianSignedTest(byte[] alpha,
                                                byte[] beta)
        {
            // Act
            var result = ByteArrayMath.Min(alpha, beta, true, true);

            // Assert
            return result.ToHexString();
        }

        [TestCase(new byte[] {}, new byte[] {}, ExpectedResult = "")]
        [TestCase(new byte[] {0x00}, new byte[] {0x00}, ExpectedResult = "00")]
        [TestCase(new byte[] {0x00, 0x00}, new byte[] {0x00, 0x00}, ExpectedResult = "0000")]
        [TestCase(new byte[] {0x00}, new byte[] {0x00, 0xFF}, ExpectedResult = "00FF")]
        [TestCase(new byte[] {0x00}, new byte[] {0xFF, 0x00}, ExpectedResult = "00")]
        [TestCase(new byte[] {0x00, 0xFF}, new byte[] {0x00, 0x00}, ExpectedResult = "00FF")]
        [TestCase(new byte[] {0xFF, 0x00}, new byte[] {0x00, 0x00}, ExpectedResult = "0000")]
        [TestCase(new byte[] {0xFF, 0x00}, new byte[] {0x00, 0x00}, ExpectedResult = "0000")]
        [TestCase(new byte[] {0x00, 0xFF}, new byte[] {0x00, 0x00}, ExpectedResult = "00FF")]
        [TestCase(new byte[] {0xFF, 0x00}, new byte[] {0x00, 0xFF}, ExpectedResult = "00FF")]
        [TestCase(new byte[] {0x00, 0xFF}, new byte[] {0x00, 0x00}, ExpectedResult = "00FF")]
        public string MinBigEndianUnsignedTest(byte[] alpha,
                                               byte[] beta)
        {
            // Act
            var result = ByteArrayMath.Min(alpha, beta);

            // Assert
            return result.ToHexString();
        }

        [TestCase(new byte[] {}, new byte[] {}, ExpectedResult = "")]
        [TestCase(new byte[] {0x00}, new byte[] {0x00}, ExpectedResult = "00")]
        [TestCase(new byte[] {0x00, 0x00}, new byte[] {0x00, 0x00}, ExpectedResult = "0000")]
        [TestCase(new byte[] {0x00}, new byte[] {0x00, 0xFF}, ExpectedResult = "00")]
        [TestCase(new byte[] {0x00}, new byte[] {0xFF, 0x00}, ExpectedResult = "FF00")]
        [TestCase(new byte[] {0x00, 0xFF}, new byte[] {0x00, 0x00}, ExpectedResult = "0000")]
        [TestCase(new byte[] {0xFF, 0x00}, new byte[] {0x00, 0x00}, ExpectedResult = "FF00")]
        [TestCase(new byte[] {0xFF, 0x00}, new byte[] {0x00, 0x00}, ExpectedResult = "FF00")]
        [TestCase(new byte[] {0x00, 0xFF}, new byte[] {0x00, 0x00}, ExpectedResult = "0000")]
        [TestCase(new byte[] {0xFF, 0x00}, new byte[] {0x00, 0xFF}, ExpectedResult = "FF00")]
        [TestCase(new byte[] {0x00, 0xFF}, new byte[] {0x00, 0x00}, ExpectedResult = "0000")]
        public string MinLittleEndianUnsignedTest(byte[] alpha,
                                                  byte[] beta)
        {
            // Act
            var result = ByteArrayMath.Min(alpha, beta, true);

            // Assert
            return result.ToHexString();
        }

        [Test]
        public void BitwiseAndAlphaLargerTest()
        {
            // arrange
            var alpha = new byte[] {0x00, 0x00};
            var beta = new byte[] {0x00};

            // Assert
            Assert.Throws<InvalidOperationException>(() => alpha.BitwiseAnd(beta));
        }

        [Test]
        public void BitwiseAndAlphaNullTest()
        {
            Assert.Throws<ArgumentNullException>(() => ((byte[]) null).BitwiseAnd(new byte[] {0x42}));
        }

        [Test]
        public void BitwiseAndBetaLargerTest()
        {
            // arrange
            var alpha = new byte[] {0x00};
            var beta = new byte[] {0x00, 0x00};

            // Assert
            Assert.Throws<InvalidOperationException>(() => alpha.BitwiseAnd(beta));
        }

        [Test]
        public void BitwiseAndBetaNullTest()
        {
            Assert.Throws<ArgumentNullException>(() => new byte[] {0x42}.BitwiseAnd(null));
        }

        [Test]
        public void BitwiseAndTest()
        {
            // arrange
            var alpha = new byte[] {0x0F, 0xF0, 0x00, 0xFF, 0x00, 0xFF};
            var beta = new byte[] {0xF0, 0x0F, 0xFF, 0x00, 0x00, 0xFF};

            // act
            var alphaExpectedResult = alpha.BitwiseAnd(beta);
            var betaExpectedResult = beta.BitwiseAnd(alpha);

            // assert
            Assert.IsTrue(alphaExpectedResult.SequenceEqual(betaExpectedResult));
            Assert.IsTrue(alphaExpectedResult.SequenceEqual(new byte[] {0x00, 0x00, 0x00, 0x00, 0x00, 0xFF}));
        }

        [Test]
        public void BitwiseNotInputNullTest()
        {
            Assert.Throws<ArgumentNullException>(() => ((byte[]) null).BitwiseNot());
        }

        [Test]
        public void BitwiseNotTest()
        {
            // arrange
            var input = new byte[] {0x0F, 0xF0, 0x00, 0xFF};

            // act
            var result = input.BitwiseNot();

            // assert
            Assert.IsTrue(result.SequenceEqual(new byte[] {0xF0, 0x0F, 0xFF, 0x00}));
        }

        [Test]
        public void BitwiseXNorTest()
        {
            var alpha = new byte[] {0x00, 0x01, 0x02, 0xF0, 0x0F, 0xFF};
            var beta = new byte[] {0xFF, 0x10, 0x20, 0x0F, 0xF0, 0x00};

            var alphaExpectedResult = alpha.BitwiseXNor(beta);
            var betaExpectedResult = beta.BitwiseXNor(alpha);

            var checkAgainst = new byte[] {0x00, 0xEE, 0xDD, 0x00, 0x00, 0x00};
           
            Assert.IsTrue(alphaExpectedResult.SequenceEqual(betaExpectedResult));
            Assert.IsTrue(alphaExpectedResult.SequenceEqual(checkAgainst));
        }

        [Test]
        public void BitwiseOrAlphaLargerTest()
        {
            // arrange
            var alpha = new byte[] {0x00, 0x00};
            var beta = new byte[] {0x00};

            // Assert
            Assert.Throws<InvalidOperationException>(() => alpha.BitwiseOr(beta));
        }

        [Test]
        public void BitwiseOrAlphaNullTest()
        {
            Assert.Throws<ArgumentNullException>(() => ((byte[]) null).BitwiseOr(new byte[] {0x42}));
        }

        [Test]
        public void BitwiseOrBetaLargerTest()
        {
            // arrange
            var alpha = new byte[] {0x00};
            var beta = new byte[] {0x00, 0x00};

            // Assert
            Assert.Throws<InvalidOperationException>(() => alpha.BitwiseOr(beta));
        }

        [Test]
        public void BitwiseOrBetaNullTest()
        {
            Assert.Throws<ArgumentNullException>(() => new byte[] {0x42}.BitwiseOr(null));
        }

        [Test]
        public void BitwiseOrTest()
        {
            // arrange
            var alpha = new byte[] {0x0F, 0xF0, 0x00, 0xFF, 0x00, 0xFF};
            var beta = new byte[] {0xF0, 0x0F, 0xFF, 0x00, 0x00, 0xFF};

            // act
            var alphaExpectedResult = alpha.BitwiseOr(beta);
            var betaExpectedResult = beta.BitwiseOr(alpha);

            // assert
            Assert.IsTrue(alphaExpectedResult.SequenceEqual(betaExpectedResult));
            Assert.IsTrue(alphaExpectedResult.SequenceEqual(new byte[] {0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0xFF}));
        }

        [Test]
        public void BitwiseXorAlphaLargerTest()
        {
            // arrange
            var alpha = new byte[] {0x00, 0x00};
            var beta = new byte[] {0x00};

            // Assert
            Assert.Throws<InvalidOperationException>(() => alpha.BitwiseXor(beta));
        }

        [Test]
        public void BitwiseXorAlphaNullTest()
        {
            Assert.Throws<ArgumentNullException>(() => ((byte[]) null).BitwiseXor(new byte[] {0x42}));
        }

        [Test]
        public void BitwiseXorBetaLargerTest()
        {
            // arrange
            var alpha = new byte[] {0x00};
            var beta = new byte[] {0x00, 0x00};

            // Assert
            Assert.Throws<InvalidOperationException>(() => alpha.BitwiseXor(beta));
        }

        [Test]
        public void BitwiseXorBetaNullTest()
        {
            Assert.Throws<ArgumentNullException>(() => new byte[] {0x42}.BitwiseXor(null));
        }

        [Test]
        public void BitwiseXorTest()
        {
            // arrange
            var alpha = new byte[] {0x0F, 0xF0, 0x00, 0xFF, 0x00, 0xFF};
            var beta = new byte[] {0xF0, 0x0F, 0xFF, 0x00, 0x00, 0xFF};

            // act
            var alphaExpectedResult = alpha.BitwiseXor(beta);
            var betaExpectedResult = beta.BitwiseXor(alpha);

            // assert
            Assert.IsTrue(alphaExpectedResult.SequenceEqual(betaExpectedResult));
            Assert.IsTrue(alphaExpectedResult.SequenceEqual(new byte[] {0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00}));
        }

        [Test]
        public void GetBitsAllNoSpecifiedLengthTest()
        {
            var bytes = new byte[] {0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF};
            var result = bytes.GetBits(0);
            Assert.IsTrue(bytes.SequenceEqual(result));
        }

        [Test]
        public void GetBitsAllSpecifiedLengthTest()
        {
            var bytes = new byte[] {0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF};
            var result = bytes.GetBits(0, bytes.Length * 8);
            Assert.IsTrue(bytes.SequenceEqual(result));
        }

        [Test]
        public void GetBitsByteCrossingByteBoundaries()
        {
            var bytes = new byte[] {0x01, 0x23, 0xF5, 0x67, 0x89, 0xAB, 0xCD, 0xEF};

            Assert.AreEqual(0x12, bytes.GetBits(4, 8)
                                       .First());
        }

        [Test]
        public void GetBitsOneAndAHalfByteCrossingByteBoundaries()
        {
            var bytes = new byte[] {0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF};

            var result = bytes.GetBits(12, 12);
            Assert.IsTrue(result.SequenceEqual(new byte[] {0x03, 0x45}));
        }

        [Test]
        public void GetBitsReadLengthTooGreatTest()
        {
            var bytes = new byte[] {0x00};

            // Assert
            Assert.Throws<InvalidOperationException>(() => bytes.GetBits(12, 12));
        }

        [Test]
        public void GetBitsSourceNullTest()
        {
            Assert.Throws<ArgumentNullException>(() => ((byte[]) null).GetBits(0));
        }

        [Test]
        public void GetBitsSubBytes()
        {
            var bytes = new byte[] {0xFF};

            for (var i = 0; i < 8; i++)
            {
                var result = bytes.GetBits(0, 8 - i);
                var expected = (byte) (0xFF >> i);
                Assert.AreEqual(expected, result[0]);
            }
        }

        [Test]
        public void GetBitsSubSequenceNotCrossingByteBoundariesTest()
        {
            var bytes = new byte[] {0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF};

            for (var i = 0; i < bytes.Length; i++)
            {
                var result = bytes.GetBits(i * 8, 8);
                Assert.AreEqual(bytes[i], result[0]);
            }
        }

        [Test]
        public void GetBitsTwoByteCrossingByteBoundaries()
        {
            var bytes = new byte[] {0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF};

            var result = bytes.GetBits(4, 16);
            Assert.IsTrue(result.SequenceEqual(new byte[] {0x12, 0x34}));
        }

        [Test]
        public void MaxNullAlphaTest()
        {
            Assert.Throws<ArgumentNullException>(() => ByteArrayMath.Max(null, new byte[] {0x42}));
        }

        [Test]
        public void MaxNullBetaTest()
        {
            Assert.Throws<ArgumentNullException>(() => ByteArrayMath.Max(new byte[] {0x42}, null));
        }

        [Test]
        public void MinNullAlphaTest()
        {
            Assert.Throws<ArgumentNullException>(() => ByteArrayMath.Min(null, new byte[] {0x42}));
        }

        [Test]
        public void MinNullBetaTest()
        {
            Assert.Throws<ArgumentNullException>(() => ByteArrayMath.Min(new byte[] {0x42}, null));
        }

        [Test]
        public void ShiftBitsLeftBasicTest()
        {
            // arrange
            var bytes = new byte[] {0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF};

            // act
            byte[] carry;
            var shiftBitsLeft = bytes.ShiftBitsLeft(8, out carry);

            // assert
            Assert.AreEqual(0x01, carry[0]);
            Assert.IsTrue(shiftBitsLeft.SequenceEqual(new byte[] {0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF, 0x00}));
        }

        [Test]
        public void ShiftBitsLeftCaryInputNullTest()
        {
            Assert.Throws<ArgumentNullException>(() =>
                                                 {
                                                     byte[] bytes;
                                                     ((byte[]) null).ShiftBitsLeft(1, out bytes);
                                                 });
        }

        [Test]
        public void ShiftBitsLeftHalfByteBoundaryTest()
        {
            // arrange
            var bytes = new byte[] {0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF};

            // act
            byte[] carry;
            var shiftBitsLeft = bytes.ShiftBitsLeft(4, out carry);

            // assert
            Assert.AreEqual(0x00, carry[0]);
            Assert.IsTrue(shiftBitsLeft.SequenceEqual(new byte[] {0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xF0}));
        }

        [Test]
        public void ShiftBitsLeftInputNullTest()
        {
            Assert.Throws<ArgumentNullException>(() => ((byte[]) null).ShiftBitsLeft());
        }

        [Test]
        public void ShiftBitsLeftSimpleTest()
        {
            // arrange
            var bytes = new byte[] {0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF};

            // act
            var shiftBitsLeft = bytes.ShiftBitsLeft(8);

            // assert
            Assert.IsTrue(shiftBitsLeft.SequenceEqual(new byte[] {0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF, 0x00}));
        }

        [Test]
        public void ShiftBitsRightBasicTest()
        {
            // arrange
            var bytes = new byte[] {0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF};

            // act
            byte[] carry;
            var shiftBitsRight = bytes.ShiftBitsRight(8, out carry);

            // assert
            Assert.AreEqual(0xEF, carry[0]);
            Assert.IsTrue(shiftBitsRight.SequenceEqual(new byte[] {0x00, 0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD}));
        }

        [Test]
        public void ShiftBitsRightCaryInputNullTest()
        {
            Assert.Throws<ArgumentNullException>(() =>
                                                 {
                                                     byte[] bytes;
                                                     ((byte[]) null).ShiftBitsRight(1, out bytes);
                                                 });
        }

        [Test]
        public void ShiftBitsRightHalfByteBoundaryTest()
        {
            // arrange
            var bytes = new byte[] {0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF};

            // act
            byte[] carry;
            var shiftBitsRight = bytes.ShiftBitsRight(4, out carry);

            // assert
            Assert.AreEqual(0x0F, carry[0]);
            Assert.IsTrue(shiftBitsRight.SequenceEqual(new byte[] {0x00, 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE}));
        }

        [Test]
        public void ShiftBitsRightInputNullTest()
        {
            Assert.Throws<ArgumentNullException>(() => ((byte[]) null).ShiftBitsRight());
        }

        [Test]
        public void ShiftBitsRightSimpleTest()
        {
            // arrange
            var bytes = new byte[] {0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF};

            // act
            var shiftBitsRight = bytes.ShiftBitsRight(8);

            // assert
            Assert.IsTrue(shiftBitsRight.SequenceEqual(new byte[] {0x00, 0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD}));
        }
    }
}
