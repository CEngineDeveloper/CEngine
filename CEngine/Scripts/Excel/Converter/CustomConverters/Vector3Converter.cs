using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace CYM.Excel
{
    /// <summary>
    /// Default converter for <see cref="UnityEngine.Vector3"/>. 
    /// </summary>
    /// <remarks>Valid pattern is <c>(x,y,z)</c> where x,y,z are <see cref="float"/></remarks>
    public sealed class Vector3Converter : CustomConverter<Vector3>
    {
        /// <summary>
        /// Convert input string to <see cref="UnityEngine.Vector3"/>
        /// </summary>
        /// <param name="input">Input value</param>
        /// <remarks>Valid pattern is <c>(x,y,z)</c> where x,y,z are <see cref="float"/></remarks>
        /// <returns>Parsed value</returns>
        /// <exception cref="FormatException">Vector3 value expression invalid</exception>
        public override Vector3 Convert(string input)
        {
            if (!Regex.IsMatch(input, @"^\([-+]?[0-9]*\.?[0-9]+\b,[-+]?[0-9]*\.?[0-9]+\b,[-+]?[0-9]*\.?[0-9]+\b\)$"))
            {
                throw new FormatException("Vector3 value expression invalid: " + input);
            }

            string[] parameters = Split(input.Trim('(', ')'), ',');
            float x = ValueConverter.Convert<float>(parameters[0]);
            float y = ValueConverter.Convert<float>(parameters[1]);
            float z = ValueConverter.Convert<float>(parameters[2]);
            return new Vector3(x, y, z);
        }
    }
}