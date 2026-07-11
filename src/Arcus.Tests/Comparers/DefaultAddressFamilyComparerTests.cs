using System.Net.Sockets;
using Arcus.Comparers;

namespace Arcus.Tests.Comparers
{
    /// <summary>Unit tests for <see cref="DefaultAddressFamilyComparer"/>.</summary>
    public class DefaultAddressFamilyComparerTests
    {
        /// <summary>
        ///     Verifies that <see cref="DefaultAddressFamilyComparer" /> is assignable to <see cref="IComparer{AddressFamily}" />.
        /// </summary>
        [Fact]
        public void Assignability_Test()
        {
            // Arrange
            var type = typeof(DefaultAddressFamilyComparer);

            // Act
            var isAssignableFrom = typeof(IComparer<AddressFamily>).IsAssignableFrom(type);

            // Assert
            Assert.True(isAssignableFrom);
        }

        #region Compare

        /// <summary>
        ///     Gets test data for <see cref="Compare_AddressFamilies_ReturnsExpectedOrdering_Test" />.
        ///     Covers all combinations of <see cref="AddressFamily.InterNetwork" /> and
        ///     <see cref="AddressFamily.InterNetworkV6" /> to verify ordinal ordering matches
        ///     <see cref="System.Enum.CompareTo" />.
        ///     <para>Parameters: expected comparison result (int), x (AddressFamily), y (AddressFamily).</para>
        /// </summary>
        /// <value>
        ///     Covers all combinations of <see cref="AddressFamily.InterNetwork" /> and
        ///     <see cref="AddressFamily.InterNetworkV6" /> to verify ordinal ordering matches
        ///     <see cref="System.Enum.CompareTo" />.
        ///     <para>Parameters: expected comparison result (int), x (AddressFamily), y (AddressFamily).</para>
        /// </value>
        public static TheoryData<int, AddressFamily, AddressFamily> Compare_AddressFamilies_ReturnsExpectedOrdering_Test_Data
        {
            get
            {
                var data = new TheoryData<int, AddressFamily, AddressFamily>();
                var concernedAddressFamilies = new[] { AddressFamily.InterNetwork, AddressFamily.InterNetworkV6 };

                foreach (var i in concernedAddressFamilies)
                {
                    foreach (var j in concernedAddressFamilies)
                    {
                        data.Add(i.CompareTo(j), i, j);
                    }
                }

                return data;
            }
        }

        /// <summary>
        ///     Verifies that <see cref="DefaultAddressFamilyComparer.Compare" /> returns the expected ordinal comparison
        ///     result for pairs of <see cref="AddressFamily" /> values.
        /// </summary>
        /// <param name="expected">the expected comparison result.</param>
        /// <param name="x">the left operand.</param>
        /// <param name="y">the right operand.</param>
        [Theory]
        [MemberData(nameof(Compare_AddressFamilies_ReturnsExpectedOrdering_Test_Data))]
        public void Compare_AddressFamilies_ReturnsExpectedOrdering_Test(int expected, AddressFamily x, AddressFamily y)
        {
            // Arrange
            var comparer = new DefaultAddressFamilyComparer();

            // Act
            var result = comparer.Compare(x, y);

            // Assert
            Assert.Equal(expected, result);
        }

        #endregion // end: Compare

        #region Instance

        /// <summary>
        ///     Verifies that <see cref="DefaultAddressFamilyComparer.Instance" /> is not <see langword="null" />
        ///     and is an instance of <see cref="DefaultAddressFamilyComparer" />.
        /// </summary>
        [Fact]
        public void Instance_IsNotNull_Test()
        {
            // Arrange
            // Act
            var instance = DefaultAddressFamilyComparer.Instance;

            // Assert
            Assert.NotNull(instance);
            Assert.IsType<DefaultAddressFamilyComparer>(instance);
        }

        #endregion // end: Instance
    }
}
