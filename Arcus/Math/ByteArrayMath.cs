using System;
using System.Linq;
using System.Numerics;
using Arcus.Converters;
using Arcus.Utilities;
using JetBrains.Annotations;

namespace Arcus.Math
{
    /// <summary>
    ///     Static utility class containing mathematical methods for <see langword="byte" /> arrays
    /// </summary>
    public static class ByteArrayMath
    {
        #region byte[] comparison

        /// <summary>
        ///     Return maximum of the two provided <see langword="byte" /> arrays according to the desired rules
        /// </summary>
        /// <param name="alpha">first operand</param>
        /// <param name="beta">second operand</param>
        /// <param name="littleEndian">true if operation should be little endian, otherwise big endian (defaults to false)</param>
        /// <param name="unsigned">true if operation should be unsigned (defaults to signed)</param>
        /// <returns>byte array result of operation</returns>
        /// <exception cref="ArgumentNullException"><paramref name="alpha" /> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="beta" /> is <see langword="null" />.</exception>
        [NotNull]
        public static byte[] Max([NotNull] byte[] alpha,
                                 [NotNull] byte[] beta,
                                 bool littleEndian = false,
                                 bool unsigned = false)
        {
            if (alpha == null)
            {
                throw new ArgumentNullException(nameof(alpha));
            }

            if (beta == null)
            {
                throw new ArgumentNullException(nameof(beta));
            }

            BigInteger alphaBigInteger,
                       betaBigInteger;

            if (unsigned)
            {
                alphaBigInteger = alpha.ToUnsignedBigInteger(littleEndian);
                betaBigInteger = beta.ToUnsignedBigInteger(littleEndian);

            }
            else
            {
                alphaBigInteger = alpha.ToBigInteger(littleEndian);
                betaBigInteger = beta.ToBigInteger(littleEndian);
            }

            return alphaBigInteger < betaBigInteger
                       ? beta
                       : alpha;
        }

        /// <summary>
        ///     Return minimum of the two provided <see langword="byte" /> arrays according to the desired rules
        /// </summary>
        /// <param name="alpha">first operand</param>
        /// <param name="beta">second operand</param>
        /// <param name="littleEndian">true if operation should be little endian, otherwise big endian (defaults to false)</param>
        /// <param name="unsigned">true if operation should be unsigned (defaults to signed)</param>
        /// <returns>byte array result of operation</returns>
        /// <exception cref="ArgumentNullException"><paramref name="alpha" /> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="beta" /> is <see langword="null" />.</exception>
        [NotNull]
        public static byte[] Min([NotNull] byte[] alpha,
                                 [NotNull] byte[] beta,
                                 bool littleEndian = false,
                                 bool unsigned = false)
        {
            if (alpha == null)
            {
                throw new ArgumentNullException(nameof(alpha));
            }

            if (beta == null)
            {
                throw new ArgumentNullException(nameof(beta));
            }

            BigInteger alphaBigInteger,
                       betaBigInteger;
            if (unsigned)
            {
                alphaBigInteger = alpha.ToUnsignedBigInteger(littleEndian);
                betaBigInteger = beta.ToUnsignedBigInteger(littleEndian);
            }
            else
            {
                alphaBigInteger = alpha.ToBigInteger(littleEndian);
                betaBigInteger = beta.ToBigInteger(littleEndian);
            }

            return alphaBigInteger > betaBigInteger
                       ? beta
                       : alpha;
        }

        #endregion

        #region Bit based math

        /// <summary>
        ///     Bitwise AND two arrays of bytes of the same size
        /// </summary>
        /// <param name="alpha">first operand</param>
        /// <param name="beta">second operand</param>
        /// <returns>bitwise AND of the two arrays of bytes</returns>
        /// <exception cref="InvalidOperationException"><paramref name="alpha" /> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="beta" /> is <see langword="null" />.</exception>
        /// <exception cref="InvalidOperationException">alpha and beta input must be the same length</exception>
        [NotNull]
        public static byte[] BitwiseAnd([NotNull] this byte[] alpha,
                                        [NotNull] byte[] beta)
        {
            if (alpha == null)
            {
                throw new ArgumentNullException(nameof(alpha));
            }

            if (beta == null)
            {
                throw new ArgumentNullException(nameof(beta));
            }

            if (alpha.Length != beta.Length)
            {
                throw new InvalidOperationException("alpha and beta input must be the same length");
            }

            var result = alpha.ToArray();

            for (var i = 0; i < alpha.Length; i++)
            {
                result[i] &= beta[i];
            }

            return result;
        }

        /// <summary>
        ///     Perform a bitwise inversion on bits within array of bytes
        /// </summary>
        /// <param name="input">the <see langword="byte" /> array to invert</param>
        /// <returns>the inverted byte array</returns>
        /// <exception cref="ArgumentNullException"><paramref name="input" /> is <see langword="null" />.</exception>
        [NotNull]
        public static byte[] BitwiseNot([NotNull] this byte[] input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            var result = ByteArrayUtilities.CreateFilledByteArray(input.Length, 0x00);

            for (var i = 0; i < input.Length; i++)
            {
                result[i] = (byte) ~input[i];
            }

            return result;
        }

        /// <summary>
        ///     Bitwise OR two arrays of bytes of the same size
        /// </summary>
        /// <param name="alpha">first operand</param>
        /// <param name="beta">second operand</param>
        /// <returns>bitwise OR of the two arrays of bytes</returns>
        /// <exception cref="ArgumentNullException"><paramref name="alpha" /> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="beta" /> is <see langword="null" />.</exception>
        /// <exception cref="InvalidOperationException">alpha and beta input must be the same length</exception>
        [NotNull]
        public static byte[] BitwiseOr([NotNull] this byte[] alpha,
                                       [NotNull] byte[] beta)
        {
            if (alpha == null)
            {
                throw new ArgumentNullException(nameof(alpha));
            }

            if (beta == null)
            {
                throw new ArgumentNullException(nameof(beta));
            }

            if (alpha.Length != beta.Length)
            {
                throw new InvalidOperationException("alpha and beta input must be the same length");
            }

            var result = alpha.ToArray();

            for (var i = 0; i < alpha.Length; i++)
            {
                result[i] |= beta[i];
            }

            return result;
        }

        /// <summary>
        ///     Bitwise XOR two arrays of bytes of the same size
        /// </summary>
        /// <param name="alpha">first operand</param>
        /// <param name="beta">second operand</param>
        /// <returns>bitwise OR of the two arrays of bytes</returns>
        /// <exception cref="ArgumentNullException"><paramref name="alpha" /> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="beta" /> is <see langword="null" />.</exception>
        /// <exception cref="InvalidOperationException">alpha and beta input must be the same length</exception>
        [NotNull]
        public static byte[] BitwiseXor([NotNull] this byte[] alpha,
                                        [NotNull] byte[] beta)
        {
            if (alpha == null)
            {
                throw new ArgumentNullException(nameof(alpha));
            }

            if (beta == null)
            {
                throw new ArgumentNullException(nameof(beta));
            }

            if (alpha.Length != beta.Length)
            {
                throw new InvalidOperationException("alpha and beta input must be the same length");
            }

            var result = alpha.ToArray();

            for (var i = 0; i < alpha.Length; i++)
            {
                result[i] ^= beta[i];
            }

            return result;
        }

        /// <summary>
        ///     Perform XNOR bitwise operation on two byte arrays of the same size
        /// </summary>
        /// <param name="alpha">first byte array</param>
        /// <param name="beta">second byte array</param>
        /// <returns>bitwise XNor of the two byte arrays</returns>
        public static byte[] BitwiseXNor([NotNull] this byte[] alpha,
            [NotNull] byte[] beta)
        {
            return BitwiseXor(alpha.BitwiseNot(), beta.BitwiseNot()).BitwiseNot();
        }

        /// <summary>
        ///     Shift a bits in byte array shiftCount times left
        /// </summary>
        /// <param name="input">byte array of bits to shift</param>
        /// <param name="shiftCount">the number of bit positions to shift</param>
        /// <returns>a byte array of shifted bits</returns>
        /// <exception cref="ArgumentNullException"><paramref name="input" /> is <see langword="null" />.</exception>
        [NotNull]
        public static byte[] ShiftBitsLeft([NotNull] this byte[] input,
                                           int shiftCount = 1)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            byte[] outBytes;
            return ShiftBitsLeft(input, shiftCount, out outBytes);
        }

        /// <summary>
        ///     Shift a bits in byte array <paramref name="shiftCount" /> times left
        /// </summary>
        /// <param name="input">the byte array to shift</param>
        /// <param name="shiftCount">the number of shifts to perform</param>
        /// <param name="carry">the carried bits as bytes</param>
        /// <returns>the shifted byte array</returns>
        /// <exception cref="ArgumentNullException"><paramref name="input" /> is <see langword="null" />.</exception>
        [NotNull]
        public static byte[] ShiftBitsLeft([NotNull] this byte[] input,
                                           int shiftCount,
                                           out byte[] carry)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            var bytes = ByteArrayUtilities.CreateFilledByteArray(input.Length, 0x00);
            carry = input.GetBits(0, shiftCount); // fill carry bits

            var bitWriteIndex = 0;
            for (var bitReadIndex = shiftCount; bitReadIndex < bytes.Length * 8; bitReadIndex++, bitWriteIndex++)
            {
                var bitMask = (byte) (0x80 >> bitReadIndex % 8); // bit mask built from current bit position in byte

                if ((input[bitReadIndex / 8] & bitMask) > 0) // bit is set
                {
                    bytes[bitWriteIndex / 8] |= (byte) (0x80 >> bitWriteIndex % 8);
                }
            }

            return bytes;
        }

        /// <summary>
        ///     Shift a bits in byte array <paramref name="shiftCount" /> times right
        /// </summary>
        /// <param name="input">byte array of bits to shift</param>
        /// <param name="shiftCount">the number of bit positions to shift</param>
        /// <returns>a byte array of shifted bits</returns>
        /// <exception cref="ArgumentNullException"><paramref name="input" /> is <see langword="null" />.</exception>
        [NotNull]
        public static byte[] ShiftBitsRight([NotNull] this byte[] input,
                                            int shiftCount = 1)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            byte[] outBytes;
            return ShiftBitsRight(input, shiftCount, out outBytes);
        }

        /// <summary>
        ///     Shift bits in byte array <paramref name="shiftCount" /> times right
        /// </summary>
        /// <param name="input">the byte array to shift</param>
        /// <param name="shiftCount">the number of shifts to perform</param>
        /// <param name="carry">the carried bits as bytes</param>
        /// <returns>the shifted byte array</returns>
        /// <exception cref="ArgumentNullException"><paramref name="input" /> is <see langword="null" />.</exception>
        [NotNull]
        public static byte[] ShiftBitsRight([NotNull] this byte[] input,
                                            int shiftCount,
                                            out byte[] carry)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            var bytes = ByteArrayUtilities.CreateFilledByteArray(input.Length, 0x00);
            carry = input.GetBits(input.Length * 8 - shiftCount, shiftCount); // fill carry bits

            var bitWriteIndex = shiftCount;
            for (var bitReadIndex = 0; bitReadIndex < bytes.Length * 8 - shiftCount; bitReadIndex++, bitWriteIndex++)
            {
                var bitMask = (byte) (0x80 >> bitReadIndex % 8); // bit mask built from current bit position in byte

                if ((input[bitReadIndex / 8] & bitMask) > 0) // bit is set
                {
                    bytes[bitWriteIndex / 8] |= (byte) (0x80 >> bitWriteIndex % 8);
                }
            }

            return bytes;
        }

        /// <summary>
        ///     Get bits from a byte array
        /// </summary>
        /// <param name="sourceBytes">the source to read bytes from</param>
        /// <param name="start">the (bit) index as to where to start the read</param>
        /// <param name="length">the number of bits to read</param>
        /// <returns>a byte array of read bits. If bit total cannot reach bounds of byte byte will be 0 prefixed</returns>
        /// <exception cref="InvalidOperationException">can't capture more bits than present</exception>
        /// <exception cref="ArgumentNullException"><paramref name="sourceBytes" /> is <see langword="null" />.</exception>
        [NotNull]
        public static byte[] GetBits([NotNull] this byte[] sourceBytes,
                                     int start,
                                     int? length = null)
        {
            if (sourceBytes == null)
            {
                throw new ArgumentNullException(nameof(sourceBytes));
            }

            var sourceBitLength = sourceBytes.Length * 8; // the total number of bits in the source
            var readLength = length ?? sourceBitLength - start; // the length of bits that will be read

            if (readLength > sourceBitLength)
            {
                throw new InvalidOperationException("can't capture more bits than present");
            }

            var writeBytesLength = (int) System.Math.Ceiling((double) readLength / 8); // the length of the writable bytes
            var writeBytes = ByteArrayUtilities.CreateFilledByteArray(writeBytesLength, 0x00); // destination array

            var destinationBitIndex = 0; // the bit index of the destination

            for (var i = 0; i < readLength; i++, destinationBitIndex++)
            {
                var sourceBitIndex = start + readLength - 1 - i; // bit index of the source
                var bitMask = (byte) (0x80 >> sourceBitIndex % 8); // bit mask built from current bit position in byte

                if ((sourceBytes[sourceBitIndex / 8] & bitMask) > 0) // bit is set
                {
                    writeBytes[destinationBitIndex / 8] |= (byte) (0x01 << destinationBitIndex % 8);
                }
            }

            return writeBytes.Reverse()
                             .ToArray();
        }

        #endregion
    }
}
