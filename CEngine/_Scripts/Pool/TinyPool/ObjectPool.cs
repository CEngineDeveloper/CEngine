namespace CYM.Pool
{
    public class ObjectPool<T> : BaseObjectPool<T> where T : new()
    {
        protected override T CreateInstanceDefualt()
        {
            return new T();
        }

        public ObjectPool(int count = 0) : base(null, count)
        {
        }
    }
}