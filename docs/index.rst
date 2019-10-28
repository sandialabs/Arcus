Arcus
=====

.. image:: https://img.shields.io/nuget/vpre/arcus?logo=nuget
   :alt: Nuget (with prereleases)
   :target: https://www.nuget.org/packages/Arcus/
.. image:: https://img.shields.io/badge/GitHub-Arcus-lightgray?logo=github
   :alt: Visit us on GitHub
   :target: https://github.com/sandialabs/arcus
.. image:: https://img.shields.io/github/license/sandialabs/arcus?logo=apache
   :alt: Apache 2.0 License
   :target: https://github.com/sandialabs/Arcus/blob/master/LICENSE
.. image:: https://img.shields.io/badge/targets-.NETStandard%201.3-5C2D91?logo=.net
   :alt: .NetStandard 1.3
   :target: https://docs.microsoft.com/en-us/dotnet/standard/net-standard
.. image:: https://badges.gitter.im/sandialabs/Arcus.svg
   :alt: Join the chat at https://gitter.im/sandialabs/Arcus
   :target: https://gitter.im/sandialabs/Arcus?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge

Arcus is a C# manipulation library for calculating, parsing, formatting, converting, and comparing both IPv4 and IPv6 addresses and subnets. It accounts for 128-bit numbers on 32-bit platforms.

Arcus provides extension and helper methods for the pre-existing ``System.Net.IPAddress`` and other objects within that realm. It was created to fill in some of the gaps left by the absence of a representation of a :doc:`Subnet <Subnet>`. As more gaps were found, they were filled. Like nearly any new opensource project, Arcus is a work in progress. We rely on both our free time and our :doc:`community <Community>` in order to provide the best solution we can given the constraints we must conform to.

.. hint:: Chances are you're primarily here looking for the :doc:`Subnet <Subnet>` object.

Arcus can exists because of libraries such as `Gulliver <https://github.com/sandialabs/gulliver>`_, if you're interested in byte manipulation it is worth checking out.

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
