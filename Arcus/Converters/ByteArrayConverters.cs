using System;
using System.Linq;
using System.Numerics;
using Arcus.Utilities;
using JetBrains.Annotations;

namespace Arcus.Converters
{
    /// <summary>
    ///     Static utility class containing conversion methods for converting <see langword="byte" /> arrays into something
    ///     else
    /// </summary>
    public static class ByteArrayConverters
    {
        #region string conversions

        /// <summary>
        ///     Convert a <see langword="byte" /> array to a hex string
        /// </summary>
        /// <param name="input">the bytes to convert</param>
        /// <returns>the hexadecimal string</returns>
        /// <exception cref="ArgumentNullException"><paramref name="input" /> is <see langword="null" />.</exception>
        [NotNull]
        public static string ToHexString([NotNull] this byte[] input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            return BitConverter.ToString(input)
                               .Replace("-", string.Empty);
        }

        #endregion

        #region BigInteger conversions

        /// <summary>
        ///     Bytes as <see cref="BigInteger" />
        /// </summary>
        /// <param name="input">the bytes to convert</param>
        /// <param name="littleEndian">true if should be little endian (default is true)</param>
        /// <returns>the created BigInteger</returns>
        /// <exception cref="ArgumentNullException"><paramref name="input" /> is <see langword="null" />.</exception>
        public static BigInteger ToBigInteger([NotNull] this byte[] input,
                                              bool littleEndian = true)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            return new BigInteger(input.ReverseIf(littleEndian)
                                       .ToArray());
        }

        /// <summary>
        ///     Bytes as Unsigned <see cref="BigInteger" />
        /// </summary>
        /// <param name="input">the bytes to convert</param>
        /// <param name="littleEndian">true if should be little endian (default is true)</param>
        /// <returns>the created BigInteger</returns>
        /// <exception cref="ArgumentNullException"><paramref name="input" /> is <see langword="null" />.</exception>
        public static BigInteger ToUnsignedBigInteger([NotNull] this byte[] input,
                                                      bool littleEndian = true)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            // take into account the littleendianness of the constructor call reversing the byte order
            var bytes = input.ReverseIf(littleEndian)
                             .ToList();

            bytes.Add(0x00); // append a 00 byte to the end of an array (thus forcing unsigned value)

            return new BigInteger(bytes.ToArray());
        }

        #endregion
    }
}
