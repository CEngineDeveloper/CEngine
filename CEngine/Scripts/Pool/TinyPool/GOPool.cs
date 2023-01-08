//------------------------------------------------------------------------------
// GOPool.cs
// Copyright 2018 2018/3/29 
// Created by CYM on 2018/3/29
// Owner: CYM
// 临时GameObject对象池
//------------------------------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;

namespace CYM.Pool
{
    public class GOPool
    {
        #region prop
        public List<GameObject> UnUsed { get; private set; } = new List<GameObject>();
        public List<GameObject> Used { get; private set; } = new List<GameObject>();
        GameObject Prefab;
        Transform Parent;
        bool IsActiveByPos = false;
        #endregion

        #region life
        public GOPool(GameObject prefab, Transform parent, bool isActiveByPos = false)
        {
            Prefab = prefab;
            Parent = parent;
            IsActiveByPos = isActiveByPos;
        }
        #endregion

        #region set
        public GameObject Spawn()
        {
            GameObject ret = null;
            if (UnUsed.Count > 0)
            {
                ret = UnUsed[0];
                UnUsed.RemoveAt(0);
            }
            else
            {
                ret = GameObject.Instantiate(Prefab, Parent);
            }
            if (!IsActiveByPos) ret.SetActive(true);
            ret.transform.position = Prefab.transform.position;
            ret.transform.rotation = Prefab.transform.rotation;
            Used.Add(ret);
            return ret;
        }
        public void Despawn(GameObject go, bool isRemove = true)
        {
            UnUsed.Add(go);
            if (isRemove) Used.Remove(go);
            if (!IsActiveByPos) go.SetActive(false);
            else go.transform.position = SysConst.VEC_FarawayPos;
        }
        public void DespawnAll()
        {
            foreach (var item in Used)
            {
                //UnUsed.Add(item);
                //item.SetActive(false);
                Despawn(item, false);
            }
            Used.Clear();
        }
        public void Destroy()
        {
            foreach (var item in Used)
                GameObject.Destroy(item);
            foreach (var item in UnUsed)
                GameObject.Destroy(item);
            Used.Clear();
            UnUsed.Clear();
        }
        #endregion
    }
}