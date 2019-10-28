.. _IIPAddressRange:

IIPAddressRange
===============

``IIPAddressRange`` is an interface defining a contract for all of Arcus' implementations of a consecutive a range of ``IPAddress`` objects. It implements both ``IFormattable`` and ``IEnumerable<IPAddress>``

.. caution:: ``IIPAddressRange`` implements ``IEnumerable<IPAddress>``, this means that you should pay particular attention when you may be iterating over large ranges. Such as the full set of IPv6 addresses. That'll take a while. A long while. It isn't recommended.

.. hint:: When dealing with more than one ``IPAddress`` or multiple implementations of ``IIPAddressRange`` unless otherwise explicitly stated their ``AddressFamily`` , or equivalent properties, **must** match.

.. hint:: ``AddressFamily`` unless otherwise explicitly stated are expected to be either ``InterNetwork`` or ``InterNetworkV6``.

``IIPAddressRange`` is implemented by :ref:`AbstractIPAddressRange` , :ref:`IPAddressRange` , and :ref:`Subnet`

Functionality Promises
----------------------

Properties
^^^^^^^^^^

``IIPAddressRange`` has a handful of hopefully useful properties for your use

:``AddressFamily`` AddressFamily: The family of the Address Range. You'll most likely encounter ``InterNetwork`` or ``InterNetworkV6``
:``IPAddress`` Head: The first ``IPAddress`` within the range
:``bool`` IsIPv4: Returns ``true`` iff the range is IPv4
:``bool`` IsIPv6: Returns ``true`` iff the range is IPv6
:``bool`` IsSingleIP: Returns ``true`` iff the range is comprised of only a single ``IPAddress``
:``BigInteger`` Length: The number of ``IPAddress`` within the range
:``IPAddress`` Tail: The last ``IPAddress`` within the range


Set Based Operations
^^^^^^^^^^^^^^^^^^^^

Inherently a ``IIPAddressRange`` is simply a range of consecutive ``IPAddress`` objects, as such there are some set based operations available.

HeadOverlappedBy
~~~~~~~~~~~~~~~~

``HeadOverlappedBy`` will return ``true`` if the ``head`` of ``this IIPAddressRange`` is within the ``IIPAddressRange that``

.. code-block:: c#

   bool HeadOverlappedBy(IIPAddressRange that);

TailOverlappedBy
~~~~~~~~~~~~~~~~

``TailOverlappedBy`` will return ``true`` if the ``tail`` of ``this IIPAddressRange`` is within the ``IIPAddressRange that``

.. code-block:: c#

   bool TailOverlappedBy(IIPAddressRange that);

Overlaps
~~~~~~~~

``Overlaps`` will return ``true`` if the ``head`` or ``tail`` of ``IIPAddressRange that`` is within the ``this IIPAddressRange``

.. code-block:: c#

   bool Overlaps(IIPAddressRange that);

Touches
~~~~~~~

``Touches`` will return ``true`` if the ``tail`` of ``this IIPAddressRange`` is followed consecutively by the ``head`` of ``IIPAddressRange that`` , or if the ``tail`` of ``IIPAddressRange that`` is followed consecutively by the ``head`` of ``this IIPAddressRange`` without any additional ``IPAddress`` objects in between.

.. code-block:: c#

   bool Touches(IIPAddressRange that);

Length and TryGetLength
^^^^^^^^^^^^^^^^^^^^^^^

The ``IIPAddressRange`` implements ``IEnumerable<IPAddress>``, but because of the possible size of this range it may not always be safe to attempt to do a count or get the length in a traditional manner. A ``BigInteger Length`` property is provided but not always ideal but often necessary. Keep in mind the full range of IPv6 Addresses is :math:`2^{128}` in length. That's :math:`3.4\times10^{38}` or over 340 undecillion. Certainly not something that should be iterated in order to be counted.

Given that the ``BigInteger`` object isn't always the handiest things to drag around Arcus uses the *magic* of math and with the various implementations of ``TryGetLength`` will attempt to get the length of the range in a more portable manner if possible, returning ``true`` on success and outing the more reasonable  ``int`` or ``long`` length.

.. code-block:: c#

   bool TryGetLength(out int length);

.. code-block:: c#

   bool TryGetLength(out long length);
