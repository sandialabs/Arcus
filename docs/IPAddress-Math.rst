IP Address Math
===============

Too frequently the existing implementation of the C# ``IPAddress`` object is too limited for anything beyond some of the most trivial interactions. Mathematical operations in fact are wholly absent, forcing developers to directly manipulate bytes [#Gulliver_001]_, often requiring a great deal of manual implementation of non-existent byte math. Don't worry though, Arcus is here to fill in some of those gaps.

.. note:: Unless otherwise specified regarding the math of the ``IPAddress`` object treats it as an unsigned integer based on its bytes interpenetrated as 32-bit for IPv4 and 128-bit for IPv6 all in big-endian byte order.

Increment
^^^^^^^^^

Incrementing an ``IPAddress`` allows for the the addition or subtraction of a provided optional ``long delta`` value.

There exist two implementations of Increment methods. ``Increment`` and the safe ``TryIncrement``.


.. code-block:: c#

   public static IPAddress Increment(this IPAddress input, long delta = 1)

.. code-block:: c#

   public static bool TryIncrement(IPAddress input, out IPAddress address, long delta = 1)

Comparisons
^^^^^^^^^^^

Compare to Another IPAddress
----------------------------

The ``IPAddress`` does not implement the standard comparison operators, and thus far we can't write extension methods for operators on a class [#OperatorExtensionMethods]_. Arcus did the next best thing, deciding not to extend the ``IPAddress``, opting to provide a handful of simple extension methods to bend the will of the ``IPAddress`` to suit our needs.

.. note:: Barring the use of the methods below, the  :ref:`DefaultIPAddressComparer` may also be of interest to you.

It should be pretty obvious based on name alone as to what each of the following five methods will accomplish.

.. code-block:: c#

   public static bool IsEqualTo(this IPAddress left, IPAddress right)

.. code-block:: c#

   public static bool IsGreaterThan(this IPAddress left, IPAddress right)

.. code-block:: c#

   public static bool IsGreaterThanOrEqualTo(this IPAddress left, IPAddress right)

.. code-block:: c#

   public static bool IsLessThan(this IPAddress left, IPAddress right)

.. code-block:: c#

   public static bool IsLessThanOrEqualTo(this IPAddress left, IPAddress right)

Get IsBetween
-------------

Slightly different than the other comparison extension method above is the `IsBetween` method. As is hopefully is obvious it will test if an ``IPAddress`` occurs numerically between the given ``high`` and ``low`` addresses. Likewise the `inclusive` bit may be set to include equality to either ``low`` or ``high`` to be considered an inclusive between.

.. code-block:: c#

   public static bool IsBetween(this IPAddress input, IPAddress low, IPAddress high, bool inclusive = true)

Get Min / Max
-------------

The ``Min`` and ``Max`` methods will return the ``IPAddress left`` or ``IPAddress right`` that is the smallest or largest of the two respectively.

.. code-block:: c#

   public static IPAddress Min(IPAddress left, IPAddress right)

.. code-block:: c#

   public static IPAddress Max(IPAddress left, IPAddress right)

Determine Scale
---------------

``IsAtMin`` and ``IsAtMax`` tests the ``IPAddress address`` to determine if it is at its minimum or maximum value respectively.

.. note:: For IPv4 the minimum value is ``0.0.0.0`` (:math:`0`), and maximum is ``255.255.255.255`` (:math:`2^{32}-1`)

.. note:: For IPv6 the minimum value is ``::`` (:math:`0`), and maximum is ``ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff`` (:math:`2^{128}-1`)

.. code-block:: c#

   public static bool IsAtMin(this IPAddress address)

.. code-block:: c#

   public static bool IsAtMax(this IPAddress address)

.. rubric:: Footnotes

.. [#Gulliver_001] If you actually want to manipulate bytes take a gander at `Gulliver <https://github.com/sandialabs/gulliver>`_, an C# library developed by the same folks that wrote Arcus. They're kinda great.

.. [#OperatorExtensionMethods] A GitHub issue for `Extension function members <https://github.com/dotnet/csharplang/issues/192>`_ requesting a champion for some proposed changes regarding the future of extension methods.
