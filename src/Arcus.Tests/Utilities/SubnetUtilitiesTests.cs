using System.Net;
using Arcus.Comparers;
using Arcus.Math;
using Arcus.Utilities;

namespace Arcus.Tests.Utilities
{
    /// <summary>Unit tests for <see cref="SubnetUtilities"/>.</summary>
    public class SubnetUtilitiesTests
    {
        #region PrivateIPAddressRangesList

        /// <summary>Verifies that <see cref="SubnetUtilities.PrivateIPAddressRangesList"/> returns a read-only list containing the four expected private subnets.</summary>
        [Fact]
        public void PrivateIPAddressRangesList_Test()
        {
            // Arrange
            var expectedSubnets = new[]
            {
                Subnet.Parse("10.0.0.0", 8),
                Subnet.Parse("172.16.0.0", 12),
                Subnet.Parse("192.168.0.0", 16),
                Subnet.Parse("fd00::", 8),
            };

            // Act
            var list = SubnetUtilities.PrivateIPAddressRangesList;

            // Assert
            Assert.IsType<IReadOnlyList<Subnet>>(list, exactMatch: false);
            Assert.Equal(4, list.Count);
            Assert.Equal(list.Count, list.Distinct().Count());
            Assert.Contains(list, s => s.IsIPv4);
            Assert.Contains(list, s => s.IsIPv6);
            Assert.All(
                list,
                subnet =>
                {
                    Assert.NotNull(subnet);
                    Assert.Contains(subnet, expectedSubnets);
                }
            );
        }

        #endregion // end: PrivateIPAddressRangesList

        #region LinkLocalIPAddressRangesList

        /// <summary>Verifies that <see cref="SubnetUtilities.LinkLocalIPAddressRangesList"/> returns a read-only list containing the two expected link-local subnets.</summary>
        [Fact]
        public void LinkLocalIPAddressRangesList_Test()
        {
            // Arrange
            var expectedSubnets = new[] { Subnet.Parse("169.254.0.0", 16), Subnet.Parse("fe80::", 10) };

            // Act
            var list = SubnetUtilities.LinkLocalIPAddressRangesList;

            // Assert
            Assert.IsType<IReadOnlyList<Subnet>>(list, exactMatch: false);
            Assert.Equal(2, list.Count);
            Assert.Equal(list.Count, list.Distinct().Count());
            Assert.Contains(list, s => s.IsIPv4);
            Assert.Contains(list, s => s.IsIPv6);
            Assert.All(
                list,
                subnet =>
                {
                    Assert.NotNull(subnet);
                    Assert.Contains(subnet, expectedSubnets);
                }
            );
        }

        #endregion // end: LinkLocalIPAddressRangesList

        #region FewestConsecutiveSubnetsFor

        /// <summary>Gets theory data for <see cref="FewestConsecutiveSubnetsFor_ValidInputs_ReturnsExpectedSubnets_Test"/>.</summary>
        /// <returns>Parameters: expected (IEnumerable&lt;Subnet&gt;), left (IPAddress), right (IPAddress).</returns>
        public static IEnumerable<
            TheoryDataRow<IEnumerable<Subnet>, IPAddress, IPAddress>
        > FewestConsecutiveSubnetsFor_ValidInputs_ReturnsExpectedSubnets_Test_Data()
        {
            // single IPv4 address (same left and right)
            yield return new TheoryDataRow<IEnumerable<Subnet>, IPAddress, IPAddress>(
                [Subnet.Parse("128.64.20.3/32")],
                IPAddress.Parse("128.64.20.3"),
                IPAddress.Parse("128.64.20.3")
            );

            // small IPv4 range
            yield return new TheoryDataRow<IEnumerable<Subnet>, IPAddress, IPAddress>(
                SourceArray.Select(s => Subnet.Parse(s)),
                IPAddress.Parse("128.64.20.3"),
                IPAddress.Parse("128.64.20.12")
            );

            // small IPv4 range (reversed order - verifies min/max normalization)
            yield return new TheoryDataRow<IEnumerable<Subnet>, IPAddress, IPAddress>(
                SourceArray.Select(s => Subnet.Parse(s)),
                IPAddress.Parse("128.64.20.12"),
                IPAddress.Parse("128.64.20.3")
            );

            // IPv4 range ending in /31
            yield return new TheoryDataRow<IEnumerable<Subnet>, IPAddress, IPAddress>(
                SmallIPv4Range.Select(s => Subnet.Parse(s)),
                IPAddress.Parse("192.168.1.3"),
                IPAddress.Parse("192.168.1.5")
            );

            // single IPv6 address (same left and right)
            yield return new TheoryDataRow<IEnumerable<Subnet>, IPAddress, IPAddress>(
                [Subnet.Parse("2001:400:4402::/128")],
                IPAddress.Parse("2001:400:4402::"),
                IPAddress.Parse("2001:400:4402::")
            );

            // full IPv6 /48 block - exact subnet boundary
            yield return new TheoryDataRow<IEnumerable<Subnet>, IPAddress, IPAddress>(
                [Subnet.Parse("2001:400:4402::/48")],
                IPAddress.Parse("2001:400:4402::"),
                IPAddress.Parse("2001:400:4402:ffff:ffff:ffff:ffff:ffff")
            );

            // complex IPv6 range crossing many subnet boundaries
            yield return new TheoryDataRow<IEnumerable<Subnet>, IPAddress, IPAddress>(
                ComplexIPv6Range.Select(s => Subnet.Parse(s)),
                IPAddress.Parse("2001:400:4402::ffff"),
                IPAddress.Parse("2001:400:4402:ffff:ffff:ffff:ffff:ffff")
            );

            // near-full IPv4 range (0.0.0.1 to 255.255.255.254)
            yield return new TheoryDataRow<IEnumerable<Subnet>, IPAddress, IPAddress>(
                NearFullIPv4Range.Select(s => Subnet.Parse(s)),
                IPAddress.Parse("0.0.0.1"),
                IPAddress.Parse("255.255.255.254")
            );

            // near-full IPv6 range (::1 to ffff:...:fffe)
            yield return new TheoryDataRow<IEnumerable<Subnet>, IPAddress, IPAddress>(
                NearFullIPv6Range.Select(s => Subnet.Parse(s)),
                IPAddress.Parse("::1"),
                IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:ffff:ffff:fffe")
            );
        }

        /// <summary>Verifies that <see cref="SubnetUtilities.FewestConsecutiveSubnetsFor"/> returns the expected minimal set of subnets for valid address pairs.</summary>
        /// <param name="expected">Expected collection of subnets covering the range.</param>
        /// <param name="left">Left bound of the IP address range.</param>
        /// <param name="right">Right bound of the IP address range.</param>
        [Theory]
        [MemberData(nameof(FewestConsecutiveSubnetsFor_ValidInputs_ReturnsExpectedSubnets_Test_Data))]
        public void FewestConsecutiveSubnetsFor_ValidInputs_ReturnsExpectedSubnets_Test(
            IEnumerable<Subnet> expected,
            IPAddress left,
            IPAddress right
        )
        {
            // Arrange
            var expectedList = expected.ToList();

            // Act
            var result = SubnetUtilities.FewestConsecutiveSubnetsFor(left, right).ToList();

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.All(result, Assert.NotNull);
            Assert.Equal(expectedList.Count, result.Count);
            Assert.All(expectedList, subnet => Assert.Contains(subnet, result));
            Assert.All(result, subnet => Assert.Contains(subnet, expectedList));
            Assert.True(result.SequenceEqual(new SortedSet<Subnet>(result, new DefaultIIPAddressRangeComparer())));
        }

        /// <summary>Gets theory data for <see cref="FewestConsecutiveSubnetsFor_MismatchedAddressFamilies_ThrowsInvalidOperationException_Test"/>.</summary>
        /// <value>Parameters: left (IPAddress), right (IPAddress).</value>
        public static TheoryData<
            IPAddress,
            IPAddress
        > FewestConsecutiveSubnetsFor_MismatchedAddressFamilies_ThrowsInvalidOperationException_Test_Data =>
            new() { { IPAddress.Any, IPAddress.IPv6Any }, { IPAddress.IPv6Any, IPAddress.Any } };

        /// <summary>Verifies that <see cref="SubnetUtilities.FewestConsecutiveSubnetsFor"/> throws <see cref="InvalidOperationException"/> when the two addresses belong to different address families.</summary>
        /// <param name="left">Left bound address.</param>
        /// <param name="right">Right bound address.</param>
        [Theory]
        [MemberData(nameof(FewestConsecutiveSubnetsFor_MismatchedAddressFamilies_ThrowsInvalidOperationException_Test_Data))]
        public void FewestConsecutiveSubnetsFor_MismatchedAddressFamilies_ThrowsInvalidOperationException_Test(
            IPAddress left,
            IPAddress right
        )
        {
            // Arrange
            // Act
            // Assert
            Assert.Throws<InvalidOperationException>(() => SubnetUtilities.FewestConsecutiveSubnetsFor(left, right));
        }

        /// <summary>Gets theory data for <see cref="FewestConsecutiveSubnetsFor_NullArgument_ThrowsArgumentNullException_Test"/>.</summary>
        /// <value>Parameters: left (IPAddress), right (IPAddress).</value>
        public static TheoryData<
            IPAddress,
            IPAddress
        > FewestConsecutiveSubnetsFor_NullArgument_ThrowsArgumentNullException_Test_Data =>
            new()
            {
                { null, null },
                { IPAddress.Any, null },
                { null, IPAddress.Any },
            };

        private static readonly string[] SourceArray =
        [
            "128.64.20.3/32",
            "128.64.20.4/30",
            "128.64.20.8/30",
            "128.64.20.12/32",
        ];

        private static readonly string[] SmallIPv4Range = ["192.168.1.3/32", "192.168.1.4/31"];

        private static readonly string[] ComplexIPv6Range =
        [
            "2001:400:4402::ffff/128",
            "2001:400:4402::1:0/112",
            "2001:400:4402::2:0/111",
            "2001:400:4402::4:0/110",
            "2001:400:4402::8:0/109",
            "2001:400:4402::10:0/108",
            "2001:400:4402::20:0/107",
            "2001:400:4402::40:0/106",
            "2001:400:4402::80:0/105",
            "2001:400:4402::100:0/104",
            "2001:400:4402::200:0/103",
            "2001:400:4402::400:0/102",
            "2001:400:4402::800:0/101",
            "2001:400:4402::1000:0/100",
            "2001:400:4402::2000:0/99",
            "2001:400:4402::4000:0/98",
            "2001:400:4402::8000:0/97",
            "2001:400:4402::1:0:0/96",
            "2001:400:4402::2:0:0/95",
            "2001:400:4402::4:0:0/94",
            "2001:400:4402::8:0:0/93",
            "2001:400:4402::10:0:0/92",
            "2001:400:4402::20:0:0/91",
            "2001:400:4402::40:0:0/90",
            "2001:400:4402::80:0:0/89",
            "2001:400:4402::100:0:0/88",
            "2001:400:4402::200:0:0/87",
            "2001:400:4402::400:0:0/86",
            "2001:400:4402::800:0:0/85",
            "2001:400:4402::1000:0:0/84",
            "2001:400:4402::2000:0:0/83",
            "2001:400:4402::4000:0:0/82",
            "2001:400:4402::8000:0:0/81",
            "2001:400:4402:0:1::/80",
            "2001:400:4402:0:2::/79",
            "2001:400:4402:0:4::/78",
            "2001:400:4402:0:8::/77",
            "2001:400:4402:0:10::/76",
            "2001:400:4402:0:20::/75",
            "2001:400:4402:0:40::/74",
            "2001:400:4402:0:80::/73",
            "2001:400:4402:0:100::/72",
            "2001:400:4402:0:200::/71",
            "2001:400:4402:0:400::/70",
            "2001:400:4402:0:800::/69",
            "2001:400:4402:0:1000::/68",
            "2001:400:4402:0:2000::/67",
            "2001:400:4402:0:4000::/66",
            "2001:400:4402:0:8000::/65",
            "2001:400:4402:1::/64",
            "2001:400:4402:2::/63",
            "2001:400:4402:4::/62",
            "2001:400:4402:8::/61",
            "2001:400:4402:10::/60",
            "2001:400:4402:20::/59",
            "2001:400:4402:40::/58",
            "2001:400:4402:80::/57",
            "2001:400:4402:100::/56",
            "2001:400:4402:200::/55",
            "2001:400:4402:400::/54",
            "2001:400:4402:800::/53",
            "2001:400:4402:1000::/52",
            "2001:400:4402:2000::/51",
            "2001:400:4402:4000::/50",
            "2001:400:4402:8000::/49",
        ];

        private static readonly string[] NearFullIPv4Range =
        [
            "0.0.0.1/32",
            "0.0.0.2/31",
            "0.0.0.4/30",
            "0.0.0.8/29",
            "0.0.0.16/28",
            "0.0.0.32/27",
            "0.0.0.64/26",
            "0.0.0.128/25",
            "0.0.1.0/24",
            "0.0.2.0/23",
            "0.0.4.0/22",
            "0.0.8.0/21",
            "0.0.16.0/20",
            "0.0.32.0/19",
            "0.0.64.0/18",
            "0.0.128.0/17",
            "0.1.0.0/16",
            "0.2.0.0/15",
            "0.4.0.0/14",
            "0.8.0.0/13",
            "0.16.0.0/12",
            "0.32.0.0/11",
            "0.64.0.0/10",
            "0.128.0.0/9",
            "1.0.0.0/8",
            "2.0.0.0/7",
            "4.0.0.0/6",
            "8.0.0.0/5",
            "16.0.0.0/4",
            "32.0.0.0/3",
            "64.0.0.0/2",
            "128.0.0.0/2",
            "192.0.0.0/3",
            "224.0.0.0/4",
            "240.0.0.0/5",
            "248.0.0.0/6",
            "252.0.0.0/7",
            "254.0.0.0/8",
            "255.0.0.0/9",
            "255.128.0.0/10",
            "255.192.0.0/11",
            "255.224.0.0/12",
            "255.240.0.0/13",
            "255.248.0.0/14",
            "255.252.0.0/15",
            "255.254.0.0/16",
            "255.255.0.0/17",
            "255.255.128.0/18",
            "255.255.192.0/19",
            "255.255.224.0/20",
            "255.255.240.0/21",
            "255.255.248.0/22",
            "255.255.252.0/23",
            "255.255.254.0/24",
            "255.255.255.0/25",
            "255.255.255.128/26",
            "255.255.255.192/27",
            "255.255.255.224/28",
            "255.255.255.240/29",
            "255.255.255.248/30",
            "255.255.255.252/31",
            "255.255.255.254/32",
        ];

        private static readonly string[] NearFullIPv6Range =
        [
            "::1/128",
            "::2/127",
            "::4/126",
            "::8/125",
            "::10/124",
            "::20/123",
            "::40/122",
            "::80/121",
            "::100/120",
            "::200/119",
            "::400/118",
            "::800/117",
            "::1000/116",
            "::2000/115",
            "::4000/114",
            "::8000/113",
            "::0.1.0.0/112",
            "::0.2.0.0/111",
            "::0.4.0.0/110",
            "::0.8.0.0/109",
            "::0.16.0.0/108",
            "::0.32.0.0/107",
            "::0.64.0.0/106",
            "::0.128.0.0/105",
            "::1.0.0.0/104",
            "::2.0.0.0/103",
            "::4.0.0.0/102",
            "::8.0.0.0/101",
            "::16.0.0.0/100",
            "::32.0.0.0/99",
            "::64.0.0.0/98",
            "::128.0.0.0/97",
            "::1:0:0/96",
            "::2:0:0/95",
            "::4:0:0/94",
            "::8:0:0/93",
            "::10:0:0/92",
            "::20:0:0/91",
            "::40:0:0/90",
            "::80:0:0/89",
            "::100:0:0/88",
            "::200:0:0/87",
            "::400:0:0/86",
            "::800:0:0/85",
            "::1000:0:0/84",
            "::2000:0:0/83",
            "::4000:0:0/82",
            "::8000:0:0/81",
            "::1:0:0:0/80",
            "::2:0:0:0/79",
            "::4:0:0:0/78",
            "::8:0:0:0/77",
            "::10:0:0:0/76",
            "::20:0:0:0/75",
            "::40:0:0:0/74",
            "::80:0:0:0/73",
            "::100:0:0:0/72",
            "::200:0:0:0/71",
            "::400:0:0:0/70",
            "::800:0:0:0/69",
            "::1000:0:0:0/68",
            "::2000:0:0:0/67",
            "::4000:0:0:0/66",
            "::8000:0:0:0/65",
            "0:0:0:1::/64",
            "0:0:0:2::/63",
            "0:0:0:4::/62",
            "0:0:0:8::/61",
            "0:0:0:10::/60",
            "0:0:0:20::/59",
            "0:0:0:40::/58",
            "0:0:0:80::/57",
            "0:0:0:100::/56",
            "0:0:0:200::/55",
            "0:0:0:400::/54",
            "0:0:0:800::/53",
            "0:0:0:1000::/52",
            "0:0:0:2000::/51",
            "0:0:0:4000::/50",
            "0:0:0:8000::/49",
            "0:0:1::/48",
            "0:0:2::/47",
            "0:0:4::/46",
            "0:0:8::/45",
            "0:0:10::/44",
            "0:0:20::/43",
            "0:0:40::/42",
            "0:0:80::/41",
            "0:0:100::/40",
            "0:0:200::/39",
            "0:0:400::/38",
            "0:0:800::/37",
            "0:0:1000::/36",
            "0:0:2000::/35",
            "0:0:4000::/34",
            "0:0:8000::/33",
            "0:1::/32",
            "0:2::/31",
            "0:4::/30",
            "0:8::/29",
            "0:10::/28",
            "0:20::/27",
            "0:40::/26",
            "0:80::/25",
            "0:100::/24",
            "0:200::/23",
            "0:400::/22",
            "0:800::/21",
            "0:1000::/20",
            "0:2000::/19",
            "0:4000::/18",
            "0:8000::/17",
            "1::/16",
            "2::/15",
            "4::/14",
            "8::/13",
            "10::/12",
            "20::/11",
            "40::/10",
            "80::/9",
            "100::/8",
            "200::/7",
            "400::/6",
            "800::/5",
            "1000::/4",
            "2000::/3",
            "4000::/2",
            "8000::/2",
            "c000::/3",
            "e000::/4",
            "f000::/5",
            "f800::/6",
            "fc00::/7",
            "fe00::/8",
            "ff00::/9",
            "ff80::/10",
            "ffc0::/11",
            "ffe0::/12",
            "fff0::/13",
            "fff8::/14",
            "fffc::/15",
            "fffe::/16",
            "ffff::/17",
            "ffff:8000::/18",
            "ffff:c000::/19",
            "ffff:e000::/20",
            "ffff:f000::/21",
            "ffff:f800::/22",
            "ffff:fc00::/23",
            "ffff:fe00::/24",
            "ffff:ff00::/25",
            "ffff:ff80::/26",
            "ffff:ffc0::/27",
            "ffff:ffe0::/28",
            "ffff:fff0::/29",
            "ffff:fff8::/30",
            "ffff:fffc::/31",
            "ffff:fffe::/32",
            "ffff:ffff::/33",
            "ffff:ffff:8000::/34",
            "ffff:ffff:c000::/35",
            "ffff:ffff:e000::/36",
            "ffff:ffff:f000::/37",
            "ffff:ffff:f800::/38",
            "ffff:ffff:fc00::/39",
            "ffff:ffff:fe00::/40",
            "ffff:ffff:ff00::/41",
            "ffff:ffff:ff80::/42",
            "ffff:ffff:ffc0::/43",
            "ffff:ffff:ffe0::/44",
            "ffff:ffff:fff0::/45",
            "ffff:ffff:fff8::/46",
            "ffff:ffff:fffc::/47",
            "ffff:ffff:fffe::/48",
            "ffff:ffff:ffff::/49",
            "ffff:ffff:ffff:8000::/50",
            "ffff:ffff:ffff:c000::/51",
            "ffff:ffff:ffff:e000::/52",
            "ffff:ffff:ffff:f000::/53",
            "ffff:ffff:ffff:f800::/54",
            "ffff:ffff:ffff:fc00::/55",
            "ffff:ffff:ffff:fe00::/56",
            "ffff:ffff:ffff:ff00::/57",
            "ffff:ffff:ffff:ff80::/58",
            "ffff:ffff:ffff:ffc0::/59",
            "ffff:ffff:ffff:ffe0::/60",
            "ffff:ffff:ffff:fff0::/61",
            "ffff:ffff:ffff:fff8::/62",
            "ffff:ffff:ffff:fffc::/63",
            "ffff:ffff:ffff:fffe::/64",
            "ffff:ffff:ffff:ffff::/65",
            "ffff:ffff:ffff:ffff:8000::/66",
            "ffff:ffff:ffff:ffff:c000::/67",
            "ffff:ffff:ffff:ffff:e000::/68",
            "ffff:ffff:ffff:ffff:f000::/69",
            "ffff:ffff:ffff:ffff:f800::/70",
            "ffff:ffff:ffff:ffff:fc00::/71",
            "ffff:ffff:ffff:ffff:fe00::/72",
            "ffff:ffff:ffff:ffff:ff00::/73",
            "ffff:ffff:ffff:ffff:ff80::/74",
            "ffff:ffff:ffff:ffff:ffc0::/75",
            "ffff:ffff:ffff:ffff:ffe0::/76",
            "ffff:ffff:ffff:ffff:fff0::/77",
            "ffff:ffff:ffff:ffff:fff8::/78",
            "ffff:ffff:ffff:ffff:fffc::/79",
            "ffff:ffff:ffff:ffff:fffe::/80",
            "ffff:ffff:ffff:ffff:ffff::/81",
            "ffff:ffff:ffff:ffff:ffff:8000::/82",
            "ffff:ffff:ffff:ffff:ffff:c000::/83",
            "ffff:ffff:ffff:ffff:ffff:e000::/84",
            "ffff:ffff:ffff:ffff:ffff:f000::/85",
            "ffff:ffff:ffff:ffff:ffff:f800::/86",
            "ffff:ffff:ffff:ffff:ffff:fc00::/87",
            "ffff:ffff:ffff:ffff:ffff:fe00::/88",
            "ffff:ffff:ffff:ffff:ffff:ff00::/89",
            "ffff:ffff:ffff:ffff:ffff:ff80::/90",
            "ffff:ffff:ffff:ffff:ffff:ffc0::/91",
            "ffff:ffff:ffff:ffff:ffff:ffe0::/92",
            "ffff:ffff:ffff:ffff:ffff:fff0::/93",
            "ffff:ffff:ffff:ffff:ffff:fff8::/94",
            "ffff:ffff:ffff:ffff:ffff:fffc::/95",
            "ffff:ffff:ffff:ffff:ffff:fffe::/96",
            "ffff:ffff:ffff:ffff:ffff:ffff::/97",
            "ffff:ffff:ffff:ffff:ffff:ffff:8000:0/98",
            "ffff:ffff:ffff:ffff:ffff:ffff:c000:0/99",
            "ffff:ffff:ffff:ffff:ffff:ffff:e000:0/100",
            "ffff:ffff:ffff:ffff:ffff:ffff:f000:0/101",
            "ffff:ffff:ffff:ffff:ffff:ffff:f800:0/102",
            "ffff:ffff:ffff:ffff:ffff:ffff:fc00:0/103",
            "ffff:ffff:ffff:ffff:ffff:ffff:fe00:0/104",
            "ffff:ffff:ffff:ffff:ffff:ffff:ff00:0/105",
            "ffff:ffff:ffff:ffff:ffff:ffff:ff80:0/106",
            "ffff:ffff:ffff:ffff:ffff:ffff:ffc0:0/107",
            "ffff:ffff:ffff:ffff:ffff:ffff:ffe0:0/108",
            "ffff:ffff:ffff:ffff:ffff:ffff:fff0:0/109",
            "ffff:ffff:ffff:ffff:ffff:ffff:fff8:0/110",
            "ffff:ffff:ffff:ffff:ffff:ffff:fffc:0/111",
            "ffff:ffff:ffff:ffff:ffff:ffff:fffe:0/112",
            "ffff:ffff:ffff:ffff:ffff:ffff:ffff:0/113",
            "ffff:ffff:ffff:ffff:ffff:ffff:ffff:8000/114",
            "ffff:ffff:ffff:ffff:ffff:ffff:ffff:c000/115",
            "ffff:ffff:ffff:ffff:ffff:ffff:ffff:e000/116",
            "ffff:ffff:ffff:ffff:ffff:ffff:ffff:f000/117",
            "ffff:ffff:ffff:ffff:ffff:ffff:ffff:f800/118",
            "ffff:ffff:ffff:ffff:ffff:ffff:ffff:fc00/119",
            "ffff:ffff:ffff:ffff:ffff:ffff:ffff:fe00/120",
            "ffff:ffff:ffff:ffff:ffff:ffff:ffff:ff00/121",
            "ffff:ffff:ffff:ffff:ffff:ffff:ffff:ff80/122",
            "ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffc0/123",
            "ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffe0/124",
            "ffff:ffff:ffff:ffff:ffff:ffff:ffff:fff0/125",
            "ffff:ffff:ffff:ffff:ffff:ffff:ffff:fff8/126",
            "ffff:ffff:ffff:ffff:ffff:ffff:ffff:fffc/127",
            "ffff:ffff:ffff:ffff:ffff:ffff:ffff:fffe/128",
        ];

        /// <summary>Verifies that <see cref="SubnetUtilities.FewestConsecutiveSubnetsFor"/> throws <see cref="ArgumentNullException"/> when either argument is null.</summary>
        /// <param name="left">Left bound address.</param>
        /// <param name="right">Right bound address.</param>
        [Theory]
        [MemberData(nameof(FewestConsecutiveSubnetsFor_NullArgument_ThrowsArgumentNullException_Test_Data))]
        public void FewestConsecutiveSubnetsFor_NullArgument_ThrowsArgumentNullException_Test(IPAddress left, IPAddress right)
        {
            // Arrange
            // Act
            // Assert
            Assert.Throws<ArgumentNullException>(() => SubnetUtilities.FewestConsecutiveSubnetsFor(left, right));
        }

        /// <summary>
        ///     Verifies that <see cref="SubnetUtilities.FewestConsecutiveSubnetsFor"/> correctly decomposes
        ///     a range ending at the IPv4 maximum address (255.255.255.255).
        /// </summary>
        [Fact]
        public void FewestConsecutiveSubnetsFor_RangeEndingAtIPv4Max_ReturnsExpectedSubnets_Test()
        {
            // Arrange
            var left = IPAddress.Parse("255.255.255.253");
            var right = IPAddress.Parse("255.255.255.255");

            // Act
            var results = SubnetUtilities.FewestConsecutiveSubnetsFor(left, right).ToArray();

            // Assert — correct decomposition:
            //   255.255.255.253/32 (one address: 255.255.255.253)
            //   255.255.255.254/31 (two addresses: 255.255.255.254-255.255.255.255)
            Assert.Collection(
                results,
                subnet => Assert.Equal(new Subnet(IPAddress.Parse("255.255.255.253"), 32), subnet),
                subnet => Assert.Equal(new Subnet(IPAddress.Parse("255.255.255.254"), 31), subnet)
            );
        }

        /// <summary>
        ///     Sentinel for the C2 perf refactor (recursive <c>Concat</c> -> <c>yield return</c>):
        ///     enumerating an asymmetric IPv4 range that requires many splits must produce the same
        ///     count and a contiguous, gap-free, overlap-free cover of the closed interval
        ///     [10.0.0.1, 10.1.255.254]. The golden count (32) was recorded against the pre-refactor
        ///     implementation and must not change. No timing assertion (flaky in CI); the linear-time
        ///     property is established by code inspection and benchmarks.
        /// </summary>
        [Fact]
        public void FewestConsecutiveSubnetsFor_AsymmetricManySplits_ReturnsExpectedCount_Test()
        {
            // Arrange
            var left = IPAddress.Parse("10.0.0.1");
            var right = IPAddress.Parse("10.1.255.254");

            // Act
            var subnets = SubnetUtilities.FewestConsecutiveSubnetsFor(left, right).ToList();

            // Assert — golden count recorded against the pre-refactor (Concat) implementation.
            const int expectedCount = 32;
            Assert.Equal(expectedCount, subnets.Count);

            AssertContiguousCover(subnets, left, right);
        }

        /// <summary>
        ///     Regression sentinel for the C2 refactor: a large bounded IPv4 range
        ///     (<c>0.0.0.1</c> -> <c>255.255.255.254</c>) must yield the golden count (62)
        ///     recorded against the pre-refactor (Concat) implementation. Verifies count and
        ///     contiguous coverage so the Concat -> yield-return change cannot alter behavior.
        /// </summary>
        [Fact]
        public void FewestConsecutiveSubnetsFor_LargeBoundedRange_ReturnsExpectedCount_Test()
        {
            // Arrange
            var left = IPAddress.Parse("0.0.0.1");
            var right = IPAddress.Parse("255.255.255.254");

            // Act
            var subnets = SubnetUtilities.FewestConsecutiveSubnetsFor(left, right).ToList();

            // Assert — golden count recorded against the pre-refactor (Concat) implementation.
            const int expectedCount = 62;
            Assert.Equal(expectedCount, subnets.Count);

            AssertContiguousCover(subnets, left, right);
        }

        /// <summary>
        ///     Asserts that <paramref name="subnets"/> forms an ascending, gap-free, overlap-free
        ///     cover of the closed interval [<paramref name="left"/>, <paramref name="right"/>].
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         The C2 refactor (Concat -> yield return) is a pure performance change; this
        ///         helper codifies the behavioral contract that must not regress: ordered by
        ///         <see cref="Subnet.NetworkPrefixAddress"/>, the head of each subnet equals the
        ///         tail of its predecessor plus one, the first head equals the lower bound, and
        ///         the last tail equals the upper bound.
        ///     </para>
        /// </remarks>
        private static void AssertContiguousCover(List<Subnet> subnets, IPAddress left, IPAddress right)
        {
            Assert.NotEmpty(subnets);
            Assert.All(subnets, Assert.NotNull);

            var lowerBound = IPAddressMath.Min(left, right);
            var upperBound = IPAddressMath.Max(left, right);

            // First head equals the lower bound; last tail equals the upper bound.
            Assert.Equal(lowerBound, subnets[0].Head);
            Assert.Equal(upperBound, subnets[subnets.Count - 1].Tail);

            // Sorted ascending by NetworkPrefixAddress, gap-free, no overlap.
            for (var i = 0; i < subnets.Count; i++)
            {
                Assert.True(
                    subnets[i].Tail.IsGreaterThanOrEqualTo(subnets[i].Head),
                    $"subnet {i} tail {subnets[i].Tail} is below its head {subnets[i].Head}"
                );
                Assert.True(
                    subnets[i].Head.IsGreaterThanOrEqualTo(lowerBound),
                    $"subnet {i} head {subnets[i].Head} is below the range lower bound {lowerBound}"
                );
                Assert.True(
                    subnets[i].Tail.IsLessThanOrEqualTo(upperBound),
                    $"subnet {i} tail {subnets[i].Tail} exceeds range upper bound {upperBound}"
                );

                // gap-free: next head = this tail + 1
                if (i < subnets.Count - 1)
                {
                    Assert.True(
                        IPAddressMath.TryIncrement(subnets[i].Tail, out var nextHead),
                        $"overflow incrementing tail {subnets[i].Tail} at index {i}"
                    );
                    Assert.Equal(nextHead, subnets[i + 1].Head);
                }
            }
        }

        #endregion // end: FewestConsecutiveSubnetsFor

        #region LargestSubnet

        /// <summary>Verifies that <see cref="SubnetUtilities.LargestSubnet"/> returns one of the tied-largest subnets when multiple share the largest size.</summary>
        [Fact]
        public void LargestSubnet_Ambiguous_ReturnsOneOfLargest_Test()
        {
            // Arrange
            var subnets = new[]
            {
                new Subnet(IPAddress.Any, 24),
                new Subnet(IPAddress.Any, 16),
                new Subnet(IPAddress.Any, 16),
                new Subnet(IPAddress.Any, 32),
            };

            // Act
            var result = SubnetUtilities.LargestSubnet(subnets);

            // Assert
            Assert.Equal(new Subnet(IPAddress.Any, 16), result);
        }

        /// <summary>Verifies that <see cref="SubnetUtilities.LargestSubnet"/> throws <see cref="InvalidOperationException"/> when given an empty sequence.</summary>
        [Fact]
        public void LargestSubnet_EmptyInput_ThrowsInvalidOperationException_Test()
        {
            // Arrange
            var subnets = Enumerable.Empty<Subnet>();

            // Act
            // Assert
            Assert.Throws<InvalidOperationException>(() => SubnetUtilities.LargestSubnet(subnets));
        }

        /// <summary>Verifies that <see cref="SubnetUtilities.LargestSubnet"/> throws <see cref="ArgumentNullException"/> when given a null input.</summary>
        [Fact]
        public void LargestSubnet_NullInput_ThrowsArgumentNullException_Test()
        {
            // Arrange
            // Act
            // Assert
            Assert.Throws<ArgumentNullException>(() => SubnetUtilities.LargestSubnet(null));
        }

        /// <summary>Verifies that <see cref="SubnetUtilities.LargestSubnet"/> throws <see cref="ArgumentException"/> when the collection contains null elements.</summary>
        [Fact]
        public void LargestSubnet_NullElements_ThrowsArgumentException_Test()
        {
            // Arrange
            var subnets = new Subnet[] { null, null, null };

            // Act
            // Assert
            Assert.Throws<ArgumentException>(() => SubnetUtilities.LargestSubnet(subnets));
        }

        /// <summary>Verifies that <see cref="SubnetUtilities.LargestSubnet"/> throws <see cref="ArgumentException"/> when a null element appears mid-sequence (not as the first element).</summary>
        [Fact]
        public void LargestSubnet_NullElementMidSequence_ThrowsArgumentException_Test()
        {
            // Arrange: the first element is valid, so the initial current check passes;
            // the null appears mid-iteration, exercising the loop-level guard.
            var subnets = new Subnet[] { Subnet.Parse("10.0.0.0/24"), null, Subnet.Parse("192.168.0.0/16") };

            // Act / Assert
            Assert.Throws<ArgumentException>(() => SubnetUtilities.LargestSubnet(subnets));
        }

        /// <summary>Verifies that <see cref="SubnetUtilities.LargestSubnet"/> returns the single element when the input contains exactly one subnet.</summary>
        [Fact]
        public void LargestSubnet_Single_ReturnsSingle_Test()
        {
            // Arrange
            var expected = new Subnet(IPAddress.Any, 16);
            var subnets = new[] { expected };

            // Act
            var result = SubnetUtilities.LargestSubnet(subnets);

            // Assert
            Assert.Same(expected, result);
        }

        /// <summary>Verifies that <see cref="SubnetUtilities.LargestSubnet"/> returns the largest subnet from a collection of mixed sizes.</summary>
        [Fact]
        public void LargestSubnet_ReturnsLargest_Test()
        {
            // Arrange
            var expected = new Subnet(IPAddress.Any, 16);
            var subnets = new[] { expected, new Subnet(IPAddress.Any, 24), new Subnet(IPAddress.Any, 32) };

            // Act
            var result = SubnetUtilities.LargestSubnet(subnets);

            // Assert
            Assert.Same(expected, result);
        }

        #endregion // end: LargestSubnet

        #region SmallestSubnet

        /// <summary>Verifies that <see cref="SubnetUtilities.SmallestSubnet"/> returns one of the tied-smallest subnets when multiple share the smallest size.</summary>
        [Fact]
        public void SmallestSubnet_Ambiguous_ReturnsOneOfSmallest_Test()
        {
            // Arrange
            var subnets = new[]
            {
                new Subnet(IPAddress.Any, 24),
                new Subnet(IPAddress.Any, 32),
                new Subnet(IPAddress.Any, 32),
                new Subnet(IPAddress.Any, 16),
            };

            // Act
            var result = SubnetUtilities.SmallestSubnet(subnets);

            // Assert
            Assert.Equal(new Subnet(IPAddress.Any, 32), result);
        }

        /// <summary>Verifies that <see cref="SubnetUtilities.SmallestSubnet"/> throws <see cref="InvalidOperationException"/> when given an empty sequence.</summary>
        [Fact]
        public void SmallestSubnet_EmptyInput_ThrowsInvalidOperationException_Test()
        {
            // Arrange
            var subnets = Enumerable.Empty<Subnet>();

            // Act
            // Assert
            Assert.Throws<InvalidOperationException>(() => SubnetUtilities.SmallestSubnet(subnets));
        }

        /// <summary>Verifies that <see cref="SubnetUtilities.SmallestSubnet"/> throws <see cref="ArgumentNullException"/> when given a null input.</summary>
        [Fact]
        public void SmallestSubnet_NullInput_ThrowsArgumentNullException_Test()
        {
            // Arrange
            // Act
            // Assert
            Assert.Throws<ArgumentNullException>(() => SubnetUtilities.SmallestSubnet(null));
        }

        /// <summary>Verifies that <see cref="SubnetUtilities.SmallestSubnet"/> throws <see cref="ArgumentException"/> when the collection contains null elements.</summary>
        [Fact]
        public void SmallestSubnet_NullElements_ThrowsArgumentException_Test()
        {
            // Arrange
            var subnets = new Subnet[] { null, null, null };

            // Act
            // Assert
            Assert.Throws<ArgumentException>(() => SubnetUtilities.SmallestSubnet(subnets));
        }

        /// <summary>Verifies that <see cref="SubnetUtilities.SmallestSubnet"/> returns the single element when the input contains exactly one subnet.</summary>
        [Fact]
        public void SmallestSubnet_Single_ReturnsSingle_Test()
        {
            // Arrange
            var expected = new Subnet(IPAddress.Any, 16);
            var subnets = new[] { expected };

            // Act
            var result = SubnetUtilities.SmallestSubnet(subnets);

            // Assert
            Assert.Same(expected, result);
        }

        /// <summary>Verifies that <see cref="SubnetUtilities.SmallestSubnet"/> returns the smallest subnet from a collection of mixed sizes.</summary>
        [Fact]
        public void SmallestSubnet_ReturnsSmallest_Test()
        {
            // Arrange
            var expected = new Subnet(IPAddress.Any, 32);
            var subnets = new[] { expected, new Subnet(IPAddress.Any, 24), new Subnet(IPAddress.Any, 16) };

            // Act
            var result = SubnetUtilities.SmallestSubnet(subnets);

            // Assert
            Assert.Same(expected, result);
        }

        #endregion // end: SmallestSubnet

        #region maxEnumerationExponent propagation

        /// <summary>Verifies that <see cref="SubnetUtilities.FewestConsecutiveSubnetsFor(IPAddress, IPAddress, int)"/> propagates maxEnumerationExponent.</summary>
        [Fact]
        public void FewestConsecutiveSubnetsFor_ExponentPropagates_Test()
        {
            var results = SubnetUtilities.FewestConsecutiveSubnetsFor(
                IPAddress.Parse("10.0.0.0"),
                IPAddress.Parse("10.0.0.255"),
                maxEnumerationExponent: 4
            );
            foreach (var subnet in results)
            {
                Assert.Equal(4, subnet.MaxEnumerationExponent);
            }
        }

        #endregion // end: maxEnumerationExponent propagation
    }
}
