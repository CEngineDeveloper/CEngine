//------------------------------------------------------------------------------
// OrderDic.cs
// Copyright 2020 2020/2/19 
// Created by CYM on 2020/2/19
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;

namespace CYM
{
    [Unobfus]
    public class OrderDic<TKey, TValue>
    {
        public List<TKey> Keys { get; private set; } = new List<TKey>();
        public Dictionary<TKey, TValue> Dics { get; private set; } = new Dictionary<TKey, TValue>();

        public TValue this[TKey key]
        {
            set
            {
                if (!ContainsKey(key))
                    return;
                Dics[key] = value;
            }

            get
            {
                if (!ContainsKey(key))
                    return default;
                return Dics[key];
            }
        }

        public void Add(TKey key, TValue val)
        {
            if (ContainsKey(key))
                return;
            Dics.Add(key, val);
            Keys.Add(key);
        }
        public void Remove(TKey key)
        {
            if (!ContainsKey(key))
                return;
            Keys.Remove(key);
            Dics.Remove(key);
        }

        public KeyValuePair<TKey, TValue> First()
        {
            if (Keys.Count <= 0)
                return new KeyValuePair<TKey, TValue>();
            return new KeyValuePair<TKey, TValue>(Keys[0], Dics[Keys[0]]);
        }

        public int Count
        {
            get
            {
                return Dics.Count;
            }
        }

        public void Clear()
        {
            Keys.Clear();
            Dics.Clear();
        }
        public bool ContainsKey(TKey key)
        {
            return Dics.ContainsKey(key);
        }

        public static explicit operator List<TKey>(OrderDic<TKey, TValue> data)
        {
            return data.Keys;
        }
        public static explicit operator Dictionary<TKey, TValue>(OrderDic<TKey, TValue> data)
        {
            return data.Dics;
        }

        public IList ToList() => new List<KeyValuePair<TKey, TValue>>(Dics);
    }
}