using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using Arcus.Math;
using JetBrains.Annotations;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace Arcus.Tests
{
    [PublicAPI]
    public class AbstractIPAddressRangeTests
    {
        #region Setup / Teardown

        public AbstractIPAddressRangeTests(ITestOutputHelper testOutputHelper)
        {
            this._testOutputHelper = testOutputHelper;
        }

        private readonly ITestOutputHelper _testOutputHelper;

        #endregion

        #region Deconstructors

        [Fact]
        public void Deconstruct_Head_Tail_Test()
        {
            // Arrange
            const string headString = "192.168.1.1";
            const string tailString = "192.168.1.3";
            var head = IPAddress.Parse(headString);
            var tail = IPAddress.Parse(tailString);

            // Act
            var (resultHead, resultTail) = CreateMockAbstractIPAddressRange(head, tail);

            // Assert
            Assert.Equal(resultHead, head);
            Assert.Equal(resultTail, tail);
        }

        #endregion // end: Deconstructors

        #region other members

        private static AbstractIPAddressRange CreateMockAbstractIPAddressRange(IPAddress head,
                                                                               IPAddress tail)
        {
            return new Mock<AbstractIPAddressRange>(MockBehavior.Strict, head, tail).Object;
        }

        #endregion

        #region IEnumerable

        [Theory]
        [InlineData("::", "::")]
        [InlineData("::", "::FF")]
        [InlineData("0.0.0.0", "0.0.0.0")]
        [InlineData("192.168.1.1", "192.168.1.1")]
        [InlineData("192.168.1.1", "192.168.1.5")]
        [InlineData("255.255.255.128", "255.255.255.255")]
        [InlineData("255.255.255.255", "255.255.255.255")]
        [InlineData("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ff00", "ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff")]
        [InlineData("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff", "ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff")]
        public void GetEnumerator_Test(string headAddressString,
                                       string tailAddressString)
        {
            // Arrange
            var head = IPAddress.Parse(headAddressString);
            var tail = IPAddress.Parse(tailAddressString);

            var iPAddressRange = CreateMockAbstractIPAddressRange(head, tail);

            // Act
            var ipAddressArray = iPAddressRange.ToArray();

            // Assert
            Assert.Equal(iPAddressRange.Count(), ipAddressArray.Length);
            Assert.Equal(head, ipAddressArray.First());
            Assert.Equal(tail, ipAddressArray.Last());
        }

        [Theory]
        [InlineData("::", "::")]
        [InlineData("::", "::FF")]
        [InlineData("0.0.0.0", "0.0.0.0")]
        [InlineData("192.168.1.1", "192.168.1.1")]
        [InlineData("192.168.1.1", "192.168.1.5")]
        [InlineData("255.255.255.128", "255.255.255.255")]
        [InlineData("255.255.255.255", "255.255.255.255")]
        [InlineData("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ff00", "ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff")]
        [InlineData("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff", "ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff")]
        public void GetIEnumerableEnumerator_Test(string headAddressString,
                                                  string tailAddressString)
        {
            // Arrange
            var head = IPAddress.Parse(headAddressString);
            var tail = IPAddress.Parse(tailAddressString);

            var iPAddressRange = CreateMockAbstractIPAddressRange(head, tail);

            // Act
            var result = new List<IPAddress>();
            var enumerator = ((IEnumerable) iPAddressRange).GetEnumerator();
            while (enumerator.MoveNext())
            {
                result.Add(enumerator.Current as IPAddress);
            }

            // Assert
            Assert.Equal(iPAddressRange.Count(), result.Count);
            Assert.Equal(head, result.First());
            Assert.Equal(tail, result.Last());
        }

        #endregion // end: IEnumerable

        #region IsSingleIP

        public static IEnumerable<object[]> IsSingleIP_Test_Values()
        {
            yield return new object[] {true, CreateMockAbstractIPAddressRange(IPAddress.Any, IPAddress.Any)};
            yield return new object[] {true, CreateMockAbstractIPAddressRange(IPAddress.IPv6Any, IPAddress.IPv6Any)};

            yield return new object[] {false, CreateMockAbstractIPAddressRange(IPAddress.Any, IPAddress.Broadcast)};
            yield return new object[] {false, CreateMockAbstractIPAddressRange(IPAddress.IPv6Any, IPAddress.IPv6Loopback)};
        }

        [Theory]
        [MemberData(nameof(IsSingleIP_Test_Values))]
        public void IsSingleIP_Test(bool expected,
                                    AbstractIPAddressRange ipAddressRange)
        {
            // Arrange
            // Act
            var isSingleIP = ipAddressRange.IsSingleIP;

            // Assert
            Assert.Equal(expected, isSingleIP);
        }

        #endregion // end: IsSingleIP

        #region Length / TryGetLength

        public static IEnumerable<object[]> Length_Test_Values()
        {
            // single address
            yield return new object[] {new BigInteger(1), CreateMockAbstractIPAddressRange(IPAddress.Any, IPAddress.Any)};
            yield return new object[] {new BigInteger(1), CreateMockAbstractIPAddressRange(IPAddress.IPv6Any, IPAddress.IPv6Any)};

            // maximum length ipv4
            yield return new object[] {BigInteger.Pow(2, 32), CreateMockAbstractIPAddressRange(IPAddress.Parse("0.0.0.0"), IPAddress.Parse("255.255.255.255"))};

            // maximum length ipv6
            yield return new object[] {BigInteger.Pow(2, 128), CreateMockAbstractIPAddressRange(IPAddress.Parse("::"), IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff"))};

            // ipv6 length at int.MaxValue
            yield return new object[]
                         {
                             new BigInteger(int.MaxValue), CreateMockAbstractIPAddressRange(IPAddress.Parse("::"),
                                                                                            IPAddress.Parse("::")
                                                                                                     .Increment(int.MaxValue - 1))
                         };

            // ipv6 length at long.MaxValue
            yield return new object[]
                         {
                             new BigInteger(long.MaxValue), CreateMockAbstractIPAddressRange(IPAddress.Parse("::"),
                                                                                             IPAddress.Parse("::")
                                                                                                      .Increment(long.MaxValue - 1))
                         };

            // ipv6 length at int.MaxValue + 1
            yield return new object[]
                         {
                             new BigInteger(int.MaxValue) + 1, CreateMockAbstractIPAddressRange(IPAddress.Parse("::"),
                                                                                                IPAddress.Parse("::")
                                                                                                         .Increment(int.MaxValue))
                         };

            // ipv6 length at long.MaxValue + 1
            yield return new object[]
                         {
                             new BigInteger(long.MaxValue) + 1, CreateMockAbstractIPAddressRange(IPAddress.Parse("::"),
                                                                                                 IPAddress.Parse("::")
                                                                                                          .Increment(long.MaxValue))
                         };
        }

        [Theory]
        [MemberData(nameof(Length_Test_Values))]
        public static void Length_Test(BigInteger expected,
                                       AbstractIPAddressRange ipAddressRange)
        {
            // Arrange
            // Act

            var result = ipAddressRange.Length;

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [MemberData(nameof(Length_Test_Values))]
        public static void TryGetLength_Integer_Test(BigInteger expected,
                                                     AbstractIPAddressRange ipAddressRange)
        {
            // Arrange
            // Act
            var success = ipAddressRange.TryGetLength(out int length);

            // Assert
            Assert.Equal(expected <= int.MaxValue, success);
            Assert.Equal(expected <= int.MaxValue
                             ? (int) expected
                             : -1,
                         length);
        }

        [Theory]
        [MemberData(nameof(Length_Test_Values))]
        public static void TryGetLength_Long_Test(BigInteger expected,
                                                  AbstractIPAddressRange ipAddressRange)
        {
            // Arrange
            // Act
            var success = ipAddressRange.TryGetLength(out long length);

            // Assert
            Assert.Equal(expected <= long.MaxValue, success);
            Assert.Equal(expected <= long.MaxValue
                             ? (long) expected
                             : -1,
                         length);
        }

        #endregion // end: Length / TryGetLength

        #region Contains

        #region Contains(IPAddress)

        [Theory]
        [InlineData(true, "::", "::ABCD", "::1234")]                        // IPv6 range contains IPv6
        [InlineData(false, "::", "::ABCD", "::FFFF")]                       // IPv6 range  doesn't contains IPv6
        [InlineData(true, "::", "::ABCD", "::")]                            // IPv6 range contains IPv6 head
        [InlineData(true, "::", "::ABCD", "::ABCD")]                        // IPv6 range contains IPv6 tail
        [InlineData(false, "::", "::ABCD", "192.168.1.1")]                  // IPv6 range doesn't contains IPv4
        [InlineData(false, "::", "::ABCD", null)]                           // IPv6 range doesn't contains null
        [InlineData(true, "192.168.0.1", "192.168.0.254", "192.168.0.128")] // IPv4 range contains IPv4
        [InlineData(false, "192.168.0.1", "192.168.0.254", "10.1.1.1")]     // IPv4 range doesn't contains IPv4
        [InlineData(true, "192.168.0.1", "192.168.0.254", "192.168.0.1")]   // IPv4 range contains IPv4 head
        [InlineData(true, "192.168.0.1", "192.168.0.254", "192.168.0.254")] // IPv4 range contains IPv4 tail
        [InlineData(false, "192.168.0.1", "192.168.0.254", "::")]           // IPv4 range doesn't contains IPv6
        [InlineData(false, "192.168.0.1", "192.168.0.254", null)]           // IPv4 range doesn't contains null
        public void ContainsIPAddress_Test(bool expected,
                                           string headString,
                                           string tailString,
                                           string addressString)
        {
            // Arrange
            var head = IPAddress.Parse(headString);
            var tail = IPAddress.Parse(tailString);
            _ = IPAddress.TryParse(addressString, out var containsAddress);

            var iPAddressRange = CreateMockAbstractIPAddressRange(head, tail);

            // Act
            var result = iPAddressRange.Contains(containsAddress);

            // Assert
            Assert.Equal(expected, result);
        }

        #endregion // end: Contains(IPAddress)

        #region Contains(IIPAddressRange)

        [Theory]
        [InlineData(true, "::", "::ABCD", "::", "::ABCD")]                         // IPv6 range contains self
        [InlineData(true, "::", "::ABCD", "::1", "::1234")]                        // IPv6 range contains internal IPv6 range
        [InlineData(false, "::", "::ABCD", "AA::FFFF", "AB::FFFF")]                // IPv6 range doesn't contains external IPv6 range
        [InlineData(false, "A::", "A::ABCD", "::", "A::")]                         // IPv6 range  doesn't contains IPv6 overhang head
        [InlineData(false, "::", "::ABCD", "::", "AA::FFFF")]                      // IPv6 range  doesn't contains IPv6 overhang tail
        [InlineData(false, "::", "::ABCD", "192.168.1.1", "192.168.1.10")]         // IPv6 range doesn't contains IPv4 range
        [InlineData(false, "::", "A::ABCD", null, null)]                           // IPv6 range  doesn't contain null range
        [InlineData(true, "128.0.0.1", "128.0.0.254", "128.0.0.1", "128.0.0.254")] // IPv4 range contains self
        [InlineData(true, "128.0.0.1", "128.0.0.254", "128.0.0.5", "128.0.0.100")] // IPv4 range contains internal IPv4 range
        [InlineData(false, "128.0.0.1", "128.0.0.254", "140.0.0.0", "145.0.0.0")]  // IPv4 range doesn't contains external IPv4 range
        [InlineData(false, "128.0.0.1", "128.0.0.254", "", "128.0.0.100")]         // IPv4 range doesn't contains null head IPv4 range
        [InlineData(false, "128.0.0.1", "128.0.0.254", "128.0.0.5", "")]           // IPv4 range doesn't contains null tail IPv4 range
        [InlineData(false, "128.0.0.1", "128.0.0.254", "0.0.0.0", "128.0.0.100")]  // IPv4 range  doesn't contains IPv4 overhang head
        [InlineData(false, "128.0.0.1", "128.0.0.254", "128.0.0.5", "145.0.0.0")]  // IPv4 range  doesn't contains IPv4 overhang tail
        [InlineData(false, "128.0.0.1", "128.0.0.254", "::ABCD", "42::ABCD")]      // IPv4 range doesn't contains IPv6 range
        [InlineData(false, "128.0.0.1", "128.0.0.254", null, null)]                // IPv4 range  doesn't contain null range
        public void ContainsIPAddressRange_Test(bool expected,
                                                string headString,
                                                string tailString,
                                                string containsHeadString,
                                                string containsTailString)
        {
            // Arrange
            var head = IPAddress.Parse(headString);
            var tail = IPAddress.Parse(tailString);

            var mockIPAddressSubRange = IPAddress.TryParse(containsHeadString, out var subhead)
                                        && IPAddress.TryParse(containsTailString, out var subtail)
                                            ? new Mock<AbstractIPAddressRange>(subhead, subtail)
                                            : null;

            var iPAddressRange = CreateMockAbstractIPAddressRange(head, tail);

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            var containedIPAddressRange = mockIPAddressSubRange?.Object;

            // Act
            var result = iPAddressRange.Contains(containedIPAddressRange);

            // Assert
            Assert.Equal(expected, result);
        }

        #endregion // end: Contains(IIPAddressRange)

        #endregion // end: Contains

        #region Class

        [Fact]
        public void Implementation_Test()
        {
            // Arrange
            var type = typeof(AbstractIPAddressRange);

            // Act
            // Assert
            Assert.True(typeof(IIPAddressRange).IsAssignableFrom(type));
        }

        [Fact]
        public void AbstractClass_Test()
        {
            // Arrange
            var type = typeof(AbstractIPAddressRange);

            // Act
            var isAbstract = type.IsAbstract;

            // Assert
            Assert.True(isAbstract);
        }

        #endregion // end: Class

        #region  IEnumerable / IEnumerable<IPAddress>

        [Fact] //Test that the expected addresses appear in the given IPv6 range
        public void Enumerable_IPv6_ContainsExpected_Test()
        {
            // Arrange
            const string headString = "::a";
            const string tailString = "::c";
            var head = IPAddress.Parse(headString);
            var tail = IPAddress.Parse(tailString);

            var iPAddressRange = CreateMockAbstractIPAddressRange(head, tail);

            // Act
            var result = iPAddressRange.ToList();

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Equal(new[] {head, IPAddress.Parse("::b"), tail}, result);
        }

        [Fact]
        public void Enumerable_IPv6_TakePastEnd_Test()
        {
            // Arrange
            var head = IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:ffff:ffff:fff0");
            var tail = IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff");

            var iPAddressRange = CreateMockAbstractIPAddressRange(head, tail);

            // Act
            var result = iPAddressRange.Take(100)
                                       .ToList();

            // Assert
            Assert.Equal(16, result.Count);
            Assert.Equal(tail, result.Last());
        }

        [Fact]
        public void Enumerable_IPv4_TakePastEnd_Test()
        {
            // Arrange
            var head = IPAddress.Parse("255.255.255.240");
            var tail = IPAddress.Parse("255.255.255.255");

            var iPAddressRange = CreateMockAbstractIPAddressRange(head, tail);

            // Act
            var result = iPAddressRange.Take(100)
                                       .ToList();

            // Assert
            Assert.Equal(16, result.Count);
            Assert.Equal(tail, result.Last());
        }

        [Fact] // Test that the expected addresses appear in the given IPv4 range
        public void Enumerable_IPv4_ContainsExpected_Test()
        {
            // Arrange
            const string headString = "192.168.1.1";
            const string tailString = "192.168.1.3";
            var head = IPAddress.Parse(headString);
            var tail = IPAddress.Parse(tailString);

            var iPAddressRange = CreateMockAbstractIPAddressRange(head, tail);

            // Act
            var result = iPAddressRange.ToList();

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Equal(new[] {head, IPAddress.Parse("192.168.1.2"), tail}, result);
        }

        [Fact] // Test that a range of one returns a single address when Addresses is called
        public void Enumerable_SameHead_And_SameTail_ReturnsSingle_Test()
        {
            // Arrange
            var ipAddress = IPAddress.Parse("192.168.1.1");

            var iPAddressRange = CreateMockAbstractIPAddressRange(ipAddress, ipAddress);

            // Act
            var addresses = iPAddressRange.ToArray();

            // Assert
            Assert.Single(addresses);
            Assert.Equal(ipAddress, addresses.Single());
        }

        [Fact]
        public void Enumerable_ReasonableIteration_Test()
        {
            // Arrange

            var iPAddressRange = CreateMockAbstractIPAddressRange(IPAddress.Parse("::"), IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff"));

            // Act
            var addresses = iPAddressRange.Skip(10)
                                          .Take(10)
                                          .ToArray();

            // Assert
            Assert.NotNull(addresses); // if this test completed it likely means that we didn't iterate through 2^128 ip addresses to skip 10 and take 10
        }

        #endregion // end: IEnumerable / IEnumerable<IPAddress>

        #region Ctor

        [Theory]
        [InlineData("192.168.1.1", "192.168.1.5")]
        [InlineData("::beef", "::dead")]
        public void Ctor_HappyPath_Test(string headString,
                                        string tailString)
        {
            // Arrange
            var head = IPAddress.Parse(headString);
            var tail = IPAddress.Parse(tailString);

            // Act
            var iPAddressRange = CreateMockAbstractIPAddressRange(head, tail);

            // Assert
            Assert.Equal(head, iPAddressRange.Head);
            Assert.Equal(tail, iPAddressRange.Tail);
        }

        [Theory]
        [InlineData("192.168.1.1", null)]
        [InlineData(null, "192.168.1.5")]
        [InlineData(null, null)]
        public void Ctor_Null_Input_Throws_ArgumentNullException_Test(string headString,
                                                                      string tailString)
        {
            // Arrange
            var head = headString != null
                           ? IPAddress.Parse(headString)
                           : null;

            var tail = tailString != null
                           ? IPAddress.Parse(tailString)
                           : null;

            // Act
            // Assert
            var exception = Assert.ThrowsAny<Exception>(() => CreateMockAbstractIPAddressRange(head, tail));
            Assert.IsAssignableFrom<ArgumentNullException>(exception.InnerException);
        }

        [Theory]
        [InlineData("192.168.1.1", "::beef")]
        [InlineData("::beef", "192.168.1.5")]
        public void Ctor_MismatchAddressFamilies_Throws_InvalidOperationException_Test(string headString,
                                                                                       string tailString)
        {
            // Arrange
            var head = IPAddress.Parse(headString);
            var tail = IPAddress.Parse(tailString);

            // Act
            // Assert
            var exception = Assert.ThrowsAny<Exception>(() => CreateMockAbstractIPAddressRange(head, tail));
            Assert.IsAssignableFrom<InvalidOperationException>(exception.InnerException);
        }

        [Theory]
        [InlineData("192.168.1.5", "192.168.1.1")]
        [InlineData("::dead", "::beef")]
        public void Ctor_BadAddressSequencing_Throws_InvalidOperationException_Test(string headString,
                                                                                    string tailString)
        {
            // Arrange
            var head = IPAddress.Parse(headString);
            var tail = IPAddress.Parse(tailString);

            // Act
            // Assert
            var exception = Assert.ThrowsAny<Exception>(() => CreateMockAbstractIPAddressRange(head, tail));
            Assert.IsAssignableFrom<InvalidOperationException>(exception.InnerException);
        }

        #endregion // end: Ctor

        #region AddressFamily

        [Fact]
        public void AddressFamily_IPv4_Test()
        {
            // Arrange
            const string headString = "192.168.1.1";
            const string tailString = "192.168.1.5";

            var head = IPAddress.Parse(headString);
            var tail = IPAddress.Parse(tailString);

            var iPAddressRange = CreateMockAbstractIPAddressRange(head, tail);

            // Act
            // Assert
            Assert.Equal(AddressFamily.InterNetwork, iPAddressRange.AddressFamily);
            Assert.True(iPAddressRange.IsIPv4);
            Assert.False(iPAddressRange.IsIPv6);
        }

        [Fact]
        public void AddressFamily_IPv6_Test()
        {
            // Arrange
            const string headString = "::beef";
            const string tailString = "::dead";

            var head = IPAddress.Parse(headString);
            var tail = IPAddress.Parse(tailString);

            var iPAddressRange = CreateMockAbstractIPAddressRange(head, tail);

            // Act
            // Assert
            Assert.Equal(AddressFamily.InterNetworkV6, iPAddressRange.AddressFamily);
            Assert.False(iPAddressRange.IsIPv4);
            Assert.True(iPAddressRange.IsIPv6);
        }

        #endregion // end: AddressFamily

        #region Set Operations

        #region Contains

        #region Contains IIPAddressRange

        public static IEnumerable<object[]> Contains_IIPAddressRange_Test_Values()
        {
            var ipv4Range = CreateMockAbstractIPAddressRange(IPAddress.Any, IPAddress.Broadcast);
            var ipv6Range = CreateMockAbstractIPAddressRange(IPAddress.IPv6Any, IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff"));

            // null overlap checking
            yield return new object[] {false, ipv4Range, null};
            yield return new object[] {false, ipv6Range, null};

            // same overlap checking
            yield return new object[] {true, ipv4Range, ipv4Range};
            yield return new object[] {true, ipv6Range, ipv6Range};

            // equal overlap checking
            yield return new object[] {true, CreateMockAbstractIPAddressRange(IPAddress.Any, IPAddress.Broadcast), CreateMockAbstractIPAddressRange(IPAddress.Any, IPAddress.Broadcast)};
            yield return new object[] {true, CreateMockAbstractIPAddressRange(IPAddress.IPv6Any, IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff")), CreateMockAbstractIPAddressRange(IPAddress.IPv6Any, IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff"))};

            // differing address families
            yield return new object[] {false, ipv4Range, ipv6Range};
            yield return new object[] {false, ipv6Range, ipv4Range};

            // head only overlapped
            yield return new object[] {false, CreateMockAbstractIPAddressRange(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("192.168.1.128")), CreateMockAbstractIPAddressRange(IPAddress.Parse("192.168.1.128"), IPAddress.Parse("192.168.1.192"))};
            yield return new object[] {false, CreateMockAbstractIPAddressRange(IPAddress.Parse("::"), IPAddress.Parse("::ffff")), CreateMockAbstractIPAddressRange(IPAddress.Parse("::ffff"), IPAddress.Parse("1::ffff"))};

            // full head and tail overlapped
            yield return new object[] {true, CreateMockAbstractIPAddressRange(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("192.168.1.255")), CreateMockAbstractIPAddressRange(IPAddress.Parse("192.168.1.128"), IPAddress.Parse("192.168.1.192"))};
            yield return new object[] {true, CreateMockAbstractIPAddressRange(IPAddress.Parse("::"), IPAddress.Parse("ffff::ffff")), CreateMockAbstractIPAddressRange(IPAddress.Parse("::ff00"), IPAddress.Parse("ff::ff00"))};

            // tail only overlapped
            yield return new object[] {false, CreateMockAbstractIPAddressRange(IPAddress.Parse("192.168.1.192"), IPAddress.Parse("192.168.1.255")), CreateMockAbstractIPAddressRange(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("192.168.1.192"))};
            yield return new object[] {false, CreateMockAbstractIPAddressRange(IPAddress.Parse("::ffff"), IPAddress.Parse("1::ffff")), CreateMockAbstractIPAddressRange(IPAddress.Parse("::"), IPAddress.Parse("::ffff"))};

            // not touching
            yield return new object[] {false, CreateMockAbstractIPAddressRange(IPAddress.Parse("192.168.1.1"), IPAddress.Parse("192.168.1.128")), CreateMockAbstractIPAddressRange(IPAddress.Parse("10.1.1.0"), IPAddress.Parse("10.1.1.100"))};
            yield return new object[] {false, CreateMockAbstractIPAddressRange(IPAddress.Parse("ab::"), IPAddress.Parse("ab::f")), CreateMockAbstractIPAddressRange(IPAddress.Parse("ef::"), IPAddress.Parse("ef::f"))};

            // disparate ranges
            yield return new object[] {false, CreateMockAbstractIPAddressRange(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("192.168.2.0")), CreateMockAbstractIPAddressRange(IPAddress.Parse("10.1.1.1"), IPAddress.Parse("10.1.5.0"))};
            yield return new object[] {false, CreateMockAbstractIPAddressRange(IPAddress.Parse("::"), IPAddress.Parse("::ff")), CreateMockAbstractIPAddressRange(IPAddress.Parse("f::"), IPAddress.Parse("f:1::"))};
        }

        [Theory]
        [MemberData(nameof(Contains_IIPAddressRange_Test_Values))]
        public void Contains_IIPAddressRange_Test(bool expected,
                                                  IIPAddressRange left,
                                                  IIPAddressRange right)
        {
            // Arrange
            // Act
            var result = left.Contains(right);

            // Assert
            Assert.Equal(expected, result);
        }

        #endregion // end: Contains IIPAddressRange

        #region Contians IPAddress

        public static IEnumerable<object[]> Contains_Test_Values()
        {
            var ipv4Range = CreateMockAbstractIPAddressRange(IPAddress.Any, IPAddress.Broadcast);
            var ipv6Range = CreateMockAbstractIPAddressRange(IPAddress.IPv6Any, IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff"));

            // does not contain null
            yield return new object[] {false, ipv4Range, null};
            yield return new object[] {false, ipv6Range, null};

            // differing address families
            yield return new object[] {false, ipv4Range, IPAddress.IPv6Any};
            yield return new object[] {false, ipv6Range, IPAddress.Any};

            // contains head
            yield return new object[] {true, ipv4Range, ipv4Range.Head};
            yield return new object[] {true, ipv6Range, ipv6Range.Head};

            // contains tail
            yield return new object[] {true, ipv4Range, ipv4Range.Tail};
            yield return new object[] {true, ipv6Range, ipv6Range.Tail};

            // contains all inside
            var ipv4InsideRange = CreateMockAbstractIPAddressRange(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("192.168.1.5"));
            foreach (var ip in ipv4InsideRange)
            {
                yield return new object[] {true, ipv4InsideRange, ip};
            }

            var ipv6InsideRange = CreateMockAbstractIPAddressRange(IPAddress.Parse("::ff00"), IPAddress.Parse("::ff0f"));
            foreach (var ip in ipv6InsideRange)
            {
                yield return new object[] {true, ipv6InsideRange, ip};
            }

            // does not contain outside before
            yield return new object[] {false, CreateMockAbstractIPAddressRange(IPAddress.Parse("192.168.1.1"), IPAddress.Parse("192.168.1.200")), IPAddress.Parse("192.168.1.0")};
            yield return new object[] {false, CreateMockAbstractIPAddressRange(IPAddress.Parse("::ff01"), IPAddress.Parse("::ff08")), IPAddress.Parse("::ff00")};

            // does not contain outside after
            yield return new object[] {false, CreateMockAbstractIPAddressRange(IPAddress.Parse("192.168.1.1"), IPAddress.Parse("192.168.1.200")), IPAddress.Parse("192.168.1.201")};
            yield return new object[] {false, CreateMockAbstractIPAddressRange(IPAddress.Parse("::ff01"), IPAddress.Parse("::ff08")), IPAddress.Parse("::ff09")};
        }

        [Theory]
        [MemberData(nameof(Contains_Test_Values))]
        public void Contains_Test(bool expected,
                                  IIPAddressRange range,
                                  IPAddress address)
        {
            // Arrange
            // Act
            var result = range.Contains(address);

            // Assert
            Assert.Equal(expected, result);
        }

        #endregion // end: Contians IPAddress

        #endregion // end: Contains

        #region Ovelap and Touches

        #region HeadOverlappedBy

        public static IEnumerable<object[]> HeadOverlappedBy_Test_Values()
        {
            var ipv4Range = CreateMockAbstractIPAddressRange(IPAddress.Any, IPAddress.Broadcast);
            var ipv6Range = CreateMockAbstractIPAddressRange(IPAddress.IPv6Any, IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff"));

            // null overlap checking
            yield return new object[] {false, ipv4Range, null};
            yield return new object[] {false, ipv6Range, null};

            // same overlap checking
            yield return new object[] {true, ipv4Range, ipv4Range};
            yield return new object[] {true, ipv6Range, ipv6Range};

            // equal overlap checking
            yield return new object[] {true, CreateMockAbstractIPAddressRange(IPAddress.Any, IPAddress.Broadcast), CreateMockAbstractIPAddressRange(IPAddress.Any, IPAddress.Broadcast)};
            yield return new object[] {true, CreateMockAbstractIPAddressRange(IPAddress.IPv6Any, IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff")), CreateMockAbstractIPAddressRange(IPAddress.IPv6Any, IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff"))};

            // differing address families
            yield return new object[] {false, ipv4Range, ipv6Range};
            yield return new object[] {false, ipv6Range, ipv4Range};

            // head only overlapped
            yield return new object[] {true, CreateMockAbstractIPAddressRange(IPAddress.Parse("192.168.1.128"), IPAddress.Parse("192.168.1.192")), CreateMockAbstractIPAddressRange(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("192.168.1.128"))};
            yield return new object[] {true, CreateMockAbstractIPAddressRange(IPAddress.Parse("::ffff"), IPAddress.Parse("1::ffff")), CreateMockAbstractIPAddressRange(IPAddress.Parse("::"), IPAddress.Parse("::ffff"))};

            // full head and tail overlapped
            yield return new object[] {true, CreateMockAbstractIPAddressRange(IPAddress.Parse("192.168.1.128"), IPAddress.Parse("192.168.1.192")), CreateMockAbstractIPAddressRange(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("192.168.1.255"))};
            yield return new object[] {true, CreateMockAbstractIPAddressRange(IPAddress.Parse("::ff00"), IPAddress.Parse("ff::ff00")), CreateMockAbstractIPAddressRange(IPAddress.Parse("::"), IPAddress.Parse("ffff::ffff"))};

            // tail only overlapped
            yield return new object[] {false, CreateMockAbstractIPAddressRange(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("192.168.1.192")), CreateMockAbstractIPAddressRange(IPAddress.Parse("192.168.1.192"), IPAddress.Parse("192.168.1.255"))};
            yield return new object[] {false, CreateMockAbstractIPAddressRange(IPAddress.Parse("::"), IPAddress.Parse("::ffff")), CreateMockAbstractIPAddressRange(IPAddress.Parse("::ffff"), IPAddress.Parse("1::ffff"))};

            // not touching
            yield return new object[] {false, CreateMockAbstractIPAddressRange(IPAddress.Parse("10.1.1.0"), IPAddress.Parse("10.1.1.100")), CreateMockAbstractIPAddressRange(IPAddress.Parse("192.168.1.1"), IPAddress.Parse("192.168.1.128"))};
            yield return new object[] {false, CreateMockAbstractIPAddressRange(IPAddress.Parse("ef::"), IPAddress.Parse("ef::f")), CreateMockAbstractIPAddressRange(IPAddress.Parse("ab::"), IPAddress.Parse("ab::f"))};

            // disparate ranges
            yield return new object[] {false, CreateMockAbstractIPAddressRange(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("192.168.2.0")), CreateMockAbstractIPAddressRange(IPAddress.Parse("10.1.1.1"), IPAddress.Parse("10.1.5.0"))};
            yield return new object[] {false, CreateMockAbstractIPAddressRange(IPAddress.Parse("::"), IPAddress.Parse("::ff")), CreateMockAbstractIPAddressRange(IPAddress.Parse("f::"), IPAddress.Parse("f:1::"))};
        }

        [Theory]
        [MemberData(nameof(HeadOverlappedBy_Test_Values))]
        public void HeadOverlappedBy_Test(bool expected,
                                          IIPAddressRange left,
                                          IIPAddressRange right)
        {
            // Arrange
            // Act
            var result = left.HeadOverlappedBy(right);

            // Assert
            Assert.Equal(expected, result);
        }

        #endregion // end: HeadOverlappedBy

        #region TailOverlappedBy

        public static IEnumerable<object[]> TailOverlappedBy_Test_Values()
        {
            var ipv4Range = CreateMockAbstractIPAddressRange(IPAddress.Any, IPAddress.Broadcast);
            var ipv6Range = CreateMockAbstractIPAddressRange(IPAddress.IPv6Any, IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff"));

            // null overlap checking
            yield return new object[] {false, ipv4Range, null};
            yield return new object[] {false, ipv6Range, null};

            // same overlap checking
            yield return new object[] {true, ipv4Range, ipv4Range};
            yield return new object[] {true, ipv6Range, ipv6Range};

            // equal overlap checking
            yield return new object[] {true, CreateMockAbstractIPAddressRange(IPAddress.Any, IPAddress.Broadcast), CreateMockAbstractIPAddressRange(IPAddress.Any, IPAddress.Broadcast)};
            yield return new object[] {true, CreateMockAbstractIPAddressRange(IPAddress.IPv6Any, IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff")), CreateMockAbstractIPAddressRange(IPAddress.IPv6Any, IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff"))};

            // differing address families
            yield return new object[] {false, ipv4Range, ipv6Range};
            yield return new object[] {false, ipv6Range, ipv4Range};

            // head only overlapped
            yield return new object[] {false, CreateMockAbstractIPAddressRange(IPAddress.Parse("192.168.1.128"), IPAddress.Parse("192.168.1.192")), CreateMockAbstractIPAddressRange(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("192.168.1.128"))};
            yield return new object[] {false, CreateMockAbstractIPAddressRange(IPAddress.Parse("::ffff"), IPAddress.Parse("1::ffff")), CreateMockAbstractIPAddressRange(IPAddress.Parse("::"), IPAddress.Parse("::ffff"))};

            // full head and tail overlapped
            yield return new object[] {true, CreateMockAbstractIPAddressRange(IPAddress.Parse("192.168.1.128"), IPAddress.Parse("192.168.1.192")), CreateMockAbstractIPAddressRange(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("192.168.1.255"))};
            yield return new object[] {true, CreateMockAbstractIPAddressRange(IPAddress.Parse("::ff00"), IPAddress.Parse("ff::ff00")), CreateMockAbstractIPAddressRange(IPAddress.Parse("::"), IPAddress.Parse("ffff::ffff"))};

            // tail only overlapped
            yield return new object[] {true, CreateMockAbstractIPAddressRange(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("192.168.1.192")), CreateMockAbstractIPAddressRange(IPAddress.Parse("192.168.1.192"), IPAddress.Parse("192.168.1.255"))};
            yield return new object[] {true, CreateMockAbstractIPAddressRange(IPAddress.Parse("::"), IPAddress.Parse("::ffff")), CreateMockAbstractIPAddressRange(IPAddress.Parse("::ffff"), IPAddress.Parse("1::ffff"))};

            // not touching
            yield return new object[] {false, CreateMockAbstractIPAddressRange(IPAddress.Parse("10.1.1.0"), IPAddress.Parse("10.1.1.100")), CreateMockAbstractIPAddressRange(IPAddress.Parse("192.168.1.1"), IPAddress.Parse("192.168.1.128"))};
            yield return new object[] {false, CreateMockAbstractIPAddressRange(IPAddress.Parse("ef::"), IPAddress.Parse("ef::f")), CreateMockAbstractIPAddressRange(IPAddress.Parse("ab::"), IPAddress.Parse("ab::f"))};

            // disparate ranges
            yield return new object[] {false, CreateMockAbstractIPAddressRange(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("192.168.2.0")), CreateMockAbstractIPAddressRange(IPAddress.Parse("10.1.1.1"), IPAddress.Parse("10.1.5.0"))};
            yield return new object[] {false, CreateMockAbstractIPAddressRange(IPAddress.Parse("::"), IPAddress.Parse("::ff")), CreateMockAbstractIPAddressRange(IPAddress.Parse("f::"), IPAddress.Parse("f:1::"))};
        }

        [Theory]
        [MemberData(nameof(TailOverlappedBy_Test_Values))]
        public void TailOverlappedBy_Test(bool expected,
                                          IIPAddressRange left,
                                          IIPAddressRange right)
        {
            // Arrange
            // Act
            var result = left.TailOverlappedBy(right);

            // Assert
            Assert.Equal(expected, result);
        }

        #endregion // end: TailOverlappedBy

        #region Overlaps

        public static IEnumerable<object[]> Overlaps_Test_Values()
        {
            var ipv4Range = CreateMockAbstractIPAddressRange(IPAddress.Any, IPAddress.Broadcast);
            var ipv6Range = CreateMockAbstractIPAddressRange(IPAddress.IPv6Any, IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff"));

            // null overlap checking
            yield return new object[] {false, ipv4Range, null};
            yield return new object[] {false, ipv6Range, null};

            // same overlap checking
            yield return new object[] {true, ipv4Range, ipv4Range};
            yield return new object[] {true, ipv6Range, ipv6Range};

            // equal overlap checking
            yield return new object[] {true, CreateMockAbstractIPAddressRange(IPAddress.Any, IPAddress.Broadcast), CreateMockAbstractIPAddressRange(IPAddress.Any, IPAddress.Broadcast)};
            yield return new object[] {true, CreateMockAbstractIPAddressRange(IPAddress.IPv6Any, IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff")), CreateMockAbstractIPAddressRange(IPAddress.IPv6Any, IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff"))};

            // differing address families
            yield return new object[] {false, ipv4Range, ipv6Range};
            yield return new object[] {false, ipv6Range, ipv4Range};

            // head only overlapped
            yield return new object[] {true, CreateMockAbstractIPAddressRange(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("192.168.1.128")), CreateMockAbstractIPAddressRange(IPAddress.Parse("192.168.1.128"), IPAddress.Parse("192.168.1.192"))};
            yield return new object[] {true, CreateMockAbstractIPAddressRange(IPAddress.Parse("::"), IPAddress.Parse("::ffff")), CreateMockAbstractIPAddressRange(IPAddress.Parse("::ffff"), IPAddress.Parse("1::ffff"))};

            // full head and tail overlapped
            yield return new object[] {true, CreateMockAbstractIPAddressRange(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("192.168.1.255")), CreateMockAbstractIPAddressRange(IPAddress.Parse("192.168.1.128"), IPAddress.Parse("192.168.1.192"))};
            yield return new object[] {true, CreateMockAbstractIPAddressRange(IPAddress.Parse("::"), IPAddress.Parse("ffff::ffff")), CreateMockAbstractIPAddressRange(IPAddress.Parse("::ff00"), IPAddress.Parse("ff::ff00"))};

            // tail only overlapped
            yield return new object[] {true, CreateMockAbstractIPAddressRange(IPAddress.Parse("192.168.1.192"), IPAddress.Parse("192.168.1.255")), CreateMockAbstractIPAddressRange(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("192.168.1.192"))};
            yield return new object[] {true, CreateMockAbstractIPAddressRange(IPAddress.Parse("::ffff"), IPAddress.Parse("1::ffff")), CreateMockAbstractIPAddressRange(IPAddress.Parse("::"), IPAddress.Parse("::ffff"))};

            // not touching
            yield return new object[] {false, CreateMockAbstractIPAddressRange(IPAddress.Parse("192.168.1.1"), IPAddress.Parse("192.168.1.128")), CreateMockAbstractIPAddressRange(IPAddress.Parse("10.1.1.0"), IPAddress.Parse("10.1.1.100"))};
            yield return new object[] {false, CreateMockAbstractIPAddressRange(IPAddress.Parse("ab::"), IPAddress.Parse("ab::f")), CreateMockAbstractIPAddressRange(IPAddress.Parse("ef::"), IPAddress.Parse("ef::f"))};

            // disparate ranges
            yield return new object[] {false, CreateMockAbstractIPAddressRange(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("192.168.2.0")), CreateMockAbstractIPAddressRange(IPAddress.Parse("10.1.1.1"), IPAddress.Parse("10.1.5.0"))};
            yield return new object[] {false, CreateMockAbstractIPAddressRange(IPAddress.Parse("::"), IPAddress.Parse("::ff")), CreateMockAbstractIPAddressRange(IPAddress.Parse("f::"), IPAddress.Parse("f:1::"))};
        }

        [Theory]
        [MemberData(nameof(Overlaps_Test_Values))]
        public void Overlaps_Test(bool expected,
                                  IIPAddressRange left,
                                  IIPAddressRange right)
        {
            // Arrange
            // Act
            var result = left.Overlaps(right);

            // Assert
            Assert.Equal(expected, result);
        }

        #endregion // end: Overlaps

        #region Touches

        public static IEnumerable<object[]> Touches_Test_Values()
        {
            var ipv4Range = CreateMockAbstractIPAddressRange(IPAddress.Any, IPAddress.Broadcast);
            var ipv6Range = CreateMockAbstractIPAddressRange(IPAddress.IPv6Any, IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff"));

            // null overlap checking
            yield return new object[] {false, CreateMockAbstractIPAddressRange(IPAddress.Any, IPAddress.Broadcast), null};
            yield return new object[] {false, CreateMockAbstractIPAddressRange(IPAddress.IPv6Any, IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff")), null};

            // differing address families
            yield return new object[] {false, ipv4Range, ipv6Range};
            yield return new object[] {false, ipv6Range, ipv4Range};

            // left tail touches right head
            yield return new object[] {true, CreateMockAbstractIPAddressRange(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("192.168.1.100")), CreateMockAbstractIPAddressRange(IPAddress.Parse("192.168.1.101"), IPAddress.Parse("192.168.1.200"))};
            yield return new object[] {true, CreateMockAbstractIPAddressRange(IPAddress.Parse("::"), IPAddress.Parse("::abcd")), CreateMockAbstractIPAddressRange(IPAddress.Parse("::abce"), IPAddress.Parse("::ffff"))};

            // left head touches right tail
            yield return new object[] {true, CreateMockAbstractIPAddressRange(IPAddress.Parse("192.168.1.101"), IPAddress.Parse("192.168.1.200")), CreateMockAbstractIPAddressRange(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("192.168.1.100"))};
            yield return new object[] {true, CreateMockAbstractIPAddressRange(IPAddress.Parse("::abce"), IPAddress.Parse("::ffff")), CreateMockAbstractIPAddressRange(IPAddress.Parse("::"), IPAddress.Parse("::abcd"))};

            // head only overlapped
            yield return new object[] {false, CreateMockAbstractIPAddressRange(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("192.168.1.128")), CreateMockAbstractIPAddressRange(IPAddress.Parse("192.168.1.128"), IPAddress.Parse("192.168.1.192"))};
            yield return new object[] {false, CreateMockAbstractIPAddressRange(IPAddress.Parse("::"), IPAddress.Parse("::ffff")), CreateMockAbstractIPAddressRange(IPAddress.Parse("::ffff"), IPAddress.Parse("1::ffff"))};

            // full head and tail overlapped
            yield return new object[] {false, CreateMockAbstractIPAddressRange(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("192.168.1.255")), CreateMockAbstractIPAddressRange(IPAddress.Parse("192.168.1.128"), IPAddress.Parse("192.168.1.192"))};
            yield return new object[] {false, CreateMockAbstractIPAddressRange(IPAddress.Parse("::"), IPAddress.Parse("ffff::ffff")), CreateMockAbstractIPAddressRange(IPAddress.Parse("::ff00"), IPAddress.Parse("ff::ff00"))};

            // tail only overlapped
            yield return new object[] {false, CreateMockAbstractIPAddressRange(IPAddress.Parse("192.168.1.192"), IPAddress.Parse("192.168.1.255")), CreateMockAbstractIPAddressRange(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("192.168.1.192"))};
            yield return new object[] {false, CreateMockAbstractIPAddressRange(IPAddress.Parse("::ffff"), IPAddress.Parse("1::ffff")), CreateMockAbstractIPAddressRange(IPAddress.Parse("::"), IPAddress.Parse("::ffff"))};

            // disparate ranges
            yield return new object[] {false, CreateMockAbstractIPAddressRange(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("192.168.2.0")), CreateMockAbstractIPAddressRange(IPAddress.Parse("10.1.1.1"), IPAddress.Parse("10.1.5.0"))};
            yield return new object[] {false, CreateMockAbstractIPAddressRange(IPAddress.Parse("::"), IPAddress.Parse("::ff")), CreateMockAbstractIPAddressRange(IPAddress.Parse("f::"), IPAddress.Parse("f:1::"))};

            // this tail at max
            yield return new object[] {false, CreateMockAbstractIPAddressRange(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("255.255.255.255")), CreateMockAbstractIPAddressRange(IPAddress.Parse("10.1.1.1"), IPAddress.Parse("10.1.5.0"))};
            yield return new object[] {false, CreateMockAbstractIPAddressRange(IPAddress.Parse("::"), IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff")), CreateMockAbstractIPAddressRange(IPAddress.Parse("f::"), IPAddress.Parse("f:1::"))};

            // that tail at max
            yield return new object[] {false, CreateMockAbstractIPAddressRange(IPAddress.Parse("10.1.1.1"), IPAddress.Parse("10.1.5.0")), CreateMockAbstractIPAddressRange(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("255.255.255.255"))};
            yield return new object[] {false, CreateMockAbstractIPAddressRange(IPAddress.Parse("f::"), IPAddress.Parse("f:1::")), CreateMockAbstractIPAddressRange(IPAddress.Parse("::"), IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff"))};
        }

        [Theory]
        [MemberData(nameof(Touches_Test_Values))]
        public void Touches_Test(bool expected,
                                 IIPAddressRange left,
                                 IIPAddressRange right)
        {
            // Arrange
            // Act
            var result = left.Touches(right);

            // Assert
            Assert.Equal(expected, result);
        }

        #endregion // end: Touches

        #endregion // end: Ovelap and Touches

        #endregion // end: Set Operations
    }
}
