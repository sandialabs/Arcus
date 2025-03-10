using System;
using System.Collections.Generic;
using System.Net;
using System.Numerics;
using Arcus.Comparers;
using NSubstitute;
using Xunit;

namespace Arcus.Tests.Comparers
{
#if NET6_0_OR_GREATER
#pragma warning disable IDE0062 // Make local function static (IDE0062); purposely allowing non-static functions that could be static for .net4.8 compatibility
#endif
    public class DefaultIIPAddressRangeComparerTests
    {
        [Fact]
        public void Assignability_Test()
        {
            // Arrange
            var type = typeof(DefaultIIPAddressRangeComparer);

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

            Assert.Throws<ArgumentNullException>(() => new DefaultIIPAddressRangeComparer(null));
        }

        #endregion // end: Ctor

        #region Compare

        public static IEnumerable<object[]> Compare_Test_Values()
        {
            // equal ranges
            yield return new object[] { 0, CreateIIPAddressRange("192.168.1.0", 0), CreateIIPAddressRange("192.168.1.0", 0) };
            yield return new object[] { 0, CreateIIPAddressRange("a::", 0), CreateIIPAddressRange("a::", 0) };

            // same range
            var ipv4Same = CreateIIPAddressRange("192.168.1.0");
            yield return new object[] { 0, ipv4Same, ipv4Same };

            var ipv6Same = CreateIIPAddressRange("a::");
            yield return new object[] { 0, ipv6Same, ipv6Same };

            // null compare
            yield return new object[] { 0, null, null };
            yield return new object[] { -1, null, CreateIIPAddressRange("192.168.1.0") };
            yield return new object[] { 1, CreateIIPAddressRange("192.168.1.0"), null };
            yield return new object[] { -1, null, CreateIIPAddressRange("a::") };
            yield return new object[] { 1, CreateIIPAddressRange("a::"), null };

            // numerically equivalent, different address families
            yield return new object[] { -1, CreateIIPAddressRange("0.0.0.0", 100), CreateIIPAddressRange("::", 100) };
            yield return new object[] { 1, CreateIIPAddressRange("::", 100), CreateIIPAddressRange("0.0.0.0", 100) };

            // satisfies ordinal ordering by length
            yield return new object[] { -1, CreateIIPAddressRange("192.0.0.0", 100), CreateIIPAddressRange("192.0.0.0", 500) };
            yield return new object[] { 1, CreateIIPAddressRange("192.0.0.0", 500), CreateIIPAddressRange("192.0.0.0", 100) };
            yield return new object[] { -1, CreateIIPAddressRange("ab::", 100), CreateIIPAddressRange("ab::", 500) };
            yield return new object[] { 1, CreateIIPAddressRange("ab::", 500), CreateIIPAddressRange("ab::", 100) };

            IIPAddressRange CreateIIPAddressRange(string head, BigInteger? length = null)
            {
                var substitute = Substitute.For<IIPAddressRange>();

                substitute.Head.Returns(IPAddress.Parse(head));

                if (length != null)
                {
                    substitute.Length.Returns(length.Value);
                }

                return substitute;
            }
        }

        [Theory]
        [MemberData(nameof(Compare_Test_Values))]
        public void Compare_Test(int expected, IIPAddressRange x, IIPAddressRange y)
        {
            // Arrange
            var comparer = new DefaultIIPAddressRangeComparer();

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

            var substituteIPAddressComparer = Substitute.For<IComparer<IPAddress>>();
            substituteIPAddressComparer.Compare(xHead, yHead).Returns(expectedResult);

            var comparer = new DefaultIIPAddressRangeComparer(substituteIPAddressComparer);

            // Act
            var result = comparer.Compare(x, y);

            // Assert
            Assert.Equal(expectedResult, result);
            substituteIPAddressComparer.Received(1).Compare(xHead, yHead); // Verify that Compare was called once

            IIPAddressRange CreateIIPAddressRange(IPAddress head)
            {
                var substitute = Substitute.For<IIPAddressRange>();

                substitute.Head.Returns(head);

                return substitute;
            }
        }

        #endregion // end: Compare
    }
}
