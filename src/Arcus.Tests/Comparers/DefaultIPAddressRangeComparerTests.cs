using System;
using System.Collections.Generic;
using System.Net;
using System.Numerics;
using Arcus.Comparers;
using Moq;
using Xunit;

namespace Arcus.Tests.Comparers
{
    public class DefaultIPAddressRangeComparerTests
    {
        [Fact]
        public void Assignability_Test()
        {
            // Arrange
            var type = typeof(DefaultIPAddressRangeComparer);

            // Act
            var isAssignableFrom = typeof(IComparer<IIPAddressRange>).IsAssignableFrom(type);

            // Assert
            Assert.True(isAssignableFrom);
        }

        #region Ctor

        [Fact]
        public void Ctor_NullInput_Throws_ArgumentNullException_Test()
        {
            // Arrange
            // Act
            // Assert
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => new DefaultIPAddressRangeComparer(null));
        }

        #endregion // end: Ctor

        #region Compare

        public static IEnumerable<object[]> Compare_Test_Values()
        {
            // equal ranges
            //yield return new object[] {0, CreateMockIPAddressRange("", "", 0), CreateMockIPAddressRange("", "", 0) };

            yield return new object[] {0, CreateIIPAddressRange("192.168.1.0", 0), CreateIIPAddressRange("192.168.1.0", 0)};
            yield return new object[] {0, CreateIIPAddressRange("a::", 0), CreateIIPAddressRange("a::", 0)};

            // same range
            var ipv4Same = CreateIIPAddressRange("192.168.1.0");
            yield return new object[] {0, ipv4Same, ipv4Same};

            var ipv6Same = CreateIIPAddressRange("a::");
            yield return new object[] {0, ipv6Same, ipv6Same};

            // null compare
            yield return new object[] {0, null, null};
            yield return new object[] { -1, null, CreateIIPAddressRange("192.168.1.0")};
            yield return new object[] {1, CreateIIPAddressRange("192.168.1.0"), null};
            yield return new object[] { -1, null, CreateIIPAddressRange("a::")};
            yield return new object[] {1, CreateIIPAddressRange("a::"), null};

            // numerically equivalent, different address families
            yield return new object[] { -1, CreateIIPAddressRange("0.0.0.0", 100), CreateIIPAddressRange("::", 100)};
            yield return new object[] {1, CreateIIPAddressRange("::", 100), CreateIIPAddressRange("0.0.0.0", 100)};

            // satisfies ordinal ordering by length
            yield return new object[] { -1, CreateIIPAddressRange("192.0.0.0", 100), CreateIIPAddressRange("192.0.0.0", 500)};
            yield return new object[] {1, CreateIIPAddressRange("192.0.0.0", 500), CreateIIPAddressRange("192.0.0.0", 100)};
            yield return new object[] { -1, CreateIIPAddressRange("ab::", 100), CreateIIPAddressRange("ab::", 500)};
            yield return new object[] {1, CreateIIPAddressRange("ab::", 500), CreateIIPAddressRange("ab::", 100)};

            IIPAddressRange CreateIIPAddressRange(string head,
                                                  BigInteger? length = null)
            {
                var xMock = new Mock<IIPAddressRange>(MockBehavior.Strict);

                xMock.Setup(m => m.ToString())
                     .Returns($"{head}[{length}]");

                xMock.Setup(m => m.Head)
                     .Returns(IPAddress.Parse(head));

                if (length != null)
                {
                    xMock.Setup(m => m.Length)
                         .Returns(length.Value);
                }

                return xMock.Object;
            }
        }

        [Theory]
        [MemberData(nameof(Compare_Test_Values))]
        public void Compare_Test(int expected,
                                 IIPAddressRange x,
                                 IIPAddressRange y)
        {
            // Arrange
            var comparer = new DefaultIPAddressRangeComparer();

            // Act

            var result = comparer.Compare(x, y);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void DeferToIPAddressComparerTest()
        {
            // Arrange
            const int expectedResult = 42;

            var xHead = IPAddress.Parse("0.0.0.0");
            var yHead = IPAddress.Parse("abc::");

            var x = CreateIIPAddressRange(xHead);
            var y = CreateIIPAddressRange(yHead);

            var mockIPAddressComparer = new Mock<IComparer<IPAddress>>();
            mockIPAddressComparer.Setup(c => c.Compare(xHead, yHead))
                                 .Returns(expectedResult);

            var comparer = new DefaultIPAddressRangeComparer(mockIPAddressComparer.Object);

            // Act
            var result = comparer.Compare(x, y);

            // Assert
            Assert.Equal(expectedResult, result);
            mockIPAddressComparer.Verify(c => c.Compare(xHead, yHead), Times.Once);

            IIPAddressRange CreateIIPAddressRange(IPAddress head)
            {
                var xMock = new Mock<IIPAddressRange>(MockBehavior.Strict);

                xMock.Setup(m => m.ToString());
                xMock.Setup(m => m.Head)
                     .Returns(head);

                return xMock.Object;
            }
        }

        #endregion // end: Compare
    }
}
