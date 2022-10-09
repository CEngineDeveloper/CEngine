using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CYM.Excel
{
    /// <summary>
    /// Base class of all custom converters.
    /// </summary>
    /// <typeparam name="T">The data type to convert a string input to</typeparam>
    public abstract class CustomConverter<T> : IConverter
    {
        Type IConverter.Type { get { return typeof(T); } }

        /// <summary>
        /// Convert input string to <see cref="T"/>
        /// </summary>
        /// <param name="input">String input</param>
        /// <returns>Output value</returns>
        public abstract T Convert(string input);

        /// <summary>
        /// Converts the string value to <see cref="T"/>. A return value indicates whether the conversion succeeded.
        /// </summary>
        /// <param name="input">String input</param>
        /// <param name="value">Output value</param>
        /// <returns>true if s was converted successfully; otherwise, false.</returns>
		public bool TryConvert(string input, out T value)
        {
            value = default(T);
            try
            {
                value = Convert(input);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Split array with specified separators
        /// </summary>
        /// <param name="input">String value to split</param>
        /// <param name="separators">Delimiters</param>
        /// <returns>A string array</returns>
        protected string[] Split(string input, char separators)//params char[] separators
        {
            return input.Split(separators);
            //string pattern = separators.ToString();
            //for (int i = 0; i < separators.Length; i++)
            //{
            //    pattern = i > 0 ? "|" : string.Empty + pattern + @"(?<!\\)" + separators[i];
            //}
            //string[] vals = Regex.Split(input, pattern, RegexOptions.Multiline);
            //for (int i = 0; i < vals.Length; i++)
            //{
            //    //vals[i] = Regex.Unescape(vals[i]);
            //    foreach (char separator in separators)
            //    {
            //        vals[i] = vals[i].Replace("\\" + separator, separator.ToString());
            //    }
            //}
            //return vals;
        }

        /// <summary>
        /// Split grouped array like [a,b][c,d]...
        /// </summary>
        /// <param name="input">String value to split</param>
        /// <param name="start">Left anchor</param>
        /// <param name="end">Right anchor</param>
        /// <returns>Split value</returns>
        /// <exception cref="ArgumentException">Group expression invalid</exception>
        protected IEnumerable<string> SplitGroup(string input, char start, char end)
        {
            string pattern = @"\G\[(\\\[|\\\]|[^\[\]])+\]".Replace('[', start).Replace(']', end);
            foreach (Match match in Regex.Matches(input, pattern, RegexOptions.Multiline))
            {
                string val = match.Value;
                if (!match.Success)
                    throw new ArgumentException("Group expression invalid", input);

                //val = Regex.Unescape(val);
                val = val.TrimStart(start).TrimEnd(end);
                val = val.Replace("\\" + start, start.ToString()).Replace("\\" + end, end.ToString());
                if (Regex.IsMatch(val, @"(?<=\([^]]*),(?=[^]]*\))"))
                    val = Regex.Replace(val, @"(?<=\([^]]*),(?=[^]]*\))", @"\,");
                yield return val;
            }
        }

        object IConverter.Convert(string input)
        {
            return this.Convert(input);
        }
    }
}