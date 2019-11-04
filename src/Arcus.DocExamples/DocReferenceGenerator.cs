using System.Text;
using Arcus.Utilities;
using Gulliver;
using Xunit;
using Xunit.Abstractions;

namespace Arcus.DocExamples
{
    public class DocReferenceGenerator
    {
        #region Setup / Teardown

        public DocReferenceGenerator(ITestOutputHelper output)
        {
            this.output = output;
        }

        private readonly ITestOutputHelper output;

        #endregion

        [Fact]
        public void IPv4CirdRefGen()
        {
            var sb = new StringBuilder();

            sb.Append("CIDR")
              .Append(',')
              .Append("Network Prefix Address")
              .Append(',')
              .Append("Route Prefix")
              .Append(',')
              .Append("Netmask")
              .Append(',')
              .Append("Netmask (bits)")
              .Append(',')
              .Append("Address Count")
              .Append(',')
              .Append("Address Count 2^n")
              .AppendLine();

            for (var i = 32; i >= 0; i--)
            {
                var subnet = new Subnet(IPAddressUtilities.IPv4MaxAddress, i);

                sb.Append(subnet)
                  .Append(',')
                  .Append(subnet.NetworkPrefixAddress)
                  .Append(',')
                  .Append(subnet.RoutingPrefix)
                  .Append(',')
                  .Append(subnet.Netmask)
                  .Append(',')
                  .Append(subnet.Netmask.GetAddressBytes()
                                .ToString("b"))
                  .Append(',')
                  .Append(subnet.Length)
                  .Append(',')
                  .Append(32 - i)
                  .AppendLine();
            }

            this.output.WriteLine(sb.ToString());
        }

        [Fact]
        public void IPv6CirdRefGen()
        {
            var sb = new StringBuilder();

            sb.Append("CIDR")
              .Append(',')
              .Append("Network Prefix Address")
              .Append(',')
              .Append("Route Prefix")
              .Append(',')
              .Append("Address Count")
              .Append(',')
              .Append("Address Count 2^n")
              .AppendLine();

            for (var i = 128; i >= 0; i--)
            {
                var subnet = new Subnet(IPAddressUtilities.IPv6MaxAddress, i);

                sb.Append(subnet)
                  .Append(',')
                  .Append(subnet.NetworkPrefixAddress)
                  .Append(',')
                  .Append(subnet.RoutingPrefix)
                  .Append(',')
                  .Append(subnet.Length)
                  .Append(',')
                  .Append(128 - i)
                  .AppendLine();
            }

            this.output.WriteLine(sb.ToString());
        }
    }
}
