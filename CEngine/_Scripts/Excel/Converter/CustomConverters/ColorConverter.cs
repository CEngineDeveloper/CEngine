using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace CYM.Excel
{
    /// <summary>
    /// Default converter for <see cref="UnityEngine.Color"/>(<seealso cref="UnityEngine.Color32"/>)
    /// </summary>
    /// <remarks>Valid patterns are <c>(r,g,b,a)</c> and <c>(r,g,b)</c> where r,g,b,a are <see cref="float"/> between 0 and 1;
    /// HEX color string(starts with or without <c>#</c>) is also an alternative
    /// </remarks>
    public sealed class ColorConverter : CustomConverter<Color>
    {
        /// <summary>
        /// Convert input string to <see cref="UnityEngine.Color"/>
        /// </summary>
        /// <param name="input">Input value</param>
        /// <remarks>Valid patterns are <c>(r,g,b,a)</c> and <c>(r,g,b)</c> where r,g,b,a are <see cref="float"/> between 0 and 1;
        /// HEX color string(starts with or without <c>#</c>) is also an alternative
        /// </remarks>
        /// <returns>Parsed value</returns>
        /// <exception cref="FormatException">Color32 value expression invalid</exception>
		public override Color Convert(string input)
        {
            if (Regex.IsMatch(input, "^[#]?([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3}|[A-Fa-f0-9]{6}[Ff]{2})$"))
            //HEX
            {
                return HexToColor(input.TrimStart('#'));
            }

            if (Regex.IsMatch(input,
                @"^\((0(\.\d+)?|1(\.0+)?),(0(\.\d+)?|1(\.0+)?),(0(\.\d+)?|1(\.0+)?),(0(\.\d+)?|1(\.0+)?)\)$"))
            //RGBA in (r,g,b,a) format; r/g/b/a is between 0 and 1
            {
                string[] parameters = Split(input.Trim('(', ')'), ',');
                float r = ValueConverter.Convert<float>(parameters[0]);
                float g = ValueConverter.Convert<float>(parameters[1]);
                float b = ValueConverter.Convert<float>(parameters[2]);
                float a = ValueConverter.Convert<float>(parameters[3]);

                return new Color(r, g, b, a);
            }

            if (Regex.IsMatch(input,
                @"^\((0(\.\d+)?|1(\.0+)?),(0(\.\d+)?|1(\.0+)?),(0(\.\d+)?|1(\.0+)?)\)$"))
            //RGB in (r,g,b) format; r/g/b is between 0 and 1
            {
                string[] parameters = Split(input.Trim('(', ')'), ',');
                float r = ValueConverter.Convert<float>(parameters[0]);
                float g = ValueConverter.Convert<float>(parameters[1]);
                float b = ValueConverter.Convert<float>(parameters[2]);

                return new Color(r, g, b);
            }

            throw new FormatException("Color value expression invalid: " + input);
        }

        /// <summary>
        /// Convert <see cref="UnityEngine.Color"/> to hex string
        /// </summary>
        /// <param name="color">Color to convert</param>
        /// <returns>hex string</returns>
        public static string ColorToHex(Color color)
        {
            string hex = color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
            return hex;
        }

        /// <summary>
        /// Convert hex string to <see cref="UnityEngine.Color"/>
        /// </summary>
        /// <param name="hex">Hex string</param>
        /// <returns>color</returns>
        public static Color HexToColor(string hex)
        {
            byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
            return new Color32(r, g, b, 255);
        }
    }
}