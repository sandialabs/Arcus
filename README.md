# ![Arcus](src/Arcus/icon.png) Arcus

[![Build](https://img.shields.io/github/actions/workflow/status/sandialabs/Arcus/build.yml?branch=main&logo=github)](https://github.com/sandialabs/Arcus/actions/workflows/build.yml)
[![NuGet](https://img.shields.io/nuget/v/Arcus?logo=nuget)](https://www.nuget.org/packages/Arcus)
![Targets](https://img.shields.io/badge/.NET_Standard_2.0_%7C_.NET_8.0_%7C_.NET_9.0_%7C_.NET_10.0-blue)
[![License](https://img.shields.io/github/license/sandialabs/Arcus?logo=apache)](https://github.com/sandialabs/Arcus/blob/main/LICENSE)

## About the Project

Arcus is a C# manipulation library for calculating, parsing, formatting, converting, and comparing both IPv4 and IPv6 addresses and subnets. It accounts for 128-bit numbers on 32-bit platforms.

## ❗Breaking Changes in v5.0.0

### Gulliver dependency removed

The [Gulliver](https://github.com/sandialabs/gulliver) NuGet package is no longer a dependency. All byte-level manipulation previously delegated to Gulliver is now handled internally by the new `BigEndianBitWrapper` type.

**Why:** Removing the external dependency simplifies the dependency graph for consumers, avoids version conflicts, and gives Arcus full control over its byte-level operations without relying on an outside library's API surface.

**Migration:** If your project depended on Gulliver being available transitively through Arcus (e.g. you used `ByteArrayUtils`, `ShiftBitsLeft`, or the `byte[].ToString("HC"/"IBE"/"b")` format extensions without a direct reference to Gulliver), you must add a direct `PackageReference` to Gulliver or replace those usages with equivalent implementations.

### .NET 10 target added

Arcus now targets `net10.0` in addition to `netstandard2.0`, `net8.0`, and `net9.0`. The test project similarly targets `net10.0`.

**Why:** Stay current with the latest .NET releases and take advantage of new platform features and performance improvements.

### Enumeration cap via `maxEnumerationExponent` and `DefaultMaxEnumerationExponent`

Every range type now stores a `MaxEnumerationExponent` property (0–128). Enumeration via `ToIPAddresses()` (and `foreach` / `IEnumerable<T>` for backwards compatibility) yields at most **2<sup>MaxEnumerationExponent</sup>** addresses.

- The **default** is `DefaultMaxEnumerationExponent` = 12 (2<sup>12</sup> = 4096 addresses).
- If a range contains more than 2<sup>MaxEnumerationExponent</sup> addresses, `InvalidOperationException` is thrown at the point the limit is exceeded.
- This prevents accidental enumeration of enormous address spaces (e.g., a `/8` IPv4 subnet has 16,777,216 addresses).

**Why:** Previously, `foreach` on an arbitrary range would silently enumerate every address — potentially trillions of iterations for large ranges. The cap forces developers to consciously opt into large enumeration by specifying a higher exponent. A value of `0` disables enumeration entirely; `128` allows enumeration up to the full IPv6 space (though practically impossible).

**How to increase the cap:** Pass `maxEnumerationExponent` to constructors:

```csharp
// Default cap: 4096 addresses
var subnet = new Subnet(IPAddress.Parse("10.0.0.0"), 8);

// This will throw InvalidOperationException (> 4096 addresses)
foreach (var addr in subnet) { /* throws */ }

// Raise cap to 24 (16,777,216 addresses — enough for /8)
var large = new Subnet(IPAddress.Parse("10.0.0.0"), 8, maxEnumerationExponent: 24);
foreach (var addr in large) { /* OK */ }
```

The exponent parameter is available on all range constructors (`Subnet`, `IPAddressRange`, `AbstractIPAddressRange`), static factory methods (`Subnet.Parse`, `Subnet.FromNetMask`, `IPAddressRange.TryCollapseAll`, `IPAddressRange.TryExcludeAll`, `IPAddressRange.TryMerge`), and `SubnetUtilities.FewestConsecutiveSubnetsFor`.

**Migration:**

| Old pattern | New pattern |
|---|---|
| `subnet.Enumerate()` | `foreach (var addr in subnet)` (uses constructor cap; default 4096) |
| `subnet.Enumerate(20)` | Construct with `maxEnumerationExponent: 20` and use `foreach` |
| unlimited `foreach` on large ranges | Throws `InvalidOperationException` by default; increase exponent or use explicit arithmetic operations |

### New `ToIPAddresses()` method

A new method `IEnumerable<IPAddress> ToIPAddresses()` has been added to the `IIPAddressRange` interface and implemented on `AbstractIPAddressRange`. It returns an enumerable of the addresses in the range, capped by `MaxEnumerationExponent`.

The existing `GetEnumerator()` is now `[Obsolete("Use ToIPAddresses() instead")]`. It still works (delegating to `ToIPAddresses()`), but produces a compile-time warning.

```csharp
// Current (warning-free):
foreach (var addr in range.ToIPAddresses()) { ... }

// Old style (produces obsolete warning in v5, will break in v6):
foreach (var addr in range) { ... }
```

### Removed types and members

The following previously `[Obsolete]` types and members have been removed.

| Removed | Namespace / Location | Migration |
| --- | --- | --- |
| `MacAddress` (entire type) | `Arcus` | No direct replacement in this library |
| `DefaultIPAddressRangeComparer` (entire type) | `Arcus.Comparers` | Use `DefaultIIPAddressRangeComparer` |

### `SubnetUtilities` static fields are now `readonly`

`SubnetUtilities.PrivateIPAddressRangesList` and `SubnetUtilities.LinkLocalIPAddressRangesList` are now declared `readonly`. Previously the field *reference* could be replaced by external code (e.g., `SubnetUtilities.PrivateIPAddressRangesList = myList`). That pattern will no longer compile. The `IReadOnlyList<Subnet>` type already prevented mutation of the list *contents*; `readonly` now also prevents replacement of the list itself.

**Migration:** If you were replacing these fields to customize private-address detection, extract that logic into a separate variable and pass it explicitly to your own helper methods.

### Behavior corrections (non-breaking for correct usage)

The following bugs have been fixed. If your code was intentionally relying on the incorrect behavior, you will need to update it.

| Type / Member | Previous (incorrect) | Fixed |
| --- | --- | --- |
| `AbstractIPAddressRange.Overlaps(IIPAddressRange)` | `B.Overlaps(A)` returned `false` when B was wholly inside A | Both directions now return `true` (symmetric) |
| `AbstractIPAddressRange.ContainsAnyPrivateAddresses()` | Checked only endpoints; missed ranges that span a private block with public endpoints | Uses range-overlap detection across `PrivateIPAddressRangesList` |
| `AbstractIPAddressRange.ContainsAllPublicAddresses()` | Checked only endpoints; returned `true` for ranges whose endpoints are public but whose interior spans a private block | Uses range-overlap detection |
| `IPAddressRange.TryExcludeAll` | Threw `InvalidOperationException` when an exclusion ended at the family maximum address | Returns `(true, leading segment)` or `(true, [])` as appropriate |

### IP Address Parsing across .NET Targets

In .NET versions up to and including .NET 4.8 (which corresponds to .NET Standard 2.0), stricter parsing rules are enforced for `IPAddress` according to the IPv6 specification. Specifically, the presence of a terminal '%' character without a valid zone index is considered invalid in these versions. As a result, the input `abcd::%` fails to parse, leading to a null or failed address parsing depending on `Parse`/`TryParse`.

In newer versions of .NET, including .NET 8, .NET 9, and .NET 10, the parsing rules have been relaxed. The trailing '%' character is now ignored during parsing, allowing for inputs that would have previously failed.

It is important to note that this scenario appears to be an extreme edge case. If in doubt, sanitize IP address user input to meet your development needs.

### Obsolete members (compile-time warning in v5, will be removed in v6)

The following members are marked `[Obsolete]` in v5.0.0. They continue to work but produce compile-time warnings. They will be **removed in v6.0.0**.

| Obsolete member | Replacement | Notes |
|---|---|---|
| `IIPAddressRange.GetEnumerator()` (and `foreach` on any range type) | `ToIPAddresses()` | `GetEnumerator()` now delegates to `ToIPAddresses()`; direct `foreach` on range types is deprecated |
| `IIPAddressRange` implementing `IEnumerable<IPAddress>` | None — the interface itself will be removed | Avoid relying on `IIPAddressRange` being enumerable; use `ToIPAddresses()` explicitly |
| `Subnet.TryIPv6FromPartial(string, out IEnumerable<Subnet>)` | To be replaced by more explicit methods in a future release | This method's behavior was highly specialized and often surprising to consumers |

### Performance and modernization

- **Regex source generators:** `Subnet` parsing and `IPAddressUtilities` hexadecimal/octal parsing now use C# `[GeneratedRegex]` source generators for improved startup and throughput on targets that support it.
- **Partial class split:** Large types (`Subnet`, `IPAddressRange`, `AbstractIPAddressRange`, `BigEndianBitWrapper`) have been split into partial files organized by interface implementation for better maintainability.
- **Internal `BigEndianBitWrapper`:** A new internal type provides big-endian byte-array arithmetic, replacing the Gulliver dependency.
- **`InternalsVisibleTo`:** Tests and benchmarks now have access to internal types for more thorough testing.

### Anticipated future breaking change: `IEnumerable<IPAddress>` removal from `IIPAddressRange`

In a future major version, `IIPAddressRange` (and its inheritors `Subnet` and `IPAddressRange`) will **no longer implement `IEnumerable<IPAddress>`**. The `GetEnumerator()` method will be removed entirely.

**Why:** The `IEnumerable<IPAddress>` interface makes it too easy to accidentally enumerate astronomically large address spaces (e.g., a full IPv4 `/0` has ~4 billion addresses). The enumeration cap via `MaxEnumerationExponent` mitigates this at runtime, but the interface contract itself encourages direct `foreach` usage that is semantically misleading — iterating over billions of items is rarely the intent.

**Migration strategy:** Replace all `foreach (var addr in range)` patterns with `foreach (var addr in range.ToIPAddresses())` now. This produces no warnings in v5 and will be required in the future version.

```csharp
// v5 (warning-free, will continue to work):
foreach (var addr in range.ToIPAddresses()) { ... }

// v5 (produces obsolete warning, will break in v6):
foreach (var addr in range) { ... }
```

## Getting Started

The latest stable release of Arcus is [available on NuGet](https://www.nuget.org/packages/Arcus/).

The latest [Arcus documentation](https://arcus.readthedocs.io/en/latest/) may be found on [ReadTheDocs](https://arcus.readthedocs.io/en/latest/).

### Usage

Arcus provides types and utilities for working with IP addresses and subnets beyond what the .NET BCL offers. The library is organized around a few core concepts.

#### Quick Start

```csharp
using Arcus;
using Arcus.Utilities;

// Parse a subnet
var subnet = Subnet.Parse("192.168.1.0/24");
Console.WriteLine($"Network: {subnet.Head}, Broadcast: {subnet.Tail}");
Console.WriteLine($"Usable addresses: {subnet.UsableHostAddressCount}");

// Check containment
var testAddress = IPAddress.Parse("192.168.1.50");
Console.WriteLine(subnet.Contains(testAddress)); // True

// Enumerate (capped at 2^MaxEnumerationExponent = 4096 by default)
foreach (var addr in subnet.ToIPAddresses().Take(5))
{
    Console.WriteLine(addr);
}

// IP math
var nextAddress = testAddress.Increment(); // 192.168.1.51
var prevAddress = testAddress.Increment(-2); // 192.168.1.48

// Comparison
testAddress.IsBetween(subnet.Head, subnet.Tail);      // True
testAddress.IsGreaterThan(IPAddress.Parse("10.0.0.1")); // True

// Address family detection
testAddress.IsIPv4(); // True
testAddress.IsIPv6(); // False

// Work with ranges
var range = new IPAddressRange(
    IPAddress.Parse("10.0.0.0"),
    IPAddress.Parse("10.0.0.255"));
Console.WriteLine($"Range length: {range.Length}");

// Fewest subnets covering a range
var covering = SubnetUtilities.FewestConsecutiveSubnetsFor(
    IPAddress.Parse("128.64.20.3"),
    IPAddress.Parse("128.64.20.12"));
```

#### Enumeration safety

All range types enforce an enumeration cap via `MaxEnumerationExponent`. Use `ToIPAddresses()` to enumerate explicitly:

```csharp
// Safe enumeration (throws if range exceeds cap)
foreach (var addr in range.ToIPAddresses())
{
    Process(addr);
}

// Check if enumeration would succeed
if (range.Length <= (BigInteger.One << range.MaxEnumerationExponent))
{
    foreach (var addr in range.ToIPAddresses()) { ... }
}
```

#### `Subnet`

An IPv4 or IPv6 subnetwork representation conforming to CIDR rules (power-of-two length, aligned to a prefix boundary). Implements `IIPAddressRange`, `IEquatable<Subnet>`, `IComparable<Subnet>`, `IFormattable`, `IEnumerable<IPAddress>`, and `ISerializable`.

**Construction:**

```csharp
// From two addresses (smallest subnet containing both)
var subnet = new Subnet(
    IPAddress.Parse("192.168.1.0"),
    IPAddress.Parse("192.168.1.255"));

// From address and routing prefix
var subnet = new Subnet(IPAddress.Parse("192.168.1.1"), 24); // autocorrects to 192.168.1.0/24

// From a CIDR string
var subnet = Subnet.Parse("192.168.1.0/24");
Subnet.TryParse("10.0.0.0/8", out var result);

// From netmask
var subnet = Subnet.FromNetMask(
    IPAddress.Parse("192.168.1.0"),
    IPAddress.Parse("255.255.255.0"));

// Single-address subnet
var single = new Subnet(IPAddress.Parse("192.168.1.1")); // 192.168.1.1/32
```

All constructors and factory methods accept an optional `maxEnumerationExponent` parameter to control enumeration limits.

**Properties:** `Head`, `Tail`, `RoutingPrefix`, `Netmask` (IPv4 only), `BroadcastAddress`, `NetworkPrefixAddress`, `UsableHostAddressCount`, `Length`, `MaxEnumerationExponent`.

**Set operations:** `Contains(Subnet)`, `Overlaps(Subnet)`, `Touches(IIPAddressRange)`.

**Public constants:**

| Constant | Description |
|---|---|
| `Ipv4OctetPartialPattern` | Regex pattern matching partial IPv4 octet strings |
| `RoughSubnetStringPattern` | Regex pattern matching rough subnet string shape |

#### `IPAddressRange`

An inclusive range of IP addresses of the same address family. Unlike `Subnet`, not restricted to CIDR boundaries — any `head` through `tail` inclusive.

```csharp
var range = new IPAddressRange(
    IPAddress.Parse("10.0.0.5"),
    IPAddress.Parse("10.0.0.42"));

// Static utilities
IPAddressRange.TryCollapseAll(ranges, out var collapsed);
IPAddressRange.TryExcludeAll(initial, exclusions, out var remaining);
IPAddressRange.TryMerge(left, right, out var merged);
```

All static utility methods accept an optional `maxEnumerationExponent` parameter.

#### `IIPAddressRange`

The core interface implemented by both `Subnet` and `IPAddressRange`. Provides:

- `Head`, `Tail`, `Length`, `AddressFamily`
- `IsSingleIP`, `IsIPv4`, `IsIPv6`
- `Contains(IIPAddressRange)`, `Contains(IPAddress)`
- `Overlaps(IIPAddressRange)`, `Touches(IIPAddressRange)`
- `HeadOverlappedBy(IIPAddressRange)`, `TailOverlappedBy(IIPAddressRange)`
- `ContainsAnyPrivateAddresses()`, `ContainsAllPrivateAddresses()`
- `ContainsAnyPublicAddresses()`, `ContainsAllPublicAddresses()`
- `ToIPAddresses()` — enumerate addresses capped at `MaxEnumerationExponent`
- `MaxEnumerationExponent` — the exponent controlling the enumeration cap

#### Comparers

| Comparer | Description |
|---|---|
| `DefaultAddressFamilyComparer` | Compares `AddressFamily` values (`InterNetwork` < `InterNetworkV6`) |
| `DefaultIPAddressComparer` | Compares `IPAddress` by family then unsigned big-endian value |
| `DefaultIIPAddressRangeComparer` | Compares ranges by head address then by length |

#### `IPAddressMath` — Arithmetic and Comparison

```csharp
// Increment and decrement
var next = address.Increment();       // +1
var prev = address.Increment(-5);     // -5
address.TryIncrement(out var result, delta: 10);

// Comparison
address.IsEqualTo(other);
address.IsGreaterThan(other);
address.IsLessThan(other);
address.IsBetween(low, high);

// Bounds
address.IsAtMin();    // true for 0.0.0.0 or ::
address.IsAtMax();    // true for 255.255.255.255 or ffff:...:ffff
IPAddressMath.Min(a, b);
IPAddressMath.Max(a, b);
```

#### `IPAddressConverters`

```csharp
// CIDR conversion (IPv4 only)
netmask.NetmaskToCidrRoutePrefix(); // IPAddress → int

// String formatting
address.ToDottedQuadString();       // IPv6 → mixed IPv4/IPv6 notation
address.ToHexString();              // Big-endian hex
address.ToNumericString();          // Unsigned integer string
address.ToUncompressedString();     // Fully expanded
address.ToBase85String();           // RFC 1924 (IPv6 only)
```

#### `IPAddressUtilities`

```csharp
// Address family
address.IsIPv4();
address.IsIPv6();

// Detection
address.IsIPv4MappedIPv6();         // RFC 4291
netmask.IsValidNetMask();           // IPv4 netmask validation

// Special values
AddressFamily.InterNetwork.MaxIPAddress();
AddressFamily.InterNetworkV6.MinIPAddress();

// Parsing
IPAddressUtilities.ParseFromHexString("C0A80101", AddressFamily.InterNetwork);
IPAddressUtilities.TryParseFromHexString(input, family, out address);
IPAddressUtilities.ParseIgnoreOctalInIPv4("010.000.001.001");
IPAddressUtilities.TryParse(bigInteger, family, out address);
```

**Public constants:**

| Constant | Description |
|---|---|
| `DottedQuadLeadingZerosPattern` | Regex pattern matching leading zeros in dotted-quad octets |
| `DottedQuadRegularExpressionPattern` | Regex pattern checking dotted-quad format |
| `HexLikePattern` | Regex pattern matching hexadecimal digit strings |
| `IPv4BitCount` | 32 |
| `IPv4ByteCount` | 4 |
| `IPv4OctetCount` | 4 |
| `IPv6BitCount` | 128 |
| `IPv6ByteCount` | 16 |
| `IPv6HextetCount` | 8 |

#### `SubnetUtilities`

```csharp
// Fewest consecutive subnets covering an address range
var subnets = SubnetUtilities.FewestConsecutiveSubnetsFor(
    IPAddress.Parse("128.64.20.3"),
    IPAddress.Parse("128.64.20.12"));

// Largest / smallest subnet in a collection
SubnetUtilities.LargestSubnet(subnets);
SubnetUtilities.SmallestSubnet(subnets);

// Private/public range detection (constants)
SubnetUtilities.PrivateIPAddressRangesList;
SubnetUtilities.LinkLocalIPAddressRangesList;
```

## Built With

This project was built with the aid of:

- [BenchmarkDotNet](https://benchmarkdotnet.com/) - microbenchmarking
- [CSharpier](https://csharpier.com/) - code formatting
- [dotnet-outdated](https://github.com/dotnet-outdated/dotnet-outdated) - dependency updates
- [Husky.Net](https://alirezanet.github.io/Husky.Net/) - git hooks
- [Pre-commit](https://pre-commit.com/) - pre-commit hooks (trailing whitespace, codespell, markdownlint)
- [Roslynator](https://josefpihrt.github.io/docs/roslynator/) - Roslyn analyzers
- [SonarAnalyzer](https://www.sonarsource.com/products/sonarlint/features/visual-studio/) - code quality
- [StyleCop.Analyzers](https://github.com/DotNetAnalyzers/StyleCopAnalyzers) - style enforcement
- [xUnit.net](https://xunit.net/) - unit testing

### Versioning

This project uses [Semantic Versioning](https://semver.org/)

### Targeting

The library targets [.NET Standard 2.0](https://learn.microsoft.com/en-us/dotnet/standard/net-standard?tabs=net-standard-2-0), [.NET 8](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-8), [.NET 9](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-9/overview), and [.NET 10](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-10/overview). The test project targets `net48` (Windows only), `net8.0`, `net9.0`, and `net10.0`. The benchmark project targets `net48`, `net8.0`, `net9.0`, and `net10.0`.

Language version: C# 14.0.

### Commit Hook

The project itself has a configured pre-commit git hook, via [Husky.Net](https://alirezanet.github.io/Husky.Net/) that automatically lints and formats code via [dotnet format](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-format) and [csharpier](https://csharpier.com/).

#### Disable husky in CI/CD pipelines

Per the [Husky.Net instructions](https://alirezanet.github.io/Husky.Net/guide/automate.html#disable-husky-in-ci-cd-pipelines)

> You can set the `HUSKY` environment variable to `0` in order to disable husky in CI/CD pipelines.

#### Manual Linting and Formatting

On occasion a manual run is desired it may be done so via the `src` directory and with the command

```shell
dotnet format style; dotnet format analyzers; dotnet csharpier format .
```

These commands may be called independently, but order may matter.

#### Testing

After making changes, tests should be run that include all targets:

```shell
cd src
dotnet test
```

Tests target `net48` (Windows only, for `netstandard2.0` consumers), `net8.0`, `net9.0`, and `net10.0`. On Ubuntu CI, tests run per-TFM sequentially; on Windows, all TFMs run in one `dotnet test` call.

To run a single TFM or test class:

```shell
dotnet test --framework net10.0
dotnet test --filter "FullyQualifiedName~SubnetTests"
```

Smoke tests validate the packed NuGet package against real runtimes:

```shell
cd smoketests
./run-smoke-tests.sh
```

## Acknowledgments

This project was built by the Production Tools Team at Sandia National Laboratories. Special thanks to all contributors and reviewers who helped shape and improve this library.

Including, but not limited to:

- **Robert H. Engelhardt** - *Primary Developer, Source of Ideas Good and Bad* - [rheone](https://github.com/rheone)
- **Andrew Steele** - *Review and Suggestions* - [ahsteele](https://github.com/ahsteele)
- **Nick Bachicha** - *Git Wrangler and DevOps Extraordinaire* - [nicksterx](https://github.com/nicksterx)
- **Drew Antonich** - *Review and Suggestions* [drewantonich](https://github.com/drewantonich)

## Copyright

> Copyright 2025 National Technology & Engineering Solutions of Sandia, LLC (NTESS). Under the terms of Contract DE-NA0003525 with NTESS, the U.S. Government retains certain rights in this software.

## License

> Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
>
> <http://www.apache.org/licenses/LICENSE-2.0>
>
> Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.
