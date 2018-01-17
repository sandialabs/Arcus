# Arcus

Arcus is a C# manipulation library for calculating, parsing, formatting, converting, and comparing both IPv4 and IPv6 addresses and subnets. It accounts for 128-bit numbers on 32-bit platforms.

## Getting Started

You need to acquire a copy of the source code and compile it in C#, thus creating an dll file.

In the future, we hope to have Arcus available via a [NuGet]( https://www.nuget.org/) package.

An example of code usage can be found later in the **Usage** section.

## Usage

At its heart Arcus is split amongst five separate interdependent units. *Types*, *Comparers*, *Converters*, *Math*, and *Utilities*.

### Types

#### `Subnet`

An IPv4 or IPv6 subnetwork representation - the work horse and original reason for the Arcus library. Outside the concept of the `Subnet` object, most everything else in Arcus is auxiliary and exists only in support of making this once facet work. That’s not to say that the remaining pieces of the Arcus library aren’t useful, on the contrary their utility can benefit a developer greatly. That said, once the `Subnet` is mastered the rest of Arcus should follow through nicely.

A `Subnet` may be instantiated in several ways:

To the most common ways to create a subnet is to construct it via a high and low `IPAddress` call the constructor `Subnet(IPAddress primary,  IPAddress secondary)` This construct the smallest possible subnet that would contain both IP addresses typically the address specified are the Network and Broadcast addresses (lower and higher bounds) but this is not necessary. Addresses *MUST* be the same address family (either Internetwork or InternetworkV6)

It is also possible to create a `Subnet` from an `IPAddress` and an `integer` based *route prefix*. Eg: `Subnet(IPAddress ipAddress, int routingPrefix)`

Likewise it may be desired to statically parse a subnet string with `Subnet. Parse(string input)` or it’s safe equivalent of ` bool Subnet.TryParse(string input,  out Subnet subnet)`. For example, one could safely parse the `string` "192.168.1.0/16" via
```
Subnet subnet;
var success = Subnet.TryParse("192.168.1.0/16",  out subnet)
```

#### `IPAddressRange`

A basic implementation of a IIPAddressRange used to represent an inclusive range of arbitrary IP Addresses of the same address family. It isn't restricted like a `Subnet` is, allowing for non-power of two sizes, but because of this it's functionality is likewise limited.

### Comparers

The *Comparers* package contains useful Comparer objects for comparing properties of IP Addresses and IP Address composite objects

 * `DefaultAddressFamilyComparer` - A comparer that compares address families. Most frequently `Internetwork` (IPv4) and `InternetworkV6` (IPv6)
 * `DefaultIPAddressComparer` - A comparer for `IPAddress` objects
 * `DefaultIPAddressRangeComparer` - A comparer for `IPAddressRange`. Compares such that lower order ranges are less that higher order ranges accounting for size at matching range starts
 * `DefaultSubnetComparer` - A comparer for `Subnet` objects. Compares such that lower order ranges are less that higher order ranges accounting for size at matching range starts

### Converters

The *Converters* package is a package of static utility classes for converting on type into another type

#### `BigIntegerConverters`

Static utility class containing conversion methods for converting `BigInteger` objects into something else

#### `ByteArrayConverters`

Static utility class containing conversion methods for converting `byte` arrays into something else

#### `IPAddressConverters`

Static utility class containing conversion methods for converting `IPAddress` objects into something else

##### String Conversions

Sometimes it makes sense to convert an `IPAddress` into a hexadecimal representation of the underlying value, this can be done via the extension method `string ToHexString(this IPAddress ipAddress)`

Likewise, a stringified numeric representation may be desired. For this use the `string ToNumericString(this IPAddress ipAddress)` extension method.

Possibly more useful, when dealing with IPv6, it isn't unreasonable to want to convert an IPv6 "compressed" address into its fullest form. This can be done with the `string ToUncompressedString(this IPAddress ipAddress)` extension.

```
    var address = IPAddress.Parse("1080::8:800:200C:417A");
    var result = address.ToUncompressedString();    // where result is equal to 1080:0000:0000:0000:0008:0800:200C:417A
```

Other times it is useful to have the numeric representation of an IPAddress. This can be done both in signed `BigInteger ToBigInteger(this IPAddress ipAddress)` and unsigned `BigInteger ToUnsignedBigInteger(this IPAddress ipAddress)`


[RFC 1924](http://tools.ietf.org/html/rfc1924), an April Fool's Day joke, provides a manner in which to convert an IPv6 address in to a Ascii85/Base85 representation. If one is feeling particularity foolish it is possible to do this conversion via `string ToBase85String(this IPAddress ipAddress)`

```
    var address = IPAddress.Parse("1080::8:800:200C:417A");
    string result = address.ToBase85String();    // where result is equal to the string "4)+k&C#VzJ4br>0wv%Yp"
```



### Math

The *Math* package is a package of static utility classes for doing computational mathematics on objects

#### `BigIntegerMath`

Static utility class containing mathematical methods for `BigInteger` objects

#### `ByteArrayMath`

Static utility class containing mathematical methods for `byte` arrays

#### `IPAddressMath`

In some cases the C# `IPAddress` object doesn't go far enough with what you can do with it mathematically, this static utility class containing mathematical methods to fill in the gaps.

##### Incrementing and Decrementing

Incrementing and Decrementing an `IPAddress` is as easy as a call to the `IPAddress Increment(this IPAddress ipAddress, long delta = 1)` method

Incrementing by one is a simple call to the extension method

```
var address = IPAddress.Parse(192.168.1.1);
var result = address.Increment();   // result is 192.168.1.2
```

Decrementing is just as simple 

```
var address = IPAddress.Parse(192.168.1.1);
var result = address.Increment(-2);   // result is 192.168.0.0
```

*Overflow* and *Underflow* conditions will result in an `InvalidOperationException`

##### Equality

Equality may also be tested via a host of equality extension methods

 * `bool IsEqualTo(this IPAddress alpha, IPAddress beta)`
 * `bool IsGreaterThan(this IPAddress alpha, IPAddress beta)`
 * `bool IsGreaterThanOrEqualTo(this IPAddress alpha, IPAddress beta)`
 * `bool IsLessThan(this IPAddress alpha, IPAddress beta)`
 * `bool IsLessThanOrEqualTo(this IPAddress alpha, IPAddress beta)`


### Utilities

The *Utilities* package contains static classes for miscellaneous operations on specific types

#### `ByteArrayUtilities`

Static utility class containing miscellaneous operations for `IEnumerable<byte>` and byte arrays

#### `EnumerableUtilities`

Static utility class containing miscellaneous operations for generic `IEnumerable<T>`

#### `IPAddressUtilities`

Static utility class containing miscellaneous operations for `IPAddress` objects

Among other utilities the `IPAddressUtilities` class has static methods for parsing `IPAddresses` from other types

##### Address Family Detection

A couple of extension methods were created to quickly determine the address family of an IP Address. To determine if an address is IPv4 use `bool IsIPv4(this IPAddress ipAddress)`, likewise `bool IsIPv6(this IPAddress ipAddress)` can be used to test for IPv6.


##### Parsing

It is possible to parse an `IPAddress` from a hexadecimal string into either an IPv4 of IPv6 address using the `IPAddress ParseFromHexString(string input, AddressFamily addressFamily)` method. Likewise it can be done safely with `bool TryParseFromHexString(string input, AddressFamily addressFamily, out IPAddress address)`

Similarly, conversion may be done from an octal string by using `bool TryParseIgnoreOctalInIPv4(string input, out IPAddress address)` or even a `BigInteger` by way of `bool TryParse(BigInteger input, AddressFamily addressFamily, out IPAddress address)`


#### `SubnetUtilities`

Static utility class containing miscellaneous operations for `Subnet` objects that didn't make sense to put on the object itself.

Given two arbitrary IP Addresses of the same family it may be desired to calculate the fewest consecuitive subnetes that would hold the inclusive range between them. For example

```
    SubnetUtilities.FewestConsecutiveSubnetsFor(IPAddress.Parse("128.64.20.3"), IPAddress.Parse(128.64.20.12"))
```

would return an `Enumerable` containing the subnets "128.64.20.3/32", "128.64.20.4/30", "128.64.20.8/30", "128.64.20.12/32"

## Built With

* [NuGet]( https://www.nuget.org/) - Dependency Management
* [JetBrains.Annotations]( https://www.jetbrains.com/help/resharper/10.0/Code_Analysis__Code_Annotations.html) - Used to keep developers honest

## Versioning

We use [SemVer](http://semver.org/) for versioning.

## Authors and Contributors

* **Robert Engelhardt** - *Initial work* - [@rheone]( https://twitter.com/rheone)
* **Andrew Steele** - *Review and Suggestions* - [@ahsteele]( https://twitter.com/ahsteele)

## Copyright

```
Copyright 2018 National Technology & Engineering Solutions of
Sandia, LLC (NTESS). Under the terms of Contract DE-NA0003525 with NTESS, the
U.S. Government retains certain rights in this software.

For five (5) years from  the United States Government is granted for itself and
others acting on its behalf a paid-up, nonexclusive, irrevocable worldwide
license in this data to reproduce, prepare derivative works, and perform
publicly and display publicly, by or on behalf of the Government. There is
provision for the possible extension of the term of this license. Subsequent to
that period or any extension granted, the United States Government is granted
for itself and others acting on its behalf a paid-up, nonexclusive, irrevocable
worldwide license in this data to reproduce, prepare derivative works,
distribute copies to the public, perform publicly and display publicly, and to
permit others to do so. The specific term of the license can be identified by
inquiry made to National Technology and Engineering Solutions of Sandia, LLC or
DOE.
 
NEITHER THE UNITED STATES GOVERNMENT, NOR THE UNITED STATES DEPARTMENT OF
ENERGY, NOR NATIONAL TECHNOLOGY AND ENGINEERING SOLUTIONS OF SANDIA, LLC, NOR
ANY OF THEIR EMPLOYEES, MAKES ANY WARRANTY, EXPRESS OR IMPLIED, OR ASSUMES ANY
LEGAL RESPONSIBILITY FOR THE ACCURACY, COMPLETENESS, OR USEFULNESS OF ANY
INFORMATION, APPARATUS, PRODUCT, OR PROCESS DISCLOSED, OR REPRESENTS THAT ITS
USE WOULD NOT INFRINGE PRIVATELY OWNED RIGHTS.
 
Any licensee of this software has the obligation and responsibility to abide by
the applicable export control laws, regulations, and general prohibitions
relating to the export of technical data. Failure to obtain an export control
license or other authority from the Government may result in criminal liability
under U.S. laws.
```

## License

```
Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
```
