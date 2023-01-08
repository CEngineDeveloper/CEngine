using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
//**********************************************
// Discription	：Base Core Calss .All the Mono will inherit this class
// Author	：CYM
// Team		：MoBaGame
// Date		：2020-7-16
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
// Desc     ：此代码由陈宜明于2020年编写,版权归陈宜明所有
// Copyright (c) 2020 陈宜明 All rights reserved.
namespace CYM
{
    [Serializable, Unobfus]
    public static class EnumTool<T> where T : Enum
    {
        #region set
        public static T Start()
        {
            return (T)Enum.GetValues(typeof(T)).GetValue(0);
        }
        public static T End()
        {
            var data = Enum.GetValues(typeof(T));
            return (T)data.GetValue(data.Length-1);
        }
        public static int Length()
        {
            return Enum.GetValues(typeof(T)).Length;
        }
        public static int Int(T enu)
        {
#if UNITY_2022
            return _wrapper(enu);
#else
            return (int)(object)enu;
#endif
        }
        public static T Invert(int val)
        {
#if UNITY_2022
            return _wrapperInvert(val);
#else
            return (T)(object)val;
#endif
        }
        public static T Parse(string str)
        {
            var ret = Enum.Parse(typeof(T), str);
            return (T)ret;
        }

        public static void For(Action<T> callback)
        {
            for (var type = 0; type < Enum.GetValues(typeof(T)).Length; ++type)
            {
                callback(Invert(type));
            }
        }

        public static void ForIndex(Action<int> callback)
        {
            for (var type = 0; type < Enum.GetValues(typeof(T)).Length; ++type)
            {
                callback(type);
            }
        }
#endregion

#region life
#if UNITY_2022
        static readonly Func<T, int> _wrapper;
        static readonly Func<int, T> _wrapperInvert;
        static EnumTool()
        {
            var p = Expression.Parameter(typeof(T), null);
            var c = Expression.ConvertChecked(p, typeof(int));
            _wrapper = Expression.Lambda<Func<T, int>>(c, p).Compile();
            //逆向
            var p2 = Expression.Parameter(typeof(int), null);
            var c2 = Expression.ConvertChecked(p2, typeof(T));
            _wrapperInvert = Expression.Lambda<Func<int, T>>(c2, p2).Compile();
        }
#endif
#endregion

#region extend
        private static Dictionary<string, Dictionary<string, string>> _enumCache;

        /// <summary>
        /// 缓存
        /// </summary>
        private static Dictionary<string, Dictionary<string, string>> EnumCache
        {
            get { return _enumCache ?? (_enumCache = new Dictionary<string, Dictionary<string, string>>()); }
            set { _enumCache = value; }
        }

        /// <summary>
        /// 获取枚举描述信息
        /// </summary>
        /// <param name="en"></param>
        /// <returns></returns>
        public static string GetText()
        {
            string enString = string.Empty;

            Type type = typeof(T);
            enString = typeof(T).Name;
            if (!EnumCache.ContainsKey(type.FullName))
            {
                FieldInfo[] fields = type.GetFields();
                Dictionary<string, string> temp = new Dictionary<string, string>();
                foreach (FieldInfo item in fields)
                {
                    object[] attrs = item.GetCustomAttributes(typeof(TextAttribute), false);
                    if (attrs.Length == 1)
                    {
                        string v = ((TextAttribute)attrs[0]).Value;
                        temp.Add(item.Name, v);
                    }
                }
                EnumCache.Add(type.FullName, temp);
            }
            if (EnumCache[type.FullName].ContainsKey(enString))
            {
                return EnumCache[type.FullName][enString];
            }
            return enString;
        }

        /// <summary>
        /// 遍历枚举对象的所有元素
        /// </summary>
        /// <typeparam name="T">枚举对象</typeparam>
        /// <returns>Dictionary：枚举值-描述</returns>
        public static Dictionary<int, string> GetValues()
        {
            Dictionary<int, string> dictionary = new Dictionary<int, string>();
            foreach (var code in Enum.GetValues(typeof(T)))
            {
                ////获取名称
                //string strName = System.Enum.GetName(typeof(T), code);

                object[] objAttrs = code.GetType().GetField(code.ToString()).GetCustomAttributes(typeof(TextAttribute), true);
                if (objAttrs.Length > 0)
                {
                    TextAttribute descAttr = objAttrs[0] as TextAttribute;
                    if (!dictionary.ContainsKey((int)code))
                    {
                        if (descAttr != null) dictionary.Add((int)code, descAttr.Value);
                    }
                    //Console.WriteLine(string.Format("[{0}]", descAttr.Value));
                }
                //Console.WriteLine(string.Format("{0}={1}", code.ToString(), Convert.ToInt32(code)));
            }
            return dictionary;
        }
#endregion
    }
}
