//------------------------------------------------------------------------------
// ExtensionType.cs
// Copyright 2020 2020/7/15 
// Created by CYM on 2020/7/15
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;

namespace CYM
{
    public static class ExtensionType
    {
        public static bool IsDerivedFromOpenGenericType(
                this Type type, Type openGenericType
            )
        {
            Contract.Requires(type != null);
            Contract.Requires(openGenericType != null);
            Contract.Requires(openGenericType.IsGenericTypeDefinition);
            return type.GetTypeHierarchy()
                       .Where(t => t.IsGenericType)
                       .Select(t => t.GetGenericTypeDefinition())
                       .Any(t => openGenericType.Equals(t));
        }

        public static IEnumerable<Type> GetTypeHierarchy(this Type type)
        {
            Contract.Requires(type != null);
            Type currentType = type;
            while (currentType != null)
            {
                yield return currentType;
                currentType = currentType.BaseType;
            }
        }

        public static IEnumerable<string> Keys(this Type type, BindingFlags? propBindingAttr = null, BindingFlags? fieldBindingAttr = null)
        {
            List<string> result = new List<string>();
            result.AddRange(PropertyKeys(type, propBindingAttr));
            result.AddRange(FieldKeys(type, fieldBindingAttr));
            return result;
        }

        public static IEnumerable<string> PropertyKeys(this Type type, BindingFlags? bindingAttr = null)
        {
            PropertyInfo[] props = bindingAttr.HasValue ? type.GetProperties(bindingAttr.Value) : type.GetProperties();
            return props.Select(x => x.Name);
        }

        public static IEnumerable<string> FieldKeys(this Type type, BindingFlags? bindingAttr = null)
        {
            FieldInfo[] fields = bindingAttr.HasValue ? type.GetFields(bindingAttr.Value) : type.GetFields();
            return fields.Select(x => x.Name);
        }

        public static IDictionary<string, object> KeyValueList(this Type type, object obj, BindingFlags? propBindingAttr = null, BindingFlags? fieldBindingAttr = null)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            PropertyInfo[] props = propBindingAttr.HasValue ? type.GetProperties(propBindingAttr.Value) : type.GetProperties();
            Array.ForEach(props, x => result.Add(x.Name, x.GetValue(obj)));
            FieldInfo[] fields = fieldBindingAttr.HasValue ? type.GetFields(fieldBindingAttr.Value) : type.GetFields();
            Array.ForEach(fields, x => result.Add(x.Name, x.GetValue(obj)));
            return result;
        }

        public static IDictionary<string, object> PropertyKeyValueList(this Type type, object obj, BindingFlags? bindingAttr = null)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            PropertyInfo[] props = bindingAttr.HasValue ? type.GetProperties(bindingAttr.Value) : type.GetProperties();
            Array.ForEach(props, x => result.Add(x.Name, x.GetValue(obj)));
            return result;
        }

        public static IDictionary<string, object> FieldKeyValueList(this Type type, object obj, BindingFlags? bindingAttr = null)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            FieldInfo[] fields = bindingAttr.HasValue ? type.GetFields(bindingAttr.Value) : type.GetFields();
            Array.ForEach(fields, x => result.Add(x.Name, x.GetValue(obj)));
            return result;
        }

        public static bool HasKey(this Type type, string key, BindingFlags? propBindingAttr = null, BindingFlags? fieldBindingAttr = null)
        {
            return type.Keys(propBindingAttr, fieldBindingAttr).Contains(key);
        }

        public static object GetValue(this Type type, string key, object obj, BindingFlags? propBindingAttr = null, BindingFlags? fieldBindingAttr = null)
        {
            IDictionary<string, object> propertyKeyValueList = PropertyKeyValueList(type, obj, propBindingAttr);
            if (propertyKeyValueList.ContainsKey(key))
            {
                return propertyKeyValueList[key];
            }
            IDictionary<string, object> fieldKeyValueList = FieldKeyValueList(type, obj, fieldBindingAttr);
            if (fieldKeyValueList.ContainsKey(key))
            {
                return fieldKeyValueList[key];
            }
            return null;
        }
    }
}