.. _AbstractIPAddressRange:

AbstractIPAddressRange
======================

The ``AbstractIPAddressRange`` is an abstract implementation of :ref:`IIPAddressRange`. It is extended by both :ref:`IPAddressRange`, and :ref:`Subnet`.

Functionality Implementation
----------------------------

IFormatable
^^^^^^^^^^^

Extensions of ``AbstractIPAddressRange``, depending on overrides and implementation, provide a general format (``G``, ``g``, or empty string) that will express a range of IP addresses in a ``head - tail`` format for example ``192.168.1.1 - 192.168.1.10``.

.. code-block:: c#
   :emphasize-lines: 12
   :caption: AbstractIPAddressRange  IFormattable Example
   :name: AbstractIPAddressRange  IFormattable Example

   [Fact]
   public void IFormattable_Example()
   {
       // Arrange
       var head = IPAddress.Parse("192.168.0.0");
       var tail = IPAddress.Parse("192.168.128.0");
       var ipAddressRange = new IPAddressRange(head, tail);

       const string expected = "192.168.0.0 - 192.168.128.0";

       // Act
       var formattableString = string.Format("{0:g}", ipAddressRange);

       // Assert
       Assert.Equal(expected, formattableString);
   }
