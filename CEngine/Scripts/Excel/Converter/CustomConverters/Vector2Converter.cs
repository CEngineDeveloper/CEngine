using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace CYM.Excel
{
    /// <summary>
    /// Default converter for <see cref="UnityEngine.Vector2"/>. 
    /// </summary>
    /// <remarks>Valid pattern is <c>(x,y)</c> where x,y are <see cref="float"/></remarks>
    public sealed class Vector2Converter : CustomConverter<Vector2>
    {
        /// <summary>
        /// Convert input string to <see cref="UnityEngine.Vector2"/>
        /// </summary>
        /// <param name="input">Input value</param>
        /// <remarks>Valid pattern is <c>(x,y)</c> where x,y are <see cref="float"/></remarks>
        /// <returns>Parsed value</returns>
        /// <exception cref="FormatException">Vector2 value expression invalid</exception>
        public override Vector2 Convert(string input)
        {
           if (!Regex.IsMatch(input, @"^\([-+]?[0-9]*\.?[0-9]+\b,[-+]?[0-9]*\.?[0-9]+\b\)$"))
           {
               throw new FormatException("Vector2 value expression invalid: " + input);
           }

            string[] parameters = Split(input.Trim('(', ')'), ',');
            float x = ValueConverter.Convert<float>(parameters[0]);
            float y = ValueConverter.Convert<float>(parameters[1]);
            return new Vector2(x, y);
        }

    }
}