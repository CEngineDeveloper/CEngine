using System.Collections.Generic;

namespace CYM
{
    [Unobfus]
    public class CDUpdater<TData> : Dictionary<TData, CD>
    {
        #region event
        public event Callback<TData, CD> Callback_OnAdded;
        public event Callback<TData> Callback_OnRemoved;
        #endregion

        List<TData> ClearDatas = new List<TData>();
        public void Update()
        {
            ClearDatas.Clear();
            foreach (var item in this)
            {
                item.Value.Update();
                if (item.Value.IsOver())
                    ClearDatas.Add(item.Key);
            }
            foreach (var item in ClearDatas)
            {
                this.Remove(item);
                Callback_OnRemoved?.Invoke(item);
            }
        }
        public bool IsHave(TData key)
        {
            return this.ContainsKey(key);
        }
        public void AddCD(TData key, int cd)
        {
            if (this.ContainsKey(key)) return;
            var cdData = new CD(cd);
            this.Add(key, cdData);
            Callback_OnAdded?.Invoke(key, cdData);
        }
    }
}
