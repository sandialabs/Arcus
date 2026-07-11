using System.Net;
using System.Numerics;
using Arcus.Comparers;
using NSubstitute;

namespace Arcus.Tests.Comparers
{
    /// <summary>Unit tests for <see cref="DefaultIIPAddressRangeComparer"/>.</summary>
    public class DefaultIIPAddressRangeComparerTests
    {
        /// <summary>
        ///     Verifies that <see cref="DefaultIIPAddressRangeComparer" /> is assignable to
        ///     <see cref="IComparer{IIPAddressRange}" />.
        /// </summary>
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

        #region Instance

        /// <summary>
        ///     Verifies that <see cref="DefaultIIPAddressRangeComparer.Instance" /> is not <see langword="null" />
        ///     and is an instance of <see cref="DefaultIIPAddressRangeComparer" />.
        /// </summary>
        [Fact]
        public void Instance_IsNotNull_Test()
        {
            // Arrange
            // Act
            var instance = DefaultIIPAddressRangeComparer.Instance;

            // Assert
            Assert.NotNull(instance);
            Assert.IsType<DefaultIIPAddressRangeComparer>(instance);
        }

        #endregion // end: Instance

        #region Ctor

        /// <summary>
        ///     Verifies that constructing a <see cref="DefaultIIPAddressRangeComparer" /> with a <see langword="null" />
        ///     <see cref="IComparer{IPAddress}" /> throws <see cref="ArgumentNullException" />.
        /// </summary>
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

        /// <summary>
        ///     Gets test data for <see cref="Compare_Ranges_ReturnsExpectedOrdering_Test" />.
        ///     Covers equal ranges, same-reference ranges, null operands, cross-family comparisons,
        ///     and ordinal ordering by length within the same address family.
        ///     <para>Parameters: expected comparison result (int), x (IIPAddressRange), y (IIPAddressRange).</para>
        /// </summary>
        /// <value>
        /// Test data for <see cref="Compare_Ranges_ReturnsExpectedOrdering_Test" />.
        ///     Covers equal ranges, same-reference ranges, null operands, cross-family comparisons,
        ///     and ordinal ordering by length within the same address family.
        ///     <para>Parameters: expected comparison result (int), x (IIPAddressRange), y (IIPAddressRange).</para>
        /// </value>
        public static TheoryData<int, IIPAddressRange, IIPAddressRange> Compare_Ranges_ReturnsExpectedOrdering_Test_Data
        {
            get
            {
                var data = new TheoryData<int, IIPAddressRange, IIPAddressRange>
                {
                    // equal ranges
                    { 0, CreateRangeFromHead("192.168.1.0", 0), CreateRangeFromHead("192.168.1.0", 0) },
                    { 0, CreateRangeFromHead("a::", 0), CreateRangeFromHead("a::", 0) },
                };

                // same range (reference equality)
                var ipv4Same = CreateRangeFromHead("192.168.1.0");
                data.Add(0, ipv4Same, ipv4Same);

                var ipv6Same = CreateRangeFromHead("a::");
                data.Add(0, ipv6Same, ipv6Same);

                // null compare
                data.Add(0, null, null);
                data.Add(-1, null, CreateRangeFromHead("192.168.1.0"));
                data.Add(1, CreateRangeFromHead("192.168.1.0"), null);
                data.Add(-1, null, CreateRangeFromHead("a::"));
                data.Add(1, CreateRangeFromHead("a::"), null);

                // numerically equivalent, different address families
                data.Add(-1, CreateRangeFromHead("0.0.0.0", 100), CreateRangeFromHead("::", 100));
                data.Add(1, CreateRangeFromHead("::", 100), CreateRangeFromHead("0.0.0.0", 100));

                // satisfies ordinal ordering by length
                data.Add(-1, CreateRangeFromHead("192.0.0.0", 100), CreateRangeFromHead("192.0.0.0", 500));
                data.Add(1, CreateRangeFromHead("192.0.0.0", 500), CreateRangeFromHead("192.0.0.0", 100));
                data.Add(-1, CreateRangeFromHead("ab::", 100), CreateRangeFromHead("ab::", 500));
                data.Add(1, CreateRangeFromHead("ab::", 500), CreateRangeFromHead("ab::", 100));

                return data;
            }
        }

        /// <summary>
        ///     Verifies that <see cref="DefaultIIPAddressRangeComparer.Compare" /> returns the expected ordinal comparison
        ///     result for pairs of <see cref="IIPAddressRange" /> values.
        /// </summary>
        /// <param name="expected">the expected comparison result.</param>
        /// <param name="x">the left operand.</param>
        /// <param name="y">the right operand.</param>
        [Theory]
        [MemberData(nameof(Compare_Ranges_ReturnsExpectedOrdering_Test_Data))]
        public void Compare_Ranges_ReturnsExpectedOrdering_Test(int expected, IIPAddressRange x, IIPAddressRange y)
        {
            // Arrange
            var comparer = new DefaultIIPAddressRangeComparer();

            // Act
            var result = comparer.Compare(x, y);

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        ///     Verifies that <see cref="DefaultIIPAddressRangeComparer.Compare" /> defers to the injected
        ///     <see cref="IComparer{IPAddress}" /> when comparing the head addresses of two ranges.
        /// </summary>
        [Fact]
        public void Compare_DifferentHeads_DefersToIPAddressComparer_Test()
        {
            // Arrange
            const int expectedResult = 42;

            var xHead = IPAddress.Parse("0.0.0.0");
            var yHead = IPAddress.Parse("abc::");

            var x = CreateRangeFromIPAddress(xHead);
            var y = CreateRangeFromIPAddress(yHead);

            var substituteIPAddressComparer = Substitute.For<IComparer<IPAddress>>();
            substituteIPAddressComparer.Compare(xHead, yHead).Returns(expectedResult);

            var comparer = new DefaultIIPAddressRangeComparer(substituteIPAddressComparer);

            // Act
            var result = comparer.Compare(x, y);

            // Assert
            Assert.Equal(expectedResult, result);
            substituteIPAddressComparer.Received(1).Compare(xHead, yHead);
        }

        #endregion // end: Compare

        private static IIPAddressRange CreateRangeFromHead(string head, BigInteger? length = null)
        {
            var substitute = Substitute.For<IIPAddressRange>();
            substitute.Head.Returns(IPAddress.Parse(head));
            if (length != null)
            {
                substitute.Length.Returns(length.Value);
            }

            return substitute;
        }

        private static IIPAddressRange CreateRangeFromIPAddress(IPAddress head)
        {
            var substitute = Substitute.For<IIPAddressRange>();
            substitute.Head.Returns(head);
            return substitute;
        }
    }
}
