using System;
using System.Collections.Generic;
using System.Linq;

namespace CYM.Excel
{
    public abstract class TableMapperBase<T> : MapperBase<T> where T : TableMapperBase<T>
    {
        protected readonly List<int> excludes;
        protected int index;

        protected TableMapperBase(Type type) : base(type)
        {
            excludes = new List<int>();
        }

        public override void Extract()
        {
            var attribute = Attribute.GetCustomAttribute(type, typeof(TableAttribute)) as TableAttribute;
            if (attribute != null)
            {
                if (attribute.Ignore != null && attribute.Ignore.Length > 0)
                {
                    Exclude(attribute.Ignore);
                }
                SafeMode = attribute.SafeMode;
            }
            base.Extract();
        }

        /// <summary>
        /// Exclude target rows from being instantiated
        /// </summary>
        /// <param name="rows">One-based row indices</param>
        /// <returns>Mapper instance</returns>
        public T Exclude(params int[] rows)
        {
            if (rows == null || rows.Length == 0)
            {
                throw new ArgumentException("Rows must be specified");
            }
            for (int i = 0; i < rows.Length; i++)
            {
                if (rows[i] < 1)
                {
                    throw new ArgumentException("One-based row index must be greater than 0");
                }
                rows[i]--;
            }
            excludes.AddRange(rows);
            return (T)this;
        }
        public T Exclude(int rows)
        {
            if(rows<=0)
                throw new ArgumentException("One-based row index must be greater than 0");
            for (int i = 0; i < rows; ++i)
                excludes.Add(i);
            return (T)this;
        }
        /// <summary>
        /// Include target rows to being instantiated
        /// </summary>
        /// <param name="rows">One-based row indices</param>
        /// <returns>Mapper instance</returns>
        public T Include(params int[] rows)
        {
            if (rows == null || rows.Length == 0)
            {
                throw new ArgumentException("Rows must be specified");
            }
            for (int i = 0; i < rows.Length; i++)
            {
                if (rows[i] < 1)
                {
                    throw new ArgumentException("One-based row index must be greater than 0");
                }
                rows[i]--;
            }
            excludes.RemoveAll(i => rows.Contains(i));
            return (T)this;
        }

        /// <summary>
        /// Include all rows to being instantiated
        /// </summary>
        /// <returns>Mapper instance</returns>
        public T IncludeAll()
        {
            excludes.Clear();
            return (T)this;
        }
    }
}