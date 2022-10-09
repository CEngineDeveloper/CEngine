using System;
using System.Collections.Generic;
using System.Linq;

namespace CYM.Excel
{
    public sealed class TableMapper<T> : TableMapperBase<TableMapper<T>>, IGenerator<T>, ITableGenerator<T> where T : new()
    {
        public TableMapper() : base(typeof(T))
        {
        }

        protected override object CreateInstance()
        {
            return new T();
        }

        T IGenerator<T>.Instantiate(Row row)
        {
            return (T)Instantiate(row);
        }

        IEnumerable<T> ITableGenerator<T>.Instantiate(Table table)
        {
            return table.Where((r, i) => !excludes.Contains(i)).Select(row => (T)Instantiate(row));
        }
    }

    public sealed class TableMapper : TableMapperBase<TableMapper>, IGenerator, ITableGenerator
    {
        public TableMapper(Type type) : base(type)
        {

        }

        object IGenerator.Instantiate(Row row)
        {
            return Instantiate(row);
        }

        IEnumerable<object> ITableGenerator.Instantiate(Table table)
        {
            return table.Where((r, i) => !excludes.Contains(i)).Select(row => Instantiate(row));
        }
    }
}