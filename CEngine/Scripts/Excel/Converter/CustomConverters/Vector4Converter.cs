using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace CYM.Excel
{
	/// <summary>
	/// Default converter for <see cref="UnityEngine.Vector4"/>. 
	/// </summary>
    /// <remarks>Valid pattern is <c>(x,y,z,w)</c> where x,y,w,h are <see cref="float"/></remarks>
	public sealed class Vector4Converter : CustomConverter<Vector4>
	{
        /// <summary>
        /// Convert input string to <see cref="UnityEngine.Vector4"/>
        /// </summary>
        /// <param name="input">Input value</param>
        /// <remarks>Valid pattern is <c>(x,y,z,w)</c> where x,y,w,h are <see cref="float"/></remarks>
        /// <returns>Parsed value</returns>
        /// <exception cref="FormatException">Vector4 value expression invalid</exception>
		public override Vector4 Convert(string input)
		{
            if (!Regex.IsMatch(input,
                @"^\([-+]?[0-9]*\.?[0-9]+\b,[-+]?[0-9]*\.?[0-9]+\b,[-+]?[0-9]*\.?[0-9]+\b\),[-+]?[0-9]*\.?[0-9]+\b$"))
            {
                throw new FormatException("Vector4 value expression invalid: " + input);
            }

			string[] parameters = Split(input.Trim('(', ')'), ',');
            float x = ValueConverter.Convert<float>(parameters[0]);
            float y = ValueConverter.Convert<float>(parameters[1]);
            float z = ValueConverter.Convert<float>(parameters[2]);
            float w = ValueConverter.Convert<float>(parameters[3]);
			return new Vector4(x, y, z, w);
		}

	}
}