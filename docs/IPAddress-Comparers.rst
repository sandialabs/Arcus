
IPAddress Comparers
===================

|version| v5.0.0

IP Addresses are just numbers. Numbers are comparable. Some are bigger, some are smaller, some are even equal.

.. warning::

   **Removed in v5.0.0:** The ``DefaultIPAddressRangeComparer`` type has been removed. Use :ref:`DefaultIIPAddressRangeComparer` instead. This type was previously marked ``[Obsolete]`` and has been fully removed in v5.0.0.

.. _DefaultIPAddressComparer:

DefaultIPAddressComparer
^^^^^^^^^^^^^^^^^^^^^^^^

.. note:: the ``DefaultIPAddressComparer`` will gladly compare ``IPAddress`` of differing address families.

The ``DefaultIPAddressComparer`` extends ``Comparer<IPAddress>``. Its behavior is to first compare two ``IPAddress`` objects via the ``IComparer<AddressFamily>`` and then ordinally based on the ``IPAddress`` big-endian unsigned integer value.

By default the :ref:`DefaultAddressFamilyComparer` is used to compare the address families of the addresses, but that may be overridden by providing your own ``IComparer<AddressFamily>`` to the appropriate constructor

.. code-block:: c#

   public DefaultIPAddressComparer()

.. code-block:: c#

   public DefaultIPAddressComparer(IComparer<AddressFamily> addressFamilyComparer)
