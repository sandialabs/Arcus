Glossary
========

.. glossary::

   Arcus
    Arcus is the lesser known Roman equivalent of the Greek goddess Iris. She is the Olympian messenger god. You know, because IP Addresses and Subnets are all about sending messages. Rainbows are cool too.

   AddressFamily
    The C# `AddressFamily <https://docs.microsoft.com/en-us/dotnet/api/system.net.sockets.addressfamily?view=netstandard-1.3>`_ is an enum that defines the type of an ``IPAddress``. Both IPAddress and Arcus are only concerned with ``InterNetwork`` an IPv4 address, and ``InterNetworkV6`` an IPv6 address.

   CIDR
    Short for **Classless Inter-Domain Routing**, is a way of expressing a range of IP addresses.

     `see CIDR on Wikipedia <https://en.wikipedia.org/wiki/Classless_Inter-Domain_Routing>`_

   Endianness
    Endianness referees to the ordering of bytes in the binary representation of data.

     `see Endianness on Wikipedia <https://en.wikipedia.org/wiki/Endianness>`_

   Big-Endian
    Big-Endian ordering, at times also referred to as *Network Byte Order*, is a left-to-right ordering of bytes where the left most bytes are most significant than right most.

    For example, the decimal value of the unsigned integer ``6060842`` may be represented as ``0x5C7B2A`` in hexadecimal. This hexadecimal value is composed of the three bytes ``0x5C``, ``0x7B``, and ``0x28``. As such the value ``6060842`` may be represented in Big-Endian as a byte array of ``[0x5C, 0x7B, 0x2A]``.

     `see Gulliver's What is Endianness <https://gulliver.readthedocs.io/en/latest/What-is-Endianness.html#what-is-endianness>`_

   Gulliver
     Gulliver is a C# utility package and library engineered for the manipulation of arbitrary sized byte arrays accounting for appropriate endianness and jagged byte length. It was developed by the same folks who created Arcus.

     `see Gulliver on GitHub <https://github.com/sandialabs/gulliver>`_

   IP Address
    Short for **Internet Protocol Address** it is a numeric representation that typically comes in two flavors IPv4 and IPv6.

     `see IP Address on Wikipedia <https://en.wikipedia.org/wiki/IP_address>`_

   IPv4
    IPv4 is an IP Address that follows version 4 of the Internet Protocol. It is a 32-bit number, four bytes, with :math:`2^{32}` distinct addresses. IPv4 Addresses are typically represented in a format referred to as *Dotted Quad* or *Quad-dotted* in which the four bytes making the address are delimited by a period (*.*) character in decimal big-endian order, such as ``192.168.1.0``.

     `see IPv4 on Wikipedia <https://en.wikipedia.org/wiki/IPv4>`_

   IPv6
    IPv6 is an IP Address following version 6 of the Internet Protocol. It is a 128-bit number, 16 bytes, with :math:`2^{128}` distinct addresses. It is typically expressed in a "human readable" [#IPv6HumanReadable]_ format in Big-Endian byte order typically with hextets delimited with colons and collapses, such as the equivalent ``fd04:f0bf:44a0:df4e::`` and ``fd04:f0bf:44a0:df4e:0000:0000:0000:0000``.

     `see IPv6 on Wikipedia <https://en.wikipedia.org/wiki/IPv6>`_

   Subnet
    Subnet, also known as **Subnetwork**, is a logical subdivision of an Internet Protocol network. Much like IP Addresses they come in both IPv4 and IPv6 flavors.

     `see Subnetwork on Wikipedia <https://en.wikipedia.org/wiki/Subnetwork>`_

.. rubric:: Footnotes

.. [#IPv6HumanReadable] And by "human readable" the author means a draconian format consisting of groupings of two byte hextets delimited by colons that aren't always two bytes long and sometimes the colons do funny things as do zeros, and oh yeah, occasionally the IPv4 dotted-quad format pops up and makes things even more interesting. `see RFC5952 <https://tools.ietf.org/html/rfc5952>`_.
