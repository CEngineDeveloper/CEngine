using System.Collections.Generic;

namespace CYM
{
    [Unobfus]
    public class OverlapValue
    {
        public string Key;
        public float Value;
    }
    [Unobfus]
    public class OverlapValuePool
    {
        #region prop
        public bool IsContainNegative { get; set; } = false;
        public Dictionary<string, OverlapValue> Data { get; protected set; } = new Dictionary<string, OverlapValue>();
        #endregion

        #region set
        public void Increase(string key, float val)
        {
            if (Data.ContainsKey(key))
            {
                Data[key].Value += val;
            }
            else
            {
                OverlapValue tempVal = new OverlapValue();
                tempVal.Value = val;
                tempVal.Key = key;
                Data.Add(key, tempVal);
            }
        }
        public void Decrease(string key, float val)
        {
            OverlapValue tempVal = null;
            if (Data.ContainsKey(key))
            {
                tempVal = Data[key];
                tempVal.Value -= val;
                if (tempVal.Value <= 0 && !IsContainNegative)
                {
                    Data.Remove(key);
                }
            }
            else
            {
                if (IsContainNegative)
                {
                    tempVal = new OverlapValue();
                    tempVal.Key = key;
                    tempVal.Value -= val;
                }
            }
        }
        #endregion

        #region get
        public float GetTotal()
        {
            float total = 0;
            foreach (var item in Data)
            {
                total += item.Value.Value;
            }
            return total;
        }
        #endregion
    }
}
