IP Address Range Comparers
==========================

|version| v5.0.0

.. warning::

   **Removed in v5.0.0:** The ``DefaultIPAddressRangeComparer`` type has been removed. Use :ref:`DefaultIIPAddressRangeComparer` (documented below) instead. This type was previously marked ``[Obsolete]`` and has been fully removed in v5.0.0.

Unsurprisingly, sometimes it is necessary to compare an :ref:`IIPAddressRange` to another. For that an implementation of a ``Comparer<IIPAddressRange>`` is just what the code monkey ordered.

.. _DefaultIIPAddressRangeComparer:

DefaultIIPAddressRangeComparer
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

.. note:: the ``DefaultIIPAddressRangeComparer`` will happily compare ``IIPAddressRange`` of differing address families.

The ``DefaultIIPAddressRangeComparer`` is a ``Comparer<IIPAddressRange>`` that compares implementations of ``IIPAddressRange`` first by their ``IIPAddressRange.Head`` and then by their total length.

By default the two ``IIPAddressRange.Head`` values are compared via the :ref:`DefaultIPAddressComparer`, but that may be overridden by providing your own ``IComparer<IPAddress>`` to the appropriate constructor.

.. code-block:: c#

   public DefaultIIPAddressRangeComparer()

.. code-block:: c#

   public DefaultIIPAddressRangeComparer(IComparer<IPAddress> ipAddressComparer)
