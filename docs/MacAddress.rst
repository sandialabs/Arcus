.. _MacAddress:

MacAddress
==========

The ``MacAddress`` type represents a 48-bit MAC Address [#48-BitMAC]_ as per the IEEE EUI standard [#IEEE-Eui]_. It serves the purpose of a Networking Adjacent worker class, and as a handy way to represent, store, format, and compare MAC addresses.

The ``MacAddress`` class implements ``IEquatable<MacAddress>``, ``IComparable<MacAddress>``, ``IComparable``, ``IFormattable``, and ``ISerializable``.

.. note::

   Unless otherwise stated recognized readable MAC Address formats include only the following formats:

   -  IEEE 802 format for printing **EUI-48** and **MAC-48** addresses in six groups of two hexadecimal digits, separated by a dash (``-``). *E.g.* ``AA-BB-CC-DD-EE-FF``
   -  Common Six groups of two hexadecimal digits separated by colons (``:``). *E.g.* ``AA:BB:CC:DD:EE:FF``
   -  Six groups of two hexadecimal digits separated by a space character. *E.g.* ``AA BB CC DD EE FF``
   -  12 hexadecimal digits with no delimitation. *E.g.* ``AABBCCDDEEFF``
   -  Cisco three groups of four hexadecimal digits separated by dots (``.``). *E.g.* ``AABB.CCDD.EEFF``

   For the sake of parsing and reading these formats are case insensitive.

.. figure:: img/MAC-48_Address.svg
   :scale: 50 %
   :align: center
   :alt: MAC-48 Address.svg This file is licensed under the Creative Commons Attribution-Share Alike 2.5 Generic, 2.0 Generic and 1.0 Generic license.
   :target: https://commons.wikimedia.org/wiki/File:MAC-48_Address.svg

   Structure of a MAC-48 Address

Creation
--------

Constructor
^^^^^^^^^^^

``IEnumerable<byte>``
*********************

A new ``MacAddress`` may be constructed by providing an ``IEnumerable<byte>`` of six bytes to the constructor.

.. code-block:: c#

   public MacAddress(IEnumerable<byte> bytes)

Factory
^^^^^^^

Parse ``string``
****************

A ``MacAddress`` may also be created via either the ``Parse`` or safe ``TryParse`` method. Not that these methods are strict in that they will only succeed with a MAC address in a known format. If you wish to more liberally parse a string into a ``MacAddress`` see the ``ParseAny`` and ``TryParseAny`` defined below.

.. code-block:: c#

   public static MacAddress Parse(string input)

.. code-block:: c#

    public static bool TryParse(string input, out MacAddress macAddress)

ParseAny ``string``
*******************

``ParseAny`` and the safe ``TryParseAny`` allow the parsing of an arbitrary string that may be a Mac address into a ``MacAddress``. It looks for six hexadecimal digits within the string, joins them and interprets the result as consecutive big-endian hextets. If six, and only six, hexadecimal digits are not found the parse will fail. 

.. code-block:: c#

   public static MacAddress ParseAny(string input)

.. code-block:: c#

    public static bool TryParseAny(string input, out MacAddress macAddress)

Functionality
-------------    

Properties
^^^^^^^^^^
:``bool`` IsDefault: returns ``true`` if, and only if, the MAC Address is the EUI-48 default [#EUI-48Default]_, meaning all bits of the MAC Address are set making it equivalent to ``FF:FF:FF:FF:FF:FF``.
:``bool`` IsGloballyUnique: returns ``true`` if, and only if, is globally unique (OUI [#Eui-Oui]_ enforced).
:``bool`` IsLocallyAdministered: returns ``true`` if, and only if, is locally administered.
:``bool`` IsMulticast: returns ``true`` if, and only if, the MAC Address is multicast.
:``bool`` IsUnicast: returns ``true`` if, and only if, the MAC Address is unicast.
:``bool`` IsUnusable: returns ``true`` if, and only if, the MAC Address is "unusable", meaning all OUI bits of the MAC Address are unset.

:``MacAddress`` DefaultMacAddress: Provides a ``MacAddress`` that represents the default or ``null`` case MAC address.

:``Regex`` AllFormatMacAddressRegularExpression: Returns a regular expression for matching accepted MAC Address formats.
:``Regex`` CommonFormatMacAddressRegularExpression: Returns a regular expression for matching the "common" six groups of two uppercase hexadecimal digits format.

:``string`` AllFormatMacAddressPattern: Returns a regular expression pattern for matching accepted MAC Address formats.
:``string`` CommonFormatMacAddressPattern: Returns a regular expression pattern for matching the "common" six groups of two uppercase hexadecimal digits format.

Methods
^^^^^^^

GetAddressBytes
+++++++++++++++

``GetAddressBytes`` returns a copy of the underlying big-endian bytes of the ``MacAddress``. This will always be six bytes in length.

.. code-block:: c#

   public byte[] GetAddressBytes()

GetOuiBytes
+++++++++++

``GetOuiBytes`` returns the *Organizationally Unique Identifier (OUI)* [#Eui-Oui]_ of the ``MAcAddress``.

.. code-block:: c#

   public byte[] GetOuiBytes()

GetCidBytes
+++++++++++

``GetCidBytes`` returns the *Company ID (CID)* [#Eui-Cid]_ of the ``MAcAddress``.

.. code-block:: c#

   public byte[] GetCidBytes()

IFormatable
^^^^^^^^^^^

``MacAddress`` offers a number or preexisting formats that are accessible via the standard ``ToString`` method provided by ``IFormattable`` interface.

.. csv-table:: Subnet format values
   :file: macaddress-formats.csv
   :header-rows: 1

Operators
^^^^^^^^^

``MacAddress`` implements all the standard C# equality and comparison operators. The comparison operators treat the ``MacAddress`` bytes as an unsigned big-endian integer value.

.. rubric:: Footnotes

.. [#48-BitMAC] **48-Bit MAC** is a Media Access Control Address (MAC) following both the now deprecated *MAC-48* and the active *EUI-48* specifications.

.. [#EUI-48Default] The recommended null or default value for **EUI-48** is ``FF-FF-FF-FF-FF-FF``

.. [#IEEE-Eui] `Guidelines for Use of Extended Unique Identifier (EUI), Organizationally Unique Identifier (OUI), and Company ID (CID) <https://standards.ieee.org/content/dam/ieee-standards/standards/web/documents/tutorials/eui.pdf>`_ 

.. [#Eui-Oui] *Organizationally Unique Identifier (OUI)* is the first 3-bytes (24-bits) of a MAC-48 MAC Address.

.. [#Eui-Cid] *Company Id (Cid)*  is the last 3-bytes (24-bits) of a MAC-48 MAC Address.

.. [#EUI-Usable] Usable **EUI-48** values are based on a zeroed OUI. A all zero EUI value, such as  ``00-00-00-00-00-00`` and ``00-00-00-FA-BC-21``, according to spec shall not be used as an identifier.
