using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Arcus.Utilities
{
    /// <summary>
    ///     Static utility class containing miscellaneous operations for
    ///     <see cref="IEnumerable{T}" /> and <see langword="byte" /> arrays
    /// </summary>
    public static class ByteArrayUtilities
    {
        /// <summary>
        ///     Transform an <see cref="Enumerable"/> of <see langword="byte" /> input to a given length, trimming LSB / padding with LSB 0x00's as necessary
        /// </summary>
        /// <param name="input">the bytes to transform</param>
        /// <param name="desiredLength">the length of the bytes</param>
        /// <returns>the transfored bytes</returns>
        [NotNull]
        public static byte[] AffixByteLength([CanBeNull] this IEnumerable<byte> input,
                                             int desiredLength)
        {
            var inputArray = (input ?? Enumerable.Empty<byte>()).ToArray();

            if (inputArray.Length > desiredLength)
            {
                return inputArray.Take(desiredLength)
                                 .ToArray();
            }

            if (inputArray.Length < desiredLength)
            {
                return inputArray.Concat(Enumerable.Repeat((byte) 0x00, desiredLength - inputArray.Length))
                                 .ToArray();
            }

            return inputArray;
        }

        /// <summary>
        ///     Create a filled <see langword="byte" /> array
        /// </summary>
        /// <param name="size">the number of bytes within the byte array</param>
        /// <param name="initializer">the byte value to fill the array with</param>
        /// <returns>the filled byte array</returns>
        [NotNull]
        public static byte[] CreateFilledByteArray(int size,
                                                   byte initializer = 0xff) => Enumerable.Repeat(initializer, size)
                                                                                         .ToArray();

        /// <summary>
        ///     Fill a <see langword="byte" /> array with the given value
        /// </summary>
        /// <param name="bytes">bytes</param>
        /// <param name="fillValue">the bytes to fill the array with</param>
        /// <returns>the array</returns>
        /// <exception cref="ArgumentNullException"><paramref name="bytes" /> is <see langword="null" />.</exception>
        [NotNull]
        public static byte[] FillByteArray([NotNull] this byte[] bytes,
                                           byte fillValue)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException(nameof(bytes));
            }

            for (var i = 0; i < bytes.Length; i++)
            {
                bytes[i] = fillValue;
            }
            return bytes;
        }
    }
}
