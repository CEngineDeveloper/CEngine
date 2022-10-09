#if UNITY_IOS || ENABLE_IL2CPP
#define AOT
#endif

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.Serialization;

namespace CYM.Excel
{
    /// <summary>
    /// Converter class for string data parsing
    /// </summary>
    public static class ValueConverter
    {
        private static readonly List<IConverter> converters = new List<IConverter>();

        static ValueConverter()
        {
            Reset();
        }

        /// <summary>
        /// Restore default converters
        /// </summary>
        public static void Reset()
        {
            converters.Clear();
            Register<ColorConverter>();
            Register<Color32Converter>();
            Register<RectConverter>();
            Register<Vector4Converter>();
            Register<Vector3Converter>();
            Register<Vector2Converter>();
            Register<ObjectConverter>();
#if AOT
            Register(System.Convert.ToBoolean);
            Register(System.Convert.ToByte);
            Register(System.Convert.ToChar);
            Register(System.Convert.ToDateTime);
            Register(System.Convert.ToDecimal);
            Register(System.Convert.ToDouble);
            Register(System.Convert.ToInt16);
            Register(System.Convert.ToInt32);
            Register(System.Convert.ToInt64);
            Register(System.Convert.ToSByte);
            Register(System.Convert.ToSingle);
            Register(System.Convert.ToString);
            Register(System.Convert.ToUInt16);
            Register(System.Convert.ToUInt32);
            Register(System.Convert.ToUInt64);
            Register(new ArrayConverter<string>());
            Register(new ArrayConverter<int>());
            Register(new ArrayConverter<float>());
            Register(new ListConverter<string>());
            Register(new ListConverter<int>());
            Register(new ListConverter<float>());
            Register(new DictionaryConverter<string, string>());
            Register(new DictionaryConverter<string, int>());
            Register(new DictionaryConverter<int, int>());
            Register(new DictionaryConverter<int, string>());
#else
            Register<ArrayConverter<string>>();
            Register<ArrayConverter<int>>();
            Register<ArrayConverter<float>>();
            Register<ListConverter<string>>();
            Register<ListConverter<int>>();
            Register<ListConverter<float>>();
            Register<DictionaryConverter<string, string>>();
            Register<DictionaryConverter<string, int>>();
            Register<DictionaryConverter<int, int>>();
            Register<DictionaryConverter<int, string>>();
#endif
        }

        /// <summary>
        /// Clear all registered converters
        /// </summary>
        public static void Empty()
        {
            converters.Clear();
        }

        /// <summary>
        /// Convert input string to target type
        /// </summary>
        /// <param name="type">Target type</param>
        /// <param name="value">input value</param>
        /// <returns>Parsed value</returns>
        /// <remarks>
        /// Registered global converters(<seealso cref="CustomConverter{T}"/>) will be applied if available
        /// </remarks>
        /// <exception cref="ArgumentNullException">Type is null</exception>
        /// <exception cref="NotSupportedException">Converter not found for target type</exception>
        /// <exception cref="FormatException"/>
        public static object Convert(Type type, string value)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            var converter = converters.Find(c => c.Type == type);
            if (converter == null)
            {
#if !AOT
                var @default = TypeDescriptor.GetConverter(type);
                if (@default == null)
                {
                    throw new NotSupportedException("Converter not found for target type: " + type.Name);
                }
                converter = new Converter(i => @default.ConvertFromString(i), type);
                Register(converter);
#else
                throw new NotSupportedException("Converter not found for target type: " + type.Name);
#endif
            }

            try
            {
                return converter.Convert(value);
            }
            catch (FormatException ex)
            {
                throw new FormatException(ex.Message, ex);
            }
            catch (Exception ex)
            {
                throw new FormatException(ex.Message, ex);
            }
        }

        /// <summary>
        /// Convert input string to target type
        /// </summary>
        /// <typeparam name="T">Target type</typeparam>
        /// <param name="value">input value</param>
        /// <returns>Parsed value</returns>
        /// <remarks>
        /// Registered global converters(<seealso cref="CustomConverter{T}"/>) will be applied if available
        /// </remarks>
        public static T Convert<T>(string value)
        {
            return (T)Convert(typeof(T), value);
        }

        /// <summary>
        /// Register a converter
        /// </summary>
        /// <remarks>
        /// This overrides all converters of the same output type
        /// </remarks>
        /// <param name="converter">Converter instance</param>
        /// <returns>true if registration succeeded</returns>
        public static bool Register(IConverter converter)
        {
            if (converter == null)
            {
                return false;
            }
            converters.RemoveAll(c => c.Type == converter.Type);
            converters.Add(converter);
            return true;
        }

        /// <summary>
        /// Register a converter
        /// </summary>
        /// <remarks>
        /// This overrides all converters of the same output type
        /// </remarks>
        /// <typeparam name="T">Converter type</typeparam>
        /// <returns>true if registration succeeded</returns>
        public static bool Register<T>() where T : IConverter, new()
        {
            return Register(new T());
        }

        /// <summary>
        /// Register global delegate converter
        /// </summary>
        /// <remarks>
        /// This overrides all converters of the same output type
        /// </remarks>
        /// <typeparam name="T">Output type</typeparam>
        /// <param name="converter">Converter delegate</param>
        /// <returns>true if registration succeeded</returns>
        public static bool Register<T>(Converter<string, T> converter)
        {
            return Register(new Converter<T>(converter));
        }

        /// <summary>
        /// Register global delegate converter
        /// </summary>
        /// <remarks>
        /// This overrides all converters of the same output type
        /// </remarks>
        /// <param name="converter">Converter delegate</param>
        /// <param name="type">Output type</param>
        /// <returns>true if registration succeeded</returns>
        public static bool Register(Converter<string, object> converter, Type type)
        {
            return Register(new Converter(converter, type));
        }

        /// <summary>
        /// Unregister global converter of target output type
        /// </summary>
        /// <param name="type">Output type</param>
        /// <returns>true if unregistration succeeded</returns>
        public static bool Unregister(Type type)
        {
            return converters.RemoveAll(c => c.Type == type) > 0;
        }

        /// <summary>
        /// Unregister global converter of target output type
        /// </summary>
        /// <typeparam name="T">Output type</typeparam>
        /// <returns>true if unregistration succeeded</returns>
        public static bool Unregister<T>()
        {
            return Unregister(typeof(T));
        }

        private class Converter<T> : IConverter
        {
            private Converter<string, T> _converter;

            public Converter(Converter<string, T> converter)
            {
                if (converter == null)
                {
                    throw new ArgumentNullException();
                }
                _converter = converter;
            }

            Type IConverter.Type { get { return typeof(T); } }

            object IConverter.Convert(string input)
            {
                return _converter(input);
            }
        }

        private class Converter : IConverter
        {
            private Converter<string, object> _converter;
            private Type _type;

            public Converter(Converter<string, object> converter, Type type)
            {
                if (converter == null || type == null)
                {
                    throw new ArgumentNullException();
                }
                _converter = converter;
                _type = type;
            }

            Type IConverter.Type { get { return _type; } }

            object IConverter.Convert(string input)
            {
                return _converter(input);
            }
        }
    }
}