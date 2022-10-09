using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace CYM.Excel
{
    /// <summary>
    /// Default converter for <see cref="UnityEngine.Rect"/>
    /// </summary>
    /// <remarks>Valid pattern is <c>(x,y,w,h)</c> where x,y,w,h are <see cref="float"/></remarks>
    public sealed class RectConverter : CustomConverter<Rect>
    {
        /// <summary>
        /// Convert input string to <see cref="UnityEngine.Rect"/>
        /// </summary>
        /// <param name="input">Input value</param>
        /// <remarks>Valid pattern is <c>(x,y,w,h)</c> where x,y,w,h are <see cref="float"/></remarks>
        /// <returns>Parsed value</returns>
        /// <exception cref="FormatException">Rect value expression invalid</exception>
        public override Rect Convert(string input)
        {
            if (!Regex.IsMatch(input,
                @"^\([-+]?[0-9]*\.?[0-9]+\b,[-+]?[0-9]*\.?[0-9]+\b,[-+]?[0-9]*\.?[0-9]+\b,[-+]?[0-9]*\.?[0-9]+\b\)$"))
            {
                throw new FormatException("Rect value expression invalid: " + input);
            }

            string[] parameters = Split(input.Trim('(', ')'), ',');
            float x = ValueConverter.Convert<float>(parameters[0]);
            float y = ValueConverter.Convert<float>(parameters[1]);
            float w = ValueConverter.Convert<float>(parameters[2]);
            float h = ValueConverter.Convert<float>(parameters[3]);

            return new Rect(x, y, w, h);
        }
    }
}