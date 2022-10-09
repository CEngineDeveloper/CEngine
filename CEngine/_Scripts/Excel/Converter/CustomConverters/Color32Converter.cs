using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace CYM.Excel
{
    /// <summary>
    /// Default converter for <see cref="UnityEngine.Color32"/>(<seealso cref="UnityEngine.Color"/>)
    /// </summary>
    /// <remarks>Valid patterns are <c>(r,g,b,a)</c> and <c>(r,g,b)</c> where r,g,b,a are <see cref="byte"/>(0 ~ 255)</remarks>
    public class Color32Converter : CustomConverter<Color32>
    {
        #region Overrides of CustomConverter<Color32>

        /// <summary>
        /// Convert input string to <see cref="UnityEngine.Color32"/>
        /// </summary>
        /// <param name="input">Input value</param>
        /// <remarks>Valid patterns are <c>(r,g,b,a)</c> and <c>(r,g,b)</c> where r,g,b,a are <see cref="byte"/>(0 ~ 255)</remarks>
        /// <returns>Parsed value</returns>
        /// <exception cref="FormatException">Color32 value expression invalid</exception>
        public override Color32 Convert(string input)
        {
            if (Regex.IsMatch(input,
                @"^\(([01]?[0-9]?[0-9]|2[0-4][0-9]|25[0-5]),([01]?[0-9]?[0-9]|2[0-4][0-9]|25[0-5]),([01]?[0-9]?[0-9]|2[0-4][0-9]|25[0-5]),([01]?[0-9]?[0-9]|2[0-4][0-9]|25[0-5])\)$"))
                //RGBA in (r,g,b,a) format;  r/g/b/a is between 0 and 255
            {
                string[] parameters = Split(input.Trim('(', ')'), ',');
                int r = ValueConverter.Convert<int>(parameters[0]);
                int g = ValueConverter.Convert<int>(parameters[1]);
                int b = ValueConverter.Convert<int>(parameters[2]);
                int a = ValueConverter.Convert<int>(parameters[3]);

                return new Color(r, g, b, a);
            }

            if (Regex.IsMatch(input,
                @"^\(([01]?[0-9]?[0-9]|2[0-4][0-9]|25[0-5]),([01]?[0-9]?[0-9]|2[0-4][0-9]|25[0-5]),([01]?[0-9]?[0-9]|2[0-4][0-9]|25[0-5])\)$"))
                //RGB in (r,g,b) format;  r/g/b is between 0 and 255
            {
                string[] parameters = Split(input.Trim('(', ')'), ',');
                int r = ValueConverter.Convert<int>(parameters[0]);
                int g = ValueConverter.Convert<int>(parameters[1]);
                int b = ValueConverter.Convert<int>(parameters[2]);

                return new Color(r, g, b);
            }

            throw new FormatException("Color32 value expression invalid: " + input);
        }

        #endregion
    }
}