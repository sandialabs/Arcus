IP Address Utilities
====================

``Arcus.Utilities.IPAddressUtilities`` is a static utility class containing miscellaneous methods and definitions for the ``IPAddress`` object.

Useful Values
^^^^^^^^^^^^^
Included within are some handy-dandy constant values and static readonly properties:

:``int`` IPv4BitCount: The number of bits in an IPv4 address (``32``)
:``int`` IPv4ByteCount: The number of bytes in an IPv4 address (``4``)
:``int`` IPv4OctetCount: The number of octets in an IPv4 address (``4``)
:``int`` IPv6BitCount: The number of bits in an IPv6 address (``128``)
:``int`` IPv6ByteCount: The number of bytes in an IPv6 address (``16``)
:``int`` IPv6HextetCount: The number of hextets in an IPv6 address (``8``)
:``IPAddress`` IPv4MaxAddress: The maximum IPv4 Address value (``0.0.0.0``)
:``IPAddress`` IPv4MinAddress: The minimum IPv4 Address value (``255.255.255.255``)
:``IPAddress`` IPv6MaxAddress: The maximum IPv6 value (``ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff``)
:``IPAddress`` IPv6MinAddress: The minimum IPv6 value (``::``)
:``IReadOnlyCollection<AddressFamily>`` ValidAddressFamilies: The standard valid ``AddressFamily`` values (``InterNetwork`` and ``InterNetworkV6``)

Methods
^^^^^^^

Minimum and Maximum Address
---------------------------

Given an instance of ``AddressFamily`` the ``MinIPAddress`` and ``MaxIPAddress`` methods will return the minimum value of an address with the ``AddressFamily`` or the maximum value respectively.

.. warning:: these methods are only valid for ``InterNetwork`` and ``InterNetworkV6``

.. code-block:: c#

    public static IPAddress MinIPAddress(this AddressFamily addressFamily)

.. code-block:: c#

    public static IPAddress MaxIPAddress(this AddressFamily addressFamily)

Address Family Detection
------------------------

Given an instance of ``IPAddress ipAddress`` the ``IsIPv4`` and ``IsIPv6`` methods will return ``true`` if the given address has the address family ``InterNetwork`` or ``InterNetworkV6`` respectively.

.. code-block:: c#

    public static bool IsIPv4(this IPAddress ipAddress)

.. code-block:: c#

    public static bool IsIPv6(this IPAddress ipAddress)

Address Format Detection
------------------------

Arcus provides a few ways to detect the format of an ``IPAddress`` that isn't already built into the pre-existing C# packages.

IsIPv4MappedIPv6
++++++++++++++++

``IsIPv4MappedIPv6`` will return ``true`` if, and only if,``IPAddress ipAddress`` is an IPv4 addressed mapped to IPv6.

This check is made in accordance of in accordance to `RFC4291 - IP Version 6 Addressing Architecture <https://tools.ietf.org/html/rfc4291#section-2.5.5.2>`_ - 2.5.5.2. "IPv4-Mapped IPv6 Address."

.. code-block:: c#

    public static bool IsIPv4MappedIPv6(this IPAddress ipAddress)


IsValidNetMask
++++++++++++++

``IsValidNetMask`` checks if the given ``IPAddress netmask`` is a valid IPv4 netmask, if, and only if, it is then the method returns ``true``.

.. code-block:: c#

    public static bool IsValidNetMask(this IPAddress netmask)

Parsing
-------

Arcus provides a few more out of the box parsing mechanisms to convert different types of input into an ``IPAddress``.

Most of these new parsing routines have a "safe" method that will be prefixed by "Try" that will return ``true`` on a successful parsing and will *out* the ``IPAddress``.

Hexadecimal
+++++++++++

``ParseFromHexString`` and ``TryParseFromHexString`` will attempt to parse a hexadecimal ``string input`` as an IP Address of the given ``AddressFamily addressFamily``.

.. note:: Valid input must be comprised of only hexadecimal characters with an optional "0x" prefix. Input is case insensitive, and assumed to be in big-endian byte order. Zero valued most significant bytes will be ignored.

.. code-block:: c#

    public static IPAddress ParseFromHexString(string input, AddressFamily addressFamily)

.. code-block:: c#

    public static bool TryParseFromHexString(string input, AddressFamily addressFamily, out IPAddress address)

Octal
+++++

By Microsoft's implementation of the ``IPAddress.Parse(string)`` any string representation of an IP Address having a zero-valued most significant number in an octet position is interpreted as octal (base 8) rather than decimal (base 10). This isn't always a desired way to go about parsing values.

These methods convert an ``string input`` IPv4 address representation to ``IPAddress`` instance ignoring leading zeros (octal notation) of dotted quad format.

.. code-block:: c#

    public static IPAddress ParseIgnoreOctalInIPv4(string input)

.. code-block:: c#

    public static bool TryParseIgnoreOctalInIPv4(string input, out IPAddress address)

byte[]
++++++

The following ``byte[]`` parsing methods will attempt to convert a big-endian ordered byte array to an ``IPAddress`` automatically providing the appropriate number of zero-valued most significant bytes as needed to meet the desired address family.

.. note:: This implementation differs from the constructor implementation on ``IPAddress`` that takes ``byte[]`` as input. Said constructor takes an explicit sized byte array and will outright fail if the input isn't explicitly 4 or 16 bytes long.

.. code-block:: c#

    public static IPAddress Parse(byte[] input, AddressFamily addressFamily)


.. code-block:: c#

    public static bool TryParse(byte[] input, AddressFamily addressFamily, out IPAddress address)
