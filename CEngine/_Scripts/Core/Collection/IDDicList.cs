//------------------------------------------------------------------------------
// DicList.cs
// Copyright 2019 2019/1/18 
// Created by CYM on 2019/1/18
// Owner: CYM
// 用来管理用TDID 和 ID 作为key的数据对象
// TDID是配置表ID(重复的时候会覆盖)
// ID是运行时动态生成ID
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
namespace CYM
{
    public interface IIDDicList
    {
        int Add(object value);
    }
    [Serializable]
    [Unobfus]
    public class IDDicList<T> : List<T>, IIDDicList, IDeserializationCallback 
        where T : IBase
    {
        [NonSerialized]
        protected Dictionary<string, T> Hash;
        [NonSerialized]
        protected Dictionary<long, T> IDHash;

        public IDDicList(List<T> rawData) : base()
        {
            Hash = new Dictionary<string, T>();
            IDHash = new Dictionary<long, T>();
            Clear();
            foreach (var item in rawData)
            {
                Add(item);
            }
        }

        public IDDicList() : base()
        {
            Hash = new Dictionary<string, T>();
            IDHash = new Dictionary<long, T>();
        }

        public T First()
        {
            if (Count <= 0)
                return default;
            return this[0];
        }

        public new void Add(T ent)
        {
            if (ent == null)
            {
                throw new NotImplementedException("DicList.Add:ent 为 null!!!");
            }
            if (ent.TDID == null)
            {
                throw new NotImplementedException("DicList.Add:ent.TDID 为 null!!!");
            }
            //添加以TDID为基准的数据
            if (Hash.ContainsKey(ent.TDID))
                Hash[ent.TDID] = ent;
            else
                Hash.Add(ent.TDID, ent);

            //添加ID基准的数据
            if (!IDHash.ContainsKey(ent.ID))
                IDHash.Add(ent.ID, ent);
            else
                IDHash[ent.ID] = ent;

            base.Add(ent);
        }
        public new void Remove(T ent)
        {
            if (ent == null) return;
            Hash.Remove(ent.TDID);
            IDHash.Remove(ent.ID);
            base.Remove(ent);
        }
        public new void RemoveAll(Predicate<T> match)
        {
            foreach (var item in this)
            {
                if (match(item))
                {
                    Hash.Remove(item.TDID);
                    IDHash.Remove(item.ID);
                }
            }
            base.RemoveAll(match);
        }
        public void Remove(long id)
        {
            Remove(Get(id));
        }
        public bool ContainsID(long id)
        {
            return IDHash.ContainsKey(id);
        }
        public bool ContainsValue(T ent)
        {
            return IDHash.ContainsValue(ent);
        }
        public bool ContainsTDID(string id)
        {
            return Hash.ContainsKey(id);
        }
        public T Get(string id)
        {
            if (id == null) return default;
            if (Hash.ContainsKey(id))
                return Hash[id];
            return default;
        }
        public T Get(long id)
        {
            if (IDHash.ContainsKey(id))
                return IDHash[id];
            return default;
        }
        public new void Clear()
        {
            Hash.Clear();
            IDHash.Clear();
            base.Clear();
        }
        public int Add(object value)
        {
            Add((T)value);
            return Count;
        }
        void IDeserializationCallback.OnDeserialization(object sender)
        {
            Hash = new Dictionary<string, T>();
            IDHash = new Dictionary<long, T>();
            foreach (var ent in this)
            {
                //添加以TDID为基准的数据
                if (Hash.ContainsKey(ent.TDID))
                    Hash[ent.TDID] = ent;
                else
                    Hash.Add(ent.TDID, ent);

                //添加ID基准的数据
                if (!IDHash.ContainsKey(ent.ID))
                    IDHash.Add(ent.ID, ent);
            }
        }

        public static explicit operator Dictionary<string, T>(IDDicList<T> data)
        {
            return data.Hash;
        }

        public static explicit operator Dictionary<long, T>(IDDicList<T> data)
        {
            return data.IDHash;
        }
    }
}