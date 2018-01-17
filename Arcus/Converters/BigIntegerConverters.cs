using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using JetBrains.Annotations;

namespace Arcus.Converters
{
    /// <summary>
    ///     Static utility class containing conversion methods for converting <see cref="BigInteger" /> objects into something
    ///     else
    /// </summary>
    public static class BigIntegerConverters
    {
        #region string conversions

        /// <summary>
        ///     Convert <see cref="BigInteger" /> to Base85 for IPv6 address compact representation
        ///     from RFC 1924 ( http://tools.ietf.org/html/rfc1924 )
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static IEnumerable<char> ToBase85Chars(this BigInteger input)
        {
            do
            {
                BigInteger charIndex;
                input = BigInteger.DivRem(input, 85, out charIndex);
                yield return "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz!#$%&()*+-;<=>?@^_`{|}~"[(int) charIndex];
            } while (input > 0);
        }

        /// <summary>
        ///     Converts a <see cref="BigInteger" /> to a binary string.
        ///     Based on http://stackoverflow.com/a/15447131
        /// </summary>
        /// <param name="bigint">A <see cref="BigInteger" />.</param>
        /// <returns>
        ///     A <see cref="System.String" /> containing a binary
        ///     representation of the supplied <see cref="BigInteger" />.
        /// </returns>
        [NotNull]
        public static string ToBinaryString(this BigInteger bigint)
        {
            var bytes = bigint.ToByteArray();
            var idx = bytes.Length - 1;

            // Create a StringBuilder having appropriate capacity.
            var base2 = new StringBuilder(bytes.Length * 8);

            // Convert first byte to binary.
            var binary = Convert.ToString(bytes[idx], 2);

            // Ensure leading zero exists if value is positive.
            if (binary[0] != '0'
                && bigint.Sign == 1)
            {
                base2.Append('0');
            }

            // Append binary string to StringBuilder.
            base2.Append(binary);

            // Convert remaining bytes adding leading zeros.
            for (idx--; idx >= 0; idx--)
            {
                base2.Append(Convert.ToString(bytes[idx], 2)
                                    .PadLeft(8, '0'));
            }

            return base2.ToString();
        }

        /// <summary>
        ///     Convert to hexadecimal string
        /// </summary>
        /// <param name="bigInteger"></param>
        /// <param name="bigEndian"></param>
        /// <returns></returns>
        [NotNull]
        public static string ToHexString(this BigInteger bigInteger,
                                         bool bigEndian = false)
        {
            var byteArray = bigEndian
                                ? bigInteger.ToByteArray()
                                : bigInteger.ToByteArray()
                                            .Reverse()
                                            .ToArray();

            return string.Format("0x{0}", BitConverter.ToString(byteArray)
                                                      .Replace("-", string.Empty));
        }

        /// <summary>
        ///     Converts a <see cref="BigInteger" /> to a octal string.
        ///     Based on http://stackoverflow.com/a/15447131/1090923
        /// </summary>
        /// <param name="bigint">A <see cref="BigInteger" />.</param>
        /// <returns>
        ///     A <see cref="System.String" /> containing an octal representation of the supplied <see cref="BigInteger" />.
        /// </returns>
        [NotNull]
        public static string ToOctalString(this BigInteger bigint)
        {
            var bytes = bigint.ToByteArray();
            var idx = bytes.Length - 1;

            // Create a StringBuilder having appropriate capacity.
            var base8 = new StringBuilder((bytes.Length / 3 + 1) * 8);

            // Calculate how many bytes are extra when byte array is split
            // into three-byte (24-bit) chunks.
            var extra = bytes.Length % 3;

            // If no bytes are extra, use three bytes for first chunk.
            if (extra == 0)
            {
                extra = 3;
            }

            // Convert first chunk (24-bits) to integer value.
            var int24 = 0;
            for (; extra != 0; extra--)
            {
                int24 <<= 8;
                int24 += bytes[idx--];
            }

            // Convert 24-bit integer to octal without adding leading zeros.
            var octal = Convert.ToString(int24, 8);

            // Ensure leading zero exists if value is positive.
            if (octal[0] != '0'
                && bigint.Sign == 1)
            {
                base8.Append('0');
            }

            // Append first converted chunk to StringBuilder.
            base8.Append(octal);

            // Convert remaining 24-bit chunks, adding leading zeros.
            for (; idx >= 0; idx -= 3)
            {
                int24 = (bytes[idx] << 16) + (bytes[idx - 1] << 8) + bytes[idx - 2];
                base8.Append(Convert.ToString(int24, 8)
                                    .PadLeft(8, '0'));
            }

            return base8.ToString();
        }

        #endregion
    }
}
