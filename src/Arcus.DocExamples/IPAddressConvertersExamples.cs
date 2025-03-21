﻿using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using Arcus.Converters;
using Gulliver;
using Xunit;
using Xunit.Abstractions;

namespace Arcus.DocExamples
{
    public class IPAddressConvertersExamples
    {
        #region Setup / Teardown

        public IPAddressConvertersExamples(ITestOutputHelper output)
        {
            this._output = output;
        }

        private readonly ITestOutputHelper _output;

        #endregion

        [Fact]
        public void NetmaskToCidrRoutePrefix_Example()
        {
            // equivalent byte value of 255.255.255.255 or 2^32
            var maxIPv4Bytes = Enumerable.Repeat((byte)0xFF, 4).ToArray();

            // build all valid net masks
            var allNetMasks = Enumerable
                .Range(7, 10)
                .Select(i => maxIPv4Bytes.ShiftBitsLeft(32 - i)) // use Gulliver to shift bits of byte array
                .Select(b => new IPAddress(b))
                .ToArray();

            var sb = new StringBuilder();

            foreach (var netmask in allNetMasks)
            {
                var routePrefix = netmask.NetmaskToCidrRoutePrefix();

                sb.Append(routePrefix)
                    .Append('\t')
                    .AppendFormat(CultureInfo.InvariantCulture, "{0,-15}", netmask)
                    .Append('\t')
                    .Append(netmask.GetAddressBytes().ToString("b")) // using Gulliver to print bytes as bits
                    .AppendLine();
            }

            this._output.WriteLine(sb.ToString());
        }

        [Fact]
        public void ToBase85String_Example()
        {
            var addresses = new[]
            {
                "::",
                "::ffff",
                "1080:0:0:0:8:800:200C:417A", // specific example from RFC 1924
                "ffff::",
                "ffff::0102:0304",
                "ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff",
            }
                .Select(IPAddress.Parse)
                .ToArray();

            var sb = new StringBuilder();

            foreach (var address in addresses)
            {
                var base85String = address.ToBase85String();

                sb.AppendFormat(CultureInfo.InvariantCulture, "{0,-40}", address)
                    .Append("\t=>\t")
                    .Append(base85String)
                    .AppendLine();
            }

            this._output.WriteLine(sb.ToString());
        }

        [Fact]
        public void ToDottedQuadString_Example()
        {
            var addresses = new[]
            {
                "::",
                "::ffff",
                "a:b:c::ff00:ff",
                "ffff::",
                "ffff::0102:0304",
                "ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff",
            }
                .Select(IPAddress.Parse)
                .ToArray();

            var sb = new StringBuilder();

            foreach (var address in addresses)
            {
                var dottedQuadString = address.ToDottedQuadString();

                sb.AppendFormat(CultureInfo.InvariantCulture, "{0,-40}", address)
                    .Append("\t=>\t")
                    .Append(dottedQuadString)
                    .AppendLine();
            }

            this._output.WriteLine(sb.ToString());
        }

        [Fact]
        public void ToHexString_Example()
        {
            var addresses = new[]
            {
                "::",
                "::ffff",
                "10.1.1.1",
                "192.168.1.1",
                "255.255.255.255",
                "ffff::",
                "ffff::0102:0304",
                "ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff",
            }
                .Select(IPAddress.Parse)
                .ToArray();

            var sb = new StringBuilder();

            foreach (var address in addresses)
            {
                var hexString = address.ToHexString();

                sb.AppendFormat(CultureInfo.InvariantCulture, "{0,-40}", address)
                    .Append("\t=>\t")
                    .Append(hexString)
                    .AppendLine();
            }

            this._output.WriteLine(sb.ToString());
        }

        [Fact]
        public void ToNumericString_Example()
        {
            var addresses = new[]
            {
                "::",
                "::ffff",
                "10.1.1.1",
                "192.168.1.1",
                "255.255.255.255",
                "ffff::",
                "ffff::0102:0304",
                "ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff",
            }
                .Select(IPAddress.Parse)
                .ToArray();

            var sb = new StringBuilder();

            foreach (var address in addresses)
            {
                var numericString = address.ToNumericString();

                sb.AppendFormat(CultureInfo.InvariantCulture, "{0,-40}", address)
                    .Append("\t=>\t")
                    .Append(numericString)
                    .AppendLine();
            }

            this._output.WriteLine(sb.ToString());
        }

        [Fact]
        public void ToUncompressedString_Example()
        {
            var addresses = new[] { "::", "::ffff", "10.1.1.1", "192.168.1.1", "255.255.255.255", "ffff::", "ffff::0102:0304" }
                .Select(IPAddress.Parse)
                .ToArray();

            var sb = new StringBuilder();

            foreach (var address in addresses)
            {
                var uncompressedString = address.ToUncompressedString();

                sb.AppendFormat(CultureInfo.InvariantCulture, "{0,-40}", address)
                    .Append("\t=>\t")
                    .Append(uncompressedString)
                    .AppendLine();
            }

            this._output.WriteLine(sb.ToString());
        }
    }
}
