using System.Collections.Generic;

namespace CYM
{
    public class ListIndexer<T>
    {
        public int CurIndex { get; private set; } = 0;
        IList<T> Data;
        public ListIndexer()
        {
            SetData(new List<T>());
        }
        public ListIndexer(IList<T> data)
        {
            SetData(data);
        }
        public void SetData(IList<T> data)
        {
            Data = data;
            Reset();
        }
        public void Reset()
        {
            CurIndex = 0;
        }
        public T Next()
        {
            if (Data.Count <= 0)
                return default;
            if (CurIndex >= Data.Count)
                CurIndex = 0;
            if (CurIndex < 0)
                CurIndex = Data.Count - 1;

            var ret = Data[CurIndex];
            CurIndex++;
            return ret;
        }
        public T Prev()
        {
            if (Data.Count <= 0)
                return default;
            if (CurIndex >= Data.Count)
                CurIndex = 0;
            if (CurIndex < 0)
                CurIndex = Data.Count - 1;

            var ret = Data[CurIndex];
            CurIndex--;
            return ret;
        }
    }
}