using System;
using System.Collections.Generic;

namespace CYM.Excel
{
    /// <summary>
    /// Default converter for <see cref="Dictionary{TKey,TValue}"/>
    /// </summary>
    /// <typeparam name="TKey">Key type</typeparam>
    /// <typeparam name="TValue">Value type</typeparam>
    /// <remarks>Valid pattern is <c>[key,value][key,value]...</c>;
    /// if elements contain <c>,</c> and/or <c>[</c> and/or <c>]</c>, use <c>\,</c>, <c>\[</c>, <c>\]</c> instead</remarks>
    public sealed class DictionaryConverter<TKey, TValue> : CustomConverter<Dictionary<TKey, TValue>>
    {
        /// <summary>
        /// Convert input string to <see cref="Dictionary{TKey,TValue}"/>
        /// </summary>
        /// <param name="input">Input value</param>
        /// <remarks>Valid pattern is <c>[key,value][key,value]...</c>;
        /// if elements contain <c>,</c> and/or <c>[</c> and/or <c>]</c>, use <c>\,</c>, <c>\[</c>, <c>\]</c> instead</remarks>
        /// <returns>Parsed value</returns>
        /// <exception cref="FormatException">Dictionary key is duplicate</exception>
        public override Dictionary<TKey, TValue> Convert(string input)
        {
            if (string.IsNullOrEmpty(input))
                return null;

            Dictionary<TKey, TValue> dict = new Dictionary<TKey, TValue>();

            foreach (string group in SplitGroup(input, '[', ']'))
            {
                string[] pair = Split(group, ',');
                TKey key = ValueConverter.Convert<TKey>(pair[0]);
                TValue value = ValueConverter.Convert<TValue>(pair[1]);
                if (dict.ContainsKey(key))
                    throw new FormatException("Dictionary key is duplicate: " + key);
                dict.Add(key, value);
            }

            return dict;
        }
    }
}