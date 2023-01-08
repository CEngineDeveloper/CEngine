//------------------------------------------------------------------------------
// BatterDic.cs
// Copyright 2019 2019/7/4 
// Created by CYM on 2019/7/4
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using System.Collections.Generic;
namespace CYM
{
    [Unobfus]
    public class ObjDic<K, V> : Dictionary<K, V> where V : class, new()
    {
        public new V this[K k]
        {
            get
            {
                if (!ContainsKey(k))
                    Add(k, new V());
                return base[k];
            }
            set
            {
                base[k] = value;
            }
        }
    }
}