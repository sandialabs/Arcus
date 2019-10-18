using System;
using System.Collections.Generic;
using System.Net;
using Arcus.Math;
using Xunit;

namespace Arcus.Tests.Math
{
    public class IPAddressMathTests
    {
        #region IsEqualTo

        public static IEnumerable<object[]> IsEqualTo_Test_Values()
        {
            foreach (var testCase in NonTransitiveTestCases())
            {
                yield return new object[] {testCase.expected, testCase.left, testCase.right};
            }

            foreach (var testCase in TransitiveTestCases())
            {
                yield return new object[] {testCase.expected, testCase.left, testCase.right};
                yield return new object[] {testCase.expected, testCase.right, testCase.left}; // Transitive law test
            }

            IEnumerable<(bool expected, IPAddress left, IPAddress right)> NonTransitiveTestCases()
            {
                // reference equal
                var ipv4SameAddress = IPAddress.Parse("192.168.1.1");
                yield return (true, ipv4SameAddress, ipv4SameAddress);

                var ipv6SameAddress = IPAddress.Parse("abc::123");
                yield return (true, ipv6SameAddress, ipv6SameAddress);

                // equal
                yield return (true, IPAddress.Parse("192.168.1.1"), IPAddress.Parse("192.168.1.1"));
                yield return (true, IPAddress.Parse("abc::123"), IPAddress.Parse("abc::123"));

                // null comparison
                yield return (true, null, null);
            }

            IEnumerable<(bool expected, IPAddress left, IPAddress right)> TransitiveTestCases()
            {
                // not equal
                yield return (false, IPAddress.Parse("192.168.1.25"), IPAddress.Parse("192.168.1.1"));
                yield return (false, IPAddress.Parse("abc::fff"), IPAddress.Parse("abc::123"));

                // null comparison
                yield return (false, null, IPAddress.Parse("192.168.1.1"));
                yield return (false, null, IPAddress.Parse("abc::123"));

                // differing address families
                yield return (false, IPAddress.Parse("192.168.1.1"), IPAddress.Parse("abc::123"));
            }
        }

        [Theory]
        [MemberData(nameof(IsEqualTo_Test_Values))]
        public void IsEqualTo_Test(bool expected,
                                   IPAddress left,
                                   IPAddress right)
        {
            // Arrange
            // Act
            var result = left.IsEqualTo(right);

            // Assert
            Assert.Equal(expected, result);
        }

        #endregion // end: IsEqualTo

        #region IsGreaterThan

        public static IEnumerable<object[]> IsGreaterThan_Test_Values()
        {
            foreach (var testCase in NonTransitiveTestCases())
            {
                yield return new object[] {testCase.expected, testCase.left, testCase.right};
            }

            foreach (var testCase in TransitiveInverse())
            {
                yield return new object[] {testCase.expected, testCase.left, testCase.right};
                yield return new object[] {!testCase.expected, testCase.right, testCase.left}; // Transitive law test
            }

            IEnumerable<(bool expected, IPAddress left, IPAddress right)> NonTransitiveTestCases()
            {
                // reference equal
                var ipv4SameAddress = IPAddress.Parse("192.168.1.1");
                yield return (false, ipv4SameAddress, ipv4SameAddress);

                var ipv6SameAddress = IPAddress.Parse("abc::123");
                yield return (false, ipv6SameAddress, ipv6SameAddress);

                // equal
                yield return (false, IPAddress.Parse("192.168.1.1"), IPAddress.Parse("192.168.1.1"));
                yield return (false, IPAddress.Parse("abc::123"), IPAddress.Parse("abc::123"));

                // null comparison
                yield return (false, null, null);
                yield return (false, null, IPAddress.Parse("192.168.1.1"));
                yield return (false, null, IPAddress.Parse("abc::123"));
                yield return (false, IPAddress.Parse("192.168.1.1"), null);
                yield return (false, IPAddress.Parse("abc::123"), null);

                // differing address families
                yield return (false, IPAddress.Parse("192.168.1.1"), IPAddress.Parse("abc::123"));
                yield return (false, IPAddress.Parse("abc::123"), IPAddress.Parse("192.168.1.1"));
            }

            IEnumerable<(bool expected, IPAddress left, IPAddress right)> TransitiveInverse()
            {
                // greater than
                yield return (true, IPAddress.Parse("192.168.1.25"), IPAddress.Parse("192.168.1.1"));
                yield return (true, IPAddress.Parse("abc::fff"), IPAddress.Parse("abc::123"));

                // less than
                yield return (false, IPAddress.Parse("192.168.1.1"), IPAddress.Parse("192.168.1.25"));
                yield return (false, IPAddress.Parse("abc::123"), IPAddress.Parse("abc::fff"));
            }
        }

        [Theory]
        [MemberData(nameof(IsGreaterThan_Test_Values))]
        public void IsGreaterThan_Test(bool expected,
                                       IPAddress left,
                                       IPAddress right)
        {
            // Arrange
            // Act
            var result = left.IsGreaterThan(right);

            // Assert
            Assert.Equal(expected, result);
        }

        #endregion // end: IsGreaterThan

        #region IsGreaterThanOrEqualTo

        public static IEnumerable<object[]> IsGreaterThanOrEqualTo_Test_Values()
        {
            foreach (var testCase in NonTransitiveTestCases())
            {
                yield return new object[] {testCase.expected, testCase.left, testCase.right};
            }

            foreach (var testCase in TransitiveInverse())
            {
                yield return new object[] {testCase.expected, testCase.left, testCase.right};
                yield return new object[] {!testCase.expected, testCase.right, testCase.left}; // Transitive law test
            }

            IEnumerable<(bool expected, IPAddress left, IPAddress right)> NonTransitiveTestCases()
            {
                // reference equal
                var ipv4SameAddress = IPAddress.Parse("192.168.1.1");
                yield return (true, ipv4SameAddress, ipv4SameAddress);

                var ipv6SameAddress = IPAddress.Parse("abc::123");
                yield return (true, ipv6SameAddress, ipv6SameAddress);

                // equal
                yield return (true, IPAddress.Parse("192.168.1.1"), IPAddress.Parse("192.168.1.1"));
                yield return (true, IPAddress.Parse("abc::123"), IPAddress.Parse("abc::123"));

                // null comparison
                yield return (false, null, IPAddress.Parse("192.168.1.1"));
                yield return (false, null, IPAddress.Parse("abc::123"));
                yield return (false, IPAddress.Parse("192.168.1.1"), null);
                yield return (false, IPAddress.Parse("abc::123"), null);
                yield return (true, null, null);

                // differing address families
                yield return (false, IPAddress.Parse("192.168.1.1"), IPAddress.Parse("abc::123"));
                yield return (false, IPAddress.Parse("abc::123"), IPAddress.Parse("192.168.1.1"));
            }

            IEnumerable<(bool expected, IPAddress left, IPAddress right)> TransitiveInverse()
            {
                // greater than
                yield return (true, IPAddress.Parse("192.168.1.25"), IPAddress.Parse("192.168.1.1"));
                yield return (true, IPAddress.Parse("abc::fff"), IPAddress.Parse("abc::123"));

                // less than
                yield return (false, IPAddress.Parse("192.168.1.1"), IPAddress.Parse("192.168.1.25"));
                yield return (false, IPAddress.Parse("abc::123"), IPAddress.Parse("abc::fff"));
            }
        }

        [Theory]
        [MemberData(nameof(IsGreaterThanOrEqualTo_Test_Values))]
        public void IsGreaterThanOrEqualTo_Test(bool expected,
                                                IPAddress left,
                                                IPAddress right)
        {
            // Arrange
            // Act
            var result = left.IsGreaterThanOrEqualTo(right);

            // Assert
            Assert.Equal(expected, result);
        }

        #endregion // end: IsGreaterThanOrEqualTo

        #region IsLessThan

        public static IEnumerable<object[]> IsLessThan_Test_Values()
        {
            foreach (var testCase in NonTransitiveTestCases())
            {
                yield return new object[] {testCase.expected, testCase.left, testCase.right};
            }

            foreach (var testCase in TransitiveInverse())
            {
                yield return new object[] {testCase.expected, testCase.left, testCase.right};
                yield return new object[] {!testCase.expected, testCase.right, testCase.left}; // Transitive law test
            }

            IEnumerable<(bool expected, IPAddress left, IPAddress right)> NonTransitiveTestCases()
            {
                // reference equal
                var ipv4SameAddress = IPAddress.Parse("192.168.1.1");
                yield return (false, ipv4SameAddress, ipv4SameAddress);

                var ipv6SameAddress = IPAddress.Parse("abc::123");
                yield return (false, ipv6SameAddress, ipv6SameAddress);

                // equal
                yield return (false, IPAddress.Parse("192.168.1.1"), IPAddress.Parse("192.168.1.1"));
                yield return (false, IPAddress.Parse("abc::123"), IPAddress.Parse("abc::123"));

                // null comparison
                yield return (false, null, null);
                yield return (false, null, IPAddress.Parse("192.168.1.1"));
                yield return (false, null, IPAddress.Parse("abc::123"));
                yield return (false, IPAddress.Parse("192.168.1.1"), null);
                yield return (false, IPAddress.Parse("abc::123"), null);

                // differing address families
                yield return (false, IPAddress.Parse("192.168.1.1"), IPAddress.Parse("abc::123"));
                yield return (false, IPAddress.Parse("abc::123"), IPAddress.Parse("192.168.1.1"));
            }

            IEnumerable<(bool expected, IPAddress left, IPAddress right)> TransitiveInverse()
            {
                // greater than
                yield return (false, IPAddress.Parse("192.168.1.25"), IPAddress.Parse("192.168.1.1"));
                yield return (false, IPAddress.Parse("abc::fff"), IPAddress.Parse("abc::123"));

                // less than
                yield return (true, IPAddress.Parse("192.168.1.1"), IPAddress.Parse("192.168.1.25"));
                yield return (true, IPAddress.Parse("abc::123"), IPAddress.Parse("abc::fff"));
            }
        }

        [Theory]
        [MemberData(nameof(IsLessThan_Test_Values))]
        public void IsLessThan_Test(bool expected,
                                    IPAddress left,
                                    IPAddress right)
        {
            // Arrange
            // Act
            var result = left.IsLessThan(right);

            // Assert
            Assert.Equal(expected, result);
        }

        #endregion // end: IsLessThan

        #region IsLessThanOrEqualTo

        public static IEnumerable<object[]> IsLessThanOrEqualTo_Test_Values()
        {
            foreach (var testCase in NonTransitiveTestCases())
            {
                yield return new object[] {testCase.expected, testCase.left, testCase.right};
            }

            foreach (var testCase in TransitiveInverse())
            {
                yield return new object[] {testCase.expected, testCase.left, testCase.right};
                yield return new object[] {!testCase.expected, testCase.right, testCase.left}; // Transitive law test
            }

            IEnumerable<(bool expected, IPAddress left, IPAddress right)> NonTransitiveTestCases()
            {
                // reference equal
                var ipv4SameAddress = IPAddress.Parse("192.168.1.1");
                yield return (true, ipv4SameAddress, ipv4SameAddress);

                var ipv6SameAddress = IPAddress.Parse("abc::123");
                yield return (true, ipv6SameAddress, ipv6SameAddress);

                // equal
                yield return (true, IPAddress.Parse("192.168.1.1"), IPAddress.Parse("192.168.1.1"));
                yield return (true, IPAddress.Parse("abc::123"), IPAddress.Parse("abc::123"));

                // null comparison
                yield return (true, null, null);
                yield return (false, IPAddress.Parse("192.168.1.1"), null);
                yield return (false, IPAddress.Parse("abc::123"), null);
                yield return (false, null, IPAddress.Parse("192.168.1.1"));
                yield return (false, null, IPAddress.Parse("abc::123"));

                // differing address families
                yield return (false, IPAddress.Parse("192.168.1.1"), IPAddress.Parse("abc::123"));
                yield return (false, IPAddress.Parse("abc::123"), IPAddress.Parse("192.168.1.1"));
            }

            IEnumerable<(bool expected, IPAddress left, IPAddress right)> TransitiveInverse()
            {
                // greater than
                yield return (false, IPAddress.Parse("192.168.1.25"), IPAddress.Parse("192.168.1.1"));
                yield return (false, IPAddress.Parse("abc::fff"), IPAddress.Parse("abc::123"));

                // less than
                yield return (true, IPAddress.Parse("192.168.1.1"), IPAddress.Parse("192.168.1.25"));
                yield return (true, IPAddress.Parse("abc::123"), IPAddress.Parse("abc::fff"));
            }
        }

        [Theory]
        [MemberData(nameof(IsLessThanOrEqualTo_Test_Values))]
        public void IsLessThanOrEqualTo_Test(bool expected,
                                             IPAddress left,
                                             IPAddress right)
        {
            // Arrange
            // Act
            var result = left.IsLessThanOrEqualTo(right);

            // Assert
            Assert.Equal(expected, result);
        }

        #endregion // end: IsLessThanOrEqual

        #region IsBetween

        public static IEnumerable<object[]> IsBetween_Test_Values()
        {
            // inclusive tests
            foreach (var testCase in EdgeEqualityTestCases())
            {
                yield return new object[] {true, testCase.input, testCase.low, testCase.high, true};
                yield return new object[] {false, testCase.input, testCase.low, testCase.high, false};
            }

            // exclusive tests
            foreach (var testCase in OverlappedTestCases())
            {
                yield return new object[] {testCase.expected, testCase.input, testCase.low, testCase.high, true};
                yield return new object[] {testCase.expected, testCase.input, testCase.low, testCase.high, false};
            }

            // edges are equal
            IEnumerable<(IPAddress input, IPAddress low, IPAddress high)> EdgeEqualityTestCases()
            {
                // reference equals
                var ipv4SameAddress = IPAddress.Parse("192.168.0.1");
                var ipv6SameAddress = IPAddress.Parse("abc::123");

                // reference equals low
                yield return (ipv4SameAddress, ipv4SameAddress, IPAddress.Parse("192.168.1.10"));
                yield return (ipv6SameAddress, ipv6SameAddress, IPAddress.Parse("abc::f123"));

                // reference equals high
                yield return (ipv4SameAddress, IPAddress.Parse("192.168.0.0"), ipv4SameAddress);
                yield return (ipv6SameAddress, IPAddress.Parse("abc::"), ipv6SameAddress);

                // reference equals low and high
                yield return (ipv4SameAddress, ipv4SameAddress, ipv4SameAddress);
                yield return (ipv6SameAddress, ipv6SameAddress, ipv6SameAddress);

                // equals
                // equals low
                yield return (IPAddress.Parse("192.168.1.1"), IPAddress.Parse("192.168.1.1"), IPAddress.Parse("192.168.1.10"));
                yield return (IPAddress.Parse("abc::123"), IPAddress.Parse("abc::123"), IPAddress.Parse("abc::f123"));

                // equals high
                yield return (IPAddress.Parse("192.168.1.10"), IPAddress.Parse("192.168.0.0"), IPAddress.Parse("192.168.1.10"));
                yield return (IPAddress.Parse("abc::123"), IPAddress.Parse("abc::"), IPAddress.Parse("abc::123"));

                // equals low and high
                yield return (IPAddress.Parse("192.168.1.1"), IPAddress.Parse("192.168.1.1"), IPAddress.Parse("192.168.1.1"));
                yield return (IPAddress.Parse("abc::123"), IPAddress.Parse("abc::123"), IPAddress.Parse("abc::123"));
            }

            // Inclusive and Exclusive
            IEnumerable<(bool expected, IPAddress input, IPAddress low, IPAddress high)> OverlappedTestCases()
            {
                // before low
                yield return (false, IPAddress.Parse("192.168.1.0"), IPAddress.Parse("192.168.10.0"), IPAddress.Parse("192.168.10.255"));
                yield return (false, IPAddress.Parse("abc::"), IPAddress.Parse("abc::ff"), IPAddress.Parse("abc::ffff"));

                // after high
                yield return (false, IPAddress.Parse("192.168.20.0"), IPAddress.Parse("192.168.10.0"), IPAddress.Parse("192.168.10.255"));
                yield return (false, IPAddress.Parse("abcd::"), IPAddress.Parse("abc::ff"), IPAddress.Parse("abc::ffff"));

                // inside range
                yield return (true, IPAddress.Parse("192.168.10.128"), IPAddress.Parse("192.168.10.0"), IPAddress.Parse("192.168.10.255"));
                yield return (true, IPAddress.Parse("abc::fff0"), IPAddress.Parse("abc::ff"), IPAddress.Parse("abc::ffff"));
            }
        }

        [Theory]
        [MemberData(nameof(IsBetween_Test_Values))]
        public void IsBetween_Test(bool expected,
                                   IPAddress input,
                                   IPAddress low,
                                   IPAddress high,
                                   bool inclusive)
        {
            // Arrange
            // Act
            var result = input.IsBetween(low, high, inclusive);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void IsBetween_NullInput_Throws_ArgumentNullException_Test()
        {
            // Arrange
            // Act
            // Assert
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => ((IPAddress) null).IsBetween(IPAddress.Any, IPAddress.Any));
        }

        [Fact]
        public void IsBetween_NullHigh_Throws_ArgumentNullException_Test()
        {
            // Arrange
            // Act
            // Assert
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => IPAddress.Any.IsBetween(IPAddress.Any, null));
        }

        [Fact]
        public void IsBetween_NullLow_Throws_ArgumentNullException_Test()
        {
            // Arrange
            // Act
            // Assert
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => IPAddress.Any.IsBetween(null, IPAddress.Any));
        }

        [Fact]
        public void IsBetween_LowGreaterThanHigh_Throws_InvalidOperationException_Test()
        {
            // Arrange
            // Act
            // Assert
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<InvalidOperationException>(() => IPAddress.Any.IsBetween(IPAddress.Parse("100.1.1.1"), IPAddress.Parse("10.1.1.1")));
        }

        public static IEnumerable<object[]> IsBetween_UnmatchedAddressFamilies_Test_Values()
        {
            var ipv4 = IPAddress.Any;
            var ipv6 = IPAddress.IPv6Any;

            yield return new object[] {ipv4, ipv4, ipv6};
            yield return new object[] {ipv4, ipv6, ipv4};
            yield return new object[] {ipv6, ipv4, ipv4};

            yield return new object[] {ipv6, ipv6, ipv4};
            yield return new object[] {ipv6, ipv4, ipv6};
            yield return new object[] {ipv4, ipv6, ipv6};
        }

        [Theory]
        [MemberData(nameof(IsBetween_UnmatchedAddressFamilies_Test_Values))]
        public void IsBetween_UnmatchedAddressFamilies_Throws_InvalidOperationException_Test(IPAddress input,
                                                                                             IPAddress low,
                                                                                             IPAddress high)
        {
            // Arrange
            // Act
            // Assert
            Assert.Throws<InvalidOperationException>(() => input.IsBetween(low, high));
        }

        #endregion // end: IsBetween

        #region IsAtMax

        [Theory]
        [InlineData(false, "::")]
        [InlineData(false, "0.0.0.0")]
        [InlineData(false, "128.128.128.128")]
        [InlineData(false, "7777:7777:7777:7777:7777:7777:7777:7777")]
        [InlineData(true, "255.255.255.255")]
        [InlineData(true, "ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff")]
        public void IsAtMax_Test(bool expected,
                                 string input)
        {
            // Arrange
            var address = IPAddress.Parse(input);

            // Act
            var result = address.IsAtMax();

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void IsAtMax_NullInput_Throws_ArgumentNullException_Test()
        {
            // Arrange
            // Act
            // Assert
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => ((IPAddress) null).IsAtMax());
        }

        #endregion // end: IsAtMax

        #region IsAtMin

        [Theory]
        [InlineData(false, "255.255.255.255")]
        [InlineData(false, "ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff")]
        [InlineData(false, "128.128.128.128")]
        [InlineData(false, "7777:7777:7777:7777:7777:7777:7777:7777")]
        [InlineData(true, "::")]
        [InlineData(true, "0.0.0.0")]
        public void IsAtMin_Test(bool expected,
                                 string input)
        {
            // Arrange
            var address = IPAddress.Parse(input);

            // Act
            var result = address.IsAtMin();

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void IsAtMin_NullInput_Throws_ArgumentNullException_Test()
        {
            // Arrange
            // Act
            // Assert
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => ((IPAddress) null).IsAtMin());
        }

        #endregion // end: IsAtMin

        #region Max

        public static IEnumerable<object[]> Max_Test_Values()
        {
            var minIpv4 = IPAddress.Parse("192.168.1.1");
            var maxIpv4 = IPAddress.Parse("192.168.100.1");

            yield return new object[] {maxIpv4, maxIpv4, maxIpv4};
            yield return new object[] {maxIpv4, minIpv4, maxIpv4};
            yield return new object[] {maxIpv4, maxIpv4, minIpv4};

            var minIpv6 = IPAddress.Parse("abc::01");
            var maxIpv6 = IPAddress.Parse("ffff::f123");

            yield return new object[] {maxIpv6, maxIpv6, maxIpv6};
            yield return new object[] {maxIpv6, minIpv6, maxIpv6};
            yield return new object[] {maxIpv6, maxIpv6, minIpv6};
        }

        [Theory]
        [MemberData(nameof(Max_Test_Values))]
        public void Max_Test(IPAddress expected,
                             IPAddress left,
                             IPAddress right)
        {
            // Arrange
            // Act
            var result = IPAddressMath.Max(left, right);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void Max_NullInput_Throws_ArgumentNullException_Test()
        {
            // Arrange
            // Act
            // Assert
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => IPAddressMath.Max(null, IPAddress.Any));
            Assert.Throws<ArgumentNullException>(() => IPAddressMath.Max(IPAddress.Any, null));
            Assert.Throws<ArgumentNullException>(() => IPAddressMath.Max(null, null));
        }

        [Fact]
        public void Max_MismatchedAddressFamily_Throws_InvalidOperationException_Test()
        {
            // Arrange
            // Act
            // Assert
            Assert.Throws<InvalidOperationException>(() => IPAddressMath.Max(IPAddress.Any, IPAddress.IPv6Any));
            Assert.Throws<InvalidOperationException>(() => IPAddressMath.Max(IPAddress.IPv6Any, IPAddress.Any));
        }

        #endregion // end: Max

        #region Min

        public static IEnumerable<object[]> Min_Test_Values()
        {
            var minIpv4 = IPAddress.Parse("192.168.1.1");
            var maxIpv4 = IPAddress.Parse("192.168.100.1");

            yield return new object[] {minIpv4, minIpv4, minIpv4};
            yield return new object[] {minIpv4, minIpv4, maxIpv4};
            yield return new object[] {minIpv4, maxIpv4, minIpv4};

            var minIpv6 = IPAddress.Parse("abc::01");
            var maxIpv6 = IPAddress.Parse("ffff::f123");

            yield return new object[] {minIpv6, minIpv6, minIpv6};
            yield return new object[] {minIpv6, minIpv6, maxIpv6};
            yield return new object[] {minIpv6, maxIpv6, minIpv6};
        }

        [Theory]
        [MemberData(nameof(Min_Test_Values))]
        public void Min_Test(IPAddress expected,
                             IPAddress left,
                             IPAddress right)
        {
            // Arrange
            // Act
            var result = IPAddressMath.Min(left, right);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void Min_NullInput_Throws_ArgumentNullException_Test()
        {
            // Arrange
            // Act
            // Assert
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => IPAddressMath.Min(null, IPAddress.Any));
            Assert.Throws<ArgumentNullException>(() => IPAddressMath.Min(IPAddress.Any, null));
            Assert.Throws<ArgumentNullException>(() => IPAddressMath.Min(null, null));
        }

        [Fact]
        public void Min_MismatchedAddressFamily_Throws_InvalidOperationException_Test()
        {
            // Arrange
            // Act
            // Assert
            Assert.Throws<InvalidOperationException>(() => IPAddressMath.Min(IPAddress.Any, IPAddress.IPv6Any));
            Assert.Throws<InvalidOperationException>(() => IPAddressMath.Min(IPAddress.IPv6Any, IPAddress.Any));
        }

        #endregion // end: Min

        #region Increment

        [Theory]
        [InlineData("::", "::", 0)]
        [InlineData("::1", "::", 1)]
        [InlineData("abcd::53b0", "abcd::7ac0", -10000)]
        [InlineData("abcd::72c0", "abcd::7ac0", -2048)]
        [InlineData("abcd::82c0", "abcd::7ac0", 2048)]
        [InlineData("abcd::a1d0", "abcd::7ac0", 10000)]
        [InlineData("ffff:ffff:ffff:ffff:ffff:ffff:ffff:fffe", "ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff", -1)]
        [InlineData("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff", "ffff:ffff:ffff:ffff:ffff:ffff:ffff:fffe", 1)]
        [InlineData("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff", "ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff", 0)]
        [InlineData("192.167.253.0", "192.168.1.0", -1024)]
        [InlineData("192.168.0.255", "192.168.1.0", -1)]
        [InlineData("192.168.1.0", "192.168.1.0", 0)]
        [InlineData("192.168.1.0", "192.168.1.255", -255)]
        [InlineData("192.168.1.1", "192.168.1.0", 1)]
        [InlineData("192.168.1.2", "192.168.1.0", 2)]
        [InlineData("192.168.1.255", "192.168.1.0", 255)]
        [InlineData("192.168.5.0", "192.168.1.0", 1024)]
        [InlineData("255.255.255.254", "255.255.255.255", -1)]
        [InlineData("255.255.255.255", "255.255.255.254", 1)]
        [InlineData("255.255.255.255", "255.255.255.255", 0)]
        public void Increment_Test(string expected,
                                   string input,
                                   long delta)
        {
            // Arrange
            var address = IPAddress.Parse(input);

            // Act
            var result = address.Increment(delta);

            // Assert
            Assert.Equal(expected, result.ToString());
        }

        [Theory]
        [InlineData("::", -1)]
        [InlineData("::FF", -1024)]
        [InlineData("0.0.0.0", -1)]
        [InlineData("0.0.0.255", -1024)]
        public void Increment_Underflow_Throws_InvalidOperationException_Test(string input,
                                                                              long delta)
        {
            // Arrange
            var address = IPAddress.Parse(input);

            // Act
            // Assert
            Assert.Throws<InvalidOperationException>(() => address.Increment(delta));
        }

        [Theory]
        [InlineData("255.255.255.0", 1024)]
        [InlineData("255.255.255.255", 1)]
        [InlineData("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ff", 65535)]
        [InlineData("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff", 1)]
        public void Increment_OverflowThrows_InvalidOperationException_Test(string input,
                                                                            long delta)
        {
            // Arrange
            var address = IPAddress.Parse(input);

            // Act
            // Assert
            Assert.Throws<InvalidOperationException>(() => address.Increment(delta));
        }

        [Fact]
        public void Increment_NullInput_Throws_ArgumentNullException_Test()
        {
            // Arrange
            // Act
            // Assert
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => ((IPAddress) null).Increment());
        }

        #region TryIncrement

        [Theory]
        [InlineData(false, null, null, 0)]
        [InlineData(false, null, "255.255.255.0", 1024)]
        [InlineData(false, null, "255.255.255.255", 1)]
        [InlineData(false, null, "ffff:ffff:ffff:ffff:ffff:ffff:ffff:ff", 65535)]
        [InlineData(false, null, "ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff", 1)]
        [InlineData(false, null, "::", -1)]
        [InlineData(false, null, "::FF", -1024)]
        [InlineData(false, null, "0.0.0.0", -1)]
        [InlineData(false, null, "0.0.0.255", -1024)]
        [InlineData(true, "::", "::", 0)]
        [InlineData(true, "::1", "::", 1)]
        [InlineData(true, "abcd::53b0", "abcd::7ac0", -10000)]
        [InlineData(true, "abcd::72c0", "abcd::7ac0", -2048)]
        [InlineData(true, "abcd::82c0", "abcd::7ac0", 2048)]
        [InlineData(true, "abcd::a1d0", "abcd::7ac0", 10000)]
        [InlineData(true, "ffff:ffff:ffff:ffff:ffff:ffff:ffff:fffe", "ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff", -1)]
        [InlineData(true, "ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff", "ffff:ffff:ffff:ffff:ffff:ffff:ffff:fffe", 1)]
        [InlineData(true, "ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff", "ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff", 0)]
        [InlineData(true, "192.167.253.0", "192.168.1.0", -1024)]
        [InlineData(true, "192.168.0.255", "192.168.1.0", -1)]
        [InlineData(true, "192.168.1.0", "192.168.1.0", 0)]
        [InlineData(true, "192.168.1.0", "192.168.1.255", -255)]
        [InlineData(true, "192.168.1.1", "192.168.1.0", 1)]
        [InlineData(true, "192.168.1.2", "192.168.1.0", 2)]
        [InlineData(true, "192.168.1.255", "192.168.1.0", 255)]
        [InlineData(true, "192.168.5.0", "192.168.1.0", 1024)]
        [InlineData(true, "255.255.255.254", "255.255.255.255", -1)]
        [InlineData(true, "255.255.255.255", "255.255.255.254", 1)]
        [InlineData(true, "255.255.255.255", "255.255.255.255", 0)]
        public void TryIncrement_Test(bool expectedSuccess,
                                      string expectedResultString,
                                      string inputString,
                                      long delta)
        {
            // Arrange
            _ = IPAddress.TryParse(inputString, out var input);

            // Act
            var successResult = IPAddressMath.TryIncrement(input, out var result, delta);

            // Assert
            Assert.Equal(expectedSuccess, successResult);

            _ = IPAddress.TryParse(expectedResultString, out var expectResultAddress);
            Assert.Equal(expectResultAddress, result);
        }

        #endregion // end: TryIncrement

        #endregion // end: Increment
    }
}
