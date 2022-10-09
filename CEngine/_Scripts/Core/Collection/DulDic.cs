//------------------------------------------------------------------------------
// DulDic.cs
// Copyright 2019 2019/7/17 
// Created by CYM on 2019/7/17
// Owner: CYM
// TKey,TValue都可以用来索引,删除,查找
//------------------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CYM
{
    public interface IDulDic
    {
        void Set(object key, object value);
        int Add(object key, object value);
    }
    [Unobfus]
    public class DulDic<TKey, TValue> : Dictionary<TKey, TValue>, IDulDic
    {
        public Dictionary<TValue, TKey> ValueKeys { get; protected set; } = new Dictionary<TValue, TKey>();

        public TKey this[TValue value]
        {
            get
            {
                if (value == null)
                    return default;
                return ValueKeys[value];
            }
        }
        public void Set(object key,object value)
        {
            this[(TKey)key] = (TValue)value;
        }

        public new void Add(TKey key, TValue value)
        {
            if (key == null)
            {
                Debug.LogError("DulDic key is null");
                return;
            }
            if (value == null)
            {
                Debug.LogError("DulDic value is null,Key:" + key);
                return;
            }
            base.Add(key, value);
            if (!ValueKeys.ContainsKey(value))
                ValueKeys.Add(value, key);
        }
        public int Add(object key, object value)
        {
            Add((TKey)key,(TValue)value);
            return Count;
        }
        public new void Remove(TKey key)
        {
            if (key == null)
                return;
            if (ContainsKey(key))
                ValueKeys.Remove(this[key]);
            base.Remove(key);
        }
        public void Remove(TValue value)
        {
            if (value == null)
                return;
            if (ValueKeys.ContainsKey(value))
                base.Remove(ValueKeys[value]);
            ValueKeys.Remove(value);
        }
        public new void Clear()
        {
            base.Clear();
            ValueKeys.Clear();
        }

        public IList ToList()=> new List<KeyValuePair<TKey, TValue>>(this);
    }
}