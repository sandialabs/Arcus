AddressFamily Comparers
=======================

AddressFamily comparers are simply classes that extend ``Comparer<AddressFamily>``.

.. _DefaultAddressFamilyComparer:

DefaultAddressFamilyComparer
^^^^^^^^^^^^^^^^^^^^^^^^^^^^

Behind the scenes ``AddressFamily`` is simply an ``enum``. Typically we're only concerned with ``InterNetwork``, with a value of 2, and ``InternNetworkV6`` which is valued at 23.

The ``DefaultAddressFamilyComparer`` is used to compare the address families of the addresses. No real magic here, we're simply comparing two ``AddressFamily`` values based on their inherit inherit value.

.. code-block:: c#
   :emphasize-lines: 4
   :caption: Compare Implementation
   :name: Compare Implementation

   public override int Compare(AddressFamily x,
                               AddressFamily y)
   {
       return x.CompareTo(y);
   }
