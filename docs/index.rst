Arcus
=====

.. image:: https://img.shields.io/github/actions/workflow/status/sandialabs/Arcus/build.yml?branch=main)
   :alt: GitHub Actions Workflow Status
.. image:: https://img.shields.io/nuget/v/Arcus
   :alt: nuget Version
   :target: https://www.nuget.org/packages/Arcus/
.. image:: https://img.shields.io/github/v/release/sandialabs/Arcus
   :alt: GitHub Release
   :target: https://github.com/sandialabs/Arcus/releases
.. image:: https://img.shields.io/github/v/tag/sandialabs/Arcus
   :alt: GitHub Tag
   :target: https://github.com/sandialabs/Arcus/tags
.. image:: https://img.shields.io/badge/.NET%20Standard%202.0%20|%20.NET%208.0%20|%20.NET%209.0-blue?logo=.net
   :alt: Targets .NET Standard 2.0, .NET 8, and .NET 9
.. image:: https://img.shields.io/github/license/sandialabs/Arcus?logo=apache
   :alt: Apache 2.0 License
   :target: https://github.com/sandialabs/Arcus/blob/main/LICENSE

Arcus is a C# manipulation library for calculating, parsing, formatting, converting, and comparing both IPv4 and IPv6 addresses and subnets. It accounts for 128-bit numbers on 32-bit platforms.

Arcus provides extension and helper methods for the pre-existing ``System.Net.IPAddress`` and other objects within that realm. It was created to fill in some of the gaps left by the absence of a representation of a :doc:`Subnet <Subnet>`. As more gaps were found, they were filled. Like all coding projects, Arcus is a work in progress. We rely on both our free time and our :doc:`community <Community>` in order to provide the best solution we can given the constraints we must conform to.

.. hint:: Chances are you're primarily here looking for the :doc:`Subnet <Subnet>` object.

Arcus heavily relies upon one of our other libraries `Gulliver <https://github.com/sandialabs/gulliver>`_, if you're interested in byte manipulation it is worth checking out.

.. warning::

    Breaking Changes in Version 3+

    In .NET versions up to and including .NET 4.8 (which corresponds to .NET Standard 2.0), stricter parsing rules were enforced for `IPAddress` according to the IPv6 specification.Specifically, the presence of a terminal '%' character without a valid zone index is considered invalid in these versions. As a result, the input `abcd::%` fails to parse, leading to a null or failed address parsing depending on `Parse`/`TryParse`. This behavior represents a breaking change from Arcus's previous target of .NET Standard 1.3. and may provide confusion for .NET 4.8 / .NET Standard 2.0 versions.

    In contrast, in newer versions of .NET, including .NET 8 and .NET 9, the parsing rules have been relaxed. The trailing '%' character is now ignored during parsing, allowing for inputs that would have previously failed.

    It is important to note that this scenario appears to be an extreme edge case, and developers should ensure that their applications handle `IPAddress` parsing appropriately across different target frameworks as expected.

    If in doubt it is suggested that IP Address based user input should be sanitized to meet your development needs.

.. toctree::
   :maxdepth: 1
   :caption: IP Addresses Ranges

   IIPAddressRange
   AbstractIPAddressRange
   IPAddressRange
   Subnet
   Subnet-Utilities
   IPAddressRange-Comparers

.. toctree::
   :maxdepth: 2
   :caption: IP Addresses

   IPAddress-Converters
   IPAddress-Math
   IPAddress-Utilities
   IPAddress-Comparers
   AddressFamily-Comparers

.. toctree::
   :maxdepth: 2
   :caption: Additional Tools

   MacAddress

.. toctree::
   :maxdepth: 1
   :caption: References

   FAQ
   Glossary
   Reference

.. toctree::
   :maxdepth: 2
   :caption: Development

   Community
   Acknowledgements
