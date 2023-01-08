using System.Collections.Generic;
using System.Linq;

namespace CYM.Excel
{
    /// <summary>
    /// Default converter for <see cref="T:T[]"/>(<seealso cref="System.Array"/>).
    /// </summary>
    /// <typeparam name="T">Element type</typeparam>
    /// <remarks>Valid delimiter is <c>#</c>;
    /// if elements contain <c>#</c>, use <c>\#</c> instead
    /// </remarks>
    public sealed class ArrayConverter<T> : CustomConverter<T[]>
    {
        /// <summary>
        /// Convert input string to <see cref="T:T[]"/>
        /// </summary>
        /// <param name="input">Input value</param>
        /// <remarks>Valid delimiter is <c>#</c>;
        /// if elements contain <c>#</c>, use <c>\#</c> instead
        /// </remarks>
        /// <returns>Parsed value</returns>
        public override T[] Convert(string input)
        {
            if (string.IsNullOrEmpty(input))
                return null;

            string[] parameters = Split(input, '|');
            IEnumerable<T> collection = parameters.Select(p => ValueConverter.Convert<T>(p));
            return collection.ToArray();
        }
    }
}