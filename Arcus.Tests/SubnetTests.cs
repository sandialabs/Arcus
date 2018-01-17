using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using NUnit.Framework;

namespace Arcus.Tests
{
    [TestFixture]
    public class SubnetTests
    {
        [TestCase("192.168.1.1", ExpectedResult = "192.168.1.1/32")]
        [TestCase("192.168.1", ExpectedResult = "192.168.1.0/24")]
        [TestCase("192.168.1.", ExpectedResult = "192.168.1.0/24")]
        [TestCase("192.168", ExpectedResult = "192.168.0.0/16")]
        [TestCase("192.168.", ExpectedResult = "192.168.0.0/16")]
        [TestCase("192", ExpectedResult = "192.0.0.0/8")]
        [TestCase("192.", ExpectedResult = "192.0.0.0/8")]
        public string TryIPv4FromPartialTestCasesPass(string input)
        {
            // act
            Subnet subnet;
            var success = Subnet.TryIPv4FromPartial(input, out subnet);

            // assert
            Assert.IsTrue(success);
            Assert.IsNotNull(subnet);
            return subnet.ToString();
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("              ")]
        [TestCase("abca::")]
        [TestCase("192.168.1.1.")]
        [TestCase("192.168.1.1.1")]
        [TestCase("192.abc.1.1.1")]
        [TestCase("potato")]
        public void TryIPv4FromPartialTestCasesFail(string input)
        {
            // act
            Subnet subnet;
            var success = Subnet.TryIPv4FromPartial(input, out subnet);

            // assert
            Assert.IsFalse(success);
            Assert.IsNull(subnet);
        }

        [TestCase("::", ExpectedResult = new[] {"::/0", "::/16", "::/32", "::/48", "::/64", "::/80", "::/96", "::/112", "::/128"})]
        [TestCase("0::", ExpectedResult = new[] {"::/16", "::/32", "::/48", "::/64", "::/80", "::/96", "::/112", "::/128"})]
        [TestCase("0:0::", ExpectedResult = new[] {"::/32", "::/48", "::/64", "::/80", "::/96", "::/112", "::/128"})]
        [TestCase("0:0:0::", ExpectedResult = new[] {"::/48", "::/64", "::/80", "::/96", "::/112", "::/128"})]
        [TestCase("0:0:0:0::", ExpectedResult = new[] {"::/64", "::/80", "::/96", "::/112", "::/128"})]
        [TestCase("0:0:0:0:0::", ExpectedResult = new[] {"::/80", "::/96", "::/112", "::/128"})]
        [TestCase("0:0:0:0:0:0::", ExpectedResult = new[] {"::/96", "::/112", "::/128"})]
        [TestCase("0:0:0:0:0:0:0::", ExpectedResult = new[] {"::/112", "::/128"})]
        [TestCase("0:0:0:0:0:0:0:0", ExpectedResult = new[] {"::/128"})]
        [TestCase("1:2:3:4:5:6:7:8", ExpectedResult = new[] {"1:2:3:4:5:6:7:8/128"})]
        [TestCase("a:b:c:d:e:f:0:1", ExpectedResult = new[] {"a:b:c:d:e:f:0:1/128"})]
        [TestCase("abcd", ExpectedResult = new[] {"abcd::/16", "abcd::/32", "abcd::/48", "abcd::/64", "abcd::/80", "abcd::/96", "abcd::/112", "abcd::/128"})]
        [TestCase("abcd:", ExpectedResult = new[] {"abcd::/16", "abcd::/32", "abcd::/48", "abcd::/64", "abcd::/80", "abcd::/96", "abcd::/112", "abcd::/128"})]
        [TestCase("abcd::", ExpectedResult = new[] {"abcd::/16", "abcd::/32", "abcd::/48", "abcd::/64", "abcd::/80", "abcd::/96", "abcd::/112", "abcd::/128"})]
        [TestCase("abcd::123", ExpectedResult = new[] {"abcd:123::/32", "abcd:0:123::/48", "abcd:0:0:123::/64", "abcd::123:0:0:0/80", "abcd::123:0:0/96", "abcd::123:0/112", "abcd::123/128"})]
        [TestCase("abcd::123:", ExpectedResult = new[] {"abcd:123::/32", "abcd:0:123::/48", "abcd:0:0:123::/64", "abcd::123:0:0:0/80", "abcd::123:0:0/96", "abcd::123:0/112", "abcd::123/128"})]
        [TestCase("abcd::123:0", ExpectedResult = new[] {"abcd:123::/48", "abcd:0:123::/64", "abcd:0:0:123::/80", "abcd::123:0:0:0/96", "abcd::123:0:0/112", "abcd::123:0/128"})]
        [TestCase("abcd:0:123:0", ExpectedResult = new[] {"abcd:0:123::/64", "abcd:0:123::/80", "abcd:0:123::/96", "abcd:0:123::/112", "abcd:0:123::/128"})]
        [TestCase("abcd:0:123:0:", ExpectedResult = new[] {"abcd:0:123::/64", "abcd:0:123::/80", "abcd:0:123::/96", "abcd:0:123::/112", "abcd:0:123::/128"})]
        [TestCase("abcd:0:123:0::", ExpectedResult = new[] {"abcd:0:123::/64", "abcd:0:123::/80", "abcd:0:123::/96", "abcd:0:123::/112", "abcd:0:123::/128"})]
        [TestCase("abcd::/48", ExpectedResult = new[] {"abcd::/48"})]
        [TestCase("abcd::/16", ExpectedResult = new[] {"abcd::/16"})]
        [TestCase("abcd::/126", ExpectedResult = new[] {"abcd::/126"})]
        public IEnumerable<string> TryIPv6FromPartialTestCasesPass(string input)
        {
            // act
            IEnumerable<Subnet> subnets;
            var success = Subnet.TryIPv6FromPartial(input, out subnets);

            // assert
            Assert.IsTrue(success);
            return subnets.Select(subnet => subnet.ToString());
        }

        [TestCase("")]
        [TestCase("  ")]
        [TestCase(null)]
        [TestCase("cat")]
        [TestCase("abcde::")]
        [TestCase("::abcde")]
        [TestCase("0:0:0:0:0:0:0:0:")]
        [TestCase("0:0:0:0:0:0:0:0:0")]
        public void TryIPv6FromPartialTestCasesFail(string input)
        {
            // act
            IEnumerable<Subnet> subnets;
            var success = Subnet.TryIPv6FromPartial(input, out subnets);

            // assert
            Assert.IsFalse(success);
            Assert.IsEmpty(subnets);
        }

        [TestCase("0.0.0.0", "0.0.0.0", ExpectedResult = "0.0.0.0/32")]
        [TestCase("0.0.0.0", "255.255.255.255", ExpectedResult = "0.0.0.0/0")]
        [TestCase("0.0.0.0", "63.255.255.255", ExpectedResult = "0.0.0.0/2")]
        [TestCase("0.0.0.0", "0.0.0.15", ExpectedResult = "0.0.0.0/28")]
        [TestCase("ffff::", "ffff::", ExpectedResult = "ffff::/128")]
        [TestCase("::", "3:ffff:ffff:ffff:ffff:ffff:ffff:ffff", ExpectedResult = "::/14")]
        [TestCase("ffff::", "ffff::1", ExpectedResult = "ffff::/127")]
        [TestCase("::", "::ffff:ffff:ffff:ffff", ExpectedResult = "::/64")]
        public string SubnetFromByteArrays(string primary,
                                           string secondary)
        {
            var subnet = new Subnet(
                IPAddress.Parse(primary)
                         .GetAddressBytes(),
                IPAddress.Parse(secondary)
                         .GetAddressBytes());

            Assert.IsNotNull(subnet);
            return subnet.ToString();
        }

        [TestCase("0.0.0.0", "0.0.0.0", ExpectedResult = "0.0.0.0/32")]
        [TestCase("0.0.0.0", "255.255.255.255", ExpectedResult = "0.0.0.0/0")]
        [TestCase("0.0.0.0", "63.255.255.255", ExpectedResult = "0.0.0.0/2")]
        [TestCase("0.0.0.0", "0.0.0.15", ExpectedResult = "0.0.0.0/28")]
        [TestCase("ffff::", "ffff::", ExpectedResult = "ffff::/128")]
        [TestCase("::", "3:ffff:ffff:ffff:ffff:ffff:ffff:ffff", ExpectedResult = "::/14")]
        [TestCase("ffff::", "ffff::1", ExpectedResult = "ffff::/127")]
        [TestCase("::", "::ffff:ffff:ffff:ffff", ExpectedResult = "::/64")]
        public string SubnetFromIPAddresses(string primary,
                                            string secondary)
        {
            var subnet = new Subnet(IPAddress.Parse(primary), IPAddress.Parse(secondary));

            Assert.IsNotNull(subnet);
            return subnet.ToString();
        }

        [TestCase("::1/128")]
        [TestCase("::2/127")]
        [TestCase("::4/126")]
        [TestCase("::8/125")]
        [TestCase("::10/124")]
        [TestCase("::20/123")]
        [TestCase("::40/122")]
        [TestCase("::80/121")]
        [TestCase("::100/120")]
        [TestCase("::200/119")]
        [TestCase("::400/118")]
        [TestCase("::800/117")]
        [TestCase("::1000/116")]
        [TestCase("::2000/115")]
        [TestCase("::4000/114")]
        [TestCase("::8000/113")]
        [TestCase("::0.1.0.0/112")]
        [TestCase("::0.2.0.0/111")]
        [TestCase("::0.4.0.0/110")]
        [TestCase("::0.8.0.0/109")]
        [TestCase("::0.16.0.0/108")]
        [TestCase("::0.32.0.0/107")]
        [TestCase("::0.64.0.0/106")]
        [TestCase("::0.128.0.0/105")]
        [TestCase("::1.0.0.0/104")]
        [TestCase("::2.0.0.0/103")]
        [TestCase("::4.0.0.0/102")]
        [TestCase("::8.0.0.0/101")]
        [TestCase("::16.0.0.0/100")]
        [TestCase("::32.0.0.0/99")]
        [TestCase("::64.0.0.0/98")]
        [TestCase("::128.0.0.0/97")]
        [TestCase("::1:0:0/96")]
        [TestCase("::2:0:0/95")]
        [TestCase("::4:0:0/94")]
        [TestCase("::8:0:0/93")]
        [TestCase("::10:0:0/92")]
        [TestCase("::20:0:0/91")]
        [TestCase("::40:0:0/90")]
        [TestCase("::80:0:0/89")]
        [TestCase("::100:0:0/88")]
        [TestCase("::200:0:0/87")]
        [TestCase("::400:0:0/86")]
        [TestCase("::800:0:0/85")]
        [TestCase("::1000:0:0/84")]
        [TestCase("::2000:0:0/83")]
        [TestCase("::4000:0:0/82")]
        [TestCase("::8000:0:0/81")]
        [TestCase("::1:0:0:0/80")]
        [TestCase("::2:0:0:0/79")]
        [TestCase("::4:0:0:0/78")]
        [TestCase("::8:0:0:0/77")]
        [TestCase("::10:0:0:0/76")]
        [TestCase("::20:0:0:0/75")]
        [TestCase("::40:0:0:0/74")]
        [TestCase("::80:0:0:0/73")]
        [TestCase("::100:0:0:0/72")]
        [TestCase("::200:0:0:0/71")]
        [TestCase("::400:0:0:0/70")]
        [TestCase("::800:0:0:0/69")]
        [TestCase("::1000:0:0:0/68")]
        [TestCase("::2000:0:0:0/67")]
        [TestCase("::4000:0:0:0/66")]
        [TestCase("::8000:0:0:0/65")]
        [TestCase("0:0:0:1::/64")]
        [TestCase("0:0:0:2::/63")]
        [TestCase("0:0:0:4::/62")]
        [TestCase("0:0:0:8::/61")]
        [TestCase("0:0:0:10::/60")]
        [TestCase("0:0:0:20::/59")]
        [TestCase("0:0:0:40::/58")]
        [TestCase("0:0:0:80::/57")]
        [TestCase("0:0:0:100::/56")]
        [TestCase("0:0:0:200::/55")]
        [TestCase("0:0:0:400::/54")]
        [TestCase("0:0:0:800::/53")]
        [TestCase("0:0:0:1000::/52")]
        [TestCase("0:0:0:2000::/51")]
        [TestCase("0:0:0:4000::/50")]
        [TestCase("0:0:0:8000::/49")]
        [TestCase("0:0:1::/48")]
        [TestCase("0:0:2::/47")]
        [TestCase("0:0:4::/46")]
        [TestCase("0:0:8::/45")]
        [TestCase("0:0:10::/44")]
        [TestCase("0:0:20::/43")]
        [TestCase("0:0:40::/42")]
        [TestCase("0:0:80::/41")]
        [TestCase("0:0:100::/40")]
        [TestCase("0:0:200::/39")]
        [TestCase("0:0:400::/38")]
        [TestCase("0:0:800::/37")]
        [TestCase("0:0:1000::/36")]
        [TestCase("0:0:2000::/35")]
        [TestCase("0:0:4000::/34")]
        [TestCase("0:0:8000::/33")]
        [TestCase("0:1::/32")]
        [TestCase("0:2::/31")]
        [TestCase("0:4::/30")]
        [TestCase("0:8::/29")]
        [TestCase("0:10::/28")]
        [TestCase("0:20::/27")]
        [TestCase("0:40::/26")]
        [TestCase("0:80::/25")]
        [TestCase("0:100::/24")]
        [TestCase("0:200::/23")]
        [TestCase("0:400::/22")]
        [TestCase("0:800::/21")]
        [TestCase("0:1000::/20")]
        [TestCase("0:2000::/19")]
        [TestCase("0:4000::/18")]
        [TestCase("0:8000::/17")]
        [TestCase("1::/16")]
        [TestCase("2::/15")]
        [TestCase("4::/14")]
        [TestCase("8::/13")]
        [TestCase("10::/12")]
        [TestCase("20::/11")]
        [TestCase("40::/10")]
        [TestCase("80::/9")]
        [TestCase("100::/8")]
        [TestCase("200::/7")]
        [TestCase("400::/6")]
        [TestCase("800::/5")]
        [TestCase("1000::/4")]
        [TestCase("2000::/3")]
        [TestCase("4000::/2")]
        [TestCase("8000::/2")]
        [TestCase("c000::/3")]
        [TestCase("e000::/4")]
        [TestCase("f000::/5")]
        [TestCase("f800::/6")]
        [TestCase("fc00::/7")]
        [TestCase("fe00::/8")]
        [TestCase("ff00::/9")]
        [TestCase("ff80::/10")]
        [TestCase("ffc0::/11")]
        [TestCase("ffe0::/12")]
        [TestCase("fff0::/13")]
        [TestCase("fff8::/14")]
        [TestCase("fffc::/15")]
        [TestCase("fffe::/16")]
        [TestCase("ffff::/17")]
        [TestCase("ffff:8000::/18")]
        [TestCase("ffff:c000::/19")]
        [TestCase("ffff:e000::/20")]
        [TestCase("ffff:f000::/21")]
        [TestCase("ffff:f800::/22")]
        [TestCase("ffff:fc00::/23")]
        [TestCase("ffff:fe00::/24")]
        [TestCase("ffff:ff00::/25")]
        [TestCase("ffff:ff80::/26")]
        [TestCase("ffff:ffc0::/27")]
        [TestCase("ffff:ffe0::/28")]
        [TestCase("ffff:fff0::/29")]
        [TestCase("ffff:fff8::/30")]
        [TestCase("ffff:fffc::/31")]
        [TestCase("ffff:fffe::/32")]
        [TestCase("ffff:ffff::/33")]
        [TestCase("ffff:ffff:8000::/34")]
        [TestCase("ffff:ffff:c000::/35")]
        [TestCase("ffff:ffff:e000::/36")]
        [TestCase("ffff:ffff:f000::/37")]
        [TestCase("ffff:ffff:f800::/38")]
        [TestCase("ffff:ffff:fc00::/39")]
        [TestCase("ffff:ffff:fe00::/40")]
        [TestCase("ffff:ffff:ff00::/41")]
        [TestCase("ffff:ffff:ff80::/42")]
        [TestCase("ffff:ffff:ffc0::/43")]
        [TestCase("ffff:ffff:ffe0::/44")]
        [TestCase("ffff:ffff:fff0::/45")]
        [TestCase("ffff:ffff:fff8::/46")]
        [TestCase("ffff:ffff:fffc::/47")]
        [TestCase("ffff:ffff:fffe::/48")]
        [TestCase("ffff:ffff:ffff::/49")]
        [TestCase("ffff:ffff:ffff:8000::/50")]
        [TestCase("ffff:ffff:ffff:c000::/51")]
        [TestCase("ffff:ffff:ffff:e000::/52")]
        [TestCase("ffff:ffff:ffff:f000::/53")]
        [TestCase("ffff:ffff:ffff:f800::/54")]
        [TestCase("ffff:ffff:ffff:fc00::/55")]
        [TestCase("ffff:ffff:ffff:fe00::/56")]
        [TestCase("ffff:ffff:ffff:ff00::/57")]
        [TestCase("ffff:ffff:ffff:ff80::/58")]
        [TestCase("ffff:ffff:ffff:ffc0::/59")]
        [TestCase("ffff:ffff:ffff:ffe0::/60")]
        [TestCase("ffff:ffff:ffff:fff0::/61")]
        [TestCase("ffff:ffff:ffff:fff8::/62")]
        [TestCase("ffff:ffff:ffff:fffc::/63")]
        [TestCase("ffff:ffff:ffff:fffe::/64")]
        [TestCase("ffff:ffff:ffff:ffff::/65")]
        [TestCase("ffff:ffff:ffff:ffff:8000::/66")]
        [TestCase("ffff:ffff:ffff:ffff:c000::/67")]
        [TestCase("ffff:ffff:ffff:ffff:e000::/68")]
        [TestCase("ffff:ffff:ffff:ffff:f000::/69")]
        [TestCase("ffff:ffff:ffff:ffff:f800::/70")]
        [TestCase("ffff:ffff:ffff:ffff:fc00::/71")]
        [TestCase("ffff:ffff:ffff:ffff:fe00::/72")]
        [TestCase("ffff:ffff:ffff:ffff:ff00::/73")]
        [TestCase("ffff:ffff:ffff:ffff:ff80::/74")]
        [TestCase("ffff:ffff:ffff:ffff:ffc0::/75")]
        [TestCase("ffff:ffff:ffff:ffff:ffe0::/76")]
        [TestCase("ffff:ffff:ffff:ffff:fff0::/77")]
        [TestCase("ffff:ffff:ffff:ffff:fff8::/78")]
        [TestCase("ffff:ffff:ffff:ffff:fffc::/79")]
        [TestCase("ffff:ffff:ffff:ffff:fffe::/80")]
        [TestCase("ffff:ffff:ffff:ffff:ffff::/81")]
        [TestCase("ffff:ffff:ffff:ffff:ffff:8000::/82")]
        [TestCase("ffff:ffff:ffff:ffff:ffff:c000::/83")]
        [TestCase("ffff:ffff:ffff:ffff:ffff:e000::/84")]
        [TestCase("ffff:ffff:ffff:ffff:ffff:f000::/85")]
        [TestCase("ffff:ffff:ffff:ffff:ffff:f800::/86")]
        [TestCase("ffff:ffff:ffff:ffff:ffff:fc00::/87")]
        [TestCase("ffff:ffff:ffff:ffff:ffff:fe00::/88")]
        [TestCase("ffff:ffff:ffff:ffff:ffff:ff00::/89")]
        [TestCase("ffff:ffff:ffff:ffff:ffff:ff80::/90")]
        [TestCase("ffff:ffff:ffff:ffff:ffff:ffc0::/91")]
        [TestCase("ffff:ffff:ffff:ffff:ffff:ffe0::/92")]
        [TestCase("ffff:ffff:ffff:ffff:ffff:fff0::/93")]
        [TestCase("ffff:ffff:ffff:ffff:ffff:fff8::/94")]
        [TestCase("ffff:ffff:ffff:ffff:ffff:fffc::/95")]
        [TestCase("ffff:ffff:ffff:ffff:ffff:fffe::/96")]
        [TestCase("ffff:ffff:ffff:ffff:ffff:ffff::/97")]
        [TestCase("ffff:ffff:ffff:ffff:ffff:ffff:8000:0/98")]
        [TestCase("ffff:ffff:ffff:ffff:ffff:ffff:c000:0/99")]
        [TestCase("ffff:ffff:ffff:ffff:ffff:ffff:e000:0/100")]
        [TestCase("ffff:ffff:ffff:ffff:ffff:ffff:f000:0/101")]
        [TestCase("ffff:ffff:ffff:ffff:ffff:ffff:f800:0/102")]
        [TestCase("ffff:ffff:ffff:ffff:ffff:ffff:fc00:0/103")]
        [TestCase("ffff:ffff:ffff:ffff:ffff:ffff:fe00:0/104")]
        [TestCase("ffff:ffff:ffff:ffff:ffff:ffff:ff00:0/105")]
        [TestCase("ffff:ffff:ffff:ffff:ffff:ffff:ff80:0/106")]
        [TestCase("ffff:ffff:ffff:ffff:ffff:ffff:ffc0:0/107")]
        [TestCase("ffff:ffff:ffff:ffff:ffff:ffff:ffe0:0/108")]
        [TestCase("ffff:ffff:ffff:ffff:ffff:ffff:fff0:0/109")]
        [TestCase("ffff:ffff:ffff:ffff:ffff:ffff:fff8:0/110")]
        [TestCase("ffff:ffff:ffff:ffff:ffff:ffff:fffc:0/111")]
        [TestCase("ffff:ffff:ffff:ffff:ffff:ffff:fffe:0/112")]
        [TestCase("ffff:ffff:ffff:ffff:ffff:ffff:ffff:0/113")]
        [TestCase("ffff:ffff:ffff:ffff:ffff:ffff:ffff:8000/114")]
        [TestCase("ffff:ffff:ffff:ffff:ffff:ffff:ffff:c000/115")]
        [TestCase("ffff:ffff:ffff:ffff:ffff:ffff:ffff:e000/116")]
        [TestCase("ffff:ffff:ffff:ffff:ffff:ffff:ffff:f000/117")]
        [TestCase("ffff:ffff:ffff:ffff:ffff:ffff:ffff:f800/118")]
        [TestCase("ffff:ffff:ffff:ffff:ffff:ffff:ffff:fc00/119")]
        [TestCase("ffff:ffff:ffff:ffff:ffff:ffff:ffff:fe00/120")]
        [TestCase("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ff00/121")]
        [TestCase("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ff80/122")]
        [TestCase("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffc0/123")]
        [TestCase("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffe0/124")]
        [TestCase("ffff:ffff:ffff:ffff:ffff:ffff:ffff:fff0/125")]
        [TestCase("ffff:ffff:ffff:ffff:ffff:ffff:ffff:fff8/126")]
        [TestCase("ffff:ffff:ffff:ffff:ffff:ffff:ffff:fffc/127")]
        [TestCase("ffff:ffff:ffff:ffff:ffff:ffff:ffff:fffe/128")]
        public void ParseFromStringIPv6Test(string input)
        {
            Subnet subnet;
            Assert.IsTrue(Subnet.TryParse(input, out subnet), "could not parse '{0}'", input);
            Assert.AreEqual(AddressFamily.InterNetworkV6, subnet.AddressFamily);
            Assert.AreEqual(input, subnet.ToString());
        }

        [TestCase("0.0.0.1/32")]
        [TestCase("0.0.0.2/31")]
        [TestCase("0.0.0.4/30")]
        [TestCase("0.0.0.8/29")]
        [TestCase("0.0.0.16/28")]
        [TestCase("0.0.0.32/27")]
        [TestCase("0.0.0.64/26")]
        [TestCase("0.0.0.128/25")]
        [TestCase("0.0.1.0/24")]
        [TestCase("0.0.2.0/23")]
        [TestCase("0.0.4.0/22")]
        [TestCase("0.0.8.0/21")]
        [TestCase("0.0.16.0/20")]
        [TestCase("0.0.32.0/19")]
        [TestCase("0.0.64.0/18")]
        [TestCase("0.0.128.0/17")]
        [TestCase("0.1.0.0/16")]
        [TestCase("0.2.0.0/15")]
        [TestCase("0.4.0.0/14")]
        [TestCase("0.8.0.0/13")]
        [TestCase("0.16.0.0/12")]
        [TestCase("0.32.0.0/11")]
        [TestCase("0.64.0.0/10")]
        [TestCase("0.128.0.0/9")]
        [TestCase("1.0.0.0/8")]
        [TestCase("2.0.0.0/7")]
        [TestCase("4.0.0.0/6")]
        [TestCase("8.0.0.0/5")]
        [TestCase("16.0.0.0/4")]
        [TestCase("32.0.0.0/3")]
        [TestCase("64.0.0.0/2")]
        [TestCase("128.0.0.0/2")]
        [TestCase("192.0.0.0/3")]
        [TestCase("224.0.0.0/4")]
        [TestCase("240.0.0.0/5")]
        [TestCase("248.0.0.0/6")]
        [TestCase("252.0.0.0/7")]
        [TestCase("254.0.0.0/8")]
        [TestCase("255.0.0.0/9")]
        [TestCase("255.128.0.0/10")]
        [TestCase("255.192.0.0/11")]
        [TestCase("255.224.0.0/12")]
        [TestCase("255.240.0.0/13")]
        [TestCase("255.248.0.0/14")]
        [TestCase("255.252.0.0/15")]
        [TestCase("255.254.0.0/16")]
        [TestCase("255.255.0.0/17")]
        [TestCase("255.255.128.0/18")]
        [TestCase("255.255.192.0/19")]
        [TestCase("255.255.224.0/20")]
        [TestCase("255.255.240.0/21")]
        [TestCase("255.255.248.0/22")]
        [TestCase("255.255.252.0/23")]
        [TestCase("255.255.254.0/24")]
        [TestCase("255.255.255.0/25")]
        [TestCase("255.255.255.128/26")]
        [TestCase("255.255.255.192/27")]
        [TestCase("255.255.255.224/28")]
        [TestCase("255.255.255.240/29")]
        [TestCase("255.255.255.248/30")]
        [TestCase("255.255.255.252/31")]
        [TestCase("255.255.255.254/32")]
        public void ParseFromStringIPv4Test(string input)
        {
            Subnet subnet;
            Assert.IsTrue(Subnet.TryParse(input, out subnet), "could not parse '{0}'", input);
            Assert.AreEqual(AddressFamily.InterNetwork, subnet.AddressFamily);
            Assert.AreEqual(input, subnet.ToString());
        }

        [TestCase("192.168.1.1/32", ExpectedResult = "192.168.1.1")]
        [TestCase("192.168.1.1/16", ExpectedResult = "192.168.0.0/16")]
        [TestCase("192.168.1.1/0", ExpectedResult = "0.0.0.0/0")]
        [TestCase("::/128", ExpectedResult = "::")]
        [TestCase("::/64", ExpectedResult = "::/64")]
        [TestCase("::/0", ExpectedResult = "::/0")]
        public string ToFriendlyStringTest(string input)
        {
            // Arrange
            var subnet = Subnet.Parse(input);

            // Act
            var result = subnet.ToFriendlyString();

            // Assert
            return result;
        }

        [TestCase("192.168.1.1", "0.0.0.0", ExpectedResult = "0.0.0.0/0")]
        [TestCase("192.168.1.1", "128.0.0.0", ExpectedResult = "128.0.0.0/1")]
        [TestCase("192.168.1.1", "192.0.0.0", ExpectedResult = "192.0.0.0/2")]
        [TestCase("192.168.1.1", "224.0.0.0", ExpectedResult = "192.0.0.0/3")]
        [TestCase("192.168.1.1", "240.0.0.0", ExpectedResult = "192.0.0.0/4")]
        [TestCase("192.168.1.1", "248.0.0.0", ExpectedResult = "192.0.0.0/5")]
        [TestCase("192.168.1.1", "252.0.0.0", ExpectedResult = "192.0.0.0/6")]
        [TestCase("192.168.1.1", "254.0.0.0", ExpectedResult = "192.0.0.0/7")]
        [TestCase("192.168.1.1", "255.0.0.0", ExpectedResult = "192.0.0.0/8")]
        [TestCase("192.168.1.1", "255.128.0.0", ExpectedResult = "192.128.0.0/9")]
        [TestCase("192.168.1.1", "255.192.0.0", ExpectedResult = "192.128.0.0/10")]
        [TestCase("192.168.1.1", "255.224.0.0", ExpectedResult = "192.160.0.0/11")]
        [TestCase("192.168.1.1", "255.240.0.0", ExpectedResult = "192.160.0.0/12")]
        [TestCase("192.168.1.1", "255.248.0.0", ExpectedResult = "192.168.0.0/13")]
        [TestCase("192.168.1.1", "255.252.0.0", ExpectedResult = "192.168.0.0/14")]
        [TestCase("192.168.1.1", "255.254.0.0", ExpectedResult = "192.168.0.0/15")]
        [TestCase("192.168.1.1", "255.255.0.0", ExpectedResult = "192.168.0.0/16")]
        [TestCase("192.168.1.1", "255.255.128.0", ExpectedResult = "192.168.0.0/17")]
        [TestCase("192.168.1.1", "255.255.192.0", ExpectedResult = "192.168.0.0/18")]
        [TestCase("192.168.1.1", "255.255.224.0", ExpectedResult = "192.168.0.0/19")]
        [TestCase("192.168.1.1", "255.255.240.0", ExpectedResult = "192.168.0.0/20")]
        [TestCase("192.168.1.1", "255.255.248.0", ExpectedResult = "192.168.0.0/21")]
        [TestCase("192.168.1.1", "255.255.252.0", ExpectedResult = "192.168.0.0/22")]
        [TestCase("192.168.1.1", "255.255.254.0", ExpectedResult = "192.168.0.0/23")]
        [TestCase("192.168.1.1", "255.255.255.0", ExpectedResult = "192.168.1.0/24")]
        [TestCase("192.168.1.1", "255.255.255.128", ExpectedResult = "192.168.1.0/25")]
        [TestCase("192.168.1.1", "255.255.255.192", ExpectedResult = "192.168.1.0/26")]
        [TestCase("192.168.1.1", "255.255.255.224", ExpectedResult = "192.168.1.0/27")]
        [TestCase("192.168.1.1", "255.255.255.240", ExpectedResult = "192.168.1.0/28")]
        [TestCase("192.168.1.1", "255.255.255.248", ExpectedResult = "192.168.1.0/29")]
        [TestCase("192.168.1.1", "255.255.255.252", ExpectedResult = "192.168.1.0/30")]
        [TestCase("192.168.1.1", "255.255.255.254", ExpectedResult = "192.168.1.0/31")]
        [TestCase("192.168.1.1", "255.255.255.255", ExpectedResult = "192.168.1.1/32")]
        public string FromNetMaskTest(string networkPrefix,
                                      string netmask)
        {
            // Arrange
            var networkPrefixAddress = IPAddress.Parse(networkPrefix);
            var netmaskAddress = IPAddress.Parse(netmask);

            // Act
            var result = Subnet.FromNetMask(networkPrefixAddress, netmaskAddress);

            // Assert
            return result.ToString();
        }

        [TestCase("", "", ExpectedResult = null)]
        [TestCase(null, null, ExpectedResult = null)]
        [TestCase("::", "potato", ExpectedResult = null)]
        [TestCase("potato", "::", ExpectedResult = null)]
        [TestCase("192.168.1.1", "potato", ExpectedResult = null)]
        [TestCase("potato", "192.168.1.1", ExpectedResult = null)]
        [TestCase("192.168.1.1", null, ExpectedResult = null)]
        [TestCase(null, "192.168.1.1", ExpectedResult = null)]
        [TestCase("::", null, ExpectedResult = null)]
        [TestCase(null, "::", ExpectedResult = null)]
        [TestCase("ffff::", "::", ExpectedResult = null)]
        [TestCase("255.0.0.0::", "128.0.0.0", ExpectedResult = null)]
        [TestCase("192.168.1.1", "::", ExpectedResult = null)]
        [TestCase("::", "192.168.1.1", ExpectedResult = null)]
        [TestCase("::", "::", ExpectedResult = "::/128")]
        [TestCase("0.0.0.0", "0.0.0.0", ExpectedResult = "0.0.0.0/32")]
        [TestCase("::", "ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff", ExpectedResult = "::/0")]
        [TestCase("0.0.0.0", "255.255.255.255", ExpectedResult = "0.0.0.0/0")]
        [TestCase("192.168.0.0", "192.168.255.255", ExpectedResult = "192.168.0.0/16")]
        [TestCase("192.168.15.4", "192.168.255.255", ExpectedResult = "192.168.0.0/16")]
        [TestCase("192.168.0.0", "192.168.255.250", ExpectedResult = "192.168.0.0/16")]
        public string TryParseHighLowResultTest(string low,
                                                string high)
        {
            // Arrange
            Subnet subnet;

            // Act
            Subnet.TryParse(low, high, out subnet);

            // Assert
            return subnet?.ToString();
        }

        [TestCase("", "", ExpectedResult = false)]
        [TestCase(null, null, ExpectedResult = false)]
        [TestCase("::", "potato", ExpectedResult = false)]
        [TestCase("potato", "::", ExpectedResult = false)]
        [TestCase("192.168.1.1", "potato", ExpectedResult = false)]
        [TestCase("potato", "192.168.1.1", ExpectedResult = false)]
        [TestCase("192.168.1.1", null, ExpectedResult = false)]
        [TestCase(null, "192.168.1.1", ExpectedResult = false)]
        [TestCase("::", null, ExpectedResult = false)]
        [TestCase(null, "::", ExpectedResult = false)]
        [TestCase("ffff::", "::", ExpectedResult = false)]
        [TestCase("255.0.0.0::", "128.0.0.0", ExpectedResult = false)]
        [TestCase("192.168.1.1", "::", ExpectedResult = false)]
        [TestCase("::", "192.168.1.1", ExpectedResult = false)]
        [TestCase("::", "::", ExpectedResult = true)]
        [TestCase("0.0.0.0", "0.0.0.0", ExpectedResult = true)]
        [TestCase("::", "ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff", ExpectedResult = true)]
        [TestCase("0.0.0.0", "255.255.255.255", ExpectedResult = true)]
        [TestCase("192.168.0.0", "192.168.255.255", ExpectedResult = true)]
        [TestCase("192.168.15.4", "192.168.255.255", ExpectedResult = true)]
        [TestCase("192.168.0.0", "192.168.255.250", ExpectedResult = true)]
        public bool TryParseHighLowSuccessTest(string low,
                                               string high)
        {
            // Arrange
            Subnet subnet;

            // Act
            var success = Subnet.TryParse(low, high, out subnet);

            // Assert
            return success;
        }

        [TestCase("0.0.0.0/0", "0.0.0.0/0", ExpectedResult = true)]
        [TestCase("::/0", "::/0", ExpectedResult = true)]
        [TestCase("0.0.0.0/0", "255.255.0.0/16", ExpectedResult = true)]
        [TestCase("255.255.0.0/16", "0.0.0.0/0", ExpectedResult = true)]
        [TestCase("::/0", "abcd:ef01::/64", ExpectedResult = true)]
        [TestCase("abcd:ef01::/64", "::/0", ExpectedResult = true)]
        [TestCase("0.0.0.0/0", null, ExpectedResult = false)]
        [TestCase("::/0", null, ExpectedResult = false)]
        [TestCase("0.0.0.0/0", "::/0", ExpectedResult = false)]
        [TestCase("::/0", "0.0.0.0/0", ExpectedResult = false)]
        public bool OverlapsTest(string subnetAString,
                                 string subnetBString)
        {
            // Arrange
            var subnetA = Subnet.Parse(subnetAString);

            Subnet subnetB;
            if (!Subnet.TryParse(subnetBString, out subnetB))
            {
                subnetB = null;
            }

            // Act
            var result = subnetA.Overlaps(subnetB);

            // Assert
            return result;
        }

        [TestCase("::/0", "::/0", ExpectedResult = true)]
        [TestCase("0.0.0.0/0", "0.0.0.0/0", ExpectedResult = true)]
        [TestCase("ffff::/64", "ffff::/64", ExpectedResult = true)]
        [TestCase("255.255.0.0/16", "255.255.0.0/16", ExpectedResult = true)]
        [TestCase("ffff::/64", "ffff::/32", ExpectedResult = false)]
        [TestCase("255.255.0.0/16", "255.255.0.0/8", ExpectedResult = false)]
        [TestCase("ffff::/64", "aabb::/64", ExpectedResult = false)]
        [TestCase("255.255.0.0/16", "128.128.0.0/16", ExpectedResult = false)]
        [TestCase("0.0.0.0/0", "::/0", ExpectedResult = false)]
        [TestCase("::/0", "0.0.0.0/0", ExpectedResult = false)]
        [TestCase("0.0.0.0/0", null, ExpectedResult = false)]
        [TestCase("::/0", null, ExpectedResult = false)]
        public bool EqualsTest(string subnetAString,
                               string subnetBString)
        {
            // Arrange
            var subnetA = Subnet.Parse(subnetAString);

            Subnet subnetB;
            if (!Subnet.TryParse(subnetBString, out subnetB))
            {
                subnetB = null;
            }

            // Act
            var result = subnetA.Equals((object) subnetB);

            // Assert
            return result;
        }

        [TestCase("::/0", "::/0", ExpectedResult = true)]
        [TestCase("0.0.0.0/0", "0.0.0.0/0", ExpectedResult = true)]
        [TestCase("ffff::/64", "ffff::/64", ExpectedResult = true)]
        [TestCase("255.255.0.0/16", "255.255.0.0/16", ExpectedResult = true)]
        [TestCase("ffff::/64", "ffff::/32", ExpectedResult = false)]
        [TestCase("255.255.0.0/16", "255.255.0.0/8", ExpectedResult = false)]
        [TestCase("ffff::/64", "aabb::/64", ExpectedResult = false)]
        [TestCase("255.255.0.0/16", "128.128.0.0/16", ExpectedResult = false)]
        [TestCase("0.0.0.0/0", "::/0", ExpectedResult = false)]
        [TestCase("::/0", "0.0.0.0/0", ExpectedResult = false)]
        [TestCase("0.0.0.0/0", null, ExpectedResult = false)]
        [TestCase("::/0", null, ExpectedResult = false)]
        public bool EqualExplicitTest(string subnetAString,
                                      string subnetBString)
        {
            // Arrange
            var subnetA = Subnet.Parse(subnetAString);

            Subnet subnetB;
            if (!Subnet.TryParse(subnetBString, out subnetB))
            {
                subnetB = null;
            }

            // Act
            var result = subnetA.Equals(subnetB);

            // Assert
            return result;
        }

        [TestCase("192.168.0.0/16", "192.168.0.0/16", ExpectedResult = 0)]
        [TestCase("ab:cd::/64", "ab:cd::/64", ExpectedResult = 0)]
        [TestCase("192.168.0.0/16", null, ExpectedResult = 1)]
        [TestCase("ab:cd::/64", null, ExpectedResult = 1)]
        [TestCase("192.168.0.0/16", "192.168.0.0/20", ExpectedResult = 1)]
        [TestCase("192.168.0.0/20", "192.168.0.0/16", ExpectedResult = -1)]
        [TestCase("ab:cd::/64", "ab:cd::/96", ExpectedResult = 1)]
        [TestCase("ab:cd::/96", "ab:cd::/64", ExpectedResult = -1)]
        [TestCase("0.0.0.0/0", "::/0", ExpectedResult = -1)]
        [TestCase("::/0", "0.0.0.0/0", ExpectedResult = 1)]
        [TestCase("0.0.0.0/32", "::/128", ExpectedResult = -1)]
        [TestCase("::/128", "0.0.0.0/32", ExpectedResult = 1)]
        public int CompareToTest(string x,
                                 string y)
        {
            // Arrange
            var thisSubnet = Subnet.Parse(x);

            Subnet other;
            if (!Subnet.TryParse(y, out other))
            {
                other = null;
            }

            // Act
            var result = thisSubnet.CompareTo(other);

            // Assert
            return result;
        }

        [TestCase("192.168.1.0/24")]
        [TestCase("16.8.14.12/28")]
        [TestCase("16.8.14.12/32")]
        [TestCase("::/128")]
        [TestCase("feed:beef::/120")]
        public void AddressesTest(string input)
        {
            // Arrange
            var subnet = Subnet.Parse(input);

            // Act
            var hosts = subnet.Addresses;

            // Assert
            CollectionAssert.AreEquivalent(subnet.Addresses, hosts);
        }

        [Test]
        public void ConstructNullIPAddressTest()
        {
            Assert.Throws<ArgumentNullException>(() => new Subnet((IPAddress) null, 42));
        }

        [Test]
        public void EqualsSameObjectExplicitTest()
        {
            // Arrange
            var subnet = new Subnet(IPAddress.Any, 16);

            // Act
            var result = subnet.Equals(subnet);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void EqualsSameObjectTest()
        {
            // Arrange
            var subnet = new Subnet(IPAddress.Any, 16);

            // Act
            var result = subnet.Equals((object) subnet);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void FromNetMaskAddressNullTest()
        {
            Assert.Throws<ArgumentNullException>(() => Subnet.FromNetMask(null, IPAddress.Any));
        }

        [Test]
        public void FromNetMaskInvalidNetMaskTest()
        {
            Assert.Throws<InvalidOperationException>(() => Subnet.FromNetMask(IPAddress.Any, IPAddress.IPv6Any));
        }

        [Test]
        public void FromNetMaskIPv6AddressTest()
        {
            Assert.Throws<InvalidOperationException>(() => Subnet.FromNetMask(IPAddress.IPv6Any, IPAddress.Any));
        }

        [Test]
        public void FromNetMaskNetMaskNullTest()
        {
            Assert.Throws<ArgumentNullException>(() => Subnet.FromNetMask(IPAddress.Any, null));
        }

        [Test]
        public void IllegalRoutePrefixTest()
        {
            // arrange
            var ip = IPAddress.Parse("192.168.3.0");
            const int routePrefix = -1;

            var subnetString = string.Format("{0}/{1}", ip, routePrefix);

            // act
            Subnet subnet;
            var tryParse = Subnet.TryParse(subnetString, out subnet);

            // assert
            Assert.IsNull(subnet);
            Assert.IsFalse(tryParse);
        }

        [Test]
        public void ImplementationTest()
        {
            Assert.That(typeof (IIPAddressRange).IsAssignableFrom(typeof (Subnet)));
            Assert.That(typeof (IComparable<Subnet>).IsAssignableFrom(typeof (Subnet)));
            Assert.That(typeof (IEquatable<Subnet>).IsAssignableFrom(typeof (Subnet)));
        }

        [Test]
        public void IPv4BasicSubnetTest()
        {
            // arrange
            var ip = IPAddress.Parse("192.168.3.0");
            const int routePrefix = 11;

            var subnetString = string.Format("{0}/{1}", ip, routePrefix);

            // act
            Subnet subnet;
            var parseSuccess = Subnet.TryParse(subnetString, out subnet);

            // assert
            Assert.IsTrue(parseSuccess);

            Assert.AreEqual(IPAddress.Parse("192.160.0.0"), subnet.NetworkPrefixAddress);
            Assert.AreEqual(routePrefix, subnet.RoutingPrefix);

            Assert.AreEqual(IPAddress.Parse("255.224.0.0"), subnet.Netmask);

            Assert.AreEqual(IPAddress.Parse("192.160.0.0"), subnet.NetworkPrefixAddress);
            Assert.AreEqual(IPAddress.Parse("192.191.255.255"), subnet.BroadcastAddress);

            Assert.AreEqual((BigInteger) 2097152, subnet.Length);
            Assert.AreEqual((BigInteger) 2097150, subnet.UsableHostAddressCount);

            Assert.IsTrue(subnet.Contains(subnet.NetworkPrefixAddress));
            Assert.IsTrue(subnet.Contains(subnet.BroadcastAddress));
            Assert.IsTrue(subnet.Contains(IPAddress.Parse("192.168.0.3")));

            Assert.IsFalse(subnet.Contains(IPAddress.Parse("255.255.255.255")));
            Assert.IsFalse(subnet.Contains(IPAddress.Parse("0.0.0.0")));
            Assert.IsFalse(subnet.IsSingleIP);
        }

        [Test]
        public void Ipv4EqualityTest()
        {
            // arrange
            var ip = IPAddress.Parse("192.168.3.0");
            const int routePrefix = 8;

            var subnetString = string.Format("{0}/{1}", ip, routePrefix);

            // act
            var subnetAlpha = Subnet.Parse(subnetString);
            var subnetBeta = Subnet.Parse(subnetString);
            var subnetGamma = Subnet.Parse("2001:0db8:85a3:0042:1000:8a2e:0370:7334/8");

            // assert
            Assert.IsTrue(subnetAlpha.Equals(subnetBeta));
            Assert.IsTrue(subnetBeta.Equals(subnetAlpha));

            Assert.IsFalse(subnetAlpha.Equals(subnetGamma));
        }

        [Test]
        public void IPv4FromIPsFullRangeSubnetTest()
        {
            var minIPAddress = IPAddress.Parse("0.0.0.0");
            var maxIPAddress = IPAddress.Parse("255.255.255.255");

            // act
            var subnet = new Subnet(minIPAddress, maxIPAddress);

            // assert
            Assert.AreEqual(0, subnet.RoutingPrefix);
            Assert.AreEqual(minIPAddress, subnet.Netmask);

            Assert.AreEqual(minIPAddress, subnet.NetworkPrefixAddress);
            Assert.AreEqual(maxIPAddress, subnet.BroadcastAddress);

            Assert.IsFalse(subnet.IsSingleIP);
        }

        [Test]
        public void IPv4FromIPsSubnetTest()
        {
            // act
            var subnet = new Subnet(IPAddress.Parse("192.168.3.25"), IPAddress.Parse("192.148.3.74"));

            // assert
            Assert.AreEqual(10, subnet.RoutingPrefix);
            Assert.AreEqual(IPAddress.Parse("255.192.0.0"), subnet.Netmask);

            Assert.AreEqual(IPAddress.Parse("192.128.0.0"), subnet.NetworkPrefixAddress);
            Assert.AreEqual(IPAddress.Parse("192.191.255.255"), subnet.BroadcastAddress);

            Assert.IsFalse(subnet.IsSingleIP);
        }

        [Test]
        public void IPv4FromSameIPSubnetTest()
        {
            var ipAddress = IPAddress.Parse("192.168.3.25");

            // act

            var subnet = new Subnet(ipAddress, ipAddress);

            // assert
            Assert.AreEqual(32, subnet.RoutingPrefix);
            Assert.AreEqual(IPAddress.Parse("255.255.255.255"), subnet.Netmask);

            Assert.AreEqual(ipAddress, subnet.NetworkPrefixAddress);
            Assert.AreEqual(ipAddress, subnet.BroadcastAddress);

            Assert.IsTrue(subnet.IsSingleIP);
        }

        [Test]
        public void IPv4FromSubnetNoPrefixTest()
        {
            // arrange
            var ip = IPAddress.Parse("0.0.0.0");

            // act
            var subnet = new Subnet(ip);

            // assert
            Assert.AreEqual(new Subnet(ip, 32), subnet);
            Assert.AreEqual(32, subnet.RoutingPrefix);
            Assert.AreEqual(ip, subnet.NetworkPrefixAddress);
            Assert.AreEqual(ip, subnet.BroadcastAddress);
            Assert.IsTrue(subnet.IsSingleIP);
        }

        [Test]
        public void IPv4IllegalRoutePrefixTest()
        {
            // arrange
            var ip = IPAddress.Parse("192.168.3.0");
            const int routePrefix = 33;

            var subnetString = string.Format("{0}/{1}", ip, routePrefix);

            // act
            Subnet subnet;
            var tryParse = Subnet.TryParse(subnetString, out subnet);

            // assert
            Assert.IsNull(subnet);
            Assert.IsFalse(tryParse);
        }

        [Test]
        public void IPv4LengthTest([Range(0, 32)] int routePrefix)
        {
            // Arrange
            var subnet = new Subnet(IPAddress.Any, routePrefix);

            // Act
            var length = subnet.Length;
            var hostAddressCount = subnet.Length;

            var expectedLength = BigInteger.Pow(2, 32 - routePrefix);

            // Assert
            Assert.AreEqual(expectedLength, length);
            Assert.AreEqual(expectedLength, hostAddressCount);
        }

        [Test]
        public void IPv4SubnetDoesNotContainIPv6Test()
        {
            // arrange
            var subnet = Subnet.Parse("0.0.0.0/0");

            // act

            // assert
            Assert.IsFalse(subnet.Contains(IPAddress.Parse("::")));
        }

        [Test]
        public void IPv4SubnetToStringTest()
        {
            // arrange
            var ip = IPAddress.Parse("192.168.3.0");
            const int routePrefix = 11;

            var subnetString = string.Format("{0}/{1}", ip, routePrefix);

            // act
            var subnet = Subnet.Parse(subnetString);

            // assert
            Assert.AreEqual("192.160.0.0/11", subnet.ToString());
        }

        [Test]
        public void IPv6BasicSubnetTest()
        {
            // arrange
            var ip = IPAddress.Parse("2001:0db8:85a3:0042:1000:8a2e:0370:7334");
            const int routePrefix = 107;

            var subnetString = string.Format("{0}/{1}", ip, routePrefix);

            // act
            Subnet subnet;
            var parseSuccess = Subnet.TryParse(subnetString, out subnet);

            // assert
            Assert.IsTrue(parseSuccess);

            Assert.AreEqual(IPAddress.Parse("2001:db8:85a3:42:1000:8a2e:360:0"), subnet.NetworkPrefixAddress);
            Assert.AreEqual(routePrefix, subnet.RoutingPrefix);

            Assert.AreEqual(IPAddress.Parse("FFFF:FFFF:FFFF:FFFF:FFFF:FFFF:FFE0:0000"), subnet.Netmask);

            Assert.AreEqual(IPAddress.Parse("2001:db8:85a3:42:1000:8a2e:360:0"), subnet.NetworkPrefixAddress);
            Assert.AreEqual(IPAddress.Parse("2001:db8:85a3:42:1000:8a2e:37f:FFFF"), subnet.BroadcastAddress);

            Assert.AreEqual((BigInteger) 2097152, subnet.Length);
            Assert.AreEqual((BigInteger) 2097150, subnet.UsableHostAddressCount);

            Assert.IsTrue(subnet.Contains(subnet.NetworkPrefixAddress));
            Assert.IsTrue(subnet.Contains(subnet.BroadcastAddress));
            Assert.IsTrue(subnet.Contains(IPAddress.Parse("2001:db8:85a3:42:1000:8a2e:360:5")));

            Assert.IsFalse(subnet.Contains(IPAddress.Parse("FFFF:FFFF:FFFF:FFFF:FFFF:FFFF:FFFF:FFFF")));
            Assert.IsFalse(subnet.Contains(IPAddress.Parse("0000:0000:0000:0000:0000:0000:0000:0000")));

            Assert.IsFalse(subnet.IsSingleIP);
        }

        [Test]
        public void Ipv6EqualityTest()
        {
            // arrange
            var ip = IPAddress.Parse("2001:0db8:85a3:0042:1000:8a2e:0370:7334");
            const int routePrefix = 107;

            var subnetString = string.Format("{0}/{1}", ip, routePrefix);

            // act
            var subnetAlpha = Subnet.Parse(subnetString);
            var subnetBeta = Subnet.Parse(subnetString);
            var subnetGamma = Subnet.Parse("2001:0db8:85a3:0042:1000:8a2e:0370:7334/22");

            // assert
            Assert.IsTrue(subnetAlpha.Equals(subnetBeta));
            Assert.IsTrue(subnetBeta.Equals(subnetAlpha));

            Assert.IsFalse(subnetAlpha.Equals(subnetGamma));
        }

        [Test]
        public void IPv6FromIPsFullRangeSubnetTest()
        {
            var minIPAddress = IPAddress.Parse("::");
            var maxIPAddress = IPAddress.Parse("FFFF:FFFF:FFFF:FFFF:FFFF:FFFF:FFFF:FFFF");

            // act
            var subnet = new Subnet(minIPAddress, maxIPAddress);

            // assert
            Assert.AreEqual(0, subnet.RoutingPrefix);
            Assert.AreEqual(minIPAddress, subnet.Netmask);

            Assert.AreEqual(minIPAddress, subnet.NetworkPrefixAddress);
            Assert.AreEqual(maxIPAddress, subnet.BroadcastAddress);

            Assert.IsFalse(subnet.IsSingleIP);
        }

        [Test]
        public void IPv6FromIPsSubnetTest()
        {
            // act
            var subnet = new Subnet(IPAddress.Parse("2001:0db8:85a3:0042:1000:8a2e:0370:7334"), IPAddress.Parse("2001:0db8:85a3:0042:0010:8aee:0370:7334"));

            // assert
            Assert.AreEqual(67, subnet.RoutingPrefix);
            Assert.AreEqual(IPAddress.Parse("ffff:ffff:ffff:ffff:e000::"), subnet.Netmask);

            Assert.AreEqual(IPAddress.Parse("2001:db8:85a3:42::"), subnet.NetworkPrefixAddress);
            Assert.AreEqual(IPAddress.Parse("2001:db8:85a3:42:1fff:ffff:ffff:ffff"), subnet.BroadcastAddress);

            Assert.IsFalse(subnet.IsSingleIP);
        }

        [Test]
        public void IPv6FromSameIPSubnetTest()
        {
            var ipAddress = IPAddress.Parse("2001:0db8:85a3:0042:1000:8a2e:0370:7334");

            // act
            var subnet = new Subnet(ipAddress, ipAddress);

            // assert
            Assert.AreEqual(128, subnet.RoutingPrefix);
            Assert.AreEqual(IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff"), subnet.Netmask);

            Assert.AreEqual(ipAddress, subnet.NetworkPrefixAddress);
            Assert.AreEqual(ipAddress, subnet.BroadcastAddress);

            Assert.IsTrue(subnet.IsSingleIP);
        }

        [Test]
        public void IPv6FromSubnetNoPrefixTest()
        {
            // arrange
            var ip = IPAddress.Parse("::");

            // act
            var subnet = new Subnet(ip);

            // assert
            Assert.AreEqual(new Subnet(ip, 128), subnet);
            Assert.AreEqual(128, subnet.RoutingPrefix);
            Assert.AreEqual(ip, subnet.NetworkPrefixAddress);
            Assert.AreEqual(ip, subnet.BroadcastAddress);
            Assert.IsTrue(subnet.IsSingleIP);
        }

        [Test]
        public void IPv6IllegalRoutePrefixTest()
        {
            // arrange
            var ip = IPAddress.Parse("2001:0db8:85a3:0042:1000:8a2e:0370:7334");
            const int routePrefix = 129;

            var subnetString = string.Format("{0}/{1}", ip, routePrefix);

            // act
            Subnet subnet;
            var tryParse = Subnet.TryParse(subnetString, out subnet);

            // assert
            Assert.IsNull(subnet);
            Assert.IsFalse(tryParse);
        }

        [Test]
        public void IPv6LengthTest([Range(0, 128)] int routePrefix)
        {
            // Arrange
            var subnet = new Subnet(IPAddress.IPv6Any, routePrefix);

            // Act
            var length = subnet.Length;
            var hostAddressCount = subnet.Length;
            var expectedLength = BigInteger.Pow(2, 128 - routePrefix);

            // Assert
            Assert.AreEqual(expectedLength, length);
            Assert.AreEqual(expectedLength, hostAddressCount);
        }

        [Test]
        public void IPv6SubnetDoesNotContainIP46Test()
        {
            // arrange
            var subnet = Subnet.Parse("::/0");

            // act

            // assert
            Assert.IsFalse(subnet.Contains(IPAddress.Parse("0.0.0.0")));
        }

        [Test]
        public void IPv6SubnetToStringTest()
        {
            // arrange
            const string ipString = "2001:0DB8:85A3:0042:1000:8A2E:0370:7334";
            const int routePrefix = 107;

            var subnetString = string.Format("{0}/{1}", ipString, routePrefix);

            // act
            var subnet = Subnet.Parse(subnetString);

            // assert
            Assert.AreEqual("2001:db8:85a3:42:1000:8a2e:360:0/107", subnet.ToString());
        }

        [Test]
        public void ParseFromStringFailTest()
        {
            Assert.Throws<ArgumentException>(() => Subnet.Parse("potato"));
        }

        [Test]
        public void ParseIllegalIpTest()
        {
            // arrange
            const int routePrefix = 11;

            var subnetString = string.Format("{0}{1}", "banana", routePrefix);

            // act
            Subnet subnet;
            var parseSuccess = Subnet.TryParse(subnetString, out subnet);

            Assert.IsFalse(parseSuccess);
            Assert.IsNull(subnet);
        }

        [Test]
        public void ParseIllegalRoutePrefixTest()
        {
            // arrange
            var ip = IPAddress.Parse("192.168.3.0");

            var subnetString = string.Format("{0}{1}", ip, "banana");

            // act
            Subnet subnet;
            var parseSuccess = Subnet.TryParse(subnetString, out subnet);

            Assert.IsFalse(parseSuccess);
            Assert.IsNull(subnet);
        }

        [Test]
        public void ParseIpWhitespaceTest()
        {
            // arrange
            const int routePrefix = 11;

            var subnetString = string.Format("{0}/{1}", string.Empty, routePrefix);

            // act
            Subnet subnet;
            var parseSuccess = Subnet.TryParse(subnetString, out subnet);

            Assert.IsFalse(parseSuccess);
            Assert.IsNull(subnet);
        }

        [Test]
        public void ParseNoRoutePrefixIPv4Test()
        {
            // arrange
            var ip = IPAddress.Parse("192.168.3.0");

            var subnetString = string.Format("{0}", ip);

            // act
            Subnet subnet;
            var parseSuccess = Subnet.TryParse(subnetString, out subnet);

            Assert.IsTrue(parseSuccess);
            Assert.AreEqual(ip, subnet.NetworkPrefixAddress);
            Assert.AreEqual(ip, subnet.BroadcastAddress);
            Assert.AreEqual(32, subnet.RoutingPrefix);
        }

        [Test]
        public void ParseNoRoutePrefixIPv6Test()
        {
            // arrange
            var ip = IPAddress.Parse("abcd::0123");

            var subnetString = string.Format("{0}", ip);

            // act
            Subnet subnet;
            var parseSuccess = Subnet.TryParse(subnetString, out subnet);

            Assert.IsTrue(parseSuccess);
            Assert.AreEqual(ip, subnet.NetworkPrefixAddress);
            Assert.AreEqual(ip, subnet.BroadcastAddress);
            Assert.AreEqual(128, subnet.RoutingPrefix);
        }

        [Test]
        public void ParseRoutePrefixWhiteSpaceTest()
        {
            // arrange
            var ip = IPAddress.Parse("192.168.3.0");

            var subnetString = string.Format("{0}/{1}", ip, string.Empty);

            // act
            Subnet subnet;
            var parseSuccess = Subnet.TryParse(subnetString, out subnet);

            Assert.IsFalse(parseSuccess);
            Assert.IsNull(subnet);
        }

        [Test]
        public void SetHeadThrowsNotSupportedExceptionTest()
        {
            // arrange
            var subnet = new Subnet(IPAddress.Any, 16);

            // assert
            Assert.Throws<NotSupportedException>(() => subnet.Head = IPAddress.Any);
        }

        [Test]
        public void SetTailThrowsNotSupportedExceptionTest()
        {
            // arrange
            var subnet = new Subnet(IPAddress.Any, 16);

            // assert
            Assert.Throws<NotSupportedException>(() => subnet.Tail = IPAddress.Any);
        }

        [Test]
        public void SubnetContainsNullIPTest()
        {
            // arrange
            var subnet = Subnet.Parse("192.168.3.0/16");

            // act
            Assert.IsFalse(subnet.Contains((IPAddress) null));
        }

        [Test]
        public void SubnetContainsSubnetTest()
        {
            // arrange
            var subnet = Subnet.Parse("192.168.0.0/16");

            // act
            Assert.IsTrue(subnet.Contains(Subnet.Parse("192.168.0.0/16")));
            Assert.IsTrue(subnet.Contains(Subnet.Parse("192.168.0.0/32")));
            Assert.IsFalse(subnet.Contains(Subnet.Parse("192.168.0.0/8")));
            Assert.IsFalse(subnet.Contains(null));
        }

        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Core.Domain.Helpers.Subnet", Justification = "return value not set due to expected exception")]
        [Test]
        public void SubnetFromIpsMixedNetworkAltTest()
        {
            // Assert
            Assert.Throws<ArgumentException>(() => new Subnet(IPAddress.Parse("192.168.3.25"), IPAddress.Parse("2001:0db8:85a3:0042:1000:8a2e:0370:7334")));
        }

        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Core.Domain.Helpers.Subnet", Justification = "return value not set due to expected exception")]
        [Test]
        public void SubnetFromIpsMixedNetworkTest()
        {
            // act
            Assert.Throws<ArgumentException>(() => new Subnet(IPAddress.Parse("2001:0db8:85a3:0042:1000:8a2e:0370:7334"), IPAddress.Parse("192.168.3.25")));
        }

        [Test]
        public void TryIPv4FromPartial0OctetIP()
        {
            // arrange
            const string ipPartial = "";

            // act
            Subnet subnet;
            Assert.IsFalse(Subnet.TryIPv4FromPartial(ipPartial, out subnet));
            Assert.IsNull(subnet);
        }

        [Test]
        public void TryIPv4FromPartial1OctetIP()
        {
            // arrange
            const string ipPartial = "192";

            // act
            Subnet subnet;
            var success = Subnet.TryIPv4FromPartial(ipPartial, out subnet);

            // assert
            Assert.IsTrue(success);
            Assert.IsNotNull(subnet);
            Assert.AreEqual("192.0.0.0", subnet.NetworkPrefixAddress.ToString());
            Assert.AreEqual(8, subnet.RoutingPrefix);
        }

        [Test]
        public void TryIPv4FromPartial2OctetIP()
        {
            // arrange
            const string ipPartial = "192.168";

            // act
            Subnet subnet;
            var success = Subnet.TryIPv4FromPartial(ipPartial, out subnet);

            // assert
            Assert.IsTrue(success);
            Assert.IsNotNull(subnet);
            Assert.AreEqual("192.168.0.0", subnet.NetworkPrefixAddress.ToString());
            Assert.AreEqual(16, subnet.RoutingPrefix);
        }

        [Test]
        public void TryIPv4FromPartial3OctetIP()
        {
            // arrange
            const string ipPartial = "192.168.1";

            // act
            Subnet subnet;
            var success = Subnet.TryIPv4FromPartial(ipPartial, out subnet);

            // assert
            Assert.IsTrue(success);
            Assert.IsNotNull(subnet);
            Assert.AreEqual("192.168.1.0", subnet.NetworkPrefixAddress.ToString());
            Assert.AreEqual(24, subnet.RoutingPrefix);
        }

        [Test]
        public void TryIPv4FromPartial4OctetIP()
        {
            // arrange
            const string ipPartial = "192.168.1.1";

            // act
            Subnet subnet;
            var success = Subnet.TryIPv4FromPartial(ipPartial, out subnet);

            // assert
            Assert.IsTrue(success);
            Assert.IsNotNull(subnet);
            Assert.AreEqual("192.168.1.1", subnet.NetworkPrefixAddress.ToString());
            Assert.AreEqual(32, subnet.RoutingPrefix);
        }

        [Test]
        public void TryIPv4FromPartial5OctetIP()
        {
            // arrange
            const string ipPartial = "192.168.0.1.5";

            // act
            Subnet subnet;
            Assert.IsFalse(Subnet.TryIPv4FromPartial(ipPartial, out subnet));
            Assert.IsNull(subnet);
        }

        [Test]
        public void TryIPv4FromPartialStripsTrailingDot()
        {
            // arrange
            const string ipPartial = "192.168.";

            // act
            Subnet subnet;
            var success = Subnet.TryIPv4FromPartial(ipPartial, out subnet);

            // assert
            Assert.IsTrue(success);
            Assert.IsNotNull(subnet);
            Assert.AreEqual("192.168.0.0", subnet.NetworkPrefixAddress.ToString());
            Assert.AreEqual(16, subnet.RoutingPrefix);
        }

        [Test]
        public void ZeroUsableHostSubnetTest()
        {
            // arrange
            var subnet = Subnet.Parse("192.168.3.0/32");

            // assert
            Assert.AreEqual((BigInteger) 0, subnet.UsableHostAddressCount);
        }
    }
}
