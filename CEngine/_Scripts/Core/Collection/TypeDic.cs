/// <summary>
/// 访问更加安全的Dictionary
/// </summary>
namespace CYM
{
    public class TypeDic<T, V> : SafeDic<T, HashList<V>>
        where V:class
    {
        public HashList<V> Get(T key)
        {
            if (!ContainsKey(key))
            {
                Add(key, new HashList<V>());
            }
            return this[key];
        }
        public void Add(T type, V val)
        {
            if (!ContainsKey(type))
            {
                Add(type, new HashList<V>());
            }
            this[type].Add(val);
        }
        /// <summary>
        /// 唯一添加,先从其他类型中删除,再添加在指定类型
        /// </summary>
        public void AddOnly(T type,V val)
        {
            Remove(val);
            Add(type,val);
        }

        public void Remove(T type, V val)
        {
            if (!ContainsKey(type))
            {
                Add(type, new HashList<V>());
            }
            this[type].Remove(val);
        }
        public void Clear(T type)
        {
            if (!ContainsKey(type))
            {
                Add(type, new HashList<V>());
            }
            this[type].Clear();
        }
        public int GetCount(T type)
        {
            return this[type].Count;
        }

        public void Remove(V val)
        {
            foreach (var item in this.Keys)
            {
                Remove(item,val);
            }
        }
    }
}
