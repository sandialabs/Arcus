# ![Arcus](src/Arcus/icon.png) Arcus

![GitHub Actions Workflow Status](https://img.shields.io/github/actions/workflow/status/sandialabs/Arcus/build.yml?branch=main)
[![nuget Version](https://img.shields.io/nuget/v/Arcus)](https://www.nuget.org/packages/Arcus)
[![GitHub Release](https://img.shields.io/github/v/release/sandialabs/Arcus)](https://github.com/sandialabs/Arcus/releases)
[![GitHub Tag](https://img.shields.io/github/v/tag/sandialabs/Arcus)](https://github.com/sandialabs/Arcus/tags)
![Targets](https://img.shields.io/badge/.NET%20Standard%202.0%20|%20.NET%208.0%20|%20.NET%209.0|%20.NET%2010.0-blue)
[![Apache 2.0 License](https://img.shields.io/github/license/sandialabs/Arcus?logo=apache)](https://github.com/sandialabs/Arcus/blob/main/LICENSE)

## About the Project

Arcus is a C# manipulation library for calculating, parsing, formatting, converting, and comparing both IPv4 and IPv6 addresses and subnets. It accounts for 128-bit numbers on 32-bit platforms.

## ❗Breaking Changes in Version 3+

### IP Address Parsing based on .NET Targets

In .NET versions up to and including .NET 4.8 (which corresponds to .NET Standard 2.0), stricter parsing rules were enforced for `IPAddress` according to the IPv6 specification. Specifically, the presence of a terminal '%' character without a valid zone index is considered invalid in these versions. As a result, the input `abcd::%` fails to parse, leading to a null or failed address parsing depending on `Parse`/`TryParse`. This behavior represents a breaking change from Arcus's previous target of .NET Standard 1.3. and may provide confusion for .NET 4.8 / .NET Standard 2.0 versions.

In contrast, in newer versions of .NET, including .NET 8 and .NET 9, and .NET 10 the parsing rules have been relaxed. The trailing '%' character is now ignored during parsing, allowing for inputs that would have previously failed.

It is important to note that this scenario appears to be an extreme edge case, and developers should ensure that their applications handle `IPAddress` parsing appropriately across different target frameworks as expected.

If in doubt it is suggested that IP Address based user input should be sanitized to meet your development needs.

## Getting Started

The latest stable release of Arcus is [available on NuGet](https://www.nuget.org/packages/Arcus/).

The latest [Arcus documentation](https://arcus.readthedocs.io/en/latest/) may be found on [ReadTheDocs](https://arcus.readthedocs.io/en/latest/).

### Usage

At its heart Arcus is split amongst five separate interdependent units. _Types_, _Comparers_, _Converters_, _Math_, and _Utilities_.

These units each work across Arcus's `IPAddressRange`, `Subnet` and .NET's `System.Net.IPAddress`. Arcus adds extra desired functionality where the standard C# libraries left off.

#### Types

##### `Subnet`

An IPv4 or IPv6 subnetwork representation - the work horse and original reason for the Arcus library. Outside the concept of the `Subnet` object, most everything else in Arcus is auxiliary and exists only in support of the `Subnet` object. That’s not to say that the remaining pieces of the Arcus library aren’t useful, on the contrary their utility can benefit a developer greatly.

A `Subnet` may be instantiated in several ways:

The most common way to create a `Subnet` object is to construct it via a high and low `IPAddress` by calling the constructor `Subnet(IPAddress primary, IPAddress secondary)`. This constructs the smallest possible subnet that would contain both IP addresses. Typically the addresses specified are the Network and Broadcast addresses (lower and higher bounds of a subnet) but this is not necessary. Addresses _MUST_ be the same address family (either Internetwork or InternetworkV6).

It is also possible to create a `Subnet` from an `IPAddress` and an `integer` based _route prefix_. Eg: `Subnet(IPAddress ipAddress, int routingPrefix)`.

Likewise it may be desired to statically parse a subnet string with `Subnet.Parse(string input)` or it’s safe equivalent of `bool Subnet.TryParse(string input, out Subnet subnet)`

For example, one could safely parse the `string` "192.168.1.0/16" via

```c#
Subnet subnet;
var success = Subnet.TryParse("192.168.1.0/16", out subnet)
```

##### `IPAddressRange`

`IPAddressRange` is a basic implementation of `IIPAddressRange` it is used to represent an inclusive range of arbitrary IP Addresses of the same address family. Unlike `Subnet`, `IPAddressRange` is not restricted to a power of two length, nor a valid broadcast address head.

#### Comparers

The _Comparers_ package contains useful Comparer objects for comparing properties of IP Addresses and IP Address composite objects.

- `DefaultAddressFamilyComparer` - A comparer that compares address families. Most frequently `Internetwork` (IPv4) and `InternetworkV6` (IPv6)
- `DefaultIPAddressComparer` - A comparer for `IPAddress` objects
- `DefaultIPAddressRangeComparer` - A comparer for `IIPAddressRange`. Compares such that lower order ranges are less that higher order ranges accounting for size at matching range starts

#### Converters

The _Converters_ package is a package of static utility classes for converting one type into another type.

##### `IPAddressConverters`

Static utility class containing conversion methods for converting `IPAddress` objects into something else.

#### Math

The _Math_ package is a package of static utility classes for doing computational mathematics on objects.

##### `IPAddressMath`

In some cases the C# `IPAddress` object doesn't go far enough with what you can do with it mathematically, this static utility class containing mathematical methods to fill in the gaps.

###### Incrementing and Decrementing

Incrementing and Decrementing an `IPAddress` is easy.

Incrementing by one is a simple call to the extension method:

```c#
var address = IPAddress.Parse("192.168.1.1");
var result = address.Increment(); // result is 192.168.1.2
```

Decrementing is just as simple:

```c#
var address = IPAddress.Parse("192.168.1.1");
var result = address.Increment(-2); // result is 192.168.0.0
```

_Overflow_ and _Underflow_ conditions will result in an `InvalidOperationException`.

###### Equality

Equality may also be tested via a host of equality extension methods:

- `bool IsEqualTo(this IPAddress alpha, IPAddress beta)`
- `bool IsGreaterThan(this IPAddress alpha, IPAddress beta)`
- `bool IsGreaterThanOrEqualTo(this IPAddress alpha, IPAddress beta)`
- `bool IsLessThan(this IPAddress alpha, IPAddress beta)`
- `bool IsLessThanOrEqualTo(this IPAddress alpha, IPAddress beta)`

#### Utilities

The _Utilities_ package contains static classes for miscellaneous operations on specific types.

##### `IPAddressUtilities`

Static utility class containing miscellaneous operations for `IPAddress` objects

###### Address Family Detection

A couple of extension methods were created to quickly determine the address family of an IP Address. To determine if an address is IPv4 use `bool IsIPv4(this IPAddress ipAddress)`, likewise `bool IsIPv6(this IPAddress ipAddress)` can be used to test for IPv6.

###### Parsing

It is possible to parse an `IPAddress` from a hexadecimal string into either an IPv4 of IPv6 address using the `IPAddress ParseFromHexString(string input, AddressFamily addressFamily)` method. Likewise it can be done safely with `bool TryParseFromHexString(string input, AddressFamily addressFamily, out IPAddress address)`.

Similarly, conversion may be done from an octal string by using `bool TryParseIgnoreOctalInIPv4(string input, out IPAddress address)` or even a `BigInteger` by way of `bool TryParse(BigInteger input, AddressFamily addressFamily, out IPAddress address)`.

##### `SubnetUtilities`

Static utility class containing miscellaneous operations for `Subnet` objects that didn't make sense to put on the object itself.

Given two arbitrary IP Addresses of the same family it may be desired to calculate the fewest consecutive subnets that would hold the inclusive range between them. For example

```c#
SubnetUtilities.FewestConsecutiveSubnetsFor(IPAddress.Parse("128.64.20.3"), IPAddress.Parse("128.64.20.12"))
```

would return an `Enumerable` containing the subnets `128.64.20.3/32`, `128.64.20.4/30`, `128.64.20.8/30`, `128.64.20.12/32`.

### Developer Notes

## Built With

This project was built with the aid of:

- [CSharpier](https://csharpier.com/)
- [dotnet-outdated](https://github.com/dotnet-outdated/dotnet-outdated)
- [Gulliver](https://github.com/sandialabs/gulliver) - A self created library that helped us keep our bits and bytes in order
- [Husky.Net](https://alirezanet.github.io/Husky.Net/)
- [Roslynator](https://josefpihrt.github.io/docs/roslynator/)
- [SonarAnalyzer](https://www.sonarsource.com/products/sonarlint/features/visual-studio/)
- [StyleCop.Analyzers](https://github.com/DotNetAnalyzers/StyleCopAnalyzers)
- [xUnit.net](https://xunit.net/)

### Versioning

This project uses [Semantic Versioning](https://semver.org/)

### Targeting

The project targets [.NET Standard 2.0](https://learn.microsoft.com/en-us/dotnet/standard/net-standard?tabs=net-standard-2-0), [.NET 8](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-8), [.NET 9](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-9/overview), and [.NET 10](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-10/overview). The test project similarly targets .NET 8, .NET 9, .NET 10, but targets [.NET Framework 4.8](https://dotnet.microsoft.com/en-us/download/dotnet-framework/net48) for the .NET Standard 2.0 tests.

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

After making changes tests should be run that include all targets

## Acknowledgments

This project was built by the Production Tools Team at Sandia National Laboratories. Special thanks to all contributors and reviewers who helped shape and improve this library.

Including, but not limited to:

- **Robert H. Engelhardt** - _Primary Developer, Source of Ideas Good and Bad_ - [rheone](https://github.com/rheone)
- **Andrew Steele** - _Review and Suggestions_ - [ahsteele](https://github.com/ahsteele)
- **Nick Bachicha** - _Git Wrangler and DevOps Extraordinaire_ - [nicksterx](https://github.com/nicksterx)

## Copyright

> Copyright 2025 National Technology & Engineering Solutions of Sandia, LLC (NTESS). Under the terms of Contract DE-NA0003525 with NTESS, the U.S. Government retains certain rights in this software.

## License

> Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
>
> http://www.apache.org/licenses/LICENSE-2.0
>
> Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.
