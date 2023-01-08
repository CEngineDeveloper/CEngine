using System;
using System.Text.RegularExpressions;

namespace CYM.Excel
{
    /// <summary>
    /// Default converter for <see cref="object"/>
    /// </summary>
    /// <remarks>
    /// This converter will try to parse input value as number types(ascend by precision), 
    /// then try <see cref="bool"/>; if failed, <see cref="string"/> will be returned
    /// </remarks>
    public class ObjectConverter : CustomConverter<object>
    {
        /// <summary>
        /// Convert input string to <see cref="object"/>
        /// </summary>
        /// <param name="input">Input value</param>
        /// <returns>Parsed value</returns>
        /// <remarks>
        /// This converter will try to parse input value as number types(ascending by precision), 
        /// then try <see cref="bool"/>; if failed, <see cref="string"/> will be returned
        /// </remarks>
        public override object Convert(string input)
        {
            //match number
            if (Regex.IsMatch(input, @"^[+-]?[\d]+[\d,]*\.{0,1}[\d]*$", RegexOptions.IgnoreCase))
            {
                input = Regex.Replace(input, @",|\+", string.Empty);
                if (Regex.IsMatch(input, @"\."))
                {
                    float f;
                    if (Single.TryParse(input, out f))
                        return f;
                    double d;
                    if (Double.TryParse(input, out d))
                        return d;
                }

                ushort us;
                if (UInt16.TryParse(input, out us))
                    return us;
                short s;
                if (Int16.TryParse(input, out s))
                    return s;
                uint ui;
                if (UInt32.TryParse(input, out ui))
                    return ui;
                int i;
                if (Int32.TryParse(input, out i))
                    return i;
                ulong ul;
                if (UInt64.TryParse(input, out ul))
                    return ul;
                long l;
                if (Int64.TryParse(input, out l))
                    return l;
            }

            //Boolean
            if (Regex.IsMatch(input, @"^(true|false)$", RegexOptions.IgnoreCase))
            {
                bool b;
                if (Boolean.TryParse(input, out b))
                    return b;
            }

            //String
            return input;
        }
    }
}