Subnet Utilities
================

``Arcus.Utilities.SubnetUtilities`` is a static utility class containing miscellaneous operations for :ref:`Subnet` and collections there of. It is a catchall for methods and functionally that didn't make sense on the ``Subnet`` class itself.

find Fewest Consecutive Subnets
-------------------------------

Given an inclusive range of IP Addresses defined by ``IPAddress left`` and ``IPAddress right`` get the fewest consecutive subnets that would contain all addresses within the range between and no other addresses.

.. code-block:: c#

   public static IEnumerable<Subnet> FewestConsecutiveSubnetsFor(IPAddress left, IPAddress right)

The following examples shows that the range defined by ``192.168.1.3`` - ``192.168.1.5`` fits in  two consecutive subnets defined by ``192.168.1.4/31`` and ``192.168.1.3/32``.

.. code-block:: c#
   :emphasize-lines: 9
   :caption: FewestConsecutiveSubnetsFor Example
   :name: FewestConsecutiveSubnetsFor Example

    [Fact]
    public void FewestConsecutiveSubnetsFor_Example()
    {
        // Arrange
        var left = IPAddress.Parse("192.168.1.3");
        var right = IPAddress.Parse("192.168.1.5");

        // Act
        var result = SubnetUtilities.FewestConsecutiveSubnetsFor(left, right);

        // Assert
        Assert.Equal(2, result.Length);
        Assert.Contains(Subnet.Parse("192.168.1.4/31"), result);
        Assert.Contains(Subnet.Parse("192.168.1.3/32"), result);
    }


find the Largest Subnet in an enumerable
----------------------------------------

The ``LargestSubnet` method, given an ``IEnumerable<Subnet>`` will select first largest subnet from within the collection.

.. note:: If there is no single largest in the input, the first largest subnet encountered will be returned. In cases such as this it may be preferable to consider usage of the ``DefaultSubnetComparer``.

.. code-block:: c#

   public static Subnet LargestSubnet(IEnumerable<Subnet> subnets)

The following example provides that given the several oddly named sizes of subnets that *trenta*, composed of 1048576 addresses, is both largest and probably more caffeine than your originally anticipated.

.. code-block:: c#
   :emphasize-lines: 13
   :caption: LargestSubnet Example
   :name: LargestSubnet Example

   [Fact]
   public void LargestSubnet_Example()
   {
       // Arrange
       var tall = Subnet.Parse("255.255.255.254/31");  // 2^1 = 2
       var grande = Subnet.Parse("192.168.1.0/24");    // 2^8 = 256
       var vente = Subnet.Parse("10.10.0.0/16");       // 2^16 = 65536
       var trenta = Subnet.Parse("16.240.0.0/12");     // 2^20 = 1048576

       var subnets = new[] { tall, grande, vente, trenta };

       // Act
       var result = SubnetUtilities.LargestSubnet(subnets);

       // Assert
       Assert.Equal(trenta, result);
   }


find the Smallest Subnet in an enumerable
-----------------------------------------

The ``SmallestSubnet`` method, given an ``IEnumerable<Subnet>`` will select the first smallest subnet from within the collection.

.. note:: If there is no single smallest in the input, the first smallest subnet encountered will be returned. In cases such as this it may be preferable to consider usage of the ``DefaultSubnetComparer``.

.. code-block:: c#

   public static Subnet SmallestSubnet(IEnumerable<Subnet> subnets)

The included example shows that given the several seemingly familiar named subnets that *tall*, composed of 2 addresses, is not only the smallest, but likely will cost you a few bucks and taste a bit burnt.

.. code-block:: c#
   :emphasize-lines: 13
   :caption: SmallestSubnet Example
   :name: SmallestSubnet Example

   [Fact]
   public void SmallestSubnet_Example()
   {
       // Arrange
       var tall = Subnet.Parse("255.255.255.254/31");  // 2^1 = 2
       var grande = Subnet.Parse("192.168.1.0/24");    // 2^8 = 256
       var vente = Subnet.Parse("10.10.0.0/16");       // 2^16 = 65536
       var trenta = Subnet.Parse("16.240.0.0/12");     // 2^20 = 1048576

       var subnets = new[] { tall, grande, vente, trenta };

       // Act
       var result = SubnetUtilities.SmallestSubnet(subnets);

       // Assert
       Assert.Equal(tall, result);
   }
