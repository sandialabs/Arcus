IP Address Range Comparers
==========================

Unsurprisingly, sometimes it is necessary to compare an :ref:`IIPAddressRange` to another. For that an implementation of a ``Comparer<IIPAddressRange>`` is just what the code monkey ordered.

.. _DefaultIPAddressRangeComparer:

DefaultIPAddressRangeComparer
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

.. note:: the ``DefaultIPAddressRangeComparer`` will happily compare ``IIPAddressRange`` of differing address families.

The ``DefaultIPAddressRangeComparer`` is a ``Comparer<IIPAddressRange>`` that compares implementations of ``IIPAddressRange`` first by their ``IIPAddressRange.Head`` and then by their total length.

By default the two ``IIPAddressRange.Head`` values are compared via the :ref:`DefaultIPAddressComparer`, but that may be overridden by providing your own ``IComparer<IPAddress>`` to the appropriate constructor.

.. code-block:: c#

   public DefaultIPAddressRangeComparer()

.. code-block:: c#

   public DefaultIPAddressRangeComparer(IComparer<IPAddress> ipAddressComparer)
