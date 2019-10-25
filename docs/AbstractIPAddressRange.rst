AbstractIPAddressRange
======================

The ``AbstractIPAddressRange`` is an abstract implementation of `IIPAddressRange <#IIPAddressRange>`_. It is extended by both `IPAddressRange <IPAddressRange>`_\ , and `Subnet <Subnet>`_.

Functionality Implementation
----------------------------

IFormatable
^^^^^^^^^^^

Extensions of ``AbstractIPAddressRange``\ , depending on overrides and implementation, provide a general format (\ ``G``\ , ``g``\ , or empty string) that will express a range of IP addresses in a "\ ``head`` - ``tail``\ " format.

.. code-block:: c#

   [Fact]
   public void IFormattable_Example()
   {
       // Arrange
       var head = IPAddress.Parse("192.168.0.0");
       var tail = IPAddress.Parse("192.168.128.0");
       var ipAddressRange = new IPAddressRange(head, tail);

       const string expected = "192.168.0.0 - 192.168.128.0";

       // Act
       var formattableString = $"{ipAddressRange:g}";

       // Assert
       Assert.Equal(expected, formattableString);
   }
