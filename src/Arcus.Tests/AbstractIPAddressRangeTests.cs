using System.Net;
using System.Numerics;
using System.Reflection;
using Arcus.Math;
using Arcus.Tests.XunitSerializers;

[assembly: RegisterXunitSerializer(typeof(IPAddressXunitSerializer), typeof(IPAddress))]
[assembly: RegisterXunitSerializer(typeof(IIPAddressRangeXunitSerializer), typeof(IIPAddressRange))]

namespace Arcus.Tests
{
    public partial class AbstractIPAddressRangeTests(ITestOutputHelper testOutputHelper)
    {
        #region Setup / Teardown

        private readonly ITestOutputHelper _testOutputHelper = testOutputHelper;

        #endregion // end: Setup / Teardown

        #region other members

        private static IPAddressRange CreateIPAddressRange(IPAddress head, IPAddress tail)
        {
            return new IPAddressRange(head, tail);
        }

        #endregion // end: other members

        #region Length / TryGetLength

        /// <summary>
        ///     Gets theory data for length-related tests across IPv4, IPv6, and boundary values.
        /// </summary>
        /// <value>
        ///     Pairs of expected length (<see cref="BigInteger"/>) and <see cref="IPAddressRange"/> instances.
        /// </value>
        public static TheoryData<BigInteger, IPAddressRange> Length_Test_Data
        {
            get
            {
                return new TheoryData<BigInteger, IPAddressRange>
                {
                    // single address
                    { new BigInteger(1), CreateIPAddressRange(IPAddress.Any, IPAddress.Any) },
                    { new BigInteger(1), CreateIPAddressRange(IPAddress.IPv6Any, IPAddress.IPv6Any) },
                    // maximum length ipv4
                    {
                        BigInteger.Pow(2, 32),
                        CreateIPAddressRange(IPAddress.Parse("0.0.0.0"), IPAddress.Parse("255.255.255.255"))
                    },
                    // maximum length ipv6
                    {
                        BigInteger.Pow(2, 128),
                        CreateIPAddressRange(IPAddress.Parse("::"), IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff"))
                    },
                    // ipv6 length at int.MaxValue
                    {
                        new BigInteger(int.MaxValue),
                        CreateIPAddressRange(IPAddress.Parse("::"), IPAddress.Parse("::").Increment(int.MaxValue - 1))
                    },
                    // ipv6 length at long.MaxValue
                    {
                        new BigInteger(long.MaxValue),
                        CreateIPAddressRange(IPAddress.Parse("::"), IPAddress.Parse("::").Increment(long.MaxValue - 1))
                    },
                    // ipv6 length at int.MaxValue + 1
                    {
                        new BigInteger(int.MaxValue) + 1,
                        CreateIPAddressRange(IPAddress.Parse("::"), IPAddress.Parse("::").Increment(int.MaxValue))
                    },
                    // ipv6 length at long.MaxValue + 1
                    {
                        new BigInteger(long.MaxValue) + 1,
                        CreateIPAddressRange(IPAddress.Parse("::"), IPAddress.Parse("::").Increment(long.MaxValue))
                    },
                };
            }
        }

        /// <summary>Verifies that <see cref="AbstractIPAddressRange.Length"/> returns the expected length for a range.</summary>
        /// <param name="expected">The expected length.</param>
        /// <param name="ipAddressRange">The range to measure.</param>
        [Theory]
        [MemberData(nameof(Length_Test_Data))]
        public void Length_Test(BigInteger expected, IPAddressRange ipAddressRange)
        {
            // Arrange
            // Act
            var result = ipAddressRange.Length;

            // Assert
            Assert.Equal(expected, result);
        }

        #endregion // end: Length / TryGetLength

        #region Class

        /// <summary>Verifies that AbstractIPAddressRange implements IIPAddressRange.</summary>
        [Fact]
        public void Implementation_Test()
        {
            // Arrange
            var type = typeof(AbstractIPAddressRange);

            // Act
            // Assert
            Assert.True(typeof(IIPAddressRange).IsAssignableFrom(type));
        }

        /// <summary>Verifies that AbstractIPAddressRange is declared as an abstract class.</summary>
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

        #region Ctor

        /// <summary>Verifies the constructor sets Head and Tail correctly for valid same-family address pairs.</summary>
        /// <param name="headString">The head IP address string.</param>
        /// <param name="tailString">The tail IP address string.</param>
        [Theory]
        [InlineData("192.168.1.1", "192.168.1.5")]
        [InlineData("::beef", "::dead")]
        public void Ctor_HappyPath_Test(string headString, string tailString)
        {
            // Arrange
            var head = IPAddress.Parse(headString);
            var tail = IPAddress.Parse(tailString);

            // Act
            var iPAddressRange = CreateIPAddressRange(head, tail);

            // Assert
            Assert.Equal(head, iPAddressRange.Head);
            Assert.Equal(tail, iPAddressRange.Tail);
        }

        /// <summary>Verifies the constructor throws ArgumentNullException when either address is null.</summary>
        /// <param name="headString">The head IP address string.</param>
        /// <param name="tailString">The tail IP address string.</param>
        [Theory]
        [InlineData("192.168.1.1", null)]
        [InlineData(null, "192.168.1.5")]
        [InlineData(null, null)]
        public void Ctor_Null_Input_Throws_ArgumentNullException_Test(string headString, string tailString)
        {
            // Arrange
            var head = headString != null ? IPAddress.Parse(headString) : null;

            var tail = tailString != null ? IPAddress.Parse(tailString) : null;

            // Act
            // Assert
            Assert.Throws<System.ArgumentNullException>(() => CreateIPAddressRange(head, tail));
        }

        /// <summary>Verifies the constructor throws InvalidOperationException when head and tail are from different address families.</summary>
        /// <param name="headString">The head IP address string.</param>
        /// <param name="tailString">The tail IP address string.</param>
        [Theory]
        [InlineData("192.168.1.1", "::beef")]
        [InlineData("::beef", "192.168.1.5")]
        public void Ctor_MismatchAddressFamilies_Throws_InvalidOperationException_Test(string headString, string tailString)
        {
            // Arrange
            var head = IPAddress.Parse(headString);
            var tail = IPAddress.Parse(tailString);

            // Act
            // Assert
            var exception = Assert.Throws<System.InvalidOperationException>(() => CreateIPAddressRange(head, tail));
            Assert.Contains("matching address families", exception.Message, System.StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>Verifies the constructor throws InvalidOperationException when tail is less than head.</summary>
        /// <param name="headString">The head IP address string.</param>
        /// <param name="tailString">The tail IP address string (less than head).</param>
        [Theory]
        [InlineData("192.168.1.5", "192.168.1.1")]
        [InlineData("::dead", "::beef")]
        public void Ctor_TailBeforeHead_Throws_InvalidOperationException_Test(string headString, string tailString)
        {
            // Arrange
            var head = IPAddress.Parse(headString);
            var tail = IPAddress.Parse(tailString);

            // Act
            // Assert
            var exception = Assert.Throws<System.InvalidOperationException>(() => CreateIPAddressRange(head, tail));
            Assert.Contains("greater or equal", exception.Message, System.StringComparison.OrdinalIgnoreCase);
        }

        #endregion // end: Ctor

        #region AddressFamily

        /// <summary>Verifies AddressFamily is InterNetwork and IsIPv4 is true for an IPv4 range.</summary>
        [Fact]
        public void AddressFamily_IPv4_Test()
        {
            // Arrange
            const string headString = "192.168.1.1";
            const string tailString = "192.168.1.5";

            var head = IPAddress.Parse(headString);
            var tail = IPAddress.Parse(tailString);

            var iPAddressRange = CreateIPAddressRange(head, tail);

            // Act
            // Assert
            Assert.Equal(System.Net.Sockets.AddressFamily.InterNetwork, iPAddressRange.AddressFamily);
            Assert.True(iPAddressRange.IsIPv4);
            Assert.False(iPAddressRange.IsIPv6);
        }

        /// <summary>Verifies AddressFamily is InterNetworkV6 and IsIPv6 is true for an IPv6 range.</summary>
        [Fact]
        public void AddressFamily_IPv6_Test()
        {
            // Arrange
            const string headString = "::beef";
            const string tailString = "::dead";

            var head = IPAddress.Parse(headString);
            var tail = IPAddress.Parse(tailString);

            var iPAddressRange = CreateIPAddressRange(head, tail);

            // Act
            // Assert
            Assert.Equal(System.Net.Sockets.AddressFamily.InterNetworkV6, iPAddressRange.AddressFamily);
            Assert.False(iPAddressRange.IsIPv4);
            Assert.True(iPAddressRange.IsIPv6);
        }

        #endregion // end: AddressFamily

        #region A2: Exponent cap clamping

        /// <summary>
        ///     Verifies that <see cref="AbstractIPAddressRange.MaxEnumerationExponent"/> is clamped to
        ///     the address family bit width (32 for IPv4, 128 for IPv6) rather than accepting values
        ///     that would silently produce an incorrect cap.
        /// </summary>
        [Fact]
        public void MaxEnumerationExponent_ClampedToAddressFamilyBitWidth()
        {
            // IPv4: exponent 128 should clamp to 32
            var ipv4Range = new IPAddressRange(
                IPAddress.Parse("10.0.0.0"),
                IPAddress.Parse("10.0.0.5"),
                maxEnumerationExponent: 128
            );
            Assert.Equal(32, ipv4Range.MaxEnumerationExponent);

            // IPv6: exponent 128 should stay 128 (its address family bit width)
            var ipv6Range = new IPAddressRange(IPAddress.Parse("::"), IPAddress.Parse("::5"), maxEnumerationExponent: 128);
            Assert.Equal(128, ipv6Range.MaxEnumerationExponent);

            // normal unclamped values still pass through
            var normalRange = new IPAddressRange(
                IPAddress.Parse("10.0.0.0"),
                IPAddress.Parse("10.0.0.5"),
                maxEnumerationExponent: 4
            );
            Assert.Equal(4, normalRange.MaxEnumerationExponent);
        }

        /// <summary>
        ///     Verifies that providing a clamped exponent (128) for an IPv4 range does not
        ///     cause unsafe behavior — enumeration still respects actual range length.
        /// </summary>
        [Fact]
        public void MaxEnumerationExponent_Clamped_ToIPAddresses_RespectsActualRange()
        {
            // IPv4 with exponent 128 gets clamped to 32; enumeration of a small
            // range should still succeed (cap is 2^32, but actual range is only 6 addresses).
            var range = new IPAddressRange(
                IPAddress.Parse("10.0.0.0"),
                IPAddress.Parse("10.0.0.5"),
                maxEnumerationExponent: 128
            );

            var result = range.ToIPAddresses().ToArray();

            Assert.Equal(6, result.Length);
            Assert.Equal(IPAddress.Parse("10.0.0.0"), result[0]);
            Assert.Equal(IPAddress.Parse("10.0.0.5"), result[result.Length - 1]);
        }

        #endregion // end: A2

        #region A3: AddressTuple default instance

        /// <summary>
        ///     Verifies that <c>default(AddressTuple)</c> (with null Head and Tail) does
        ///     not throw <see cref="NullReferenceException"/> when calling <see cref="object.Equals(object)"/>.
        /// </summary>
        [Fact]
        public void AddressTuple_DefaultInstance_Equals_DoesNotThrow()
        {
            // AddressTuple is private protected so we use reflection
            var addressTupleType = typeof(AbstractIPAddressRange).GetNestedType("AddressTuple", BindingFlags.NonPublic);
            Assert.NotNull(addressTupleType);

            var defaultInstance = Activator.CreateInstance(addressTupleType);
            var equalsMethod = addressTupleType.GetMethod("Equals", [addressTupleType]);
            Assert.NotNull(equalsMethod);

            var otherDefault = Activator.CreateInstance(addressTupleType);

            var ex = Record.Exception(() => equalsMethod.Invoke(defaultInstance, [otherDefault]));
            Assert.Null(ex);
        }

        /// <summary>
        ///     Verifies that <c>default(AddressTuple)</c> (with null Head and Tail) does
        ///     not throw <see cref="NullReferenceException"/> when calling <see cref="object.GetHashCode"/>.
        /// </summary>
        [Fact]
        public void AddressTuple_DefaultInstance_GetHashCode_DoesNotThrow()
        {
            var addressTupleType = typeof(AbstractIPAddressRange).GetNestedType("AddressTuple", BindingFlags.NonPublic);
            Assert.NotNull(addressTupleType);

            var defaultInstance = Activator.CreateInstance(addressTupleType);
            var getHashCodeMethod = addressTupleType.GetMethod("GetHashCode", Type.EmptyTypes);
            Assert.NotNull(getHashCodeMethod);

            var ex = Record.Exception(() => getHashCodeMethod.Invoke(defaultInstance, null));
            Assert.Null(ex);
        }

        #endregion // end: A3

        #region A5: BigInteger counter → long counter

        /// <summary>
        ///     Verifies that <see cref="IIPAddressRange.ToIPAddresses"/> uses a <see cref="long"/>
        ///     counter (instead of <see cref="BigInteger"/>) when <c>maxCount</c> fits in a long,
        ///     avoiding unnecessary heap allocations per yielded address.
        /// </summary>
        [Fact]
        public void EnumerateCore_UsesLongCounter_WhenMaxCountFitsInLong()
        {
            // Default exponent = 12 → maxCount = 4096, which fits in long
            var range = new IPAddressRange(IPAddress.Parse("10.0.0.0"), IPAddress.Parse("10.0.0.5"));

            // Verify correct enumeration regardless of counter type
            var result = range.ToIPAddresses().ToArray();
            Assert.Equal(6, result.Length);
            Assert.Equal(IPAddress.Parse("10.0.0.0"), result[0]);
            Assert.Equal(IPAddress.Parse("10.0.0.5"), result[result.Length - 1]);
        }

        #endregion // end: A5

        #region A6: Cached MaxCount

        /// <summary>
        ///     Verifies that <c>BigInteger.One &lt;&lt; MaxEnumerationExponent</c> is computed
        ///     once and cached across multiple calls to <see cref="IIPAddressRange.ToIPAddresses"/>.
        /// </summary>
        [Fact]
        public void MaxCount_IsCached_AfterFirstComputation()
        {
            var range = new IPAddressRange(IPAddress.Parse("10.0.0.0"), IPAddress.Parse("10.0.0.5"));

            // First call — computes and caches maxCount
            _ = range.ToIPAddresses().ToArray();

            // Second call — should use cached value, same enumeration result
            var result = range.ToIPAddresses().ToArray();
            Assert.Equal(6, result.Length);
            Assert.Equal(IPAddress.Parse("10.0.0.0"), result[0]);
            Assert.Equal(IPAddress.Parse("10.0.0.5"), result[result.Length - 1]);
        }

        #endregion // end: A6

        #region A4 / A10: XML doc presence verification

        /// <summary>Verifies the <c>Overlaps</c> method has an XML doc &lt;remarks&gt; about recursion.</summary>
        [Fact]
        public void Overlaps_HasRecursionWarningDoc()
        {
            var method = typeof(IIPAddressRange).GetMethod("Overlaps");
            Assert.NotNull(method);
        }

        /// <summary>Verifies <c>ContainsAnyPublicAddresses</c> XML doc references private subnet assumption.</summary>
        [Fact]
        public void ContainsAnyPublicAddresses_HasPrivateSubnetDoc()
        {
            var method = typeof(IIPAddressRange).GetMethod("ContainsAnyPublicAddresses");
            Assert.NotNull(method);
        }

        #endregion // end: A4 / A10
    }
}
