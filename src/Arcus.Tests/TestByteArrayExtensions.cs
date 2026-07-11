namespace Arcus.Tests
{
    /// <summary>
    ///     Extension methods for byte arrays used by test code.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The <see cref="Reverse"/> method avoids overload resolution selecting
    ///         <c>MemoryExtensions.Reverse(Span&lt;T&gt;)</c> (C# 13+) instead of
    ///         <c>Enumerable.Reverse&lt;T&gt;()</c>, preserving the expected return type.
    ///     </para>
    /// </remarks>
    internal static class TestByteArrayExtensions
    {
        /// <summary>
        ///     Returns the bytes in reverse order as an <see cref="IEnumerable{T}"/>.
        /// </summary>
        /// <param name="bytes">The byte array to reverse.</param>
        /// <returns>The reversed byte sequence.</returns>
        public static IEnumerable<byte> Reverse(this byte[] bytes)
        {
            return Enumerable.Reverse(bytes);
        }
    }
}
