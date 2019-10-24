# IIPAddressRange

`IIPAddressRange` is an interface defining a contract for all of Arcus's implementations of a consecutive a range of `IPAddress` objects. It implements both `IFormattable` and `IEnumerable<IPAddress>`

`IIPAddressRange` is implemented by [AbstractIPAddressRange](#AbstractIPAddressRange), [IPAddressRange](IPAddressRange), and [Subnet](Subnet)

## Functionality Promises

### Set Based Operations

Inherently a `IIPAddressRange` is simply a range of consecutive `IPAddress` objects, as such there is some set based operations available.

#### HeadOverlappedBy

`HeadOverlappedBy` will return `true` if the `head` of `this IIPAddressRange` is within the `IIPAddressRange that`

```c#
bool HeadOverlappedBy(IIPAddressRange that);
```

#### TailOverlappedBy

`TailOverlappedBy` will return `true` if the `tail` of `this IIPAddressRange` is within the `IIPAddressRange that`

```c#
bool TailOverlappedBy(IIPAddressRange that);
```

#### Overlaps

`Overlaps` will return `true` if the `head` or `tail` of `IIPAddressRange that` is within the `this IIPAddressRange`

```c#
bool Overlaps(IIPAddressRange that);
```

#### Touches

`Touches` will return `true` if the `tail` of `this IIPAddressRange` is followed consecutively by the `head` of `IIPAddressRange that`, or if the `tail` of `IIPAddressRange that` is followed consecutively by the `head` of `this IIPAddressRange` with out any additional `IPAddress` objects in between.

```c#
bool Touches(IIPAddressRange that);
```

### Length and TryGetLength

The `IIPAddressRange` implements `IEnumerable<IPAddress>`, but because of the possible size of this range it may not always be safe to attempt to do a count or get the length in a traditional manner. A `BigInteger` `Length` property is provided as keep in mind the full range of IPv6Addresses is 2<sup>128</sup> in length. That's 3.4x10<sup>38</sup> or over 340 undecillion. Certainly not something that should be iterated in order to be counted.

However, `BigInteger`s aren't always the handiest things to drag around. Using the _magic_ of math, the various implementations of `TryGetLength` will attempt to get the length of the range in a more portable manner if possible, returning `true` on success.

```c#
bool TryGetLength(out int length);
```

```c#
bool TryGetLength(out long length);
```

# AbstractIPAddressRange

The `AbstractIPAddressRange` is an abstract implementation of [IIPAddressRange](#IIPAddressRange). It is extended by both [IPAddressRange](IPAddressRange), and [Subnet](Subnet).

## Functionality Implementation

### IFormatable

Extensions of `AbstractIPAddressRange`, depending on overrides and implementation, provide a general format (`G`, `g`, or empty string) that will express a range of IP addresses in a "`head` - `tail`" format.

```c#
[Fact]
public void IFormattable_Example()
{
    // Arrange
    var head = IPAddress.Parse("192.168.0.0");
    var tail = IPAddress.Parse("192.168.128.0");
    var ipAddressRange = new IPAddressRange(head, tail);

    const string expected = "192.168.0.0 - 192.168.128.0";

    // Act
    var formattableString = $"{ipAddressRange:g}";

    // Assert
    Assert.Equal(expected, formattableString);
}
```
