Frequently Asked Questions
==========================

IPv6 is big, huh?
-----------------

Yes

Can you elaborate?
^^^^^^^^^^^^^^^^^^

Absolutely.

There are :math:`2^{128}` possible IPv6 Addresses, compared to the :math:`2^{32}` possible IPv4 addresses.

That's roughly :math:`3.4\times10^{38}` addresses.

340 undecillion 282 decillion 366 nonillion 920 octillion 938 septillion 463 sextillion 463 quintillion 374 quadrillion 607 trillion 431 billion 768 million 211 thousand 456 to be exact.

Let's face it, arbitrary numbers much bigger than 7 are hard to conceptualize for some of us. I personally get lost after three-ish. The awe inspiring scale of IPv6 is much bigger than 3, at least double, probably even over triple that. It is so big we had to jump through some hoops to make C# do the math necessary. This is why both the Arcus and `Gulliver <https://github.com/sandialabs/gulliver>`_ libraries now exist.

As a thought exercise let's try to visualize the mighty scale of IPv6.

According to `un data <http://data.un.org/>`_ estimates there are approximately 7.55 billion people alive as I write this sentence.

If we take all :math:`2^{128}` IPv6 addresses and distribute them equally amongst everyone we'd each get about :math:`4.51\times10^{28}` addresses. That's a rather lot of IoT devices to keep track of.

Thanks to new and inventive imaginary non-existent technology we're going to assign each of our grain of sand sized network devices an address from our own personal IPv6 address pool. This will be a wireless network obviously, it is rather difficult to jam a RJ45 cable into something :math:`0.05mm^3`.

As it turns out that's approximately :math:`2.25\times10^{19}m^3` of much bigger than nano-bot devices you've got there. Hope you have some deep pockets, as that's nearly the volume of 1.8 times all of earths oceans. That's per person.

This means that with the power of all our sand-bots combined we'd have roughly the volume of twelve of earth's suns.

Conversely, all :math:`2^{32}` IPv4 addresses would slightly overflow a 50-gallon drum amassing a measly 56.7 gallons. It is not a surprise that we've practically exhausted the IPv4 address space. That said, if we mismanage IPv6 we may run out there too, and Arcus will have to do 256-bit or 1024-bit math, I'm ready.

IPv6 is :math:`7.9\times10^{28}` times larger than IPv4
