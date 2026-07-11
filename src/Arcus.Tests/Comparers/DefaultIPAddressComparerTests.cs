using System.Net;
using System.Net.Sockets;
using Arcus.Comparers;
using NSubstitute;

namespace Arcus.Tests.Comparers
{
    /// <summary>Unit tests for <see cref="DefaultIPAddressComparer"/>.</summary>
    public class DefaultIPAddressComparerTests
    {
        /// <summary>
        ///     Verifies that <see cref="DefaultIPAddressComparer" /> is assignable to <see cref="IComparer{IPAddress}" />.
        /// </summary>
        [Fact]
        public void Assignability_Test()
        {
            // Arrange
            var type = typeof(DefaultIPAddressComparer);

            // Act
            var isAssignableFrom = typeof(IComparer<IPAddress>).IsAssignableFrom(type);

            // Assert
            Assert.True(isAssignableFrom);
        }

        #region Ctor

        /// <summary>
        ///     Verifies that constructing a <see cref="DefaultIPAddressComparer" /> with a <see langword="null" />
        ///     <see cref="IComparer{AddressFamily}" /> throws <see cref="ArgumentNullException" />.
        /// </summary>
        [Fact]
        public void Ctor_NullInput_Throws_ArgumentNullException_Test()
        {
            // Arrange
            // Act
            // Assert

            Assert.Throws<ArgumentNullException>(() => new DefaultIPAddressComparer(null));
        }

        /// <summary>
        ///     Verifies that constructing a <see cref="DefaultIPAddressComparer" /> with a valid
        ///     <see cref="IComparer{AddressFamily}" /> succeeds without throwing.
        /// </summary>
        [Fact]
        public void Ctor_ValidAddressFamilyComparer_DoesNotThrow_Test()
        {
            // Arrange
            var addressFamilyComparer = Substitute.For<IComparer<AddressFamily>>();

            // Act
            var comparer = new DefaultIPAddressComparer(addressFamilyComparer);

            // Assert
            Assert.NotNull(comparer);
        }

        #endregion // end: Ctor

        #region Compare

        /// <summary>
        ///     Gets test data for <see cref="Compare_IPAddresses_ReturnsExpectedOrdering_Test" />.
        ///     Covers equal addresses (by value and by reference), null operands, cross-family comparisons,
        ///     and ordinal ordering within the same address family to partition the comparison logic.
        ///     <para>Parameters: expected comparison result (int), x (IPAddress), y (IPAddress).</para>
        /// </summary>
        /// <value>
        ///     Test data for <see cref="Compare_IPAddresses_ReturnsExpectedOrdering_Test" />.
        ///     Covers equal addresses (by value and by reference), null operands, cross-family comparisons,
        ///     and ordinal ordering within the same address family to partition the comparison logic.
        ///     <para>Parameters: expected comparison result (int), x (IPAddress), y (IPAddress).</para>
        /// </value>
        public static TheoryData<int, IPAddress, IPAddress> Compare_IPAddresses_ReturnsExpectedOrdering_Test_Data
        {
            get
            {
                var data = new TheoryData<int, IPAddress, IPAddress>
                {
                    // equal addresses (value equality - distinct IPAddress instances with same value)
                    { 0, IPAddress.Parse("10.0.0.1"), IPAddress.Parse("10.0.0.1") },
                    { 0, IPAddress.Parse("dead::beef"), IPAddress.Parse("dead::beef") },
                };

                // same address (reference equality - identical object)
                var ipv4Same = IPAddress.Parse("192.168.1.1");
                data.Add(0, ipv4Same, ipv4Same);

                var ipv6Same = IPAddress.Parse("abcd::1");
                data.Add(0, ipv6Same, ipv6Same);

                // null compare
                data.Add(0, null, null);
                data.Add(-1, null, IPAddress.Parse("192.168.1.1"));
                data.Add(1, IPAddress.Parse("192.168.1.1"), null);
                data.Add(-1, null, IPAddress.Parse("dead::beef"));
                data.Add(1, IPAddress.Parse("dead::beef"), null);

                // numerically equivalent, different address families
                data.Add(-1, IPAddress.Parse("0.0.0.0"), IPAddress.Parse("::"));
                data.Add(1, IPAddress.Parse("::"), IPAddress.Parse("0.0.0.0"));

                // satisfies ordinal ordering
                data.Add(-1, IPAddress.Parse("192.168.1.1"), IPAddress.Parse("192.168.1.2"));
                data.Add(1, IPAddress.Parse("192.168.1.2"), IPAddress.Parse("192.168.1.1"));
                data.Add(-1, IPAddress.Parse("abc::123"), IPAddress.Parse("abc::124"));
                data.Add(1, IPAddress.Parse("abc::124"), IPAddress.Parse("abc::123"));

                return data;
            }
        }

        /// <summary>
        ///     Verifies that <see cref="DefaultIPAddressComparer.Compare" /> returns the expected ordinal comparison
        ///     result for pairs of <see cref="IPAddress" /> values.
        /// </summary>
        /// <param name="expected">the expected comparison result.</param>
        /// <param name="x">the left operand.</param>
        /// <param name="y">the right operand.</param>
        [Theory]
        [MemberData(nameof(Compare_IPAddresses_ReturnsExpectedOrdering_Test_Data))]
        public void Compare_IPAddresses_ReturnsExpectedOrdering_Test(int expected, IPAddress x, IPAddress y)
        {
            // Arrange
            var comparer = new DefaultIPAddressComparer();

            // Act
            var result = comparer.Compare(x, y);

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        ///     Verifies that <see cref="DefaultIPAddressComparer.Compare" /> defers to the injected
        ///     <see cref="IComparer{AddressFamily}" /> when the two addresses belong to different address families.
        /// </summary>
        [Fact]
        public void Compare_DifferentAddressFamilies_DefersToAddressFamilyComparer_Test()
        {
            // Arrange
            const int expectedResult = 42;
            var x = IPAddress.Any;
            var y = IPAddress.IPv6Any;

            var substituteAddressFamilyComparer = Substitute.For<IComparer<AddressFamily>>();
            substituteAddressFamilyComparer.Compare(x.AddressFamily, y.AddressFamily).Returns(expectedResult);

            var comparer = new DefaultIPAddressComparer(substituteAddressFamilyComparer);

            // Act
            var result = comparer.Compare(x, y);

            // Assert
            Assert.Equal(expectedResult, result);
            substituteAddressFamilyComparer.Received(1).Compare(x.AddressFamily, y.AddressFamily);
        }

        #endregion // end: Compare

        #region Instance

        /// <summary>
        ///     Verifies that <see cref="DefaultIPAddressComparer.Instance" /> is not <see langword="null" />
        ///     and is an instance of <see cref="DefaultIPAddressComparer" />.
        /// </summary>
        [Fact]
        public void Instance_IsNotNull_Test()
        {
            // Arrange
            // Act
            var instance = DefaultIPAddressComparer.Instance;

            // Assert
            Assert.NotNull(instance);
            Assert.IsType<DefaultIPAddressComparer>(instance);
        }

        #endregion // end: Instance
    }
}
