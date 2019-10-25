.. role:: raw-html-m2r(raw)
   :format: html


Subnet
======

The ``Subnet`` type, flavored in both IPv4 or IPv6, is a representation of a subnetwork. Its is the work horse and original reason for the Arcus library. Outside the concept of the ``Subnet`` object, most everything else in Arcus is auxiliary and exists only in support of making this once facet work. That’s not to say that the remaining pieces of the Arcus library aren’t useful, on the contrary their utility can benefit a developer greatly. But that said, once the dark and mysterious magic of the ``Subnet`` is understood the rest of Arcus should follow through nicely.

Keep in mind that a ``Subnet`` is not an arbitrary range of addresses, for that you want an `IPAddress Range <IPAddressRange>`_\ , but rather conforms to a range of length 2\ :raw-html-m2r:`<sup>`\ n</sub> starting a particular position, often expressed by a `CIDR <https://en.wikipedia.org/wiki/Classless_Inter-Domain_Routing>`_.

The ``Subnet`` class extends `<AbstractIPAddressRange>`_ and implements `<IIPAddressRange>`_, ``IEquatable<Subnet>``, ``IComparable<Subnet>``, ``IFormattable``, and ``IEnumerable<IPAddress>``.

Note that ``Subnet`` does *not* extend `<IPAddressRange>`_ but does implement `<IIPAddressRange>`_.

Creation
--------

There are a number of ways to instantiate a ``Subnet``. Your most likely candidates are direct construction with a ``new``\ , the use of a static factory method on the ``Subnet`` class, or the use of sub-set of static factory methods that handle parsing of strings. Most of the factory methods have a "try" style safe alternative that will return a ``bool`` and out the constructed value.

Unless otherwise specified each creation technique is valid for both IPv4 and IPv6 subnetworks.

Ctor ``IPAddress``\ , ``IPAddress``
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

To the most common ways to create a ``Subnet`` is to construct it via a ``IPAddress lowAddress`` and ``IPAddress highAddress``. This will construct the smallest possible ``Subnet`` that would contain both IP addresses. Typically, the address specified are the Network and Broadcast addresses (lower and higher bounds) but this is not necessary.

Addresses *MUST* be the same address family (either ``Internetwork`` or ``InternetworkV6``\ ).

.. code-block:: c#

   public Subnet(IPAddress lowAddress, IPAddress highAddress)

Ctor ``IPAddress``\ , ``int``
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

It is also possible to create a ``Subnet`` from an ``IPAddress address`` and an ``int routingPrefix``. This is equivalent of programmatically using a `CIDR <https://en.wikipedia.org/wiki/Classless_Inter-Domain_Routing>`_ to define your ``Subnet``.

.. code-block:: c#

   public Subnet(IPAddress address, int routingPrefix)

Ctor ``IPAddress``
^^^^^^^^^^^^^^^^^^^^^^

On the rare occasion it may be desired to make a ``Subnet`` comprised of a single ``IPAddress``. This too is possible with the following constructor.

.. code-block:: c#

   public Subnet(IPAddress address)

Factory IPAddress and NetMask
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

A once popular way to define a IPv4 subnetwork was to use a *netmask*\ , a specialized form of consecutive *bitmasking*\ , along side an ``IPAddress``.

The following factory methods may be used to create an IPv4 ``Subnet`` where as the ``IPAddress address`` is the address, and the ``IPAddress netmask`` is the valid *netmask*.

.. code-block:: c#

   public static Subnet FromNetMask(IPAddress address, IPAddress netmask)

.. code-block:: c#

   public static bool TryFromNetMask(IPAddress address, IPAddress netmask, out Subnet subnet)

Factory FromBytes
^^^^^^^^^^^^^^^^^

``IPAddress`` objects may not always be handy, in some cases only a couple of big-endian byte arrays may be available. This will construct the smallest possible ``Subnet`` that would contain both byte arrays as IP addresses. Typically, the address specified are the Network and Broadcast addresses (lower and higher bounds) but this is not necessary.

Both byte arrays *MUST* represent be the same address family (either ``Internetwork`` or ``InternetworkV6``\ ).

.. code-block:: c#

   public static Subnet FromBytes(byte[] lowAddressBytes, byte[] highAddressBytes)

.. code-block:: c#

   public static bool TryFromBytes(byte[] lowAddressBytes, byte[] highAddressBytes, out Subnet subnet)

Parse Subnet String
^^^^^^^^^^^^^^^^^^^

It is pretty common to tote around a ``string`` as a representation of a subnet, but you needn't do such any longer. Assuming said ``string subnetString`` represents something roughly similar to a `CIDR <https://en.wikipedia.org/wiki/Classless_Inter-Domain_Routing>`_ Arcus will hand you a ``Subnet``.

.. code-block:: c#

   public static Subnet Parse(string subnetString)

.. code-block:: c#

   public static bool TryParse(string subnetString, out Subnet subnet)

Parse IPAddress String and RoutingPrefix int
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

It is also possible to parse a ``Subnet`` from an ``String address`` and an ``int routingPrefix``.

.. code-block:: c#

   public static Subnet Parse(string addressString, int routingPrefix)

.. code-block:: c#

   public static bool TryParse(string addressString, int routingPrefix, out Subnet subnet)

Parse IPAddress strings
^^^^^^^^^^^^^^^^^^^^^^^

A rather common way to to build a ``Subnet`` is to provide a pair of ``string`` objects, in this case a ``string lowAddress`` and ``string highAddress``. This will construct the smallest possible ``Subnet`` that would contain both IP addresses. Typically, the address specified are the Network and Broadcast addresses (lower and higher bounds) but this is not necessary.

Addresses *MUST* be the same address family (either ``Internetwork`` or ``InternetworkV6``\ ).

.. code-block:: c#

   public static Subnet Parse(string lowAddressString, string highAddressString)

.. code-block:: c#

   public static bool TryParse(string lowAddressString, string highAddressString, out Subnet subnet)

Functionality
-------------

The ``Subnet`` implements `<IIPAddressRange>`_\ , ``IEquatable<Subnet>``\ , ``IComparable<Subnet>``\ , ``IFormattable``\ , and ``IEnumerable<IPAddress>``\ , and there by contains all the expected functionality from such.

Set Based Operations
^^^^^^^^^^^^^^^^^^^^

Inherently a ``Subnet`` is a range of ``IPAddress`` objects, as such there is some set based operations available.

In addition to the `set based operations promised by ``IIPAddressRange`` <IIPAddressRange#Set-Based-Operations>`_\ , the ``Subnet`` type also has a few new options.

Contains
~~~~~~~~

It is possible to easily check if a subnet is entirely encapsulates another subnet by using the ``Contains`` method on the larger ``Subnet``.

.. code-block:: c#

   public bool Subnet.Contains(Subnet subnet)

In the following example it is shown that ``192.168.1.0/8`` contains ``192.168.0.0``\ , but as expected ``192.168.1.0/8`` does not contain ``255.0.0.0/8``

.. code-block:: c#

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

This is a transitive operation, if ``Subnet`` A overlaps ``Subnet`` B then B overlaps A.

.. code-block:: c#

   public bool Overlaps(Subnet subnet)

In the following example it is shown that ``255.255.0.0/16`` and ``0.0.0.0/0`` each overlap each other. However, due to their disparate address families, ``::/0`` and ``0.0.0.0/0`` do not overlap despite being equivalent ranges in the differing in integer spaces.

.. code-block:: c#

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

.. code-block:: c#

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
