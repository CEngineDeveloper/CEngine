//------------------------------------------------------------------------------
// IDDic.cs
// Copyright 2019/2/19 
// Created by CYM on 2019/2/19
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CYM
{
    #region HashManager
    [Unobfus]
    public class IDDic<K, T>: Dictionary<K, T> where T : IBase
    {
        T defaultVal = default(T);

        public virtual bool Contains(K id)
        {
            return ContainsKey(id);
        }

        public virtual new void Add(K id, T ent)
        {
            if (ContainsKey(id))
            {
                Debug.LogError("重复的id:" + id);
                return;
            }
            base.Add(id, ent);
        }

        public virtual new void Remove(K id)
        {
            base.Remove(id);
        }
        public virtual T Get(K id)
        {
            T temp = defaultVal;
            if (id == null)
                return temp;
            base.TryGetValue(id, out temp);
            return temp;
        }

        public virtual bool IsEmpty()
        {
            return base.Count == 0;
        }

        public virtual new void Clear()
        {
            base.Clear();
        }


        public IList ToList() => new List<KeyValuePair<K, T>>(this);
    }
    #endregion

}
