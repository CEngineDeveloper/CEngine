using System.Collections;
using System.Collections.Generic;
// <summary>
// 访问更加安全的Dictionary
// SafeDic.cs
// Copyright 2019/2/19 
// Created by CYM on 2019/2/19
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------
namespace CYM
{
    public class SafeDic<T,V> : Dictionary<T,V>
    {
        public new V this[T key]
        {
            get
            {
                if (ContainsKey(key))
                {
                    return base[key];
                }
                return default;
            }
            set
            {
                if (ContainsKey(key))
                {
                    base[key] = value;
                }
                else
                {
                    Add(key, value);
                }
            }
        }

        public IList ToList() => new List<KeyValuePair<T, V>>(this);
    }
}
