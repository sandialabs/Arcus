using System.Net;
using Arcus.Math;
using Arcus.Utilities;

namespace Arcus.Tests
{
    /// <content>
    ///     <see cref="AbstractIPAddressRange"/> tests for <see cref="IIPAddressRange"/>
    /// </content>
    public partial class AbstractIPAddressRangeTests
    {
        #region Deconstructors

        /// <summary>Verifies Deconstruct returns the Head and Tail of the range.</summary>
        [Fact]
        public void Deconstruct_Head_Tail_Test()
        {
            // Arrange
            const string headString = "192.168.1.1";
            const string tailString = "192.168.1.3";
            var head = IPAddress.Parse(headString);
            var tail = IPAddress.Parse(tailString);

            // Act
            var (resultHead, resultTail) = CreateIPAddressRange(head, tail);

            // Assert
            Assert.Equal(resultHead, head);
            Assert.Equal(resultTail, tail);
        }

        #endregion // end: Deconstructors

        #region IsSingleIP

        /// <summary>
        ///     Gets parameters: expected (bool), ipAddressRange (AbstractIPAddressRange)
        /// </summary>
        /// <value>
        ///     Parameters: expected (bool), ipAddressRange (AbstractIPAddressRange)
        /// </value>
        public static TheoryData<bool, IPAddressRange> IsSingleIP_Test_Data
        {
            get
            {
                var data = new TheoryData<bool, IPAddressRange>
                {
                    { true, CreateIPAddressRange(IPAddress.Any, IPAddress.Any) },
                    { true, CreateIPAddressRange(IPAddress.IPv6Any, IPAddress.IPv6Any) },
                    { false, CreateIPAddressRange(IPAddress.Any, IPAddress.Broadcast) },
                    { false, CreateIPAddressRange(IPAddress.IPv6Any, IPAddress.IPv6Loopback) },
                };
                return data;
            }
        }

        /// <summary>Verifies IsSingleIP returns expected for a range of length one vs longer.</summary>
        /// <param name="expected">Expected result.</param>
        /// <param name="ipAddressRange">The range under test.</param>
        [Theory]
        [MemberData(nameof(IsSingleIP_Test_Data))]
        public void IsSingleIP_Test(bool expected, IPAddressRange ipAddressRange)
        {
            // Arrange
            // Act
            var isSingleIP = ipAddressRange.IsSingleIP;

            // Assert
            Assert.Equal(expected, isSingleIP);
        }

        #endregion // end: IsSingleIP

        #region TryGetLength

        /// <summary>Verifies TryGetLength (int overload) returns success and the correct value when the length fits in int.</summary>
        /// <param name="expected">Expected result.</param>
        /// <param name="ipAddressRange">The range under test.</param>
        [Theory]
        [MemberData(nameof(Length_Test_Data))]
        public void TryGetLength_Integer_Test(System.Numerics.BigInteger expected, IPAddressRange ipAddressRange)
        {
            // Arrange
            // Act
            var success = ipAddressRange.TryGetLength(out int length);

            // Assert
            Assert.Equal(expected <= int.MaxValue, success);
            Assert.Equal(expected <= int.MaxValue ? (int)expected : -1, length);
        }

        /// <summary>Verifies TryGetLength (long overload) returns success and the correct value when the length fits in long.</summary>
        /// <param name="expected">Expected result.</param>
        /// <param name="ipAddressRange">The range under test.</param>
        [Theory]
        [MemberData(nameof(Length_Test_Data))]
        public void TryGetLength_Long_Test(System.Numerics.BigInteger expected, IPAddressRange ipAddressRange)
        {
            // Arrange
            // Act
            var success = ipAddressRange.TryGetLength(out long length);

            // Assert
            Assert.Equal(expected <= long.MaxValue, success);
            Assert.Equal(expected <= long.MaxValue ? (long)expected : -1, length);
        }

        #endregion // end: TryGetLength

        #region Contains

        #region Contains(IPAddress)

        /// <summary>Verifies Contains(IPAddress) returns the expected result for addresses inside, outside, and at the boundary of the range.</summary>
        /// <param name="expected">Expected result.</param>
        /// <param name="headString">The head IP address string for the range.</param>
        /// <param name="tailString">The tail IP address string for the range.</param>
        /// <param name="addressString">The IP address string to test for containment.</param>
        [Theory]
        [InlineData(true, "::", "::ABCD", "::1234")] // IPv6 range contains IPv6
        [InlineData(false, "::", "::ABCD", "::FFFF")] // IPv6 range  doesn't contains IPv6
        [InlineData(true, "::", "::ABCD", "::")] // IPv6 range contains IPv6 head
        [InlineData(true, "::", "::ABCD", "::ABCD")] // IPv6 range contains IPv6 tail
        [InlineData(false, "::", "::ABCD", "192.168.1.1")] // IPv6 range doesn't contains IPv4
        [InlineData(false, "::", "::ABCD", null)] // IPv6 range doesn't contains null
        [InlineData(true, "192.168.0.1", "192.168.0.254", "192.168.0.128")] // IPv4 range contains IPv4
        [InlineData(false, "192.168.0.1", "192.168.0.254", "10.1.1.1")] // IPv4 range doesn't contains IPv4
        [InlineData(true, "192.168.0.1", "192.168.0.254", "192.168.0.1")] // IPv4 range contains IPv4 head
        [InlineData(true, "192.168.0.1", "192.168.0.254", "192.168.0.254")] // IPv4 range contains IPv4 tail
        [InlineData(false, "192.168.0.1", "192.168.0.254", "::")] // IPv4 range doesn't contains IPv6
        [InlineData(false, "192.168.0.1", "192.168.0.254", null)] // IPv4 range doesn't contains null
        public void ContainsIPAddress_Test(bool expected, string headString, string tailString, string addressString)
        {
            // Arrange
            var head = IPAddress.Parse(headString);
            var tail = IPAddress.Parse(tailString);
            _ = IPAddress.TryParse(addressString, out var containsAddress);

            var iPAddressRange = CreateIPAddressRange(head, tail);

            // Act
            var result = iPAddressRange.Contains(containsAddress);

            // Assert
            Assert.Equal(expected, result);
        }

        #endregion // end: Contains(IPAddress)

        #region Contains(IIPAddressRange)

        /// <summary>Verifies Contains(IIPAddressRange) returns the expected result for ranges that fit, overlap, or fall outside.</summary>
        /// <param name="expected">Expected result.</param>
        /// <param name="headString">The head IP address string for the range.</param>
        /// <param name="tailString">The tail IP address string for the range.</param>
        /// <param name="containsHeadString">The head IP address string for the contained range.</param>
        /// <param name="containsTailString">The tail IP address string for the contained range.</param>
        [Theory]
        [InlineData(true, "::", "::ABCD", "::", "::ABCD")] // IPv6 range contains self
        [InlineData(true, "::", "::ABCD", "::1", "::1234")] // IPv6 range contains internal IPv6 range
        [InlineData(false, "::", "::ABCD", "AA::FFFF", "AB::FFFF")] // IPv6 range doesn't contains external IPv6 range
        [InlineData(false, "A::", "A::ABCD", "::", "A::")] // IPv6 range  doesn't contains IPv6 overhang head
        [InlineData(false, "::", "::ABCD", "::", "AA::FFFF")] // IPv6 range  doesn't contains IPv6 overhang tail
        [InlineData(false, "::", "::ABCD", "192.168.1.1", "192.168.1.10")] // IPv6 range doesn't contains IPv4 range
        [InlineData(false, "::", "A::ABCD", null, null)] // IPv6 range  doesn't contain null range
        [InlineData(true, "128.0.0.1", "128.0.0.254", "128.0.0.1", "128.0.0.254")] // IPv4 range contains self
        [InlineData(true, "128.0.0.1", "128.0.0.254", "128.0.0.5", "128.0.0.100")] // IPv4 range contains internal IPv4 range
        [InlineData(false, "128.0.0.1", "128.0.0.254", "140.0.0.0", "145.0.0.0")] // IPv4 range doesn't contains external IPv4 range
        [InlineData(false, "128.0.0.1", "128.0.0.254", "", "128.0.0.100")] // IPv4 range doesn't contains null head IPv4 range
        [InlineData(false, "128.0.0.1", "128.0.0.254", "128.0.0.5", "")] // IPv4 range doesn't contains null tail IPv4 range
        [InlineData(false, "128.0.0.1", "128.0.0.254", "0.0.0.0", "128.0.0.100")] // IPv4 range  doesn't contains IPv4 overhang head
        [InlineData(false, "128.0.0.1", "128.0.0.254", "128.0.0.5", "145.0.0.0")] // IPv4 range  doesn't contains IPv4 overhang tail
        [InlineData(false, "128.0.0.1", "128.0.0.254", "::ABCD", "42::ABCD")] // IPv4 range doesn't contains IPv6 range
        [InlineData(false, "128.0.0.1", "128.0.0.254", null, null)] // IPv4 range  doesn't contain null range
        public void ContainsIPAddressRange_Test(
            bool expected,
            string headString,
            string tailString,
            string containsHeadString,
            string containsTailString
        )
        {
            // Arrange
            var head = IPAddress.Parse(headString);
            var tail = IPAddress.Parse(tailString);

            var substituteIPAddressSubRange =
                IPAddress.TryParse(containsHeadString, out var subhead)
                && IPAddress.TryParse(containsTailString, out var subtail)
                    ? new IPAddressRange(subhead, subtail)
                    : null;

            var iPAddressRange = CreateIPAddressRange(head, tail);

            var containedIPAddressRange = substituteIPAddressSubRange;

            // Act
            var result = iPAddressRange.Contains(containedIPAddressRange);

            // Assert
            Assert.Equal(expected, result);
        }

        #endregion // end: Contains(IIPAddressRange)

        #region Contains IIPAddressRange

        /// <summary>
        ///     Gets parameters: expected (bool), left (IIPAddressRange), right (IIPAddressRange)
        /// </summary>
        /// <value>
        ///     Parameters: expected (bool), left (IIPAddressRange), right (IIPAddressRange)
        /// </value>
        public static TheoryData<bool, IIPAddressRange, IIPAddressRange> Contains_IIPAddressRange_Test_Data
        {
            get
            {
                var ipv4Range = CreateIPAddressRange(IPAddress.Any, IPAddress.Broadcast);
                var ipv6Range = CreateIPAddressRange(
                    IPAddress.IPv6Any,
                    IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff")
                );

                var data = new TheoryData<bool, IIPAddressRange, IIPAddressRange>
                {
                    // null overlap checking
                    { false, ipv4Range, null },
                    { false, ipv6Range, null },
                    // same overlap checking
                    { true, ipv4Range, ipv4Range },
                    { true, ipv6Range, ipv6Range },
                    // differing address families
                    { false, ipv4Range, ipv6Range },
                    { false, ipv6Range, ipv4Range },
                    // head only overlapped
                    {
                        false,
                        CreateIPAddressRange(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("192.168.1.128")),
                        CreateIPAddressRange(IPAddress.Parse("192.168.1.128"), IPAddress.Parse("192.168.1.192"))
                    },
                    {
                        false,
                        CreateIPAddressRange(IPAddress.Parse("::"), IPAddress.Parse("::ffff")),
                        CreateIPAddressRange(IPAddress.Parse("::ffff"), IPAddress.Parse("1::ffff"))
                    },
                    // full head and tail overlapped
                    {
                        true,
                        CreateIPAddressRange(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("192.168.1.255")),
                        CreateIPAddressRange(IPAddress.Parse("192.168.1.128"), IPAddress.Parse("192.168.1.192"))
                    },
                    {
                        true,
                        CreateIPAddressRange(IPAddress.Parse("::"), IPAddress.Parse("ffff::ffff")),
                        CreateIPAddressRange(IPAddress.Parse("::ff00"), IPAddress.Parse("ff::ff00"))
                    },
                    // tail only overlapped
                    {
                        false,
                        CreateIPAddressRange(IPAddress.Parse("192.168.1.192"), IPAddress.Parse("192.168.1.255")),
                        CreateIPAddressRange(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("192.168.1.192"))
                    },
                    {
                        false,
                        CreateIPAddressRange(IPAddress.Parse("::ffff"), IPAddress.Parse("1::ffff")),
                        CreateIPAddressRange(IPAddress.Parse("::"), IPAddress.Parse("::ffff"))
                    },
                    // not touching
                    {
                        false,
                        CreateIPAddressRange(IPAddress.Parse("192.168.1.1"), IPAddress.Parse("192.168.1.128")),
                        CreateIPAddressRange(IPAddress.Parse("10.1.1.0"), IPAddress.Parse("10.1.1.100"))
                    },
                    {
                        false,
                        CreateIPAddressRange(IPAddress.Parse("ab::"), IPAddress.Parse("ab::f")),
                        CreateIPAddressRange(IPAddress.Parse("ef::"), IPAddress.Parse("ef::f"))
                    },
                    // disparate ranges
                    {
                        false,
                        CreateIPAddressRange(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("192.168.2.0")),
                        CreateIPAddressRange(IPAddress.Parse("10.1.1.1"), IPAddress.Parse("10.1.5.0"))
                    },
                    {
                        false,
                        CreateIPAddressRange(IPAddress.Parse("::"), IPAddress.Parse("::ff")),
                        CreateIPAddressRange(IPAddress.Parse("f::"), IPAddress.Parse("f:1::"))
                    },
                };
                return data;
            }
        }

        /// <summary>Verifies Contains(IIPAddressRange) returns the expected result for the given range pair.</summary>
        /// <param name="expected">Expected result.</param>
        /// <param name="left">Left operand range.</param>
        /// <param name="right">Right operand range.</param>
        [Theory]
        [MemberData(nameof(Contains_IIPAddressRange_Test_Data))]
        public void Contains_IIPAddressRange_Test(bool expected, IIPAddressRange left, IIPAddressRange right)
        {
            // Arrange
            // Act
            var result = left.Contains(right);

            // Assert
            Assert.Equal(expected, result);
        }

        #endregion // end: Contains IIPAddressRange

        #region Contains IPAddress

        /// <summary>
        ///     Gets parameters: expected (bool), range (IIPAddressRange), address (IPAddress)
        /// </summary>
        /// <value>
        ///     Parameters: expected (bool), range (IIPAddressRange), address (IPAddress)
        /// </value>
        public static TheoryData<bool, IIPAddressRange, IPAddress> Contains_Test_Data
        {
            get
            {
                var ipv4Range = CreateIPAddressRange(IPAddress.Any, IPAddress.Broadcast);
                var ipv6Range = CreateIPAddressRange(
                    IPAddress.IPv6Any,
                    IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff")
                );

                var data = new TheoryData<bool, IIPAddressRange, IPAddress>
                {
                    // does not contain null
                    { false, ipv4Range, null },
                    { false, ipv6Range, null },
                    // differing address families
                    { false, ipv4Range, IPAddress.IPv6Any },
                    { false, ipv6Range, IPAddress.Any },
                    // contains head
                    { true, ipv4Range, ipv4Range.Head },
                    { true, ipv6Range, ipv6Range.Head },
                    // contains tail
                    { true, ipv4Range, ipv4Range.Tail },
                    { true, ipv6Range, ipv6Range.Tail },
                    // does not contain outside before
                    {
                        false,
                        CreateIPAddressRange(IPAddress.Parse("192.168.1.1"), IPAddress.Parse("192.168.1.200")),
                        IPAddress.Parse("192.168.1.0")
                    },
                    {
                        false,
                        CreateIPAddressRange(IPAddress.Parse("::ff01"), IPAddress.Parse("::ff08")),
                        IPAddress.Parse("::ff00")
                    },
                    // does not contain outside after
                    {
                        false,
                        CreateIPAddressRange(IPAddress.Parse("192.168.1.1"), IPAddress.Parse("192.168.1.200")),
                        IPAddress.Parse("192.168.1.201")
                    },
                    {
                        false,
                        CreateIPAddressRange(IPAddress.Parse("::ff01"), IPAddress.Parse("::ff08")),
                        IPAddress.Parse("::ff09")
                    },
                };

                // contains all inside IPv4
                var ipv4InsideRange = CreateIPAddressRange(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("192.168.1.5"));
                foreach (var ip in ipv4InsideRange.ToIPAddresses())
                {
                    data.Add(true, ipv4InsideRange, ip);
                }

                // contains all inside IPv6
                var ipv6InsideRange = CreateIPAddressRange(IPAddress.Parse("::ff00"), IPAddress.Parse("::ff0f"));
                foreach (var ip in ipv6InsideRange.ToIPAddresses())
                {
                    data.Add(true, ipv6InsideRange, ip);
                }

                return data;
            }
        }

        /// <summary>Verifies Contains(IPAddress) via IIPAddressRange returns the expected result.</summary>
        /// <param name="expected">Expected result.</param>
        /// <param name="range">The range under test.</param>
        /// <param name="address">The address to test for containment.</param>
        [Theory]
        [MemberData(nameof(Contains_Test_Data))]
        public void Contains_Test(bool expected, IIPAddressRange range, IPAddress address)
        {
            // Arrange
            // Act
            var result = range.Contains(address);

            // Assert
            Assert.Equal(expected, result);
        }

        #endregion // end: Contains IPAddress

        #endregion // end: Contains

        #region Overlap and Touches

        #region HeadOverlappedBy

        /// <summary>
        ///     Gets parameters: expected (bool), left (IIPAddressRange), right (IIPAddressRange)
        /// </summary>
        /// <value>
        ///     Parameters: expected (bool), left (IIPAddressRange), right (IIPAddressRange)
        /// </value>
        public static TheoryData<bool, IIPAddressRange, IIPAddressRange> HeadOverlappedBy_Test_Data
        {
            get
            {
                var ipv4Range = CreateIPAddressRange(IPAddress.Any, IPAddress.Broadcast);
                var ipv6Range = CreateIPAddressRange(
                    IPAddress.IPv6Any,
                    IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff")
                );

                var data = new TheoryData<bool, IIPAddressRange, IIPAddressRange>
                {
                    // null overlap checking
                    { false, ipv4Range, null },
                    { false, ipv6Range, null },
                    // same overlap checking
                    { true, ipv4Range, ipv4Range },
                    { true, ipv6Range, ipv6Range },
                    // differing address families
                    { false, ipv4Range, ipv6Range },
                    { false, ipv6Range, ipv4Range },
                    // head only overlapped
                    {
                        true,
                        CreateIPAddressRange(IPAddress.Parse("192.168.1.128"), IPAddress.Parse("192.168.1.192")),
                        CreateIPAddressRange(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("192.168.1.128"))
                    },
                    {
                        true,
                        CreateIPAddressRange(IPAddress.Parse("::ffff"), IPAddress.Parse("1::ffff")),
                        CreateIPAddressRange(IPAddress.Parse("::"), IPAddress.Parse("::ffff"))
                    },
                    // full head and tail overlapped
                    {
                        true,
                        CreateIPAddressRange(IPAddress.Parse("192.168.1.128"), IPAddress.Parse("192.168.1.192")),
                        CreateIPAddressRange(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("192.168.1.255"))
                    },
                    {
                        true,
                        CreateIPAddressRange(IPAddress.Parse("::ff00"), IPAddress.Parse("ff::ff00")),
                        CreateIPAddressRange(IPAddress.Parse("::"), IPAddress.Parse("ffff::ffff"))
                    },
                    // tail only overlapped
                    {
                        false,
                        CreateIPAddressRange(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("192.168.1.192")),
                        CreateIPAddressRange(IPAddress.Parse("192.168.1.192"), IPAddress.Parse("192.168.1.255"))
                    },
                    {
                        false,
                        CreateIPAddressRange(IPAddress.Parse("::"), IPAddress.Parse("::ffff")),
                        CreateIPAddressRange(IPAddress.Parse("::ffff"), IPAddress.Parse("1::ffff"))
                    },
                    // not touching
                    {
                        false,
                        CreateIPAddressRange(IPAddress.Parse("10.1.1.0"), IPAddress.Parse("10.1.1.100")),
                        CreateIPAddressRange(IPAddress.Parse("192.168.1.1"), IPAddress.Parse("192.168.1.128"))
                    },
                    {
                        false,
                        CreateIPAddressRange(IPAddress.Parse("ef::"), IPAddress.Parse("ef::f")),
                        CreateIPAddressRange(IPAddress.Parse("ab::"), IPAddress.Parse("ab::f"))
                    },
                    // disparate ranges
                    {
                        false,
                        CreateIPAddressRange(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("192.168.2.0")),
                        CreateIPAddressRange(IPAddress.Parse("10.1.1.1"), IPAddress.Parse("10.1.5.0"))
                    },
                    {
                        false,
                        CreateIPAddressRange(IPAddress.Parse("::"), IPAddress.Parse("::ff")),
                        CreateIPAddressRange(IPAddress.Parse("f::"), IPAddress.Parse("f:1::"))
                    },
                };
                return data;
            }
        }

        /// <summary>Verifies HeadOverlappedBy returns the expected result for the given pair of ranges.</summary>
        /// <param name="expected">Expected result.</param>
        /// <param name="left">Left operand range.</param>
        /// <param name="right">Right operand range.</param>
        [Theory]
        [MemberData(nameof(HeadOverlappedBy_Test_Data))]
        public void HeadOverlappedBy_Test(bool expected, IIPAddressRange left, IIPAddressRange right)
        {
            // Arrange
            // Act
            var result = left.HeadOverlappedBy(right);

            // Assert
            Assert.Equal(expected, result);
        }

        #endregion // end: HeadOverlappedBy

        #region TailOverlappedBy

        /// <summary>
        ///     Gets parameters: expected (bool), left (IIPAddressRange), right (IIPAddressRange)
        /// </summary>
        /// <value>
        ///     Parameters: expected (bool), left (IIPAddressRange), right (IIPAddressRange)
        /// </value>
        public static TheoryData<bool, IIPAddressRange, IIPAddressRange> TailOverlappedBy_Test_Data
        {
            get
            {
                var ipv4Range = CreateIPAddressRange(IPAddress.Any, IPAddress.Broadcast);
                var ipv6Range = CreateIPAddressRange(
                    IPAddress.IPv6Any,
                    IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff")
                );

                var data = new TheoryData<bool, IIPAddressRange, IIPAddressRange>
                {
                    // null overlap checking
                    { false, ipv4Range, null },
                    { false, ipv6Range, null },
                    // same overlap checking
                    { true, ipv4Range, ipv4Range },
                    { true, ipv6Range, ipv6Range },
                    // differing address families
                    { false, ipv4Range, ipv6Range },
                    { false, ipv6Range, ipv4Range },
                    // head only overlapped
                    {
                        false,
                        CreateIPAddressRange(IPAddress.Parse("192.168.1.128"), IPAddress.Parse("192.168.1.192")),
                        CreateIPAddressRange(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("192.168.1.128"))
                    },
                    {
                        false,
                        CreateIPAddressRange(IPAddress.Parse("::ffff"), IPAddress.Parse("1::ffff")),
                        CreateIPAddressRange(IPAddress.Parse("::"), IPAddress.Parse("::ffff"))
                    },
                    // full head and tail overlapped
                    {
                        true,
                        CreateIPAddressRange(IPAddress.Parse("192.168.1.128"), IPAddress.Parse("192.168.1.192")),
                        CreateIPAddressRange(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("192.168.1.255"))
                    },
                    {
                        true,
                        CreateIPAddressRange(IPAddress.Parse("::ff00"), IPAddress.Parse("ff::ff00")),
                        CreateIPAddressRange(IPAddress.Parse("::"), IPAddress.Parse("ffff::ffff"))
                    },
                    // tail only overlapped
                    {
                        true,
                        CreateIPAddressRange(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("192.168.1.192")),
                        CreateIPAddressRange(IPAddress.Parse("192.168.1.192"), IPAddress.Parse("192.168.1.255"))
                    },
                    {
                        true,
                        CreateIPAddressRange(IPAddress.Parse("::"), IPAddress.Parse("::ffff")),
                        CreateIPAddressRange(IPAddress.Parse("::ffff"), IPAddress.Parse("1::ffff"))
                    },
                    // not touching
                    {
                        false,
                        CreateIPAddressRange(IPAddress.Parse("10.1.1.0"), IPAddress.Parse("10.1.1.100")),
                        CreateIPAddressRange(IPAddress.Parse("192.168.1.1"), IPAddress.Parse("192.168.1.128"))
                    },
                    {
                        false,
                        CreateIPAddressRange(IPAddress.Parse("ef::"), IPAddress.Parse("ef::f")),
                        CreateIPAddressRange(IPAddress.Parse("ab::"), IPAddress.Parse("ab::f"))
                    },
                    // disparate ranges
                    {
                        false,
                        CreateIPAddressRange(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("192.168.2.0")),
                        CreateIPAddressRange(IPAddress.Parse("10.1.1.1"), IPAddress.Parse("10.1.5.0"))
                    },
                    {
                        false,
                        CreateIPAddressRange(IPAddress.Parse("::"), IPAddress.Parse("::ff")),
                        CreateIPAddressRange(IPAddress.Parse("f::"), IPAddress.Parse("f:1::"))
                    },
                };
                return data;
            }
        }

        /// <summary>Verifies TailOverlappedBy returns the expected result for the given pair of ranges.</summary>
        /// <param name="expected">Expected result.</param>
        /// <param name="left">Left operand range.</param>
        /// <param name="right">Right operand range.</param>
        [Theory]
        [MemberData(nameof(TailOverlappedBy_Test_Data))]
        public void TailOverlappedBy_Test(bool expected, IIPAddressRange left, IIPAddressRange right)
        {
            // Arrange
            // Act
            var result = left.TailOverlappedBy(right);

            // Assert
            Assert.Equal(expected, result);
        }

        #endregion // end: TailOverlappedBy

        #region Overlaps

        /// <summary>
        ///     Gets parameters: expected (bool), left (IIPAddressRange), right (IIPAddressRange)
        /// </summary>
        /// <value>
        ///     Parameters: expected (bool), left (IIPAddressRange), right (IIPAddressRange)
        /// </value>
        public static TheoryData<bool, IIPAddressRange, IIPAddressRange> Overlaps_Test_Data
        {
            get
            {
                var ipv4Range = CreateIPAddressRange(IPAddress.Any, IPAddress.Broadcast);
                var ipv6Range = CreateIPAddressRange(
                    IPAddress.IPv6Any,
                    IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff")
                );

                var data = new TheoryData<bool, IIPAddressRange, IIPAddressRange>
                {
                    // null overlap checking
                    { false, ipv4Range, null },
                    { false, ipv6Range, null },
                    // same overlap checking (also covers value equality since same values produce same display name)
                    { true, ipv4Range, ipv4Range },
                    { true, ipv6Range, ipv6Range },
                    // differing address families
                    { false, ipv4Range, ipv6Range },
                    { false, ipv6Range, ipv4Range },
                    // head only overlapped
                    {
                        true,
                        CreateIPAddressRange(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("192.168.1.128")),
                        CreateIPAddressRange(IPAddress.Parse("192.168.1.128"), IPAddress.Parse("192.168.1.192"))
                    },
                    {
                        true,
                        CreateIPAddressRange(IPAddress.Parse("::"), IPAddress.Parse("::ffff")),
                        CreateIPAddressRange(IPAddress.Parse("::ffff"), IPAddress.Parse("1::ffff"))
                    },
                    // full head and tail overlapped
                    {
                        true,
                        CreateIPAddressRange(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("192.168.1.255")),
                        CreateIPAddressRange(IPAddress.Parse("192.168.1.128"), IPAddress.Parse("192.168.1.192"))
                    },
                    {
                        true,
                        CreateIPAddressRange(IPAddress.Parse("::"), IPAddress.Parse("ffff::ffff")),
                        CreateIPAddressRange(IPAddress.Parse("::ff00"), IPAddress.Parse("ff::ff00"))
                    },
                    // tail only overlapped
                    {
                        true,
                        CreateIPAddressRange(IPAddress.Parse("192.168.1.192"), IPAddress.Parse("192.168.1.255")),
                        CreateIPAddressRange(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("192.168.1.192"))
                    },
                    {
                        true,
                        CreateIPAddressRange(IPAddress.Parse("::ffff"), IPAddress.Parse("1::ffff")),
                        CreateIPAddressRange(IPAddress.Parse("::"), IPAddress.Parse("::ffff"))
                    },
                    // not touching
                    {
                        false,
                        CreateIPAddressRange(IPAddress.Parse("192.168.1.1"), IPAddress.Parse("192.168.1.128")),
                        CreateIPAddressRange(IPAddress.Parse("10.1.1.0"), IPAddress.Parse("10.1.1.100"))
                    },
                    {
                        false,
                        CreateIPAddressRange(IPAddress.Parse("ab::"), IPAddress.Parse("ab::f")),
                        CreateIPAddressRange(IPAddress.Parse("ef::"), IPAddress.Parse("ef::f"))
                    },
                    // disparate ranges
                    {
                        false,
                        CreateIPAddressRange(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("192.168.2.0")),
                        CreateIPAddressRange(IPAddress.Parse("10.1.1.1"), IPAddress.Parse("10.1.5.0"))
                    },
                    {
                        false,
                        CreateIPAddressRange(IPAddress.Parse("::"), IPAddress.Parse("::ff")),
                        CreateIPAddressRange(IPAddress.Parse("f::"), IPAddress.Parse("f:1::"))
                    },
                    // wholly contained - both directions must return true (symmetry)
                    {
                        true,
                        CreateIPAddressRange(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("192.168.1.255")),
                        CreateIPAddressRange(IPAddress.Parse("192.168.1.64"), IPAddress.Parse("192.168.1.192"))
                    },
                    {
                        true,
                        CreateIPAddressRange(IPAddress.Parse("192.168.1.64"), IPAddress.Parse("192.168.1.192")),
                        CreateIPAddressRange(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("192.168.1.255"))
                    },
                    {
                        true,
                        CreateIPAddressRange(IPAddress.Parse("::"), IPAddress.Parse("ffff::ffff")),
                        CreateIPAddressRange(IPAddress.Parse("1::"), IPAddress.Parse("fffe::ffff"))
                    },
                    {
                        true,
                        CreateIPAddressRange(IPAddress.Parse("1::"), IPAddress.Parse("fffe::ffff")),
                        CreateIPAddressRange(IPAddress.Parse("::"), IPAddress.Parse("ffff::ffff"))
                    },
                };
                return data;
            }
        }

        /// <summary>Verifies that Overlaps returns the expected result for the given pair of ranges.</summary>
        /// <param name="expected">Expected result.</param>
        /// <param name="left">Left operand range.</param>
        /// <param name="right">Right operand range.</param>
        [Theory]
        [MemberData(nameof(Overlaps_Test_Data))]
        public void Overlaps_Test(bool expected, IIPAddressRange left, IIPAddressRange right)
        {
            // Arrange
            // Act
            var result = left.Overlaps(right);

            // Assert
            Assert.Equal(expected, result);
        }

        #endregion // end: Overlaps

        #region Touches

        /// <summary>
        ///     Gets parameters: expected (bool), left (IIPAddressRange), right (IIPAddressRange)
        /// </summary>
        /// <value>
        ///     Parameters: expected (bool), left (IIPAddressRange), right (IIPAddressRange)
        /// </value>
        public static TheoryData<bool, IIPAddressRange, IIPAddressRange> Touches_Test_Data
        {
            get
            {
                var ipv4Range = CreateIPAddressRange(IPAddress.Any, IPAddress.Broadcast);
                var ipv6Range = CreateIPAddressRange(
                    IPAddress.IPv6Any,
                    IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff")
                );

                var data = new TheoryData<bool, IIPAddressRange, IIPAddressRange>
                {
                    // null overlap checking
                    { false, CreateIPAddressRange(IPAddress.Any, IPAddress.Broadcast), null },
                    {
                        false,
                        CreateIPAddressRange(IPAddress.IPv6Any, IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff")),
                        null
                    },
                    // differing address families
                    { false, ipv4Range, ipv6Range },
                    { false, ipv6Range, ipv4Range },
                    // left tail touches right head
                    {
                        true,
                        CreateIPAddressRange(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("192.168.1.100")),
                        CreateIPAddressRange(IPAddress.Parse("192.168.1.101"), IPAddress.Parse("192.168.1.200"))
                    },
                    {
                        true,
                        CreateIPAddressRange(IPAddress.Parse("::"), IPAddress.Parse("::abcd")),
                        CreateIPAddressRange(IPAddress.Parse("::abce"), IPAddress.Parse("::ffff"))
                    },
                    // left head touches right tail
                    {
                        true,
                        CreateIPAddressRange(IPAddress.Parse("192.168.1.101"), IPAddress.Parse("192.168.1.200")),
                        CreateIPAddressRange(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("192.168.1.100"))
                    },
                    {
                        true,
                        CreateIPAddressRange(IPAddress.Parse("::abce"), IPAddress.Parse("::ffff")),
                        CreateIPAddressRange(IPAddress.Parse("::"), IPAddress.Parse("::abcd"))
                    },
                    // head only overlapped
                    {
                        false,
                        CreateIPAddressRange(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("192.168.1.128")),
                        CreateIPAddressRange(IPAddress.Parse("192.168.1.128"), IPAddress.Parse("192.168.1.192"))
                    },
                    {
                        false,
                        CreateIPAddressRange(IPAddress.Parse("::"), IPAddress.Parse("::ffff")),
                        CreateIPAddressRange(IPAddress.Parse("::ffff"), IPAddress.Parse("1::ffff"))
                    },
                    // full head and tail overlapped
                    {
                        false,
                        CreateIPAddressRange(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("192.168.1.255")),
                        CreateIPAddressRange(IPAddress.Parse("192.168.1.128"), IPAddress.Parse("192.168.1.192"))
                    },
                    {
                        false,
                        CreateIPAddressRange(IPAddress.Parse("::"), IPAddress.Parse("ffff::ffff")),
                        CreateIPAddressRange(IPAddress.Parse("::ff00"), IPAddress.Parse("ff::ff00"))
                    },
                    // tail only overlapped
                    {
                        false,
                        CreateIPAddressRange(IPAddress.Parse("192.168.1.192"), IPAddress.Parse("192.168.1.255")),
                        CreateIPAddressRange(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("192.168.1.192"))
                    },
                    {
                        false,
                        CreateIPAddressRange(IPAddress.Parse("::ffff"), IPAddress.Parse("1::ffff")),
                        CreateIPAddressRange(IPAddress.Parse("::"), IPAddress.Parse("::ffff"))
                    },
                    // disparate ranges
                    {
                        false,
                        CreateIPAddressRange(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("192.168.2.0")),
                        CreateIPAddressRange(IPAddress.Parse("10.1.1.1"), IPAddress.Parse("10.1.5.0"))
                    },
                    {
                        false,
                        CreateIPAddressRange(IPAddress.Parse("::"), IPAddress.Parse("::ff")),
                        CreateIPAddressRange(IPAddress.Parse("f::"), IPAddress.Parse("f:1::"))
                    },
                    // this tail at max
                    {
                        false,
                        CreateIPAddressRange(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("255.255.255.255")),
                        CreateIPAddressRange(IPAddress.Parse("10.1.1.1"), IPAddress.Parse("10.1.5.0"))
                    },
                    {
                        false,
                        CreateIPAddressRange(IPAddress.Parse("::"), IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff")),
                        CreateIPAddressRange(IPAddress.Parse("f::"), IPAddress.Parse("f:1::"))
                    },
                    // that tail at max
                    {
                        false,
                        CreateIPAddressRange(IPAddress.Parse("10.1.1.1"), IPAddress.Parse("10.1.5.0")),
                        CreateIPAddressRange(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("255.255.255.255"))
                    },
                    {
                        false,
                        CreateIPAddressRange(IPAddress.Parse("f::"), IPAddress.Parse("f:1::")),
                        CreateIPAddressRange(IPAddress.Parse("::"), IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff"))
                    },
                };
                return data;
            }
        }

        /// <summary>Verifies that Touches returns the expected result for the given pair of ranges.</summary>
        /// <param name="expected">Expected result.</param>
        /// <param name="left">Left operand range.</param>
        /// <param name="right">Right operand range.</param>
        [Theory]
        [MemberData(nameof(Touches_Test_Data))]
        public void Touches_Test(bool expected, IIPAddressRange left, IIPAddressRange right)
        {
            // Arrange
            // Act
            var result = left.Touches(right);

            // Assert
            Assert.Equal(expected, result);
        }

        #endregion // end: Touches

        #endregion // end: Overlap and Touches

        #region Contains Any/All Public/Private Addresses

        /// <summary>
        ///     Gets parameters: expectedHasPublic (bool), expectedHasPrivate (bool), range (IIPAddressRange)
        /// </summary>
        /// <value>
        ///     Parameters: expectedHasPublic (bool), expectedHasPrivate (bool), range (IIPAddressRange)
        /// </value>
        public static TheoryData<bool, bool, IIPAddressRange> ContainsPublicPrivate_Data
        {
            get
            {
                var data = new TheoryData<bool, bool, IIPAddressRange>();

                // known private ranges
                foreach (var subnet in SubnetUtilities.PrivateIPAddressRangesList)
                {
                    // has private
                    data.Add(false, true, CreateIPAddressRange(subnet.Head, subnet.Tail)); // on the border of private
                    data.Add(false, true, CreateIPAddressRange(subnet.Head.Increment(2), subnet.Tail.Increment(-2))); // wholly inside of private
                    data.Add(false, true, CreateIPAddressRange(subnet.Head.Increment(2), subnet.Tail)); // partially within of private
                    data.Add(false, true, CreateIPAddressRange(subnet.Head, subnet.Tail.Increment(-2))); // partially within of private

                    // has private and public
                    data.Add(true, true, CreateIPAddressRange(subnet.Head.Increment(-2), subnet.Tail)); // partially outside of private
                    data.Add(true, true, CreateIPAddressRange(subnet.Head, subnet.Tail.Increment(2))); // partially outside of private
                }

                // public only
                var publicSpace = new (IPAddress head, IPAddress tail)[]
                {
                    (IPAddress.Parse("128.64.32.0"), IPAddress.Parse("128.64.32.16")),
                    (IPAddress.Parse("FFFF:7FFF:3FFF::"), IPAddress.Parse("FFFF:7FFF:3FFF:1FFF::")),
                };

                foreach (var (head, tail) in publicSpace)
                {
                    data.Add(true, false, CreateIPAddressRange(head, tail)); // on the border of public
                    data.Add(true, false, CreateIPAddressRange(head.Increment(2), tail.Increment(-2))); // wholly inside public
                }

                return data;
            }
        }

        /// <summary>Verifies ContainsAnyPrivateAddresses returns the expected result for ranges with known public/private composition.</summary>
        /// <param name="expectedHasPublic">Whether the range is expected to contain public addresses.</param>
        /// <param name="expectedHasPrivate">Whether the range is expected to contain private addresses.</param>
        /// <param name="range">The range under test.</param>
        [Theory]
        [MemberData(nameof(ContainsPublicPrivate_Data))]
        public void ContainsAnyPrivateAddresses_Test(bool expectedHasPublic, bool expectedHasPrivate, IIPAddressRange range)
        {
            // Arrange
            // Act
            var result = range.ContainsAnyPrivateAddresses();

            this._testOutputHelper.WriteLine($"pr:{expectedHasPrivate} pu:{expectedHasPublic} {range.Head} - {range.Tail}");

            // Assert
            Assert.Equal(expectedHasPrivate, result);
        }

        /// <summary>Verifies ContainsAllPrivateAddresses returns true only when the range is wholly within private space.</summary>
        /// <param name="expectedHasPublic">Whether the range is expected to contain public addresses.</param>
        /// <param name="expectedHasPrivate">Whether the range is expected to contain private addresses.</param>
        /// <param name="range">The range under test.</param>
        [Theory]
        [MemberData(nameof(ContainsPublicPrivate_Data))]
        public void ContainsAllPrivateAddresses_Test(bool expectedHasPublic, bool expectedHasPrivate, IIPAddressRange range)
        {
            // Arrange
            // Act
            var result = range.ContainsAllPrivateAddresses();

            this._testOutputHelper.WriteLine($"pr:{expectedHasPrivate} pu:{expectedHasPublic} {range.Head} - {range.Tail}");

            // Assert
            Assert.Equal(expectedHasPrivate && !expectedHasPublic, result);
        }

        /// <summary>Verifies ContainsAnyPublicAddresses returns the expected result for ranges with known public/private composition.</summary>
        /// <param name="expectedHasPublic">Whether the range is expected to contain public addresses.</param>
        /// <param name="expectedHasPrivate">Whether the range is expected to contain private addresses.</param>
        /// <param name="range">The range under test.</param>
        [Theory]
        [MemberData(nameof(ContainsPublicPrivate_Data))]
        public void ContainsAnyPublicAddresses_Test(bool expectedHasPublic, bool expectedHasPrivate, IIPAddressRange range)
        {
            // Arrange
            // Act
            var result = range.ContainsAnyPublicAddresses();

            this._testOutputHelper.WriteLine($"pr:{expectedHasPrivate} pu:{expectedHasPublic} {range.Head} - {range.Tail}");

            // Assert
            Assert.Equal(expectedHasPublic, result);
        }

        /// <summary>Verifies ContainsAllPublicAddresses returns true only when the range has no private addresses.</summary>
        /// <param name="expectedHasPublic">Whether the range is expected to contain public addresses.</param>
        /// <param name="expectedHasPrivate">Whether the range is expected to contain private addresses.</param>
        /// <param name="range">The range under test.</param>
        [Theory]
        [MemberData(nameof(ContainsPublicPrivate_Data))]
        public void ContainsAllPublicAddresses_Test(bool expectedHasPublic, bool expectedHasPrivate, IIPAddressRange range)
        {
            // Arrange
            // Act
            var result = range.ContainsAllPublicAddresses();

            this._testOutputHelper.WriteLine($"pr:{expectedHasPrivate} pu:{expectedHasPublic} {range.Head} - {range.Tail}");

            // Assert
            Assert.Equal(!expectedHasPrivate && expectedHasPublic, result);
        }

        /// <summary>Regression: a range whose endpoints are both public but spans a private block must report private addresses present.</summary>
        [Fact]
        public void ContainsAnyPrivateAddresses_RangeSpansPrivateBlock_WithPublicEndpoints_Test()
        {
            // Regression: endpoints 11.0.0.0 and 173.0.0.0 are both public, but the range
            // spans 172.16.0.0/12 - the endpoint heuristic incorrectly returned false.
            var range = CreateIPAddressRange(IPAddress.Parse("11.0.0.0"), IPAddress.Parse("173.0.0.0"));

            Assert.True(range.ContainsAnyPrivateAddresses());
        }

        /// <summary>Regression: a range whose endpoints are both public but spans a private block must not be classified as all-public.</summary>
        [Fact]
        public void ContainsAllPublicAddresses_RangeSpansPrivateBlock_WithPublicEndpoints_Test()
        {
            // Regression: endpoints are both public, but the range spans 172.16.0.0/12 -
            // not all addresses are public, so this must return false.
            var range = CreateIPAddressRange(IPAddress.Parse("11.0.0.0"), IPAddress.Parse("173.0.0.0"));

            Assert.False(range.ContainsAllPublicAddresses());
        }

        /// <summary>Verifies ContainsAnyPrivateAddresses and ContainsAllPublicAddresses are logical inverses.</summary>
        /// <param name="expectedHasPublic">Whether the range is expected to contain public addresses.</param>
        /// <param name="expectedHasPrivate">Whether the range is expected to contain private addresses.</param>
        /// <param name="range">The range under test.</param>
        [Theory]
        [MemberData(nameof(ContainsPublicPrivate_Data))]
        public void ContainsAnyPrivateAddresses_IsInverseOf_ContainsAllPublicAddresses_Test(
            bool expectedHasPublic,
            bool expectedHasPrivate,
            IIPAddressRange range
        )
        {
            // ContainsAnyPrivateAddresses and ContainsAllPublicAddresses must be logical inverses.
            var hasPrivate = range.ContainsAnyPrivateAddresses();
            var allPublic = range.ContainsAllPublicAddresses();

            this._testOutputHelper.WriteLine($"pr:{expectedHasPrivate} pu:{expectedHasPublic} {range.Head} - {range.Tail}");

            Assert.NotEqual(hasPrivate, allPublic);
        }

        /// <summary>Verifies ContainsAllPrivateAddresses and ContainsAnyPublicAddresses are logical inverses.</summary>
        /// <param name="expectedHasPublic">Whether the range is expected to contain public addresses.</param>
        /// <param name="expectedHasPrivate">Whether the range is expected to contain private addresses.</param>
        /// <param name="range">The range under test.</param>
        [Theory]
        [MemberData(nameof(ContainsPublicPrivate_Data))]
        public void ContainsAllPrivateAddresses_IsInverseOf_ContainsAnyPublicAddresses_Test(
            bool expectedHasPublic,
            bool expectedHasPrivate,
            IIPAddressRange range
        )
        {
            // ContainsAllPrivateAddresses and ContainsAnyPublicAddresses must be logical inverses.
            var allPrivate = range.ContainsAllPrivateAddresses();
            var hasPublic = range.ContainsAnyPublicAddresses();

            this._testOutputHelper.WriteLine($"pr:{expectedHasPrivate} pu:{expectedHasPublic} {range.Head} - {range.Tail}");

            Assert.NotEqual(allPrivate, hasPublic);
        }

        #endregion // end: Contains Any/All Public/Private Addresses
    }
}
