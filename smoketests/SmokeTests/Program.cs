// Arcus NuGet package smoke tests.
//
// This program is a consumer of the *packed NuGet package* (not a project reference).
// Its only job is to prove that each target-framework asset (netstandard2.0 via net48,
// net8.0, net9.0, net10.0) loads correctly and that the public API behaves at runtime.
//
// Run via smoke-tests/run-smoke-tests.{sh,ps1} — those scripts pack the library first.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using Arcus;
using Arcus.Comparers;
using Arcus.Converters;
using Arcus.Math;
using Arcus.Utilities;

var tfm = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription;
Console.WriteLine($"=== Arcus Smoke Tests ({tfm}) ===");
Console.WriteLine();

var passed = 0;
var failed = 0;

void Check(string name, Action test)
{
    try
    {
        test();
        Console.WriteLine($"  [PASS] {name}");
        passed++;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  [FAIL] {name}: {ex.Message}");
        failed++;
    }
}

void Require(bool condition, string message)
{
    if (!condition)
        throw new InvalidOperationException(message);
}

void RequireEqual<T>(T expected, T actual, string field)
{
    if (!Equals(expected, actual))
        throw new InvalidOperationException($"{field}: expected '{expected}', got '{actual}'");
}

// ---------------------------------------------------------------------------
// Subnet — IPv4
// ---------------------------------------------------------------------------
Console.WriteLine("Subnet (IPv4)");

Check(
    "Parse CIDR notation",
    () =>
    {
        var s = Subnet.Parse("192.168.0.0/24");
        RequireEqual(
            IPAddress.Parse("192.168.0.0"),
            s.NetworkPrefixAddress,
            "NetworkPrefixAddress"
        );
        RequireEqual(IPAddress.Parse("192.168.0.255"), s.BroadcastAddress, "BroadcastAddress");
        RequireEqual(24, s.RoutingPrefix, "RoutingPrefix");
        RequireEqual(new BigInteger(256), s.Length, "Length");
        RequireEqual(new BigInteger(254), s.UsableHostAddressCount, "UsableHostAddressCount");
        Require(s.IsIPv4, "IsIPv4");
    }
);

Check(
    "Construct from address + routing prefix",
    () =>
    {
        var s = new Subnet(IPAddress.Parse("10.0.0.0"), 8);
        RequireEqual(IPAddress.Parse("10.0.0.0"), s.NetworkPrefixAddress, "NetworkPrefixAddress");
        RequireEqual(IPAddress.Parse("10.255.255.255"), s.BroadcastAddress, "BroadcastAddress");
        RequireEqual(8, s.RoutingPrefix, "RoutingPrefix");
    }
);

Check(
    "Construct from two addresses (smallest containing subnet)",
    () =>
    {
        var s = new Subnet(IPAddress.Parse("192.168.1.100"), IPAddress.Parse("192.168.1.200"));
        Require(s.Contains(IPAddress.Parse("192.168.1.100")), "Contains low bound");
        Require(s.Contains(IPAddress.Parse("192.168.1.200")), "Contains high bound");
    }
);

Check(
    "Netmask",
    () =>
    {
        var s = Subnet.Parse("10.0.0.0/8");
        RequireEqual(IPAddress.Parse("255.0.0.0"), s.Netmask, "Netmask");
    }
);

Check(
    "Contains IPAddress",
    () =>
    {
        var s = Subnet.Parse("172.16.0.0/12");
        Require(s.Contains(IPAddress.Parse("172.20.0.1")), "inside");
        Require(!s.Contains(IPAddress.Parse("172.32.0.1")), "outside");
    }
);

Check(
    "Contains Subnet",
    () =>
    {
        var outer = Subnet.Parse("10.0.0.0/8");
        var inner = Subnet.Parse("10.1.0.0/16");
        Require(outer.Contains(inner), "inner ⊆ outer");
        Require(!inner.Contains(outer), "outer ⊄ inner");
    }
);

Check(
    "Overlaps",
    () =>
    {
        var a = Subnet.Parse("192.168.0.0/24");
        var b = Subnet.Parse("192.168.0.128/25");
        Require(a.Overlaps(b), "overlapping subnets");
        var c = Subnet.Parse("10.0.0.0/8");
        Require(!a.Overlaps(c), "disjoint subnets");
    }
);

Check(
    "Touches (adjacent subnets)",
    () =>
    {
        var a = Subnet.Parse("192.168.0.0/25");
        var b = Subnet.Parse("192.168.0.128/25");
        Require(a.Touches(b), "adjacent /25 subnets touch");
    }
);

Check(
    "Enumerate (small /30 = 4 addresses)",
    () =>
    {
        var s = Subnet.Parse("192.168.1.0/30");
        var addresses = s.ToList();
        RequireEqual(4, addresses.Count, "address count");
        RequireEqual(IPAddress.Parse("192.168.1.0"), addresses[0], "first address");
        RequireEqual(IPAddress.Parse("192.168.1.3"), addresses[3], "last address");
    }
);

Check(
    "ContainsAnyPrivateAddresses",
    () =>
    {
        Require(Subnet.Parse("10.1.2.0/24").ContainsAnyPrivateAddresses(), "10.x/24 is private");
        Require(!Subnet.Parse("8.8.8.0/24").ContainsAnyPrivateAddresses(), "8.8.8.0/24 is public");
    }
);

Check(
    "ContainsAllPrivateAddresses",
    () =>
    {
        Require(Subnet.Parse("192.168.5.0/24").ContainsAllPrivateAddresses(), "all private");
    }
);

Check(
    "TryParse success and failure",
    () =>
    {
        Require(Subnet.TryParse("192.168.100.0/22", out var s), "valid CIDR parses");
        Require(s != null, "result not null");
        RequireEqual(22, s!.RoutingPrefix, "RoutingPrefix");
        Require(!Subnet.TryParse("not-an-ip", out _), "garbage input returns false");
    }
);

Check(
    "Deconstruct (address, prefix)",
    () =>
    {
        var s = Subnet.Parse("10.2.3.0/24");
        s.Deconstruct(out var addr, out var prefix);
        RequireEqual(IPAddress.Parse("10.2.3.0"), addr, "address");
        RequireEqual(24, prefix, "prefix");
    }
);

// ---------------------------------------------------------------------------
// Subnet — IPv6
// ---------------------------------------------------------------------------
Console.WriteLine("\nSubnet (IPv6)");

Check(
    "Parse IPv6 CIDR",
    () =>
    {
        var s = Subnet.Parse("2001:db8::/32");
        RequireEqual(AddressFamily.InterNetworkV6, s.Head.AddressFamily, "AddressFamily");
        RequireEqual(32, s.RoutingPrefix, "RoutingPrefix");
        Require(s.IsIPv6, "IsIPv6");
    }
);

Check(
    "Construct IPv6 from address + prefix",
    () =>
    {
        var s = new Subnet(IPAddress.Parse("fd00::"), 8);
        Require(s.Contains(IPAddress.Parse("fd00::1")), "Contains fd00::1");
        Require(s.IsIPv6, "IsIPv6");
    }
);

Check(
    "IPv6 Contains and Overlaps",
    () =>
    {
        var outer = Subnet.Parse("2001:db8::/32");
        var inner = Subnet.Parse("2001:db8:1::/48");
        Require(outer.Contains(inner), "inner ⊆ outer");
        Require(outer.Overlaps(inner), "overlaps");
    }
);

// ---------------------------------------------------------------------------
// IPAddressRange
// ---------------------------------------------------------------------------
Console.WriteLine("\nIPAddressRange");

Check(
    "Construct arbitrary range",
    () =>
    {
        var r = new IPAddressRange(IPAddress.Parse("10.0.0.5"), IPAddress.Parse("10.0.0.10"));
        RequireEqual(IPAddress.Parse("10.0.0.5"), r.Head, "Head");
        RequireEqual(IPAddress.Parse("10.0.0.10"), r.Tail, "Tail");
        RequireEqual(new BigInteger(6), r.Length, "Length");
    }
);

Check(
    "Contains IPAddress",
    () =>
    {
        var r = new IPAddressRange(IPAddress.Parse("192.168.0.1"), IPAddress.Parse("192.168.0.50"));
        Require(r.Contains(IPAddress.Parse("192.168.0.25")), "inside");
        Require(!r.Contains(IPAddress.Parse("192.168.0.51")), "outside");
    }
);

Check(
    "Overlaps with Subnet",
    () =>
    {
        var r = new IPAddressRange(IPAddress.Parse("10.0.0.0"), IPAddress.Parse("10.0.0.100"));
        var s = Subnet.Parse("10.0.0.50/30");
        Require(r.Overlaps(s), "range overlaps subnet");
    }
);

// ---------------------------------------------------------------------------
// SubnetUtilities
// ---------------------------------------------------------------------------
Console.WriteLine("\nSubnetUtilities");

Check(
    "FewestConsecutiveSubnetsFor",
    () =>
    {
        var subnets = SubnetUtilities
            .FewestConsecutiveSubnetsFor(
                IPAddress.Parse("192.168.0.1"),
                IPAddress.Parse("192.168.0.10")
            )
            .ToList();
        Require(subnets.Count > 0, "at least one result subnet");
        Require(subnets.All(s => s.IsIPv4), "all results are IPv4");
        Require(subnets.First().Contains(IPAddress.Parse("192.168.0.1")), "covers left bound");
        Require(subnets.Last().Contains(IPAddress.Parse("192.168.0.10")), "covers right bound");
    }
);

Check(
    "PrivateIPAddressRangesList",
    () =>
    {
        Require(SubnetUtilities.PrivateIPAddressRangesList.Count > 0, "list not empty");
        Require(
            SubnetUtilities.PrivateIPAddressRangesList.Any(s =>
                s.Contains(IPAddress.Parse("10.1.2.3"))
            ),
            "10.1.2.3 is in private list"
        );
        Require(
            SubnetUtilities.PrivateIPAddressRangesList.Any(s =>
                s.Contains(IPAddress.Parse("192.168.0.1"))
            ),
            "192.168.0.1 is in private list"
        );
    }
);

Check(
    "LargestSubnet / SmallestSubnet",
    () =>
    {
        var subnets = new[]
        {
            Subnet.Parse("10.0.0.0/24"),
            Subnet.Parse("10.0.0.0/16"),
            Subnet.Parse("10.0.0.0/28"),
        };
        RequireEqual(16, SubnetUtilities.LargestSubnet(subnets).RoutingPrefix, "largest prefix");
        RequireEqual(28, SubnetUtilities.SmallestSubnet(subnets).RoutingPrefix, "smallest prefix");
    }
);

// ---------------------------------------------------------------------------
// IPAddressMath
// ---------------------------------------------------------------------------
Console.WriteLine("\nIPAddressMath");

Check(
    "Increment (default delta = 1)",
    () =>
    {
        var addr = IPAddress.Parse("192.168.0.1");
        RequireEqual(IPAddress.Parse("192.168.0.2"), addr.Increment(), "incremented by 1");
    }
);

Check(
    "Increment with explicit delta",
    () =>
    {
        var addr = IPAddress.Parse("10.0.0.0");
        RequireEqual(IPAddress.Parse("10.0.1.0"), addr.Increment(256), "incremented by 256");
    }
);

Check(
    "Increment negative delta (acts as decrement)",
    () =>
    {
        var addr = IPAddress.Parse("10.0.1.0");
        RequireEqual(IPAddress.Parse("10.0.0.255"), addr.Increment(-1), "decremented by 1");
    }
);

Check(
    "TryIncrement within bounds",
    () =>
    {
        Require(
            IPAddressMath.TryIncrement(IPAddress.Parse("10.0.0.1"), out var result),
            "TryIncrement succeeded"
        );
        RequireEqual(IPAddress.Parse("10.0.0.2"), result!, "result");
    }
);

Check(
    "IsGreaterThan / IsLessThan",
    () =>
    {
        var a = IPAddress.Parse("192.168.0.2");
        var b = IPAddress.Parse("192.168.0.1");
        Require(a.IsGreaterThan(b), "a > b");
        Require(b.IsLessThan(a), "b < a");
        Require(!a.IsLessThan(b), "a not < b");
    }
);

Check(
    "IsGreaterThanOrEqualTo / IsLessThanOrEqualTo",
    () =>
    {
        var a = IPAddress.Parse("10.0.0.5");
        Require(a.IsGreaterThanOrEqualTo(a), "a >= a");
        Require(a.IsLessThanOrEqualTo(a), "a <= a");
    }
);

Check(
    "IsEqualTo",
    () =>
    {
        var a = IPAddress.Parse("192.168.1.1");
        var b = IPAddress.Parse("192.168.1.1");
        Require(a.IsEqualTo(b), "equal addresses");
    }
);

Check(
    "IsBetween",
    () =>
    {
        var low = IPAddress.Parse("10.0.0.1");
        var mid = IPAddress.Parse("10.0.0.100");
        var high = IPAddress.Parse("10.0.0.200");
        Require(mid.IsBetween(low, high), "mid is between");
        Require(!low.IsBetween(mid, high), "low is not between mid and high");
        Require(low.IsBetween(low, high), "low is between (inclusive)");
    }
);

Check(
    "Max / Min",
    () =>
    {
        var a = IPAddress.Parse("192.168.1.5");
        var b = IPAddress.Parse("192.168.1.10");
        RequireEqual(b, IPAddressMath.Max(a, b), "max");
        RequireEqual(a, IPAddressMath.Min(a, b), "min");
    }
);

Check(
    "IsAtMin / IsAtMax",
    () =>
    {
        Require(IPAddressUtilities.IPv4MinAddress.IsAtMin(), "0.0.0.0 is at min");
        Require(IPAddressUtilities.IPv4MaxAddress.IsAtMax(), "255.255.255.255 is at max");
        Require(!IPAddress.Parse("10.0.0.1").IsAtMin(), "10.0.0.1 not at min");
    }
);

// ---------------------------------------------------------------------------
// IPAddressUtilities
// ---------------------------------------------------------------------------
Console.WriteLine("\nIPAddressUtilities");

Check(
    "IsIPv4 / IsIPv6",
    () =>
    {
        Require(IPAddress.Parse("192.168.0.1").IsIPv4(), "IsIPv4 returns true for v4");
        Require(IPAddress.Parse("::1").IsIPv6(), "IsIPv6 returns true for v6");
        Require(!IPAddress.Parse("::1").IsIPv4(), "IsIPv4 returns false for v6");
        Require(!IPAddress.Parse("10.0.0.1").IsIPv6(), "IsIPv6 returns false for v4");
    }
);

Check(
    "IsPrivate",
    () =>
    {
        Require(IPAddress.Parse("10.1.2.3").IsPrivate(), "10.x is private");
        Require(IPAddress.Parse("172.20.0.1").IsPrivate(), "172.20.x is private");
        Require(IPAddress.Parse("192.168.0.1").IsPrivate(), "192.168.x is private");
        Require(!IPAddress.Parse("8.8.8.8").IsPrivate(), "8.8.8.8 is not private");
    }
);

Check(
    "IPv4 min/max address constants",
    () =>
    {
        RequireEqual(IPAddress.Parse("0.0.0.0"), IPAddressUtilities.IPv4MinAddress, "IPv4Min");
        RequireEqual(
            IPAddress.Parse("255.255.255.255"),
            IPAddressUtilities.IPv4MaxAddress,
            "IPv4Max"
        );
    }
);

Check(
    "IsValidNetMask",
    () =>
    {
        Require(IPAddress.Parse("255.255.255.0").IsValidNetMask(), "255.255.255.0 valid");
        Require(IPAddress.Parse("255.0.0.0").IsValidNetMask(), "255.0.0.0 valid");
        Require(!IPAddress.Parse("255.255.0.128").IsValidNetMask(), "255.255.0.128 invalid");
    }
);

Check(
    "ValidAddressFamilies",
    () =>
    {
        Require(
            IPAddressUtilities.ValidAddressFamilies.Contains(AddressFamily.InterNetwork),
            "InterNetwork valid"
        );
        Require(
            IPAddressUtilities.ValidAddressFamilies.Contains(AddressFamily.InterNetworkV6),
            "InterNetworkV6 valid"
        );
    }
);

Check(
    "ParseFromHexString (IPv4)",
    () =>
    {
        var addr = IPAddressUtilities.ParseFromHexString("c0a80001", AddressFamily.InterNetwork);
        RequireEqual(IPAddress.Parse("192.168.0.1"), addr, "parsed address");
    }
);

// ---------------------------------------------------------------------------
// IPAddressConverters
// ---------------------------------------------------------------------------
Console.WriteLine("\nIPAddressConverters");

Check(
    "NetmaskToCidrRoutePrefix",
    () =>
    {
        RequireEqual(24, IPAddress.Parse("255.255.255.0").NetmaskToCidrRoutePrefix(), "/24");
        RequireEqual(8, IPAddress.Parse("255.0.0.0").NetmaskToCidrRoutePrefix(), "/8");
        RequireEqual(32, IPAddress.Parse("255.255.255.255").NetmaskToCidrRoutePrefix(), "/32");
    }
);

Check(
    "ToHexString",
    () =>
    {
        var hex = IPAddress.Parse("192.168.0.1").ToHexString();
        Require(!string.IsNullOrEmpty(hex), "hex not empty");
        // Convert.ToHexString produces uppercase; compare case-insensitively
        RequireEqual("c0a80001", hex.ToLowerInvariant(), "hex value (lowercase)");
    }
);

Check(
    "ToNumericString",
    () =>
    {
        var numeric = IPAddress.Parse("0.0.1.0").ToNumericString();
        RequireEqual("256", numeric, "numeric string");
    }
);

// ---------------------------------------------------------------------------
// Comparers
// ---------------------------------------------------------------------------
Console.WriteLine("\nComparers");

Check(
    "DefaultIPAddressComparer",
    () =>
    {
        var comparer = DefaultIPAddressComparer.Instance;
        var a = IPAddress.Parse("10.0.0.1");
        var b = IPAddress.Parse("10.0.0.2");
        Require(comparer.Compare(a, b) < 0, "a < b");
        Require(comparer.Compare(b, a) > 0, "b > a");
        Require(comparer.Compare(a, a) == 0, "a == a");

        var sorted = new List<IPAddress> { b, a };
        sorted.Sort(comparer);
        RequireEqual(a, sorted[0], "sorted[0]");
    }
);

Check(
    "DefaultIIPAddressRangeComparer",
    () =>
    {
        var comparer = DefaultIIPAddressRangeComparer.Instance;
        var a = Subnet.Parse("10.0.0.0/24");
        var b = Subnet.Parse("10.0.1.0/24");
        Require(comparer.Compare(a, b) < 0, "a < b by head address");
        Require(comparer.Compare(b, a) > 0, "b > a");
        Require(comparer.Compare(a, a) == 0, "a == a");
    }
);

Check(
    "DefaultAddressFamilyComparer",
    () =>
    {
        var comparer = DefaultAddressFamilyComparer.Instance;
        var result = comparer.Compare(AddressFamily.InterNetwork, AddressFamily.InterNetworkV6);
        Require(result != 0, "IPv4 != IPv6");
        Require(
            comparer.Compare(AddressFamily.InterNetwork, AddressFamily.InterNetwork) == 0,
            "IPv4 == IPv4"
        );
    }
);

// ---------------------------------------------------------------------------
// Summary
// ---------------------------------------------------------------------------
Console.WriteLine();
Console.WriteLine($"Results: {passed} passed, {failed} failed.");

if (failed > 0)
    Environment.Exit(1);
