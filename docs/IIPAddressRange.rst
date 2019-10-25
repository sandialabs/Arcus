IIPAddressRange
===============

``IIPAddressRange`` is an interface defining a contract for all of Arcus's implementations of a consecutive a range of ``IPAddress`` objects. It implements both ``IFormattable`` and ``IEnumerable<IPAddress>``

``IIPAddressRange`` is implemented by `AbstractIPAddressRange <AbstractIPAddressRange>`_\ , `IPAddressRange <IPAddressRange>`_\ , and `Subnet <Subnet>`_

Functionality Promises
----------------------

Set Based Operations
^^^^^^^^^^^^^^^^^^^^

Inherently a ``IIPAddressRange`` is simply a range of consecutive ``IPAddress`` objects, as such there is some set based operations available.

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

``Touches`` will return ``true`` if the ``tail`` of ``this IIPAddressRange`` is followed consecutively by the ``head`` of ``IIPAddressRange that``\ , or if the ``tail`` of ``IIPAddressRange that`` is followed consecutively by the ``head`` of ``this IIPAddressRange`` with out any additional ``IPAddress`` objects in between.

.. code-block:: c#

   bool Touches(IIPAddressRange that);

Length and TryGetLength
^^^^^^^^^^^^^^^^^^^^^^^

The ``IIPAddressRange`` implements ``IEnumerable<IPAddress>``\ , but because of the possible size of this range it may not always be safe to attempt to do a count or get the length in a traditional manner. A ``BigInteger`` ``Length`` property is provided as keep in mind the full range of IPv6Addresses is :math:`2^{128}` in length. That's :math:`3.4\times10^{38}` or over 340 undecillion. Certainly not something that should be iterated in order to be counted.

However, ``BigInteger``\ s aren't always the handiest things to drag around. Using the *magic* of math, the various implementations of ``TryGetLength`` will attempt to get the length of the range in a more portable manner if possible, returning ``true`` on success.

.. code-block:: c#

   bool TryGetLength(out int length);

.. code-block:: c#

   bool TryGetLength(out long length);

