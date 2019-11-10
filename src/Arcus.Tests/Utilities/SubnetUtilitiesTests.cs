using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Arcus.Comparers;
using Arcus.Utilities;
using Xunit;

namespace Arcus.Tests.Utilities
{
#pragma warning disable SA1404
    public class SubnetUtilitiesTests
    {
        #region PrivateIPAddressRangesList

        [Fact]
        public void PrivateIPAddressRangesList_Test()
        {
            // Arrange
            var expectedSubnets = new[]
                                  {
                                      Subnet.Parse("10.0.0.0", 8),
                                      Subnet.Parse("172.16.0.0", 12),
                                      Subnet.Parse("192.168.0.0", 16),
                                      Subnet.Parse("fd00::", 8)
                                  };

            // Act
            var list = SubnetUtilities.PrivateIPAddressRangesList;

            // Assert
            Assert.IsAssignableFrom<IReadOnlyList<Subnet>>(list);
            Assert.Equal(4, list.Count);
            Assert.Equal(list.Count,
                         list.Distinct()
                             .Count());

            Assert.Contains(list, s => s.IsIPv4);
            Assert.Contains(list, s => s.IsIPv6);

            Assert.All(list,
                       subnet =>
                       {
                           Assert.NotNull(subnet);
                           Assert.Contains(subnet,
                                           expectedSubnets);
                       });
        }

        #endregion end: PrivateIPAddressRangesList

        #region LinkLocalIPAddressRangesList

        [Fact]
        public void LinkLocalIPAddressRangesList_Test()
        {
            // Arrange
            var expectedSubnets = new[]
                                  {
                                      Subnet.Parse("169.254.0.0", 16),
                                      Subnet.Parse("fe80::", 10)
                                  };

            // Act
            var list = SubnetUtilities.LinkLocalIPAddressRangesList;

            // Assert
            Assert.IsAssignableFrom<IReadOnlyList<Subnet>>(list);
            Assert.Equal(2, list.Count);
            Assert.Equal(list.Count,
                         list.Distinct()
                             .Count());

            Assert.Contains(list, s => s.IsIPv4);
            Assert.Contains(list, s => s.IsIPv6);

            Assert.All(list,
                       subnet =>
                       {
                           Assert.NotNull(subnet);
                           Assert.Contains(subnet,
                                           expectedSubnets);
                       });
        }

        #endregion end: LinkLocalIPAddressRangesList

        #region FewestConsecutiveSubnetsFor

        public static IEnumerable<object[]> FewestConsecutiveSubnetsFor_Test_Values()
        {
            yield return new object[]
                         {
                             new[] {Subnet.Parse("128.64.20.3/32")},
                             IPAddress.Parse("128.64.20.3"), IPAddress.Parse("128.64.20.3")
                         };

            yield return new object[]
                         {
                             new[] {"128.64.20.3/32", "128.64.20.4/30", "128.64.20.8/30", "128.64.20.12/32"}.Select(s => Subnet.Parse(s)),
                             IPAddress.Parse("128.64.20.3"), IPAddress.Parse("128.64.20.12")
                         };

            yield return new object[]
                         {
                             new[] {"128.64.20.3/32", "128.64.20.4/30", "128.64.20.8/30", "128.64.20.12/32"}.Select(s => Subnet.Parse(s)),
                             IPAddress.Parse("128.64.20.12"), IPAddress.Parse("128.64.20.3")
                         };

            yield return new object[]
                         {
                             new[] {"192.168.1.3/32", "192.168.1.4/31"}.Select(s => Subnet.Parse(s)),
                             IPAddress.Parse("192.168.1.3"), IPAddress.Parse("192.168.1.5")
                         };

            yield return new object[]
                         {
                             new[] {Subnet.Parse("2001:400:4402::/128")}, IPAddress.Parse("2001:400:4402::"), IPAddress.Parse("2001:400:4402::")
                         };

            yield return new object[]
                         {
                             new[] {Subnet.Parse("2001:400:4402::/48")}, IPAddress.Parse("2001:400:4402::"), IPAddress.Parse("2001:400:4402:ffff:ffff:ffff:ffff:ffff")
                         };

            yield return new object[]
                         {
                             new[] {"2001:400:4402::ffff/128", "2001:400:4402::1:0/112", "2001:400:4402::2:0/111", "2001:400:4402::4:0/110", "2001:400:4402::8:0/109", "2001:400:4402::10:0/108", "2001:400:4402::20:0/107", "2001:400:4402::40:0/106", "2001:400:4402::80:0/105", "2001:400:4402::100:0/104", "2001:400:4402::200:0/103", "2001:400:4402::400:0/102", "2001:400:4402::800:0/101", "2001:400:4402::1000:0/100", "2001:400:4402::2000:0/99", "2001:400:4402::4000:0/98", "2001:400:4402::8000:0/97", "2001:400:4402::1:0:0/96", "2001:400:4402::2:0:0/95", "2001:400:4402::4:0:0/94", "2001:400:4402::8:0:0/93", "2001:400:4402::10:0:0/92", "2001:400:4402::20:0:0/91", "2001:400:4402::40:0:0/90", "2001:400:4402::80:0:0/89", "2001:400:4402::100:0:0/88", "2001:400:4402::200:0:0/87", "2001:400:4402::400:0:0/86", "2001:400:4402::800:0:0/85", "2001:400:4402::1000:0:0/84", "2001:400:4402::2000:0:0/83", "2001:400:4402::4000:0:0/82", "2001:400:4402::8000:0:0/81", "2001:400:4402:0:1::/80", "2001:400:4402:0:2::/79", "2001:400:4402:0:4::/78", "2001:400:4402:0:8::/77", "2001:400:4402:0:10::/76", "2001:400:4402:0:20::/75", "2001:400:4402:0:40::/74", "2001:400:4402:0:80::/73", "2001:400:4402:0:100::/72", "2001:400:4402:0:200::/71", "2001:400:4402:0:400::/70", "2001:400:4402:0:800::/69", "2001:400:4402:0:1000::/68", "2001:400:4402:0:2000::/67", "2001:400:4402:0:4000::/66", "2001:400:4402:0:8000::/65", "2001:400:4402:1::/64", "2001:400:4402:2::/63", "2001:400:4402:4::/62", "2001:400:4402:8::/61", "2001:400:4402:10::/60", "2001:400:4402:20::/59", "2001:400:4402:40::/58", "2001:400:4402:80::/57", "2001:400:4402:100::/56", "2001:400:4402:200::/55", "2001:400:4402:400::/54", "2001:400:4402:800::/53", "2001:400:4402:1000::/52", "2001:400:4402:2000::/51", "2001:400:4402:4000::/50", "2001:400:4402:8000::/49"}.Select(s => Subnet.Parse(s)),
                             IPAddress.Parse("2001:400:4402::ffff"), IPAddress.Parse("2001:400:4402:ffff:ffff:ffff:ffff:ffff")
                         };

            yield return new object[]
                         {
                             new[] {"0.0.0.1/32", "0.0.0.2/31", "0.0.0.4/30", "0.0.0.8/29", "0.0.0.16/28", "0.0.0.32/27", "0.0.0.64/26", "0.0.0.128/25", "0.0.1.0/24", "0.0.2.0/23", "0.0.4.0/22", "0.0.8.0/21", "0.0.16.0/20", "0.0.32.0/19", "0.0.64.0/18", "0.0.128.0/17", "0.1.0.0/16", "0.2.0.0/15", "0.4.0.0/14", "0.8.0.0/13", "0.16.0.0/12", "0.32.0.0/11", "0.64.0.0/10", "0.128.0.0/9", "1.0.0.0/8", "2.0.0.0/7", "4.0.0.0/6", "8.0.0.0/5", "16.0.0.0/4", "32.0.0.0/3", "64.0.0.0/2", "128.0.0.0/2", "192.0.0.0/3", "224.0.0.0/4", "240.0.0.0/5", "248.0.0.0/6", "252.0.0.0/7", "254.0.0.0/8", "255.0.0.0/9", "255.128.0.0/10", "255.192.0.0/11", "255.224.0.0/12", "255.240.0.0/13", "255.248.0.0/14", "255.252.0.0/15", "255.254.0.0/16", "255.255.0.0/17", "255.255.128.0/18", "255.255.192.0/19", "255.255.224.0/20", "255.255.240.0/21", "255.255.248.0/22", "255.255.252.0/23", "255.255.254.0/24", "255.255.255.0/25", "255.255.255.128/26", "255.255.255.192/27", "255.255.255.224/28", "255.255.255.240/29", "255.255.255.248/30", "255.255.255.252/31", "255.255.255.254/32"}.Select(s => Subnet.Parse(s)),
                             IPAddress.Parse("0.0.0.1"), IPAddress.Parse("255.255.255.254")
                         };

            yield return new object[]
                         {
                             new[] {"::1/128", "::2/127", "::4/126", "::8/125", "::10/124", "::20/123", "::40/122", "::80/121", "::100/120", "::200/119", "::400/118", "::800/117", "::1000/116", "::2000/115", "::4000/114", "::8000/113", "::0.1.0.0/112", "::0.2.0.0/111", "::0.4.0.0/110", "::0.8.0.0/109", "::0.16.0.0/108", "::0.32.0.0/107", "::0.64.0.0/106", "::0.128.0.0/105", "::1.0.0.0/104", "::2.0.0.0/103", "::4.0.0.0/102", "::8.0.0.0/101", "::16.0.0.0/100", "::32.0.0.0/99", "::64.0.0.0/98", "::128.0.0.0/97", "::1:0:0/96", "::2:0:0/95", "::4:0:0/94", "::8:0:0/93", "::10:0:0/92", "::20:0:0/91", "::40:0:0/90", "::80:0:0/89", "::100:0:0/88", "::200:0:0/87", "::400:0:0/86", "::800:0:0/85", "::1000:0:0/84", "::2000:0:0/83", "::4000:0:0/82", "::8000:0:0/81", "::1:0:0:0/80", "::2:0:0:0/79", "::4:0:0:0/78", "::8:0:0:0/77", "::10:0:0:0/76", "::20:0:0:0/75", "::40:0:0:0/74", "::80:0:0:0/73", "::100:0:0:0/72", "::200:0:0:0/71", "::400:0:0:0/70", "::800:0:0:0/69", "::1000:0:0:0/68", "::2000:0:0:0/67", "::4000:0:0:0/66", "::8000:0:0:0/65", "0:0:0:1::/64", "0:0:0:2::/63", "0:0:0:4::/62", "0:0:0:8::/61", "0:0:0:10::/60", "0:0:0:20::/59", "0:0:0:40::/58", "0:0:0:80::/57", "0:0:0:100::/56", "0:0:0:200::/55", "0:0:0:400::/54", "0:0:0:800::/53", "0:0:0:1000::/52", "0:0:0:2000::/51", "0:0:0:4000::/50", "0:0:0:8000::/49", "0:0:1::/48", "0:0:2::/47", "0:0:4::/46", "0:0:8::/45", "0:0:10::/44", "0:0:20::/43", "0:0:40::/42", "0:0:80::/41", "0:0:100::/40", "0:0:200::/39", "0:0:400::/38", "0:0:800::/37", "0:0:1000::/36", "0:0:2000::/35", "0:0:4000::/34", "0:0:8000::/33", "0:1::/32", "0:2::/31", "0:4::/30", "0:8::/29", "0:10::/28", "0:20::/27", "0:40::/26", "0:80::/25", "0:100::/24", "0:200::/23", "0:400::/22", "0:800::/21", "0:1000::/20", "0:2000::/19", "0:4000::/18", "0:8000::/17", "1::/16", "2::/15", "4::/14", "8::/13", "10::/12", "20::/11", "40::/10", "80::/9", "100::/8", "200::/7", "400::/6", "800::/5", "1000::/4", "2000::/3", "4000::/2", "8000::/2", "c000::/3", "e000::/4", "f000::/5", "f800::/6", "fc00::/7", "fe00::/8", "ff00::/9", "ff80::/10", "ffc0::/11", "ffe0::/12", "fff0::/13", "fff8::/14", "fffc::/15", "fffe::/16", "ffff::/17", "ffff:8000::/18", "ffff:c000::/19", "ffff:e000::/20", "ffff:f000::/21", "ffff:f800::/22", "ffff:fc00::/23", "ffff:fe00::/24", "ffff:ff00::/25", "ffff:ff80::/26", "ffff:ffc0::/27", "ffff:ffe0::/28", "ffff:fff0::/29", "ffff:fff8::/30", "ffff:fffc::/31", "ffff:fffe::/32", "ffff:ffff::/33", "ffff:ffff:8000::/34", "ffff:ffff:c000::/35", "ffff:ffff:e000::/36", "ffff:ffff:f000::/37", "ffff:ffff:f800::/38", "ffff:ffff:fc00::/39", "ffff:ffff:fe00::/40", "ffff:ffff:ff00::/41", "ffff:ffff:ff80::/42", "ffff:ffff:ffc0::/43", "ffff:ffff:ffe0::/44", "ffff:ffff:fff0::/45", "ffff:ffff:fff8::/46", "ffff:ffff:fffc::/47", "ffff:ffff:fffe::/48", "ffff:ffff:ffff::/49", "ffff:ffff:ffff:8000::/50", "ffff:ffff:ffff:c000::/51", "ffff:ffff:ffff:e000::/52", "ffff:ffff:ffff:f000::/53", "ffff:ffff:ffff:f800::/54", "ffff:ffff:ffff:fc00::/55", "ffff:ffff:ffff:fe00::/56", "ffff:ffff:ffff:ff00::/57", "ffff:ffff:ffff:ff80::/58", "ffff:ffff:ffff:ffc0::/59", "ffff:ffff:ffff:ffe0::/60", "ffff:ffff:ffff:fff0::/61", "ffff:ffff:ffff:fff8::/62", "ffff:ffff:ffff:fffc::/63", "ffff:ffff:ffff:fffe::/64", "ffff:ffff:ffff:ffff::/65", "ffff:ffff:ffff:ffff:8000::/66", "ffff:ffff:ffff:ffff:c000::/67", "ffff:ffff:ffff:ffff:e000::/68", "ffff:ffff:ffff:ffff:f000::/69", "ffff:ffff:ffff:ffff:f800::/70", "ffff:ffff:ffff:ffff:fc00::/71", "ffff:ffff:ffff:ffff:fe00::/72", "ffff:ffff:ffff:ffff:ff00::/73", "ffff:ffff:ffff:ffff:ff80::/74", "ffff:ffff:ffff:ffff:ffc0::/75", "ffff:ffff:ffff:ffff:ffe0::/76", "ffff:ffff:ffff:ffff:fff0::/77", "ffff:ffff:ffff:ffff:fff8::/78", "ffff:ffff:ffff:ffff:fffc::/79", "ffff:ffff:ffff:ffff:fffe::/80", "ffff:ffff:ffff:ffff:ffff::/81", "ffff:ffff:ffff:ffff:ffff:8000::/82", "ffff:ffff:ffff:ffff:ffff:c000::/83", "ffff:ffff:ffff:ffff:ffff:e000::/84", "ffff:ffff:ffff:ffff:ffff:f000::/85", "ffff:ffff:ffff:ffff:ffff:f800::/86", "ffff:ffff:ffff:ffff:ffff:fc00::/87", "ffff:ffff:ffff:ffff:ffff:fe00::/88", "ffff:ffff:ffff:ffff:ffff:ff00::/89", "ffff:ffff:ffff:ffff:ffff:ff80::/90", "ffff:ffff:ffff:ffff:ffff:ffc0::/91", "ffff:ffff:ffff:ffff:ffff:ffe0::/92", "ffff:ffff:ffff:ffff:ffff:fff0::/93", "ffff:ffff:ffff:ffff:ffff:fff8::/94", "ffff:ffff:ffff:ffff:ffff:fffc::/95", "ffff:ffff:ffff:ffff:ffff:fffe::/96", "ffff:ffff:ffff:ffff:ffff:ffff::/97", "ffff:ffff:ffff:ffff:ffff:ffff:8000:0/98", "ffff:ffff:ffff:ffff:ffff:ffff:c000:0/99", "ffff:ffff:ffff:ffff:ffff:ffff:e000:0/100", "ffff:ffff:ffff:ffff:ffff:ffff:f000:0/101", "ffff:ffff:ffff:ffff:ffff:ffff:f800:0/102", "ffff:ffff:ffff:ffff:ffff:ffff:fc00:0/103", "ffff:ffff:ffff:ffff:ffff:ffff:fe00:0/104", "ffff:ffff:ffff:ffff:ffff:ffff:ff00:0/105", "ffff:ffff:ffff:ffff:ffff:ffff:ff80:0/106", "ffff:ffff:ffff:ffff:ffff:ffff:ffc0:0/107", "ffff:ffff:ffff:ffff:ffff:ffff:ffe0:0/108", "ffff:ffff:ffff:ffff:ffff:ffff:fff0:0/109", "ffff:ffff:ffff:ffff:ffff:ffff:fff8:0/110", "ffff:ffff:ffff:ffff:ffff:ffff:fffc:0/111", "ffff:ffff:ffff:ffff:ffff:ffff:fffe:0/112", "ffff:ffff:ffff:ffff:ffff:ffff:ffff:0/113", "ffff:ffff:ffff:ffff:ffff:ffff:ffff:8000/114", "ffff:ffff:ffff:ffff:ffff:ffff:ffff:c000/115", "ffff:ffff:ffff:ffff:ffff:ffff:ffff:e000/116", "ffff:ffff:ffff:ffff:ffff:ffff:ffff:f000/117", "ffff:ffff:ffff:ffff:ffff:ffff:ffff:f800/118", "ffff:ffff:ffff:ffff:ffff:ffff:ffff:fc00/119", "ffff:ffff:ffff:ffff:ffff:ffff:ffff:fe00/120", "ffff:ffff:ffff:ffff:ffff:ffff:ffff:ff00/121", "ffff:ffff:ffff:ffff:ffff:ffff:ffff:ff80/122", "ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffc0/123", "ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffe0/124", "ffff:ffff:ffff:ffff:ffff:ffff:ffff:fff0/125", "ffff:ffff:ffff:ffff:ffff:ffff:ffff:fff8/126", "ffff:ffff:ffff:ffff:ffff:ffff:ffff:fffc/127", "ffff:ffff:ffff:ffff:ffff:ffff:ffff:fffe/128"}.Select(s => Subnet.Parse(s)),
                             IPAddress.Parse("::1"), IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:ffff:ffff:fffe")
                         };
        }

        [Theory]
        [MemberData(nameof(FewestConsecutiveSubnetsFor_Test_Values))]
        public void FewestConsecutiveSubnetsFor_Test(IEnumerable<Subnet> expected,
                                                     IPAddress left,
                                                     IPAddress right)
        {
            // Arrange
            // Act
            var result = SubnetUtilities.FewestConsecutiveSubnetsFor(left, right)
                                        .ToList();

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.All(result, Assert.NotNull);

            var expectedList = expected.ToList();

            // Assert.Equal(expectedList, result); // directly calling Assert.Equals results in unexpected behavior; unwinding equality explicitly
            Assert.Equal(expectedList.Count, result.Count);
            Assert.All(expectedList, subnet => Assert.Contains(subnet, result));
            Assert.All(result, subnet => Assert.Contains(subnet, expectedList));

            Assert.True(result.SequenceEqual(new SortedSet<Subnet>(result, new DefaultIPAddressRangeComparer())));
        }

        public static IEnumerable<object[]> FewestConsecutiveSubnetsFor_MissMatchAddressFamilies_ThrowsInvalidOperationException_Test_Values()
        {
            yield return new object[] {IPAddress.Any, IPAddress.IPv6Any};
            yield return new object[] {IPAddress.IPv6Any, IPAddress.Any};
        }

        [Theory]
        [MemberData(nameof(FewestConsecutiveSubnetsFor_MissMatchAddressFamilies_ThrowsInvalidOperationException_Test_Values))]
        public void FewestConsecutiveSubnetsFor_MissMatchAddressFamilies_ThrowsInvalidOperationException_Test(IPAddress alpha,
                                                                                                              IPAddress beta)
        {
            // Act
            // Assert
            Assert.Throws<InvalidOperationException>(() => SubnetUtilities.FewestConsecutiveSubnetsFor(alpha, beta));
        }

        public static IEnumerable<object[]> FewestConsecutiveSubnetsFor_Input_Null_ThrowsArgumentNullException_Test_Values()
        {
            yield return new object[] {null, null};
            yield return new object[] {IPAddress.Any, null};
            yield return new object[] {null, IPAddress.Any};
        }

        [Theory]
        [MemberData(nameof(FewestConsecutiveSubnetsFor_Input_Null_ThrowsArgumentNullException_Test_Values))]
        public void FewestConsecutiveSubnetsFor_Input_Null_ThrowsArgumentNullException_Test(IPAddress alpha,
                                                                                            IPAddress beta)
        {
            // Act
            // Assert
            Assert.Throws<ArgumentNullException>(() => SubnetUtilities.FewestConsecutiveSubnetsFor(alpha, beta));
        }

        #endregion

        #region LargestSubnet

        [Fact]
        public void LargestSubnet_Ambiguous_ReturnsOneOfLargest_Test()
        {
            // Arrange
            var subnets = new[]
                          {
                              new Subnet(IPAddress.Any, 24),
                              new Subnet(IPAddress.Any, 16),
                              new Subnet(IPAddress.Any, 16),
                              new Subnet(IPAddress.Any, 32)
                          };

            // Act
            var result = SubnetUtilities.LargestSubnet(subnets);

            // Assert
            Assert.Equal(new Subnet(IPAddress.Any, 16), result);
        }

        [Fact]
        public void LargestSubnet_EmptyInput_ReturnsNull_Test()
        {
            // Arrange
            var subnets = Enumerable.Empty<Subnet>();

            // Act
            var result = SubnetUtilities.LargestSubnet(subnets);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void LargestSubnet_NullInput_ReturnsNull_Test()
        {
            // Arrange
            // Act
            var result = SubnetUtilities.LargestSubnet(null);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void LargestSubnet_Single_ReturnSingle_Test()
        {
            // Arrange
            var expected = new Subnet(IPAddress.Any, 16);

            var subnets = new[]
                          {
                              expected
                          };

            // Act

            var result = SubnetUtilities.LargestSubnet(subnets);

            // Assert
            Assert.Same(expected, result);
        }

        [Fact]
        public void LargestSubnet_ReturnsLargest_Test()
        {
            // Arrange
            var expected = new Subnet(IPAddress.Any, 16);

            var subnets = new[]
                          {
                              expected,
                              new Subnet(IPAddress.Any, 24),
                              new Subnet(IPAddress.Any, 32)
                          };

            // Act

            var result = SubnetUtilities.LargestSubnet(subnets);

            // Assert
            Assert.Same(expected, result);
        }

        #endregion

        #region SmallestSubnet

        [Fact]
        public void SmallestSubnet_Ambiguous_ReturnsOneOfSmallest_Test()
        {
            // Arrange
            var subnets = new[]
                          {
                              new Subnet(IPAddress.Any, 24),
                              new Subnet(IPAddress.Any, 32),
                              new Subnet(IPAddress.Any, 32),
                              new Subnet(IPAddress.Any, 16)
                          };

            // Act
            var result = SubnetUtilities.SmallestSubnet(subnets);

            // Assert
            Assert.Equal(new Subnet(IPAddress.Any, 32), result);
        }

        [Fact]
        public void SmallestSubnet_EmptyInput_ReturnsNull_Test()
        {
            // Arrange
            var subnets = Enumerable.Empty<Subnet>();

            // Act
            var result = SubnetUtilities.SmallestSubnet(subnets);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void SmallestSubnet_NullInput_ReturnsNull_Test()
        {
            // Arrange
            // Act
            var result = SubnetUtilities.SmallestSubnet(null);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void SmallestSubnet_Single_ReturnSingle_Test()
        {
            // Arrange
            var expected = new Subnet(IPAddress.Any, 16);

            var subnets = new[]
                          {
                              expected
                          };

            // Act
            var result = SubnetUtilities.SmallestSubnet(subnets);

            // Assert
            Assert.Same(expected, result);
        }

        [Fact]
        public void SmallestSubnet_ReturnsSmallestSubnet_Test()
        {
            // Arrange
            var expected = new Subnet(IPAddress.Any, 32);

            var subnets = new[]
                          {
                              expected,
                              new Subnet(IPAddress.Any, 24),
                              new Subnet(IPAddress.Any, 16)
                          };

            // Act

            var result = SubnetUtilities.SmallestSubnet(subnets);

            // Assert
            Assert.Same(expected, result);
        }

        #endregion
    }
}
