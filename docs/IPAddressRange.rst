.. _IPAddressRange:

IPAddress Range
===============

|version| v5.0.0

``IPAddressRange`` is a very basic implementation of an :ref:`AbstractIPAddressRange` used to represent an inclusive range of arbitrary IP Addresses of the same address family. It isn't restricted to a `CIDR <https://en.wikipedia.org/wiki/Classless_Inter-Domain_Routing>`_ representation like a :ref:`Subnet` is, allowing for non-power of two range sizes.

The ``IPAddressRange`` class extends :ref:`AbstractIPAddressRange` and implements :ref:`IIPAddressRange`, ``IEquatable<IPAddressRange>``, ``IComparable<IPAddressRange>``, ``IFormattable``, and ``ISerializable``.

.. warning::

   ``IEnumerable<IPAddress>`` (and ``GetEnumerator()``) are **deprecated in v5.0.0** and will be **removed in v6.0.0**. Use :ref:`ToIPAddresses` instead. See :ref:`IIPAddressRange` for details.

.. note::

   Beginning in v5.0.0, **all constructors and factory methods** accept an optional ``maxEnumerationExponent`` parameter (default 12) that controls enumeration limits. See the constructor signatures below.

Creation
--------

constructor ``IPAddress head``, ``IPAddress tail``
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

To standard way of creating an ``IPAddressRange`` is to construct it via a ``IPAddress head`` and ``IPAddress tail``. This will construct an ``IPAddressRange`` that would inclusively start with the provided ``head`` and end with ``tail``.

Addresses *MUST* be the same address family (either ``InterNetwork`` or ``InterNetworkV6``).

.. code-block:: c#

   public IPAddressRange(IPAddress head, IPAddress tail, int maxEnumerationExponent = 12)

The optional ``maxEnumerationExponent`` parameter controls the enumeration cap: ``ToIPAddresses()`` will yield at most ``2^maxEnumerationExponent`` addresses (default 4096).

constructor ``IPAddress address``
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

On the rare occasion it may be desirable to make a ``IPAddressRange`` comprised of a single ``IPAddress``. This too is possible with the following constructor.

.. code-block:: c#

   public IPAddressRange(IPAddress address, int maxEnumerationExponent = 12)

Static Functionality
--------------------

TryCollapseAll
^^^^^^^^^^^^^^

``TryCollapseAll`` attempts to or collapse the given input of ``IEnumerable<IPAddressRange> ranges`` into as few ranges as possible thus minifying the number or ranges supporting the same data.

Ranges may be collapsed if, and only if, they either overlap, or touch each other and they share the same ``AddressFamily``.

The function call will return ``true`` if it could collapse two or more ranges. Regardless of if a collapse was possible the *out* value for ``result`` will be comprised of an ``IEnumerable<IPAddressRange>`` of the calculated ranges.

.. code-block:: c#

   public static bool TryCollapseAll(IEnumerable<IPAddressRange> ranges, out IEnumerable<IPAddressRange> result, int maxEnumerationExponent = 12)

The optional ``maxEnumerationExponent`` parameter controls the enumeration cap for any ranges created during collapse.

.. note::

   **v5.0.0 behavior correction — TryExcludeAll boundary guards:** When an exclusion ends at the family maximum address (e.g., ``255.255.255.255`` for IPv4), the method returns ``(true, leading segment)`` or ``(true, [])`` instead of throwing ``InvalidOperationException``. The trailing-segment path is guarded symmetrically for family-minimum exclusions.

The following example shows that the three touching ranges of ``192.168.1.0 - 192.168.1.5``, ``192.168.1.6 - 192.168.1.7``, and ``192.168.1.8 - 192.168.1.20`` were collapsed into the new ``IPAddressRange`` of ``192.168.1.0 - 192.168.1.20``.

.. code-block:: c#
   :emphasize-lines: 13
   :caption: IPAddressRange TryCollapseAll Example
   :name: IPAddressRange TryCollapseAll Example

   [Fact]
   public void TryCollapseAll_Consecutive_Example()
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

``TryExcludeAll`` is a tricky beast, but if you're willing to take the time to tame it'll not only respect you, but it may also take care of you in very specific cases. The method takes a ``IPAddressRange initialRange`` and with that it attempts to systematically remove each of the sub ranges defined within ``IEnumerable<IPAddressRange> excludedRanges``. On success, the operation returns ``true`` and will *out* an ``IEnumerable<IPAddressRange> result`` which is comprised of a distinct remaining ranges after ``excludedRanges`` have been carved out.

.. note::

   A return value of ``false`` signals an error condition (null input, address-family mismatch, or null elements in ``excludedRanges``), **not** an empty result set. A ``true`` return with an empty ``result`` means the exclusions cover the entire ``initialRange``.

   **Family-boundary behavior:** when an exclusion ends at the family maximum address (e.g., ``255.255.255.255`` for IPv4) no trailing segment can be produced; the method returns ``true`` with only the leading segment (which may itself be empty). When an exclusion starts at the family minimum address (e.g., ``0.0.0.0``) no leading segment can be produced; the method returns ``true`` with only the trailing segment.

.. code-block:: c#

   public static bool TryExcludeAll(IPAddressRange initialRange, IEnumerable<IPAddressRange> excludedRanges, out IEnumerable<IPAddressRange> result, int maxEnumerationExponent = 12)

TryMerge
^^^^^^^^

``TryMerge`` will take the input of ``IPAddressRange left`` and ``IPAddressRange right``, and if the two ranges touch or overlap, regardless of order, it will return ``true`` and *out* ``IPAddressRange mergedRange`` comprised of the now combined ranges sourcing its ``head`` from the lowest valued address of the two inputs and its ``tail`` from the highest valued address of the two.

.. code-block:: c#

   public static bool TryMerge(IPAddressRange left, IPAddressRange right, out IPAddressRange mergedRange, int maxEnumerationExponent = 12)
