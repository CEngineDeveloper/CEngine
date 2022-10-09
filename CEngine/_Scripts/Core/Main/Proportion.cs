using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CYM
{
    /// <summary>
    /// 比例数值
    /// </summary>
    [Serializable]
    [Unobfus]
    public class Proportion<T> where T : Enum
    {
        public Dictionary<int, float> Data { get; private set; } = new Dictionary<int, float>();
        public Proportion()
        {
            Init();
        }
        void Init()
        {
            Data.Clear();
            EnumTool<T>.ForIndex(x =>
            {
                Data.Add(x, 0);
            });
            if (Data.Count > 0)
            {
                Data[0] = 1.0f;
            }
            else
            {
                Debug.LogError(string.Format("{0}:没有设置任何值", typeof(T).Name));
            }
        }

        public void SetVal(T type, float val, bool isConst = true)
        {
            if (val == 0) return;
            var index = EnumTool<T>.Int(type);
            var sourceVal = Data[index];
            var adt = val - sourceVal;
            Data[index] = val;
            if (isConst && adt != 0)
            {
                float average = adt / (Data.Count - 1);
                foreach (var key in Data.Keys.ToArray())
                {
                    if (key == EnumTool<T>.Int(type)) continue;
                    Data[key] += average;
                }
            }
        }

        public float GetVal(T type)
        {
            return Data[EnumTool<T>.Int(type)];
        }
    }
}
