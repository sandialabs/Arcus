using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Arcus.Tests
{
    /// <content>
    ///     <see cref="Subnet"/> tests for static factory methods
    /// </content>
    public partial class SubnetTests
    {
        #region TryIPv4FromPartial

        /// <summary>Gets theory data for <see cref="TryIPv4FromPartial_Test"/>.</summary>
        /// <returns>Parameters: expected subnet result (<see cref="Subnet"/> or null), partial IPv4 input string.</returns>
        public static TheoryData<Subnet, string> TryIPv4FromPartial_Test_Values()
        {
            return new TheoryData<Subnet, string>
            {
                { null, null },
                { null, string.Empty },
                { null, "potato" },
                { Subnet.Parse("192.0.0.0/8"), "192" },
                { Subnet.Parse("192.0.0.0/8"), "192." },
                { Subnet.Parse("192.168.0.0/16"), "192.168" },
                { Subnet.Parse("192.168.0.0/16"), "192.168." },
                { Subnet.Parse("192.168.1.0/24"), "192.168.1" },
                { Subnet.Parse("192.168.1.0/24"), "192.168.1." },
                { Subnet.Parse("192.168.1.1/32"), "192.168.1.1" },
                { null, "192.168.1.1." },
                { null, "192.168.0.1.5" },
                // Addresses #59 - TryIPv4FromPartial throws FormatException for 0-prefixed octet not valid as octal
                { null, "10.209.005.029" },
            };
        }

        /// <summary>Verifies that <see cref="Subnet.TryIPv4FromPartial"/> returns the expected subnet for a given partial IPv4 string.</summary>
        /// <param name="expected">Expected subnet when parsing succeeds.</param>
        /// <param name="input">Partial IPv4 address string to parse.</param>
        [Theory]
        [MemberData(nameof(TryIPv4FromPartial_Test_Values))]
        public void TryIPv4FromPartial_Test(Subnet expected, string input)
        {
            // Arrange

            // Act
            var success = Subnet.TryIPv4FromPartial(input, out var subnet);

            // Assert
            Assert.Equal(expected != null, success);
            Assert.Equal(expected, subnet);
        }

        #endregion // end: TryIPv4FromPartial

        #region Ipv4OctetPartialPattern

        /// <summary>Gets theory data for <see cref="Ipv4OctetPartialPattern_IsMatch_ReturnsExpected_Test"/>.</summary>
        /// <value>Parameters: expected match result (bool), input string to test against the pattern.</value>
        public static TheoryData<bool, string> Ipv4OctetPartialPattern_Test_Data =>
            new()
            {
                // single octet - valid range
                { true, "0" },
                { true, "1" },
                { true, "192" },
                { true, "255" },
                // two octets
                { true, "192.168" },
                { true, "10.0" },
                // three octets
                { true, "192.168.1" },
                { true, "10.0.0" },
                // four octets - full addresses
                { true, "192.168.1.1" },
                { true, "0.0.0.0" },
                { true, "255.255.255.255" },
                // trailing dot (partial entry)
                { true, "192." },
                { true, "192.168." },
                { true, "192.168.1." },
                // non-matching - empty
                { false, string.Empty },
                // non-matching - octet out of range
                { false, "256" },
                { false, "999" },
                // non-matching - five groups
                { false, "192.168.0.1.5" },
                // non-matching - non-IPv4
                { false, "::" },
                { false, "potato" },
            };

        /// <summary>Verifies that <see cref="Subnet.Ipv4OctetPartialPattern"/> matches the expected inputs.</summary>
        /// <param name="expected">Whether the pattern should match the input.</param>
        /// <param name="input">The string to test against the pattern.</param>
        [Theory]
        [MemberData(nameof(Ipv4OctetPartialPattern_Test_Data))]
        public void Ipv4OctetPartialPattern_IsMatch_ReturnsExpected_Test(bool expected, string input)
        {
            // Arrange
            var regex = MyRegex();

            // Act
            var result = regex.IsMatch(input);

            // Assert
            Assert.Equal(expected, result);
        }

        #endregion // end: Ipv4OctetPartialPattern

        #region TryIPv6FromPartial

        /// <summary>Gets theory data for <see cref="TryIPv6FromPartial_Test"/>.</summary>
        /// <returns>Parameters: expected subnets (<see cref="IEnumerable{Subnet}"/>), partial IPv6 input string.</returns>
        public static TheoryData<IEnumerable<Subnet>, string> TryIPv6FromPartial_Test_Values()
        {
            var data = new TheoryData<IEnumerable<Subnet>, string>();

            // bad input formats
            data.Add(Enumerable.Empty<Subnet>(), null);
            data.Add(Enumerable.Empty<Subnet>(), string.Empty);
            data.Add(Enumerable.Empty<Subnet>(), "potato");

            // invalid input
            data.Add(Enumerable.Empty<Subnet>(), ":");
            data.Add(Enumerable.Empty<Subnet>(), ":::");
            data.Add(Enumerable.Empty<Subnet>(), "0:0:0:0:0:0:0:0:0"); // too many hextets
            data.Add(Enumerable.Empty<Subnet>(), "0:0:0:0:0:0:0:0::");

            // invalid input, multiple "::"
            data.Add(Enumerable.Empty<Subnet>(), "::0::");
            data.Add(Enumerable.Empty<Subnet>(), "0::0::0");
            data.Add(Enumerable.Empty<Subnet>(), "::0:0:0::");

            // explicit valid subnets
            data.Add(new[] { Subnet.Parse("::/128") }, "::/128");
            data.Add(new[] { Subnet.Parse("2001:db8:85a3:42::/64") }, "2001:db8:85a3:42::/64");
            data.Add(
                new[] { Subnet.Parse("2001:0db8:85a3:0042:1000:0001:0370:7334/128") },
                "2001:0db8:85a3:0042:1000:0001:0370:7334/128"
            );

            for (var hextetCount = 0; hextetCount <= 8; hextetCount++)
            {
                // CodeQL [cs/stringbuilder-creation-in-loop] Intentional — each iteration builds a distinct string from scratch; sharing a StringBuilder would require resetting and complicate the per-iteration boundary
                var sb = new StringBuilder();

#if NET48
                sb.Append(string.Join(":", Enumerable.Repeat("0", hextetCount)));
#else
                sb.AppendJoin(":", Enumerable.Repeat("0", hextetCount));
#endif

                if (hextetCount < 8)
                {
                    sb.Append("::");
                }

                var subnets = new List<Subnet>();
                for (var i = 0; i <= 8 - hextetCount; i++)
                {
                    subnets.Add(Subnet.Parse($"::/{128 - (16 * i)}"));
                }

                data.Add(subnets, sb.ToString());
            }

            var hextets = "2001:0db8:85a3:0042:1000:0001:0370:7334".Split(':');

            for (var hextetCount = 1; hextetCount <= 8; hextetCount++)
            {
                var subnets = new List<Subnet>();
                for (var i = 0; i <= 8 - hextetCount; i++)
                {
                    var enumerable = hextets.Take(hextetCount).Select(s => new string([.. s.SkipWhile(c => c == '0')]));
                    var trimmedLeadingZero = string.Join(":", enumerable);
                    var subnet =
                        hextetCount < 8
                            ? Subnet.Parse($"{trimmedLeadingZero}::/{128 - (16 * i)}")
                            : Subnet.Parse($"{trimmedLeadingZero}");

                    subnets.Add(subnet);
                }

                var inputString = string.Join(":", hextets.Take(hextetCount));

                if (hextetCount < 8)
                {
                    data.Add(subnets, $"{inputString}::");
                    data.Add(subnets, $"{inputString}:");
                }

                // A bare hextet pair without any trailing colon is treated as a partial address.
                // A single bare hextet (hextets[0]) is handled separately via the bare-hex path.
                if (!string.IsNullOrEmpty(inputString) && inputString != hextets[0])
                {
                    data.Add(subnets, inputString);
                }
            }

            // edge cases - CIDR boundary values
            data.Add(Enumerable.Empty<Subnet>(), "::/129"); // prefix exceeds 128
            data.Add(new[] { Subnet.Parse("::/0") }, "::/0"); // prefix of zero
            data.Add(Enumerable.Empty<Subnet>(), "::/"); // CIDR with missing prefix number
            data.Add(Enumerable.Empty<Subnet>(), "::/abc"); // CIDR with non-numeric prefix

            // edge cases - collapse in middle of address
            data.Add(
                [
                    Subnet.Parse("2001:db8:0:0:0:0:0:1/128"),
                    Subnet.Parse("2001:db8:0:0:0:0:1::/112"),
                    Subnet.Parse("2001:db8:0:0:0:1::/96"),
                    Subnet.Parse("2001:db8:0:0:1::/80"),
                    Subnet.Parse("2001:db8:0:1::/64"),
                    Subnet.Parse("2001:db8:1::/48"),
                ],
                "2001:db8::1"
            );

            // edge cases - uppercase hex (IPv6 is case-insensitive)
            data.Add(
                [
                    Subnet.Parse("2001:db8:0:0:0:0:0:0/128"),
                    Subnet.Parse("2001:db8:0:0:0:0:0::/112"),
                    Subnet.Parse("2001:db8:0:0:0:0::/96"),
                    Subnet.Parse("2001:db8:0:0:0::/80"),
                    Subnet.Parse("2001:db8:0:0::/64"),
                    Subnet.Parse("2001:db8:0::/48"),
                    Subnet.Parse("2001:db8::/32"),
                ],
                "2001:DB8::"
            );

            // edge cases - IPv4 mapped IPv6 should be treated as an exact /128
            data.Add(new[] { Subnet.Parse("::ffff:192.168.0.1/128") }, "::ffff:192.168.0.1/128");

            // edge cases - valid but incomplete, 6 hextets without "::"
            data.Add(
                [
                    Subnet.Parse("2001:db8:0:0:0:0:0:0/128"),
                    Subnet.Parse("2001:db8:0:0:0:0:0::/112"),
                    Subnet.Parse("2001:db8:0:0:0:0::/96"),
                ],
                "2001:db8:0:0:0:0:"
            );

            // edge cases - single non-zero hextet with trailing "::"
            data.Add(
                [
                    Subnet.Parse("abba:0:0:0:0:0:0:0/128"),
                    Subnet.Parse("abba:0:0:0:0:0:0::/112"),
                    Subnet.Parse("abba:0:0:0:0:0::/96"),
                    Subnet.Parse("abba:0:0:0:0::/80"),
                    Subnet.Parse("abba:0:0:0::/64"),
                    Subnet.Parse("abba:0:0::/48"),
                    Subnet.Parse("abba:0::/32"),
                    Subnet.Parse("abba::/16"),
                ],
                "abba::"
            );

            // edge cases - bare hextet pair without trailing "::"
            data.Add(
                [
                    Subnet.Parse("2001:db8:0:0:0:0:0:0/128"),
                    Subnet.Parse("2001:db8:0:0:0:0:0::/112"),
                    Subnet.Parse("2001:db8:0:0:0:0::/96"),
                    Subnet.Parse("2001:db8:0:0:0::/80"),
                    Subnet.Parse("2001:db8:0:0::/64"),
                    Subnet.Parse("2001:db8:0::/48"),
                    Subnet.Parse("2001:db8::/32"),
                ],
                "2001:db8:"
            );

            // edge cases - IPv6 input that looks like a valid complete address without hextet count of 8
            data.Add(new[] { Subnet.Parse("::1/128") }, "::1/128");
            data.Add(new[] { Subnet.Parse("fe80::1/128") }, "fe80::1/128");

            // bare hex word (no colons) - treated as single hextet with implicit "::"
            data.Add(
                [
                    Subnet.Parse("2001:0:0:0:0:0:0:0/128"),
                    Subnet.Parse("2001:0:0:0:0:0:0::/112"),
                    Subnet.Parse("2001:0:0:0:0:0::/96"),
                    Subnet.Parse("2001:0:0:0:0::/80"),
                    Subnet.Parse("2001:0:0:0::/64"),
                    Subnet.Parse("2001:0:0::/48"),
                    Subnet.Parse("2001:0::/32"),
                    Subnet.Parse("2001::/16"),
                ],
                "2001"
            );

            // bare hex word same as trailing "::" variant
            data.Add(
                [
                    Subnet.Parse("abba:0:0:0:0:0:0:0/128"),
                    Subnet.Parse("abba:0:0:0:0:0:0::/112"),
                    Subnet.Parse("abba:0:0:0:0:0::/96"),
                    Subnet.Parse("abba:0:0:0:0::/80"),
                    Subnet.Parse("abba:0:0:0::/64"),
                    Subnet.Parse("abba:0:0::/48"),
                    Subnet.Parse("abba:0::/32"),
                    Subnet.Parse("abba::/16"),
                ],
                "abba"
            );

            // whitespace trimming - leading/trailing spaces
            data.Add(new[] { Subnet.Parse("::/128") }, "  ::/128");
            data.Add(new[] { Subnet.Parse("::/128") }, "::/128  ");
            data.Add(new[] { Subnet.Parse("::/128") }, "  ::/128  ");

            // whitespace trimming with partial
            data.Add(
                [
                    Subnet.Parse("2001:db8:0:0:0:0:0:0/128"),
                    Subnet.Parse("2001:db8:0:0:0:0:0::/112"),
                    Subnet.Parse("2001:db8:0:0:0:0::/96"),
                    Subnet.Parse("2001:db8:0:0:0::/80"),
                    Subnet.Parse("2001:db8:0:0::/64"),
                    Subnet.Parse("2001:db8:0::/48"),
                    Subnet.Parse("2001:db8::/32"),
                ],
                "  2001:db8::  "
            );

            // URL-bracketed address (brackets are stripped; result matches the unbracketed case)
            data.Add(
                [
                    Subnet.Parse("2001:db8:0:0:0:0:0:1/128"),
                    Subnet.Parse("2001:db8:0:0:0:0:1::/112"),
                    Subnet.Parse("2001:db8:0:0:0:1::/96"),
                    Subnet.Parse("2001:db8:0:0:1::/80"),
                    Subnet.Parse("2001:db8:0:1::/64"),
                    Subnet.Parse("2001:db8:1::/48"),
                ],
                "[2001:db8::1]"
            );

            // URL-bracketed with CIDR
            data.Add(new[] { Subnet.Parse("2001:db8::/32") }, "[2001:db8::/32]");

            // URL-bracketed with whitespace
            data.Add(new[] { Subnet.Parse("::/0") }, "  [::/0]  ");

            // bracketed partial
            data.Add(
                [
                    Subnet.Parse("2001:db8:0:0:0:0:0:0/128"),
                    Subnet.Parse("2001:db8:0:0:0:0:0::/112"),
                    Subnet.Parse("2001:db8:0:0:0:0::/96"),
                    Subnet.Parse("2001:db8:0:0:0::/80"),
                    Subnet.Parse("2001:db8:0:0::/64"),
                    Subnet.Parse("2001:db8:0::/48"),
                    Subnet.Parse("2001:db8::/32"),
                ],
                "[2001:db8::]"
            );

            // invalid - unclosed bracket (no matching "]")
            data.Add(Enumerable.Empty<Subnet>(), "[2001:db8::");

            // invalid - unopened bracket (no matching "[")
            data.Add(Enumerable.Empty<Subnet>(), "2001:db8::]");

            // invalid - bare hex with colon is ambiguous
            data.Add(Enumerable.Empty<Subnet>(), "xyz::");

            // invalid - hex with invalid characters
            data.Add(Enumerable.Empty<Subnet>(), "2001:db8::zzzz");

            return data;
        }

        /// <summary>Verifies that <see cref="Subnet.TryIPv6FromPartial"/> returns the expected subnets
        /// for a given partial IPv6 string, in the correct order (most-specific prefix first).</summary>
        /// <param name="expected">Expected subnets in order, or an empty enumerable when the parse should fail.</param>
        /// <param name="input">Partial IPv6 address string to parse.</param>
        [Theory]
        [MemberData(nameof(TryIPv6FromPartial_Test_Values))]
        public void TryIPv6FromPartial_Test(IEnumerable<Subnet> expected, string input)
        {
            // Arrange
            // Act
#pragma warning disable CS0618 // testing a known obsolete method; ignoring the fact that it is obsolete
            var success = Subnet.TryIPv6FromPartial(input, out var subnets);
#pragma warning restore CS0618

            // Assert: verify count, content, AND order (most-specific prefix first)
            var expectedList = expected.ToList();
            var subnetList = subnets.ToList();

            Assert.Equal(expectedList.Count != 0, success);

            Assert.Equal(expectedList.Count, subnetList.Count);
            for (var i = 0; i < expectedList.Count; i++)
            {
                Assert.True(expectedList[i].Equals(subnetList[i]));
            }
        }

        #endregion // end: TryIPv6FromPartial

        #region Static Factory Methods

        #region FromBytes

        /// <summary>Gets theory data for <see cref="FromBytes_Bytes_Bytes_Test"/>.</summary>
        /// <returns>Parameters: expected subnet (<see cref="Subnet"/>), low address bytes (<see cref="T:byte[]"/>), high address bytes (<see cref="T:byte[]"/>).</returns>
        public static TheoryData<Subnet, byte[], byte[]> FromBytes_Bytes_Bytes_Test_Values()
        {
            var data = new TheoryData<Subnet, byte[], byte[]>();
            var seen = new HashSet<string>();

            foreach (var ipAddress in IPv4Addresses())
            {
                for (var i = 0; i <= 32; i++)
                {
                    var subnet = new Subnet(ipAddress, i);
                    var key =
                        $"{BitConverter.ToString(subnet.Head.GetAddressBytes())}|{BitConverter.ToString(subnet.Tail.GetAddressBytes())}";
                    if (seen.Add(key))
                    {
                        data.Add(subnet, subnet.Head.GetAddressBytes(), subnet.Tail.GetAddressBytes());
                    }
                }
            }

            foreach (var ipAddress in IPv6Addresses())
            {
                for (var i = 0; i <= 128; i++)
                {
                    var subnet = new Subnet(ipAddress, i);
                    var key =
                        $"{BitConverter.ToString(subnet.Head.GetAddressBytes())}|{BitConverter.ToString(subnet.Tail.GetAddressBytes())}";
                    if (seen.Add(key))
                    {
                        data.Add(subnet, subnet.Head.GetAddressBytes(), subnet.Tail.GetAddressBytes());
                    }
                }
            }

            return data;

            IEnumerable<IPAddress> IPv4Addresses()
            {
                yield return IPAddress.Any;
                yield return IPAddress.Loopback;
                yield return IPAddress.None;
                yield return IPAddress.Parse("192.168.1.1");
            }

            IEnumerable<IPAddress> IPv6Addresses()
            {
                yield return IPAddress.IPv6Any;
                yield return IPAddress.IPv6Loopback;
                yield return IPAddress.Parse("2001:0db8:85a3:0042:1000:8a2e:0370:7334");
            }
        }

        /// <summary>Verifies that <see cref="Subnet.FromBytes(byte[], byte[], int)"/> produces the expected subnet from address byte arrays.</summary>
        /// <param name="expected">Expected subnet.</param>
        /// <param name="lowAddressBytes">Byte array for the low address.</param>
        /// <param name="highAddressBytes">Byte array for the high address.</param>
        [Theory]
        [MemberData(nameof(FromBytes_Bytes_Bytes_Test_Values))]
        public void FromBytes_Bytes_Bytes_Test(Subnet expected, byte[] lowAddressBytes, byte[] highAddressBytes)
        {
            // Arrange
            // Act
            var subnet = Subnet.FromBytes(lowAddressBytes, highAddressBytes);

            // Assert
            Assert.Equal(expected, subnet);
        }

        /// <summary>Verifies that <see cref="Subnet.FromBytes(byte[], byte[], int)"/> throws <see cref="ArgumentNullException"/> for null or empty byte arrays.</summary>
        /// <param name="lowAddressBytes">Low address byte array.</param>
        /// <param name="highAddressBytes">High address byte array.</param>
        [Theory]
        [InlineData(null, new byte[] { 0x01, 0x01, 0xA8, 0xC0 })]
        [InlineData(new byte[] { 0x01, 0x01, 0xA8, 0xC0 }, null)]
        [InlineData(null, new byte[] { })]
        public void FromBytes_Null_Input_Throws_ArgumentNullException_Test(byte[] lowAddressBytes, byte[] highAddressBytes)
        {
            // Arrange
            // Act / Assert
            Assert.Throws<ArgumentNullException>(() => Subnet.FromBytes(lowAddressBytes, highAddressBytes));
        }

        /// <summary>Verifies that <see cref="Subnet.FromBytes(byte[], byte[], int)"/> throws <see cref="ArgumentException"/> for invalid byte array lengths.</summary>
        /// <param name="lowAddressBytes">Low address byte array with invalid length.</param>
        /// <param name="highAddressBytes">High address byte array with invalid length.</param>
        [Theory]
        [InlineData(new byte[] { 0x01, 0x01, 0xA8, 0xC0, 0xFF }, new byte[] { 0x01, 0x01, 0xA8, 0xC0 })]
        [InlineData(new byte[] { 0x01, 0x01, 0xA8, 0xC0 }, new byte[] { 0x01, 0x01, 0xA8, 0xC0, 0xFF })]
        [InlineData(new byte[] { }, new byte[] { 0x01, 0x01, 0xA8, 0xC0, 0xFF })]
        [InlineData(new byte[] { 0x01, 0x01, 0xA8, 0xC0, 0xFF }, new byte[] { })]
        public void FromBytes_Invalid_Input_Throws_ArgumentException_Test(byte[] lowAddressBytes, byte[] highAddressBytes)
        {
            // Arrange
            // Act
            var exception = Assert.Throws<ArgumentException>(() => Subnet.FromBytes(lowAddressBytes, highAddressBytes));

            // Assert
            Assert.IsType<ArgumentException>(exception.InnerException);
        }

        /// <summary>Verifies that <see cref="Subnet.FromBytes(byte[], byte[], int)"/> throws <see cref="InvalidOperationException"/> when the high address is lower than the low address.</summary>
        [Fact]
        public void FromBytes_HighAddressLowerThanLowAddress_Throws_InvalidOperationException_Test()
        {
            // Arrange
            var lowBytes = IPAddress.Parse("192.168.2.0").GetAddressBytes();
            var highBytes = IPAddress.Parse("192.168.1.0").GetAddressBytes();

            // Act / Assert
            Assert.Throws<InvalidOperationException>(() => Subnet.FromBytes(lowBytes, highBytes));
        }

        #endregion // end: FromBytes

        #region TryFromBytes

        /// <summary>Gets theory data for <see cref="TryFromBytes_Bytes_Bytes_Test"/>.</summary>
        /// <returns>Parameters: expected success (bool), expected subnet (<see cref="Subnet"/> or null), low address bytes (<see cref="T:byte[]"/>), high address bytes (<see cref="T:byte[]"/>).</returns>
        public static TheoryData<bool, Subnet, byte[], byte[]> TryFromBytes_Bytes_Bytes_Test_Values()
        {
            var data = new TheoryData<bool, Subnet, byte[], byte[]>();

            data.Add(false, null, null, null);
            data.Add(false, null, IPAddress.Any.GetAddressBytes(), IPAddress.IPv6Any.GetAddressBytes());
            data.Add(false, null, Array.Empty<byte>(), IPAddress.IPv6Any.GetAddressBytes());
            data.Add(false, null, Array.Empty<byte>(), IPAddress.Any.GetAddressBytes());
            data.Add(false, null, IPAddress.IPv6Any.GetAddressBytes(), Array.Empty<byte>());
            data.Add(false, null, IPAddress.Any.GetAddressBytes(), Array.Empty<byte>());

            data.Add(false, null, new byte[] { 0x00 }, IPAddress.IPv6Any.GetAddressBytes());
            data.Add(false, null, new byte[] { 0x00 }, IPAddress.Any.GetAddressBytes());
            data.Add(false, null, IPAddress.IPv6Any.GetAddressBytes(), new byte[] { 0x00 });
            data.Add(false, null, IPAddress.Any.GetAddressBytes(), new byte[] { 0x00 });

            var seen = new HashSet<string>();

            foreach (var ipAddress in IPv4Addresses())
            {
                for (var i = 0; i <= 32; i++)
                {
                    var subnet = new Subnet(ipAddress, i);
                    var key =
                        $"{BitConverter.ToString(subnet.Head.GetAddressBytes())}|{BitConverter.ToString(subnet.Tail.GetAddressBytes())}";
                    if (seen.Add(key))
                    {
                        data.Add(true, subnet, subnet.Head.GetAddressBytes(), subnet.Tail.GetAddressBytes());
                    }
                }
            }

            foreach (var ipAddress in IPv6Addresses())
            {
                for (var i = 0; i <= 128; i++)
                {
                    var subnet = new Subnet(ipAddress, i);
                    var key =
                        $"{BitConverter.ToString(subnet.Head.GetAddressBytes())}|{BitConverter.ToString(subnet.Tail.GetAddressBytes())}";
                    if (seen.Add(key))
                    {
                        data.Add(true, subnet, subnet.Head.GetAddressBytes(), subnet.Tail.GetAddressBytes());
                    }
                }
            }

            return data;

            IEnumerable<IPAddress> IPv4Addresses()
            {
                yield return IPAddress.Any;
                yield return IPAddress.Loopback;
                yield return IPAddress.None;
                yield return IPAddress.Parse("192.168.1.1");
            }

            IEnumerable<IPAddress> IPv6Addresses()
            {
                yield return IPAddress.IPv6Any;
                yield return IPAddress.IPv6Loopback;
                yield return IPAddress.Parse("2001:0db8:85a3:0042:1000:8a2e:0370:7334");
            }
        }

        /// <summary>Verifies that <see cref="Subnet.TryFromBytes(byte[], byte[], out Subnet, int)"/> returns the expected success flag and subnet.</summary>
        /// <param name="expectedSuccess">Expected return value of the try method.</param>
        /// <param name="expectedSubnet">Expected subnet output when parsing succeeds.</param>
        /// <param name="lowAddressBytes">Byte array for the low address.</param>
        /// <param name="highAddressBytes">Byte array for the high address.</param>
        [Theory]
        [MemberData(nameof(TryFromBytes_Bytes_Bytes_Test_Values))]
        public void TryFromBytes_Bytes_Bytes_Test(
            bool expectedSuccess,
            Subnet expectedSubnet,
            byte[] lowAddressBytes,
            byte[] highAddressBytes
        )
        {
            // Arrange
            // Act
            var success = Subnet.TryFromBytes(lowAddressBytes, highAddressBytes, out var subnet);

            // Assert
            Assert.Equal(expectedSuccess, success);
            Assert.Equal(expectedSubnet, subnet);
        }

        #endregion // end: TryFromBytes

        #region Parse(string)

        /// <summary>Gets theory data for <see cref="Parse_String_Test"/>.</summary>
        /// <returns>Parameters: expected subnet (<see cref="Subnet"/>), CIDR input string.</returns>
        public static TheoryData<Subnet, string> Parse_String_Test_Values()
        {
            var data = new TheoryData<Subnet, string>();

            foreach (var ipAddress in IPv4Addresses())
            {
                for (var i = 0; i <= 32; i++)
                {
                    var subnet = new Subnet(ipAddress, i);
                    data.Add(subnet, $"{ipAddress}/{i}");
                }
            }

            foreach (var ipAddress in IPv6Addresses())
            {
                for (var i = 0; i <= 128; i++)
                {
                    var subnet = new Subnet(ipAddress, i);
                    data.Add(subnet, $"{ipAddress}/{i}");
                }
            }

            return data;

            IEnumerable<IPAddress> IPv4Addresses()
            {
                yield return IPAddress.Any;
                yield return IPAddress.Loopback;
                yield return IPAddress.None;
                yield return IPAddress.Parse("192.168.1.1");
            }

            IEnumerable<IPAddress> IPv6Addresses()
            {
                yield return IPAddress.IPv6Any;
                yield return IPAddress.IPv6Loopback;
                yield return IPAddress.Parse("2001:0db8:85a3:0042:1000:8a2e:0370:7334");
            }
        }

        /// <summary>Verifies that <see cref="Subnet.Parse(string)"/> returns the expected subnet for a CIDR string.</summary>
        /// <param name="expected">Expected subnet.</param>
        /// <param name="input">CIDR string to parse.</param>
        [Theory]
        [MemberData(nameof(Parse_String_Test_Values))]
        public void Parse_String_Test(Subnet expected, string input)
        {
            // Arrange

            // Act
            var subnet = Subnet.Parse(input);

            // Assert
            Assert.Equal(expected, subnet);
        }

        /// <summary>Verifies that <see cref="Subnet.Parse(string)"/> throws <see cref="ArgumentNullException"/> for a null input.</summary>
        [Fact]
        public void Parse_String_Null_Throws_ArgumentNullException_Test()
        {
            // Arrange
            // Act / Assert
            // CodeQL [cs/useless-upcast] Explicit cast to string disambiguates overloads of Subnet.Parse when passing null — required for correct overload resolution
            Assert.Throws<ArgumentNullException>(() => Subnet.Parse((string)null));
        }

        /// <summary>Verifies that <see cref="Subnet.Parse(string)"/> throws <see cref="ArgumentException"/> for whitespace-only input.</summary>
        /// <param name="input">Whitespace input string.</param>
        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Parse_String_WhiteSpace_Throws_ArgumentException_Test(string input)
        {
            // Arrange
            // Act / Assert
            Assert.Throws<ArgumentException>(() => Subnet.Parse(input));
        }

        /// <summary>Verifies that <see cref="Subnet.Parse(string)"/> throws <see cref="FormatException"/> for a badly formatted string.</summary>
        [Fact]
        public void Parse_String_BadFormat_Throws_FormatException_Test()
        {
            // Arrange
            // Act / Assert
            Assert.Throws<FormatException>(() => Subnet.Parse("potato"));
        }

        /// <summary>Verifies that <see cref="Subnet.Parse(string)"/> throws <see cref="ArgumentException"/> when the IPv4 routing prefix exceeds 32.</summary>
        [Fact]
        public void Parse_String_IPv4RoutingPrefixTooLarge_Throws_ArgumentException_Test()
        {
            // Arrange
            // Act / Assert
            Assert.Throws<ArgumentException>(() => Subnet.Parse("192.168.1.0/33"));
        }

        /// <summary>Verifies that <see cref="Subnet.Parse(string)"/> throws <see cref="ArgumentException"/> when the IPv6 routing prefix exceeds 128.</summary>
        [Fact]
        public void Parse_String_IPv6RoutingPrefixTooLarge_Throws_ArgumentException_Test()
        {
            // Arrange
            // Act / Assert
            Assert.Throws<ArgumentException>(() => Subnet.Parse("::/129"));
        }

        #region S1 - Parse whitespace handling

        /// <summary>Verifies that <see cref="Subnet.Parse(string)"/> succeeds with leading whitespace.</summary>
        [Fact]
        public void Parse_WithLeadingWhitespace_Succeeds()
        {
            var subnet = Subnet.Parse(" 192.168.1.0/24");
            Assert.Equal(Subnet.Parse("192.168.1.0/24"), subnet);
        }

        /// <summary>Verifies that <see cref="Subnet.Parse(string)"/> succeeds with trailing whitespace.</summary>
        [Fact]
        public void Parse_WithTrailingWhitespace_Succeeds()
        {
            var subnet = Subnet.Parse("192.168.1.0/24 ");
            Assert.Equal(Subnet.Parse("192.168.1.0/24"), subnet);
        }

        /// <summary>Verifies that <see cref="Subnet.Parse(string)"/> succeeds with leading newline.</summary>
        [Fact]
        public void Parse_WithLeadingNewline_Succeeds()
        {
            var subnet = Subnet.Parse("\n10.0.0.0/8");
            Assert.Equal(Subnet.Parse("10.0.0.0/8"), subnet);
        }

        /// <summary>Verifies that <see cref="Subnet.Parse(string)"/> fails with internal whitespace.</summary>
        [Fact]
        public void Parse_WithInternalWhitespace_Fails()
        {
            Assert.Throws<FormatException>(() => Subnet.Parse("192.168. 1.0/24"));
        }

        #endregion // end: S1 - Parse whitespace handling

        #region S5 - Negative routing prefix

        /// <summary>Verifies that <see cref="Subnet.Parse(string)"/> throws <see cref="FormatException"/> for a negative routing prefix.</summary>
        [Fact]
        public void Parse_WithNegativeRoutingPrefix_ThrowsFormatException()
        {
            Assert.Throws<FormatException>(() => Subnet.Parse("192.168.1.0/-4"));
        }

        /// <summary>Verifies that <see cref="Subnet.Parse(string)"/> throws a <see cref="FormatException"/> with a clear message for a negative prefix.</summary>
        [Fact]
        public void Parse_WithNegativeRoutingPrefix_MessageIsClear()
        {
            var ex = Assert.Throws<FormatException>(() => Subnet.Parse("192.168.1.0/-4"));
            Assert.Contains("negative", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        #endregion // end: S5 - Negative routing prefix

        #endregion // end: Parse(string)

        #region RoughSubnetStringPattern

        /// <summary>Gets theory data for <see cref="RoughSubnetStringPattern_Matches_ReturnsExpected_Test"/>.</summary>
        /// <value>Parameters: expected match result (bool), input string to test against the pattern.</value>
        public static TheoryData<bool, string> RoughSubnetStringPattern_Test_Data =>
            new()
            {
                // IPv4 CIDR notation
                { true, "192.168.1.0/24" },
                { true, "0.0.0.0/0" },
                { true, "255.255.255.255/32" },
                // IPv6 CIDR notation
                { true, "::/0" },
                { true, "2001:db8::/32" },
                // case-insensitive (IgnoreCase)
                { true, "FEED:BEEF::/48" },
                { true, "feed:beef::/48" },
                // address only - no prefix
                { true, "192.168.1.1" },
                { true, "::" },
                { true, "2001:db8::1" },
                // non-matching - leading slash (no address part)
                { false, "/32" },
                // non-matching - empty string
                { false, string.Empty },
                // non-matching - internal space
                { false, "192.168.1.0 /24" },
            };

        /// <summary>Verifies that <see cref="Subnet.RoughSubnetStringPattern"/> matches the expected inputs.</summary>
        /// <param name="expected">Whether the pattern should produce exactly one match.</param>
        /// <param name="input">The string to test against the pattern.</param>
        [Theory]
        [MemberData(nameof(RoughSubnetStringPattern_Test_Data))]
        public void RoughSubnetStringPattern_Matches_ReturnsExpected_Test(bool expected, string input)
        {
            // Arrange
            // Production RoughSubnetRegex options (keep in sync):
            //   netstandard2.0: RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase
            //   Other TFMs:     [GeneratedRegex(RoughSubnetStringPattern, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase)]
            var regex = new Regex(Subnet.RoughSubnetStringPattern, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

            // Act
#if NET48
            var result = regex.Matches(input).Count == 1;
#else
            var result = regex.Count(input) == 1;
#endif

            // Assert
            Assert.Equal(expected, result);
        }

        #endregion // end: RoughSubnetStringPattern

        #region TryParse(string)

        /// <summary>Gets theory data for <see cref="TryParse_String_Test"/>.</summary>
        /// <returns>Parameters: expected subnet (<see cref="Subnet"/> or null), CIDR input string.</returns>
        public static TheoryData<Subnet, string> TryParse_String_Test_Values()
        {
            var data = new TheoryData<Subnet, string>();

            data.Add(null, null);
            data.Add(null, string.Empty);
            data.Add(null, "potato");
            data.Add(null, "2001:0db8:85a3:0042:1000:8a2e:0370:7334/129");
            data.Add(null, "0.0.0.0/33");
            data.Add(null, "0.0.0.0/potato");
            data.Add(null, "potato/16");
            data.Add(null, "0.0.0.0/");
            data.Add(null, "/32");

            foreach (var ipAddress in IPv4Addresses())
            {
                for (var i = 0; i <= 32; i++)
                {
                    var subnet = new Subnet(ipAddress, i);
                    data.Add(subnet, $"{ipAddress}/{i}");
                }

                data.Add(new Subnet(ipAddress, ipAddress), ipAddress.ToString());
            }

            foreach (var ipAddress in IPv6Addresses())
            {
                for (var i = 0; i <= 128; i++)
                {
                    var subnet = new Subnet(ipAddress, i);
                    data.Add(subnet, $"{ipAddress}/{i}");
                }

                data.Add(new Subnet(ipAddress, ipAddress), ipAddress.ToString());
            }

            return data;

            IEnumerable<IPAddress> IPv4Addresses()
            {
                yield return IPAddress.Any;
                yield return IPAddress.Loopback;
                yield return IPAddress.None;
                yield return IPAddress.Parse("192.168.1.1");
            }

            IEnumerable<IPAddress> IPv6Addresses()
            {
                yield return IPAddress.IPv6Any;
                yield return IPAddress.IPv6Loopback;
                yield return IPAddress.Parse("2001:0db8:85a3:0042:1000:8a2e:0370:7334");
            }
        }

        /// <summary>Verifies that <see cref="Subnet.TryParse(string, out Subnet)"/> returns the expected success flag and subnet.</summary>
        /// <param name="expected">Expected subnet output when parsing succeeds.</param>
        /// <param name="input">CIDR string to parse.</param>
        [Theory]
        [MemberData(nameof(TryParse_String_Test_Values))]
        public void TryParse_String_Test(Subnet expected, string input)
        {
            // Arrange

            // Act
            var success = Subnet.TryParse(input, out var subnet);

            // Assert
            Assert.Equal(expected != null, success);
            Assert.Equal(expected, subnet);
        }

        #endregion // end: TryParse(string)

        #region Parse(string, int, int)

        /// <summary>Gets theory data for <see cref="Parse_String_Int_Test"/>.</summary>
        /// <returns>Parameters: expected subnet (<see cref="Subnet"/>), address string, routing prefix (int).</returns>
        public static TheoryData<Subnet, string, int> Parse_String_Int_Test_Values()
        {
            var data = new TheoryData<Subnet, string, int>();

            foreach (var ipAddress in IPv4Addresses())
            {
                for (var i = 0; i <= 32; i++)
                {
                    var subnet = new Subnet(ipAddress, i);
                    data.Add(subnet, ipAddress.ToString(), i);
                }
            }

            foreach (var ipAddress in IPv6Addresses())
            {
                for (var i = 0; i <= 128; i++)
                {
                    var subnet = new Subnet(ipAddress, i);
                    data.Add(subnet, ipAddress.ToString(), i);
                }
            }

            return data;

            IEnumerable<IPAddress> IPv4Addresses()
            {
                yield return IPAddress.Any;
                yield return IPAddress.Loopback;
                yield return IPAddress.None;
                yield return IPAddress.Parse("192.168.1.1");
            }

            IEnumerable<IPAddress> IPv6Addresses()
            {
                yield return IPAddress.IPv6Any;
                yield return IPAddress.IPv6Loopback;
                yield return IPAddress.Parse("2001:0db8:85a3:0042:1000:8a2e:0370:7334");
            }
        }

        /// <summary>Verifies that <see cref="Subnet.Parse(string, int, int)"/> returns the expected subnet for an address string and routing prefix.</summary>
        /// <param name="expected">Expected subnet.</param>
        /// <param name="addressString">IP address string.</param>
        /// <param name="routePrefix">Routing prefix length.</param>
        [Theory]
        [MemberData(nameof(Parse_String_Int_Test_Values))]
        public void Parse_String_Int_Test(Subnet expected, string addressString, int routePrefix)
        {
            // Arrange
            // Act
            var subnet = Subnet.Parse(addressString, routePrefix);

            // Assert
            Assert.Equal(expected, subnet);
        }

        /// <summary>Verifies that <see cref="Subnet.Parse(string, int, int)"/> throws <see cref="ArgumentNullException"/> for a null address string.</summary>
        [Fact]
        public void Parse_String_Int_NullAddressString_Throws_ArgumentNullException_Test()
        {
            // Arrange
            // Act / Assert
            Assert.Throws<ArgumentNullException>(() => Subnet.Parse((string)null, 24));
        }

        /// <summary>Verifies that <see cref="Subnet.Parse(string, int, int)"/> throws <see cref="FormatException"/> for a badly formatted address string.</summary>
        [Fact]
        public void Parse_String_Int_BadAddressFormat_Throws_FormatException_Test()
        {
            // Arrange
            // Act / Assert
            Assert.Throws<FormatException>(() => Subnet.Parse("potato", 24));
        }

        /// <summary>Verifies that <see cref="Subnet.Parse(string, int, int)"/> throws <see cref="ArgumentOutOfRangeException"/> when the routing prefix is out of range.</summary>
        /// <param name="addressString">IP address string.</param>
        /// <param name="routingPrefix">The out-of-range routing prefix.</param>
        [Theory]
        [InlineData("192.168.1.0", -1)]
        [InlineData("192.168.1.0", 33)]
        [InlineData("::", -1)]
        [InlineData("::", 129)]
        public void Parse_String_Int_RoutingPrefixOutOfRange_Throws_ArgumentOutOfRangeException_Test(
            string addressString,
            int routingPrefix
        )
        {
            // Arrange
            // Act / Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => Subnet.Parse(addressString, routingPrefix));
        }

        #endregion // end: Parse(string, int, int)

        #region TryParse(string, int, int)

        /// <summary>Gets theory data for <see cref="TryParse_String_Int_Test"/>.</summary>
        /// <returns>Parameters: expected success (bool), expected subnet (<see cref="Subnet"/> or null), address string, routing prefix (int).</returns>
        public static TheoryData<bool, Subnet, string, int> TryParse_String_Int_Test_Values()
        {
            var data = new TheoryData<bool, Subnet, string, int>();

            data.Add(false, null, null, 0);
            data.Add(false, null, "potato", 0);
            data.Add(false, null, IPAddress.Any.ToString(), -5);
            data.Add(false, null, IPAddress.IPv6Any.ToString(), -5);
            data.Add(false, null, IPAddress.Any.ToString(), 33);
            data.Add(false, null, IPAddress.IPv6Any.ToString(), 129);

            foreach (var ipAddress in IPv4Addresses())
            {
                for (var i = 0; i <= 32; i++)
                {
                    var subnet = new Subnet(ipAddress, i);
                    data.Add(true, subnet, ipAddress.ToString(), i);
                }
            }

            foreach (var ipAddress in IPv6Addresses())
            {
                for (var i = 0; i <= 128; i++)
                {
                    var subnet = new Subnet(ipAddress, i);
                    data.Add(true, subnet, ipAddress.ToString(), i);
                }
            }

            return data;

            IEnumerable<IPAddress> IPv4Addresses()
            {
                yield return IPAddress.Any;
                yield return IPAddress.Loopback;
                yield return IPAddress.None;
                yield return IPAddress.Parse("192.168.1.1");
            }

            IEnumerable<IPAddress> IPv6Addresses()
            {
                yield return IPAddress.IPv6Any;
                yield return IPAddress.IPv6Loopback;
                yield return IPAddress.Parse("2001:0db8:85a3:0042:1000:8a2e:0370:7334");
            }
        }

        /// <summary>Verifies that <see cref="Subnet.TryParse(string, int, out Subnet, int)"/> returns the expected success flag and subnet.</summary>
        /// <param name="expectedSuccess">Expected return value of the try method.</param>
        /// <param name="expectedSubnet">Expected subnet output when parsing succeeds.</param>
        /// <param name="addressString">IP address string.</param>
        /// <param name="routePrefix">Routing prefix length.</param>
        [Theory]
        [MemberData(nameof(TryParse_String_Int_Test_Values))]
        public void TryParse_String_Int_Test(bool expectedSuccess, Subnet expectedSubnet, string addressString, int routePrefix)
        {
            // Arrange
            // Act
            var success = Subnet.TryParse(addressString, routePrefix, out var subnet);

            // Assert
            Assert.Equal(expectedSuccess, success);
            Assert.Equal(expectedSubnet, subnet);
        }

        #endregion // end: TryParse(string, int, int)

        #region Parse(string, string, int)

        /// <summary>Gets theory data for <see cref="Parse_String_String_Test"/>.</summary>
        /// <returns>Parameters: expected subnet (<see cref="Subnet"/>), low address string, high address string.</returns>
        public static TheoryData<Subnet, string, string> Parse_String_String_Test_Values()
        {
            var data = new TheoryData<Subnet, string, string>();
            var seen = new HashSet<string>();

            foreach (var ipAddress in IPv4Addresses())
            {
                for (var i = 0; i <= 32; i++)
                {
                    var subnet = new Subnet(ipAddress, i);
                    var low = subnet.Head.ToString();
                    var high = subnet.Tail.ToString();
                    if (seen.Add($"{low}|{high}"))
                    {
                        data.Add(subnet, low, high);
                    }
                }
            }

            foreach (var ipAddress in IPv6Addresses())
            {
                for (var i = 0; i <= 128; i++)
                {
                    var subnet = new Subnet(ipAddress, i);
                    var low = subnet.Head.ToString();
                    var high = subnet.Tail.ToString();
                    if (seen.Add($"{low}|{high}"))
                    {
                        data.Add(subnet, low, high);
                    }
                }
            }

            return data;

            IEnumerable<IPAddress> IPv4Addresses()
            {
                yield return IPAddress.Any;
                yield return IPAddress.Loopback;
                yield return IPAddress.None;
                yield return IPAddress.Parse("192.168.1.1");
            }

            IEnumerable<IPAddress> IPv6Addresses()
            {
                yield return IPAddress.IPv6Any;
                yield return IPAddress.IPv6Loopback;
                yield return IPAddress.Parse("2001:0db8:85a3:0042:1000:8a2e:0370:7334");
            }
        }

        /// <summary>Verifies that <see cref="Subnet.Parse(string, string, int)"/> returns the expected subnet for low and high address strings.</summary>
        /// <param name="expected">Expected subnet.</param>
        /// <param name="low">Low address string.</param>
        /// <param name="high">High address string.</param>
        [Theory]
        [MemberData(nameof(Parse_String_String_Test_Values))]
        public void Parse_String_String_Test(Subnet expected, string low, string high)
        {
            // Arrange
            // Act
            var subnet = Subnet.Parse(low, high);

            // Assert
            Assert.Equal(expected, subnet);
        }

        /// <summary>Verifies that <see cref="Subnet.Parse(string, string, int)"/> throws <see cref="ArgumentNullException"/> when either address string is null.</summary>
        /// <param name="low">Low address string.</param>
        /// <param name="high">High address string.</param>
        [Theory]
        [InlineData("::", null)]
        [InlineData(null, "::")]
        [InlineData("192.168.1.1", null)]
        [InlineData(null, "192.168.1.1")]
        [InlineData(null, null)]
        public void Parse_String_String_NullAddressString_Throws_ArgumentNullException_Test(string low, string high)
        {
            // Arrange
            // Act / Assert
            Assert.Throws<ArgumentNullException>(() => Subnet.Parse(low, high));
        }

        /// <summary>Verifies that <see cref="Subnet.Parse(string, string, int)"/> throws <see cref="FormatException"/> for badly formatted address strings.</summary>
        /// <param name="low">Low address string.</param>
        /// <param name="high">High address string.</param>
        [Theory]
        [InlineData("::", "potato")]
        [InlineData("potato", "::")]
        [InlineData("192.168.1.1", "potato")]
        [InlineData("potato", "192.168.1.1")]
        public void Parse_String_String_BadAddressFormat_Throws_FormatException_Test(string low, string high)
        {
            // Arrange
            // Act / Assert
            Assert.Throws<FormatException>(() => Subnet.Parse(low, high));
        }

        /// <summary>Verifies that <see cref="Subnet.Parse(string, string, int)"/> throws <see cref="ArgumentException"/> when the address strings are from different address families.</summary>
        /// <param name="low">Low address string.</param>
        /// <param name="high">High address string of a different family.</param>
        [Theory]
        [InlineData("192.168.1.1", "::")]
        [InlineData("::", "192.168.1.1")]
        public void Parse_String_String_MisMatchAddressFamily_Throws_ArgumentException_Test(string low, string high)
        {
            // Arrange
            // Act / Assert
            Assert.Throws<ArgumentException>(() => Subnet.Parse(low, high));
        }

        /// <summary>Verifies that <see cref="Subnet.Parse(string, string, int)"/> throws <see cref="InvalidOperationException"/> when the low address is greater than the high address.</summary>
        /// <param name="low">Low address string that is actually higher.</param>
        /// <param name="high">High address string that is actually lower.</param>
        [Theory]
        [InlineData("192.168.1.32", "192.168.1.0")]
        [InlineData("::20", "::")]
        public void Parse_String_String_InvalidRange_Throws_InvalidOperationException_Test(string low, string high)
        {
            // Arrange
            // Act / Assert
            Assert.Throws<InvalidOperationException>(() => Subnet.Parse(low, high));
        }

        #endregion // end: Parse(string, string, int)

        #region TryParse(string, string, int)

        /// <summary>Gets theory data for <see cref="TryParse_String_String_Test"/>.</summary>
        /// <returns>Parameters: expected subnet (<see cref="Subnet"/> or null), low address string, high address string.</returns>
        public static TheoryData<Subnet, string, string> TryParse_String_String_Test_Values()
        {
            var data = new TheoryData<Subnet, string, string>();

            foreach (var s in new[] { null, string.Empty, "\t", "potato" })
            {
                data.Add(null, "192.168.1.1", s);
                data.Add(null, s, "192.168.1.1");
                data.Add(null, "2001:0db8:85a3:0042:1000:8a2e:0370:7334", s);
                data.Add(null, s, "2001:0db8:85a3:0042:1000:8a2e:0370:7334");
            }

            var seen = new HashSet<string>();

            foreach (var ipAddress in IPv4Addresses())
            {
                for (var i = 0; i <= 32; i++)
                {
                    var subnet = new Subnet(ipAddress, i);
                    var low = subnet.Head.ToString();
                    var high = subnet.Tail.ToString();
                    if (seen.Add($"{low}|{high}"))
                    {
                        data.Add(subnet, low, high);
                    }
                }
            }

            foreach (var ipAddress in IPv6Addresses())
            {
                for (var i = 0; i <= 128; i++)
                {
                    var subnet = new Subnet(ipAddress, i);
                    var low = subnet.Head.ToString();
                    var high = subnet.Tail.ToString();
                    if (seen.Add($"{low}|{high}"))
                    {
                        data.Add(subnet, low, high);
                    }
                }
            }

            return data;

            IEnumerable<IPAddress> IPv4Addresses()
            {
                yield return IPAddress.Any;
                yield return IPAddress.Loopback;
                yield return IPAddress.None;
                yield return IPAddress.Parse("192.168.1.1");
            }

            IEnumerable<IPAddress> IPv6Addresses()
            {
                yield return IPAddress.IPv6Any;
                yield return IPAddress.IPv6Loopback;
                yield return IPAddress.Parse("2001:0db8:85a3:0042:1000:8a2e:0370:7334");
            }
        }

        /// <summary>Verifies that <see cref="Subnet.TryParse(string, string, out Subnet, int)"/> returns the expected success flag and subnet.</summary>
        /// <param name="expected">Expected subnet output when parsing succeeds.</param>
        /// <param name="low">Low address string.</param>
        /// <param name="high">High address string.</param>
        [Theory]
        [MemberData(nameof(TryParse_String_String_Test_Values))]
        public void TryParse_String_String_Test(Subnet expected, string low, string high)
        {
            // Arrange

            // Act
            var success = Subnet.TryParse(low, high, out var subnet);

            // Assert
            Assert.Equal(expected != null, success);
            Assert.Equal(expected, subnet);
        }

        #endregion // end: TryParse(string, string, int)

        #endregion // end: Static Factory Methods

        #region FromNetMask

        /// <summary>Gets theory data for <see cref="FromNetMask_Test"/>.</summary>
        /// <returns>Parameters: expected subnet (<see cref="Subnet"/>), network prefix address (<see cref="IPAddress"/>), netmask address (<see cref="IPAddress"/>).</returns>
        public static TheoryData<Subnet, IPAddress, IPAddress> FromNetMask_Test_Values()
        {
            var data = new TheoryData<Subnet, IPAddress, IPAddress>();
            var networkPrefix = IPAddress.Parse("192.168.1.1");

            for (var i = 0; i <= 32; i++)
            {
                var netmaskBytes = BigEndianBitWrapper.CreateMask(4, i).ToBytes();

                var netmask = new IPAddress(netmaskBytes);

                var expected = new Subnet(networkPrefix, i);

                data.Add(expected, networkPrefix, netmask);
            }

            return data;
        }

        /// <summary>Verifies that <see cref="Subnet.FromNetMask(IPAddress, IPAddress, int)"/> returns the expected subnet for a network prefix and netmask.</summary>
        /// <param name="expected">Expected subnet.</param>
        /// <param name="networkPrefix">Network prefix address.</param>
        /// <param name="netmask">Netmask address.</param>
        [Theory]
        [MemberData(nameof(FromNetMask_Test_Values))]
        public void FromNetMask_Test(Subnet expected, IPAddress networkPrefix, IPAddress netmask)
        {
            // Arrange
            // Act
            var result = Subnet.FromNetMask(networkPrefix, netmask);

            // Assert
            Assert.Equal(expected, result);
            Assert.Equal(netmask, result.Netmask);
        }

        /// <summary>Verifies that <see cref="Subnet.FromNetMask(IPAddress, IPAddress, int)"/> throws <see cref="ArgumentNullException"/> when the address is null.</summary>
        [Fact]
        public void FromNetMask_NullAddress_Throws_ArgumentNullException_Test()
        {
            // Arrange
            // Act / Assert
            Assert.Throws<ArgumentNullException>(() => Subnet.FromNetMask(null, IPAddress.Any));
        }

        /// <summary>Verifies that <see cref="Subnet.FromNetMask(IPAddress, IPAddress, int)"/> throws <see cref="ArgumentNullException"/> when the netmask is null.</summary>
        [Fact]
        public void FromNetMask_NullNetMask_Throws_ArgumentNullException_Test()
        {
            // Arrange
            // Act / Assert
            Assert.Throws<ArgumentNullException>(() => Subnet.FromNetMask(IPAddress.Any, null));
        }

        /// <summary>Verifies that <see cref="Subnet.FromNetMask(IPAddress, IPAddress, int)"/> throws <see cref="ArgumentException"/> when the netmask is invalid.</summary>
        [Fact]
        public void FromNetMask_InvalidNetMask_Throws_ArgumentException_Test()
        {
            // Arrange
            // Act / Assert
            Assert.Throws<ArgumentException>(() => Subnet.FromNetMask(IPAddress.Any, IPAddress.IPv6Any));
        }

        /// <summary>Verifies that <see cref="Subnet.FromNetMask(IPAddress, IPAddress, int)"/> throws <see cref="ArgumentException"/> when the netmask is an IPv4 address with non-contiguous 1-bits.</summary>
        [Fact]
        public void FromNetMask_NonContiguousNetMask_Throws_ArgumentException_Test()
        {
            // Arrange - 255.0.255.0 has non-contiguous 1-bits (gap in the second octet)
            var invalidNetmask = new IPAddress(new byte[] { 0xff, 0x00, 0xff, 0x00 });

            // Act / Assert
            Assert.Throws<ArgumentException>(() => Subnet.FromNetMask(IPAddress.Any, invalidNetmask));
        }

        /// <summary>Verifies that <see cref="Subnet.FromNetMask(IPAddress, IPAddress, int)"/> throws <see cref="ArgumentException"/> when the address is IPv6.</summary>
        [Fact]
        public void FromNetMask_IPv6Address_Throws_ArgumentException_Test()
        {
            // Arrange
            // Act / Assert
            Assert.Throws<ArgumentException>(() => Subnet.FromNetMask(IPAddress.IPv6Any, IPAddress.Any));
        }

        /// <summary>Gets theory data for <see cref="TryFromNetMask_Test"/>.</summary>
        /// <returns>Parameters: expected success (bool), expected subnet (<see cref="Subnet"/> or null), network prefix address (<see cref="IPAddress"/>), netmask address (<see cref="IPAddress"/>).</returns>
        public static TheoryData<bool, Subnet, IPAddress, IPAddress> TryFromNetMask_Test_Values()
        {
            var data = new TheoryData<bool, Subnet, IPAddress, IPAddress>();
            var networkPrefix = IPAddress.Parse("192.168.1.1");

            for (var i = 0; i <= 32; i++)
            {
                var netmaskBytes = BigEndianBitWrapper.CreateMask(4, i).ToBytes();

                var netmask = new IPAddress(netmaskBytes);

                var expected = new Subnet(networkPrefix, i);

                data.Add(true, expected, networkPrefix, netmask);
            }

            data.Add(false, null, null, IPAddress.Parse("192.168.1.1"));
            data.Add(false, null, IPAddress.Parse("192.168.1.1"), null);
            data.Add(false, null, null, IPAddress.Parse("::"));
            data.Add(false, null, IPAddress.Parse("::"), null);
            data.Add(false, null, null, null);

            return data;
        }

        /// <summary>Verifies that <see cref="Subnet.TryFromNetMask(IPAddress, IPAddress, out Subnet, int)"/> returns the expected success flag and subnet.</summary>
        /// <param name="expectedSuccess">Expected return value of the try method.</param>
        /// <param name="expectedSubnet">Expected subnet output when parsing succeeds.</param>
        /// <param name="networkPrefix">Network prefix address.</param>
        /// <param name="netmask">Netmask address.</param>
        [Theory]
        [MemberData(nameof(TryFromNetMask_Test_Values))]
        public void TryFromNetMask_Test(bool expectedSuccess, Subnet expectedSubnet, IPAddress networkPrefix, IPAddress netmask)
        {
            // Arrange
            // Act
            var success = Subnet.TryFromNetMask(networkPrefix, netmask, out var subnet);

            // Assert
            Assert.Equal(expectedSuccess, success);
            Assert.Equal(expectedSubnet, subnet);
        }

        #endregion // end: FromNetMask

        #region maxEnumerationExponent propagation

        /// <summary>Verifies that <see cref="Subnet.FromNetMask(IPAddress, IPAddress, int)"/> propagates maxEnumerationExponent.</summary>
        [Fact]
        public void FromNetMask_ExponentPropagates_Test()
        {
            var subnet = Subnet.FromNetMask(
                IPAddress.Parse("192.168.0.0"),
                IPAddress.Parse("255.255.0.0"),
                maxEnumerationExponent: 4
            );
            Assert.Equal(4, subnet.MaxEnumerationExponent);
        }

        /// <summary>Verifies that <see cref="Subnet.TryFromNetMask(IPAddress, IPAddress, out Subnet, int)"/> propagates maxEnumerationExponent.</summary>
        [Fact]
        public void TryFromNetMask_ExponentPropagates_Test()
        {
            Subnet.TryFromNetMask(
                IPAddress.Parse("192.168.0.0"),
                IPAddress.Parse("255.255.0.0"),
                out var subnet,
                maxEnumerationExponent: 4
            );
            Assert.Equal(4, subnet.MaxEnumerationExponent);
        }

        /// <summary>Verifies that <see cref="Subnet.FromBytes(byte[], byte[], int)"/> propagates maxEnumerationExponent.</summary>
        [Fact]
        public void FromBytes_ExponentPropagates_Test()
        {
            var low = new byte[] { 10, 0, 0, 0 };
            var high = new byte[] { 10, 0, 0, 255 };
            var subnet = Subnet.FromBytes(low, high, maxEnumerationExponent: 4);
            Assert.Equal(4, subnet.MaxEnumerationExponent);
        }

        /// <summary>Verifies that <see cref="Subnet.TryFromBytes(byte[], byte[], out Subnet, int)"/> propagates maxEnumerationExponent.</summary>
        [Fact]
        public void TryFromBytes_ExponentPropagates_Test()
        {
            var low = new byte[] { 10, 0, 0, 0 };
            var high = new byte[] { 10, 0, 0, 255 };
            Subnet.TryFromBytes(low, high, out var subnet, maxEnumerationExponent: 4);
            Assert.Equal(4, subnet.MaxEnumerationExponent);
        }

        /// <summary>Verifies that <see cref="Subnet.Parse(string, int, int)"/> propagates maxEnumerationExponent.</summary>
        [Fact]
        public void Parse_StringInt_ExponentPropagates_Test()
        {
            var subnet = Subnet.Parse("10.0.0.0", 24, maxEnumerationExponent: 4);
            Assert.Equal(4, subnet.MaxEnumerationExponent);
        }

        /// <summary>Verifies that <see cref="Subnet.Parse(string, string, int)"/> propagates maxEnumerationExponent.</summary>
        [Fact]
        public void Parse_StringString_ExponentPropagates_Test()
        {
            var subnet = Subnet.Parse("10.0.0.0", "10.0.0.255", maxEnumerationExponent: 4);
            Assert.Equal(4, subnet.MaxEnumerationExponent);
        }

        /// <summary>Verifies that <see cref="Subnet.TryParse(string, int, out Subnet, int)"/> propagates maxEnumerationExponent.</summary>
        [Fact]
        public void TryParse_StringInt_ExponentPropagates_Test()
        {
            Subnet.TryParse("10.0.0.0", 24, out var subnet, maxEnumerationExponent: 4);
            Assert.Equal(4, subnet.MaxEnumerationExponent);
        }

        /// <summary>Verifies that <see cref="Subnet.TryParse(string, string, out Subnet, int)"/> propagates maxEnumerationExponent.</summary>
        [Fact]
        public void TryParse_StringString_ExponentPropagates_Test()
        {
            Subnet.TryParse("10.0.0.0", "10.0.0.255", out var subnet, maxEnumerationExponent: 4);
            Assert.Equal(4, subnet.MaxEnumerationExponent);
        }

        /// <summary>Verifies that <see cref="Subnet.TryIPv4FromPartial(string, out Subnet, int)"/> propagates maxEnumerationExponent.</summary>
        [Fact]
        public void TryIPv4FromPartial_ExponentPropagates_Test()
        {
            Subnet.TryIPv4FromPartial("192.168", out var subnet, maxEnumerationExponent: 4);
            Assert.Equal(4, subnet.MaxEnumerationExponent);
        }

        /// <summary>Verifies that <see cref="Subnet.TryIPv6FromPartial(string, out IEnumerable{Subnet}, int)"/> propagates maxEnumerationExponent.</summary>
        [Fact]
        public void TryIPv6FromPartial_ExponentPropagates_Test()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            Subnet.TryIPv6FromPartial("2001:db8::", out var subnets, maxEnumerationExponent: 4);
#pragma warning restore CS0618
            foreach (var subnet in subnets)
            {
                Assert.Equal(4, subnet.MaxEnumerationExponent);
            }
        }

#if NET48
        private static readonly Regex MyRegexField = new(
            Subnet.Ipv4OctetPartialPattern,
            RegexOptions.Compiled | RegexOptions.CultureInvariant
        );

        private static Regex MyRegex() => MyRegexField;
#else
        [GeneratedRegex(Subnet.Ipv4OctetPartialPattern, RegexOptions.CultureInvariant)]
        private static partial Regex MyRegex();
#endif

        #endregion // end: maxEnumerationExponent propagation
    }
}
