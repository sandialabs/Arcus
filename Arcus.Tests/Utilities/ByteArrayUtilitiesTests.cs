using System;
using Arcus.Utilities;
using NUnit.Framework;

namespace Arcus.Tests.Utilities
{
    [TestFixture]
    public class ByteArrayUtilitiesTests
    {
        [TestCase(null, 4, TestName = "null input", ExpectedResult = new byte[] {0x00, 0x00, 0x00, 0x00})]
        [TestCase(new byte[] {}, 4, TestName = "empty input", ExpectedResult = new byte[] {0x00, 0x00, 0x00, 0x00})]
        [TestCase(new byte[] {0xaa, 0xff}, 2, TestName = "no change input", ExpectedResult = new byte[] {0xaa, 0xff})]
        [TestCase(new byte[] {0xaa, 0xff}, 4, TestName = "pad input", ExpectedResult = new byte[] {0xaa, 0xff, 0x00, 0x00})]
        [TestCase(new byte[] {0xaa, 0xff, 0x00, 0x00}, 2, TestName = "trim input", ExpectedResult = new byte[] {0xaa, 0xff})]
        public byte[] AffixByteLength(byte[] input,
                                      int length) => input.AffixByteLength(length);


        [TestCase(new byte[] {0x00, 0x00, 0x00}, 0x01, ExpectedResult = new byte[] {0x01, 0x01, 0x01})]
        [TestCase(new byte[] {0x02, 0x02, 0x02, 0x02}, 0x08, ExpectedResult = new byte[] {0x08, 0x08, 0x08, 0x08})]
        [TestCase(new byte[] {}, 0x01, ExpectedResult = new byte[] {})]
        public byte[] FillByteArrayTests(byte[] bytes, byte fillValue)
            =>bytes.FillByteArray(fillValue);
        

        [Test]
        public void FillByteArrayNullInputTest()
        {
            Assert.Throws<ArgumentNullException>(() => ((byte[]) null).FillByteArray(0x42));
        }

        [TestCase(8, 0x00, ExpectedResult = new byte[] {0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00})]
        [TestCase(3, 0x01, ExpectedResult = new byte[] {0x01, 0x01, 0x01})]
        [TestCase(0, 0x00, ExpectedResult = new byte[] {})]
        public byte[] CreateFilledArrayTests(int size, byte intializer)//TODO: test necessary?
            => ByteArrayUtilities.CreateFilledByteArray(size, intializer);

        [TestCase(4, ExpectedResult = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF })]
        [TestCase(0, ExpectedResult = new byte[] {})]
        public byte[] CreateFilledArrayDefaultsTo0xFF(int size)
            => ByteArrayUtilities.CreateFilledByteArray(size);
        
    }
}
