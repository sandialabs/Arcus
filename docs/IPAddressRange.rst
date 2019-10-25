
IPAddress Range
===============

``IPAddressRange``

A basic implementation of an `<IIPAddressRange>`_ used to represent an inclusive range of arbitrary IP Addresses of the same address family. It isn't restricted to a `CIDR <https://en.wikipedia.org/wiki/Classless_Inter-Domain_Routing>`_ representation like a `<Subnet>`_ is, allowing for non-power of two range sizes.

The ``IPAddressRange`` class extends `<IIPAddressRange#AbstractIPAddressRange>`_ and implements `<IIPAddressRange>`_\ , ``IEquatable<IPAddressRange>``\ , ``IComparable<IPAddressRange>``\ , ``IFormattable``\ , and ``IEnumerable<IPAddress>``.

Creation
--------

Ctor Head and Tail
^^^^^^^^^^^^^^^^^^

To standard way of creating an ``IPAddressRange`` is to construct it via a ``IPAddress head`` and ``IPAddress tail``. This will construct an ``IPAddressRange`` that would inclusively start with the provided ``head`` and end with ``tail``.

Addresses *MUST* be the same address family (either ``Internetwork`` or ``InternetworkV6``\ ).

.. code-block:: c#

   public IPAddressRange([NotNull] IPAddress head, [NotNull] IPAddress tail)

Ctor Single Address Range
^^^^^^^^^^^^^^^^^^^^^^^^^

On the rare occasion it may be desired to make a ``IPAddressRange`` comprised of a single ``IPAddress``. This too is possible with the following constructor.

.. code-block:: c#

   public IPAddressRange([NotNull] IPAddress address)

Static Functionality
--------------------

TryCollapseAll
^^^^^^^^^^^^^^

``TryCollapseAll`` attempts to or collapse the given input of ``IEnumerable<IPAddressRange> ranges`` into as few ranges as possible thus minifying the number or ranges supporting the same data.

Ranges may be collapsed if, and only if, they either overlap, or touch each other and they share the same ``AddressFamily``.

The function call will return ``true`` if it could collapse two or more ranges. Regardless of if a collapse was possible the out value for ``result`` will be comprised of an ``IEnumerable<IPAddressRange>`` of the calculated ranges.

.. code-block:: c#

   public static bool TryCollapseAll(IEnumerable<IPAddressRange> ranges, out IEnumerable<IPAddressRange> result)

The following example shows that the three touching ranges of ``192.168.1.0`` - ``192.168.1.5``\ , ``192.168.1.6`` - ``192.168.1.7``\ , and ``192.168.1.8`` - ``192.168.1.20`` was collapsed into the new ``IPAddressRange`` ``192.168.1.0`` - ``192.168.1.20``.

.. code-block:: c#

   [Fact]
   public void TryCollapseAll_Consecutive_Test()
   {
       // Arrange
       var ranges = new[]
                       {
                           new IPAddressRange(IPAddress.Parse("192.168.1.0"), IPAddress.Parse("192.168.1.5")),
                           new IPAddressRange(IPAddress.Parse("192.168.1.6"), IPAddress.Parse("192.168.1.7")),
                           new IPAddressRange(IPAddress.Parse("192.168.1.8"), IPAddress.Parse("192.168.1.20"))
                       };

       // Act
       var success = IPAddressRange.TryCollapseAll(ranges, out var results);
       var resultList = results?.ToList();

       // Assert
       Assert.True(success);
       Assert.NotNull(results);
       Assert.Single(resultList);

       var result = resultList.Single();

       Assert.Equal(IPAddress.Parse("192.168.1.0"), result.Head);
       Assert.Equal(IPAddress.Parse("192.168.1.20"), result.Tail);
   }

TryExcludeAll
^^^^^^^^^^^^^

.. code-block:: c#

   public static bool TryExcludeAll(IPAddressRange initialRange, IEnumerable<IPAddressRange> excludedRanges, out IEnumerable<IPAddressRange> result)

TryMerge
^^^^^^^^

.. code-block:: c#

   public static bool TryMerge(IPAddressRange left, IPAddressRange right, out IPAddressRange mergedRange)
