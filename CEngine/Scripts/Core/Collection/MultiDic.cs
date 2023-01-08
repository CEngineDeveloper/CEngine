//------------------------------------------------------------------------------
// MutiDic.cs
// Copyright 2019 2019/1/21 
// Created by CYM on 2019/1/21
// Owner: CYM
// 双重重Key数据结构
// 通过2个key来查询,删除,添加数据,用于国家之间的关系
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace CYM
{
    [Serializable]
    public struct MultiKey<T>
    {
        public T Item1;
        public T Item2;
        public MultiKey(T key1, T key2)
        {
            Item1 = key1;
            Item2 = key2;
        }
        public override string ToString()
        {
            return Item1.ToString() +"="+ Item2.ToString();
        }
    }
    public interface IMultiDic
    {
        int Add(object key1, object key2, object val);
    }
    [Serializable]
    public class MultiDic<TKey, TValue> : Dictionary<MultiKey<TKey>, TValue>, IMultiDic, IDeserializationCallback, ISerializable
    {
        public Dictionary<TKey, TValue> this[TKey key] => GetDatas(key);
        public Dictionary<TKey, Dictionary<TKey, TValue>> ArrayData { get; private set; } = new Dictionary<TKey, Dictionary<TKey, TValue>>();

        // 如果没有,则会自动添加一个
        public void Change(TKey key1, TKey key2, TValue value)
        {
            var oneKey = new MultiKey<TKey>(key1, key2);
            var twoKey = new MultiKey<TKey>(key2, key1);
            if (ContainsKey(oneKey)) this[oneKey] = value;
            else this[twoKey] = value;
            if (!ArrayData.ContainsKey(key1)) ArrayData[key1] = new Dictionary<TKey, TValue>();
            if (!ArrayData.ContainsKey(key2)) ArrayData[key2] = new Dictionary<TKey, TValue>();
            ArrayData[key1][key2] = value;
            ArrayData[key2][key1] = value;
        }
        public void Remove(TKey key1, TKey key2)
        {
            var oneKey = new MultiKey<TKey>(key1, key2);
            var twoKey = new MultiKey<TKey>(key2, key1);
            base.Remove(oneKey);
            base.Remove(twoKey);
            ArrayData.Remove(key1);
            ArrayData.Remove(key2);
        }
        public new void Remove(MultiKey<TKey> key)
        {
            var oneKey = new MultiKey<TKey>(key.Item1, key.Item2);
            var twoKey = new MultiKey<TKey>(key.Item2, key.Item1);
            base.Remove(oneKey);
            base.Remove(twoKey);
            ArrayData.Remove(key.Item1);
            ArrayData.Remove(key.Item2);
        }
        public bool ContainsKey(TKey key1, TKey key2)
        {
            var oneKey = new MultiKey<TKey>(key1, key2);
            var twoKey = new MultiKey<TKey>(key2, key1);
            if (ContainsKey(oneKey) || ContainsKey(twoKey))
            {
                return true;
            }
            return false;
        }
        public TValue Get(TKey key1, TKey key2)
        {
            var oneKey = new MultiKey<TKey>(key1, key2);
            var twoKey = new MultiKey<TKey>(key2, key1);
            if (ContainsKey(oneKey))
                return this[oneKey];
            else if (ContainsKey(twoKey))
                return this[twoKey];
            return default;
        }
        public TValue GetOne(TKey key1, TKey key2)
        {
            var oneKey = new MultiKey<TKey>(key1, key2);
            if (ContainsKey(oneKey))
                return this[oneKey];
            return default;
        }
        public Dictionary<TKey, TValue> GetDatas(TKey key1)
        {
            if (key1 == null)
                return new Dictionary<TKey, TValue>();
            if (ArrayData.ContainsKey(key1))
                return ArrayData[key1];
            return new Dictionary<TKey, TValue>();
        }

        public int Add(object key1, object key2, object val)
        {
            Change((TKey)key1, (TKey)key2, (TValue)val);
            return Count;
        }
        public new void Clear()
        {
            base.Clear();
            ArrayData.Clear();
        }

        #region Data
        List<TKey> Key1 = new List<TKey>();
        List<TKey> Key2 = new List<TKey>();
        List<TValue> Value = new List<TValue>();

        public MultiDic(SerializationInfo info, StreamingContext context)
        {
            Key1 = (List<TKey>)info.GetValue("key1", typeof(List<TKey>));
            Key2 = (List<TKey>)info.GetValue("key2", typeof(List<TKey>));
            Value = (List<TValue>)info.GetValue("Value", typeof(List<TValue>));
            for (int i = 0; i < Key1.Count; ++i)
            {
                Add(new MultiKey<TKey>(Key1[i], Key2[i]), Value[i]);
            }
        }
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            Key1.Clear();
            Key2.Clear();
            Value.Clear();
            foreach (var item in this)
            {
                Key1.Add(item.Key.Item1);
                Key2.Add(item.Key.Item2);
                Value.Add(item.Value);
            }
            info.AddValue("key1", Key1);
            info.AddValue("key2", Key2);
            info.AddValue("Value", Value);
        }
        public MultiDic() : base() { }
        void IDeserializationCallback.OnDeserialization(object sender) { }
        #endregion
    }
}