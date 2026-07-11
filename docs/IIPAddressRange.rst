.. _IIPAddressRange:

IIPAddressRange
===============

|version| v5.0.0

Arcus defines the ``IIPAddressRange`` interface for representation of consecutive ``IPAddress`` objects. It implements ``IFormattable``.

.. warning::

   **Will be removed in v6.0.0:** ``IIPAddressRange`` currently implements ``IEnumerable<IPAddress>`` for backwards compatibility, but this will be **removed in v6.0.0**.

   **Why:** The ``IEnumerable<IPAddress>`` interface makes it too easy to accidentally enumerate astronomically large address spaces. Even with the enumeration cap, the interface contract itself encourages direct ``foreach`` usage that is semantically misleading — iterating over billions of items is rarely the intent.

   **Migration:** Replace ``foreach (var addr in range)`` with ``foreach (var addr in range.ToIPAddresses())`` now. This produces no warnings in v5 and will be **required** in v6.0.0.

   .. seealso:: :ref:`ToIPAddresses` for the replacement enumeration method.

.. hint:: When dealing with more than one ``IPAddress`` or multiple implementations of ``IIPAddressRange`` unless otherwise explicitly stated their ``AddressFamily``, or equivalent properties, **must** match.

.. hint:: ``AddressFamily`` unless otherwise explicitly stated are expected to be either ``InterNetwork`` or ``InterNetworkV6``.

``IIPAddressRange`` is implemented by :ref:`AbstractIPAddressRange`, :ref:`IPAddressRange`, and :ref:`Subnet`.

.. note::

   **New in v5.0.0:** All range types now accept an optional ``maxEnumerationExponent`` parameter (default 12) that caps enumeration at ``2^maxEnumerationExponent`` addresses (4096 by default). This prevents accidental enumeration of enormous address spaces. See :ref:`ToIPAddresses` for details.

Functionality Promises
----------------------

Properties
^^^^^^^^^^

``IIPAddressRange`` has a handful of useful properties for your use

:``AddressFamily`` AddressFamily: The family of the Address Range. You'll most likely encounter ``InterNetwork`` or ``InterNetworkV6``
:``IPAddress`` Head: The first ``IPAddress`` within the range
:``bool`` IsIPv4: Returns ``true`` if, and only if, the range is IPv4
:``bool`` IsIPv6: Returns ``true`` if, and only if, the range is IPv6
:``bool`` IsSingleIP: Returns ``true`` if, and only if, the range is comprised of only a single ``IPAddress``
:``BigInteger`` Length: The number of ``IPAddress`` within the range
:``int`` MaxEnumerationExponent: The exponent controlling the enumeration cap (0–128). Enumeration via ``ToIPAddresses()`` yields at most ``2^MaxEnumerationExponent`` addresses. Default is 12 (4096 addresses).
:``IPAddress`` Tail: The last ``IPAddress`` within the range

Enumeration via ToIPAddresses
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

.. _ToIPAddresses:

**New in v5.0.0:** The ``ToIPAddresses()`` method returns an ``IEnumerable<IPAddress>`` yielding addresses from ``Head`` to ``Tail``, capped at ``2^MaxEnumerationExponent``. This is the recommended way to enumerate range addresses in v5 and will be the **only** way in v6.0.0.

.. code-block:: c#

   IEnumerable<IPAddress> ToIPAddresses();

.. caution::

   If the range contains more addresses than the cap allows, ``InvalidOperationException`` is thrown at the point the limit is exceeded. Increase ``maxEnumerationExponent`` at construction time to enumerate larger ranges. A value of ``0`` disables enumeration entirely; ``128`` allows enumeration up to the full IPv6 space.

.. warning::

   **Obsolete in v5.0.0, will be removed in v6.0.0:** The ``GetEnumerator()`` method is now ``[Obsolete("Use ToIPAddresses() instead")]``. It still works by delegating to ``ToIPAddresses()`` but produces a compile-time warning.

   Replace all uses of ``foreach (var addr in range)`` with ``foreach (var addr in range.ToIPAddresses())`` now to prepare for v6.0.0.

Set Based Operations
^^^^^^^^^^^^^^^^^^^^

At its core an implementation of the ``IIPAddressRange`` interface is a range of consecutive ``IPAddress`` objects, as such there are some set based operations available.

HeadOverlappedBy
~~~~~~~~~~~~~~~~

``HeadOverlappedBy`` will return ``true`` if the ``head`` of ``this`` is within the range defined by ``IIPAddressRange that``.

.. code-block:: c#

   bool HeadOverlappedBy(IIPAddressRange that);

TailOverlappedBy
~~~~~~~~~~~~~~~~

``TailOverlappedBy`` will return ``true`` if the ``tail`` of ``this`` is within the range defined by ``IIPAddressRange that``.

.. code-block:: c#

   bool TailOverlappedBy(IIPAddressRange that);

Overlaps
~~~~~~~~

``Overlaps`` will return ``true`` if the ``head`` or ``tail`` of ``IIPAddressRange that`` is within the ``this IIPAddressRange``.

.. code-block:: c#

   bool Overlaps(IIPAddressRange that);

Touches
~~~~~~~

``Touches`` will return ``true`` if the ``tail`` of ``this IIPAddressRange`` is followed consecutively by the ``head`` of ``IIPAddressRange that``, or if the ``tail`` of ``IIPAddressRange that`` is followed consecutively by the ``head`` of ``this IIPAddressRange`` without any additional ``IPAddress`` objects in between.

.. code-block:: c#

   bool Touches(IIPAddressRange that);

Length and TryGetLength
^^^^^^^^^^^^^^^^^^^^^^^

The ``IIPAddressRange`` implements ``IEnumerable<IPAddress>``, but because of the possible size of this range it may not always be safe to attempt to do a count or get the length in a traditional manner. A ``BigInteger Length`` property is provided but not always ideal but often necessary. Keep in mind the full range of IPv6 Addresses is :math:`2^{128}` in length. That's :math:`3.4\times10^{38}` or over 340 undecillion. Certainly not something that should be iterated in order to be counted.

Given that the ``BigInteger`` object isn't the best thing to drag around Arcus uses the *magic* of math and with the various implementations of ``TryGetLength`` to get the length of the range in a more portable manner if possible, returning ``true`` on success and outing the more reasonable  ``int`` or ``long`` length.

.. code-block:: c#

   bool TryGetLength(out int length);

.. code-block:: c#

   bool TryGetLength(out long length);
