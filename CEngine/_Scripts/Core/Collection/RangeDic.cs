using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
//**********************************************
// Class Name	: CYMHashManager
// Discription	：None
// Author	：CYM
// Team		：MoBaGame
// Date		：2015-11-1
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************

namespace CYM
{
    [Unobfus]
    public class RangeDic<T> : SortedDictionary<T, Range> 
        where T: Enum
    {
        float Min = float.MaxValue;
        float Max = float.MinValue;
        public T Get(float val)
        {
            foreach (var item in this)
            {
                if (item.Value.Min < Min) Min = item.Value.Min;
                if (item.Value.Max > Max) Max = item.Value.Max;
                if (item.Value.IsIn(val))
                    return item.Key;
            }
            if (val > Max) return EnumTool<T>.End();
            else if (val < Min) return EnumTool<T>.Start();
            return this.LastOrDefault().Key;
        }

        public Tuple<T, int> GetValAndIndex(float val)
        {
            int index = 0;
            foreach (var item in this)
            {
                if (item.Value.IsIn(val))
                {
                    return new Tuple<T, int>(item.Key, index);
                }
                index++;
            }
            return new Tuple<T, int>(this.LastOrDefault().Key, 0);
        }

        public IList ToList() => new List<KeyValuePair<T, Range>>(this);

    }
}
