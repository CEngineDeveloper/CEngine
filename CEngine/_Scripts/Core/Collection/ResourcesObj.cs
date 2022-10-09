using System.Collections.Generic;
using UnityEngine;
//**********************************************
// Class Name	: CYMComponsiteRegister
// Discription	：None
// Author	：CYM
// Team		：MoBaGame
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************

namespace CYM
{
    [Unobfus]
    public class ResourcesObj<T> where T : Object
    {
        public ResourcesObj()
        {
        }
        public ResourcesObj(string resPath)
        {
            path = resPath;
        }
        string path = "";
        public T this[string name]
        {
            get
            {
                if (!ContainsKey(name))
                {
                    T temp = Resources.Load(path + "/" + name) as T;
                    if (temp)
                    {
                        Add(name, temp);
                    }
                    else
                    {
                        Debug.LogError("Error Path:" + path + "/" + name);
                        return null;
                    }
                }
                return data[name];
            }
        }
        public virtual bool ContainsKey(string name)
        {
            return data.ContainsKey(name);
        }
        public virtual void Add(T c)
        {
            Add(c.name, c);
        }
        public virtual void Add(string name, T c)
        {
            data.Add(name, c);
        }
        public virtual void Remove(string name)
        {
            data.Remove(name);
        }
        public virtual T Find(string c)
        {
            return data[c];
        }
        public virtual void Clear()
        {
            data.Clear();
        }
        Dictionary<string, T> data = new Dictionary<string, T>();
    }
}
