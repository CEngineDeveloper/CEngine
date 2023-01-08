using System.Collections.Generic;
using System.Linq;

namespace CYM.Excel
{
    /// <summary>
    /// Default converter for <see cref="List{T}"/>.
    /// </summary>
    /// <typeparam name="T">Element type</typeparam>
    /// <remarks>Valid delimiter is <c>#</c>
    /// if elements contain <c>#</c>, use <c>\#</c> instead</remarks>
    public sealed class ListConverter<T> : CustomConverter<List<T>>
    {
        /// <summary>
        /// Convert input string to <see cref="List{T}"/>
        /// </summary>
        /// <param name="input">Input value</param>
        /// <remarks>Valid delimiter is <c>#</c>
        /// if elements contain <c>#</c>, use <c>\#</c> instead</remarks>
        /// <returns>Parsed value</returns>
        public override List<T> Convert(string input)
        {
            if (string.IsNullOrEmpty(input))
                return null;

            string[] parameters = Split(input, '|');
            return parameters.Select(p => ValueConverter.Convert<T>(p)).ToList();
        }

    }
}