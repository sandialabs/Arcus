.. _Subnet:

Subnet
======

The ``Subnet`` type, flavored in both IPv4 or IPv6, is a representation of a subnetwork within Arcus. Its is the work horse and original reason for the Arcus library. Outside the concept of the ``Subnet`` object, most everything else in Arcus is auxiliary and exists only in support of making this onc facet work. That’s not to say that the remaining pieces of the Arcus library aren’t useful, on the contrary their utility can benefit a developer greatly. But that said, once the dark and mysterious magic of the ``Subnet`` is understood the rest of Arcus should follow through nicely.

Keep in mind that a ``Subnet`` is not an arbitrary range of addresses, for that you want an :ref:`IPAddressRange` , but rather conforms to a range of length :math:`2^n` starting a particular position, following the typical rules of `Classless Inter-Domain Routing <https://en.wikipedia.org/wiki/Classless_Inter-Domain_Routing>`_.

The ``Subnet`` class extends :ref:`AbstractIPAddressRange` and implements :ref:`IIPAddressRange`, ``IEquatable<Subnet>``, ``IComparable<Subnet>``, ``IFormattable``, and ``IEnumerable<IPAddress>``.

.. note::  Be aware that ``Subnet`` does *not* extend :ref:`IPAddressRange` but does implement :ref:`IIPAddressRange`.

Creation
--------

There are a number of ways to instantiate a ``Subnet``. Your most likely candidates are direct construction with a ``new`` , the use of a static factory method on the ``Subnet`` class, or the use of sub-set of static factory methods that handle parsing of strings. Most of the factory methods have a "try" style safe alternative that will return a ``bool`` and *out* the constructed value.

.. note:: Unless otherwise specified each creation technique is valid for both IPv4 and IPv6 subnetworks.

constructor ``IPAddress lowAddress``, ``IPAddress highAddress``
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

To the most common ways to create a ``Subnet`` is to construct it via a ``IPAddress lowAddress`` and ``IPAddress highAddress``. This will construct the smallest possible ``Subnet`` that would contain both IP addresses. Typically, the address specified are the Network and Broadcast addresses (lower and higher bounds) but this is not necessary.

Addresses *MUST* be the same address family (either ``InterNetwork`` or ``InterNetworkV6`` ).

.. code-block:: c#

   public Subnet(IPAddress lowAddress, IPAddress highAddress)

constructor ``IPAddress address``, ``int routingPrefix``
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

It is also possible to create a ``Subnet`` from an ``IPAddress address`` and an ``int routingPrefix``. This is equivalent of programmatically using a `CIDR <https://en.wikipedia.org/wiki/Classless_Inter-Domain_Routing>`_ to define your ``Subnet``.

.. code-block:: c#

   public Subnet(IPAddress address, int routingPrefix)

constructor ``IPAddress address``
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

On the rare occasion it may be desired to make a ``Subnet`` comprised of a single ``IPAddress``. This is possible with the following constructor.

.. code-block:: c#

   public Subnet(IPAddress address)

factory IPAddress and NetMask
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

A once popular way to define a IPv4 subnetwork was to use a *netmask*\ , a specialized form of consecutive *bitmasking*\ , along side an ``IPAddress``.

The following factory methods may be used to create an IPv4 ``Subnet`` where as the ``IPAddress address`` is the address, and the ``IPAddress netmask`` is the valid *netmask*.

.. code-block:: c#

   public static Subnet FromNetMask(IPAddress address, IPAddress netmask)

.. code-block:: c#

   public static bool TryFromNetMask(IPAddress address, IPAddress netmask, out Subnet subnet)

factory From Big-Endian Byte Arrays
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

``IPAddress`` objects may not always be handy, in some cases only a couple of big-endian byte arrays may be available. This will construct the smallest possible ``Subnet`` that would contain both byte arrays as IP addresses. Typically, the address specified are the Network and Broadcast addresses (lower and upper bounds) but this is not necessary.

The given ``byte[]`` arrays are interpreted as being in big-endian ordering are are functionally the equivalent construction an ``IPAddress`` using its ``byte[]`` constructor.

.. code-block:: c#

   public static Subnet FromBytes(byte[] lowAddressBytes, byte[] highAddressBytes)

.. code-block:: c#

   public static bool TryFromBytes(byte[] lowAddressBytes, byte[] highAddressBytes, out Subnet subnet)

parse string
^^^^^^^^^^^^

It is pretty common to tote around a ``string`` as a representation of a subnet, but you needn't do such any longer. Assuming said ``string subnetString`` represents something roughly similar to a `CIDR <https://en.wikipedia.org/wiki/Classless_Inter-Domain_Routing>`_ Arcus will hand you a ``Subnet``.

If a representaion of an IP Address ``string`` is provided the resulting ``Subnet`` will consist of only that address.

.. code-block:: c#

   public static Subnet Parse(string subnetString)

.. code-block:: c#

   public static bool TryParse(string subnetString, out Subnet subnet)

parse IPAddress string and RoutingPrefix int
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

It is also possible to build  a ``Subnet`` from an ``String address`` and an ``int routingPrefix``.

.. code-block:: c#

   public static Subnet Parse(string addressString, int routingPrefix)

.. code-block:: c#

   public static bool TryParse(string addressString, int routingPrefix, out Subnet subnet)

parse IPAddress strings
^^^^^^^^^^^^^^^^^^^^^^^

A rather common way to to build a ``Subnet`` is to provide a pair of ``string`` objects, in this case a ``string lowAddress`` and ``string highAddress``. This will construct the smallest possible ``Subnet`` that would contain both IP addresses. Typically, the address specified are the Network and Broadcast addresses (lower and higher bounds) but this is not necessary.

.. code-block:: c#

   public static Subnet Parse(string lowAddressString, string highAddressString)

.. code-block:: c#

   public static bool TryParse(string lowAddressString, string highAddressString, out Subnet subnet)

Functionality
-------------

The ``Subnet`` implements :ref:`IIPAddressRange` , ``IEquatable<Subnet>`` , ``IComparable<Subnet>`` , ``IFormattable`` , and ``IEnumerable<IPAddress>`` , and there by contains all the expected functionality its inheritance.

Properties
^^^^^^^^^^

In addition to the properties defined in :ref:`IIPAddressRange` ``Subnet`` provides a few more additional options

:``IPAddress`` BroadcastAddress: An alias to the ``Tail`` property
:``IPAddress`` Netmask: The calculated netmask of the subnet, only valid for IPv4 based subnets. All others will be return a ``null`` value
:``IPAddress`` NetworkPrefixAddress: An alias to the ``Head`` property
:``int`` RoutingPrefix: The routing prefix used to specify the subnet
:``BigInteger`` UsableHostAddressCount: The number of usable addresses in the subnet ignoring both the Broadcast and Network addresses

Set Based Operations
^^^^^^^^^^^^^^^^^^^^

Inherently a ``Subnet`` is a range of ``IPAddress`` objects, as such there is some set based operations available.

In addition to the set based operations promised by :ref:`IIPAddressRange` , the ``Subnet`` type also has a few new options.

Contains
~~~~~~~~

It is possible to easily check if a subnet is entirely encapsulates another subnet by using the ``Contains`` method on the larger ``Subnet``.

.. code-block:: c#

   public bool Subnet.Contains(Subnet subnet)

In the following example it is shown that ``192.168.1.0/8`` contains ``192.168.0.0`` , but as expected ``192.168.1.0/8`` does not contain ``255.0.0.0/8``

.. code-block:: c#
   :emphasize-lines: 10-11
   :caption: Subnet Contains Example
   :name: Subnet Contains Example

   [Fact]
   public void Contains_Example()
   {
       // Arrange
       var subnetA = Subnet.Parse("192.168.1.0", 8);   // 192.0.0.0 - 192.255.255.255
       var subnetB = Subnet.Parse("192.168.0.0", 16);  // 192.168.0.0 - 192.168.255.255
       var subnetC = Subnet.Parse("255.0.0.0", 8);     // 255.0.0.0 - 255.255.255.255

       // Assert
       Assert.True(subnetA.Contains(subnetB));
       Assert.False(subnetA.Contains(subnetC));
   }

Overlaps
~~~~~~~~

It is possible determine if a subnet in any way overlaps another subnet, even if just by a single address, by using the ``Contains`` between two subnets.

This is a transitive operation, so if ``Subnet A`` overlaps ``Subnet B`` then B overlaps A as well.

.. code-block:: c#

   public bool Overlaps(Subnet subnet)

In the following example it is shown that ``255.255.0.0/16`` and ``0.0.0.0/0`` each overlap each other. However, due to their disparate address families, ``::/0`` and ``0.0.0.0/0`` do not overlap despite being equivalent ranges in the differing in integer spaces.

.. code-block:: c#
   :emphasize-lines: 12-15
   :caption: Subnet Overlaps Example
   :name: Subnet Overlaps Example

   [Fact]
   public void Overlaps_Example()
   {
      // Arrange
      var ipv4SubnetA = Subnet.Parse("255.255.0.0", 16);
      var ipv4SubnetB = Subnet.Parse("0.0.0.0", 0);

      var ipv6SubnetA = Subnet.Parse("::", 0);
      var ipv6SubnetB = Subnet.Parse("abcd:ef01::", 64);

      // Act
      Assert.True(ipv4SubnetA.Overlaps(ipv4SubnetB));
      Assert.True(ipv4SubnetB.Overlaps(ipv4SubnetA));
      Assert.True(ipv6SubnetA.Overlaps(ipv6SubnetB));
      Assert.False(ipv6SubnetA.Overlaps(ipv4SubnetA));
   }

IFormatable
^^^^^^^^^^^

``Subnet`` offers a number or preexisting formats that are accessible via the standard ``ToString`` method provided by ``IFormattable``

.. csv-table:: Subnet format values
   :file: subnet-formats.csv
   :header-rows: 1
