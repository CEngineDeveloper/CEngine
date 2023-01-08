//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2016 Tasharen Entertainment
//----------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

/// <summary>
/// This improved version of the System.Collections.Generic.List that doesn't release the buffer on Clear(),
/// resulting in better performance and less garbage collection.
/// PRO: BetterList performs faster than List when you Add and Remove items (although slower if you remove from the beginning).
/// CON: BetterList performs worse when sorting the list. If your operations involve sorting, use the standard List instead.
/// 
/// 其它：
/// 使用说明：详细测试参考TestBetterList中的说明
/// TODO：对迭代器和委托执行托管
/// 
/// 注意：
///     1）测试发现在GC方面都并不比系统提供的List好，详细测试对比TestBetterList与TestList
///     2）如果频繁地需要Add、Remove操作，性能上BetterList比List好3倍以上，与Queue相当
///     3）如果频繁地需要Insert、RemoveAt，性能上BetterList比List差很多
/// 
/// by wsh @ 2017-06-16
/// </summary>
namespace CYM
{
    [Unobfus]
    public class BetterList<T>:IList
    {
        /// <summary>
        /// Direct access to the buffer. Note that you should not use its 'Length' parameter, but instead use BetterList.size.
        /// </summary>

        public T[] buffer;

        /// <summary>
        /// Direct access to the buffer's size. Note that it's only public for speed and efficiency. You shouldn't modify it.
        /// </summary>

        public int Size { get; private set; } = 0;

        bool IList.IsReadOnly => false;

        bool IList.IsFixedSize => false;

        int ICollection.Count => Size;

        object ICollection.SyncRoot => false;

        bool ICollection.IsSynchronized => false;

        object IList.this[int index] { get => buffer[index]; set => buffer[index] = (T)value; }

        public BetterList()
        {
        }

        public BetterList(int cacheSize)
        {
            buffer = new T[cacheSize];
        }

        /// <summary>
        /// For 'foreach' functionality.
        /// </summary>

        [DebuggerHidden]
        [DebuggerStepThrough]
        public IEnumerator<T> GetEnumerator()
        {
            // 注意：尽量使用for循环变量，这个迭代器每次获取都要产生40B垃圾
            if (buffer != null)
            {
                for (int i = 0; i < Size; ++i)
                {
                    yield return buffer[i];
                }
            }
        }

        /// <summary>
        /// Convenience function. I recommend using .buffer instead.
        /// </summary>

        [DebuggerHidden]
        public T this[int i]
        {
            get { return buffer[i]; }
            set { buffer[i] = value; }
        }

        /// <summary>
        /// Helper function that expands the size of the array, maintaining the content.
        /// </summary>

        void AllocateMore()
        {
            T[] newList = (buffer != null) ? new T[Mathf.Max(buffer.Length << 1, 32)] : new T[32];
            if (buffer != null && Size > 0) buffer.CopyTo(newList, 0);
            buffer = newList;
        }

        /// <summary>
        /// Trim the unnecessary memory, resizing the buffer to be of 'Length' size.
        /// Call this function only if you are sure that the buffer won't need to resize anytime soon.
        /// </summary>

        void Trim()
        {
            if (Size > 0)
            {
                if (Size < buffer.Length)
                {
                    T[] newList = new T[Size];
                    for (int i = 0; i < Size; ++i) newList[i] = buffer[i];
                    buffer = newList;
                }
            }
            else buffer = null;
        }

        /// <summary>
        /// Clear the array by resetting its size to zero. Note that the memory is not actually released.
        /// </summary>

        public void Clear()
        {
            for (int i = 0; i < Size; i++)
            {
                buffer[i] = default(T);
            }

            Size = 0;
        }

        /// <summary>
        /// Clear the array and release the used memory.
        /// </summary>

        public void Release() { Size = 0; buffer = null; }

        /// <summary>
        /// Add the specified item to the end of the list.
        /// </summary>

        public void Add(T item)
        {
            if (buffer == null || Size == buffer.Length) AllocateMore();
            buffer[Size++] = item;
        }

        public void AddRange(BetterList<T> range)
        {
            if (range.buffer != null)
            {
                for (int i = 0; i < range.Size; i++)
                {
                    Add(range.buffer[i]);
                }
            }
        }

        public void AddRange(T[] range)
        {
            if (range != null)
            {
                for (int i = 0; i < range.Length; i++)
                {
                    Add(range[i]);
                }
            }
        }

        /// <summary>
        /// Insert an item at the specified index, pushing the entries back.
        /// </summary>

        public void Insert(int index, T item)
        {
            if (buffer == null || Size == buffer.Length) AllocateMore();

            if (index > -1 && index < Size)
            {
                for (int i = Size; i > index; --i) buffer[i] = buffer[i - 1];
                buffer[index] = item;
                ++Size;
            }
            else Add(item);
        }

        /// <summary>
        /// Returns 'true' if the specified item is within the list.
        /// </summary>

        public bool Contains(T item)
        {
            if (buffer != null)
            {
                EqualityComparer<T> comp = EqualityComparer<T>.Default;
                for (int i = 0; i < Size; ++i)
                {
                    if (comp.Equals(buffer[i], item))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Return the index of the specified item.
        /// </summary>

        public int IndexOf(T item)
        {
            if (buffer != null)
            {
                EqualityComparer<T> comp = EqualityComparer<T>.Default;
                for (int i = 0; i < Size; ++i)
                {
                    if (comp.Equals(buffer[i], item)) return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Remove the specified item from the list. Note that RemoveAt() is faster and is advisable if you already know the index.
        /// </summary>

        public bool Remove(T item)
        {
            if (buffer != null)
            {
                EqualityComparer<T> comp = EqualityComparer<T>.Default;
                for (int i = 0; i < Size; ++i)
                {
                    if (comp.Equals(buffer[i], item))
                    {
                        --Size;
                        buffer[i] = default(T);
                        for (int b = i; b < Size; ++b) buffer[b] = buffer[b + 1];
                        buffer[Size] = default(T);
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Remove an item at the specified index.
        /// </summary>

        public void RemoveAt(int index)
        {
            if (buffer != null && index > -1 && index < Size)
            {
                --Size;
                buffer[index] = default(T);
                for (int b = index; b < Size; ++b) buffer[b] = buffer[b + 1];
                buffer[Size] = default(T);
            }
        }

        /// <summary>
        /// Remove an item from the end.
        /// </summary>

        public T Pop()
        {
            if (buffer != null && Size != 0)
            {
                T val = buffer[--Size];
                buffer[Size] = default(T);
                return val;
            }
            return default(T);
        }

        /// <summary>
        /// Mimic List's ToArray() functionality, except that in this case the list is resized to match the current size.
        /// </summary>

        public T[] ToArray() { Trim(); return buffer; }

        /// <summary>
        /// List.Sort equivalent. Manual sorting causes no GC allocations.
        /// </summary>

        [DebuggerHidden]
        [DebuggerStepThrough]
        public void Sort(CompareFunc comparer)
        {
            int start = 0;
            int max = Size - 1;
            bool changed = true;

            while (changed)
            {
                changed = false;

                for (int i = start; i < max; ++i)
                {
                    // Compare the two values
                    if (comparer(buffer[i], buffer[i + 1]) > 0)
                    {
                        // Swap the values
                        T temp = buffer[i];
                        buffer[i] = buffer[i + 1];
                        buffer[i + 1] = temp;
                        changed = true;
                    }
                    else if (!changed)
                    {
                        // Nothing has changed -- we can start here next time
                        start = (i == 0) ? 0 : i - 1;
                    }
                }
            }
        }

        int IList.Add(object value)
        {
            this.Add((T)value);
            return 0;
        }

        bool IList.Contains(object value)
        {
            return this.Contains((T)value);
        }

        int IList.IndexOf(object value)
        {
            return this.IndexOf((T)value);
        }

        void IList.Insert(int index, object value)
        {
            this.Insert(index,(T)value);
        }

        void IList.Remove(object value)
        {
            this.Remove((T)value);
        }

        void ICollection.CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            if (buffer != null)
            {
                for (int i = 0; i < Size; ++i)
                {
                    yield return buffer[i];
                }
            }
        }

        /// <summary>
        /// Comparison function should return -1 if left is less than right, 1 if left is greater than right, and 0 if they match.
        /// </summary>

        public delegate int CompareFunc(T left, T right);
    }

}