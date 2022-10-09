using System;

namespace CYM.Excel
{
    public sealed class Mapper<T> : MapperBase<Mapper<T>>, IGenerator<T> where T : new()
    {
        public Mapper() : base(typeof(T))
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
    }

    public sealed class Mapper : MapperBase<Mapper>, IGenerator
    {
        public Mapper(Type type) : base(type)
        {
        }

        object IGenerator.Instantiate(Row row)
        {
            return Instantiate(row);
        }
    }
}