//------------------------------------------------------------------------------
// BaseObjectPool.cs
// Copyright 2018 2018/3/29 
// Created by CYM on 2018/3/29
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using UnityEngine;

namespace CYM.Pool
{
    public interface IObjectPool
    {
        void ReleaseAll();
        T GetT<T>() where T : class;
    }
    public class BaseObjectPool<T> : IObjectPool
    {
        List<T> UsedObjs = new List<T>();
        Func<T> _create;

        public int CountTotal
        {
            get;
            private set;
        }

        public int CountOutPool
        {
            get
            {
                return CountTotal - CountInPool;
            }
        }

        public int CountInPool
        {
            get
            {
                return Stack.Count;
            }
        }

        public string PoolInfo
        {
            get
            {
                return string.Format("BaseObjectPool<{0}>, countInPool = {1}, countTotal = {2}", typeof(T).Name, CountInPool, CountTotal);
            }
        }

        public Stack<T> Stack { get; } = new Stack<T>();

        public BaseObjectPool(Func<T> create = null, int startCount = 0)
        {
            _create = create;
            for (int i = 0; i < startCount; i++)
            {
                Stack.Push(CreateInstance());
            }
        }

        T CreateInstance()
        {
            T t;
            if (_create != null)
            {
                t = _create();
            }
            else
            {
                t = CreateInstanceDefualt();
            }
            CountTotal++;
            return t;
        }

        public virtual T Get()
        {
            T t;
            if (CountInPool == 0)
            {
                t = CreateInstance();
            }
            else
            {
                t = GetInPool();
            }
            UsedObjs.Add(t);
            return t;
        }
        public virtual TType GetT<TType>() where TType : class
        {
            return Get() as TType;
        }

        // 获取已经存在与pool中的对象
        public T GetInPool()
        {
            return this.Stack.Pop();
        }

        protected virtual T CreateInstanceDefualt()
        {
            return Activator.CreateInstance<T>();
        }

        public virtual void Release(T element, bool isRemove = true)
        {
            if (Stack.Count > 0 && ReferenceEquals(Stack.Peek(), element))
            {
                Debug.LogError("Internal error. Trying to destroy object that is already released to pool.");
            }
            if (isRemove)
                UsedObjs.Remove(element);
            Stack.Push(element);
        }
        public void ReleaseAll()
        {
            foreach (var item in UsedObjs)
                Release(item, false);
            UsedObjs.Clear();
        }
    }
}