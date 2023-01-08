using System;
using System.Collections.Generic;

namespace CYM.Pool
{
    public interface IObjPool
    {
        void RecycleAll();
        object GetObj();
    }
    public class ObjPool<T>: IObjPool
    {
        public int UsedCount => Used.Count;
        public int CountInPool => Stack.Count;
        public Stack<T> Stack { get; private set; } = new Stack<T>();
        public List<T> Used { get; private set; } = new List<T>();
        private readonly Func<T> m_ctor;
        private readonly Action<T> m_OnRecycle;

        public ObjPool(Func <T> ctor = null, Action<T> onRecycle = null)
        {
            m_OnRecycle = onRecycle;
            m_ctor = ctor;
        }
        public object GetObj() => Get();
        
        public T Get()
        {
            T item;
            if (Stack.Count == 0)
            {
                if(null != m_ctor)
                {
                    item = m_ctor();
                }
                else
                {
                    item = Activator.CreateInstance<T>();
                }
            }
            else
            {
                item = Stack.Pop();
            }
            Used.Add(item);
            return item;
        }

        public void Recycle(T item)
        {
            if(m_OnRecycle!= null)
            {
                m_OnRecycle.Invoke(item);
            }
            Stack.Push(item);
            Used.Remove(item);
        }

        public void RecycleAll()
        {
            List<T> temp = new List<T>(Used);
            foreach (var item in temp)
            {
                Recycle(item);
            }
            Used.Clear();
        }

        public override string ToString()
        {
            return string.Format("SimpleObjPool: item=[{0}], inUse=[{1}], restInPool=[{2}/{3}] ", typeof(T), UsedCount, Stack.Count,0);
        }

    }
}
