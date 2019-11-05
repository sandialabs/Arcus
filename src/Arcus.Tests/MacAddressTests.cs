using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using Gulliver;
using Xunit;

namespace Arcus.Tests
{
    /// <summary>Tests for <see cref="MacAddress" /></summary>
    public class MacAddressTests
    {
        #region IComparable<MacAddress>

        [Theory]
        [MemberData(nameof(Comparison_MacAddress_MacAddress_Values))]
        public void CompareTo_MacAddress_Test(int expected,
                                              MacAddress left,
                                              MacAddress right)
        {
            // Arrange
            // Act
            var result = left.CompareTo(right);

            // Assert
            Assert.Equal(expected, result);
        }

        #endregion end: IComparable<MacAddress>

        #region DefaultMacAddress

        [Fact]
        public void DefaultMacAddress_Test()
        {
            // Arrange
            var expected = Enumerable.Repeat((byte) 0xFF, 6)
                                     .ToArray();

            // Act
            var defaultMacAddress = MacAddress.DefaultMacAddress;

            // Assert
            Assert.NotNull(defaultMacAddress);
            Assert.Equal(0, ByteArrayUtils.CompareUnsignedBigEndian(expected, defaultMacAddress.GetAddressBytes()));
        }

        #endregion end: DefaultMacAddress

        #region GetAddressBytes

        [Theory]
        [InlineData(new byte[] {0xff, 0xff, 0xff, 0xff, 0xff, 0xff})]
        [InlineData(new byte[] {0x00, 0x00, 0x00, 0x00, 0x00, 0x00})]
        [InlineData(new byte[] {0x00, 0xff, 0x00, 0xff, 0x00, 0x00})]
        public void GetAddressBytes_Test(byte[] bytes)
        {
            // Arrange
            var macAddress = new MacAddress(bytes);

            // Act
            var addressBytes = macAddress.GetAddressBytes();

            // Assert
            Assert.NotNull(addressBytes);
            Assert.IsType<byte[]>(addressBytes);
            Assert.Equal(addressBytes.Length, addressBytes.Length);
            Assert.Equal(0, ByteArrayUtils.CompareUnsignedBigEndian(bytes, addressBytes));
        }

        #endregion end: GetAddressBytes

        #region GetCidBytes

        [Fact]
        public void GetCidBytes_Test()
        {
            // Arrange
            var bytes = new byte[] {0x01, 0x23, 0x45, 0x67, 0x89, 0xAB};

            var macAddress = new MacAddress(bytes);

            // Act
            var cidBytes = macAddress.GetCidBytes();

            // Assert
            Assert.Equal(0,
                         ByteArrayUtils.CompareUnsignedBigEndian(bytes.Skip(3)
                                                                      .Take(3)
                                                                      .ToArray(),
                                                                 cidBytes));
        }

        #endregion end: GetCidBytes

        #region GetHashCode

        [Theory]
        [MemberData(nameof(Comparison_MacAddress_MacAddress_Values))]
        public void GetHashCode_Test(int expected,
                                     MacAddress left,
                                     MacAddress right)
        {
            // Arrange
            if (left is null
                || right is null)
            {
                return; // skip null values and bail
            }

            // Act
            var leftHashCode = left.GetHashCode();
            var rightHashCode = right.GetHashCode();

            // Assert
            Assert.Equal(expected == 0, leftHashCode == rightHashCode);
        }

        #endregion end: GetHashCode

        #region GetOuiBytes

        [Fact]
        public void GetOuiBytes_Test()
        {
            // Arrange
            var bytes = new byte[] {0x01, 0x23, 0x45, 0x67, 0x89, 0xAB};

            var macAddress = new MacAddress(bytes);

            // Act
            var ouiBytes = macAddress.GetOuiBytes();

            // Assert
            Assert.Equal(0,
                         ByteArrayUtils.CompareUnsignedBigEndian(bytes.Take(3)
                                                                      .ToArray(),
                                                                 ouiBytes));
        }

        #endregion end: GetOuiBytes

        #region IsGloballyUnique

        [Theory]
        [InlineData(true, new byte[] {0b0000_0000, 0x00, 0x00, 0x00, 0x00, 0x00})]
        [InlineData(false, new byte[] {0b0000_0010, 0x00, 0x00, 0x00, 0x00, 0x00})]
        [InlineData(true, new byte[] {0b1111_1101, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF})]
        [InlineData(false, new byte[] {0b1111_1111, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF})]
        public void IsGloballyUnique_Test(bool expected,
                                          byte[] bytes)
        {
            // Arrange
            var macAddress = new MacAddress(bytes);

            // Act
            var isGloballyUnique = macAddress.IsGloballyUnique;

            // Assert
            Assert.Equal(expected, isGloballyUnique);
        }

        #endregion end: IsGloballyUnique

        #region IsLocallyAdministered

        [Theory]
        [InlineData(false, new byte[] {0b0000_0000, 0x00, 0x00, 0x00, 0x00, 0x00})]
        [InlineData(true, new byte[] {0b0000_0010, 0x00, 0x00, 0x00, 0x00, 0x00})]
        [InlineData(false, new byte[] {0b1111_1101, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF})]
        [InlineData(true, new byte[] {0b1111_1111, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF})]
        public void IsLocallyAdministered_Test(bool expected,
                                               byte[] bytes)
        {
            // Arrange
            var macAddress = new MacAddress(bytes);

            // Act
            var isLocallyAdministered = macAddress.IsLocallyAdministered;

            // Assert
            Assert.Equal(expected, isLocallyAdministered);
        }

        #endregion end: IsLocallyAdministered

        #region IsMulticast

        [Theory]
        [InlineData(false, new byte[] {0b0000_0000, 0x00, 0x00, 0x00, 0x00, 0x00})]
        [InlineData(true, new byte[] {0b0000_0001, 0x00, 0x00, 0x00, 0x00, 0x00})]
        [InlineData(false, new byte[] {0b1111_1110, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF})]
        [InlineData(true, new byte[] {0b1111_1111, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF})]
        public void IsMulticast_Test(bool expected,
                                     byte[] bytes)
        {
            // Arrange
            var macAddress = new MacAddress(bytes);

            // Act
            var isMulticast = macAddress.IsMulticast;

            // Assert
            Assert.Equal(expected, isMulticast);
        }

        #endregion end: IsMulticast

        #region IsUnicast

        [Theory]
        [InlineData(true, new byte[] {0b0000_0000, 0x00, 0x00, 0x00, 0x00, 0x00})]
        [InlineData(false, new byte[] {0b0000_0001, 0x00, 0x00, 0x00, 0x00, 0x00})]
        [InlineData(true, new byte[] {0b1111_1110, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF})]
        [InlineData(false, new byte[] {0b1111_1111, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF})]
        public void IsUnicast_Test(bool expected,
                                   byte[] bytes)
        {
            // Arrange
            var macAddress = new MacAddress(bytes);

            // Act
            var isUnicast = macAddress.IsUnicast;

            // Assert
            Assert.Equal(expected, isUnicast);
        }

        #endregion end: IsUnicast

        #region other members

        public static IEnumerable<object[]> Comparison_MacAddress_MacAddress_Values()
        {
            var byteArrayMax = Enumerable.Repeat((byte) 0xff, 6)
                                         .ToArray();

            var byteArrayMin = Enumerable.Repeat((byte) 0x00, 6)
                                         .ToArray();

            var macAddressMax = new MacAddress(byteArrayMax);

            var macAddressMin = new MacAddress(byteArrayMin);

            yield return new object[] {0, macAddressMin, macAddressMin};                // same equal
            yield return new object[] {0, macAddressMin, new MacAddress(byteArrayMin)}; // same underlying bytes

            yield return new object[] {-1, macAddressMin, macAddressMax}; // left less than right
            yield return new object[] {1, macAddressMax, macAddressMin};  // left greater than right

            yield return new object[] {1, macAddressMin, null}; // right is null
        }

        public static IEnumerable<object[]> Comparison_MacAddress_Object_Values()
        {
            var bytes = Enumerable.Repeat((byte) 0xff, 6)
                                  .ToArray();
            var macAddress = new MacAddress(bytes);

            yield return new object[] {-1, macAddress, "string"};
            yield return new object[] {-1, macAddress, 42};
            yield return new object[] {-1, macAddress, bytes};
        }

        #endregion

        #region IsDefault

        public static IEnumerable<object[]> IsDefault_Test_Values()
        {
            yield return new object[] {true, MacAddress.DefaultMacAddress};
            yield return new object[] {true, new MacAddress(Enumerable.Repeat((byte) 0xFF, 6))};
            yield return new object[] {false, new MacAddress(Enumerable.Repeat((byte) 0x00, 6))};
        }

        [Theory]
        [MemberData(nameof(IsDefault_Test_Values))]
        public void IsDefault_Test(bool expeted,
                                   MacAddress macAddress)
        {
            // Arrange
            // Act
            var isDefault = macAddress.IsDefault;

            // Assert
            Assert.Equal(expeted, isDefault);
        }

        #endregion end: IsDefault

        #region IsUnusable

        public static IEnumerable<object[]> IsUnusable_Test_Values()
        {
            yield return new object[] {false, new MacAddress(Enumerable.Repeat((byte) 0x00, 6))};
            yield return new object[]
                         {
                             false, new MacAddress(Enumerable.Repeat((byte) 0x00, 3)
                                                             .Concat(Enumerable.Repeat((byte) 0xFF, 3)))
                         };
            yield return new object[] {true, new MacAddress(Enumerable.Repeat((byte) 0x01, 6))};
            yield return new object[] {true, new MacAddress(Enumerable.Repeat((byte) 0xFF, 6))};
        }

        [Theory]
        [MemberData(nameof(IsUnusable_Test_Values))]
        public void IsUnusable_Test(bool expected,
                                    MacAddress macAddress)
        {
            // Arrange

            // Act
            var isUsable = macAddress.IsUnusable;

            // Assert
            Assert.Equal(expected, isUsable);
        }

        #endregion end: IsUnusable

        #region ctor bytes[]

        [Theory]
        [InlineData(0)]
        [InlineData(5)]
        [InlineData(7)]
        public void Ctor_WrongByteCount_Throws_ArgumentException_Test(int byteCount)
        {
            // Arrange
            var bytes = Enumerable.Repeat((byte) 0xAC, byteCount)
                                  .ToArray();

            // Act
            // Assert
            Assert.Throws<ArgumentException>(() => new MacAddress(bytes));
        }

        [Fact]
        public void Ctor_NullBytes_Throws_ArgumentNullException_Test()
        {
            // Arrange
            // Act
            // Assert
            Assert.Throws<ArgumentNullException>(() => new MacAddress(null));
        }

        #endregion end: ctor bytes[]

        #region ISerializable

        public static IEnumerable<object[]> CanSerializable_Test_Values()
        {
            yield return new object[] {new MacAddress(Enumerable.Repeat((byte) 0x00, 6))};
            yield return new object[] {new MacAddress(Enumerable.Repeat((byte) 0xFF, 6))};
            yield return new object[] {new MacAddress(new byte[] {0x00, 0xCD, 0xEF, 0x01, 0x23, 0x45})};
        }

        [Theory]
        [MemberData(nameof(CanSerializable_Test_Values))]
        public void CanSerializable_Test(MacAddress macAddress)
        {
            // Arrange
            var formatter = new BinaryFormatter();

            // Act
            using var writeStream = new MemoryStream();
            formatter.Serialize(writeStream, macAddress);

            var bytes = writeStream.ToArray();
            var readStream = new MemoryStream(bytes);
            var result = formatter.Deserialize(readStream);

            // Assert
            Assert.IsType<MacAddress>(result);
            Assert.Equal(macAddress, result);
        }

        #endregion end: ISerializable

        #region type

        [Theory]
        [InlineData(typeof(IEquatable<MacAddress>))]
        [InlineData(typeof(IComparable<MacAddress>))]
        [InlineData(typeof(IComparable))]
        [InlineData(typeof(IFormattable))]
        [InlineData(typeof(ISerializable))]
        public void Assignability_Test(Type assignableFromType)
        {
            // Arrange
            var type = typeof(MacAddress);

            // Act
            var isAssignableFrom = assignableFromType.IsAssignableFrom(type);

            // Assert
            Assert.True(isAssignableFrom);
        }

        [Fact]
        public void IsConcrete_Test()
        {
            // Arrange
            var type = typeof(MacAddress);

            // Act
            var isConcrete = type.IsClass && !type.IsAbstract;

            // Assert
            Assert.True(isConcrete);
        }

        #endregion end: type

        #region IComparable

        [Theory]
        [MemberData(nameof(Comparison_MacAddress_MacAddress_Values))]
        public void CompareTo_Object_Test(int expected,
                                          MacAddress left,
                                          object right)
        {
            // Arrange
            // Act
            var result = left.CompareTo(right);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [MemberData(nameof(Comparison_MacAddress_Object_Values))]
        public void CompareTo_Object_NotMacAddress_Test(int expected,
                                                        MacAddress left,
                                                        object right)
        {
            // Arrange
            // Act
            // Assert
            Assert.Throws<ArgumentException>(() => left.CompareTo(right));
        }

        #endregion end: IComparable

        #region Parse

        #region Parse string

        [Theory]
        [MemberData(nameof(Parse_Test_SuccessValues))]
        public void Parse_HappyPath_Test(MacAddress expected,
                                         string input)
        {
            // Arrange

            // Act
            var macAddress = MacAddress.Parse(input);

            // Assert
            Assert.Equal(expected, macAddress);
        }

        [Theory]
        [MemberData(nameof(Parse_Test_FailValues))]
        public void Parse_Failure_Test(MacAddress expected,
                                       string input)
        {
            // Arrange
            // Act
            // Assert
            Assert.ThrowsAny<Exception>(() => MacAddress.Parse(input));
        }

        [Theory]
        [MemberData(nameof(Parse_Test_SuccessValues))]
        [MemberData(nameof(Parse_Test_FailValues))]
        public void TryParse_Test(MacAddress expected,
                                  string input)
        {
            // Arrange

            // Act
            var success = MacAddress.TryParse(input, out var macAddress);

            // Assert
            Assert.True(expected is null ^ success);
            Assert.Equal(expected, macAddress);
        }

        public static IEnumerable<object[]> Parse_Test_FailValues()
        {
            yield return new object[] {null, null};
            yield return new object[] {null, string.Empty};
            yield return new object[] {null, "A"};
            yield return new object[] {null, "potato"};
        }

        public static IEnumerable<object[]> Parse_Test_SuccessValues()
        {
            var formatProvider = CultureInfo.InvariantCulture;
            var formats = new[] {"g", "c", "s", "d", "X"};

            var bytes = new byte[] {0x00, 0xCD, 0xEF, 0x01, 0x23, 0x45};

            var macAddresses = new[]
                               {
                                   new MacAddress(bytes),
                                   new MacAddress(bytes.ReverseBytes())
                               };

            foreach (var macAddress in macAddresses)
            {
                foreach (var format in formats)
                {
                    var s = macAddress.ToString(format, formatProvider);
                    yield return new object[] {macAddress, s};
                    yield return new object[] {macAddress, s.ToLower(CultureInfo.InvariantCulture)};
                }
            }
        }

        #endregion end: Parse string

        #region ParseAny

        [Theory]
        [MemberData(nameof(ParseAny_Test_SuccessValues))]
        public void ParseAny_HappyPath_Test(MacAddress expected,
                                            string input)
        {
            // Arrange

            // Act
            var macAddress = MacAddress.ParseAny(input);

            // Assert
            Assert.Equal(expected, macAddress);
        }

        [Theory]
        [MemberData(nameof(ParseAny_Test_FailValues))]
        public void ParseAny_Failure_Test(MacAddress expected,
                                          string input)
        {
            // Arrange
            // Act
            // Assert
            Assert.ThrowsAny<Exception>(() => MacAddress.ParseAny(input));
        }

        [Theory]
        [MemberData(nameof(ParseAny_Test_SuccessValues))]
        [MemberData(nameof(ParseAny_Test_FailValues))]
        public void TryParseAny_Test(MacAddress expected,
                                     string input)
        {
            // Arrange

            // Act
            var success = MacAddress.TryParseAny(input, out var macAddress);

            // Assert
            Assert.True(expected is null ^ success);
            Assert.Equal(expected, macAddress);
        }

        public static IEnumerable<object[]> ParseAny_Test_FailValues()
        {
            yield return new object[] {null, null};
            yield return new object[] {null, string.Empty};
            yield return new object[] {null, "A"};
            yield return new object[] {null, "potato"};
        }

        public static IEnumerable<object[]> ParseAny_Test_SuccessValues()
        {
            foreach (var testCase in StandardFormatTestCases())
            {
                yield return testCase;
            }

            foreach (var testCase in NonStandardFormatTestCases())
            {
                yield return testCase;
            }

            static IEnumerable<object[]> NonStandardFormatTestCases()
            {
                var bytes = new byte[] {0x00, 0xCD, 0xEF, 0x01, 0x23, 0x45};
                var expected = new MacAddress(bytes);
                var byteStrings = bytes.Select(b => $"{b:X2}")
                                       .ToArray();

                yield return new object[] {expected, string.Join("?!&*", byteStrings)};
                yield return new object[] {expected, $"-_={string.Join(string.Empty, byteStrings)}=_-"};
                yield return new object[]
                             {
                                 expected, string.Join("_",
                                                       string.Join(string.Empty, byteStrings)
                                                             .Select(c => $"{c}"))
                             };
            }

            static IEnumerable<object[]> StandardFormatTestCases()
            {
                var formatProvider = CultureInfo.InvariantCulture;
                var formats = new[] {"g", "c", "s", "d", "X"};

                var bytes = new byte[] {0x00, 0xCD, 0xEF, 0x01, 0x23, 0x45};

                var macAddresses = new[]
                                   {
                                       new MacAddress(bytes),
                                       new MacAddress(bytes.ReverseBytes())
                                   };

                foreach (var macAddress in macAddresses)
                {
                    foreach (var format in formats)
                    {
                        var s = macAddress.ToString(format, formatProvider);
                        yield return new object[] {macAddress, s};
                        yield return new object[] {macAddress, s.ToLower(CultureInfo.InvariantCulture)};
                    }
                }
            }
        }

        #endregion end: ParseAny

        #endregion end: Parse

        #region IFormattable

        public static IEnumerable<object[]> IFormattable_Test_Values()
        {
            var formatProvider = CultureInfo.InvariantCulture;
            var bytes = new byte[] {0x00, 0xCD, 0xEF, 0x01, 0x23, 0x45};

            yield return new object[] {"00:CD:EF:01:23:45", bytes, null, formatProvider};
            yield return new object[] {"00:CD:EF:01:23:45", bytes, string.Empty, formatProvider};
            yield return new object[] {"00:CD:EF:01:23:45", bytes, "g", formatProvider};

            yield return new object[] {"00cdef012345", bytes, "x", formatProvider};
            yield return new object[] {"00CDEF012345", bytes, "X", formatProvider};

            yield return new object[] {"00CD.EF01.2345", bytes, "c", formatProvider};

            yield return new object[] {"00 CD EF 01 23 45", bytes, "s", formatProvider};

            yield return new object[] {"884478124869", bytes, "i", formatProvider};

            yield return new object[] {"00-CD-EF-01-23-45", bytes, "d", formatProvider};
        }

        [Theory]
        [MemberData(nameof(IFormattable_Test_Values))]
        public void IFormattable_Test(string expected,
                                      byte[] bytes,
                                      string format,
                                      IFormatProvider formatProvider)
        {
            // Arrange
            var macAddress = new MacAddress(bytes);

            // Act
            var result = macAddress.ToString(format, formatProvider);

            // Assert
            Assert.Equal(expected, result, StringComparer.Ordinal);
        }

        #endregion end: IFormattable

        #region IEquatable<MacAddress>

        [Theory]
        [MemberData(nameof(Comparison_MacAddress_MacAddress_Values))]
        public void Equals_MacAddress_Test(int expected,
                                           MacAddress left,
                                           MacAddress right)
        {
            // Arrange
            // Act
            var result = left.Equals(right);

            // Assert
            Assert.Equal(expected == 0, result);
        }

        [Theory]
        [MemberData(nameof(Comparison_MacAddress_MacAddress_Values))]
        [MemberData(nameof(Comparison_MacAddress_Object_Values))]
        public void Equals_Object_Test(int expected,
                                       MacAddress left,
                                       object right)
        {
            // Arrange
            // Act
            var result = left.Equals(right);

            // Assert
            Assert.Equal(expected == 0, result);
        }

        #endregion end: IEquatable<MacAddress>

        #region operators

        #region equal

        [Theory]
        [MemberData(nameof(Comparison_MacAddress_MacAddress_Values))]
        public void Operator_Equal_Test(int expected,
                                        MacAddress left,
                                        MacAddress right)
        {
            // Arrange
            // Act
            var result = left == right;

            // Assert
            Assert.Equal(expected == 0, result);
        }

        #endregion end: equal

        #region not equal

        [Theory]
        [MemberData(nameof(Comparison_MacAddress_MacAddress_Values))]
        public void Operator_NotEqual_Test(int expected,
                                           MacAddress left,
                                           MacAddress right)
        {
            // Arrange
            // Act
            var result = left != right;

            // Assert
            Assert.Equal(expected != 0, result);
        }

        #endregion end: not equal

        #region less than

        [Theory]
        [MemberData(nameof(Comparison_MacAddress_MacAddress_Values))]
        public void Operator_LessThan_Test(int expected,
                                           MacAddress left,
                                           MacAddress right)
        {
            // Arrange
            // Act
            var result = left < right;

            // Assert
            Assert.Equal(expected == -1, result);
        }

        #endregion end: less than

        #region greater than

        [Theory]
        [MemberData(nameof(Comparison_MacAddress_MacAddress_Values))]
        public void Operator_GreaterThan_Test(int expected,
                                              MacAddress left,
                                              MacAddress right)
        {
            // Arrange
            // Act
            var result = left > right;

            // Assert
            Assert.Equal(expected == 1, result);
        }

        #endregion end: greater than

        #region less than or equal

        [Theory]
        [MemberData(nameof(Comparison_MacAddress_MacAddress_Values))]
        public void Operator_LessThanOrEqualTo_Test(int expected,
                                                    MacAddress left,
                                                    MacAddress right)
        {
            // Arrange
            // Act
            var result = left <= right;

            // Assert
            Assert.Equal(expected <= 0, result);
        }

        #endregion end: less than or equal

        #region greater than or equal

        [Theory]
        [MemberData(nameof(Comparison_MacAddress_MacAddress_Values))]
        public void Operator_GreaterThanOrEqual_Test(int expected,
                                                     MacAddress left,
                                                     MacAddress right)
        {
            // Arrange
            // Act
            var result = left >= right;

            // Assert
            Assert.Equal(expected >= 0, result);
        }

        #endregion end: greater than or equal

        #endregion end: operators

        #region AllFormatMacAddressRegularExpression

        public static IEnumerable<object[]> AllFormantMacAddressRefularExpressionMatches()
        {
            var values = new[]
                         {
                             "00D.7FB.576.F14",
                             "01 D5 7A D5 18 9B",
                             "015.721.263.754",
                             "02 41 7F 6E AA 59",
                             "041E8FF5EB0B",
                             "08 ED 85 A7 56 6D",
                             "0D-FF-28-25-33-B1",
                             "14:C7:A3:D3:B4:97",
                             "158213168721",
                             "15:D4:84:DE:72:A7",
                             "19-7E-1D-96-48-59",
                             "1C DD 35 BA E0 B8",
                             "1C56A2268779",
                             "1D8.E06.643.9BF",
                             "22-EA-45-59-E9-99",
                             "23E4.6AE9.14EF",
                             "24D3240A23C3",
                             "24 F4 73 7E DF 24",
                             "29:2A:00:D4:11:C1",
                             "2DD.215.C37.CF1",
                             "31-D5-77-54-5C-B4",
                             "314D705B7F5D",
                             "3277.E822.DBEE",
                             "34:ED:5C:3A:C2:26",
                             "38:46:A8:85:2A:F1",
                             "3D3E466575FE",
                             "3DE7.E1C3.F33D",
                             "3E:5B:37:15:C7:D0",
                             "3F-8B-BE-06-B6-F7",
                             "42-9E-C0-72-EF-EA",
                             "4420.6185.24C0",
                             "4ABB.D96B.6197",
                             "4E7C.49D9.ABA8",
                             "5436.5D82.9215",
                             "55 B3 13 7D 98 EF",
                             "565.FBF.891.1DB",
                             "5A6.1A2.B36.643",
                             "5C4FE09A5CEE",
                             "5E758934109F",
                             "68:40:C6:33:61:67",
                             "69-61-46-84-5B-03",
                             "6961.6D06.864A",
                             "69CA.30DC.394C",
                             "6AA3C4C8FAED",
                             "6E-B6-1B-11-CA-28",
                             "70:47:5F:E3:42:8A",
                             "70D.04B.5EA.88C",
                             "72 71 DC 01 99 79",
                             "75BB2A7F454A",
                             "765.E80.3FE.05D"
                         };

            foreach (var value in values)
            {
                yield return new object[] {value};
                yield return new object[] {value.ToLowerInvariant()};
            }
        }

        [Theory]
        [MemberData(nameof(AllFormantMacAddressRefularExpressionMatches))]
        public void AllFormantMacAddressRefularExpression_Matches_Test(string input)
        {
            // Arrange
            // Act
            var isMatch = MacAddress.AllFormatMacAddressRegularExpression.IsMatch(input);

            // Assert
            Assert.True(isMatch);
            var count = Regex.Matches(input, @"[\dA-Fa-f]")
                             .Count;
            Assert.Equal(12,
                         count);
        }

        #endregion end: AllFormatMacAddressRegularExpression

        #region CommonFormatMacAddressRegularExpression

        public static IEnumerable<object[]> CommonFormatMacAddressRegularExpressionMatches()
        {
            var values = new[]
                         {
                             "DC:F1:EE:7C:9A:E1",
                             "67:2F:7C:7A:FF:C8",
                             "B9:53:18:2A:71:7D",
                             "29:11:05:52:82:92",
                             "E9:33:5E:09:75:72",
                             "33:2F:3B:80:ED:BD",
                             "A1:29:5A:EF:DE:F0",
                             "28:9F:D1:37:29:42",
                             "F4:84:B5:55:44:66",
                             "27:3E:BF:1A:79:52",
                             "09:AF:25:C2:B0:BB",
                             "58:9B:98:C7:7D:FE",
                             "55:7D:F4:80:B8:5F",
                             "F3:E1:FC:C6:69:5E",
                             "50:BF:59:FD:80:94",
                             "1D:18:5C:C7:62:61",
                             "0C:31:21:91:B7:80",
                             "30:5B:A5:91:57:DD",
                             "A4:20:B2:52:F7:E9",
                             "63:93:49:2E:3C:5F",
                             "4B:ED:5C:DF:EE:B9",
                             "12:63:97:53:12:B4",
                             "13:DC:EE:70:5E:47",
                             "CA:1E:E1:6F:AC:3E",
                             "DF:96:00:4B:51:56",
                             "59:26:6A:11:DC:D3",
                             "D0:DC:A7:1B:CE:A1",
                             "AB:72:20:97:BD:E4",
                             "03:DD:05:A9:63:D5",
                             "93:58:E8:3F:AA:2C",
                             "48:98:C8:B8:B6:B2",
                             "C1:E0:D1:95:4C:D2",
                             "D0:3F:44:DE:07:D8",
                             "D6:26:E3:18:03:29",
                             "98:C7:5C:A6:E3:4F",
                             "FB:E8:2A:DF:55:00",
                             "E5:28:A5:B0:49:FF",
                             "1B:63:3F:2C:19:9A",
                             "2D:2D:FA:92:80:AF",
                             "DA:B2:60:C3:C3:AB",
                             "27:81:0B:67:BC:02",
                             "8F:35:3E:DB:7E:69",
                             "50:16:42:2D:5D:C3",
                             "3F:0D:45:E9:0F:90",
                             "94:58:0D:54:72:D9",
                             "FE:E8:9E:19:F2:0F",
                             "4C:7F:D1:E4:4D:4B",
                             "A0:09:97:78:65:95",
                             "E6:01:AC:DF:D3:DC",
                             "49:24:6D:2E:30:58"
                         };

            foreach (var value in values)
            {
                yield return new object[] {value};
                yield return new object[] {value.ToLowerInvariant()};
            }
        }

        [Theory]
        [MemberData(nameof(CommonFormatMacAddressRegularExpressionMatches))]
        public void CommonFormatMacAddressRegularExpression_Matches_Test(string input)
        {
            // Arrange
            // Act
            var isMatch = MacAddress.CommonFormatMacAddressRegularExpression.IsMatch(input);

            // Assert
            Assert.True(isMatch);
            var count = Regex.Matches(input, @"[\dA-Fa-f]")
                             .Count;
            Assert.Equal(12, count);
        }

        #endregion end: CommonFormatMacAddressRegularExpression
    }
}
