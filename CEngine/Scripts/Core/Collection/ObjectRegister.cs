using System.Collections.Generic;
using UnityEngine;

//**********************************************
// Class Name	: CYMObjectRegister
// Discription	：None
// Author	：CYM
// Team		：MoBaGame
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************

namespace CYM
{
    public interface IObjectRegister
    {
        void Clear();
        void RemoveNull();
        void Remove(string name);
        bool ContainsKey(string name);
    }
    [Unobfus]
    public class ObjectRegister<T>: IObjectRegister 
        where T : Object
    {
        public ObjectRegister()
        {
        }
        public T this[string name]
        {
            get
            {
                return data[name];
            }
            set
            {
                data[name] = value;
            }
        }

        public virtual bool ContainsKey(string name)
        {
            return data.ContainsKey(name);
        }
        public virtual bool ContainsKey(Object obj)
        {
            return data.ContainsKey(obj.name);
        }
        public virtual void Add(T c)
        {
            Add(c.name, c);
        }

        public virtual void Add(string name, T c)
        {
            if (!ContainsKey(name))
            {
                data.Add(name, c);
            }
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
        public void RemoveNull()
        {
            List<string> clear = new List<string>();
            foreach (var item in data)
            {
                if (item.Value == null)
                    clear.Add(item.Key);
            }
            foreach (var item in clear)
                data.Remove(item);
            clear.Clear();
        }
        Dictionary<string, T> data = new Dictionary<string, T>();

    }

}
